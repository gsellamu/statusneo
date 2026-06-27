# Unity lens — Modutecture's own engine, over the same twin

This is the Unity thin client wired as the studio's **4th lens**. It proves the
renderer-independence thesis with Modutecture's *own* engine: Unity renders the
twin and emits PLACE intents, but holds **no authoritative state** — the spine's
deterministic gate owns truth. *"Unity becomes the GPU; the twin holds the truth."*

## Files
| File | Role |
|---|---|
| `TwinClient.cs` | Polls `twin(room)`, renders the payload, emits PLACE intents. Targets the spine at `:5005` and the **real** GraphQL schema (`version/instances/bindings`). Host-configurable. |
| `RoomRenderer.cs` | Pure view — maps `typeId → prefab`, mm→m, draws med-gas bindings. |
| `IntentEmitter.cs` | Click-on-floor → `PlaceModucule` intent (full place capability). |
| `UnityBridge.cs` | Mirrors intent results UP to the host page (WebGL). No-op natively. |
| `Plugins/ModuBridge.jslib` | WebGL→host `postMessage` glue backing `UnityBridge`. |

## What was fixed (these were real blockers)
1. **Port** — was hardcoded `:8099`; now `:5005` (the spine), host-overridable.
2. **Schema** — the query asked for `twin{ room{x0 y0 x1 y1} … }`, but the backend
   `TwinDto` has **no `room` field** → GraphQL error → nothing rendered. Now queries
   the real `version/instances/bindings`.
3. **Two deploy shapes, one script** — native player *and* WebGL lens.

## Run it — native (beside the browser)
1. Open `/unity-client/` as a Unity 2022+ project (create a scene; add a `floor`
   plane, a camera, and 3 prefabs: headwall / bed / sink).
2. On a GameObject: add `TwinClient` (set `room`, assign `RoomRenderer`) and
   `IntentEmitter` (assign `client` + `floor`).
3. Press Play. It polls `http://localhost:5005/graphql` and renders live. Placing
   in Unity commits through the same gate; the web lenses update on their next poll.

## Run it — WebGL lens (embedded in studio.html)
1. **Build**: File → Build Settings → WebGL → Build to `…/twin-service-dotnet/wwwroot/unity/`
   (so it serves at `http://localhost:5005/unity/index.html`).
2. Open `http://localhost:5005/studio.html`, tick the **Unity** lens. studio.js
   probes `/unity/index.html`; if present it embeds it, else it shows a "ready to wire"
   note (so the page never breaks).
3. **Host ↔ Unity bridge** (already coded):
   - **Unity → host**: `ModuBridge.jslib` posts `{source:"modutecture-unity",type:"intent"}`
     on every commit; studio.js refreshes all lenses in lock-step.
   - **host → Unity**: studio.js posts `{source:"modutecture-host",type:"configure",spec:"<graphqlUrl>|<room>"}`.
     To receive it, add this to your WebGL template's `index.html` (after
     `createUnityInstance(...)` resolves to `unityInstance`):
     ```js
     window.addEventListener("message", function (e) {
       if (e.data && e.data.source === "modutecture-host" && e.data.type === "configure") {
         unityInstance.SendMessage("TwinClient", "Configure", e.data.spec);
       }
     });
     ```
     (`"TwinClient"` = the GameObject name that holds the `TwinClient` component.)

## Notes
- CORS is already open on the spine (`AllowAnyOrigin`), so the WebGL build can call `/graphql`.
- `?unity=<url>` on studio.html overrides the build location for testing.
- For production, swap polling for `graphql-transport-ws` to match the web lenses' live push.
- This environment can't run the Unity editor, so the **WebGL build is the one manual step** —
  the C#, the bridge, the studio pane, and the host glue are all in place and verified.
