// types.ts — TypeScript mirror of graphql/schema.graphql (the shared contract).
export interface Mds { typeId: string; name: string; footprintW: number; footprintD: number; clearance: number[]; }
export interface Instance { instanceId: string; typeId: string; x: number; y: number; rotation: number; }
export interface Binding { kind: string; from: string; to: string; }
export interface Room { x0: number; y0: number; x1: number; y1: number; }
export interface Twin { room: Room; instances: Instance[]; bindings: Binding[]; }
export interface Violation { rule: string; severity: 'ERROR' | 'WARNING'; message: string; }
export interface CommandResult { status: 'ACCEPTED' | 'REJECTED'; violations: Violation[]; event?: { seq: number }; }
export interface PlaceInput { typeId: string; x: number; y: number; rotation: number; }
export interface AgentProposal { proposal: PlaceInput | null; rationale: string; citations: string[]; }
