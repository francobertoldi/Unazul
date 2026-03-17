import { memo } from 'react';
import { Handle, Position, type NodeProps } from '@xyflow/react';
import {
  Play, Square, Globe, GitFork, Send, FormInput, Timer, GripVertical,
} from 'lucide-react';
import { cn } from '@/lib/utils';
import type { FlowNodeData, FlowNodeType } from '@/data/workflowNodes';

const iconMap: Record<string, React.ElementType> = {
  Play, Square, Globe, GitFork, Send, FormInput, Timer,
};

const styleMap: Record<FlowNodeType, { bg: string; border: string; iconBg: string }> = {
  start: { bg: 'bg-primary/5', border: 'border-primary/30', iconBg: 'bg-primary/10 text-primary' },
  end: { bg: 'bg-muted/50', border: 'border-muted-foreground/30', iconBg: 'bg-muted text-muted-foreground' },
  service_call: { bg: 'bg-primary/5', border: 'border-primary/30', iconBg: 'bg-primary/10 text-primary' },
  decision: { bg: 'bg-accent/5', border: 'border-accent/30', iconBg: 'bg-accent/10 text-accent' },
  send_message: { bg: 'bg-primary/5', border: 'border-primary/30', iconBg: 'bg-primary/10 text-primary' },
  data_capture: { bg: 'bg-primary/5', border: 'border-primary/20', iconBg: 'bg-primary/10 text-primary' },
  timer: { bg: 'bg-destructive/5', border: 'border-destructive/20', iconBg: 'bg-destructive/10 text-destructive' },
};

import { nodeTypeConfig } from '@/data/workflowNodes';

function FlowNodeComponent({ data: rawData, selected }: NodeProps) {
  const data = rawData as unknown as FlowNodeData;
  const nodeType = data.nodeType;
  const config = nodeTypeConfig[nodeType];
  const style = styleMap[nodeType];
  const IconComp = iconMap[config.icon] || Globe;
  const isDecision = nodeType === 'decision';

  return (
    <div className={cn(
      'rounded-xl border-2 shadow-sm min-w-[180px] max-w-[220px] transition-all',
      style.bg, style.border,
      selected && 'ring-2 ring-ring ring-offset-2 ring-offset-background shadow-md',
    )}>
      {/* Inputs */}
      {nodeType !== 'start' && (
        <Handle type="target" position={Position.Top}
          className="!w-3 !h-3 !bg-border !border-2 !border-background" />
      )}

      {/* Header */}
      <div className="flex items-center gap-2 px-3 py-2.5">
        <div className={cn('h-7 w-7 rounded-lg flex items-center justify-center shrink-0', style.iconBg)}>
          <IconComp className="h-3.5 w-3.5" />
        </div>
        <div className="min-w-0 flex-1">
          <p className="text-xs font-semibold text-foreground truncate">{data.label}</p>
          <p className="text-[10px] text-muted-foreground truncate">{config.label}</p>
        </div>
        <GripVertical className="h-3 w-3 text-muted-foreground/40 shrink-0 cursor-grab" />
      </div>

      {/* Details */}
      {data.description && (
        <div className="px-3 pb-2">
          <p className="text-[10px] text-muted-foreground line-clamp-2">{data.description}</p>
        </div>
      )}

      {nodeType === 'service_call' && data.serviceName && (
        <div className="px-3 pb-2">
          <span className="text-[10px] bg-muted px-1.5 py-0.5 rounded font-mono text-foreground">
            {data.method || 'GET'} {data.serviceName}
          </span>
        </div>
      )}

      {nodeType === 'timer' && data.timerValue && (
        <div className="px-3 pb-2">
          <span className="text-[10px] bg-muted px-1.5 py-0.5 rounded text-foreground">
            {data.timerType === 'delay' ? 'Esperar' : data.timerType === 'timeout' ? 'Timeout' : 'Programado'}: {data.timerValue} {data.timerUnit}
          </span>
        </div>
      )}

      {nodeType === 'send_message' && data.channel && (
        <div className="px-3 pb-2">
          <span className="text-[10px] bg-muted px-1.5 py-0.5 rounded text-foreground capitalize">
            {data.channel}
          </span>
        </div>
      )}

      {nodeType === 'data_capture' && data.formFields && (
        <div className="px-3 pb-2">
          <span className="text-[10px] bg-muted px-1.5 py-0.5 rounded text-foreground">
            {data.formFields.length} campo{data.formFields.length !== 1 ? 's' : ''}
          </span>
        </div>
      )}

      {/* Outputs */}
      {nodeType !== 'end' && !isDecision && (
        <Handle type="source" position={Position.Bottom}
          className="!w-3 !h-3 !bg-border !border-2 !border-background" />
      )}

      {isDecision && (
        <>
          <Handle type="source" position={Position.Left} id="false"
            className="!w-3 !h-3 !bg-destructive !border-2 !border-background" />
          <Handle type="source" position={Position.Right} id="true"
            className="!w-3 !h-3 !bg-primary !border-2 !border-background" />
          <div className="flex justify-between px-3 pb-1.5">
            <span className="text-[9px] text-destructive font-medium">{data.falseLabel || 'No'}</span>
            <span className="text-[9px] text-primary font-medium">{data.trueLabel || 'Sí'}</span>
          </div>
        </>
      )}
    </div>
  );
}

export default memo(FlowNodeComponent);
