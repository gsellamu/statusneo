// RoomRenderer.cs — the pure view layer. Rebuilds the scene from each twin payload.
// Maps typeId -> prefab, mm -> metres, draws med-gas bindings as gold lines.
// Keeps NO authoritative state; every Render() is a fresh projection of server truth.

using System.Collections.Generic;
using UnityEngine;

namespace Modutecture.Lens
{
    public class RoomRenderer : MonoBehaviour
    {
        [System.Serializable] public struct PrefabMap { public string typeId; public GameObject prefab; }

        public PrefabMap[] prefabs;              // headwall-hw204 / bed-icu / sink-clinical
        public Material bindingMaterial;         // gold line for med-gas edges
        public float mmToM = 0.001f;             // 1000 mm = 1 m
        public Transform parent;                 // container for spawned instances

        private readonly Dictionary<string, GameObject> _spawned = new();
        private readonly List<GameObject> _edges = new();

        GameObject PrefabFor(string typeId)
        {
            foreach (var p in prefabs) if (p.typeId == typeId) return p.prefab;
            return null;
        }

        public void Render(Twin twin)
        {
            if (twin == null) return;
            var root = parent != null ? parent : transform;

            var seen = new HashSet<string>();
            foreach (var i in twin.instances)
            {
                seen.Add(i.instanceId);
                if (!_spawned.TryGetValue(i.instanceId, out var go) || go == null)
                {
                    var prefab = PrefabFor(i.typeId);
                    if (prefab == null) continue;
                    go = Instantiate(prefab, root);
                    go.name = $"{i.typeId}:{i.instanceId}";
                    _spawned[i.instanceId] = go;
                }
                go.transform.localPosition = new Vector3(i.x * mmToM, 0f, i.y * mmToM);
                go.transform.localRotation = Quaternion.Euler(0f, i.rotation, 0f);
            }

            // despawn instances no longer in the payload
            var gone = new List<string>();
            foreach (var kv in _spawned) if (!seen.Contains(kv.Key)) gone.Add(kv.Key);
            foreach (var id in gone) { if (_spawned[id] != null) Destroy(_spawned[id]); _spawned.Remove(id); }

            DrawBindings(twin, root);
        }

        void DrawBindings(Twin twin, Transform root)
        {
            foreach (var e in _edges) if (e != null) Destroy(e);
            _edges.Clear();
            foreach (var b in twin.bindings)
            {
                if (!_spawned.TryGetValue(b.from, out var a) || a == null) continue;
                if (!_spawned.TryGetValue(b.to, out var c) || c == null) continue;
                var go = new GameObject($"edge:{b.kind}");
                go.transform.SetParent(root);
                var lr = go.AddComponent<LineRenderer>();
                lr.material = bindingMaterial;
                lr.widthMultiplier = 0.05f;
                lr.positionCount = 2;
                lr.useWorldSpace = false;
                lr.SetPositions(new[] {
                    a.transform.localPosition + Vector3.up * 0.4f,
                    c.transform.localPosition + Vector3.up * 0.4f
                });
                _edges.Add(go);
            }
        }
    }
}
