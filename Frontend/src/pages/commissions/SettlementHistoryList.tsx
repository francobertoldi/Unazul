import { useMemo, useCallback } from "react";
import { Download } from "lucide-react";
import { Button } from "@/components/ui/button";
import { DataTable, Column } from "@/components/shared/DataTable";
import { useSettlementHistoryStore, useUserStore } from "@/data/store";
import { SettlementHistoryEntry } from "@/data/types";

export default function SettlementHistoryList() {
  const { entries } = useSettlementHistoryStore();
  const { users } = useUserStore();

  const userOptions = useMemo(() =>
    users.map(u => ({ value: `${u.firstName} ${u.lastName}`, label: `${u.firstName} ${u.lastName}` }))
      .sort((a, b) => a.label.localeCompare(b.label)),
    [users]
  );

  const handleDownload = useCallback((entry: SettlementHistoryEntry) => {
    if (!entry.excelDataUrl) return;
    const link = document.createElement("a");
    link.href = entry.excelDataUrl;
    const dateStr = new Date(entry.settledAt).toISOString().slice(0, 10);
    link.download = `liquidacion-${dateStr}.xlsx`;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  }, []);

  const columns: Column<SettlementHistoryEntry>[] = [
    {
      key: "settledAt", header: "Fecha Liquidación", searchable: false, filterType: "date", sortable: true,
      render: (r) => (
        <span className="text-sm text-muted-foreground">
          {new Date(r.settledAt).toLocaleString("es-AR", { dateStyle: "short", timeStyle: "short" })}
        </span>
      ),
    },
    {
      key: "userName", header: "Usuario", searchable: true, sortable: true,
      filterType: "multiselect", filterOptions: userOptions,
      render: (r) => <span className="text-sm font-medium">{r.userName}</span>,
    },
    {
      key: "operationCount", header: "Operaciones", searchable: false, filterable: false, sortable: true, sortType: "number",
      render: (r) => <span className="text-sm font-semibold">{r.operationCount}</span>,
    },
    {
      key: "_totals" as keyof SettlementHistoryEntry, header: "Totales", searchable: false, filterable: false,
      render: (r) => (
        <div className="flex flex-col gap-0.5">
          {r.totalsByCurrency.map(t => (
            <span key={t.currency} className="text-sm font-semibold text-primary">
              {t.currency === "USD" ? "US$" : "$"} {t.total.toLocaleString("es-AR", { minimumFractionDigits: 2, maximumFractionDigits: 2 })} {t.currency}
            </span>
          ))}
        </div>
      ),
    },
    {
      key: "_download" as keyof SettlementHistoryEntry, header: "Archivo", searchable: false, filterable: false,
      render: (r) => (
        <div onClick={e => e.stopPropagation()}>
          {r.excelDataUrl ? (
            <Button variant="ghost" size="sm" className="h-8 gap-1.5 text-xs" onClick={() => handleDownload(r)}>
              <Download className="h-3.5 w-3.5" />
              Descargar
            </Button>
          ) : (
            <span className="text-xs text-muted-foreground">—</span>
          )}
        </div>
      ),
    },
  ];

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">Historial de Liquidaciones</h1>
        <p className="text-muted-foreground text-sm mt-1">Registro histórico de liquidaciones realizadas</p>
      </div>

      <DataTable
        data={entries}
        columns={columns}
        searchPlaceholder="Buscar en historial..."
        exportFileName="historial-liquidaciones"
      />
    </div>
  );
}
