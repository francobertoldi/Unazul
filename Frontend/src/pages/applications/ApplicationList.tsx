import { useState, useMemo } from "react";
import { useNavigate } from "react-router-dom";
import { FileCheck, Plus, Trash2, Eye } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { DataTable } from "@/components/shared/DataTable";
import { DeleteConfirmDialog } from "@/components/crud/DeleteConfirmDialog";
import { useApplicationStore, useProductStore } from "@/data/store";
import { Application, ApplicationStatus } from "@/data/types";
import { toast } from "sonner";

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

function formatCurrency(v: number) {
  return v >= 1_000_000 ? `$${(v / 1_000_000).toFixed(1)}M` : v >= 1_000 ? `$${(v / 1_000).toFixed(0)}K` : `$${v}`;
}

function getPlanSummary(plan: any): string {
  if (!plan) return "—";
  const parts: string[] = [];
  if (plan.loanAttributes) {
    parts.push(`TEA ${plan.loanAttributes.annualEffectiveRate}%`);
    parts.push(`CFT ${plan.loanAttributes.cftRate}%`);
    parts.push(plan.loanAttributes.amortizationType.charAt(0).toUpperCase() + plan.loanAttributes.amortizationType.slice(1));
  } else if (plan.insuranceAttributes) {
    parts.push(`SA ${formatCurrency(plan.insuranceAttributes.sumInsured)}`);
    parts.push(`Prima ${formatCurrency(plan.insuranceAttributes.premium)}`);
  } else if (plan.cardAttributes) {
    parts.push(`Límite ${formatCurrency(plan.cardAttributes.creditLimit)}`);
    parts.push(`${plan.cardAttributes.network.toUpperCase()}`);
    if (plan.cardAttributes.level) parts.push(plan.cardAttributes.level);
  } else if (plan.accountAttributes) {
    parts.push(`Mant. ${formatCurrency(plan.accountAttributes.maintenanceFee)}`);
    parts.push(`TNA ${plan.accountAttributes.interestRate}%`);
  } else if (plan.investmentAttributes) {
    parts.push(`Mín. ${formatCurrency(plan.investmentAttributes.minimumAmount)}`);
    parts.push(`Rend. ${plan.investmentAttributes.expectedReturn}%`);
    parts.push(`${plan.investmentAttributes.termDays}d`);
  }
  if (plan.price > 0 && !plan.loanAttributes) parts.push(`Precio ${formatCurrency(plan.price)}`);
  return parts.length > 0 ? parts.join(" · ") : "—";
}

export default function ApplicationList() {
  const navigate = useNavigate();
  const { applications, deleteApplication } = useApplicationStore();
  const { products } = useProductStore();
  const [deleteTarget, setDeleteTarget] = useState<Application | null>(null);

  const planMap = useMemo(() => {
    const m = new Map<string, any>();
    for (const prod of products) {
      for (const plan of prod.plans) {
        m.set(plan.id, { ...plan, familyName: prod.familyName });
      }
    }
    return m;
  }, [products]);

  const columns = [
    {
      key: "code", header: "Código", searchable: true, filterType: "text" as const, sortable: true,
      render: (a: Application) => (
        <div className="flex items-center gap-3">
          <div className="h-9 w-9 rounded-lg bg-primary/10 flex items-center justify-center shrink-0"><FileCheck className="h-4 w-4 text-primary" /></div>
          <p className="font-medium text-sm">{a.code}</p>
        </div>
      ),
    },
    {
      key: "applicant", header: "Solicitante", searchable: true, filterType: "text" as const, sortable: true,
      filterValue: (a: Application) => `${a.applicant.firstName} ${a.applicant.lastName}`,
      sortValue: (a: Application) => `${a.applicant.lastName} ${a.applicant.firstName}`,
      render: (a: Application) => (
        <div>
          <p className="text-sm">{a.applicant.firstName} {a.applicant.lastName}</p>
          <p className="text-xs text-muted-foreground">{a.applicant.documentType} {a.applicant.documentNumber}</p>
        </div>
      ),
      exportValue: (a: Application) => `${a.applicant.firstName} ${a.applicant.lastName} (${a.applicant.documentType} ${a.applicant.documentNumber})`,
    },
    { key: "entityName", header: "Entidad", searchable: true, filterType: "text" as const, sortable: true, render: (a: Application) => <span className="text-sm">{a.entityName}</span> },
    { key: "planName", header: "Plan", searchable: false, filterType: "text" as const, sortable: true, render: (a: Application) => <span className="text-sm">{a.planName}</span> },
    {
      key: "productAttributes", header: "Producto", searchable: false,
      render: (a: Application) => {
        const plan = planMap.get(a.planId);
        const summary = getPlanSummary(plan);
        return (
          <div className="max-w-[260px]">
            {plan && <p className="text-[10px] text-muted-foreground font-medium mb-0.5">{plan.familyName}</p>}
            <p className="text-xs text-foreground/80 truncate" title={summary}>{summary}</p>
          </div>
        );
      },
      exportValue: (a: Application) => getPlanSummary(planMap.get(a.planId)),
    },
    {
      key: "status", header: "Estado", searchable: false, filterType: "select" as const, filterOptions: statusOptions, sortable: true,
      render: (a: Application) => <span className={`text-xs px-2 py-1 rounded-full font-medium ${statusColors[a.status]}`}>{statusLabels[a.status]}</span>,
    },
    {
      key: "currentWorkflowStateName", header: "Etapa", searchable: false, filterType: "text" as const, sortable: true,
      render: (a: Application) => <Badge variant="outline" className="text-xs">{a.currentWorkflowStateName}</Badge>,
    },
    {
      key: "createdAt", header: "Creado", searchable: false, filterType: "date" as const, sortable: true,
      render: (a: Application) => <span className="text-xs text-muted-foreground">{new Date(a.createdAt).toLocaleString("es-AR", { dateStyle: "short", timeStyle: "short" })}</span>,
    },
    {
      key: "updatedAt", header: "Actualizado", searchable: false, filterType: "date" as const, sortable: true,
      render: (a: Application) => <span className="text-xs text-muted-foreground">{new Date(a.updatedAt).toLocaleString("es-AR", { dateStyle: "short", timeStyle: "short" })}</span>,
    },
    {
      key: "_actions", header: "", searchable: false,
      render: (a: Application) => (
        <div className="flex items-center gap-1" onClick={ev => ev.stopPropagation()}>
          <Button variant="ghost" size="icon" className="h-8 w-8" onClick={() => navigate(`/solicitudes/${a.id}`)}><Eye className="h-3.5 w-3.5" /></Button>
          <Button variant="ghost" size="icon" className="h-8 w-8 text-destructive hover:text-destructive" onClick={() => setDeleteTarget(a)}><Trash2 className="h-3.5 w-3.5" /></Button>
        </div>
      ),
    },
  ];

  return (
    <div className="space-y-6">
      <div><h1 className="text-2xl font-bold tracking-tight">Solicitudes</h1><p className="text-muted-foreground text-sm mt-1">Gestión de solicitudes de productos</p></div>
      <DataTable data={applications} columns={columns} searchPlaceholder="Buscar solicitud..." exportFileName="solicitudes" onRowClick={(a) => navigate(`/solicitudes/${a.id}`)}
        actions={<Button className="gap-2" onClick={() => navigate("/solicitudes/nuevo")}><Plus className="h-4 w-4" />Nueva Solicitud</Button>} />
      <DeleteConfirmDialog open={!!deleteTarget} onOpenChange={o => !o && setDeleteTarget(null)} title="Eliminar Solicitud"
        description={`¿Está seguro de eliminar la solicitud "${deleteTarget?.code}"?`}
        onConfirm={() => { if (deleteTarget) { deleteApplication(deleteTarget.id); toast.success("Solicitud eliminada"); setDeleteTarget(null); } }} />
    </div>
  );
}
