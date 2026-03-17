import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Package, Plus, Pencil, Trash2 } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { DataTable } from "@/components/shared/DataTable";
import { StatusBadge } from "@/components/shared/StatusBadge";
import { DeleteConfirmDialog } from "@/components/crud/DeleteConfirmDialog";
import { useProductStore, useProductFamilyStore, useEntityStore } from "@/data/store";
import { Product } from "@/data/types";
import { toast } from "sonner";

const statusOptions = [
  { value: "draft", label: "Borrador" },
  { value: "active", label: "Activo" },
  { value: "inactive", label: "Inactivo" },
  { value: "deprecated", label: "Deprecado" },
];

export default function ProductList() {
  const navigate = useNavigate();
  const { products, deleteProduct } = useProductStore();
  const { families } = useProductFamilyStore();
  const { entities } = useEntityStore();
  const [deleteTarget, setDeleteTarget] = useState<Product | null>(null);

  const familyOptions = families.map(f => ({ value: f.id, label: f.description }));
  const entityOptions = [...new Set(products.map(p => p.entityName))].sort().map(name => ({ value: name, label: name }));

  const columns = [
    {
      key: "name", header: "Producto", searchable: true, filterType: "text" as const, sortable: true,
      render: (p: Product) => (
        <div className="flex items-center gap-3">
          <div className="h-9 w-9 rounded-lg bg-primary/10 flex items-center justify-center shrink-0"><Package className="h-4 w-4 text-primary" /></div>
          <div><p className="font-medium text-sm">{p.name}</p><p className="text-xs text-muted-foreground">{p.code} · v{p.version}</p></div>
        </div>
      ),
    },
    { key: "familyName", header: "Familia", searchable: true, filterType: "multiselect" as const, filterOptions: familyOptions, filterKey: "familyId", sortable: true, render: (p: Product) => <Badge variant="outline" className="text-xs">{p.familyName}</Badge> },
    { key: "entityName", header: "Entidad", searchable: true, filterType: "multiselect" as const, filterOptions: entityOptions, sortable: true, render: (p: Product) => <span className="text-sm">{p.entityName}</span> },
    { key: "plans", header: "Sub productos", searchable: false, sortable: true, sortType: "number" as const, sortValue: (p: Product) => p.plans.length, render: (p: Product) => <span className="text-sm font-medium">{p.plans.length}</span> },
    { key: "status", header: "Estado", searchable: false, filterType: "select" as const, filterOptions: statusOptions, sortable: true, render: (p: Product) => <StatusBadge status={p.status} /> },
    { key: "validFrom", header: "Vigencia", searchable: false, filterType: "date" as const, sortable: true, render: (p: Product) => <span className="text-xs text-muted-foreground">{p.validFrom}{p.validTo ? ` – ${p.validTo}` : ""}</span> },
    {
      key: "_actions", header: "", searchable: false,
      render: (p: Product) => (
        <div className="flex items-center gap-1" onClick={ev => ev.stopPropagation()}>
          <Button variant="ghost" size="icon" className="h-8 w-8" onClick={() => navigate(`/productos/${p.id}/editar`)}><Pencil className="h-3.5 w-3.5" /></Button>
          <Button variant="ghost" size="icon" className="h-8 w-8 text-destructive hover:text-destructive" onClick={() => setDeleteTarget(p)}><Trash2 className="h-3.5 w-3.5" /></Button>
        </div>
      ),
    },
  ];

  return (
    <div className="space-y-6">
      <div><h1 className="text-2xl font-bold tracking-tight">Catálogo de Productos</h1><p className="text-muted-foreground text-sm mt-1">Gestión de productos, sub productos y coberturas</p></div>
      <DataTable data={products} columns={columns} searchPlaceholder="Buscar producto..." exportFileName="productos" onRowClick={(p) => navigate(`/productos/${p.id}`)}
        actions={<Button className="gap-2" onClick={() => navigate("/productos/nuevo")}><Plus className="h-4 w-4" />Nuevo Producto</Button>} />
      <DeleteConfirmDialog open={!!deleteTarget} onOpenChange={o => !o && setDeleteTarget(null)} title="Eliminar Producto"
        description={`¿Está seguro de eliminar "${deleteTarget?.name}"?`}
        onConfirm={() => { if (deleteTarget) { deleteProduct(deleteTarget.id); toast.success("Producto eliminado"); setDeleteTarget(null); } }} />
    </div>
  );
}
