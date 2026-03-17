import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { GitBranch, Plus, Pencil, Trash2, ChevronRight, Clock } from "lucide-react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { StatusBadge } from "@/components/shared/StatusBadge";
import { DeleteConfirmDialog } from "@/components/crud/DeleteConfirmDialog";
import { useWorkflowStore } from "@/data/store";
import { WorkflowDefinition } from "@/data/types";
import { toast } from "sonner";

export default function WorkflowList() {
  const navigate = useNavigate();
  const { workflows, deleteWorkflow } = useWorkflowStore();
  const [selectedWf, setSelectedWf] = useState(workflows[0]?.id || "");
  const [deleteTarget, setDeleteTarget] = useState<WorkflowDefinition | null>(null);

  const wf = workflows.find(w => w.id === selectedWf);

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div><h1 className="text-2xl font-bold tracking-tight">Workflows</h1><p className="text-muted-foreground text-sm mt-1">Definición de procesos y flujos de trabajo</p></div>
        <Button className="gap-2" onClick={() => navigate("/workflows/nuevo")}><Plus className="h-4 w-4" /> Nuevo Workflow</Button>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <div className="space-y-2">
          {workflows.map(w => (
            <Card key={w.id} className={`cursor-pointer transition-all ${w.id === selectedWf ? "ring-2 ring-primary shadow-md" : "hover:shadow-sm"}`} onClick={() => setSelectedWf(w.id)}>
              <CardContent className="p-4">
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-3">
                    <div className="h-9 w-9 rounded-lg bg-primary/10 flex items-center justify-center"><GitBranch className="h-4 w-4 text-primary" /></div>
                    <div>
                      <p className="font-medium text-sm">{w.name}</p>
                      <p className="text-xs text-muted-foreground">v{w.version} · {w.productCategory}</p>
                    </div>
                  </div>
                  <div className="flex items-center gap-2">
                    <StatusBadge status={w.status} />
                    <div className="flex" onClick={ev => ev.stopPropagation()}>
                      <Button variant="ghost" size="icon" className="h-7 w-7" onClick={() => navigate(`/workflows/${w.id}/editar`)}><Pencil className="h-3 w-3" /></Button>
                      <Button variant="ghost" size="icon" className="h-7 w-7 text-destructive hover:text-destructive" onClick={() => setDeleteTarget(w)}><Trash2 className="h-3 w-3" /></Button>
                    </div>
                  </div>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>

        <div className="lg:col-span-2">
          {wf ? (
            <div className="space-y-4">
              <Card>
                <CardHeader className="pb-3">
                  <CardTitle className="text-base flex items-center gap-2">
                    Estados del Workflow: <Badge>{wf.name}</Badge>
                  </CardTitle>
                  <p className="text-xs text-muted-foreground">{wf.description}</p>
                </CardHeader>
                <CardContent>
                  <div className="flex flex-wrap gap-3">
                    {wf.states.map(state => (
                      <div key={state.id} className="flex items-center gap-2 px-4 py-3 rounded-xl border bg-card">
                        <div className="h-3 w-3 rounded-full" style={{ backgroundColor: state.color }} />
                        <div>
                          <p className="text-sm font-medium">{state.name}</p>
                          <div className="flex items-center gap-2">
                            <Badge variant="outline" className="text-[10px]">{state.type === 'initial' ? 'Inicio' : state.type === 'final' ? 'Final' : 'Intermedio'}</Badge>
                            {state.slaHours && <span className="flex items-center gap-1 text-[10px] text-muted-foreground"><Clock className="h-2.5 w-2.5" />{state.slaHours}h SLA</span>}
                          </div>
                        </div>
                      </div>
                    ))}
                  </div>
                </CardContent>
              </Card>

              <Card>
                <CardHeader className="pb-3"><CardTitle className="text-base">Transiciones</CardTitle></CardHeader>
                <CardContent>
                  <div className="space-y-2">
                    {wf.transitions.map(t => {
                      const from = wf.states.find(s => s.id === t.fromStateId);
                      const to = wf.states.find(s => s.id === t.toStateId);
                      return (
                        <div key={t.id} className="flex items-center gap-3 p-3 rounded-lg border text-sm">
                          <div className="flex items-center gap-2">
                            <div className="h-2.5 w-2.5 rounded-full" style={{ backgroundColor: from?.color }} />
                            <span className="font-medium">{from?.name}</span>
                          </div>
                          <ChevronRight className="h-4 w-4 text-muted-foreground" />
                          <div className="flex items-center gap-2">
                            <div className="h-2.5 w-2.5 rounded-full" style={{ backgroundColor: to?.color }} />
                            <span className="font-medium">{to?.name}</span>
                          </div>
                          <span className="text-muted-foreground ml-2">— {t.name}</span>
                          {t.requiredRole && <Badge variant="secondary" className="text-[10px] ml-auto">{t.requiredRole}</Badge>}
                        </div>
                      );
                    })}
                  </div>
                </CardContent>
              </Card>
            </div>
          ) : (
            <div className="flex items-center justify-center h-40 text-muted-foreground">Seleccione un workflow</div>
          )}
        </div>
      </div>

      <DeleteConfirmDialog open={!!deleteTarget} onOpenChange={o => !o && setDeleteTarget(null)} title="Eliminar Workflow"
        description={`¿Está seguro de eliminar "${deleteTarget?.name}"?`}
        onConfirm={() => { if (deleteTarget) { deleteWorkflow(deleteTarget.id); if (selectedWf === deleteTarget.id) setSelectedWf(workflows[0]?.id || ""); toast.success("Workflow eliminado"); setDeleteTarget(null); } }} />
    </div>
  );
}
