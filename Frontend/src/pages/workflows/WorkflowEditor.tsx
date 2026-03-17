import { useState, useCallback, useRef, useMemo } from 'react';
import {
  ReactFlow,
  addEdge,
  useNodesState,
  useEdgesState,
  Controls,
  Background,
  BackgroundVariant,
  MiniMap,
  Panel,
  type Connection,
  type Edge,
  type Node,
  MarkerType,
  ReactFlowProvider,
} from '@xyflow/react';
import '@xyflow/react/dist/style.css';
import { useNavigate, useParams } from 'react-router-dom';
import {
  Play, Square, Globe, GitFork, Send, FormInput, Timer,
  Save, ArrowLeft, Plus, Trash2, MousePointer2, GripVertical,
  ChevronDown, ChevronRight,
} from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Separator } from '@/components/ui/separator';
import { Card } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { ScrollArea } from '@/components/ui/scroll-area';
import { Collapsible, CollapsibleContent, CollapsibleTrigger } from '@/components/ui/collapsible';
import FlowNodeComponent from '@/components/workflow/FlowNode';
import NodeConfigDialog from '@/components/workflow/NodeConfigDialog';
import { useWorkflowStore } from '@/data/store';
import { toast } from 'sonner';
import { cn } from '@/lib/utils';
import type { FlowNodeData, FlowNodeType } from '@/data/workflowNodes';
import { nodeTypeConfig } from '@/data/workflowNodes';

// ── Attribute catalog for the right panel ──

interface AttributeDef {
  name: string;
  type: 'string' | 'number' | 'date' | 'boolean' | 'enum' | 'object';
  description?: string;
}

interface AttributeGroup {
  object: string;
  icon: string;
  attributes: AttributeDef[];
}

const attributeCatalog: AttributeGroup[] = [
  {
    object: 'Persona (Solicitante)',
    icon: '👤',
    attributes: [
      { name: 'firstName', type: 'string', description: 'Nombre' },
      { name: 'lastName', type: 'string', description: 'Apellido' },
      { name: 'documentType', type: 'enum', description: 'Tipo documento (DNI/CUIT/Pasaporte)' },
      { name: 'documentNumber', type: 'string', description: 'Número de documento' },
      { name: 'email', type: 'string', description: 'Correo electrónico' },
      { name: 'phone', type: 'string', description: 'Teléfono' },
      { name: 'birthDate', type: 'date', description: 'Fecha de nacimiento' },
      { name: 'age', type: 'number', description: 'Edad (calculada)' },
      { name: 'gender', type: 'enum', description: 'Género (sexo)' },
      { name: 'occupation', type: 'string', description: 'Ocupación' },
    ],
  },
  {
    object: 'Domicilio',
    icon: '📍',
    attributes: [
      { name: 'street', type: 'string', description: 'Calle' },
      { name: 'number', type: 'string', description: 'Número' },
      { name: 'floor', type: 'string', description: 'Piso' },
      { name: 'apartment', type: 'string', description: 'Departamento' },
      { name: 'city', type: 'string', description: 'Ciudad' },
      { name: 'province', type: 'string', description: 'Provincia' },
      { name: 'postalCode', type: 'string', description: 'Código postal' },
      { name: 'latitude', type: 'number', description: 'Latitud' },
      { name: 'longitude', type: 'number', description: 'Longitud' },
    ],
  },
  {
    object: 'Solicitud',
    icon: '📋',
    attributes: [
      { name: 'code', type: 'string', description: 'Código de solicitud' },
      { name: 'status', type: 'enum', description: 'Estado (borrador/pendiente/en revisión/aprobada/rechazada/cancelada)' },
      { name: 'entityName', type: 'string', description: 'Entidad' },
      { name: 'productName', type: 'string', description: 'Nombre del producto' },
      { name: 'planName', type: 'string', description: 'Nombre del plan' },
      { name: 'currentWorkflowStateName', type: 'string', description: 'Etapa actual del workflow' },
      { name: 'assignedUserName', type: 'string', description: 'Usuario asignado' },
      { name: 'createdAt', type: 'date', description: 'Fecha de creación' },
      { name: 'updatedAt', type: 'date', description: 'Fecha de actualización' },
    ],
  },
  {
    object: 'Producto',
    icon: '📦',
    attributes: [
      { name: 'name', type: 'string', description: 'Nombre del producto' },
      { name: 'code', type: 'string', description: 'Código' },
      { name: 'familyName', type: 'string', description: 'Familia de producto' },
      { name: 'version', type: 'number', description: 'Versión' },
      { name: 'status', type: 'enum', description: 'Estado (borrador/activo/inactivo/deprecado)' },
      { name: 'validFrom', type: 'date', description: 'Vigencia desde' },
      { name: 'validTo', type: 'date', description: 'Vigencia hasta' },
    ],
  },
  {
    object: 'SubProducto (Plan)',
    icon: '📄',
    attributes: [
      { name: 'name', type: 'string', description: 'Nombre del plan' },
      { name: 'price', type: 'number', description: 'Precio' },
      { name: 'currency', type: 'string', description: 'Moneda' },
      { name: 'installments', type: 'number', description: 'Cuotas' },
      { name: 'otherCosts', type: 'number', description: 'Otros costos' },
      { name: 'status', type: 'enum', description: 'Estado' },
    ],
  },
  {
    object: 'SubProducto — Préstamo',
    icon: '🏦',
    attributes: [
      { name: 'amortizationType', type: 'enum', description: 'Sistema de amortización (francés/alemán/americano/bullet)' },
      { name: 'annualEffectiveRate', type: 'number', description: 'TEA (%)' },
      { name: 'cftRate', type: 'number', description: 'CFT (%)' },
      { name: 'adminFees', type: 'number', description: 'Gastos administrativos' },
    ],
  },
  {
    object: 'SubProducto — Seguro',
    icon: '🛡️',
    attributes: [
      { name: 'premium', type: 'number', description: 'Prima' },
      { name: 'sumInsured', type: 'number', description: 'Suma asegurada' },
      { name: 'gracePeriodDays', type: 'number', description: 'Días de gracia' },
      { name: 'coverageType', type: 'enum', description: 'Tipo cobertura (individual/grupal/colectivo)' },
    ],
  },
  {
    object: 'SubProducto — Tarjeta',
    icon: '💳',
    attributes: [
      { name: 'creditLimit', type: 'number', description: 'Límite de crédito' },
      { name: 'annualFee', type: 'number', description: 'Cuota anual' },
      { name: 'interestRate', type: 'number', description: 'Tasa de interés (%)' },
      { name: 'network', type: 'enum', description: 'Red (Visa/Mastercard/Amex/Cabal/Naranja)' },
      { name: 'gracePeriodDays', type: 'number', description: 'Días de gracia' },
      { name: 'level', type: 'string', description: 'Nivel' },
    ],
  },
  {
    object: 'SubProducto — Cuenta',
    icon: '🏧',
    attributes: [
      { name: 'maintenanceFee', type: 'number', description: 'Cuota de mantenimiento' },
      { name: 'minimumBalance', type: 'number', description: 'Balance mínimo' },
      { name: 'interestRate', type: 'number', description: 'Tasa de interés (%)' },
      { name: 'accountType', type: 'enum', description: 'Tipo cuenta (ahorro/corriente/money market)' },
    ],
  },
  {
    object: 'SubProducto — Inversión',
    icon: '📈',
    attributes: [
      { name: 'minimumAmount', type: 'number', description: 'Monto mínimo' },
      { name: 'expectedReturn', type: 'number', description: 'Rendimiento esperado (%)' },
      { name: 'termDays', type: 'number', description: 'Plazo (días)' },
      { name: 'riskLevel', type: 'enum', description: 'Nivel de riesgo (bajo/medio/alto)' },
      { name: 'instrumentType', type: 'enum', description: 'Tipo instrumento (plazo fijo/bono/FCI/acción)' },
    ],
  },
  {
    object: 'Cobertura (Seguro)',
    icon: '🔒',
    attributes: [
      { name: 'name', type: 'string', description: 'Nombre de la cobertura' },
      { name: 'description', type: 'string', description: 'Descripción' },
      { name: 'sumInsured', type: 'number', description: 'Suma asegurada' },
      { name: 'premium', type: 'number', description: 'Prima' },
    ],
  },
];

const typeColors: Record<string, string> = {
  string: 'bg-blue-100 text-blue-700 dark:bg-blue-900/40 dark:text-blue-300',
  number: 'bg-green-100 text-green-700 dark:bg-green-900/40 dark:text-green-300',
  date: 'bg-amber-100 text-amber-700 dark:bg-amber-900/40 dark:text-amber-300',
  boolean: 'bg-purple-100 text-purple-700 dark:bg-purple-900/40 dark:text-purple-300',
  enum: 'bg-pink-100 text-pink-700 dark:bg-pink-900/40 dark:text-pink-300',
  object: 'bg-muted text-muted-foreground',
};

const nodeTypes = { flowNode: FlowNodeComponent };

const paletteItems: { type: FlowNodeType; icon: React.ElementType }[] = [
  { type: 'start', icon: Play },
  { type: 'end', icon: Square },
  { type: 'service_call', icon: Globe },
  { type: 'decision', icon: GitFork },
  { type: 'send_message', icon: Send },
  { type: 'data_capture', icon: FormInput },
  { type: 'timer', icon: Timer },
];

function generateNodeId() {
  return `node_${Date.now()}_${Math.random().toString(36).slice(2, 5)}`;
}

const defaultEdgeOptions = {
  animated: true,
  style: { strokeWidth: 2 },
  markerEnd: { type: MarkerType.ArrowClosed, width: 16, height: 16 },
};

// Build initial nodes/edges from workflow states/transitions
function buildInitialFlow(workflowId: string | undefined, workflows: any[]): { nodes: Node[]; edges: Edge[] } {
  if (!workflowId || workflowId === 'nuevo') {
    const startNode: Node = {
      id: 'start_1', type: 'flowNode', position: { x: 300, y: 50 },
      data: { label: 'Inicio', nodeType: 'start' as FlowNodeType } as FlowNodeData,
    };
    return { nodes: [startNode], edges: [] };
  }

  const wf = workflows.find(w => w.id === workflowId);
  if (!wf) return { nodes: [], edges: [] };

  const nodes: Node[] = wf.states.map((s: any, i: number) => ({
    id: s.id,
    type: 'flowNode',
    position: { x: 300, y: i * 150 },
    data: {
      label: s.name,
      nodeType: (s.type === 'initial' ? 'start' : s.type === 'final' ? 'end' : 'service_call') as FlowNodeType,
      description: `SLA: ${s.slaHours || 'N/A'}h`,
    } as FlowNodeData,
  }));

  const edges: Edge[] = wf.transitions.map((t: any) => ({
    id: t.id,
    source: t.fromStateId,
    target: t.toStateId,
    label: t.name,
    ...defaultEdgeOptions,
  }));

  return { nodes, edges };
}

function WorkflowEditorInner() {
  const navigate = useNavigate();
  const { id } = useParams();
  const { workflows } = useWorkflowStore();
  const wf = workflows.find(w => w.id === id);

  const initial = useMemo(() => buildInitialFlow(id, workflows), [id, workflows]);
  const [nodes, setNodes, onNodesChange] = useNodesState(initial.nodes);
  const [edges, setEdges, onEdgesChange] = useEdgesState(initial.edges);

  const [wfName, setWfName] = useState(wf?.name || '');
  const [wfDesc, setWfDesc] = useState(wf?.description || '');
  const [wfCategory, setWfCategory] = useState(wf?.productCategory || 'loan');

  const [configNode, setConfigNode] = useState<Node | null>(null);
  const reactFlowWrapper = useRef<HTMLDivElement>(null);

  const onConnect = useCallback((connection: Connection) => {
    setEdges(eds => addEdge({ ...connection, ...defaultEdgeOptions }, eds));
  }, [setEdges]);

  const onNodeDoubleClick = useCallback((_: any, node: Node) => {
    setConfigNode(node);
  }, []);

  const onDragStart = (event: React.DragEvent, nodeType: FlowNodeType) => {
    event.dataTransfer.setData('application/reactflow', nodeType);
    event.dataTransfer.effectAllowed = 'move';
  };

  const onDragOver = useCallback((event: React.DragEvent) => {
    event.preventDefault();
    event.dataTransfer.dropEffect = 'move';
  }, []);

  const onDrop = useCallback((event: React.DragEvent) => {
    event.preventDefault();
    const type = event.dataTransfer.getData('application/reactflow') as FlowNodeType;
    if (!type) return;

    const bounds = reactFlowWrapper.current?.getBoundingClientRect();
    if (!bounds) return;

    const config = nodeTypeConfig[type];
    const position = {
      x: event.clientX - bounds.left - 90,
      y: event.clientY - bounds.top - 30,
    };

    const newNode: Node = {
      id: generateNodeId(),
      type: 'flowNode',
      position,
      data: {
        label: config.label,
        nodeType: type,
        description: '',
        ...(type === 'decision' ? { trueLabel: 'Sí', falseLabel: 'No', condition: '' } : {}),
        ...(type === 'service_call' ? { method: 'POST' } : {}),
        ...(type === 'send_message' ? { channel: 'email' } : {}),
        ...(type === 'data_capture' ? { formFields: [], screenTitle: '' } : {}),
        ...(type === 'timer' ? { timerType: 'delay', timerValue: 1, timerUnit: 'hours' } : {}),
      } as FlowNodeData,
    };

    setNodes(nds => [...nds, newNode]);
  }, [setNodes]);

  const deleteSelected = useCallback(() => {
    setNodes(nds => nds.filter(n => !n.selected));
    setEdges(eds => eds.filter(e => !e.selected));
  }, [setNodes, setEdges]);

  const handleNodeConfigSave = useCallback((updatedData: FlowNodeData) => {
    if (!configNode) return;
    setNodes(nds => nds.map(n => n.id === configNode.id ? { ...n, data: updatedData } : n));
    setConfigNode(null);
  }, [configNode, setNodes]);

  const handleSave = () => {
    if (!wfName.trim()) { toast.error('El nombre del workflow es obligatorio'); return; }
    toast.success('Workflow guardado correctamente');
    navigate('/workflows');
  };

  return (
    <div className="flex flex-col h-[calc(100vh-3.5rem)]">
      {/* Toolbar */}
      <div className="flex items-center justify-between px-4 py-2.5 border-b border-border bg-card">
        <div className="flex items-center gap-3">
          <Button variant="ghost" size="icon" className="h-8 w-8" onClick={() => navigate('/workflows')}>
            <ArrowLeft className="h-4 w-4" />
          </Button>
          <Separator orientation="vertical" className="h-6" />
          <Input value={wfName} onChange={e => setWfName(e.target.value)} placeholder="Nombre del workflow" className="h-8 w-56 text-sm font-medium" />
          <Select value={wfCategory} onValueChange={setWfCategory}>
            <SelectTrigger className="h-8 w-36 text-xs"><SelectValue /></SelectTrigger>
            <SelectContent>
              <SelectItem value="loan">Préstamos</SelectItem>
              <SelectItem value="insurance">Seguros</SelectItem>
              <SelectItem value="investment">Inversiones</SelectItem>
              <SelectItem value="credit_card">Tarjetas</SelectItem>
              <SelectItem value="account">Cuentas</SelectItem>
            </SelectContent>
          </Select>
        </div>
        <div className="flex items-center gap-2">
          <Button variant="outline" size="sm" className="h-8 text-xs gap-1" onClick={deleteSelected}>
            <Trash2 className="h-3 w-3" /> Eliminar selección
          </Button>
          <Button size="sm" className="h-8 text-xs gap-1" onClick={handleSave}>
            <Save className="h-3 w-3" /> Guardar
          </Button>
        </div>
      </div>

      <div className="flex flex-1 overflow-hidden">
        {/* Palette */}
        <div className="w-56 border-r border-border bg-card p-3 space-y-3 overflow-y-auto shrink-0">
          <p className="text-xs font-semibold text-muted-foreground uppercase tracking-wider">Componentes</p>
          <p className="text-[10px] text-muted-foreground">Arrastrá al canvas para agregar</p>
          <div className="space-y-1.5">
            {paletteItems.map(item => {
              const config = nodeTypeConfig[item.type];
              return (
                <div
                  key={item.type}
                  draggable
                  onDragStart={e => onDragStart(e, item.type)}
                  className="flex items-center gap-2.5 px-3 py-2.5 rounded-lg border border-border bg-background cursor-grab hover:shadow-sm hover:border-primary/30 transition-all active:cursor-grabbing"
                >
                  <item.icon className="h-4 w-4 text-muted-foreground" />
                  <div>
                    <p className="text-xs font-medium text-foreground">{config.label}</p>
                  </div>
                </div>
              );
            })}
          </div>

          <Separator />

          <div className="space-y-2">
            <p className="text-xs font-semibold text-muted-foreground uppercase tracking-wider">Descripción</p>
            <Textarea
              value={wfDesc}
              onChange={e => setWfDesc(e.target.value)}
              rows={3}
              className="text-xs"
              placeholder="Descripción del workflow..."
            />
          </div>

          <Separator />

          <div className="space-y-2">
            <p className="text-xs font-semibold text-muted-foreground uppercase tracking-wider">Ayuda</p>
            <div className="space-y-1.5 text-[10px] text-muted-foreground">
              <p className="flex items-center gap-1.5"><MousePointer2 className="h-3 w-3" /> Doble click para configurar</p>
              <p className="flex items-center gap-1.5"><Plus className="h-3 w-3" /> Arrastrar para conectar</p>
              <p className="flex items-center gap-1.5"><Trash2 className="h-3 w-3" /> Seleccionar + Eliminar</p>
            </div>
          </div>

          <div className="pt-2">
            <Badge variant="outline" className="text-[10px]">
              {nodes.length} nodos · {edges.length} conexiones
            </Badge>
          </div>
        </div>

        {/* Canvas */}
        <div ref={reactFlowWrapper} className="flex-1">
          <ReactFlow
            nodes={nodes}
            edges={edges}
            onNodesChange={onNodesChange}
            onEdgesChange={onEdgesChange}
            onConnect={onConnect}
            onNodeDoubleClick={onNodeDoubleClick}
            onDragOver={onDragOver}
            onDrop={onDrop}
            nodeTypes={nodeTypes}
            defaultEdgeOptions={defaultEdgeOptions}
            fitView
            deleteKeyCode="Delete"
            className="bg-background"
          >
            <Controls className="!bg-card !border-border !shadow-sm [&>button]:!bg-card [&>button]:!border-border [&>button]:!text-foreground [&>button:hover]:!bg-muted" />
            <Background variant={BackgroundVariant.Dots} gap={20} size={1} className="!bg-muted/20" />
            <MiniMap
              className="!bg-card !border-border"
              maskColor="hsl(var(--muted) / 0.5)"
              nodeColor="hsl(var(--primary) / 0.4)"
            />
          </ReactFlow>
        </div>

        {/* Attributes Panel */}
        <div className="w-64 border-l border-border bg-card shrink-0 flex flex-col overflow-hidden">
          <div className="px-3 py-2.5 border-b border-border">
            <p className="text-xs font-semibold text-muted-foreground uppercase tracking-wider">Atributos</p>
            <p className="text-[10px] text-muted-foreground mt-0.5">Arrastrá al canvas para usar en fórmulas</p>
          </div>
          <ScrollArea className="flex-1">
            <div className="p-2 space-y-1">
              {attributeCatalog.map(group => (
                <Collapsible key={group.object} defaultOpen={false}>
                  <CollapsibleTrigger className="flex items-center gap-2 w-full px-2 py-1.5 rounded-md hover:bg-muted/50 transition-colors text-left group">
                    <ChevronRight className="h-3 w-3 text-muted-foreground transition-transform group-data-[state=open]:rotate-90" />
                    <span className="text-sm">{group.icon}</span>
                    <span className="text-xs font-medium text-foreground truncate flex-1">{group.object}</span>
                    <Badge variant="secondary" className="text-[9px] px-1 py-0 h-4">{group.attributes.length}</Badge>
                  </CollapsibleTrigger>
                  <CollapsibleContent>
                    <div className="ml-3 pl-2 border-l border-border/50 space-y-0.5 py-1">
                      {group.attributes.map(attr => (
                        <div
                          key={`${group.object}.${attr.name}`}
                          draggable
                          onDragStart={e => {
                            e.dataTransfer.setData('text/plain', `{{${group.object}.${attr.name}}}`);
                            e.dataTransfer.effectAllowed = 'copy';
                          }}
                          className="flex items-center gap-1.5 px-2 py-1.5 rounded-md border border-transparent hover:border-border hover:bg-muted/30 cursor-grab active:cursor-grabbing transition-all group/attr"
                          title={`${group.object}.${attr.name} (${attr.type})${attr.description ? ' — ' + attr.description : ''}`}
                        >
                          <GripVertical className="h-3 w-3 text-muted-foreground/40 group-hover/attr:text-muted-foreground shrink-0" />
                          <div className="flex-1 min-w-0">
                            <p className="text-[11px] font-medium text-foreground truncate">{attr.description || attr.name}</p>
                            <p className="text-[9px] text-muted-foreground truncate">{group.object}.{attr.name}</p>
                          </div>
                          <span className={cn("text-[9px] px-1 py-0.5 rounded font-medium shrink-0", typeColors[attr.type] || typeColors.object)}>
                            {attr.type}
                          </span>
                        </div>
                      ))}
                    </div>
                  </CollapsibleContent>
                </Collapsible>
              ))}
            </div>
          </ScrollArea>
        </div>
      </div>

      {/* Node Config Dialog */}
      {configNode && (
        <NodeConfigDialog
          open={!!configNode}
          onOpenChange={open => { if (!open) setConfigNode(null); }}
          data={configNode.data as unknown as FlowNodeData}
          onSave={handleNodeConfigSave}
        />
      )}
    </div>
  );
}

export default function WorkflowEditor() {
  return (
    <ReactFlowProvider>
      <WorkflowEditorInner />
    </ReactFlowProvider>
  );
}
