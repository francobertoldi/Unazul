import { useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { ArrowLeft, Building2, MapPin, Mail, Phone, Globe, Plus, Pencil, Trash2, Package } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { DataTable } from "@/components/shared/DataTable";
import { StatusBadge } from "@/components/shared/StatusBadge";
import { useEntityStore, useProductStore } from "@/data/store";
import { Branch } from "@/data/types";
import { DeleteConfirmDialog } from "@/components/crud/DeleteConfirmDialog";
import { toast } from "sonner";
import EntityProductsTab from "@/components/entities/EntityProductsTab";

export default function EntityDetail() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { entities, deleteEntity, deleteBranch } = useEntityStore();
  const { products } = useProductStore();
  const entity = entities.find(e => e.id === id);
  const [deleteBranchTarget, setDeleteBranchTarget] = useState<Branch | null>(null);
  const [deleteEntityOpen, setDeleteEntityOpen] = useState(false);

  if (!entity) {
    return <div className="flex items-center justify-center h-64"><p className="text-muted-foreground">Entidad no encontrada</p></div>;
  }

  const entityProductCount = products.filter(p => p.entityId === id).length;

  const branchColumns = [
    { key: "name", header: "Sucursal", searchable: true, render: (b: Branch) => (<div><p className="font-medium text-sm">{b.name}</p><p className="text-xs text-muted-foreground">Código: {b.code}</p></div>) },
    { key: "address", header: "Dirección", searchable: true, render: (b: Branch) => <span className="text-sm">{b.address}</span> },
    { key: "city", header: "Ciudad", searchable: true, render: (b: Branch) => <span className="text-sm">{b.city}</span> },
    { key: "manager", header: "Responsable", searchable: true, render: (b: Branch) => <span className="text-sm">{b.manager}</span> },
    { key: "phone", header: "Teléfono", searchable: false, render: (b: Branch) => <span className="text-sm">{b.phone}</span> },
    { key: "status", header: "Estado", searchable: false, render: (b: Branch) => <StatusBadge status={b.status} /> },
    {
      key: "_actions", header: "", searchable: false,
      render: (b: Branch) => (
        <div className="flex items-center gap-1">
          <Button variant="ghost" size="icon" className="h-8 w-8" onClick={() => navigate(`/entidades/${id}/sucursales/${b.id}/editar`)}><Pencil className="h-3.5 w-3.5" /></Button>
          <Button variant="ghost" size="icon" className="h-8 w-8 text-destructive hover:text-destructive" onClick={() => setDeleteBranchTarget(b)}><Trash2 className="h-3.5 w-3.5" /></Button>
        </div>
      ),
    },
  ];

  const infoItems = [
    { icon: Building2, label: "Organización", value: entity.tenantName },
    { icon: Mail, label: "Email", value: entity.email },
    { icon: Phone, label: "Teléfono", value: entity.phone },
    { icon: MapPin, label: "Dirección", value: `${entity.address}, ${entity.city}` },
    { icon: Globe, label: "País", value: entity.country },
  ];

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-3">
        <Button variant="ghost" size="icon" onClick={() => navigate("/entidades")}><ArrowLeft className="h-4 w-4" /></Button>
        <div className="flex-1">
          <div className="flex items-center gap-3">
            <div className="h-10 w-10 rounded-xl bg-primary/10 flex items-center justify-center"><Building2 className="h-5 w-5 text-primary" /></div>
            <div><h1 className="text-xl font-bold tracking-tight">{entity.name}</h1><p className="text-sm text-muted-foreground">{entity.identifier}</p></div>
            <StatusBadge status={entity.status} />
          </div>
        </div>
        <div className="flex gap-2">
          <Button variant="outline" className="gap-2" onClick={() => navigate(`/entidades/${id}/editar`)}><Pencil className="h-4 w-4" /> Editar</Button>
          <Button variant="outline" className="gap-2 text-destructive hover:text-destructive" onClick={() => setDeleteEntityOpen(true)}><Trash2 className="h-4 w-4" /> Eliminar</Button>
        </div>
      </div>

      <Tabs defaultValue="info" className="space-y-4">
        <TabsList>
          <TabsTrigger value="info">Información</TabsTrigger>
          <TabsTrigger value="branches">Sucursales ({entity.branches.length})</TabsTrigger>
          <TabsTrigger value="products">Productos ({entityProductCount})</TabsTrigger>
          <TabsTrigger value="channels">Canales</TabsTrigger>
        </TabsList>

        <TabsContent value="info">
          <Card>
            <CardHeader className="pb-3"><CardTitle className="text-base">Datos de la Entidad</CardTitle></CardHeader>
            <CardContent>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                {infoItems.map(item => (
                  <div key={item.label} className="flex items-start gap-3">
                    <div className="h-9 w-9 rounded-lg bg-secondary flex items-center justify-center shrink-0"><item.icon className="h-4 w-4 text-muted-foreground" /></div>
                    <div><p className="text-xs text-muted-foreground">{item.label}</p><p className="text-sm font-medium">{item.value}</p></div>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="branches">
          <DataTable data={entity.branches} columns={branchColumns} searchPlaceholder="Buscar sucursal..." exportFileName={`sucursales_${entity.name.replace(/\s+/g, '_')}`}
            actions={<Button className="gap-2" size="sm" onClick={() => navigate(`/entidades/${id}/sucursales/nuevo`)}><Plus className="h-4 w-4" /> Nueva Sucursal</Button>} />
        </TabsContent>

        <TabsContent value="products">
          <EntityProductsTab entityId={entity.id} entityName={entity.name} />
        </TabsContent>

        <TabsContent value="channels">
          <Card>
            <CardHeader className="pb-3"><CardTitle className="text-base">Canales Habilitados</CardTitle></CardHeader>
            <CardContent>
              <div className="flex gap-3 flex-wrap">
                {entity.channels.map(ch => (
                  <div key={ch} className="flex items-center gap-2 rounded-xl border px-4 py-3 bg-card">
                    <div className="h-8 w-8 rounded-lg bg-primary/10 flex items-center justify-center"><Globe className="h-4 w-4 text-primary" /></div>
                    <span className="text-sm font-medium capitalize">{ch}</span>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>

      <DeleteConfirmDialog open={!!deleteBranchTarget} onOpenChange={o => !o && setDeleteBranchTarget(null)} title="Eliminar Sucursal" description={`¿Está seguro de eliminar "${deleteBranchTarget?.name}"?`}
        onConfirm={() => { if (deleteBranchTarget) { deleteBranch(entity.id, deleteBranchTarget.id); toast.success("Sucursal eliminada"); setDeleteBranchTarget(null); } }} />
      <DeleteConfirmDialog open={deleteEntityOpen} onOpenChange={setDeleteEntityOpen} title="Eliminar Entidad" description={`¿Está seguro de eliminar "${entity.name}"? Se eliminarán también todas sus sucursales.`}
        onConfirm={() => { deleteEntity(entity.id); toast.success("Entidad eliminada"); navigate("/entidades"); }} />
    </div>
  );
}
