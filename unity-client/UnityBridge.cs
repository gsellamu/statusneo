// UnityBridge.cs — one-way bridge from the Unity WebGL lens UP to the host page.
//
// When the Unity client commits an intent, the host studio page wants to know so it
// can refresh its OTHER lenses immediately (the whole point: one twin, many lenses,
// all in sync). In a WebGL build this calls a tiny .jslib function that posts a
// message to the parent window. In the native player it's a harmless no-op.
//
// Host page listens with:  window.addEventListener("message", e => { if (e.data?.source === "modutecture-unity") ... })

using System.Runtime.InteropServices;

public static class UnityBridge
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern void ModuNotifyIntent(string json);
#endif

    public static void NotifyIntent(CommandResult res)
    {
        if (res == null) return;
        // minimal, safe JSON — status + version + seq
        int seq = res.@event != null ? res.@event.seq : -1;
        string json = "{\"status\":\"" + (res.status ?? "") + "\",\"version\":" + res.version +
                      ",\"seq\":" + seq + "}";
#if UNITY_WEBGL && !UNITY_EDITOR
        ModuNotifyIntent(json);
#else
        UnityEngine.Debug.Log("[UnityBridge] intent → host (native no-op): " + json);
#endif
    }
}
