/* studio.js — Lens Studio: many renderers, one twin.
 * Split-screen lenses (2D / schematic / 3D / data / Unity) all render the SAME GraphQL
 * twin payload and update together. Sync = polling baseline + graphql-ws subscription push.
 * This is the architecture proving itself: the renderer owns nothing; truth is the twin.
 * The Unity lens is Modutecture's OWN engine (a WebGL build) embedded as an iframe over the
 * same twin — "Unity becomes the GPU; the twin holds the truth." It can also emit PLACE
 * intents, which it mirrors back to this page so every other lens refreshes in lock-step.
 */
const ROOM = new URLSearchParams(location.search).get("room") || "exam-12";
// Unity WebGL lens source — override with ?unity=<url>; defaults to a build under /unity/.
const UNITY_URL = new URLSearchParams(location.search).get("unity") || "/unity/index.html";
const MM_W = 4000, MM_H = 3000;
const COLORS = { "headwall-hw204": "#23375a", "bed-icu": "#3b6ea5", "sink-clinical": "#6b8fb5" };
const HEIGHT = { "headwall-hw204": 1200, "bed-icu": 600, "sink-clinical": 900 }; // mm, for 3D

let state = { twin: { instances: [], bindings: [], version: 0 }, reg: {} };
let selected = "headwall-hw204", ghostRot = 0;
let deltas = 0, live = false;

async function gql(query, variables = {}) {
  const r = await fetch("/graphql", {
    method: "POST", headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ query, variables }),
  });
  const j = await r.json();
  if (j.errors) throw new Error(JSON.stringify(j.errors));
  return j.data;
}

function dims(t, rot) { const m = state.reg[t]; if (!m) return [300, 300];
  return (rot % 180) ? [m.footprintD, m.footprintW] : [m.footprintW, m.footprintD]; }

/* ---------------------------------------------------------------- LENSES ---- */
const lenses = {
  // 1) 2D top-down floor plan (interactive: click to place)
  "2d": {
    label: "2D Floor Plan", kind: "interactive",
    mount(pane) {
      pane.innerHTML = `<canvas width="640" height="480" style="width:100%;height:auto;background:#fbfcfe;border:1px solid #dfe6ee;cursor:crosshair"></canvas>`;
      this.cv = pane.querySelector("canvas");
      const self = this;
      this.cv.addEventListener("mousemove", e => { const r = self.cv.getBoundingClientRect();
        self.mouse = { x: (e.clientX - r.left) * (MM_W / r.width), y: (e.clientY - r.top) * (MM_H / r.height) }; self.render(state.twin); });
      this.cv.addEventListener("mouseleave", () => { self.mouse = null; self.render(state.twin); });
      this.cv.addEventListener("click", async e => { const r = self.cv.getBoundingClientRect();
        const x = Math.round((e.clientX - r.left) * (MM_W / r.width)), y = Math.round((e.clientY - r.top) * (MM_H / r.height));
        await place(selected, x, y, ghostRot); });
    },
    render(twin) {
      const c = this.cv, ctx = c.getContext("2d"), SX = c.width / MM_W, SY = c.height / MM_H;
      ctx.clearRect(0, 0, c.width, c.height);
      ctx.strokeStyle = "#16263f"; ctx.lineWidth = 2; ctx.strokeRect(1, 1, c.width - 2, c.height - 2);
      twin.bindings.forEach(b => { const f = twin.instances.find(i => i.instanceId === b.from), t = twin.instances.find(i => i.instanceId === b.to);
        if (!f || !t) return; ctx.strokeStyle = "#e8a816"; ctx.lineWidth = 2; ctx.setLineDash([6, 4]);
        ctx.beginPath(); ctx.moveTo(f.x * SX, f.y * SY); ctx.lineTo(t.x * SX, t.y * SY); ctx.stroke(); ctx.setLineDash([]); });
      twin.instances.forEach(i => { const [w, d] = dims(i.typeId, i.rotation), x = (i.x - w / 2) * SX, y = (i.y - d / 2) * SY;
        ctx.fillStyle = COLORS[i.typeId] || "#888"; ctx.fillRect(x, y, w * SX, d * SY);
        ctx.strokeStyle = "#0d1626"; ctx.strokeRect(x, y, w * SX, d * SY);
        ctx.fillStyle = "#fff"; ctx.font = "10px Calibri"; ctx.fillText(state.reg[i.typeId]?.name || i.typeId, x + 3, y + 12); });
      if (this.mouse) { const [w, d] = dims(selected, ghostRot), x = (this.mouse.x - w / 2) * SX, y = (this.mouse.y - d / 2) * SY;
        ctx.globalAlpha = .5; ctx.fillStyle = "#1e7a4c"; ctx.fillRect(x, y, w * SX, d * SY); ctx.globalAlpha = 1; }
    },
  },

  // 2) Schematic / blueprint SVG — same data, architectural line-drawing language
  "schematic": {
    label: "Schematic (SVG)", kind: "view",
    mount(pane) { pane.innerHTML = `<div style="width:100%;height:100%;background:#0e2233"></div>`; this.host = pane.firstChild; },
    render(twin) {
      const W = 640, H = 480, SX = W / MM_W, SY = H / MM_H, g = [];
      g.push(`<rect x="6" y="6" width="${W - 12}" height="${H - 12}" fill="none" stroke="#9fc3e8" stroke-width="1.5"/>`);
      for (let gx = 0; gx <= MM_W; gx += 500) g.push(`<line x1="${gx*SX}" y1="0" x2="${gx*SX}" y2="${H}" stroke="#1b3450" stroke-width="0.5"/>`);
      for (let gy = 0; gy <= MM_H; gy += 500) g.push(`<line x1="0" y1="${gy*SY}" x2="${W}" y2="${gy*SY}" stroke="#1b3450" stroke-width="0.5"/>`);
      twin.bindings.forEach(b => { const f = twin.instances.find(i => i.instanceId === b.from), t = twin.instances.find(i => i.instanceId === b.to);
        if (f && t) g.push(`<line x1="${f.x*SX}" y1="${f.y*SY}" x2="${t.x*SX}" y2="${t.y*SY}" stroke="#e8a816" stroke-width="1" stroke-dasharray="5 3"/>`); });
      twin.instances.forEach(i => { const [w, d] = dims(i.typeId, i.rotation), x = (i.x - w/2)*SX, y = (i.y - d/2)*SY, ww = w*SX, hh = d*SY;
        g.push(`<rect x="${x}" y="${y}" width="${ww}" height="${hh}" fill="none" stroke="#cfe6ff" stroke-width="1.2"/>`);
        g.push(`<line x1="${x}" y1="${y}" x2="${x+ww}" y2="${y+hh}" stroke="#3a5f86" stroke-width="0.5"/>`);
        g.push(`<text x="${x+3}" y="${y+11}" fill="#9fc3e8" font-family="Consolas,monospace" font-size="9">${(state.reg[i.typeId]?.name||i.typeId)}</text>`); });
      g.push(`<text x="${W-150}" y="${H-14}" fill="#6f9bc4" font-family="Consolas,monospace" font-size="10">OBSERVATION RM · v${twin.version}</text>`);
      this.host.innerHTML = `<svg viewBox="0 0 ${W} ${H}" width="100%" height="100%" preserveAspectRatio="xMidYMid meet">${g.join("")}</svg>`;
    },
  },

  // 3) Real 3D (Three.js) — footprints extruded, orbiting, same twin
  "3d": {
    label: "3D (Three.js)", kind: "view",
    mount(pane) {
      pane.innerHTML = "";
      if (typeof THREE === "undefined") { pane.innerHTML = `<div style="padding:20px;color:#b3402f;font-size:13px">3D lens needs the Three.js CDN (internet). Other lenses are unaffected.</div>`; this.dead = true; return; }
      const w = pane.clientWidth || 480, h = pane.clientHeight || 360;
      this.scene = new THREE.Scene(); this.scene.background = new THREE.Color(0x0e1a2b);
      this.cam = new THREE.PerspectiveCamera(50, w / h, 0.1, 100);
      this.renderer = new THREE.WebGLRenderer({ antialias: true });
      this.renderer.setSize(w, h); this.renderer.setPixelRatio(window.devicePixelRatio || 1);
      pane.appendChild(this.renderer.domElement);
      this.scene.add(new THREE.AmbientLight(0xffffff, 0.7));
      const dl = new THREE.DirectionalLight(0xffffff, 0.8); dl.position.set(3, 6, 4); this.scene.add(dl);
      const floor = new THREE.Mesh(new THREE.PlaneGeometry(4, 3), new THREE.MeshStandardMaterial({ color: 0x16263f }));
      floor.rotation.x = -Math.PI / 2; this.scene.add(floor);
      this.scene.add(new THREE.GridHelper(4, 8, 0x2a4a6a, 0x1b3450));
      this.group = new THREE.Group(); this.scene.add(this.group);
      this.theta = 0.7; this.phi = 0.9; this.radius = 6; this.drag = false; this.auto = true;
      const el = this.renderer.domElement, self = this;
      el.addEventListener("mousedown", e => { self.drag = true; self.auto = false; self.px = e.clientX; self.py = e.clientY; });
      window.addEventListener("mouseup", () => self.drag = false);
      el.addEventListener("mousemove", e => { if (!self.drag) return;
        self.theta -= (e.clientX - self.px) * 0.01; self.phi = Math.max(0.2, Math.min(1.4, self.phi - (e.clientY - self.py) * 0.01));
        self.px = e.clientX; self.py = e.clientY; });
      const loop = () => { if (self.dead) return; self.raf = requestAnimationFrame(loop);
        if (self.auto) self.theta += 0.003;
        self.cam.position.set(self.radius * Math.sin(self.phi) * Math.cos(self.theta),
          self.radius * Math.cos(self.phi), self.radius * Math.sin(self.phi) * Math.sin(self.theta));
        self.cam.lookAt(0, 0.3, 0); self.renderer.render(self.scene, self.cam); };
      loop();
    },
    render(twin) {
      if (this.dead || !this.group) return;
      while (this.group.children.length) this.group.remove(this.group.children[0]);
      const toX = mm => mm / 1000 - 2, toZ = mm => mm / 1000 - 1.5;
      twin.instances.forEach(i => { const [w, d] = dims(i.typeId, i.rotation), hgt = (HEIGHT[i.typeId] || 600) / 1000;
        const mesh = new THREE.Mesh(new THREE.BoxGeometry(w / 1000, hgt, d / 1000),
          new THREE.MeshStandardMaterial({ color: new THREE.Color(COLORS[i.typeId] || "#888") }));
        mesh.position.set(toX(i.x), hgt / 2, toZ(i.y)); this.group.add(mesh); });
      twin.bindings.forEach(b => { const f = twin.instances.find(i => i.instanceId === b.from), t = twin.instances.find(i => i.instanceId === b.to);
        if (!f || !t) return; const geo = new THREE.BufferGeometry().setFromPoints([
          new THREE.Vector3(toX(f.x), 0.4, toZ(f.y)), new THREE.Vector3(toX(t.x), 0.4, toZ(t.y))]);
        this.group.add(new THREE.Line(geo, new THREE.LineBasicMaterial({ color: 0xe8a816 }))); });
    },
  },

  // 4) Data table — the twin is just data
  "data": {
    label: "Twin Data", kind: "view",
    mount(pane) { pane.innerHTML = `<div style="width:100%;height:100%;overflow:auto;background:#fff"></div>`; this.host = pane.firstChild; },
    render(twin) {
      const rows = twin.instances.map(i => `<tr><td class="m">${i.instanceId}</td><td>${i.typeId}</td><td class="m">${Math.round(i.x)},${Math.round(i.y)}</td><td class="m">${i.rotation}°</td></tr>`).join("");
      const binds = twin.bindings.map(b => `<tr><td class="m">${b.kind}</td><td class="m">${b.from}</td><td class="m">${b.to}</td></tr>`).join("") || `<tr><td colspan="3" class="muted">none</td></tr>`;
      this.host.innerHTML = `<div style="padding:8px;font-family:Calibri">
        <div style="font-size:12px;color:#5b6b7e">room version <b style="color:#16263f">${twin.version}</b> · ${twin.instances.length} instances · ${twin.bindings.length} edges</div>
        <table style="width:100%;border-collapse:collapse;font-size:11px;margin-top:6px"><tr><th>instance</th><th>type</th><th>x,y</th><th>rot</th></tr>${rows}</table>
        <div style="font-size:11px;color:#5b6b7e;margin-top:8px">earned edges (med-gas):</div>
        <table style="width:100%;border-collapse:collapse;font-size:11px"><tr><th>kind</th><th>from</th><th>to</th></tr>${binds}</table></div>`;
    },
  },

  // 5) Unity lens — Modutecture's OWN engine (WebGL build) over the SAME twin.
  // Unity can't mount like a canvas; it's a separate WebGL app embedded via iframe.
  // Render() is a no-op here: Unity polls the twin itself (TwinClient.cs) and the page
  // pushes room config + listens for its intents (see the message bridge in INIT).
  "unity": {
    label: "Unity (Moducule Builder)", kind: "view",
    mount(pane) {
      pane.innerHTML =
        `<div style="position:absolute;inset:0;display:flex;flex-direction:column;background:#0e1a2b">
           <iframe id="unityFrame" title="Unity Moducule lens" style="flex:1;border:0;width:100%;height:100%;background:#0e1a2b"></iframe>
           <div id="unityNote" style="font-size:11px;color:#c9d6e8;padding:5px 9px;background:#13243b;border-top:1px solid #22344f">
             loading Unity WebGL lens… (build to <span style="font-family:Consolas,monospace">/wwwroot/unity/</span>)
           </div>
         </div>`;
      const frame = pane.querySelector("#unityFrame");
      const note = pane.querySelector("#unityNote");
      this.frame = frame;
      // Probe the build's presence so we show a helpful message instead of a blank frame.
      fetch(UNITY_URL, { method: "HEAD" }).then(r => {
        if (r.ok) {
          frame.src = UNITY_URL;
          note.innerHTML = `Unity lens — same twin, room <b style="color:#e8a816">${ROOM}</b>. Click in Unity to place; every lens updates together.`;
          // Push room + backend origin once the build has booted.
          frame.addEventListener("load", () => pushUnityConfig());
        } else { throw new Error("no build"); }
      }).catch(() => {
        frame.style.display = "none";
        note.innerHTML =
        `<b style="color:#e8a816">Unity lens ready to wire.</b> Build the project in ` +
        `<span style="font-family:Consolas,monospace">/unity-moducule-lens/</span> to WebGL (menu ` +
        `<span style="font-family:Consolas,monospace">Modutecture → Build WebGL into Spine wwwroot</span>), ` +
        `or pass <span style="font-family:Consolas,monospace">?unity=&lt;url&gt;</span>. ` +
          `The Unity 6.1 client already targets this spine and the real twin schema.`;
      });
    },
    render(_twin) { /* Unity pulls the twin itself; nothing to push per-frame here. */ },
  },
};

// Push current room + backend origin into the Unity WebGL build (TwinClient.Configure).
// Spec is 3-part: httpUrl|wsUrl|room (matches TwinClient.Configure in Unity 6.1).
function pushUnityConfig() {
  const frame = document.getElementById("unityFrame");
  if (!frame || !frame.contentWindow) return;
  const http = `${location.origin}/graphql`;
  const ws = `${location.protocol === "https:" ? "wss" : "ws"}://${location.host}/graphql`;
  const spec = `${http}|${ws}|${ROOM}`;
  try { frame.contentWindow.postMessage({ source: "modutecture-host", type: "configure", spec }, "*"); } catch (e) {}
}

let activeLayout = ["2d", "3d"]; // default split

function buildGrid() {
  const grid = document.getElementById("grid");
  grid.innerHTML = "";
  grid.style.gridTemplateColumns = activeLayout.length >= 3 ? "1fr 1fr" : `repeat(${activeLayout.length}, 1fr)`;
  activeLayout.forEach(id => {
    const pane = document.createElement("div"); pane.className = "pane";
    pane.innerHTML = `<div class="paneHead">${lenses[id].label}</div><div class="paneBody"></div>`;
    grid.appendChild(pane);
    const body = pane.querySelector(".paneBody");
    lenses[id]._body = body;
    lenses[id].mount(body);
  });
  renderAll();
}
function renderAll() { activeLayout.forEach(id => { try { lenses[id].render(state.twin); } catch (e) { console.warn(id, e); } }); }

/* ----------------------------------------------------------- COMMANDS ------- */
async function place(typeId, x, y, rotation) {
  try {
    const d = await gql(`mutation($r:String!,$c:PlaceInput!,$v:Int){ placeModucule(room:$r,cmd:$c,expectedVersion:$v){ status version rebased violations{rule severity message} event{seq} } }`,
      { r: ROOM, c: { typeId, x, y, rotation }, v: state.twin.version });
    setStatus(d.placeModucule); await refresh();
  } catch (e) { console.error(e); }
}
async function agentSuggest() {
  const d = await gql(`mutation($r:String!){ agentSuggest(room:$r,goal:"observation room"){ proposal{typeId x y rotation} rationale citations } }`, { r: ROOM });
  const a = d.agentSuggest;
  if (!a.proposal) { document.getElementById("status").innerHTML = `<span class="warn">agent: ${a.rationale}</span>`; return; }
  if (confirm(`${a.rationale}\n\nCites: ${a.citations.join(", ")}\n\nApprove & commit?`))
    await place(a.proposal.typeId, a.proposal.x, a.proposal.y, a.proposal.rotation);
}
function setStatus(res) {
  const s = document.getElementById("status");
  if (res.status === "ACCEPTED") { const w = (res.violations || []).filter(v => v.severity === "WARNING");
    s.innerHTML = `<span class="ok">✓ COMMITTED</span> seq #${res.event.seq} → v${res.version}${res.rebased ? ' <span class="warn">(rebased)</span>' : ''}${w.length ? ` <span class="warn">⚠ ${w.map(x => x.message).join("; ")}</span>` : ''}`; }
  else s.innerHTML = `<span class="err">✗ REJECTED — nothing written.</span> ${(res.violations || []).map(v => `[${v.rule}] ${v.message}`).join("  ")}`;
}

/* ------------------------------------------------------- SYNC: poll + push -- */
async function refresh() {
  try {
    const d = await gql(`query($r:String!){ twin(room:$r){ version instances{instanceId typeId x y rotation} bindings{kind from to} } }`, { r: ROOM });
    state.twin = d.twin; document.getElementById("ver").textContent = d.twin.version; renderAll();
  } catch (e) { /* keep last good */ }
}
function connectSubscription() {
  try {
    const proto = location.protocol === "https:" ? "wss" : "ws";
    const ws = new WebSocket(`${proto}://${location.host}/graphql`, "graphql-transport-ws");
    ws.onopen = () => ws.send(JSON.stringify({ type: "connection_init" }));
    ws.onmessage = ev => {
      const m = JSON.parse(ev.data);
      if (m.type === "connection_ack")
        ws.send(JSON.stringify({ id: "1", type: "subscribe", payload: { query: `subscription($r:String!){ onTwinChanged(room:$r){ version instances{instanceId typeId x y rotation} bindings{kind from to} } }`, variables: { r: ROOM } } }));
      else if (m.type === "next" && m.payload?.data?.onTwinChanged) {
        state.twin = m.payload.data.onTwinChanged; deltas++; live = true; markSync();
        document.getElementById("ver").textContent = state.twin.version; renderAll();
      } else if (m.type === "ping") ws.send(JSON.stringify({ type: "pong" }));
    };
    ws.onclose = () => { live = false; markSync(); setTimeout(connectSubscription, 3000); };
    ws.onerror = () => {};
  } catch (e) { /* polling carries the demo */ }
}
function markSync() {
  document.getElementById("sync").innerHTML = live
    ? `<span class="live">● LIVE (push)</span> · Δ ${deltas}`
    : `<span class="poll">● polling</span> · Δ ${deltas}`;
}

// Listen for PLACE intents emitted by the Unity WebGL lens (ModuBridge.jslib ->
// window.parent.postMessage). When Unity commits, refresh so EVERY lens updates together.
// Also handles the Unity 'ready' signal: push config the moment the instance boots
// (the iframe 'load' event fires before Unity is interactive).
function connectUnityBridge() {
  window.addEventListener("message", ev => {
    const m = ev.data;
    if (!m || m.source !== "modutecture-unity") return;
    if (m.type === "ready") { pushUnityConfig(); return; }
    if (m.type === "intent") {
      const p = m.payload || {};
      if (p.status === "ACCEPTED") {
        deltas++;
        document.getElementById("status").innerHTML =
          `<span class="ok">✓ COMMITTED</span> via <b>Unity lens</b> seq #${p.seq} → v${p.version}`;
      } else if (p.status === "REJECTED") {
        document.getElementById("status").innerHTML =
          `<span class="err">✗ REJECTED</span> Unity placement — gate blocked it.`;
      }
      refresh();   // pull authoritative twin; all web lenses re-render
    }
  });
}

/* ------------------------------------------------------------- INIT --------- */
function bindUI() {
  document.querySelectorAll("[data-type]").forEach(b => b.onclick = () => {
    document.querySelectorAll("[data-type]").forEach(x => x.classList.remove("sel")); b.classList.add("sel"); selected = b.dataset.type; });
  document.getElementById("rot").onclick = () => { ghostRot = (ghostRot + 90) % 360; document.getElementById("rot").textContent = `Rotate: ${ghostRot}°`; };
  document.getElementById("agent").onclick = agentSuggest;
  document.querySelectorAll("[data-lens]").forEach(cb => cb.onchange = () => {
    activeLayout = [...document.querySelectorAll("[data-lens]:checked")].map(x => x.dataset.lens);
    if (activeLayout.length === 0) { cb.checked = true; activeLayout = [cb.dataset.lens]; }
    buildGrid();
  });
}
async function boot() {
  bindUI();
  try { const d = await gql(`{ registry{ typeId name footprintW footprintD } }`); d.registry.forEach(m => state.reg[m.typeId] = m); } catch (e) { }
  document.querySelector('[data-type="headwall-hw204"]').classList.add("sel");
  buildGrid();
  await refresh();
  connectSubscription();
  connectUnityBridge();
  markSync();
  setInterval(refresh, 800);       // polling baseline (always on)
}
window.addEventListener("resize", () => buildGrid());
boot();
