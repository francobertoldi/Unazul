import { useState } from "react";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Plus, Pencil, Trash2, Check, X } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { toast } from "sonner";
import type { ListItem } from "@/data/parameters";

interface ListParamEditorProps {
  items: ListItem[];
  onChange: (items: ListItem[]) => void;
}

const MAX_CODE = 16;
const MAX_DESC = 30;

export default function ListParamEditor({ items, onChange }: ListParamEditorProps) {
  const [newCode, setNewCode] = useState("");
  const [newDesc, setNewDesc] = useState("");
  const [editIndex, setEditIndex] = useState<number | null>(null);
  const [editCode, setEditCode] = useState("");
  const [editDesc, setEditDesc] = useState("");

  const addItem = () => {
    const code = newCode.trim();
    const desc = newDesc.trim();
    if (!code || !desc) { toast.error("Código y descripción son obligatorios"); return; }
    if (items.some(i => i.code === code)) { toast.error("El código ya existe"); return; }
    onChange([...items, { code, description: desc }]);
    setNewCode("");
    setNewDesc("");
  };

  const removeItem = (index: number) => {
    onChange(items.filter((_, i) => i !== index));
  };

  const startEdit = (index: number) => {
    setEditIndex(index);
    setEditCode(items[index].code);
    setEditDesc(items[index].description);
  };

  const saveEdit = () => {
    if (editIndex === null) return;
    const code = editCode.trim();
    const desc = editDesc.trim();
    if (!code || !desc) { toast.error("Código y descripción son obligatorios"); return; }
    if (items.some((item, i) => i !== editIndex && item.code === code)) { toast.error("El código ya existe"); return; }
    onChange(items.map((item, i) => i === editIndex ? { code, description: desc } : item));
    setEditIndex(null);
  };

  const cancelEdit = () => setEditIndex(null);

  return (
    <div className="space-y-2">
      {items.length > 0 && (
        <div className="border border-border rounded-md overflow-hidden">
          <div className="grid grid-cols-[1fr_1.5fr_auto] gap-0 bg-muted/50 px-2 py-1.5 text-[10px] font-semibold text-muted-foreground uppercase tracking-wider">
            <span>Código</span>
            <span>Descripción</span>
            <span className="w-14" />
          </div>
          {items.map((item, i) => (
            <div key={i} className="grid grid-cols-[1fr_1.5fr_auto] gap-0 items-center px-2 py-1.5 border-t border-border text-xs group">
              {editIndex === i ? (
                <>
                  <Input
                    value={editCode}
                    onChange={e => setEditCode(e.target.value.slice(0, MAX_CODE))}
                    className="h-7 text-xs"
                    maxLength={MAX_CODE}
                  />
                  <Input
                    value={editDesc}
                    onChange={e => setEditDesc(e.target.value.slice(0, MAX_DESC))}
                    className="h-7 text-xs ml-1"
                    maxLength={MAX_DESC}
                    onKeyDown={e => { if (e.key === "Enter") { e.preventDefault(); saveEdit(); } }}
                  />
                  <div className="flex gap-0.5 ml-1">
                    <Button type="button" size="icon" variant="ghost" className="h-6 w-6 text-primary" onClick={saveEdit} aria-label="Confirmar edición">
                      <Check className="h-3 w-3" />
                    </Button>
                    <Button type="button" size="icon" variant="ghost" className="h-6 w-6 text-muted-foreground" onClick={cancelEdit} aria-label="Cancelar edición">
                      <X className="h-3 w-3" />
                    </Button>
                  </div>
                </>
              ) : (
                <>
                  <code className="font-mono text-foreground truncate">{item.code}</code>
                  <span className="text-muted-foreground truncate ml-1">{item.description}</span>
                  <div className="flex gap-0.5 ml-1 opacity-0 group-hover:opacity-100 transition-opacity">
                    <Button type="button" size="icon" variant="ghost" className="h-6 w-6 text-muted-foreground hover:text-foreground" onClick={() => startEdit(i)} aria-label={`Editar ${item.code}`}>
                      <Pencil className="h-3 w-3" />
                    </Button>
                    <Button type="button" size="icon" variant="ghost" className="h-6 w-6 text-destructive hover:text-destructive" onClick={() => removeItem(i)} aria-label={`Eliminar ${item.code}`}>
                      <Trash2 className="h-3 w-3" />
                    </Button>
                  </div>
                </>
              )}
            </div>
          ))}
        </div>
      )}

      {items.length === 0 && (
        <p className="text-xs text-muted-foreground italic">Sin valores. Agregue items abajo.</p>
      )}

      <div className="flex gap-1.5 items-end">
        <div className="flex-1 space-y-0.5">
          <label className="text-[10px] text-muted-foreground">Código</label>
          <Input
            value={newCode}
            onChange={e => setNewCode(e.target.value.slice(0, MAX_CODE))}
            placeholder="Código"
            className="h-7 text-xs"
            maxLength={MAX_CODE}
          />
        </div>
        <div className="flex-[1.5] space-y-0.5">
          <label className="text-[10px] text-muted-foreground">Descripción</label>
          <Input
            value={newDesc}
            onChange={e => setNewDesc(e.target.value.slice(0, MAX_DESC))}
            placeholder="Descripción"
            className="h-7 text-xs"
            maxLength={MAX_DESC}
            onKeyDown={e => { if (e.key === "Enter") { e.preventDefault(); addItem(); } }}
          />
        </div>
        <Button type="button" size="sm" variant="outline" className="h-7 px-2 shrink-0" onClick={addItem} aria-label="Agregar valor">
          <Plus className="h-3.5 w-3.5" />
        </Button>
      </div>
      <Badge variant="outline" className="text-[10px]">{items.length} valor{items.length !== 1 ? "es" : ""}</Badge>
    </div>
  );
}
