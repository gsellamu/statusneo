// RoomRenderer.cs + IntentEmitter.cs — the pure view layer.
//
// RoomRenderer rebuilds the scene from each twin payload: it maps typeId -> prefab,
// positions instances (mm -> Unity metres), and draws med-gas bindings as lines.
// It keeps NO authoritative state — every Render() is a fresh projection of server
// truth. This is what "Unity becomes the GPU" means concretely.

using System.Collections.Generic;
using UnityEngine;

public class RoomRenderer : MonoBehaviour
{
    [System.Serializable] public struct PrefabMap { public string typeId; public GameObject prefab; }
    public PrefabMap[] prefabs;               // assign HW-204 / ICU-bed / sink prefabs
    public Material bindingMat;               // gold line for med-gas edges
    public float mmToM = 0.001f;              // 1000 mm = 1 m

    private readonly Dictionary<string, GameObject> _spawned = new();
    private readonly List<LineRenderer> _edges = new();

    GameObject PrefabFor(string typeId)
    {
        foreach (var p in prefabs) if (p.typeId == typeId) return p.prefab;
        return null;
    }

    public void Render(Twin twin)
    {
        // reconcile instances (idempotent: spawn new, move existing, despawn gone)
        var seen = new HashSet<string>();
        foreach (var i in twin.instances)
        {
            seen.Add(i.instanceId);
            if (!_spawned.TryGetValue(i.instanceId, out var go))
            {
                var prefab = PrefabFor(i.typeId);
                if (prefab == null) continue;
                go = Instantiate(prefab, transform);
                _spawned[i.instanceId] = go;
            }
            go.transform.localPosition = new Vector3(i.x * mmToM, 0, i.y * mmToM);
            go.transform.localRotation = Quaternion.Euler(0, i.rotation, 0);
        }
        foreach (var id in new List<string>(_spawned.Keys))
            if (!seen.Contains(id)) { Destroy(_spawned[id]); _spawned.Remove(id); }

        DrawBindings(twin);                   // the earned edges, visualised
    }

    void DrawBindings(Twin twin)
    {
        foreach (var e in _edges) Destroy(e.gameObject);
        _edges.Clear();
        foreach (var b in twin.bindings)
        {
            if (!_spawned.TryGetValue(b.from, out var a) || !_spawned.TryGetValue(b.to, out var c)) continue;
            var lr = new GameObject($"edge:{b.kind}").AddComponent<LineRenderer>();
            lr.transform.SetParent(transform);
            lr.material = bindingMat; lr.widthMultiplier = 0.05f; lr.positionCount = 2;
            lr.SetPositions(new[] { a.transform.localPosition, c.transform.localPosition });
            _edges.Add(lr);
        }
    }
}

// IntentEmitter.cs — turns a click on the floor into a PLACE intent. It does not
// place anything itself; it asks the server and lets the next payload render it.
namespace Spine.Unity
{
    using UnityEngine;

    public class IntentEmitter : MonoBehaviour
    {
        public TwinClient client;
        public string selectedTypeId = "bed-icu";
        public int rotation = 0;
        public Transform floor;            // a plane sized to the room
        public float mToMm = 1000f;

        void Update()
        {
            if (!Input.GetMouseButtonDown(0)) return;
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out var hit) || hit.transform != floor) return;

            float x = hit.point.x * mToMm, y = hit.point.z * mToMm;     // metres -> mm
            StartCoroutine(client.PlaceModucule(selectedTypeId, x, y, rotation, res =>
            {
                if (res.status == "REJECTED")
                    Debug.LogWarning("[intent] REJECTED: " +
                        string.Join("; ", res.violations.ConvertAll(v => $"[{v.rule}] {v.message}")));
                else
                    Debug.Log($"[intent] COMMITTED seq {res.@event.seq}");
            }));
        }
    }
}
