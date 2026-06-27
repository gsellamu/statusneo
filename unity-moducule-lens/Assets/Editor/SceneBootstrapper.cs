// SceneBootstrapper.cs — generates the Moducule lens scene PROGRAMMATICALLY so no
// fragile hand-authored .unity / .prefab YAML can break. Run once from the menu:
//   Modutecture > Build Moducule Lens Scene
// It creates: floor, camera, lighting, 3 procedural Moducule prefabs (color-coded to
// match the web lenses), a Room root, and wires TwinClient + RoomRenderer + IntentEmitter.
// Re-runnable: it rebuilds cleanly. Unity authors all GUIDs/fileIDs itself.

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Modutecture.Lens.EditorTools
{
    public static class SceneBootstrapper
    {
        const string ScenesDir   = "Assets/Scenes";
        const string PrefabsDir  = "Assets/Prefabs";
        const string MatsDir     = "Assets/Materials";
        const string ScenePath   = ScenesDir + "/ModuculeLens.unity";

        // Colors mirror the web lenses (COLORS map in studio.js).
        static readonly Color HeadwallCol = new Color(0.137f, 0.216f, 0.353f); // #23375a
        static readonly Color BedCol      = new Color(0.231f, 0.431f, 0.647f); // #3b6ea5
        static readonly Color SinkCol     = new Color(0.420f, 0.561f, 0.710f); // #6b8fb5
        static readonly Color GoldCol     = new Color(0.910f, 0.659f, 0.086f); // #e8a816

        [MenuItem("Modutecture/Build Moducule Lens Scene")]
        public static void Build()
        {
            EnsureDir(ScenesDir); EnsureDir(PrefabsDir); EnsureDir(MatsDir);

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // --- lighting ---
            var sun = new GameObject("Directional Light");
            var light = sun.AddComponent<Light>();
            light.type = LightType.Directional; light.intensity = 1.0f; light.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            RenderSettings.ambientLight = new Color(0.55f, 0.6f, 0.68f);

            // --- camera ---
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.055f, 0.105f, 0.17f);   // #0e1a2b, matches lens pane
            camGo.transform.position = new Vector3(2f, 4.2f, -2.4f);
            camGo.transform.rotation = Quaternion.Euler(48f, 0f, 0f);
            camGo.AddComponent<OrbitCamera>();

            // --- room root + floor ---
            var room = new GameObject("Room").transform;
            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Floor";
            floor.transform.SetParent(room);
            // Unity plane is 10x10 m at scale 1; room is 4x3 m -> scale 0.4 x 0.3, centered.
            floor.transform.localScale = new Vector3(0.4f, 1f, 0.3f);
            floor.transform.localPosition = new Vector3(2f, 0f, 1.5f);
            floor.GetComponent<Renderer>().sharedMaterial = MakeMat("Floor", new Color(0.086f, 0.149f, 0.247f));

            // --- procedural Moducule prefabs ---
            var headwall = MakePrefab("headwall-hw204", HeadwallCol, new Vector3(1.2f, 1.2f, 0.3f));
            var bed      = MakePrefab("bed-icu",        BedCol,      new Vector3(0.9f, 0.6f, 2.0f));
            var sink     = MakePrefab("sink-clinical",  SinkCol,     new Vector3(0.6f, 0.9f, 0.6f));

            var bindingMat = MakeMat("Binding", GoldCol);

            // --- renderer ---
            var rendererGo = new GameObject("RoomRenderer");
            rendererGo.transform.SetParent(room);
            var rr = rendererGo.AddComponent<RoomRenderer>();
            rr.parent = room;
            rr.bindingMaterial = bindingMat;
            rr.prefabs = new[]
            {
                new RoomRenderer.PrefabMap { typeId = "headwall-hw204", prefab = headwall },
                new RoomRenderer.PrefabMap { typeId = "bed-icu",        prefab = bed },
                new RoomRenderer.PrefabMap { typeId = "sink-clinical",  prefab = sink },
            };

            // --- client + intent emitter ---
            var clientGo = new GameObject("TwinClient");
            var client = clientGo.AddComponent<TwinClient>();
            client.roomRenderer = rr;

            var emitter = clientGo.AddComponent<IntentEmitter>();
            emitter.client = client;
            emitter.floor = floor.transform;
            emitter.cam = cam;

            // ensure the floor has a collider for raycasts (primitive plane already does)
            if (floor.GetComponent<Collider>() == null) floor.gameObject.AddComponent<MeshCollider>();

            EditorSceneManager.SaveScene(scene, ScenePath);
            AddSceneToBuildSettings(ScenePath);
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("Modutecture",
                "Moducule Lens scene built.\n\nPress Play (native) or File > Build Settings > WebGL.\nMake sure the spine is running on http://localhost:5005.",
                "OK");
            Debug.Log("[Bootstrapper] Scene built at " + ScenePath);
        }

        // ---- helpers ----
        static GameObject MakePrefab(string typeId, Color col, Vector3 sizeMeters)
        {
            // Parent root holds the pivot at floor level; the cube mesh is a child
            // lifted by half its height so the prefab sits ON the floor (pivot at base).
            var rootGo = new GameObject(typeId);
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = typeId + "_mesh";
            go.transform.SetParent(rootGo.transform, worldPositionStays: false);
            go.transform.localScale = sizeMeters;
            go.transform.localPosition = new Vector3(0f, sizeMeters.y / 2f, 0f);  // set AFTER parenting
            go.GetComponent<Renderer>().sharedMaterial = MakeMat(typeId, col);

            string path = $"{PrefabsDir}/{typeId}.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(rootGo, path);
            Object.DestroyImmediate(rootGo);
            return prefab;
        }

        static Material MakeMat(string name, Color col)
        {
            // URP/Built-in agnostic: use the project's default shader.
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var mat = new Material(shader) { name = name };
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", col);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", col);
            string path = $"{MatsDir}/{name}.mat";
            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }

        static void EnsureDir(string dir)
        {
            if (!AssetDatabase.IsValidFolder(dir))
            {
                var parent = Path.GetDirectoryName(dir).Replace("\\", "/");
                var leaf = Path.GetFileName(dir);
                if (!AssetDatabase.IsValidFolder(parent)) EnsureDir(parent);
                AssetDatabase.CreateFolder(parent, leaf);
            }
        }

        static void AddSceneToBuildSettings(string path)
        {
            var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            if (!scenes.Exists(s => s.path == path))
                scenes.Insert(0, new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
#endif
