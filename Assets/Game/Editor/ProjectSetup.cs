using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace FrostMaze.Editor
{
    [InitializeOnLoad]
    public static class ProjectSetup
    {
        static ProjectSetup()
        {
            EditorApplication.delayCall += EnsureAssets;
        }
        [MenuItem("FrostMaze/Prepare prototype assets")]
        public static void EnsureAssets()
        {
            if (EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode)
                return;
            Directory.CreateDirectory("Assets/Game/Maps/Resources");
            Directory.CreateDirectory("Assets/Game/Settings");
            var map = AssetDatabase.LoadAssetAtPath<MapDefinition>("Assets/Game/Maps/Resources/TestMap.asset");
            if (map == null)
            {
                map = ScriptableObject.CreateInstance<MapDefinition>();
                AssetDatabase.CreateAsset(map, "Assets/Game/Maps/Resources/TestMap.asset");
            }
            if (GraphicsSettings.defaultRenderPipeline == null)
            {
                var renderer = ScriptableObject.CreateInstance<UniversalRendererData>();
                AssetDatabase.CreateAsset(renderer, "Assets/Game/Settings/MazeRenderer.asset");
                var pipeline = UniversalRenderPipelineAsset.Create(renderer);
                AssetDatabase.CreateAsset(pipeline, "Assets/Game/Settings/MazePipeline.asset");
                GraphicsSettings.defaultRenderPipeline = pipeline;
                QualitySettings.renderPipeline = pipeline;
            }
            EnsureMaterial("PrototypeMaterial", "Universal Render Pipeline/Lit");
            EnsureMaterial("DebugMaterial", "Universal Render Pipeline/Unlit");
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene("Assets/Game/Maps/MazeLab.unity", true) };
            PlayerSettings.companyName = "FrostMaze Lab";
            PlayerSettings.productName = "FrostMaze";
            PlayerSettings.defaultScreenWidth = 1440;
            PlayerSettings.defaultScreenHeight = 900;
            AssetDatabase.SaveAssets();
        }
        static void EnsureMaterial(string name, string shaderName)
        {
            string path = "Assets/Game/Maps/Resources/" + name + ".mat";
            if (AssetDatabase.LoadAssetAtPath<Material>(path) == null)
            {
                var shader = Shader.Find(shaderName);
                if (shader == null)
                    throw new System.Exception("Missing shader: " + shaderName);
                AssetDatabase.CreateAsset(new Material(shader), path);
            }
        }
        [MenuItem("FrostMaze/Open maze lab")]
        public static void OpenScene()
        {
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Game/Maps/MazeLab.unity");
        }
        [MenuItem("FrostMaze/Select map parameters")]
        public static void SelectMap()
        {
            EnsureAssets();
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<MapDefinition>("Assets/Game/Maps/Resources/TestMap.asset");
        }
        [MenuItem("FrostMaze/Build Linux")]
        public static void BuildLinux()
        {
            Build(BuildTarget.StandaloneLinux64, "Builds/Linux/FrostMaze");
        }
        [MenuItem("FrostMaze/Build Windows")]
        public static void BuildWindows()
        {
            Build(BuildTarget.StandaloneWindows64, "Builds/Windows/FrostMaze.exe");
        }
        static void Build(BuildTarget target, string path)
        {
            EnsureAssets();
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = new[] { "Assets/Game/Maps/MazeLab.unity" }, locationPathName = path, target = target, options = BuildOptions.None });
            if (report.summary.result != BuildResult.Succeeded)
                throw new System.Exception("Build failed: " + report.summary.result);
        }
    }
}
