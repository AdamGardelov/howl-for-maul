#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEditor.SceneManagement;
using FrostMaze.Simulation;
namespace FrostMaze.Tests
{
    public sealed class FactionReadabilityTests
    {
        // Fixed framing preserves relative sizes. Color-free silhouettes expose duplicated outlines
        // which per-model auto-framed portraits and distinctive materials can otherwise disguise.
        [UnityTest]
        public IEnumerator EveryFactionHasDistinctSmallOutlinesFromTwoPlayingAngles()
        {
            EditorSceneManager.OpenScene("Assets/Game/Maps/MazeLab.unity");yield return new EnterPlayMode();yield return null;
            string output=Path.Combine(Application.dataPath,"../Logs/FactionReadability");Directory.CreateDirectory(output);
            var report=new StringBuilder("map\tfaction\tfirst\tsecond\tmean_silhouette_overlap\n");
            var failures=new List<string>();int inspected=0;
            foreach(string map in new[]{"Rimewatch","Ironfold"}){
                var game=Object.FindFirstObjectByType<Prototype>();
                if(game.Map.name!=map){game.ChooseMap(Resources.Load<MapDefinition>(map));yield return null;yield return null;game=Object.FindFirstObjectByType<Prototype>();}
                game.StartMatch();game.Paused=true;int gold=game.World.Gold;long tick=game.World.Tick;
                var white=game.MakeMaterial(Color.white,true);var portraits=new TowerPortraits();
                var cameraObject=new GameObject("Outline audit camera");var camera=cameraObject.AddComponent<Camera>();camera.enabled=false;
                camera.cullingMask=1<<31;camera.orthographic=true;camera.orthographicSize=1.45f;camera.aspect=1;camera.nearClipPlane=.01f;camera.farClipPlane=20;
                camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.black;camera.allowHDR=false;camera.allowMSAA=false;
                var target=new RenderTexture(64,64,24);var readback=new Texture2D(64,64,TextureFormat.RGB24,false);var old=RenderTexture.active;
                try{
                    for(int faction=0;faction<game.World.Config.Factions.Length;faction++){
                        var designs=game.World.Config.Factions[faction].Designs;var masks=new List<bool[][]>();
                        var sheet=new Texture2D(designs.Length*80,240,TextureFormat.RGB24,false);var blank=new Color[sheet.width*sheet.height];
                        for(int p=0;p<blank.Length;p++)blank[p]=new Color(.055f,.07f,.065f);sheet.SetPixels(blank);
                        try{
                            for(int slot=0;slot<designs.Length;slot++){
                                int design=designs[slot];var def=game.World.Config.Catalog[design];
                                portraits.Prepare(game,design);Graphics.Blit(portraits.Get(design),target);RenderTexture.active=target;
                                readback.ReadPixels(new Rect(0,0,64,64),0,0);readback.Apply();sheet.SetPixels(slot*80+8,168,64,64,readback.GetPixels());
                                // Portrait preview cleanup must finish before the audit camera uses layer 31.
                                yield return null;
                                var root=new GameObject("Outline "+def.Name);
                                try{
                                    var tower=new Tower{Design=design,Name=def.Name,Spec=def.Spec,Health=def.Spec.Health};root.AddComponent<TowerView>().Initialize(game,tower,def,faction,true);
                                    root.transform.position=new Vector3(10000,0,10000);
                                    foreach(var t in root.GetComponentsInChildren<Transform>(true))t.gameObject.layer=31;
                                    foreach(var renderer in root.GetComponentsInChildren<Renderer>())if(renderer.enabled)renderer.sharedMaterial=white;
                                    var views=new bool[2][];
                                    for(int angle=0;angle<2;angle++){
                                        camera.transform.rotation=Quaternion.Euler(55,180+angle*45,0);
                                        camera.transform.position=root.transform.position+Vector3.up*1.05f-camera.transform.forward*8;
                                        camera.targetTexture=target;camera.Render();RenderTexture.active=target;
                                        readback.ReadPixels(new Rect(0,0,64,64),0,0);readback.Apply();var pixels=readback.GetPixels();views[angle]=new bool[pixels.Length];
                                        int coverage=0;for(int p=0;p<pixels.Length;p++){views[angle][p]=pixels[p].grayscale>.5f;if(views[angle][p])coverage++;}
                                        Assert.That(coverage,Is.GreaterThan(80),def.Name+" vanished at playing scale");
                                        sheet.SetPixels(slot*80+8,88-angle*80,64,64,pixels);
                                    }
                                    masks.Add(views);inspected++;
                                }finally{root.SetActive(false);Object.Destroy(root);}
                                yield return null;
                            }
                            for(int a=0;a<designs.Length;a++)for(int b=a+1;b<designs.Length;b++){
                                float mean=0;for(int angle=0;angle<2;angle++){
                                    int intersection=0,union=0;for(int p=0;p<4096;p++){bool x=masks[a][angle][p],y=masks[b][angle][p];if(x||y)union++;if(x&&y)intersection++;}
                                    mean+=(float)intersection/union*.5f;
                                }
                                string first=game.World.Config.Catalog[designs[a]].Name,second=game.World.Config.Catalog[designs[b]].Name;
                                report.AppendLine(map+"\t"+game.World.Config.Factions[faction].Name+"\t"+first+"\t"+second+"\t"+mean.ToString("F4",System.Globalization.CultureInfo.InvariantCulture));
                                // This rejects near duplicates, not a claim of human instant recognition.
                                if(mean>=.90f)failures.Add(first+" / "+second+" overlap "+mean);
                            }
                            sheet.Apply();File.WriteAllBytes(Path.Combine(output,map+"-"+faction+".png"),sheet.EncodeToPNG());
                        }finally{Object.Destroy(sheet);}
                    }
                    Assert.That(game.World.Gold,Is.EqualTo(gold));Assert.That(game.World.Tick,Is.EqualTo(tick));Assert.That(game.World.Grid.Towers.Count,Is.Zero);
                }finally{camera.targetTexture=null;RenderTexture.active=old;Object.Destroy(target);Object.Destroy(readback);Object.Destroy(cameraObject);portraits.Dispose();}
                yield return null;
            }
            File.WriteAllText(Path.Combine(output,"Pairs.tsv"),report.ToString());
            Assert.That(inspected,Is.EqualTo(76));Assert.That(failures,Is.Empty,string.Join("\n",failures));yield return new ExitPlayMode();
        }
    }
}
#endif
