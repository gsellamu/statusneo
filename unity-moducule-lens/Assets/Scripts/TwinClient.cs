// TwinClient.cs — production thin client for the Modutecture twin (Unity 6.1).
//
// THIN-UNITY THESIS: holds NO domain state. Queries the twin, renders the payload,
// emits intents. The spine's deterministic gate owns truth. Swapping this for the
// web lenses changes nothing server-side — identical GraphQL contract.
// "Unity becomes the GPU; the twin holds the truth."
//
// TRANSPORT: WebSocket-primary (graphql-transport-ws subscription) with automatic
// polling fallback — matches the ResilientMind streaming posture. On native the
// ClientWebSocket carries live push; on WebGL (no System.Net.WebSockets) or on any
// socket failure it degrades to UnityWebRequest polling. Either way the twin renders.
//
// Unity 6.1 async: uses Awaitable (main-thread friendly) instead of coroutines.

using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Modutecture.Lens
{
    public class TwinClient : MonoBehaviour
    {
        [Header("Backend (same origin as the spine)")]
        public string httpUrl = "http://localhost:5005/graphql";
        public string wsUrl   = "ws://localhost:5005/graphql";   // graphql-transport-ws
        public string room    = "exam-12";

        [Header("Transport")]
        public bool preferWebSocket = true;
        public float pollSeconds = 1.0f;        // fallback cadence

        [Header("Renderer (assigned by bootstrapper or inspector)")]
        public RoomRenderer roomRenderer;

        public Twin Current { get; private set; } = new();
        public bool Live { get; private set; }       // true when WS push is active
        public event Action<Twin> OnTwinUpdated;
        public event Action<CommandResult> OnIntentResult;

        private ClientWebSocket _ws;
        private CancellationTokenSource _cts;

        async void Start()
        {
            _cts = new CancellationTokenSource();
            await FetchOnce();                       // immediate first paint
            if (preferWebSocket && WebSocketSupported())
                RunWebSocketLoop();                  // fire-and-forget streaming (async void wrapper)
            RunPollLoop();                           // always-on baseline (async void wrapper)
        }

        // async void wrappers so pooled Awaitables aren't discarded with `_ =`,
        // and so OperationCanceledException on shutdown stays contained.
        async void RunPollLoop()
        {
            try { await PollLoop(_cts.Token); }
            catch (OperationCanceledException) { }
        }

        async void RunWebSocketLoop()
        {
            try { await RunWebSocket(_cts.Token); }
            catch (OperationCanceledException) { }
        }

        void OnDestroy() { _cts?.Cancel(); try { _ws?.Dispose(); } catch { } }

        // ---- host control surface (WebGL: gameObject.SendMessage) ----
        // "httpUrl|wsUrl|room" — any part optional.
        public async void Configure(string spec)
        {
            if (!string.IsNullOrEmpty(spec))
            {
                var p = spec.Split('|');
                if (p.Length > 0 && !string.IsNullOrWhiteSpace(p[0])) httpUrl = p[0].Trim();
                if (p.Length > 1 && !string.IsNullOrWhiteSpace(p[1])) wsUrl   = p[1].Trim();
                if (p.Length > 2 && !string.IsNullOrWhiteSpace(p[2])) room    = p[2].Trim();
            }
            await FetchOnce();
        }

        public async void SetRoom(string r)
        {
            if (string.IsNullOrWhiteSpace(r)) return;
            room = r.Trim();
            await FetchOnce();
        }

        // ---------- polling fallback ----------
        async Awaitable PollLoop(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                await Awaitable.WaitForSecondsAsync(pollSeconds, ct);
                if (Live) continue;                  // WS carrying it; skip poll
                await FetchOnce();
            }
        }

        async Awaitable FetchOnce()
        {
            string q = "{ twin(room:\\\"" + room + "\\\"){ version " +
                       "instances{instanceId typeId x y rotation} bindings{kind from to} } }";
            string json = await PostGraphQL(Wrap(q));
            if (string.IsNullOrEmpty(json)) return;
            var resp = JsonUtility.FromJson<TwinResponse>(json);
            if (resp?.data?.twin != null) Apply(resp.data.twin);
        }

        // ---------- WebSocket subscription (graphql-transport-ws) ----------
        bool WebSocketSupported()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return false;   // System.Net.WebSockets unavailable in WebGL; polling carries it
#else
            return true;
#endif
        }

        async Awaitable RunWebSocket(CancellationToken ct)
        {
            try
            {
                _ws = new ClientWebSocket();
                _ws.Options.AddSubProtocol("graphql-transport-ws");
                await _ws.ConnectAsync(new Uri(wsUrl), ct);

                await WsSend("{\"type\":\"connection_init\"}", ct);

                string sub = "{\"id\":\"1\",\"type\":\"subscribe\",\"payload\":{\"query\":\"" +
                    "subscription($r:String!){ onTwinChanged(room:$r){ version " +
                    "instances{instanceId typeId x y rotation} bindings{kind from to} } }\"," +
                    "\"variables\":{\"r\":\"" + room + "\"}}}";

                var buf = new byte[1 << 16];
                while (!ct.IsCancellationRequested && _ws.State == WebSocketState.Open)
                {
                    var seg = new ArraySegment<byte>(buf);
                    var result = await _ws.ReceiveAsync(seg, ct);
                    if (result.MessageType == WebSocketMessageType.Close) break;
                    string msg = Encoding.UTF8.GetString(buf, 0, result.Count);

                    if (msg.Contains("\"connection_ack\""))
                        await WsSend(sub, ct);
                    else if (msg.Contains("\"ping\""))
                        await WsSend("{\"type\":\"pong\"}", ct);
                    else if (msg.Contains("onTwinChanged"))
                    {
                        var t = ExtractOnTwinChanged(msg);
                        if (t != null) { Live = true; Apply(t); }
                    }
                }
            }
            catch (OperationCanceledException) { /* shutting down */ }
            catch (Exception e) { Debug.LogWarning($"[TwinClient] WS fell back to polling: {e.Message}"); }
            finally { Live = false; }
        }

        async Awaitable WsSend(string text, CancellationToken ct)
        {
            var bytes = Encoding.UTF8.GetBytes(text);
            await _ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, ct);
        }

        Twin ExtractOnTwinChanged(string frame)
        {
            // frame: {"type":"next","id":"1","payload":{"data":{"onTwinChanged":{...}}}}
            int i = frame.IndexOf("\"payload\"", StringComparison.Ordinal);
            if (i < 0) return null;
            int b = frame.IndexOf('{', i + 9);
            if (b < 0) return null;
            string payload = frame.Substring(b, frame.Length - b - CountTrailingBraces(frame));
            var wrap = JsonUtility.FromJson<WsNextPayload>(payload);
            return wrap?.data?.onTwinChanged;
        }

        int CountTrailingBraces(string s)
        {
            int n = 0; for (int k = s.Length - 1; k >= 0 && s[k] == '}'; k--) n++;
            return Math.Max(0, n - 1);   // keep payload's own closing brace
        }

        // ---------- INTENT (place) ----------
        public async Awaitable<CommandResult> PlaceModucule(string typeId, float x, float y, int rotation)
        {
            string m = "mutation{ placeModucule(room:\\\"" + room + "\\\", cmd:{typeId:\\\"" + typeId +
                "\\\", x:" + Mathf.RoundToInt(x) + ", y:" + Mathf.RoundToInt(y) + ", rotation:" + rotation +
                "}){ status version violations{rule severity message} event{seq} } }";
            string json = await PostGraphQL(Wrap(m));
            CommandResult res = null;
            if (!string.IsNullOrEmpty(json))
            {
                var r = JsonUtility.FromJson<PlaceResponse>(json);
                res = r?.data?.placeModucule;
            }
            OnIntentResult?.Invoke(res);
            UnityBridge.NotifyIntent(res);           // mirror to host page (WebGL); native no-op
            await FetchOnce();                        // pull authoritative truth at once
            return res;
        }

        // ---------- transport helper ----------
        async Awaitable<string> PostGraphQL(string body)
        {
            using var req = new UnityWebRequest(httpUrl, "POST");
            req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            await req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.Success) return req.downloadHandler.text;
            Debug.LogError($"[TwinClient] {req.error} @ {httpUrl}");
            return null;
        }

        static string Wrap(string gql) => "{\"query\":\"" + gql + "\"}";

        void Apply(Twin t)
        {
            Current = t;
            if (roomRenderer != null) roomRenderer.Render(t);
            OnTwinUpdated?.Invoke(t);
        }
    }
}
