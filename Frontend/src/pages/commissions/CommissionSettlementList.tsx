import { useState, useMemo, useCallback } from "react";
import { useNavigate } from "react-router-dom";
import { DollarSign, Eye, Download, CheckCircle, FileSpreadsheet } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
import { DataTable, Column } from "@/components/shared/DataTable";
import { useApplicationStore, useProductStore, useCommissionPlanStore, useEntityStore, useSettlementHistoryStore } from "@/data/store";
import { Application, ApplicationStatus, CommissionPlan, ProductPlan, SettlementHistoryItem } from "@/data/types";
import { useAuthStore } from "@/data/authStore";
import { toast } from "sonner";
import * as XLSX from "xlsx";
import {
  AlertDialog, AlertDialogAction, AlertDialogCancel, AlertDialogContent,
  AlertDialogDescription, AlertDialogFooter, AlertDialogHeader, AlertDialogTitle,
} from "@/components/ui/alert-dialog";

const statusLabels: Record<ApplicationStatus, string> = {
  draft: "Borrador", pending: "Pendiente", in_review: "En Revisión",
  approved: "Aprobada", rejected: "Rechazada", cancelled: "Cancelada", settled: "Liquidado",
};
const statusColors: Record<ApplicationStatus, string> = {
  draft: "bg-muted text-muted-foreground", pending: "bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-200",
  in_review: "bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200",
  approved: "bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200",
  rejected: "bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200",
  cancelled: "bg-muted text-muted-foreground",
  settled: "bg-purple-100 text-purple-800 dark:bg-purple-900 dark:text-purple-200",
};
const statusOptions = Object.entries(statusLabels).map(([value, label]) => ({ value, label }));

export function calculateCommission(plan: ProductPlan | undefined, commissionPlan: CommissionPlan | undefined): number {
  if (!plan || !commissionPlan) return 0;
  let commission = 0;
  switch (commissionPlan.valueType) {
    case "fixed_per_sale":
      commission = commissionPlan.value;
      break;
    case "percentage_capital":
      commission = (commissionPlan.value / 100) * plan.price;
      break;
    case "percentage_total_loan": {
      const totalLoan = plan.price * (plan.installments || 1);
      commission = (commissionPlan.value / 100) * totalLoan;
      break;
    }
  }
  if (commissionPlan.maxAmount && commission > commissionPlan.maxAmount) {
    commission = commissionPlan.maxAmount;
  }
  return commission;
}

function getCommissionFormula(plan: ProductPlan | undefined, commissionPlan: CommissionPlan | undefined): string {
  if (!plan || !commissionPlan) return "—";
  const curr = plan.currency === "USD" ? "US$" : "$";
  switch (commissionPlan.valueType) {
    case "fixed_per_sale":
      return `Fijo por venta: ${curr} ${commissionPlan.value.toLocaleString("es-AR", { minimumFractionDigits: 2 })}`;
    case "percentage_capital":
      return `${commissionPlan.value}% × Capital (${curr} ${plan.price.toLocaleString("es-AR", { minimumFractionDigits: 2 })})`;
    case "percentage_total_loan": {
      const totalLoan = plan.price * (plan.installments || 1);
      return `${commissionPlan.value}% × Préstamo Total (${curr} ${totalLoan.toLocaleString("es-AR", { minimumFractionDigits: 2 })})`;
    }
  }
}

interface CommissionRow {
  id: string;
  entityName: string;
  entityId: string;
  code: string;
  createdAt: string;
  applicantName: string;
  applicantDoc: string;
  status: ApplicationStatus;
  commission: number;
  currency: string;
  commissionPlanName: string;
  commissionFormula: string;
  application: Application;
  plan: ProductPlan | undefined;
  commissionPlan: CommissionPlan | undefined;
}

export default function CommissionSettlementList() {
  const navigate = useNavigate();
  const { applications, updateApplication } = useApplicationStore();
  const { products } = useProductStore();
  const { commissionPlans } = useCommissionPlanStore();
  const { entities } = useEntityStore();
  const { addEntry } = useSettlementHistoryStore();
  const { user } = useAuthStore();
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());
  const [settleDialogOpen, setSettleDialogOpen] = useState(false);
  const [settledExcelUrl, setSettledExcelUrl] = useState<string | null>(null);
  const [settledCount, setSettledCount] = useState(0);
  const entityOptions = useMemo(() =>
    [...entities].sort((a, b) => a.name.localeCompare(b.name)).map(e => ({ value: e.name, label: e.name })),
    [entities]
  );

  const commissionPlanOptions = useMemo(() =>
    [...commissionPlans].sort((a, b) => a.description.localeCompare(b.description))
      .map(cp => ({ value: `${cp.description} (${cp.code})`, label: `${cp.description} (${cp.code})` })),
    [commissionPlans]
  );

  const planMap = useMemo(() => {
    const m = new Map<string, ProductPlan>();
    for (const prod of products) for (const plan of prod.plans) m.set(plan.id, plan);
    return m;
  }, [products]);

  const cpMap = useMemo(() => {
    const m = new Map<string, CommissionPlan>();
    for (const cp of commissionPlans) m.set(cp.id, cp);
    return m;
  }, [commissionPlans]);

  const rows: CommissionRow[] = useMemo(() =>
    applications.map(app => {
      const plan = planMap.get(app.planId);
      const cp = plan ? cpMap.get(plan.commissionPlanId) : undefined;
      return {
        id: app.id, entityName: app.entityName, entityId: app.entityId, code: app.code,
        createdAt: app.createdAt,
        applicantName: `${app.applicant.firstName} ${app.applicant.lastName}`,
        applicantDoc: `${app.applicant.documentType} ${app.applicant.documentNumber}`,
        status: app.status, commission: calculateCommission(plan, cp),
        currency: plan?.currency || "ARS",
        commissionPlanName: cp ? `${cp.description} (${cp.code})` : "—",
        commissionFormula: getCommissionFormula(plan, cp),
        application: app, plan, commissionPlan: cp,
      };
    }),
    [applications, planMap, cpMap]
  );

  const [filteredRows, setFilteredRows] = useState<CommissionRow[]>(rows);

  const handleFilteredDataChange = useCallback((data: CommissionRow[]) => {
    setFilteredRows(data);
  }, []);

  const filteredIds = useMemo(() => new Set(filteredRows.map(r => r.id)), [filteredRows]);
  const allFilteredSelected = filteredRows.length > 0 && filteredRows.every(r => selectedIds.has(r.id));
  const someFilteredSelected = filteredRows.some(r => selectedIds.has(r.id));

  const toggleSelectAll = useCallback(() => {
    setSelectedIds(prev => {
      const next = new Set(prev);
      if (allFilteredSelected) {
        filteredRows.forEach(r => next.delete(r.id));
      } else {
        filteredRows.forEach(r => next.add(r.id));
      }
      return next;
    });
  }, [filteredRows, allFilteredSelected]);

  const toggleSelect = useCallback((id: string) => {
    setSelectedIds(prev => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id); else next.add(id);
      return next;
    });
  }, []);

  const buildExcelWorkbook = useCallback((rowsForExcel: CommissionRow[], isPreview = false) => {
    const currencies = [...new Set(rowsForExcel.map(r => r.currency))].sort();
    const wb = XLSX.utils.book_new();

    for (const currency of currencies) {
      const currencyRows = rowsForExcel
        .filter(r => r.currency === currency)
        .sort((a, b) => a.entityName.localeCompare(b.entityName));

      const comisionCol = `Comisión (${currency})`;
      const formulaCol = "Fórmula Comisión";
      const currSymbol = currency === "USD" ? "US$" : "$";
      const numFmt = `"${currSymbol} "#,##0.00`;
      const sheetData: Record<string, unknown>[] = [];
      let currentEntity = "";

      for (const r of currencyRows) {
        if (r.entityName !== currentEntity) {
          if (currentEntity !== "") {
            const entityRows = currencyRows.filter(cr => cr.entityName === currentEntity);
            const subtotal = entityRows.reduce((s, cr) => s + cr.commission, 0);
            sheetData.push({
              Entidad: "", "Nro. Solicitud": "", "Fecha Solicitud": "",
              Solicitante: "", Documento: "", "Plan Comisión": "",
              [formulaCol]: "",
              [comisionCol]: subtotal,
              "": `Subtotal ${currentEntity}`,
            });
            sheetData.push({});
          }
          currentEntity = r.entityName;
        }
        sheetData.push({
          Entidad: r.entityName,
          "Nro. Solicitud": r.code,
          "Fecha Solicitud": new Date(r.createdAt).toLocaleDateString("es-AR"),
          Solicitante: r.applicantName,
          Documento: r.applicantDoc,
          "Plan Comisión": r.commissionPlanName,
          [formulaCol]: r.commissionFormula,
          [comisionCol]: r.commission,
        });
      }

      if (currentEntity !== "") {
        const entityRows = currencyRows.filter(cr => cr.entityName === currentEntity);
        const subtotal = entityRows.reduce((s, cr) => s + cr.commission, 0);
        sheetData.push({
          Entidad: "", "Nro. Solicitud": "", "Fecha Solicitud": "",
          Solicitante: "", Documento: "", "Plan Comisión": "",
          [formulaCol]: "",
          [comisionCol]: subtotal,
          "": `Subtotal ${currentEntity}`,
        });
      }

      const grandTotal = currencyRows.reduce((s, r) => s + r.commission, 0);
      sheetData.push({});
      sheetData.push({
        Entidad: "", "Nro. Solicitud": "", "Fecha Solicitud": "",
        Solicitante: "", Documento: "", "Plan Comisión": "TOTAL",
        [formulaCol]: "",
        [comisionCol]: grandTotal,
      });

      // If preview, prepend a header row with legend
      if (isPreview) {
        const headerRow: Record<string, unknown> = { Entidad: "*** VISTA PREVIA DE LIQUIDACIÓN — Este documento no representa una liquidación definitiva ***" };
        sheetData.unshift({});
        sheetData.unshift(headerRow);
      }

      const ws = XLSX.utils.json_to_sheet(sheetData);

      // If preview, merge the legend across all columns
      if (isPreview) {
        const headers = Object.keys(sheetData[2] || {});
        const colCount = headers.length;
        if (colCount > 1) {
          ws["!merges"] = [{ s: { r: 0, c: 0 }, e: { r: 0, c: colCount - 1 } }];
        }
        // Bold the legend cell
        const legendCell = ws[XLSX.utils.encode_cell({ r: 0, c: 0 })];
        if (legendCell) {
          legendCell.s = { font: { bold: true, sz: 14 } };
        }
      }

      const refHeaders = Object.keys(sheetData[isPreview ? 2 : 0] || {});
      const comisionColIdx = refHeaders.indexOf(comisionCol);
      if (comisionColIdx >= 0) {
        const range = XLSX.utils.decode_range(ws["!ref"] || "A1");
        for (let row = range.s.r + 1; row <= range.e.r; row++) {
          const cellRef = XLSX.utils.encode_cell({ r: row, c: comisionColIdx });
          if (ws[cellRef] && typeof ws[cellRef].v === "number") {
            ws[cellRef].z = numFmt;
          }
        }
      }
      const colWidths = refHeaders.map(k => ({ wch: Math.max(k.length + 2, 18) }));
      ws["!cols"] = colWidths;
      XLSX.utils.book_append_sheet(wb, ws, `Comisión en ${currency}`);
    }
    return wb;
  }, []);

  const handleSettle = useCallback(() => {
    const idsToSettle = [...selectedIds].filter(id => filteredIds.has(id));
    const settledRows = filteredRows.filter(r => idsToSettle.includes(r.id));

    // Generate Excel and convert to data URL
    const wb = buildExcelWorkbook(settledRows);
    const wbout = XLSX.write(wb, { bookType: "xlsx", type: "base64" });
    const excelDataUrl = `data:application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;base64,${wbout}`;

    // Create history entry
    const totalsByCurrencyMap = new Map<string, number>();
    const items: SettlementHistoryItem[] = settledRows.map(r => {
      totalsByCurrencyMap.set(r.currency, (totalsByCurrencyMap.get(r.currency) || 0) + r.commission);
      return {
        applicationId: r.id,
        code: r.code,
        entityName: r.entityName,
        applicantName: r.applicantName,
        applicantDoc: r.applicantDoc,
        commissionPlanName: r.commissionPlanName,
        commissionFormula: r.commissionFormula,
        commission: r.commission,
        currency: r.currency,
      };
    });

    addEntry({
      settledAt: new Date().toISOString(),
      userId: user?.id || "",
      userName: user ? `${user.firstName} ${user.lastName}` : "Sistema",
      operationCount: idsToSettle.length,
      totalsByCurrency: Array.from(totalsByCurrencyMap.entries()).map(([currency, total]) => ({ currency, total })),
      items,
      excelDataUrl,
    });

    idsToSettle.forEach(id => updateApplication(id, { status: "settled" }));
    setSettledExcelUrl(excelDataUrl);
    setSettledCount(idsToSettle.length);
    setSelectedIds(new Set());
    toast.success(`${idsToSettle.length} solicitud(es) liquidada(s)`);
  }, [selectedIds, filteredIds, filteredRows, updateApplication, addEntry, user, buildExcelWorkbook]);

  const handleDownloadSettledExcel = useCallback(() => {
    if (!settledExcelUrl) return;
    const link = document.createElement("a");
    link.href = settledExcelUrl;
    link.download = `liquidacion-${new Date().toISOString().slice(0, 10)}.xlsx`;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  }, [settledExcelUrl]);

  const handlePreSettlement = useCallback(() => {
    const selected = filteredRows.filter(r => selectedIds.has(r.id));
    if (selected.length === 0) return;
    const wb = buildExcelWorkbook(selected, true);
    XLSX.writeFile(wb, `preliquidacion-${new Date().toISOString().slice(0, 10)}.xlsx`);
    toast.success("Archivo de preliquidación descargado");
  }, [filteredRows, selectedIds, buildExcelWorkbook]);

  const handleCloseDialog = useCallback(() => {
    setSettleDialogOpen(false);
    setSettledExcelUrl(null);
    setSettledCount(0);
  }, []);

  const columns: Column<CommissionRow>[] = [
    {
      key: "_select" as keyof CommissionRow, header: "", searchable: false,
      headerRender: () => (
        <Checkbox
          checked={allFilteredSelected}
          onCheckedChange={toggleSelectAll}
          aria-label="Seleccionar todos"
          className={someFilteredSelected && !allFilteredSelected ? "data-[state=unchecked]:bg-primary/30" : ""}
        />
      ),
      render: (r) => (
        <div onClick={e => e.stopPropagation()}>
          <Checkbox
            checked={selectedIds.has(r.id)}
            onCheckedChange={() => toggleSelect(r.id)}
            aria-label={`Seleccionar ${r.code}`}
          />
        </div>
      ),
    },
    {
      key: "entityName", header: "Entidad", searchable: true, sortable: true,
      filterType: "multiselect", filterOptions: entityOptions,
      render: (r) => <span className="text-sm font-medium">{r.entityName}</span>,
    },
    {
      key: "code", header: "Nro. Solicitud", searchable: true, filterType: "text", sortable: true,
      render: (r) => (
        <div className="flex items-center gap-2">
          <div className="h-8 w-8 rounded-lg bg-primary/10 flex items-center justify-center shrink-0">
            <DollarSign className="h-4 w-4 text-primary" />
          </div>
          <span className="text-sm font-medium">{r.code}</span>
        </div>
      ),
    },
    {
      key: "createdAt", header: "Fecha Solicitud", searchable: false, filterType: "date", sortable: true,
      render: (r) => <span className="text-sm text-muted-foreground">{new Date(r.createdAt).toLocaleDateString("es-AR")}</span>,
    },
    {
      key: "applicantName", header: "Solicitante", searchable: true, filterType: "text", sortable: true,
      render: (r) => <span className="text-sm">{r.applicantName}</span>,
    },
    {
      key: "applicantDoc", header: "Documento", searchable: true, filterType: "text", sortable: true,
      render: (r) => <span className="text-sm text-muted-foreground">{r.applicantDoc}</span>,
    },
    {
      key: "status", header: "Estado", searchable: false, sortable: true,
      filterType: "multiselect", filterOptions: statusOptions,
      render: (r) => <span className={`text-xs px-2 py-1 rounded-full font-medium ${statusColors[r.status]}`}>{statusLabels[r.status]}</span>,
    },
    {
      key: "commissionPlanName", header: "Plan Comisión", searchable: true, sortable: true,
      filterType: "multiselect", filterOptions: commissionPlanOptions,
      render: (r) => <span className="text-sm">{r.commissionPlanName}</span>,
    },
    {
      key: "commission", header: "Comisión", searchable: false, sortable: true, sortType: "number" as const,
      render: (r) => (
        <span className="text-sm font-semibold text-primary">
          {r.currency === "USD" ? "US$" : "$"} {r.commission.toLocaleString("es-AR", { minimumFractionDigits: 2, maximumFractionDigits: 2 })}
        </span>
      ),
      exportValue: (r) => r.commission,
    },
    {
      key: "_actions" as keyof CommissionRow, header: "", searchable: false,
      render: (r) => (
        <div onClick={e => e.stopPropagation()}>
          <Button variant="ghost" size="icon" className="h-8 w-8" onClick={() => navigate(`/liquidacion-comisiones/${r.id}`)}>
            <Eye className="h-3.5 w-3.5" />
          </Button>
        </div>
      ),
    },
  ];

  const totalsByCurrency = useMemo(() => {
    const map = new Map<string, number>();
    for (const r of filteredRows) {
      map.set(r.currency, (map.get(r.currency) || 0) + r.commission);
    }
    return Array.from(map.entries()).sort((a, b) => a[0].localeCompare(b[0]));
  }, [filteredRows]);

  const selectedTotalsByCurrency = useMemo(() => {
    const map = new Map<string, number>();
    for (const r of filteredRows) {
      if (selectedIds.has(r.id)) {
        map.set(r.currency, (map.get(r.currency) || 0) + r.commission);
      }
    }
    return Array.from(map.entries()).sort((a, b) => a[0].localeCompare(b[0]));
  }, [filteredRows, selectedIds]);

  const selectedCount = useMemo(() =>
    [...selectedIds].filter(id => filteredIds.has(id)).length,
    [selectedIds, filteredIds]
  );

  const hasSelection = selectedIds.size > 0;

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">Liquidación de Comisiones</h1>
        <p className="text-muted-foreground text-sm mt-1">Consulta y detalle de comisiones por solicitud</p>
      </div>

      <DataTable
        data={rows}
        columns={columns}
        searchPlaceholder="Buscar por solicitud, solicitante..."
        exportFileName="liquidacion-comisiones"
        onRowClick={(r) => navigate(`/liquidacion-comisiones/${r.id}`)}
        defaultFilters={{ status: ["approved"] }}
        onFilteredDataChange={handleFilteredDataChange}
      />

      {/* Footer totals */}
      <div className="rounded-xl border bg-card p-4">
        <div className="flex flex-wrap items-center justify-between gap-4">
          <div>
            <h3 className="text-sm font-semibold mb-3">Totales por Moneda</h3>
            <div className="flex flex-wrap gap-6">
              {totalsByCurrency.map(([currency, total]) => (
                <div key={currency} className="flex items-center gap-3">
                  <div className="h-10 w-10 rounded-lg bg-primary/10 flex items-center justify-center">
                    <DollarSign className="h-5 w-5 text-primary" />
                  </div>
                  <div>
                    <p className="text-xs text-muted-foreground">{currency}</p>
                    <p className="text-lg font-bold">
                      {currency === "USD" ? "US$" : "$"} {total.toLocaleString("es-AR", { minimumFractionDigits: 2, maximumFractionDigits: 2 })}
                    </p>
                  </div>
                </div>
              ))}
            </div>
          </div>

          {hasSelection && (
            <div className="flex items-center gap-3">
              <span className="text-sm text-muted-foreground">{selectedIds.size} seleccionada(s)</span>
              <Button variant="outline" onClick={handlePreSettlement}>
                <FileSpreadsheet className="h-4 w-4 mr-2" />
                Preliquidación
              </Button>
              <Button onClick={() => { setSettledExcelUrl(null); setSettledCount(0); setSettleDialogOpen(true); }}>
                <CheckCircle className="h-4 w-4 mr-2" />
                Liquidar
              </Button>
            </div>
          )}
        </div>
      </div>

      <AlertDialog open={settleDialogOpen} onOpenChange={(open) => { if (!open) handleCloseDialog(); }}>
        <AlertDialogContent className="bg-muted">
          <AlertDialogHeader>
            <AlertDialogTitle>{settledExcelUrl ? "Liquidación Completada" : "Confirmar Liquidación"}</AlertDialogTitle>
            <AlertDialogDescription>
              {settledExcelUrl ? (
                <>Se liquidaron exitosamente <strong>{settledCount}</strong> operación(es). Puede descargar el archivo de liquidación.</>
              ) : (
                <>
                  ¿Está seguro que desea liquidar las operaciones seleccionadas ({selectedCount}) por un total de{" "}
                  {selectedTotalsByCurrency.map(([currency, total], i) => (
                    <span key={currency}>
                      {i > 0 && " + "}
                      <strong>{currency === "USD" ? "US$" : "$"} {total.toLocaleString("es-AR", { minimumFractionDigits: 2, maximumFractionDigits: 2 })}</strong>
                      {" "}{currency}
                    </span>
                  ))}?
                </>
              )}
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            {settledExcelUrl ? (
              <>
                <Button variant="outline" onClick={handleDownloadSettledExcel}>
                  <Download className="h-4 w-4 mr-2" />
                  Descargar liquidación
                </Button>
                <AlertDialogAction onClick={handleCloseDialog}>Cerrar</AlertDialogAction>
              </>
            ) : (
              <>
                <AlertDialogCancel>Cancelar</AlertDialogCancel>
                <AlertDialogAction onClick={(e) => { e.preventDefault(); handleSettle(); }}>Continuar</AlertDialogAction>
              </>
            )}
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}
