// twin.service.ts — Angular client of the SAME GraphQL contract Unity uses.
// The web viewer and the Unity client are two thin lenses over one truth.
// (Plain fetch keeps the demo dependency-free; swap to Apollo Angular in prod.)

import { Injectable, signal } from '@angular/core';
import { Twin, CommandResult, Mds, AgentProposal } from './types';

const GQL = 'http://localhost:8099/graphql';

@Injectable({ providedIn: 'root' })
export class TwinService {
  readonly twin = signal<Twin | null>(null);
  readonly registry = signal<Mds[]>([]);
  readonly lastResult = signal<CommandResult | null>(null);

  private async gql<T>(query: string, variables: any = {}): Promise<T> {
    const r = await fetch(GQL, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ query, variables }),
    });
    const { data, errors } = await r.json();
    if (errors) throw new Error(JSON.stringify(errors));
    return data as T;
  }

  async loadRegistry() {
    const d = await this.gql<{ registry: Mds[] }>(
      `{ registry { typeId name footprintW footprintD clearance } }`);
    this.registry.set(d.registry);
  }

  async refresh(room: string) {
    const d = await this.gql<{ twin: Twin }>(
      `query($r:String!){ twin(room:$r){
         room{x0 y0 x1 y1}
         instances{instanceId typeId x y rotation}
         bindings{kind from to} } }`, { r: room });
    this.twin.set(d.twin);
  }

  // INTENT: ask the server to place; render the authoritative twin it returns.
  async place(room: string, typeId: string, x: number, y: number, rotation = 0) {
    const d = await this.gql<{ placeModucule: CommandResult }>(
      `mutation($r:String!,$c:PlaceInput!){ placeModucule(room:$r,cmd:$c){
         status violations{rule severity message} event{seq} } }`,
      { r: room, c: { typeId, x, y, rotation } });
    this.lastResult.set(d.placeModucule);
    await this.refresh(room);                 // pull truth (stand-in for subscription push)
    return d.placeModucule;
  }

  async remove(room: string, instanceId: string) {
    await this.gql(
      `mutation($r:String!,$id:String!){ removeModucule(room:$r,instanceId:$id){ status } }`,
      { r: room, id: instanceId });
    await this.refresh(room);
  }

  async agentSuggest(room: string): Promise<AgentProposal> {
    const d = await this.gql<{ agentSuggest: AgentProposal }>(
      `mutation($r:String!){ agentSuggest(room:$r){
         proposal{typeId x y rotation} rationale citations } }`, { r: room });
    return d.agentSuggest;
  }
}
