import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Building2, Layers, MapPin, Globe, ChevronDown, ChevronRight } from "lucide-react";
import { useTenantStore, useEntityStore } from "@/data/store";
import { cn } from "@/lib/utils";
import { ScrollArea, ScrollBar } from "@/components/ui/scroll-area";

/* ── Status badge ── */
function StatusDot({ status }: { status?: string }) {
  if (!status) return null;
  return (
    <span
      className={cn(
        "inline-block h-2 w-2 rounded-full",
        status === "active" && "bg-green-500",
        status === "inactive" && "bg-muted-foreground/40",
        status === "suspended" && "bg-yellow-500",
      )}
    />
  );
}

/* ── Card node ── */
interface OrgCardProps {
  label: string;
  sublabel?: string;
  icon: React.ReactNode;
  status?: string;
  onClick?: () => void;
  variant?: "root" | "org" | "entity" | "branch";
}

function OrgCard({ label, sublabel, icon, status, onClick, variant = "org" }: OrgCardProps) {
  return (
    <div
      onClick={(e) => { e.stopPropagation(); onClick?.(); }}
      className={cn(
        "flex items-center gap-2.5 rounded-xl border px-4 py-3 shadow-sm transition-all cursor-pointer hover:shadow-md min-w-[170px] max-w-[220px]",
        variant === "root" && "bg-org-root text-org-root-foreground border-org-root shadow-md px-6 py-4",
        variant === "org" && "bg-org-level1 text-org-level1-foreground border-org-level1 hover:border-org-level1-foreground/30",
        variant === "entity" && "bg-org-level2 text-org-level2-foreground border-org-level2 hover:border-org-level2-foreground/30",
        variant === "branch" && "bg-org-level3 text-org-level3-foreground border-org-level3 hover:border-org-level3-foreground/30",
      )}
    >
      <span className={cn("shrink-0", variant === "root" ? "text-org-root-foreground" : "text-muted-foreground")}>{icon}</span>
      <div className="min-w-0 flex-1">
        <p className={cn("text-sm font-medium truncate", variant === "root" && "text-base font-bold")}>{label}</p>
        {sublabel && <p className="text-xs text-muted-foreground truncate">{sublabel}</p>}
      </div>
      <StatusDot status={status} />
    </div>
  );
}

/* ── Vertical connector line ── */
function VLine({ className }: { className?: string }) {
  return <div className={cn("w-px bg-border mx-auto", className)} />;
}

/* ── Horizontal connector for siblings ── */
function HLine() {
  return <div className="h-px bg-border flex-1" />;
}

/* ── Collapsible children wrapper with org-chart connectors ── */
interface ChildrenGroupProps {
  children: React.ReactNode;
  count: number;
  defaultOpen?: boolean;
}

function ChildrenGroup({ children, count, defaultOpen = true }: ChildrenGroupProps) {
  const [open, setOpen] = useState(defaultOpen);

  if (count === 0) return null;

  return (
    <div className="flex flex-col items-center">
      {/* Toggle button on the connector line */}
      <div className="flex flex-col items-center">
        <VLine className="h-4" />
        <button
          onClick={(e) => { e.stopPropagation(); setOpen(!open); }}
          className="flex items-center justify-center h-5 w-5 rounded-full border bg-card text-muted-foreground hover:bg-accent hover:text-accent-foreground transition-colors z-10"
        >
          {open ? <ChevronDown className="h-3 w-3" /> : <ChevronRight className="h-3 w-3" />}
        </button>
      </div>

      {open && (
        <>
          <VLine className="h-4" />
          {/* Horizontal rail + children */}
          <div className="relative flex items-start">
            {/* Horizontal connector across all children */}
            {count > 1 && (
              <div className="absolute top-0 left-1/2 -translate-x-1/2 flex items-center" style={{ width: `calc(100% - 170px)` }}>
                <HLine />
              </div>
            )}
            <div className="flex gap-6">
              {/* Each child column */}
              {children}
            </div>
          </div>
        </>
      )}
    </div>
  );
}

/* ── Single column for a child node + its descendants ── */
function NodeColumn({ card, children, childCount, childrenDefaultOpen }: {
  card: React.ReactNode;
  children?: React.ReactNode;
  childCount?: number;
  childrenDefaultOpen?: boolean;
}) {
  return (
    <div className="flex flex-col items-center">
      <VLine className="h-4" />
      {card}
      {children && childCount !== undefined && childCount > 0 && (
        <ChildrenGroup count={childCount} defaultOpen={childrenDefaultOpen}>
          {children}
        </ChildrenGroup>
      )}
    </div>
  );
}

/* ── Main page ── */
export default function OrgTreePage() {
  const { tenants } = useTenantStore();
  const { entities } = useEntityStore();
  const navigate = useNavigate();

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">Estructura Organizacional</h1>
        <p className="text-muted-foreground">Vista jerárquica de la plataforma</p>
      </div>

      <ScrollArea className="w-full">
        <div className="min-w-max pb-8 pt-2 px-4">
          {/* ── Root: Unazul ── */}
          <div className="flex flex-col items-center">
            <OrgCard
              label="Unazul"
              sublabel="Plataforma"
              icon={<Globe className="h-5 w-5" />}
              variant="root"
            />

            <ChildrenGroup count={tenants.length} defaultOpen>
              {tenants.map((tenant) => {
                const tenantEntities = entities.filter((e) => e.tenantId === tenant.id);
                return (
                  <NodeColumn
                    key={tenant.id}
                    card={
                      <OrgCard
                        label={tenant.name}
                        sublabel="Organización"
                        icon={<Layers className="h-4 w-4" />}
                        status={tenant.status}
                        variant="org"
                        onClick={() => navigate(`/tenants/${tenant.id}`)}
                      />
                    }
                    childCount={tenantEntities.length}
                    childrenDefaultOpen
                  >
                    {tenantEntities.map((entity) => (
                      <NodeColumn
                        key={entity.id}
                        card={
                          <OrgCard
                            label={entity.name}
                            sublabel="Entidad"
                            icon={<Building2 className="h-4 w-4" />}
                            status={entity.status}
                            variant="entity"
                            onClick={() => navigate(`/entidades/${entity.id}`)}
                          />
                        }
                        childCount={entity.branches.length}
                        childrenDefaultOpen={false}
                      >
                        {entity.branches.map((branch) => (
                          <NodeColumn
                            key={branch.id}
                            card={
                              <OrgCard
                                label={branch.name}
                                sublabel={`Sucursal · ${branch.code}`}
                                icon={<MapPin className="h-4 w-4" />}
                                status={branch.status}
                                variant="branch"
                              />
                            }
                          />
                        ))}
                      </NodeColumn>
                    ))}
                  </NodeColumn>
                );
              })}
            </ChildrenGroup>
          </div>
        </div>
        <ScrollBar orientation="horizontal" />
      </ScrollArea>
    </div>
  );
}
