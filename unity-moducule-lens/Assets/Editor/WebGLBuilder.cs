// WebGLBuilder.cs — one-click WebGL build straight into the spine's wwwroot/unity/
// so studio.html's Unity lens picks it up at http://localhost:5005/unity/index.html.
//   Modutecture > Build WebGL into Spine wwwroot
// Sets the ModuLens template (which relays host->Unity config messages) if present.

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Modutecture.Lens.EditorTools
{
    public static class WebGLBuilder
    {
        [MenuItem("Modutecture/Build WebGL into Spine wwwroot")]
        public static void Build()
        {
            // project lives at .../unity-moducule-lens; spine wwwroot is a sibling.
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;          // ...\unity-moducule-lens
            string repoRoot = Directory.GetParent(projectRoot).FullName;                      // ...\modutecture-spine-fullstack
            string outDir = Path.Combine(repoRoot, "twin-service-dotnet", "wwwroot", "unity");

            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.WebGL)
            {
                if (!EditorUtility.DisplayDialog("Switch to WebGL?",
                    "Active build target is not WebGL. Switch now?\n(One-time; can take a minute.)", "Switch", "Cancel"))
                    return;
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);
            }

            // Use our custom template if installed (relays host->Unity 'configure' messages).
            if (Directory.Exists(Path.Combine(Application.dataPath, "WebGLTemplates", "ModuLens")))
                PlayerSettings.WebGL.template = "PROJECT:ModuLens";
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled; // simplest local serve

            var scenes = EditorBuildSettings.scenes;
            if (scenes.Length == 0)
            {
                EditorUtility.DisplayDialog("Modutecture",
                    "No scenes in Build Settings. Run 'Build Moducule Lens Scene' first.", "OK");
                return;
            }

            Directory.CreateDirectory(outDir);
            var opts = new BuildPlayerOptions
            {
                scenes = System.Array.ConvertAll(scenes, s => s.path),
                locationPathName = outDir,
                target = BuildTarget.WebGL,
                options = BuildOptions.None,
            };
            var report = BuildPipeline.BuildPlayer(opts);
            var sum = report.summary;
            if (sum.result == BuildResult.Succeeded)
            {
                Debug.Log($"[WebGLBuilder] OK -> {outDir} ({sum.totalSize} bytes). Open http://localhost:5005/studio.html and tick the Unity lens.");
                EditorUtility.RevealInFinder(outDir);
            }
            else
                Debug.LogError($"[WebGLBuilder] build {sum.result}: {sum.totalErrors} errors");
        }
    }
}
#endif
