import { useState } from "react";
import { Plus, Pencil, Trash2, FolderTree } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { DataTable } from "@/components/shared/DataTable";
import { DeleteConfirmDialog } from "@/components/crud/DeleteConfirmDialog";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from "@/components/ui/dialog";
import { useProductFamilyStore } from "@/data/store";
import { ProductFamily } from "@/data/types";
import { toast } from "sonner";

export default function ProductFamilyList() {
  const { families, addFamily, updateFamily, deleteFamily } = useProductFamilyStore();
  const [deleteTarget, setDeleteTarget] = useState<ProductFamily | null>(null);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editing, setEditing] = useState<ProductFamily | null>(null);
  const [code, setCode] = useState("");
  const [description, setDescription] = useState("");

  const openNew = () => { setEditing(null); setCode(""); setDescription(""); setDialogOpen(true); };
  const openEdit = (f: ProductFamily) => { setEditing(f); setCode(f.code); setDescription(f.description); setDialogOpen(true); };

  const handleSave = () => {
    if (!code.trim() || !description.trim()) { toast.error("Código y descripción son obligatorios"); return; }
    if (code.length > 15) { toast.error("El código no puede superar 15 caracteres"); return; }
    if (description.length > 30) { toast.error("La descripción no puede superar 30 caracteres"); return; }
    if (editing) {
      updateFamily(editing.id, { code, description });
      toast.success("Familia actualizada");
    } else {
      addFamily({ code, description });
      toast.success("Familia creada");
    }
    setDialogOpen(false);
  };

  const columns = [
    {
      key: "code", header: "Código", searchable: true, filterType: "text" as const, sortable: true,
      render: (f: ProductFamily) => (
        <div className="flex items-center gap-3">
          <div className="h-9 w-9 rounded-lg bg-primary/10 flex items-center justify-center shrink-0"><FolderTree className="h-4 w-4 text-primary" /></div>
          <code className="text-sm font-mono font-medium">{f.code}</code>
        </div>
      ),
    },
    { key: "description", header: "Descripción", searchable: true, filterType: "text" as const, sortable: true, render: (f: ProductFamily) => <span className="text-sm">{f.description}</span> },
    {
      key: "_actions", header: "", searchable: false,
      render: (f: ProductFamily) => (
        <div className="flex items-center gap-1" onClick={ev => ev.stopPropagation()}>
          <Button variant="ghost" size="icon" className="h-8 w-8" onClick={() => openEdit(f)}><Pencil className="h-3.5 w-3.5" /></Button>
          <Button variant="ghost" size="icon" className="h-8 w-8 text-destructive hover:text-destructive" onClick={() => setDeleteTarget(f)}><Trash2 className="h-3.5 w-3.5" /></Button>
        </div>
      ),
    },
  ];

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">Familias de Productos</h1>
        <p className="text-muted-foreground text-sm mt-1">Clasificación y agrupación de productos del catálogo</p>
      </div>
      <DataTable
        data={families}
        columns={columns}
        searchPlaceholder="Buscar familia..."
        exportFileName="familias_productos"
        actions={<Button className="gap-2" onClick={openNew}><Plus className="h-4 w-4" />Nueva Familia</Button>}
      />
      <DeleteConfirmDialog
        open={!!deleteTarget}
        onOpenChange={o => !o && setDeleteTarget(null)}
        title="Eliminar Familia"
        description={`¿Está seguro de eliminar "${deleteTarget?.description}"?`}
        onConfirm={() => { if (deleteTarget) { deleteFamily(deleteTarget.id); toast.success("Familia eliminada"); setDeleteTarget(null); } }}
      />
      <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
        <DialogContent className="max-w-sm">
          <DialogHeader><DialogTitle>{editing ? "Editar Familia" : "Nueva Familia"}</DialogTitle></DialogHeader>
          <div className="space-y-4 mt-2">
            <div className="space-y-1.5">
              <Label>Código * <span className="text-muted-foreground text-xs">(máx. 15)</span></Label>
              <Input value={code} onChange={e => setCode(e.target.value)} maxLength={15} placeholder="PREST" />
            </div>
            <div className="space-y-1.5">
              <Label>Descripción * <span className="text-muted-foreground text-xs">(máx. 30)</span></Label>
              <Input value={description} onChange={e => setDescription(e.target.value)} maxLength={30} placeholder="Préstamos" />
            </div>
          </div>
          <DialogFooter className="mt-4">
            <Button variant="outline" onClick={() => setDialogOpen(false)}>Cancelar</Button>
            <Button onClick={handleSave}>{editing ? "Guardar" : "Crear"}</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
