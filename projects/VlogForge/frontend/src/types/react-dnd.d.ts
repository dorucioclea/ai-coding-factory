/**
 * Type declarations for react-dnd with bundler module resolution
 */

declare module 'react-dnd' {
  import type { ComponentType, ReactNode } from 'react';

  // DndProvider
  export interface DndProviderProps<BackendContext, BackendOptions> {
    backend: (manager: unknown, context?: BackendContext, options?: BackendOptions) => unknown;
    context?: BackendContext;
    options?: BackendOptions;
    children?: ReactNode;
    debugMode?: boolean;
  }

  export const DndProvider: ComponentType<DndProviderProps<unknown, unknown>>;

  // Connector type - can be used as ref prop
  // Using any to support various element types (HTMLDivElement, etc.)
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  export type ConnectDragSource = any;
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  export type ConnectDropTarget = any;

  // useDrag
  export interface DragSourceHookSpec<DragObject, DropResult, CollectedProps> {
    type: string | symbol;
    item: DragObject | (() => DragObject);
    previewOptions?: unknown;
    options?: unknown;
    end?: (item: DragObject, monitor: DragSourceMonitor<DragObject, DropResult>) => void;
    canDrag?: boolean | ((monitor: DragSourceMonitor<DragObject, DropResult>) => boolean);
    isDragging?: (monitor: DragSourceMonitor<DragObject, DropResult>) => boolean;
    collect?: (monitor: DragSourceMonitor<DragObject, DropResult>) => CollectedProps;
  }

  export interface DragSourceMonitor<DragObject = unknown, DropResult = unknown> {
    canDrag(): boolean;
    isDragging(): boolean;
    getItemType(): string | symbol | null;
    getItem<T = DragObject>(): T;
    getDropResult<T = DropResult>(): T | null;
    didDrop(): boolean;
    getInitialClientOffset(): { x: number; y: number } | null;
    getInitialSourceClientOffset(): { x: number; y: number } | null;
    getClientOffset(): { x: number; y: number } | null;
    getDifferenceFromInitialOffset(): { x: number; y: number } | null;
    getSourceClientOffset(): { x: number; y: number } | null;
  }

  export function useDrag<DragObject, DropResult, CollectedProps>(
    specArg: DragSourceHookSpec<DragObject, DropResult, CollectedProps>,
    deps?: unknown[]
  ): [CollectedProps, ConnectDragSource];

  // useDrop
  export interface DropTargetHookSpec<DragObject, DropResult, CollectedProps> {
    accept: string | symbol | Array<string | symbol>;
    options?: unknown;
    drop?: (item: DragObject, monitor: DropTargetMonitor<DragObject, DropResult>) => DropResult | undefined;
    hover?: (item: DragObject, monitor: DropTargetMonitor<DragObject, DropResult>) => void;
    canDrop?: (item: DragObject, monitor: DropTargetMonitor<DragObject, DropResult>) => boolean;
    collect?: (monitor: DropTargetMonitor<DragObject, DropResult>) => CollectedProps;
  }

  export interface DropTargetMonitor<DragObject = unknown, DropResult = unknown> {
    canDrop(): boolean;
    isOver(options?: { shallow?: boolean }): boolean;
    getItemType(): string | symbol | null;
    getItem<T = DragObject>(): T;
    getDropResult<T = DropResult>(): T | null;
    didDrop(): boolean;
    getInitialClientOffset(): { x: number; y: number } | null;
    getInitialSourceClientOffset(): { x: number; y: number } | null;
    getClientOffset(): { x: number; y: number } | null;
    getDifferenceFromInitialOffset(): { x: number; y: number } | null;
    getSourceClientOffset(): { x: number; y: number } | null;
  }

  export function useDrop<DragObject, DropResult, CollectedProps>(
    specArg: DropTargetHookSpec<DragObject, DropResult, CollectedProps>,
    deps?: unknown[]
  ): [CollectedProps, ConnectDropTarget];
}

declare module 'react-dnd-html5-backend' {
  const HTML5Backend: (manager: unknown) => unknown;
  export { HTML5Backend };
  export default HTML5Backend;
}
