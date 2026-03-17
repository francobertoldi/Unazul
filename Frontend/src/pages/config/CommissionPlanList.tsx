import { useState } from "react";
import { Plus, Pencil, Trash2, BadgePercent } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { DataTable } from "@/components/shared/DataTable";
import { DeleteConfirmDialog } from "@/components/crud/DeleteConfirmDialog";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from "@/components/ui/dialog";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { useCommissionPlanStore, useProductStore } from "@/data/store";
import { CommissionPlan, CommissionValueType } from "@/data/types";
import { toast } from "sonner";
import { cn } from "@/lib/utils";

// ── Helpers de formateo ──────────────────────────────────────────────────────

/** Formatea un número usando puntos como separadores de miles (sin decimales) */
const formatThousands = (raw: string): string => {
  const digits = raw.replace(/\D/g, "");
  if (!digits) return "";
  return Number(digits).toLocaleString("de-DE", { maximumFractionDigits: 0 });
};

/** Convierte la cadena formateada de vuelta a número */
const parseFormattedNumber = (formatted: string): number => {
  const clean = formatted.replace(/\./g, "").replace(/,/g, "");
  return Number(clean);
};

// ── Componente de input con prefijo ──────────────────────────────────────────

interface PrefixedInputProps {
  prefix: string;
  value: string;
  onChange: (raw: string) => void;
  placeholder?: string;
  isPercentage?: boolean;
  disabled?: boolean;
}

const PrefixedInput = ({ prefix, value, onChange, placeholder, isPercentage, disabled }: PrefixedInputProps) => {
  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const raw = e.target.value;

    if (isPercentage) {
      // Solo dígitos, máximo 3
      const digits = raw.replace(/\D/g, "").slice(0, 3);
      onChange(digits);
    } else {
      // Monetario: quitar puntos existentes, quedarse sólo con dígitos
      const digits = raw.replace(/\./g, "").replace(/\D/g, "");
      const formatted = formatThousands(digits);
      onChange(formatted);
    }
  };

  return (
    <div className="relative flex items-center">
      <span className={cn(
        "absolute left-3 select-none text-sm font-medium pointer-events-none",
        disabled ? "text-muted-foreground/50" : "text-muted-foreground"
      )}>
        {prefix}
      </span>
      <Input
        type="text"
        inputMode={isPercentage ? "numeric" : "decimal"}
        value={value}
        onChange={handleChange}
        placeholder={placeholder}
        disabled={disabled}
        className="pl-7"
        maxLength={isPercentage ? 3 : undefined}
      />
    </div>
  );
};

const valueTypeLabels: Record<CommissionValueType, string> = {
  fixed_per_sale: "Valor fijo por venta",
  percentage_capital: "Porcentaje del capital",
  percentage_total_loan: "Porcentaje del valor total del préstamo",
};

const formatCommissionValue = (cp: CommissionPlan) => {
  if (cp.valueType === "fixed_per_sale") return `$ ${cp.value.toLocaleString("es-AR")}`;
  return `${cp.value.toLocaleString("es-AR")} %`;
};

export default function CommissionPlanList() {
  const { commissionPlans, addCommissionPlan, updateCommissionPlan, deleteCommissionPlan } = useCommissionPlanStore();
  const { products } = useProductStore();

  const [deleteTarget, setDeleteTarget] = useState<CommissionPlan | null>(null);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editing, setEditing] = useState<CommissionPlan | null>(null);

  const [code, setCode] = useState("");
  const [description, setDescription] = useState("");
  const [valueType, setValueType] = useState<CommissionValueType>("fixed_per_sale");
  const [value, setValue] = useState<string>("");
  const [maxAmount, setMaxAmount] = useState<string>("");

  const isUsed = (cpId: string) => products.some(p => p.plans.some(pl => pl.commissionPlanId === cpId));

  const openNew = () => {
    setEditing(null);
    setCode("");
    setDescription("");
    setValueType("fixed_per_sale");
    setValue("");
    setMaxAmount("");
    setDialogOpen(true);
  };

  const isPercentage = valueType !== "fixed_per_sale";

  const openEdit = (cp: CommissionPlan) => {
    setEditing(cp);
    setCode(cp.code);
    setDescription(cp.description);
    setValueType(cp.valueType);
    // Al abrir edición: si era monetario, formatear con puntos; si porcentaje, sólo dígitos
    if (cp.valueType === "fixed_per_sale") {
      setValue(formatThousands(String(Math.round(cp.value))));
    } else {
      setValue(String(cp.value));
    }
    setMaxAmount(cp.maxAmount != null ? formatThousands(String(Math.round(cp.maxAmount))) : "");
    setDialogOpen(true);
  };

  // Cuando cambia el tipo, limpiar el valor para evitar inconsistencias
  const handleValueTypeChange = (v: CommissionValueType) => {
    setValueType(v);
    setValue("");
  };

  const handleSave = () => {
    const trimmedCode = code.trim();
    const trimmedDescription = description.trim();

    if (!trimmedCode || !trimmedDescription) {
      toast.error("Código y descripción son obligatorios");
      return;
    }
    if (trimmedCode.length > 15) {
      toast.error("El código no puede superar 15 caracteres");
      return;
    }
    if (trimmedDescription.length > 50) {
      toast.error("La descripción no puede superar 50 caracteres");
      return;
    }

    // Parsear valor según tipo
    const rawValue = value.trim();
    const parsedValue = rawValue ? (isPercentage ? Number(rawValue) : parseFormattedNumber(rawValue)) : Number.NaN;
    if (!Number.isFinite(parsedValue) || parsedValue < 0) {
      toast.error("El valor debe ser un número válido (>= 0)");
      return;
    }
    if (isPercentage && parsedValue > 100) {
      toast.error("Para comisiones porcentuales, el valor no puede superar 100%");
      return;
    }

    const rawMax = maxAmount.trim();
    const parsedMax = rawMax ? parseFormattedNumber(rawMax) : undefined;
    if (parsedMax !== undefined && (Number.isNaN(parsedMax) || parsedMax < 0)) {
      toast.error("El monto máximo debe ser un número válido (>= 0)");
      return;
    }

    // Evitar duplicados de código
    const codeKey = trimmedCode.toUpperCase();
    const duplicated = commissionPlans.some(p => p.id !== editing?.id && p.code.toUpperCase() === codeKey);
    if (duplicated) {
      toast.error("Ya existe un plan con ese código");
      return;
    }

    if (editing) {
      updateCommissionPlan(editing.id, { code: trimmedCode, description: trimmedDescription, valueType, value: parsedValue, maxAmount: parsedMax });
      toast.success("Plan de comisiones actualizado");
    } else {
      addCommissionPlan({ code: trimmedCode, description: trimmedDescription, valueType, value: parsedValue, maxAmount: parsedMax });
      toast.success("Plan de comisiones creado");
    }

    setDialogOpen(false);
  };

  const columns = [
    {
      key: "code", header: "Código", searchable: true, filterType: "text" as const, sortable: true,
      render: (cp: CommissionPlan) => (
        <div className="flex items-center gap-3">
          <div className="h-9 w-9 rounded-lg bg-primary/10 flex items-center justify-center shrink-0">
            <BadgePercent className="h-4 w-4 text-primary" />
          </div>
          <code className="text-sm font-mono font-medium">{cp.code}</code>
        </div>
      ),
    },
    {
      key: "description", header: "Descripción", searchable: true, filterType: "text" as const, sortable: true,
      render: (cp: CommissionPlan) => <span className="text-sm">{cp.description}</span>,
    },
    {
      key: "valueType", header: "Tipo de comisión", searchable: false, sortable: true,
      filterType: "multiselect" as const,
      filterOptions: Object.entries(valueTypeLabels).map(([value, label]) => ({ value, label })),
      render: (cp: CommissionPlan) => <span className="text-sm">{valueTypeLabels[cp.valueType]}</span>,
    },
    {
      key: "value", header: "Valor", searchable: false, sortable: true, sortType: "number" as const,
      render: (cp: CommissionPlan) => <span className="text-sm">{formatCommissionValue(cp)}</span>,
    },
    {
      key: "maxAmount", header: "Monto máximo", searchable: false, sortable: true, sortType: "number" as const,
      sortValue: (cp: CommissionPlan) => cp.maxAmount ?? 0,
      render: (cp: CommissionPlan) => (
        <span className="text-sm">
          {cp.maxAmount != null ? cp.maxAmount.toLocaleString("es-AR") : <span className="text-muted-foreground">—</span>}
        </span>
      ),
    },
    {
      key: "_actions",
      header: "",
      searchable: false,
      render: (cp: CommissionPlan) => (
        <div className="flex items-center gap-1" onClick={(ev) => ev.stopPropagation()}>
          <Button variant="ghost" size="icon" className="h-8 w-8" onClick={() => openEdit(cp)}>
            <Pencil className="h-3.5 w-3.5" />
          </Button>
          <Button
            variant="ghost"
            size="icon"
            className="h-8 w-8 text-destructive hover:text-destructive"
            onClick={() => setDeleteTarget(cp)}
          >
            <Trash2 className="h-3.5 w-3.5" />
          </Button>
        </div>
      ),
    },
  ];

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">Planes de comisiones</h1>
        <p className="text-muted-foreground text-sm mt-1">Configuración de opciones de pago de comisiones para entidades</p>
      </div>

      <DataTable
        data={commissionPlans}
        columns={columns}
        searchPlaceholder="Buscar plan de comisiones..."
        exportFileName="planes_comisiones"
        actions={
          <Button className="gap-2" onClick={openNew}>
            <Plus className="h-4 w-4" /> Nuevo Plan
          </Button>
        }
      />

      <DeleteConfirmDialog
        open={!!deleteTarget}
        onOpenChange={(o) => !o && setDeleteTarget(null)}
        title="Eliminar Plan de comisiones"
        description={`¿Está seguro de eliminar "${deleteTarget?.description}"?`}
        onConfirm={() => {
          if (!deleteTarget) return;
          if (isUsed(deleteTarget.id)) {
            toast.error("No se puede eliminar: el plan está asignado a un sub producto");
            return;
          }
          deleteCommissionPlan(deleteTarget.id);
          toast.success("Plan de comisiones eliminado");
          setDeleteTarget(null);
        }}
      />

      <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
        <DialogContent className="max-w-md">
          <DialogHeader>
            <DialogTitle>{editing ? "Editar Plan de comisiones" : "Nuevo Plan de comisiones"}</DialogTitle>
          </DialogHeader>

          <div className="space-y-4 mt-2">
            <div className="space-y-1.5">
              <Label>
                Código * <span className="text-muted-foreground text-xs">(máx. 15)</span>
              </Label>
              <Input value={code} onChange={(e) => setCode(e.target.value)} maxLength={15} placeholder="COM-FIJA-001" />
            </div>

            <div className="space-y-1.5">
              <Label>
                Descripción * <span className="text-muted-foreground text-xs">(máx. 50)</span>
              </Label>
              <Input value={description} onChange={(e) => setDescription(e.target.value)} maxLength={50} placeholder="Ej: Valor fijo por venta" />
            </div>

            <div className="space-y-1.5">
              <Label>Tipo de comisión *</Label>
              <Select value={valueType} onValueChange={(v) => handleValueTypeChange(v as CommissionValueType)}>
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  {Object.entries(valueTypeLabels).map(([k, v]) => (
                    <SelectItem key={k} value={k}>
                      {v}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div className="space-y-1.5">
              <Label>
                Valor *{" "}
                <span className="text-muted-foreground text-xs">
                  ({isPercentage ? "máx. 3 dígitos, 0–100" : "monto en $"})
                </span>
              </Label>
              <PrefixedInput
                prefix={isPercentage ? "%" : "$"}
                value={value}
                onChange={setValue}
                isPercentage={isPercentage}
                placeholder={isPercentage ? "Ej: 25" : "Ej: 15.000"}
              />
            </div>

            <div className="space-y-1.5">
              <Label>Monto máximo (opcional)</Label>
              <PrefixedInput
                prefix="$"
                value={maxAmount}
                onChange={setMaxAmount}
                isPercentage={false}
                placeholder="Ej: 150.000"
              />
            </div>
          </div>

          <DialogFooter className="mt-4">
            <Button variant="outline" onClick={() => setDialogOpen(false)}>
              Cancelar
            </Button>
            <Button onClick={handleSave}>{editing ? "Guardar" : "Crear"}</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
