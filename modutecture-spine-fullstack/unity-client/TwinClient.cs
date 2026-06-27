// TwinClient.cs — Unity thin client (the GPU, not the source of truth).
//
// THIN-UNITY THESIS, in code: this client holds NO domain state. It queries the
// twin, renders the payload, and emits intents (commands). The server validates
// and owns truth. Swapping this for the Angular/glTF viewer changes nothing
// server-side — both consume the identical GraphQL contract (graphql/schema.graphql).
//
// Reviewed for idiom; runs in a Unity 2022+ project (needs a scene + 3 prefabs).
// Uses UnityWebRequest + JsonUtility (no external deps). For production, swap to
// a GraphQL-over-WebSocket client for subscriptions; here we poll + push on intent.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TwinClient : MonoBehaviour
{
    [Header("Backend")]
    public string graphqlUrl = "http://localhost:8099/graphql";
    public string room = "r1";
    public float pollSeconds = 1.0f;          // stand-in for the GraphQL subscription

    [Header("Renderer (assign in inspector)")]
    public RoomRenderer renderer;             // pure view — instantiates prefabs by typeId

    void Start() => StartCoroutine(PollLoop());

    IEnumerator PollLoop()
    {
        while (true)
        {
            yield return Query(
                "{ twin(room:\\\"" + room + "\\\"){ " +
                "room{x0 y0 x1 y1} instances{instanceId typeId x y rotation} bindings{kind from to} } }",
                json =>
                {
                    var resp = JsonUtility.FromJson<TwinResponse>(json);
                    if (resp?.data?.twin != null) renderer.Render(resp.data.twin);   // render server truth
                });
            yield return new WaitForSeconds(pollSeconds);
        }
    }

    // INTENT, not mutation: the client asks the server to place; it never edits
    // its own scene as if that were truth. The next twin payload is the truth.
    public IEnumerator PlaceModucule(string typeId, float x, float y, int rotation,
                                     Action<CommandResult> done)
    {
        string m =
            "mutation{ placeModucule(room:\\\"" + room + "\\\", cmd:{typeId:\\\"" + typeId +
            "\\\", x:" + x + ", y:" + y + ", rotation:" + rotation + "}){ " +
            "status violations{rule severity message} event{seq} } }";
        yield return Query(m, json =>
        {
            var r = JsonUtility.FromJson<PlaceResponse>(json);
            done?.Invoke(r.data.placeModucule);
            // No local mutation on ACCEPTED — PollLoop pulls the authoritative twin.
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
        if (req.result == UnityWebRequest.Result.Success) onOk(req.downloadHandler.text);
        else Debug.LogError($"[TwinClient] {req.error}");
    }
}

// --- DTOs mirroring the GraphQL payload (the only contract the client knows) -
[Serializable] public class TwinResponse { public TwinData data; }
[Serializable] public class TwinData { public Twin twin; }
[Serializable] public class Twin { public Room room; public List<Instance> instances; public List<Binding> bindings; }
[Serializable] public class Room { public float x0, y0, x1, y1; }
[Serializable] public class Instance { public string instanceId; public string typeId; public float x, y; public int rotation; }
[Serializable] public class Binding { public string kind, from, to; }

[Serializable] public class PlaceResponse { public PlaceData data; }
[Serializable] public class PlaceData { public CommandResult placeModucule; }
[Serializable] public class CommandResult { public string status; public List<Violation> violations; public EventRef @event; }
[Serializable] public class Violation { public string rule, severity, message; }
[Serializable] public class EventRef { public int seq; }
