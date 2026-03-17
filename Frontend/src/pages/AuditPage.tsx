import { DataTable } from "@/components/shared/DataTable";
import { useAuditStore } from "@/data/store";
import { AuditEntry } from "@/data/types";
import { Badge } from "@/components/ui/badge";
import { useMemo } from "react";

const operationColorMap: Record<string, string> = {
  Crear: "bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200",
  Editar: "bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200",
  Eliminar: "bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200",
  Login: "bg-emerald-100 text-emerald-800 dark:bg-emerald-900 dark:text-emerald-200",
  Logout: "bg-amber-100 text-amber-800 dark:bg-amber-900 dark:text-amber-200",
  "Cambiar Contraseña": "bg-orange-100 text-orange-800 dark:bg-orange-900 dark:text-orange-200",
  "Cambiar Estado": "bg-purple-100 text-purple-800 dark:bg-purple-900 dark:text-purple-200",
  Liquidar: "bg-indigo-100 text-indigo-800 dark:bg-indigo-900 dark:text-indigo-200",
  Exportar: "bg-cyan-100 text-cyan-800 dark:bg-cyan-900 dark:text-cyan-200",
  Consultar: "bg-gray-100 text-gray-800 dark:bg-gray-800 dark:text-gray-200",
};

export default function AuditPage() {
  const { auditLog } = useAuditStore();

  const operationTypeOptions = useMemo(() =>
    [...new Set(auditLog.map(a => a.operationType))].sort().map(o => ({ value: o, label: o })), [auditLog]);
  const actionOptions = useMemo(() =>
    [...new Set(auditLog.map(a => a.action))].sort().map(a => ({ value: a, label: a })), [auditLog]);
  const moduleOptions = useMemo(() =>
    [...new Set(auditLog.map(a => a.module))].sort().map(m => ({ value: m, label: m })), [auditLog]);
  const userOptions = useMemo(() =>
    [...new Set(auditLog.map(a => a.userName))].sort().map(u => ({ value: u, label: u })), [auditLog]);

  const columns = [
    {
      key: "timestamp", header: "Fecha/Hora", searchable: false, filterType: "date" as const, sortable: true,
      render: (a: AuditEntry) => (
        <span className="text-sm text-muted-foreground">
          {new Date(a.timestamp).toLocaleString("es-AR", { dateStyle: "short", timeStyle: "short" })}
        </span>
      ),
    },
    { key: "userName", header: "Usuario", searchable: true, filterType: "multiselect" as const, filterOptions: userOptions, sortable: true, render: (a: AuditEntry) => <span className="text-sm font-medium">{a.userName}</span> },
    {
      key: "operationType", header: "Tipo", searchable: false, filterType: "multiselect" as const, filterOptions: operationTypeOptions, sortable: true,
      render: (a: AuditEntry) => (
        <Badge className={`text-xs ${operationColorMap[a.operationType] || "bg-muted text-muted-foreground"}`}>
          {a.operationType}
        </Badge>
      ),
    },
    { key: "action", header: "Acción", searchable: true, filterType: "multiselect" as const, filterOptions: actionOptions, sortable: true, render: (a: AuditEntry) => <Badge variant="secondary" className="text-xs">{a.action}</Badge> },
    { key: "module", header: "Módulo", searchable: true, filterType: "multiselect" as const, filterOptions: moduleOptions, sortable: true, render: (a: AuditEntry) => <span className="text-sm">{a.module}</span> },
    { key: "detail", header: "Detalle", searchable: true, filterType: "text" as const, sortable: true, render: (a: AuditEntry) => <span className="text-sm">{a.detail}</span> },
    { key: "ip", header: "IP", searchable: false, filterable: false, render: (a: AuditEntry) => <span className="text-xs text-muted-foreground font-mono">{a.ip}</span> },
  ];

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">Auditoría</h1>
        <p className="text-muted-foreground text-sm mt-1">Log de actividad del sistema</p>
      </div>
      <DataTable data={auditLog} columns={columns} searchPlaceholder="Buscar en auditoría..." exportFileName="auditoria" />
    </div>
  );
}
