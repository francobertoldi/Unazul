import { useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { ArrowLeft, Layers, Mail, Phone, User, Globe, Building2, Pencil, Trash2 } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { StatusBadge } from "@/components/shared/StatusBadge";
import { DataTable } from "@/components/shared/DataTable";
import { useTenantStore, useEntityStore } from "@/data/store";
import { Entity } from "@/data/types";
import { DeleteConfirmDialog } from "@/components/crud/DeleteConfirmDialog";
import { toast } from "sonner";

const typeLabels: Record<string, string> = { bank: "Banco", insurance: "Aseguradora", fintech: "Fintech", cooperative: "Cooperativa" };

export default function TenantDetail() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { tenants, deleteTenant } = useTenantStore();
  const { entities } = useEntityStore();
  const tenant = tenants.find(t => t.id === id);
  const [deleteOpen, setDeleteOpen] = useState(false);

  if (!tenant) return <div className="flex items-center justify-center h-64"><p className="text-muted-foreground">Organización no encontrada</p></div>;

  const tenantEntities = entities.filter(e => e.tenantId === tenant.id);

  const entityColumns = [
    {
      key: "name", header: "Entidad", searchable: true,
      render: (e: Entity) => (
        <div className="flex items-center gap-3">
          <div className="h-9 w-9 rounded-lg bg-primary/10 flex items-center justify-center shrink-0"><Building2 className="h-4 w-4 text-primary" /></div>
          <div><p className="font-medium text-sm">{e.name}</p><p className="text-xs text-muted-foreground">{e.identifier}</p></div>
        </div>
      ),
    },
    { key: "type", header: "Tipo", searchable: false, render: (e: Entity) => <span className="text-sm">{typeLabels[e.type] || e.type}</span> },
    { key: "city", header: "Ciudad", searchable: true, render: (e: Entity) => <span className="text-sm">{e.city}</span> },
    { key: "status", header: "Estado", searchable: false, render: (e: Entity) => <StatusBadge status={e.status} /> },
  ];

  const infoItems = [
    { icon: User, label: "Contacto", value: tenant.contactName },
    { icon: Mail, label: "Email", value: tenant.contactEmail },
    { icon: Phone, label: "Teléfono", value: tenant.contactPhone },
    { icon: Globe, label: "País", value: tenant.country },
  ];

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-3">
        <Button variant="ghost" size="icon" onClick={() => navigate("/tenants")}><ArrowLeft className="h-4 w-4" /></Button>
        <div className="flex-1">
          <div className="flex items-center gap-3">
            <div className="h-10 w-10 rounded-xl bg-accent/10 flex items-center justify-center"><Layers className="h-5 w-5 text-accent" /></div>
            <div><h1 className="text-xl font-bold tracking-tight">{tenant.name}</h1><p className="text-sm text-muted-foreground">{tenant.identifier}</p></div>
            <StatusBadge status={tenant.status} />
          </div>
        </div>
        <div className="flex gap-2">
          <Button variant="outline" className="gap-2" onClick={() => navigate(`/tenants/${id}/editar`)}><Pencil className="h-4 w-4" /> Editar</Button>
          <Button variant="outline" className="gap-2 text-destructive hover:text-destructive" onClick={() => setDeleteOpen(true)}><Trash2 className="h-4 w-4" /> Eliminar</Button>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <Card className="lg:col-span-1">
          <CardHeader className="pb-3"><CardTitle className="text-base">Información</CardTitle></CardHeader>
          <CardContent className="space-y-4">
            {infoItems.map(item => (
              <div key={item.label} className="flex items-start gap-3">
                <div className="h-9 w-9 rounded-lg bg-secondary flex items-center justify-center shrink-0"><item.icon className="h-4 w-4 text-muted-foreground" /></div>
                <div><p className="text-xs text-muted-foreground">{item.label}</p><p className="text-sm font-medium">{item.value}</p></div>
              </div>
            ))}
            {tenant.description && (
              <div className="pt-2 border-t border-border">
                <p className="text-xs text-muted-foreground mb-1">Descripción</p>
                <p className="text-sm">{tenant.description}</p>
              </div>
            )}
          </CardContent>
        </Card>

        <div className="lg:col-span-2">
          <h2 className="text-lg font-semibold tracking-tight mb-4">Entidades de la Organización ({tenantEntities.length})</h2>
          <DataTable data={tenantEntities} columns={entityColumns} searchPlaceholder="Buscar entidad..." exportFileName={`entidades_${tenant.name.replace(/\s+/g, '_')}`}
            onRowClick={(e) => navigate(`/entidades/${e.id}`)} />
        </div>
      </div>

      <DeleteConfirmDialog open={deleteOpen} onOpenChange={setDeleteOpen} title="Eliminar Organización" description={`¿Está seguro de eliminar "${tenant.name}"?`}
        onConfirm={() => { deleteTenant(tenant.id); toast.success("Organización eliminada"); navigate("/tenants"); }} />
    </div>
  );
}
