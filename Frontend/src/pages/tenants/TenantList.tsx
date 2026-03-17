import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Layers, Plus, Pencil, Trash2 } from "lucide-react";
import { Button } from "@/components/ui/button";
import { DataTable } from "@/components/shared/DataTable";
import { StatusBadge } from "@/components/shared/StatusBadge";
import { useTenantStore } from "@/data/store";
import { useParameterStore } from "@/data/store";
import { Tenant } from "@/data/types";
import { DeleteConfirmDialog } from "@/components/crud/DeleteConfirmDialog";
import { toast } from "sonner";

const statusOptions = [
  { value: "active", label: "Activo" },
  { value: "inactive", label: "Inactivo" },
];

export default function TenantList() {
  const navigate = useNavigate();
  const { tenants, deleteTenant } = useTenantStore();
  const { parameters } = useParameterStore();
  const [deleteTarget, setDeleteTarget] = useState<Tenant | null>(null);

  const countryOptions = parameters
    .filter(p => p.group === 'countries')
    .map(p => ({ value: p.value, label: p.value }))
    .sort((a, b) => a.label.localeCompare(b.label));

  const columns = [
    {
      key: "name", header: "Organización", searchable: true, filterType: "text" as const, sortable: true,
      render: (t: Tenant) => (
        <div className="flex items-center gap-3">
          <div className="h-9 w-9 rounded-lg bg-accent/10 flex items-center justify-center shrink-0"><Layers className="h-4 w-4 text-accent" /></div>
          <div><p className="font-medium text-sm">{t.name}</p><p className="text-xs text-muted-foreground">{t.identifier}</p></div>
        </div>
      ),
    },
    { key: "contactName", header: "Contacto", searchable: true, filterType: "text" as const, sortable: true, render: (t: Tenant) => <div><p className="text-sm">{t.contactName}</p><p className="text-xs text-muted-foreground">{t.contactEmail}</p></div> },
    { key: "country", header: "País", searchable: true, filterType: "multiselect" as const, filterOptions: countryOptions, sortable: true, render: (t: Tenant) => <span className="text-sm">{t.country}</span> },
    { key: "status", header: "Estado", searchable: false, filterType: "select" as const, filterOptions: statusOptions, sortable: true, render: (t: Tenant) => <StatusBadge status={t.status} /> },
    { key: "createdAt", header: "Creación", searchable: false, filterType: "date" as const, sortable: true, render: (t: Tenant) => <span className="text-xs text-muted-foreground">{t.createdAt}</span> },
    {
      key: "_actions", header: "", searchable: false,
      render: (t: Tenant) => (
        <div className="flex items-center gap-1" onClick={ev => ev.stopPropagation()}>
          <Button variant="ghost" size="icon" className="h-8 w-8" onClick={() => navigate(`/tenants/${t.id}/editar`)}><Pencil className="h-3.5 w-3.5" /></Button>
          <Button variant="ghost" size="icon" className="h-8 w-8 text-destructive hover:text-destructive" onClick={() => setDeleteTarget(t)}><Trash2 className="h-3.5 w-3.5" /></Button>
        </div>
      ),
    },
  ];

  return (
    <div className="space-y-6">
      <div><h1 className="text-2xl font-bold tracking-tight">Organizaciones</h1><p className="text-muted-foreground text-sm mt-1">Gestión de organizaciones (grupos de entidades)</p></div>
      <DataTable data={tenants} columns={columns} searchPlaceholder="Buscar organización..." exportFileName="organizaciones" onRowClick={(t) => navigate(`/tenants/${t.id}`)}
        actions={<Button className="gap-2" onClick={() => navigate("/tenants/nuevo")}><Plus className="h-4 w-4" />Nueva Organización</Button>} />
      <DeleteConfirmDialog open={!!deleteTarget} onOpenChange={o => !o && setDeleteTarget(null)} title="Eliminar Organización"
        description={`¿Está seguro de eliminar "${deleteTarget?.name}"? Las entidades asociadas quedarán sin organización.`}
        onConfirm={() => { if (deleteTarget) { deleteTenant(deleteTarget.id); toast.success("Organización eliminada"); setDeleteTarget(null); } }} />
    </div>
  );
}
