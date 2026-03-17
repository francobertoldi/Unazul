import { useState, useRef } from 'react';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from '@/components/ui/dialog';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { Button } from '@/components/ui/button';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Switch } from '@/components/ui/switch';
import { Separator } from '@/components/ui/separator';
import { ScrollArea } from '@/components/ui/scroll-area';
import { Collapsible, CollapsibleContent, CollapsibleTrigger } from '@/components/ui/collapsible';
import { Badge } from '@/components/ui/badge';
import { Plus, Trash2, ChevronRight } from 'lucide-react';
import type { FlowNodeData, FlowNodeType } from '@/data/workflowNodes';
import { nodeTypeConfig } from '@/data/workflowNodes';
import { mockServices } from '@/data/services';
import { cn } from '@/lib/utils';

// ── Attribute catalog for decision conditions ──
interface AttrDef { name: string; type: string; description: string }
interface AttrGroup { object: string; icon: string; attributes: AttrDef[] }

const attributeCatalog: AttrGroup[] = [
  { object: 'Persona', icon: '👤', attributes: [
    { name: 'firstName', type: 'string', description: 'Nombre' },
    { name: 'lastName', type: 'string', description: 'Apellido' },
    { name: 'documentType', type: 'enum', description: 'Tipo documento' },
    { name: 'documentNumber', type: 'string', description: 'Nro. documento' },
    { name: 'email', type: 'string', description: 'Email' },
    { name: 'phone', type: 'string', description: 'Teléfono' },
    { name: 'birthDate', type: 'date', description: 'Fecha de nacimiento' },
    { name: 'age', type: 'number', description: 'Edad (calculada)' },
    { name: 'gender', type: 'enum', description: 'Género (sexo)' },
    { name: 'occupation', type: 'string', description: 'Ocupación' },
  ]},
  { object: 'Domicilio', icon: '📍', attributes: [
    { name: 'street', type: 'string', description: 'Calle' },
    { name: 'city', type: 'string', description: 'Ciudad' },
    { name: 'province', type: 'string', description: 'Provincia' },
    { name: 'postalCode', type: 'string', description: 'Código postal' },
  ]},
  { object: 'Solicitud', icon: '📋', attributes: [
    { name: 'code', type: 'string', description: 'Código' },
    { name: 'status', type: 'enum', description: 'Estado' },
    { name: 'entityName', type: 'string', description: 'Entidad' },
    { name: 'createdAt', type: 'date', description: 'Fecha creación' },
  ]},
  { object: 'Producto', icon: '📦', attributes: [
    { name: 'name', type: 'string', description: 'Nombre' },
    { name: 'code', type: 'string', description: 'Código' },
    { name: 'familyName', type: 'string', description: 'Familia' },
    { name: 'version', type: 'number', description: 'Versión' },
    { name: 'status', type: 'enum', description: 'Estado' },
  ]},
  { object: 'SubProducto', icon: '📄', attributes: [
    { name: 'price', type: 'number', description: 'Precio' },
    { name: 'currency', type: 'string', description: 'Moneda' },
    { name: 'installments', type: 'number', description: 'Cuotas' },
    { name: 'otherCosts', type: 'number', description: 'Otros costos' },
  ]},
  { object: 'Préstamo', icon: '🏦', attributes: [
    { name: 'amortizationType', type: 'enum', description: 'Amortización' },
    { name: 'annualEffectiveRate', type: 'number', description: 'TEA (%)' },
    { name: 'cftRate', type: 'number', description: 'CFT (%)' },
    { name: 'adminFees', type: 'number', description: 'Gastos admin.' },
  ]},
  { object: 'Seguro', icon: '🛡️', attributes: [
    { name: 'premium', type: 'number', description: 'Prima' },
    { name: 'sumInsured', type: 'number', description: 'Suma asegurada' },
    { name: 'gracePeriodDays', type: 'number', description: 'Días de gracia' },
    { name: 'coverageType', type: 'enum', description: 'Tipo cobertura' },
  ]},
  { object: 'Tarjeta', icon: '💳', attributes: [
    { name: 'creditLimit', type: 'number', description: 'Límite crédito' },
    { name: 'annualFee', type: 'number', description: 'Cuota anual' },
    { name: 'interestRate', type: 'number', description: 'Tasa interés (%)' },
    { name: 'network', type: 'enum', description: 'Red' },
  ]},
  { object: 'Cuenta', icon: '🏧', attributes: [
    { name: 'maintenanceFee', type: 'number', description: 'Cuota mant.' },
    { name: 'minimumBalance', type: 'number', description: 'Balance mín.' },
    { name: 'interestRate', type: 'number', description: 'Tasa interés (%)' },
    { name: 'accountType', type: 'enum', description: 'Tipo cuenta' },
  ]},
  { object: 'Inversión', icon: '📈', attributes: [
    { name: 'minimumAmount', type: 'number', description: 'Monto mínimo' },
    { name: 'expectedReturn', type: 'number', description: 'Rendimiento (%)' },
    { name: 'termDays', type: 'number', description: 'Plazo (días)' },
    { name: 'riskLevel', type: 'enum', description: 'Nivel riesgo' },
  ]},
];

const attrTypeColors: Record<string, string> = {
  string: 'bg-blue-100 text-blue-700 dark:bg-blue-900/40 dark:text-blue-300',
  number: 'bg-green-100 text-green-700 dark:bg-green-900/40 dark:text-green-300',
  date: 'bg-amber-100 text-amber-700 dark:bg-amber-900/40 dark:text-amber-300',
  enum: 'bg-pink-100 text-pink-700 dark:bg-pink-900/40 dark:text-pink-300',
};

interface Props {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  data: FlowNodeData;
  onSave: (data: FlowNodeData) => void;
}

export default function NodeConfigDialog({ open, onOpenChange, data, onSave }: Props) {
  const [form, setForm] = useState<FlowNodeData>(data);
  const conditionRef = useRef<HTMLTextAreaElement>(null);

  const update = (patch: Partial<FlowNodeData>) => setForm(prev => ({ ...prev, ...patch }));

  const insertAttribute = (ref: string) => {
    const textarea = conditionRef.current;
    if (!textarea) {
      update({ condition: (form.condition || '') + ref });
      return;
    }
    const start = textarea.selectionStart;
    const end = textarea.selectionEnd;
    const current = form.condition || '';
    const newVal = current.slice(0, start) + ref + current.slice(end);
    update({ condition: newVal });
    setTimeout(() => {
      textarea.focus();
      textarea.setSelectionRange(start + ref.length, start + ref.length);
    }, 0);
  };

  const handleSave = () => {
    onSave(form);
    onOpenChange(false);
  };

  const addField = () => {
    const fields = [...(form.formFields || []), { name: '', type: 'text', required: false }];
    update({ formFields: fields });
  };

  const updateField = (idx: number, patch: Partial<{ name: string; type: string; required: boolean }>) => {
    const fields = [...(form.formFields || [])];
    fields[idx] = { ...fields[idx], ...patch };
    update({ formFields: fields });
  };

  const removeField = (idx: number) => {
    update({ formFields: (form.formFields || []).filter((_, i) => i !== idx) });
  };

  const config = nodeTypeConfig[form.nodeType];

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className={cn("max-h-[85vh] overflow-y-auto", form.nodeType === 'decision' ? "max-w-3xl" : "max-w-lg")}>
        <DialogHeader>
          <DialogTitle>Configurar: {config.label}</DialogTitle>
        </DialogHeader>

        <div className="space-y-4 mt-2">
          {/* Common fields */}
          <div className="space-y-2">
            <Label>Nombre del paso</Label>
            <Input value={form.label} onChange={e => update({ label: e.target.value })} />
          </div>
          <div className="space-y-2">
            <Label>Descripción</Label>
            <Textarea value={form.description || ''} onChange={e => update({ description: e.target.value })} rows={2} />
          </div>

          <Separator />

          {/* Service Call */}
          {form.nodeType === 'service_call' && (
            <div className="space-y-4">
              <div className="space-y-2">
                <Label>Servicio</Label>
                <Select value={form.serviceId || ''} onValueChange={v => {
                  const svc = mockServices.find(s => s.id === v);
                  update({ serviceId: v, serviceName: svc?.name || '' });
                }}>
                  <SelectTrigger><SelectValue placeholder="Seleccionar servicio" /></SelectTrigger>
                  <SelectContent>
                    {mockServices.filter(s => s.status === 'active').map(s => (
                      <SelectItem key={s.id} value={s.id}>{s.name}</SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
              <div className="grid grid-cols-2 gap-3">
                <div className="space-y-2">
                  <Label>Método</Label>
                  <Select value={form.method || 'GET'} onValueChange={v => update({ method: v as any })}>
                    <SelectTrigger><SelectValue /></SelectTrigger>
                    <SelectContent>
                      {['GET', 'POST', 'PUT', 'DELETE'].map(m => <SelectItem key={m} value={m}>{m}</SelectItem>)}
                    </SelectContent>
                  </Select>
                </div>
                <div className="space-y-2">
                  <Label>Endpoint</Label>
                  <Input value={form.endpoint || ''} onChange={e => update({ endpoint: e.target.value })} placeholder="/evaluate" />
                </div>
              </div>
            </div>
          )}

          {/* Decision */}
          {form.nodeType === 'decision' && (
            <div className="space-y-4">
              <div className="grid grid-cols-[1fr_240px] gap-4">
                {/* Left: condition + labels */}
                <div className="space-y-4">
                  <div className="space-y-2">
                    <Label>Condición</Label>
                    <Textarea
                      ref={conditionRef}
                      value={form.condition || ''}
                      onChange={e => update({ condition: e.target.value })}
                      placeholder="ej: {{Préstamo.annualEffectiveRate}} >= 30"
                      rows={4}
                      className="font-mono text-xs"
                    />
                    <p className="text-[10px] text-muted-foreground">Hacé click en un atributo del panel derecho para insertarlo en la condición</p>
                  </div>
                  <div className="grid grid-cols-2 gap-3">
                    <div className="space-y-2">
                      <Label>Etiqueta Verdadero</Label>
                      <Input value={form.trueLabel || 'Sí'} onChange={e => update({ trueLabel: e.target.value })} />
                    </div>
                    <div className="space-y-2">
                      <Label>Etiqueta Falso</Label>
                      <Input value={form.falseLabel || 'No'} onChange={e => update({ falseLabel: e.target.value })} />
                    </div>
                  </div>
                </div>

                {/* Right: attribute picker */}
                <div className="border rounded-lg overflow-hidden flex flex-col">
                  <div className="px-2.5 py-2 border-b bg-muted/30">
                    <p className="text-[10px] font-semibold text-muted-foreground uppercase tracking-wider">Atributos disponibles</p>
                  </div>
                  <ScrollArea className="flex-1 max-h-[280px]">
                    <div className="p-1.5 space-y-0.5">
                      {attributeCatalog.map(group => (
                        <Collapsible key={group.object} defaultOpen={false}>
                          <CollapsibleTrigger className="flex items-center gap-1.5 w-full px-1.5 py-1 rounded hover:bg-muted/50 transition-colors text-left group">
                            <ChevronRight className="h-2.5 w-2.5 text-muted-foreground transition-transform group-data-[state=open]:rotate-90" />
                            <span className="text-xs">{group.icon}</span>
                            <span className="text-[11px] font-medium text-foreground truncate flex-1">{group.object}</span>
                            <Badge variant="secondary" className="text-[8px] px-1 py-0 h-3.5">{group.attributes.length}</Badge>
                          </CollapsibleTrigger>
                          <CollapsibleContent>
                            <div className="ml-2.5 pl-1.5 border-l border-border/50 space-y-0.5 py-0.5">
                              {group.attributes.map(attr => (
                                <button
                                  key={`${group.object}.${attr.name}`}
                                  type="button"
                                  onClick={() => insertAttribute(`{{${group.object}.${attr.name}}}`)}
                                  className="flex items-center gap-1 w-full px-1.5 py-1 rounded text-left hover:bg-primary/10 transition-colors group/attr"
                                  title={`Insertar {{${group.object}.${attr.name}}}`}
                                >
                                  <div className="flex-1 min-w-0">
                                    <p className="text-[10px] font-medium text-foreground truncate">{attr.description}</p>
                                    <p className="text-[8px] text-muted-foreground truncate">{group.object}.{attr.name}</p>
                                  </div>
                                  <span className={cn("text-[8px] px-1 py-0.5 rounded font-medium shrink-0", attrTypeColors[attr.type] || 'bg-muted text-muted-foreground')}>
                                    {attr.type}
                                  </span>
                                </button>
                              ))}
                            </div>
                          </CollapsibleContent>
                        </Collapsible>
                      ))}
                    </div>
                  </ScrollArea>
                </div>
              </div>
            </div>
          )}

          {/* Send Message */}
          {form.nodeType === 'send_message' && (
            <div className="space-y-4">
              <div className="grid grid-cols-2 gap-3">
                <div className="space-y-2">
                  <Label>Canal</Label>
                  <Select value={form.channel || 'email'} onValueChange={v => update({ channel: v as any })}>
                    <SelectTrigger><SelectValue /></SelectTrigger>
                    <SelectContent>
                      <SelectItem value="email">Email</SelectItem>
                      <SelectItem value="sms">SMS</SelectItem>
                      <SelectItem value="push">Push</SelectItem>
                      <SelectItem value="webhook">Webhook</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
                <div className="space-y-2">
                  <Label>Destinatario</Label>
                  <Input value={form.recipient || ''} onChange={e => update({ recipient: e.target.value })} placeholder="{{applicant.email}}" />
                </div>
              </div>
              <div className="space-y-2">
                <Label>Template / Mensaje</Label>
                <Textarea value={form.template || ''} onChange={e => update({ template: e.target.value })} rows={3} placeholder="Contenido del mensaje..." />
              </div>
            </div>
          )}

          {/* Data Capture */}
          {form.nodeType === 'data_capture' && (
            <div className="space-y-4">
              <div className="space-y-2">
                <Label>Título de pantalla</Label>
                <Input value={form.screenTitle || ''} onChange={e => update({ screenTitle: e.target.value })} placeholder="Formulario de datos adicionales" />
              </div>
              <div className="flex items-center justify-between">
                <Label>Campos del formulario</Label>
                <Button variant="outline" size="sm" className="text-xs gap-1" onClick={addField}>
                  <Plus className="h-3 w-3" /> Campo
                </Button>
              </div>
              {(form.formFields || []).map((f, idx) => (
                <div key={idx} className="flex items-center gap-2">
                  <Input value={f.name} onChange={e => updateField(idx, { name: e.target.value })} placeholder="Nombre" className="flex-1" />
                  <Select value={f.type} onValueChange={v => updateField(idx, { type: v })}>
                    <SelectTrigger className="w-28"><SelectValue /></SelectTrigger>
                    <SelectContent>
                      <SelectItem value="text">Texto</SelectItem>
                      <SelectItem value="number">Número</SelectItem>
                      <SelectItem value="date">Fecha</SelectItem>
                      <SelectItem value="select">Select</SelectItem>
                      <SelectItem value="file">Archivo</SelectItem>
                    </SelectContent>
                  </Select>
                  <div className="flex items-center gap-1">
                    <Switch checked={f.required} onCheckedChange={c => updateField(idx, { required: c })} />
                    <span className="text-[10px] text-muted-foreground">Req</span>
                  </div>
                  <Button variant="ghost" size="icon" className="h-7 w-7 text-destructive" onClick={() => removeField(idx)}>
                    <Trash2 className="h-3 w-3" />
                  </Button>
                </div>
              ))}
              {(!form.formFields || form.formFields.length === 0) && (
                <p className="text-xs text-muted-foreground text-center py-2">Sin campos configurados</p>
              )}
            </div>
          )}

          {/* Timer */}
          {form.nodeType === 'timer' && (
            <div className="space-y-4">
              <div className="space-y-2">
                <Label>Tipo de temporizador</Label>
                <Select value={form.timerType || 'delay'} onValueChange={v => update({ timerType: v as any })}>
                  <SelectTrigger><SelectValue /></SelectTrigger>
                  <SelectContent>
                    <SelectItem value="delay">Espera (delay)</SelectItem>
                    <SelectItem value="schedule">Programado (schedule)</SelectItem>
                    <SelectItem value="timeout">Timeout (vencimiento)</SelectItem>
                  </SelectContent>
                </Select>
              </div>
              <div className="grid grid-cols-2 gap-3">
                <div className="space-y-2">
                  <Label>Valor</Label>
                  <Input type="number" value={form.timerValue || ''} onChange={e => update({ timerValue: parseInt(e.target.value) || 0 })} />
                </div>
                <div className="space-y-2">
                  <Label>Unidad</Label>
                  <Select value={form.timerUnit || 'hours'} onValueChange={v => update({ timerUnit: v as any })}>
                    <SelectTrigger><SelectValue /></SelectTrigger>
                    <SelectContent>
                      <SelectItem value="minutes">Minutos</SelectItem>
                      <SelectItem value="hours">Horas</SelectItem>
                      <SelectItem value="days">Días</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
              </div>
            </div>
          )}
        </div>

        <DialogFooter className="mt-4">
          <Button variant="outline" onClick={() => onOpenChange(false)}>Cancelar</Button>
          <Button onClick={handleSave}>Guardar</Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
