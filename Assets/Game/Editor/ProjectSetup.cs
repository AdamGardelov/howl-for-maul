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
        [MenuItem("Howl for Maul/Prepare prototype assets")]
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
            if (AssetDatabase.LoadAssetAtPath<MapDefinition>("Assets/Game/Maps/Resources/Frostfall.asset") == null)
            {
                var shared = ScriptableObject.CreateInstance<MapDefinition>();
                shared.Settings = FrostMaze.Simulation.Scenario.SharedDefense();
                AssetDatabase.CreateAsset(shared, "Assets/Game/Maps/Resources/Frostfall.asset");
            }
            foreach(string name in new[]{"Rimewatch","Ironfold"}) {
                string path="Assets/Game/Maps/Resources/"+name+".asset";
                if(AssetDatabase.LoadAssetAtPath<MapDefinition>(path)==null) {
                    var asset=ScriptableObject.CreateInstance<MapDefinition>();
                    string text=File.ReadAllText("Assets/Game/Maps/LayoutSources/"+name+".txt");
                    asset.Settings=name=="Rimewatch"?Simulation.ReferenceMaps.Rimewatch(text):Simulation.ReferenceMaps.Ironfold(text);
                    AssetDatabase.CreateAsset(asset,path);
                }
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
            PlayerSettings.companyName = "Howl for Maul";
            PlayerSettings.productName = "Howl for Maul";
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
        [MenuItem("Howl for Maul/Open maze lab")]
        public static void OpenScene()
        {
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Game/Maps/MazeLab.unity");
        }
        [MenuItem("Howl for Maul/Select map parameters")]
        public static void SelectMap()
        {
            EnsureAssets();
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<MapDefinition>("Assets/Game/Maps/Resources/Rimewatch.asset");
        }
        [MenuItem("Howl for Maul/Build Linux")]
        public static void BuildLinux()
        {
            Build(BuildTarget.StandaloneLinux64, "Builds/Linux/HowlForMaul");
        }
        [MenuItem("Howl for Maul/Build Windows")]
        public static void BuildWindows()
        {
            Build(BuildTarget.StandaloneWindows64, "Builds/Windows/HowlForMaul.exe");
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
