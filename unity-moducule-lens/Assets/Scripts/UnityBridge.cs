// UnityBridge.cs — one-way bridge from the Unity WebGL lens UP to the host page.
// On a commit, posts a message to the parent window so the studio refreshes its
// OTHER lenses in lock-step (one twin, many lenses). Native player: harmless no-op.

using System.Runtime.InteropServices;
using UnityEngine;

namespace Modutecture.Lens
{
    public static class UnityBridge
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern void ModuNotifyIntent(string json);
#endif

        public static void NotifyIntent(CommandResult res)
        {
            if (res == null) return;
            int seq = res.@event != null ? res.@event.seq : -1;
            string json = "{\"status\":\"" + (res.status ?? "") + "\",\"version\":" + res.version +
                          ",\"seq\":" + seq + "}";
#if UNITY_WEBGL && !UNITY_EDITOR
            ModuNotifyIntent(json);
#else
            Debug.Log("[UnityBridge] intent -> host (native no-op): " + json);
#endif
        }
    }
}
