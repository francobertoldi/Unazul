export type FlowNodeType =
  | 'start'
  | 'end'
  | 'service_call'
  | 'decision'
  | 'send_message'
  | 'data_capture'
  | 'timer';

export interface FlowNodeData {
  [key: string]: unknown;
  label: string;
  description?: string;
  nodeType: FlowNodeType;
  // service_call
  serviceId?: string;
  serviceName?: string;
  endpoint?: string;
  method?: 'GET' | 'POST' | 'PUT' | 'DELETE';
  // decision
  condition?: string;
  trueLabel?: string;
  falseLabel?: string;
  // send_message
  channel?: 'email' | 'sms' | 'push' | 'webhook';
  template?: string;
  recipient?: string;
  // data_capture
  formFields?: { name: string; type: string; required: boolean }[];
  screenTitle?: string;
  // timer
  timerType?: 'delay' | 'schedule' | 'timeout';
  timerValue?: number;
  timerUnit?: 'minutes' | 'hours' | 'days';
}

export const nodeTypeConfig: Record<FlowNodeType, { label: string; color: string; icon: string }> = {
  start: { label: 'Inicio', color: 'hsl(var(--primary))', icon: 'Play' },
  end: { label: 'Fin', color: 'hsl(var(--muted-foreground))', icon: 'Square' },
  service_call: { label: 'Consulta Servicio', color: 'hsl(var(--primary))', icon: 'Globe' },
  decision: { label: 'Decisión', color: 'hsl(var(--accent))', icon: 'GitFork' },
  send_message: { label: 'Envío Mensaje', color: 'hsl(221 83% 53%)', icon: 'Send' },
  data_capture: { label: 'Captura Datos', color: 'hsl(142 71% 45%)', icon: 'FormInput' },
  timer: { label: 'Temporizador', color: 'hsl(25 95% 53%)', icon: 'Timer' },
};
