import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Building2, Plus, Pencil, Trash2 } from "lucide-react";
import { Button } from "@/components/ui/button";
import { DataTable } from "@/components/shared/DataTable";
import { StatusBadge } from "@/components/shared/StatusBadge";
import { useEntityStore, useTenantStore } from "@/data/store";
import { Entity } from "@/data/types";
import { DeleteConfirmDialog } from "@/components/crud/DeleteConfirmDialog";
import { toast } from "sonner";

const typeLabels: Record<string, string> = { bank: "Banco", insurance: "Aseguradora", fintech: "Fintech", cooperative: "Cooperativa" };
const statusOptions = [
  { value: "active", label: "Activo" },
  { value: "inactive", label: "Inactivo" },
  { value: "suspended", label: "Suspendido" },
];
const typeOptions = Object.entries(typeLabels).map(([value, label]) => ({ value, label }));

export default function EntityList() {
  const navigate = useNavigate();
  const { entities, deleteEntity } = useEntityStore();
  const { tenants } = useTenantStore();
  const [deleteTarget, setDeleteTarget] = useState<Entity | null>(null);

  const orgOptions = tenants.map(t => ({ value: t.name, label: t.name }));

  const columns = [
    {
      key: "name", header: "Entidad", searchable: true, filterType: "text" as const, sortable: true,
      render: (e: Entity) => (
        <div className="flex items-center gap-3">
          <div className="h-9 w-9 rounded-lg bg-primary/10 flex items-center justify-center shrink-0"><Building2 className="h-4 w-4 text-primary" /></div>
          <div><p className="font-medium text-sm">{e.name}</p><p className="text-xs text-muted-foreground">{e.identifier}</p></div>
        </div>
      ),
    },
    { key: "tenantName", header: "Organización", searchable: true, filterType: "multiselect" as const, filterOptions: orgOptions, sortable: true, render: (e: Entity) => <span className="text-sm">{e.tenantName}</span> },
    { key: "type", header: "Tipo", searchable: false, filterType: "multiselect" as const, filterOptions: typeOptions, sortable: true, render: (e: Entity) => <span className="text-sm">{typeLabels[e.type] || e.type}</span> },
    { key: "city", header: "Ciudad", searchable: true, filterType: "text" as const, sortable: true, render: (e: Entity) => <span className="text-sm">{e.city}, {e.country}</span> },
    { key: "branches", header: "Sucursales", searchable: false, sortable: true, sortType: "number" as const, sortValue: (e: Entity) => e.branches.length, render: (e: Entity) => <span className="text-sm font-medium">{e.branches.length}</span> },
    { key: "status", header: "Estado", searchable: false, filterType: "select" as const, filterOptions: statusOptions, sortable: true, render: (e: Entity) => <StatusBadge status={e.status} /> },
    { key: "createdAt", header: "Creación", searchable: false, filterType: "date" as const, sortable: true, render: (e: Entity) => <span className="text-xs text-muted-foreground">{e.createdAt}</span> },
    {
      key: "_actions", header: "", searchable: false,
      render: (e: Entity) => (
        <div className="flex items-center gap-1" onClick={ev => ev.stopPropagation()}>
          <Button variant="ghost" size="icon" className="h-8 w-8" onClick={() => navigate(`/entidades/${e.id}/editar`)}><Pencil className="h-3.5 w-3.5" /></Button>
          <Button variant="ghost" size="icon" className="h-8 w-8 text-destructive hover:text-destructive" onClick={() => setDeleteTarget(e)}><Trash2 className="h-3.5 w-3.5" /></Button>
        </div>
      ),
    },
  ];

  return (
    <div className="space-y-6">
      <div><h1 className="text-2xl font-bold tracking-tight">Entidades</h1><p className="text-muted-foreground text-sm mt-1">Gestión de entidades del sistema</p></div>
      <DataTable data={entities} columns={columns} searchPlaceholder="Buscar entidad..." exportFileName="entidades" onRowClick={(e) => navigate(`/entidades/${e.id}`)}
        actions={<Button className="gap-2" onClick={() => navigate("/entidades/nuevo")}><Plus className="h-4 w-4" />Nueva Entidad</Button>} />
      <DeleteConfirmDialog open={!!deleteTarget} onOpenChange={o => !o && setDeleteTarget(null)} title="Eliminar Entidad"
        description={`¿Está seguro de eliminar "${deleteTarget?.name}"? Esta acción no se puede deshacer.`}
        onConfirm={() => { if (deleteTarget) { deleteEntity(deleteTarget.id); toast.success("Entidad eliminada"); setDeleteTarget(null); } }} />
    </div>
  );
}
