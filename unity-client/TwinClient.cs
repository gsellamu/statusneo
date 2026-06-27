// TwinClient.cs — Unity thin client (the GPU, not the source of truth).
//
// THIN-UNITY THESIS, in code: this client holds NO domain state. It queries the
// twin, renders the payload, and emits intents (commands). The server validates
// and owns truth. Swapping this for the Angular/glTF or web lenses changes nothing
// server-side — every client consumes the identical GraphQL contract
// (graphql/schema.graphql). "Unity becomes the GPU; the twin holds the truth."
//
// Runs in a Unity 2022+ project (needs a scene + 3 prefabs + a floor plane).
// Two deployment shapes, ONE script:
//   * Native player  — runs beside the browser, talks to the spine over HTTP.
//   * WebGL build     — embedded as the studio "Unity" lens (iframe). The page can
//                       push room/URL config and request a refresh via SendMessage;
//                       intents are mirrored up to the host page via a jslib bridge.
//
// Uses UnityWebRequest + JsonUtility (no external deps). Polls + refreshes on intent;
// for production swap to graphql-transport-ws to match the web lenses' live push.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TwinClient : MonoBehaviour
{
    [Header("Backend")]
    // Same origin as the spine. When embedded as a WebGL lens this is overridden at
    // runtime by the host page (Configure), so it always matches wherever the spine runs.
    public string graphqlUrl = "http://localhost:5005/graphql";
    public string room = "exam-12";
    public float pollSeconds = 1.0f;          // stand-in for the GraphQL subscription

    [Header("Renderer (assign in inspector)")]
    public RoomRenderer roomRenderer;         // pure view — instantiates prefabs by typeId

    private Coroutine _loop;

    void Start() => Restart();

    // ---- host-page control surface (WebGL): gameObject.SendMessage("Configure", "...") ----
    // Accepts "graphqlUrl|room" (either part optional, e.g. "|icu-101" to change room only).
    public void Configure(string spec)
    {
        if (!string.IsNullOrEmpty(spec))
        {
            var parts = spec.Split('|');
            if (parts.Length > 0 && !string.IsNullOrWhiteSpace(parts[0])) graphqlUrl = parts[0].Trim();
            if (parts.Length > 1 && !string.IsNullOrWhiteSpace(parts[1])) room = parts[1].Trim();
        }
        Restart();
    }

    public void SetRoom(string r) { if (!string.IsNullOrWhiteSpace(r)) { room = r.Trim(); Restart(); } }

    public void RefreshNow() => StartCoroutine(FetchOnce());

    void Restart()
    {
        if (_loop != null) StopCoroutine(_loop);
        _loop = StartCoroutine(PollLoop());
    }

    IEnumerator PollLoop()
    {
        while (true)
        {
            yield return FetchOnce();
            yield return new WaitForSeconds(pollSeconds);
        }
    }

    IEnumerator FetchOnce()
    {
        // NOTE: fields match the REAL backend TwinDto exactly — version/instances/bindings.
        // (No `room{...}` — the backend does not expose it; that was the bug that rendered nothing.)
        yield return Query(
            "{ twin(room:\\\"" + room + "\\\"){ version " +
            "instances{instanceId typeId x y rotation} bindings{kind from to} } }",
            json =>
            {
                var resp = JsonUtility.FromJson<TwinResponse>(json);
                if (resp?.data?.twin != null) roomRenderer.Render(resp.data.twin);   // render server truth
            });
    }

    // INTENT, not local edit: the client asks the server to place; it never edits
    // its own scene as if that were truth. The next twin payload is the truth.
    public IEnumerator PlaceModucule(string typeId, float x, float y, int rotation,
                                     Action<CommandResult> done)
    {
        // expectedVersion omitted here for brevity; the server still validates vs current truth.
        string m =
            "mutation{ placeModucule(room:\\\"" + room + "\\\", cmd:{typeId:\\\"" + typeId +
            "\\\", x:" + Mathf.RoundToInt(x) + ", y:" + Mathf.RoundToInt(y) + ", rotation:" + rotation + "}){ " +
            "status version violations{rule severity message} event{seq} } }";
        yield return Query(m, json =>
        {
            var r = JsonUtility.FromJson<PlaceResponse>(json);
            var res = r?.data?.placeModucule;
            done?.Invoke(res);
            UnityBridge.NotifyIntent(res);     // mirror result to the host page (WebGL); no-op natively
            // No local mutation on ACCEPTED — PollLoop / RefreshNow pulls authoritative truth.
            StartCoroutine(FetchOnce());        // immediate refresh so the placement shows at once
        });
    }

    IEnumerator Query(string gql, Action<string> onOk)
    {
        var body = "{\"query\":\"" + gql + "\"}";
        using var req = new UnityWebRequest(graphqlUrl, "POST");
        req.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(body));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        yield return req.SendWebRequest();
#if UNITY_2020_1_OR_NEWER
        bool ok = req.result == UnityWebRequest.Result.Success;
#else
        bool ok = !req.isNetworkError && !req.isHttpError;
#endif
        if (ok) onOk(req.downloadHandler.text);
        else Debug.LogError($"[TwinClient] {req.error} @ {graphqlUrl}");
    }
}

// --- DTOs mirroring the GraphQL payload (the only contract the client knows) -
// Aligned to the backend: TwinDto{ version, instances[], bindings[] }.
[Serializable] public class TwinResponse { public TwinData data; }
[Serializable] public class TwinData { public Twin twin; }
[Serializable] public class Twin { public int version; public List<Instance> instances; public List<Binding> bindings; }
[Serializable] public class Instance { public string instanceId; public string typeId; public float x, y; public int rotation; }
[Serializable] public class Binding { public string kind, from, to; }

[Serializable] public class PlaceResponse { public PlaceData data; }
[Serializable] public class PlaceData { public CommandResult placeModucule; }
[Serializable] public class CommandResult { public string status; public int version; public List<Violation> violations; public EventRef @event; }
[Serializable] public class Violation { public string rule, severity, message; }
[Serializable] public class EventRef { public int seq; }
