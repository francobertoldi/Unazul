declare module '@xyflow/react' {
  import { ComponentType, CSSProperties } from 'react';

  export interface NodeProps {
    id: string;
    data: Record<string, any>;
    selected?: boolean;
    type?: string;
    [key: string]: any;
  }

  export interface Node {
    id: string;
    type?: string;
    position: { x: number; y: number };
    data: Record<string, any>;
    selected?: boolean;
    [key: string]: any;
  }

  export interface Edge {
    id: string;
    source: string;
    target: string;
    sourceHandle?: string | null;
    targetHandle?: string | null;
    label?: string;
    type?: string;
    animated?: boolean;
    style?: CSSProperties;
    markerEnd?: any;
    selected?: boolean;
    [key: string]: any;
  }

  export interface Connection {
    source: string | null;
    target: string | null;
    sourceHandle: string | null;
    targetHandle: string | null;
  }

  export enum Position {
    Left = 'left',
    Top = 'top',
    Right = 'right',
    Bottom = 'bottom',
  }

  export enum BackgroundVariant {
    Lines = 'lines',
    Dots = 'dots',
    Cross = 'cross',
  }

  export enum MarkerType {
    Arrow = 'arrow',
    ArrowClosed = 'arrowclosed',
  }

  export const ReactFlow: ComponentType<any>;
  export const ReactFlowProvider: ComponentType<any>;
  export const Handle: ComponentType<any>;
  export const Controls: ComponentType<any>;
  export const Background: ComponentType<any>;
  export const MiniMap: ComponentType<any>;
  export const Panel: ComponentType<any>;

  export function useNodesState(initialNodes: Node[]): [Node[], (nodes: Node[] | ((nds: Node[]) => Node[])) => void, (changes: any) => void];
  export function useEdgesState(initialEdges: Edge[]): [Edge[], (edges: Edge[] | ((eds: Edge[]) => Edge[])) => void, (changes: any) => void];
  export function addEdge(connection: any, edges: Edge[]): Edge[];
}
