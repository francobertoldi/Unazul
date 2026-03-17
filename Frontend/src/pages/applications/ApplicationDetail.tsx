import { useParams, useNavigate } from "react-router-dom";
import { ArrowLeft, FileCheck, User, FileText, MessageSquare, Clock, Pencil, CheckCircle2, Circle, GitBranch, MapPin, Package, Mail, Smartphone } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Badge } from "@/components/ui/badge";
import { useApplicationStore, useWorkflowStore, useProductStore, useParameterStore } from "@/data/store";
import { ApplicationStatus, TraceEvent, WorkflowState } from "@/data/types";
import { useMemo, useState } from "react";
import { cn } from "@/lib/utils";
import SendMessageDialog from "@/components/applications/SendMessageDialog";

const statusLabels: Record<ApplicationStatus, string> = {
  draft: "Borrador", pending: "Pendiente", in_review: "En Revisión",
  approved: "Aprobada", rejected: "Rechazada", cancelled: "Cancelada", settled: "Liquidado",
};
const statusColors: Record<ApplicationStatus, string> = {
  draft: "bg-muted text-muted-foreground", pending: "bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-200",
  in_review: "bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200",
  approved: "bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200",
  rejected: "bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200",
  cancelled: "bg-muted text-muted-foreground",
  settled: "bg-purple-100 text-purple-800 dark:bg-purple-900 dark:text-purple-200",
};

export default function ApplicationDetail() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { applications } = useApplicationStore();
  const { workflows } = useWorkflowStore();
  const { products } = useProductStore();
  const { parameters } = useParameterStore();
  const app = applications.find(a => a.id === id);

  const [msgDialogOpen, setMsgDialogOpen] = useState(false);
  const [msgType, setMsgType] = useState<"email" | "sms" | "whatsapp">("email");

  const emailEnabled = parameters.find(p => p.key === 'notifications.email_enabled')?.value === 'true';
  const smsEnabled = parameters.find(p => p.key === 'sms.enabled')?.value === 'true';
  const whatsappEnabled = parameters.find(p => p.key === 'whatsapp.enabled')?.value === 'true';

  const openMsgDialog = (type: "email" | "sms" | "whatsapp") => {
    setMsgType(type);
    setMsgDialogOpen(true);
  };

  const productPlan = useMemo(() => {
    if (!app) return null;
    const product = products.find(p => p.id === app.productId);
    if (!product) return null;
    const plan = product.plans.find(pl => pl.id === app.planId);
    return { product, plan: plan || null };
  }, [app, products]);

  // Find the workflow for this application based on its current state
  const workflow = useMemo(() => {
    if (!app) return null;
    return workflows.find(wf =>
      wf.states.some(s => s.id === app.currentWorkflowStateId)
    ) || null;
  }, [app, workflows]);

  // Build the ordered path of states from the workflow
  const workflowPath = useMemo(() => {
    if (!workflow) return [];
    const { states, transitions } = workflow;
    
    // Find the initial state
    const initialState = states.find(s => s.type === 'initial');
    if (!initialState) return states;

    // Build adjacency (main happy path: follow non-rejection transitions)
    const visited = new Set<string>();
    const path: WorkflowState[] = [];
    
    const buildPath = (stateId: string) => {
      if (visited.has(stateId)) return;
      visited.add(stateId);
      const state = states.find(s => s.id === stateId);
      if (state) path.push(state);
      
      // Find outgoing transitions, prioritize non-final destinations first
      const outgoing = transitions
        .filter(t => t.fromStateId === stateId)
        .sort((a, b) => {
          const stateA = states.find(s => s.id === a.toStateId);
          const stateB = states.find(s => s.id === b.toStateId);
          // Non-final states first
          if (stateA?.type === 'final' && stateB?.type !== 'final') return 1;
          if (stateA?.type !== 'final' && stateB?.type === 'final') return -1;
          return 0;
        });
      
      for (const t of outgoing) {
        buildPath(t.toStateId);
      }
    };
    
    buildPath(initialState.id);
    return path;
  }, [workflow]);

  // Determine which states have been completed (have trace events)
  const completedStateIds = useMemo(() => {
    if (!app) return new Set<string>();
    return new Set(app.traceEvents.map(te => te.workflowStateId));
  }, [app]);

  // Map trace events by state id
  const traceByState = useMemo(() => {
    if (!app) return new Map<string, TraceEvent>();
    const map = new Map<string, TraceEvent>();
    app.traceEvents.forEach(te => {
      map.set(te.workflowStateId, te);
    });
    return map;
  }, [app]);

  if (!app) return <div className="flex items-center justify-center h-64"><p className="text-muted-foreground">Solicitud no encontrada</p></div>;

  const isFinalState = (stateId: string) => {
    const state = workflow?.states.find(s => s.id === stateId);
    return state?.type === 'final';
  };

  const isCurrentState = (stateId: string) => stateId === app.currentWorkflowStateId;

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-3">
        <Button variant="ghost" size="icon" onClick={() => navigate("/solicitudes")}><ArrowLeft className="h-4 w-4" /></Button>
        <div className="flex-1">
          <div className="flex items-center gap-3">
            <div className="h-10 w-10 rounded-xl bg-primary/10 flex items-center justify-center"><FileCheck className="h-5 w-5 text-primary" /></div>
            <div>
              <h1 className="text-xl font-bold tracking-tight">{app.code}</h1>
              <p className="text-sm font-medium">{app.applicant.firstName} {app.applicant.lastName} · {app.applicant.documentType} {app.applicant.documentNumber}</p>
              <p className="text-xs text-muted-foreground">{app.productName} — {app.planName}</p>
            </div>
            <span className={`text-xs px-2.5 py-1 rounded-full font-medium ${statusColors[app.status]}`}>{statusLabels[app.status]}</span>
            <Badge variant="outline">{app.currentWorkflowStateName}</Badge>
          </div>
        </div>
        <div className="flex items-center gap-2">
          {emailEnabled && (
            <Button variant="outline" size="sm" className="gap-1.5" onClick={() => openMsgDialog("email")}>
              <Mail className="h-4 w-4" /> Email
            </Button>
          )}
          {smsEnabled && (
            <Button variant="outline" size="sm" className="gap-1.5" onClick={() => openMsgDialog("sms")}>
              <Smartphone className="h-4 w-4" /> SMS
            </Button>
          )}
          {whatsappEnabled && (
            <Button variant="outline" size="sm" className="gap-1.5" onClick={() => openMsgDialog("whatsapp")}>
              <MessageSquare className="h-4 w-4" /> WhatsApp
            </Button>
          )}
          <Button variant="outline" className="gap-2" onClick={() => navigate(`/solicitudes/${id}/editar`)}><Pencil className="h-4 w-4" /> Editar</Button>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <div className="lg:col-span-2">
          <Tabs defaultValue="info" className="space-y-4">
            <TabsList>
              <TabsTrigger value="info">Solicitante</TabsTrigger>
              <TabsTrigger value="product" className="gap-1.5">
                <Package className="h-3.5 w-3.5" />
                Producto
              </TabsTrigger>
              <TabsTrigger value="address" className="gap-1.5">
                <MapPin className="h-3.5 w-3.5" />
                Domicilio
              </TabsTrigger>
              <TabsTrigger value="beneficiaries">Beneficiarios ({app.beneficiaries.length})</TabsTrigger>
              <TabsTrigger value="docs">Documentos ({app.documents.length})</TabsTrigger>
              <TabsTrigger value="observations">Observaciones ({app.observations.length})</TabsTrigger>
              <TabsTrigger value="traceability" className="gap-1.5">
                <GitBranch className="h-3.5 w-3.5" />
                Trazabilidad
              </TabsTrigger>
            </TabsList>

            <TabsContent value="info">
              <Card>
                <CardHeader className="pb-3"><CardTitle className="text-base">Datos del Solicitante</CardTitle></CardHeader>
                <CardContent>
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    {(() => {
                      const genderLabels: Record<string, string> = { male: 'Masculino', female: 'Femenino', other: 'Otro', not_specified: 'No especificado' };
                      const calcAge = (bd?: string) => { if (!bd) return '—'; const diff = Date.now() - new Date(bd).getTime(); return `${Math.floor(diff / 31557600000)} años`; };
                      return [
                        { label: "Nombre", value: `${app.applicant.firstName} ${app.applicant.lastName}` },
                        { label: "Documento", value: `${app.applicant.documentType} ${app.applicant.documentNumber}` },
                        { label: "Email", value: app.applicant.email },
                        { label: "Teléfono", value: app.applicant.phone },
                        { label: "Fecha de Nacimiento", value: app.applicant.birthDate ? new Date(app.applicant.birthDate).toLocaleDateString("es-AR") : '—' },
                        { label: "Edad", value: calcAge(app.applicant.birthDate) },
                        { label: "Género", value: app.applicant.gender ? genderLabels[app.applicant.gender] || app.applicant.gender : '—' },
                        { label: "Ocupación", value: app.applicant.occupation || '—' },
                      ];
                    })().map(item => (
                      <div key={item.label}><p className="text-xs text-muted-foreground">{item.label}</p><p className="text-sm font-medium">{item.value}</p></div>
                    ))}
                  </div>
                </CardContent>
              </Card>
            </TabsContent>

            <TabsContent value="product">
              <Card>
                <CardHeader className="pb-3">
                  <CardTitle className="text-base flex items-center gap-2">
                    <Package className="h-4 w-4 text-primary" />
                    Datos del Producto
                  </CardTitle>
                </CardHeader>
                <CardContent>
                  {!productPlan ? (
                    <p className="text-sm text-muted-foreground">No se encontró el producto asociado.</p>
                  ) : (
                    <div className="space-y-5">
                      {/* Datos generales del producto */}
                      <div>
                        <h4 className="text-xs font-semibold text-muted-foreground uppercase tracking-wider mb-3">Información General</h4>
                        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                          {[
                            { label: "Producto", value: productPlan.product.name },
                            { label: "Código", value: productPlan.product.code },
                            { label: "Familia", value: productPlan.product.familyName },
                            { label: "Versión", value: `v${productPlan.product.version}` },
                            { label: "Estado", value: productPlan.product.status },
                            { label: "Entidad", value: productPlan.product.entityName },
                          ].map(item => (
                            <div key={item.label}><p className="text-xs text-muted-foreground">{item.label}</p><p className="text-sm font-medium">{item.value}</p></div>
                          ))}
                        </div>
                      </div>

                      {/* Datos del plan/subproducto */}
                      {productPlan.plan && (
                        <>
                          <div className="border-t pt-4">
                            <h4 className="text-xs font-semibold text-muted-foreground uppercase tracking-wider mb-3">Plan: {productPlan.plan.name}</h4>
                            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                              {[
                                { label: "Descripción", value: productPlan.plan.description || "—" },
                                { label: "Precio", value: `$${productPlan.plan.price.toLocaleString("es-AR")}` },
                                { label: "Moneda", value: productPlan.plan.currency },
                                ...(productPlan.plan.installments ? [{ label: "Cuotas", value: `${productPlan.plan.installments}` }] : []),
                                ...(productPlan.plan.otherCosts ? [{ label: "Otros Costos", value: `$${productPlan.plan.otherCosts.toLocaleString("es-AR")}` }] : []),
                              ].map(item => (
                                <div key={item.label}><p className="text-xs text-muted-foreground">{item.label}</p><p className="text-sm font-medium">{item.value}</p></div>
                              ))}
                            </div>
                          </div>

                          {/* Atributos específicos por familia */}
                          {productPlan.plan.loanAttributes && (
                            <div className="border-t pt-4">
                              <h4 className="text-xs font-semibold text-muted-foreground uppercase tracking-wider mb-3">Atributos de Préstamo</h4>
                              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                {[
                                  { label: "Sistema de Amortización", value: { french: "Francés", german: "Alemán", american: "Americano", bullet: "Bullet" }[productPlan.plan.loanAttributes.amortizationType] },
                                  { label: "TEA", value: `${productPlan.plan.loanAttributes.annualEffectiveRate}%` },
                                  { label: "CFT", value: `${productPlan.plan.loanAttributes.cftRate}%` },
                                  { label: "Gastos Administrativos", value: `$${productPlan.plan.loanAttributes.adminFees.toLocaleString("es-AR")}` },
                                ].map(item => (
                                  <div key={item.label}><p className="text-xs text-muted-foreground">{item.label}</p><p className="text-sm font-medium">{item.value}</p></div>
                                ))}
                              </div>
                            </div>
                          )}

                          {productPlan.plan.insuranceAttributes && (
                            <div className="border-t pt-4">
                              <h4 className="text-xs font-semibold text-muted-foreground uppercase tracking-wider mb-3">Atributos de Seguro</h4>
                              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                {[
                                  { label: "Prima", value: `$${productPlan.plan.insuranceAttributes.premium.toLocaleString("es-AR")}` },
                                  { label: "Suma Asegurada", value: `$${productPlan.plan.insuranceAttributes.sumInsured.toLocaleString("es-AR")}` },
                                  { label: "Período de Gracia", value: `${productPlan.plan.insuranceAttributes.gracePeriodDays} días` },
                                  { label: "Tipo de Cobertura", value: { individual: "Individual", group: "Grupal", collective: "Colectivo" }[productPlan.plan.insuranceAttributes.coverageType] },
                                ].map(item => (
                                  <div key={item.label}><p className="text-xs text-muted-foreground">{item.label}</p><p className="text-sm font-medium">{item.value}</p></div>
                                ))}
                              </div>
                            </div>
                          )}

                          {productPlan.plan.cardAttributes && (
                            <div className="border-t pt-4">
                              <h4 className="text-xs font-semibold text-muted-foreground uppercase tracking-wider mb-3">Atributos de Tarjeta</h4>
                              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                {[
                                  { label: "Límite de Crédito", value: `$${productPlan.plan.cardAttributes.creditLimit.toLocaleString("es-AR")}` },
                                  { label: "Cuota Anual", value: `$${productPlan.plan.cardAttributes.annualFee.toLocaleString("es-AR")}` },
                                  { label: "Tasa de Interés", value: `${productPlan.plan.cardAttributes.interestRate}%` },
                                  { label: "Red", value: productPlan.plan.cardAttributes.network.toUpperCase() },
                                  { label: "Período de Gracia", value: `${productPlan.plan.cardAttributes.gracePeriodDays} días` },
                                  ...(productPlan.plan.cardAttributes.level ? [{ label: "Nivel", value: productPlan.plan.cardAttributes.level }] : []),
                                ].map(item => (
                                  <div key={item.label}><p className="text-xs text-muted-foreground">{item.label}</p><p className="text-sm font-medium">{item.value}</p></div>
                                ))}
                              </div>
                            </div>
                          )}

                          {productPlan.plan.accountAttributes && (
                            <div className="border-t pt-4">
                              <h4 className="text-xs font-semibold text-muted-foreground uppercase tracking-wider mb-3">Atributos de Cuenta</h4>
                              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                {[
                                  { label: "Cuota de Mantenimiento", value: `$${productPlan.plan.accountAttributes.maintenanceFee.toLocaleString("es-AR")}` },
                                  { label: "Balance Mínimo", value: `$${productPlan.plan.accountAttributes.minimumBalance.toLocaleString("es-AR")}` },
                                  { label: "Tasa de Interés", value: `${productPlan.plan.accountAttributes.interestRate}%` },
                                  { label: "Tipo de Cuenta", value: { savings: "Ahorro", checking: "Corriente", money_market: "Money Market" }[productPlan.plan.accountAttributes.accountType] },
                                ].map(item => (
                                  <div key={item.label}><p className="text-xs text-muted-foreground">{item.label}</p><p className="text-sm font-medium">{item.value}</p></div>
                                ))}
                              </div>
                            </div>
                          )}

                          {productPlan.plan.investmentAttributes && (
                            <div className="border-t pt-4">
                              <h4 className="text-xs font-semibold text-muted-foreground uppercase tracking-wider mb-3">Atributos de Inversión</h4>
                              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                {[
                                  { label: "Monto Mínimo", value: `$${productPlan.plan.investmentAttributes.minimumAmount.toLocaleString("es-AR")}` },
                                  { label: "Rendimiento Esperado", value: `${productPlan.plan.investmentAttributes.expectedReturn}%` },
                                  { label: "Plazo", value: `${productPlan.plan.investmentAttributes.termDays} días` },
                                  { label: "Nivel de Riesgo", value: { low: "Bajo", medium: "Medio", high: "Alto" }[productPlan.plan.investmentAttributes.riskLevel] },
                                  { label: "Tipo de Instrumento", value: { fixed_term: "Plazo Fijo", bond: "Bono", mutual_fund: "Fondo Común", stock: "Acción" }[productPlan.plan.investmentAttributes.instrumentType] },
                                ].map(item => (
                                  <div key={item.label}><p className="text-xs text-muted-foreground">{item.label}</p><p className="text-sm font-medium">{item.value}</p></div>
                                ))}
                              </div>
                            </div>
                          )}

                          {/* Coberturas */}
                          {productPlan.plan.coverages.length > 0 && (
                            <div className="border-t pt-4">
                              <h4 className="text-xs font-semibold text-muted-foreground uppercase tracking-wider mb-3">Coberturas ({productPlan.plan.coverages.length})</h4>
                              <div className="space-y-2">
                                {productPlan.plan.coverages.map(cov => (
                                  <div key={cov.id} className="flex items-center justify-between p-3 rounded-lg border">
                                    <div>
                                      <p className="text-sm font-medium">{cov.name}</p>
                                      <p className="text-xs text-muted-foreground">{cov.description}</p>
                                    </div>
                                    <div className="text-right text-sm">
                                      {cov.sumInsured != null && <p>SA: ${cov.sumInsured.toLocaleString("es-AR")}</p>}
                                      {cov.premium != null && <p className="text-xs text-muted-foreground">Prima: ${cov.premium.toLocaleString("es-AR")}</p>}
                                    </div>
                                  </div>
                                ))}
                              </div>
                            </div>
                          )}
                        </>
                      )}
                    </div>
                  )}
                </CardContent>
              </Card>
            </TabsContent>

            <TabsContent value="beneficiaries">
              <Card>
                <CardContent className="pt-6">
                  {app.beneficiaries.length === 0 ? <p className="text-sm text-muted-foreground">Sin beneficiarios</p> : (
                    <div className="space-y-3">
                      {app.beneficiaries.map((b, i) => (
                        <div key={i} className="flex items-center justify-between p-3 rounded-lg border">
                          <div><p className="text-sm font-medium">{b.firstName} {b.lastName}</p><p className="text-xs text-muted-foreground">{b.relationship}</p></div>
                          <Badge variant="secondary">{b.percentage}%</Badge>
                        </div>
                      ))}
                    </div>
                  )}
                </CardContent>
              </Card>
            </TabsContent>

            <TabsContent value="docs">
              <Card>
                <CardContent className="pt-6">
                  {app.documents.length === 0 ? <p className="text-sm text-muted-foreground">Sin documentos</p> : (
                    <div className="space-y-3">
                      {app.documents.map(doc => (
                        <div key={doc.id} className="flex items-center gap-3 p-3 rounded-lg border">
                          <FileText className="h-4 w-4 text-muted-foreground shrink-0" />
                          <div className="flex-1">
                            <p className="text-sm font-medium">{doc.name}</p>
                            <p className="text-xs text-muted-foreground">{doc.type} · {new Date(doc.uploadedAt).toLocaleString("es-AR", { dateStyle: "short", timeStyle: "short" })}</p>
                          </div>
                          <Badge variant={doc.status === 'approved' ? 'default' : doc.status === 'rejected' ? 'destructive' : 'secondary'} className="text-xs">
                            {doc.status === 'approved' ? 'Aprobado' : doc.status === 'rejected' ? 'Rechazado' : 'Pendiente'}
                          </Badge>
                        </div>
                      ))}
                    </div>
                  )}
                </CardContent>
              </Card>
            </TabsContent>

            <TabsContent value="observations">
              <Card>
                <CardContent className="pt-6">
                  {app.observations.length === 0 ? <p className="text-sm text-muted-foreground">Sin observaciones</p> : (
                    <div className="space-y-3">
                      {app.observations.map(obs => (
                        <div key={obs.id} className="flex items-start gap-3">
                          <div className="h-8 w-8 rounded-full bg-primary/10 flex items-center justify-center shrink-0 mt-0.5"><MessageSquare className="h-3.5 w-3.5 text-primary" /></div>
                          <div>
                            <div className="flex items-center gap-2"><p className="text-sm font-medium">{obs.userName}</p><span className="text-xs text-muted-foreground">{new Date(obs.timestamp).toLocaleString("es-AR", { dateStyle: "short", timeStyle: "short" })}</span></div>
                            <p className="text-sm text-muted-foreground">{obs.text}</p>
                          </div>
                        </div>
                      ))}
                    </div>
                  )}
                </CardContent>
              </Card>
            </TabsContent>

            <TabsContent value="address">
              <Card>
                <CardHeader className="pb-3">
                  <CardTitle className="text-base flex items-center gap-2">
                    <MapPin className="h-4 w-4 text-primary" />
                    Domicilio del Solicitante
                  </CardTitle>
                </CardHeader>
                <CardContent>
                  {!app.address ? (
                    <p className="text-sm text-muted-foreground">No se ha registrado un domicilio para esta solicitud.</p>
                  ) : (
                    <div className="space-y-5">
                      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <div><p className="text-xs text-muted-foreground">Calle</p><p className="text-sm font-medium">{app.address.street}</p></div>
                        <div className="grid grid-cols-3 gap-3">
                          <div><p className="text-xs text-muted-foreground">Número</p><p className="text-sm font-medium">{app.address.number}</p></div>
                          <div><p className="text-xs text-muted-foreground">Piso</p><p className="text-sm font-medium">{app.address.floor || '—'}</p></div>
                          <div><p className="text-xs text-muted-foreground">Dpto</p><p className="text-sm font-medium">{app.address.apartment || '—'}</p></div>
                        </div>
                      </div>
                      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                        <div><p className="text-xs text-muted-foreground">Provincia</p><p className="text-sm font-medium">{app.address.province}</p></div>
                        <div><p className="text-xs text-muted-foreground">Ciudad</p><p className="text-sm font-medium">{app.address.city}</p></div>
                        <div><p className="text-xs text-muted-foreground">Código Postal</p><p className="text-sm font-medium">{app.address.postalCode}</p></div>
                      </div>
                      
                      {/* Google Maps */}
                      <div className="rounded-lg overflow-hidden border" style={{ height: 300 }}>
                        <iframe
                          width="100%"
                          height="100%"
                          style={{ border: 0 }}
                          loading="lazy"
                          referrerPolicy="no-referrer-when-downgrade"
                          src={`https://www.google.com/maps?q=${encodeURIComponent(
                            `${app.address.street} ${app.address.number}, ${app.address.city}, ${app.address.province}, Argentina`
                          )}&output=embed`}
                          allowFullScreen
                        />
                      </div>
                    </div>
                  )}
                </CardContent>
              </Card>
            </TabsContent>

            <TabsContent value="traceability">
              <Card>
                <CardHeader className="pb-3">
                  <CardTitle className="text-base flex items-center gap-2">
                    <GitBranch className="h-4 w-4 text-primary" />
                    Timeline del Proceso
                  </CardTitle>
                </CardHeader>
                <CardContent>
                  {workflowPath.length === 0 ? (
                    <p className="text-sm text-muted-foreground">No se encontró un workflow asociado a esta solicitud.</p>
                  ) : (
                    <div className="relative">
                      {workflowPath.map((state, idx) => {
                        const isCompleted = completedStateIds.has(state.id);
                        const isCurrent = isCurrentState(state.id);
                        const isFinal = isFinalState(state.id);
                        const isPending = !isCompleted;
                        const trace = traceByState.get(state.id);
                        const isLast = idx === workflowPath.length - 1;

                        // Determine node color
                        const getNodeColor = () => {
                          if (isCompleted && isFinal && app.status === 'rejected') return 'bg-destructive text-destructive-foreground';
                          if (isCompleted && isFinal && app.status === 'approved') return 'bg-green-500 text-white';
                          if (isCurrent && !isFinal) return 'bg-primary text-primary-foreground ring-4 ring-primary/20';
                          if (isCompleted) return 'bg-primary text-primary-foreground';
                          return 'bg-muted text-muted-foreground';
                        };

                        const getLineColor = () => {
                          if (isCompleted && !isLast) {
                            const nextState = workflowPath[idx + 1];
                            if (nextState && completedStateIds.has(nextState.id)) return 'bg-primary';
                          }
                          return 'bg-border';
                        };

                        return (
                          <div key={state.id} className="flex gap-4">
                            {/* Timeline column */}
                            <div className="flex flex-col items-center">
                              <div className={cn(
                                "h-10 w-10 rounded-full flex items-center justify-center shrink-0 transition-all",
                                getNodeColor()
                              )}>
                                {isCompleted ? (
                                  <CheckCircle2 className="h-5 w-5" />
                                ) : isCurrent ? (
                                  <Clock className="h-5 w-5" />
                                ) : (
                                  <Circle className="h-5 w-5" />
                                )}
                              </div>
                              {!isLast && (
                                <div className={cn("w-0.5 flex-1 min-h-[32px]", getLineColor())} />
                              )}
                            </div>
                            
                            {/* Content column */}
                            <div className={cn("pb-6 flex-1 min-w-0", isLast && "pb-0")}>
                              <div className={cn(
                                "rounded-lg border p-3 transition-all",
                                isCompleted ? "bg-card border-border" : "bg-muted/30 border-dashed border-muted-foreground/20"
                              )}>
                                <div className="flex items-center gap-2 flex-wrap">
                                  <h4 className={cn(
                                    "text-sm font-semibold",
                                    isPending && "text-muted-foreground"
                                  )}>
                                    {state.name}
                                  </h4>
                                  {isCurrent && !isFinal && (
                                    <Badge variant="default" className="text-[10px] px-1.5 py-0 animate-pulse">
                                      En curso
                                    </Badge>
                                  )}
                                  {isFinal && isCompleted && (
                                    <Badge 
                                      variant={app.status === 'approved' ? 'default' : 'destructive'} 
                                      className="text-[10px] px-1.5 py-0"
                                    >
                                      {app.status === 'approved' ? 'Finalizado' : app.status === 'rejected' ? 'Rechazada' : 'Cancelada'}
                                    </Badge>
                                  )}
                                  {state.slaHours && !isFinal && (
                                    <span className="text-[10px] text-muted-foreground">SLA: {state.slaHours}h</span>
                                  )}
                                </div>

                                {trace ? (
                                  <div className="mt-2 space-y-1">
                                    <div className="flex items-center gap-2 text-xs">
                                      <Clock className="h-3 w-3 text-muted-foreground shrink-0" />
                                      <span className="font-medium">
                                        {new Date(trace.timestamp).toLocaleString("es-AR", { 
                                          day: '2-digit', month: '2-digit', year: 'numeric',
                                          hour: '2-digit', minute: '2-digit' 
                                        })}
                                      </span>
                                      <span className="text-muted-foreground">·</span>
                                      <span className="text-muted-foreground">{trace.userName}</span>
                                    </div>
                                    <p className="text-xs font-medium text-primary">{trace.action}</p>
                                    {trace.detail && (
                                      <p className="text-xs text-muted-foreground">{trace.detail}</p>
                                    )}
                                  </div>
                                ) : (
                                  <p className="mt-1.5 text-xs text-muted-foreground italic">Pendiente</p>
                                )}
                              </div>
                            </div>
                          </div>
                        );
                      })}
                    </div>
                  )}
                </CardContent>
              </Card>
            </TabsContent>
          </Tabs>
        </div>

        <div className="space-y-4">
          <Card>
            <CardHeader className="pb-3"><CardTitle className="text-sm">Información General</CardTitle></CardHeader>
            <CardContent className="space-y-3 text-sm">
              <div><p className="text-xs text-muted-foreground">Entidad</p><p className="font-medium">{app.entityName}</p></div>
              <div><p className="text-xs text-muted-foreground">Producto</p><p className="font-medium">{app.productName}</p></div>
              <div><p className="text-xs text-muted-foreground">Plan</p><p className="font-medium">{app.planName}</p></div>
              {app.assignedUserName && <div><p className="text-xs text-muted-foreground">Asignado a</p><p className="font-medium">{app.assignedUserName}</p></div>}
              <div><p className="text-xs text-muted-foreground">Creado</p><p className="font-medium">{new Date(app.createdAt).toLocaleString("es-AR")}</p></div>
              <div><p className="text-xs text-muted-foreground">Actualizado</p><p className="font-medium">{new Date(app.updatedAt).toLocaleString("es-AR")}</p></div>
            </CardContent>
          </Card>
        </div>
      </div>

      {app && (
        <SendMessageDialog
          open={msgDialogOpen}
          onOpenChange={setMsgDialogOpen}
          type={msgType}
          application={app}
          productName={app.productName}
          planName={app.planName}
          entityName={app.entityName}
        />
      )}
    </div>
  );
}
