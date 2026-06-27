// IntentEmitter.cs — turns a click on the floor into a PLACE intent. It does NOT
// place anything itself; it asks the server and the next twin payload renders it.
// Full place capability (click-to-place), matching index.html. Unity 6.1 async.

using UnityEngine;

namespace Modutecture.Lens
{
    public class IntentEmitter : MonoBehaviour
    {
        public TwinClient client;
        public Transform floor;                 // plane sized to the room
        public Camera cam;                       // defaults to Camera.main
        public string selectedTypeId = "bed-icu";
        public int rotation = 0;
        public float mToMm = 1000f;

        void Awake() { if (cam == null) cam = Camera.main; }

        async void Update()
        {
            if (client == null || floor == null) return;
            if (!Input.GetMouseButtonDown(0)) return;

            var ray = (cam != null ? cam : Camera.main).ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out var hit) || hit.transform != floor) return;

            // hit point (world) -> room-local mm. Floor is a child of the room root.
            Vector3 local = floor.InverseTransformPoint(hit.point);
            float x = local.x * mToMm;
            float y = local.z * mToMm;

            var res = await client.PlaceModucule(selectedTypeId, x, y, rotation);
            if (res == null) { Debug.LogWarning("[intent] no response"); return; }
            if (res.status == "REJECTED")
                Debug.LogWarning("[intent] REJECTED: " +
                    string.Join("; ", res.violations.ConvertAll(v => $"[{v.rule}] {v.message}")));
            else
                Debug.Log($"[intent] COMMITTED seq {res.@event?.seq} -> v{res.version}");
        }
    }
}
