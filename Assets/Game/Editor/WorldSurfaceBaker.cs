using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
namespace FrostMaze.Editor
{
    public sealed class WorldSurfaceBaker : IPreprocessBuildWithReport
    {
        const string Folder="Assets/Game/Presentation/Resources/World/Baked";
        public int callbackOrder=>10;
        public void OnPreprocessBuild(BuildReport report)=>Bake();
        static string Inputs() {
            var inputs=new System.Text.StringBuilder(WorldSurfaceSet.PaintRevision);
            foreach(var path in new[]{"Assets/Game/Presentation/MapScenery.Surface.cs","Assets/Game/Presentation/MapScenery.Composition.cs","Assets/Game/Presentation/WorldBackdrop.cs","Assets/Game/Presentation/WorldBackdrop.Layout.cs","Assets/Game/Presentation/Resources/World/HearthMeadow.png","Assets/Game/Presentation/Resources/World/HearthWaystone.png","Assets/Game/Presentation/Resources/World/HearthSlate.png","Assets/Game/Presentation/Resources/World/HearthMasonry.png"})inputs.Append(AssetDatabase.GetAssetDependencyHash(path));
            return Hash128.Compute(inputs.ToString()).ToString();
        }
        [MenuItem("Howl for Maul/Bake world surfaces")]
        public static void Bake() {
            Directory.CreateDirectory(Folder);AssetDatabase.Refresh();EnsureMeadowDetail();string inputs=Inputs();
            foreach(var name in new[]{"Ironfold","Rimewatch"}) {
                var config=Resources.Load<MapDefinition>(name).Settings;
                string path=Folder+"/"+name+".asset",key=WorldSurfaceSet.Key(config);
                var set=AssetDatabase.LoadAssetAtPath<WorldSurfaceSet>(path);
                if(set!=null&&set.Fingerprint==key&&set.EditorInputs==inputs&&set.Ground!=null&&set.Cap!=null&&set.Wall!=null&&set.Water!=null&&set.Exterior!=null)continue;
                var clock=System.Diagnostics.Stopwatch.StartNew();var root=new GameObject("Temporary world painter"){hideFlags=HideFlags.HideAndDontSave};Texture2D[] paints=null;Texture2D exterior=null;
                try {
                    paints=root.AddComponent<MapScenery>().PaintForBake(config);
                    exterior=root.AddComponent<WorldBackdrop>().PaintExteriorForBake(config);
                    if(set==null){set=ScriptableObject.CreateInstance<WorldSurfaceSet>();AssetDatabase.CreateAsset(set,path);}
                    set.Ground=Save(name+"-Ground",paints[0],false);set.Cap=Save(name+"-Cap",paints[1],false);
                    set.Wall=Save(name+"-Wall",paints[2],true);set.Water=Save(name+"-Water",paints[3],false);set.Exterior=Save(name+"-Exterior",exterior,false);
                    set.Fingerprint=key;set.EditorInputs=inputs;EditorUtility.SetDirty(set);AssetDatabase.SaveAssets();
                    Debug.Log("HOWL_SURFACES_BAKED "+name+" ms="+clock.ElapsedMilliseconds);
                } finally {
                    if(paints!=null)foreach(var texture in paints)Object.DestroyImmediate(texture);
                    if(exterior!=null)Object.DestroyImmediate(exterior);Object.DestroyImmediate(root);
                }
            }
        }
        static void EnsureMeadowDetail() {
            const string path="Assets/Game/Presentation/Resources/World/HearthMeadowDetail.mat";
            var material=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(material==null){material=new Material(Resources.Load<Material>("PrototypeMaterial"));AssetDatabase.CreateAsset(material,path);}
            material.color=Color.white;material.EnableKeyword("_DETAIL_SCALED");
            material.SetTexture("_DetailAlbedoMap",Resources.Load<Texture2D>("World/HearthMeadow"));
            material.SetFloat("_DetailAlbedoMapScale",.48f);material.SetFloat("_DetailNormalMapScale",0);
            material.SetFloat("_Smoothness",.10f);EditorUtility.SetDirty(material);AssetDatabase.SaveAssets();
        }
        static Texture2D Save(string name,Texture2D texture,bool repeat) {
            string path=Folder+"/"+name+".png";File.WriteAllBytes(path,texture.EncodeToPNG());AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Default;importer.sRGBTexture=true;importer.mipmapEnabled=true;
            importer.wrapMode=repeat?TextureWrapMode.Repeat:TextureWrapMode.Clamp;importer.wrapModeV=TextureWrapMode.Clamp;
            importer.filterMode=FilterMode.Trilinear;importer.anisoLevel=4;importer.maxTextureSize=1024;importer.isReadable=false;
            importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
    }
}
