#if UNITY_EDITOR
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEditor.SceneManagement;
using FrostMaze.Simulation;
using Object=UnityEngine.Object;
namespace FrostMaze.Tests
{
    public sealed class HowlEnemyTests
    {
        [Serializable] public class Sample { public string Map;public int Forms,MaxRenderers;public long StableSyncBytes,MovingSyncBytes; }
        [Serializable] public class Report { public string Scope="Eight Howl forms, both maps, animated geometry envelopes, shared assets, paused and moving Sync; gallery is staged, not a combat screenshot";public List<Sample> Samples=new List<Sample>(); }
        static readonly int[] waveIndices={0,11,12,13,10,4,18,19};
        [UnityTest,Category("HowlEnemies"),Timeout(300000)]
        public IEnumerator RosterFitsCollisionDiscsSharesAssetsAndFreezesOnPause()
        {
            EditorSceneManager.OpenScene("Assets/Game/Maps/MazeLab.unity");yield return new EnterPlayMode();yield return null;
            var report=new Report();string folder=Path.Combine(Application.dataPath,"../Logs/HowlEnemies");Directory.CreateDirectory(folder);
            foreach(string map in new[]{"Ironfold","Rimewatch"}) {
                Object.FindFirstObjectByType<Prototype>().ChooseMap(Resources.Load<MapDefinition>(map));yield return null;yield return null;
                var game=Object.FindFirstObjectByType<Prototype>();game.StartMatch();game.Paused=true;game.SoundEnabled=false;
                var w=game.World;int gold=w.Gold;long tick=w.Tick;var sample=new Sample{Map=map};
                var observed=new HashSet<HowlForm>();foreach(var wave in w.Config.Waves)observed.Add(EnemyIdentity.For(wave));Assert.That(observed.Count,Is.EqualTo(8));
                var stage=new GameObject("Howl roster art stage");var origin=new Vector3(10000,0,10000);stage.transform.position=origin;
                var cameraObject=new GameObject("Howl roster review camera");var camera=cameraObject.AddComponent<Camera>();camera.enabled=false;camera.cullingMask=1<<31;
                camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.06f,.08f,.07f);camera.orthographic=true;camera.orthographicSize=4.8f;camera.nearClipPlane=.1f;camera.farClipPlane=60;
                camera.transform.rotation=Quaternion.Euler(40,180,0);camera.transform.position=origin+new Vector3(0,.1f,0)-camera.transform.forward*20;
                var ground=new GameObject("Review ground");ground.transform.SetParent(stage.transform,false);ground.transform.localPosition=new Vector3(0,-.07f,0);ground.transform.localScale=new Vector3(18,.1f,11);
                ground.AddComponent<MeshFilter>().sharedMesh=game.Models.BeveledBox;ground.AddComponent<MeshRenderer>().sharedMaterial=game.TowerPalette(0)[4];ground.layer=31;
                var views=new List<EnemyView>();var enemies=new List<Enemy>();
                for(int i=0;i<8;i++) {
                    var spec=w.Config.Waves[waveIndices[i]];Assert.That(EnemyIdentity.For(spec),Is.EqualTo((HowlForm)i));
                    var e=new Enemy{Id=i+1,Spec=spec,Health=spec.Health,Position=new V2(0,0),Velocity=new V2(0,2),IntendedDirection=new V2(0,1)};
                    var root=new GameObject(EnemyIdentity.Name((HowlForm)i));root.transform.SetParent(stage.transform,false);var view=root.AddComponent<EnemyView>();
                    view.Initialize(e,game.HowlModels,game.HowlPalette,game.HowlPalette[2],game.HowlPalette[1],game.HowlPalette[3],game.Models);view.Sync(e,0);
                    views.Add(view);enemies.Add(e);Assert.That(root.GetComponentsInChildren<Collider>().Length,Is.Zero);
                    var filters=root.GetComponentsInChildren<MeshFilter>();int count=root.GetComponentsInChildren<Renderer>().Length;
                    sample.MaxRenderers=Math.Max(sample.MaxRenderers,count);Assert.That(count,Is.LessThanOrEqualTo(9),"One renderer per body detail would make waves too expensive");
                    foreach(var f in filters){Assert.That(f.sharedMesh.uv.Length,Is.EqualTo(f.sharedMesh.vertexCount));if(f.name.StartsWith("Howl batch"))Assert.That(Array.Exists(game.HowlModels.Get(view.Form).Parts,p=>ReferenceEquals(p.Mesh,f.sharedMesh)),Is.True,"Enemy allocated a private body mesh");}
                    // Exercise walking, every phase of melee lean and damage recoil, and arbitrary headings.
                    if(!spec.Flying)for(int t=1;t<=72;t++) {
                        e.LastAttackTick=t-(t%8);e.AttackDirection=new V2(Mathf.Sin(t*.8f),Mathf.Cos(t*.8f));if(t%7==0)e.Health-=1;
                        view.Sync(e,t);
                        foreach(var f in filters)if(f.name.StartsWith("Howl batch")) {
                            var matrix=root.transform.worldToLocalMatrix*f.transform.localToWorldMatrix;
                            foreach(var v in f.sharedMesh.vertices){var p=matrix.MultiplyPoint3x4(v);Assert.That(new Vector2(p.x,p.z).magnitude,Is.LessThanOrEqualTo(spec.Radius+.0001f),map+" "+view.Form+" art crossed the collision disc");}
                        }
                    }
                    e.LastAttackTick=-100;e.Health=spec.Health;view.Sync(e,100);var body=view.Body.localRotation;var limb=view.LeftMotion.localRotation;
                    for(int f=0;f<20;f++)view.Sync(e,100);
                    Assert.That(view.Body.localRotation,Is.EqualTo(body));Assert.That(view.LeftMotion.localRotation,Is.EqualTo(limb));
                    view.Sync(e,108);Assert.That(view.LeftMotion.localRotation,Is.Not.EqualTo(limb));
                    // Magnified art review; no world simulation or paid defense is represented here.
                    root.transform.localScale=Vector3.one*4;root.transform.localPosition=new Vector3(5.25f-i%4*3.5f,0,i<4?2.0f:-2.4f);root.transform.rotation=Quaternion.Euler(0,-18,0);
                    var label=new GameObject("Name");label.transform.SetParent(stage.transform,false);label.transform.localPosition=root.transform.localPosition+new Vector3(0,.1f,1.18f);label.transform.rotation=camera.transform.rotation;
                    var text=label.AddComponent<TextMesh>();text.text=EnemyIdentity.Name(view.Form);text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");text.fontSize=40;text.characterSize=.11f;text.anchor=TextAnchor.MiddleCenter;text.color=new Color(.92f,.85f,.64f);label.GetComponent<MeshRenderer>().sharedMaterial=text.font.material;
                    sample.Forms++;
                }
                foreach(var t in stage.GetComponentsInChildren<Transform>(true))t.gameObject.layer=31;
                yield return null;yield return null;Capture(camera,Path.Combine(folder,map+"-Roster.png"));
                // Warm the exact hot path, then measure paused and moving poses without fixtures/assertions in the measured block.
                for(int t=200;t<220;t++)for(int i=0;i<8;i++)views[i].Sync(enemies[i],t);
                long before=GC.GetAllocatedBytesForCurrentThread();for(int n=0;n<120;n++)for(int i=0;i<8;i++)views[i].Sync(enemies[i],219);sample.StableSyncBytes=GC.GetAllocatedBytesForCurrentThread()-before;
                before=GC.GetAllocatedBytesForCurrentThread();for(int n=220;n<340;n++)for(int i=0;i<8;i++)views[i].Sync(enemies[i],n);sample.MovingSyncBytes=GC.GetAllocatedBytesForCurrentThread()-before;
                Assert.That(sample.StableSyncBytes,Is.Zero);Assert.That(sample.MovingSyncBytes,Is.Zero);
                Assert.That(w.Gold,Is.EqualTo(gold));Assert.That(w.Tick,Is.EqualTo(tick));Assert.That(w.Grid.Towers.Count,Is.Zero);
                report.Samples.Add(sample);Object.Destroy(stage);Object.Destroy(cameraObject);yield return null;
            }
            File.WriteAllText(Path.Combine(folder,"report.json"),JsonUtility.ToJson(report,true));yield return new ExitPlayMode();
        }
        static void Capture(Camera camera,string path)
        {
            var target=new RenderTexture(1440,900,24);var previous=RenderTexture.active;var texture=new Texture2D(1440,900,TextureFormat.RGB24,false);
            try{camera.targetTexture=target;camera.Render();RenderTexture.active=target;texture.ReadPixels(new Rect(0,0,1440,900),0,0);texture.Apply();File.WriteAllBytes(path,texture.EncodeToPNG());}
            finally{camera.targetTexture=null;RenderTexture.active=previous;Object.DestroyImmediate(texture);Object.DestroyImmediate(target);}
        }
    }
}
#endif
