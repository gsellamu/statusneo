// room-planner.component.ts — Angular standalone "lens" over the twin.
// Renders the payload to a 2-D canvas, emits PLACE intents, surfaces verdicts.
// Identical contract to the Unity client; zero domain logic lives here.

import { Component, ElementRef, OnInit, ViewChild, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TwinService } from './twin.service';
import { Violation, Mds } from './types';

@Component({
  selector: 'app-room-planner',
  standalone: true,
  imports: [CommonModule],
  template: `
  <div class="bar">
    <strong>Modutecture Spine</strong> — Room Planner (Angular lens)
    <span>thin client: renders server truth, emits intents, owns nothing</span>
  </div>
  <div class="palette">
    <button *ngFor="let m of svc.registry()"
            [class.sel]="sel()===m.typeId" (click)="sel.set(m.typeId)">{{ m.name }}</button>
    <button class="tool" (click)="rotate()">Rotate ghost: {{ rot() }}°</button>
    <button class="tool" (click)="suggest()">Suggest a bed (agent)</button>
  </div>
  <canvas #cv width="800" height="600" (click)="onClick($event)"></canvas>
  <div class="status" [class.err]="result()?.status==='REJECTED'"
                      [class.ok]="result()?.status==='ACCEPTED'">{{ statusText() }}</div>
  `,
  styles: [`
    :host{display:block;font-family:Calibri,system-ui,sans-serif}
    .bar{background:#16263f;color:#fff;padding:10px 14px}
    .bar strong{color:#e8a816} .bar span{color:#c9d6e8;font-size:13px;margin-left:8px}
    .palette{display:flex;gap:8px;padding:10px 14px;flex-wrap:wrap;border-bottom:1px solid #dfe6ee}
    button{padding:6px 12px;border:1.5px solid #16263f;border-radius:18px;background:#fff;cursor:pointer}
    button.sel{background:#16263f;color:#fff} button.tool{border-color:#5b6b7e;color:#5b6b7e}
    canvas{display:block;margin:16px;border:1px solid #dfe6ee;background:#fbfcfe;cursor:crosshair}
    .status{padding:8px 16px;font-size:13px} .ok{color:#1e7a4c} .err{color:#b3402f}
  `],
})
export class RoomPlannerComponent implements OnInit {
  @ViewChild('cv', { static: true }) cv!: ElementRef<HTMLCanvasElement>;
  room = 'r1';
  sel = signal<string>('headwall-hw204');
  rot = signal<number>(0);
  result = computed(() => this.svc.lastResult());
  private SX = 800 / 4000; private SY = 600 / 3000;
  private colors: Record<string, string> = {
    'headwall-hw204': '#23375a', 'bed-icu': '#3b6ea5', 'sink-clinical': '#6b8fb5',
  };

  constructor(public svc: TwinService) {}

  async ngOnInit() {
    await this.svc.loadRegistry();
    await this.svc.refresh(this.room);
    this.draw();
    setInterval(async () => { await this.svc.refresh(this.room); this.draw(); }, 1500);
  }

  rotate() { this.rot.set((this.rot() + 90) % 360); }

  async onClick(ev: MouseEvent) {
    const r = this.cv.nativeElement.getBoundingClientRect();
    const x = Math.round((ev.clientX - r.left) / this.SX);
    const y = Math.round((ev.clientY - r.top) / this.SY);
    await this.svc.place(this.room, this.sel(), x, y, this.rot());
    this.draw();
  }

  async suggest() {
    const p = await this.svc.agentSuggest(this.room);
    if (!p.proposal) { alert(p.rationale); return; }
    if (confirm(`${p.rationale}\n\nCites: ${p.citations.join(', ')}\n\nApprove & commit?`)) {
      await this.svc.place(this.room, p.proposal.typeId, p.proposal.x, p.proposal.y, p.proposal.rotation);
      this.draw();
    }
  }

  statusText(): string {
    const r = this.result();
    if (!r) return 'Pick a Moducule, then click in the room. Server validates every placement.';
    if (r.status === 'ACCEPTED') {
      const w = r.violations.filter((v: Violation) => v.severity === 'WARNING').map((v: Violation) => v.message);
      return `✓ COMMITTED seq #${r.event?.seq}` + (w.length ? `  ⚠ ${w.join('; ')}` : '');
    }
    return `✗ REJECTED — nothing written.  ` +
      r.violations.map((v: Violation) => `[${v.rule}] ${v.message}`).join('  ');
  }

  private dims(typeId: string, rot: number): [number, number] {
    const m = this.svc.registry().find((x: Mds) => x.typeId === typeId)!;
    return rot % 180 ? [m.footprintD, m.footprintW] : [m.footprintW, m.footprintD];
  }

  draw() {
    const ctx = this.cv.nativeElement.getContext('2d')!;
    ctx.clearRect(0, 0, 800, 600);
    ctx.strokeStyle = '#16263f'; ctx.lineWidth = 2; ctx.strokeRect(1, 1, 798, 598);
    const twin = this.svc.twin(); if (!twin) return;

    for (const b of twin.bindings) {            // earned med-gas edges
      const f = twin.instances.find((i) => i.instanceId === b.from);
      const t = twin.instances.find((i) => i.instanceId === b.to);
      if (!f || !t) continue;
      ctx.strokeStyle = '#e8a816'; ctx.setLineDash([6, 4]); ctx.lineWidth = 2;
      ctx.beginPath(); ctx.moveTo(f.x * this.SX, f.y * this.SY);
      ctx.lineTo(t.x * this.SX, t.y * this.SY); ctx.stroke(); ctx.setLineDash([]);
    }
    for (const i of twin.instances) {
      const [w, d] = this.dims(i.typeId, i.rotation);
      const x = (i.x - w / 2) * this.SX, y = (i.y - d / 2) * this.SY;
      ctx.fillStyle = this.colors[i.typeId] ?? '#888';
      ctx.fillRect(x, y, w * this.SX, d * this.SY);
      ctx.strokeStyle = '#0d1626'; ctx.strokeRect(x, y, w * this.SX, d * this.SY);
      ctx.fillStyle = '#fff'; ctx.font = '11px Calibri';
      ctx.fillText(i.typeId, x + 4, y + 14);
    }
  }
}
