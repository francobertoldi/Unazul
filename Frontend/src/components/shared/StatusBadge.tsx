import { Badge } from "@/components/ui/badge";
import { cn } from "@/lib/utils";

const statusConfig: Record<string, { label: string; className: string }> = {
  active: { label: "Activo", className: "bg-[hsl(var(--success))] text-[hsl(var(--success-foreground))] hover:bg-[hsl(var(--success))]/90" },
  inactive: { label: "Inactivo", className: "bg-muted text-muted-foreground hover:bg-muted/90" },
  suspended: { label: "Suspendido", className: "bg-[hsl(var(--warning))] text-[hsl(var(--warning-foreground))] hover:bg-[hsl(var(--warning))]/90" },
  locked: { label: "Bloqueado", className: "bg-destructive text-destructive-foreground hover:bg-destructive/90" },
};

export function StatusBadge({ status }: { status: string }) {
  const config = statusConfig[status] || { label: status, className: "" };
  return (
    <Badge className={cn("text-[10px] font-semibold uppercase tracking-wide border-0", config.className)}>
      {config.label}
    </Badge>
  );
}
