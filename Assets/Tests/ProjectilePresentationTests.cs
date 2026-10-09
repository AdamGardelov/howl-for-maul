#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEditor.SceneManagement;
using FrostMaze.Simulation;
namespace FrostMaze.Tests
{
    public sealed class ProjectilePresentationTests
    {
        [UnityTest,Category("WorldCohesion")]
        public IEnumerator ProjectileRosterRetainsIdentityFlightPauseAndBounds()
        {
            EditorSceneManager.OpenScene("Assets/Game/Maps/MazeLab.unity");yield return new EnterPlayMode();yield return null;
            int inspected=0;
            foreach(string map in new[]{"Rimewatch","Ironfold"}) {
                var game=Object.FindFirstObjectByType<Prototype>();
                if(game.Map.name!=map){game.ChooseMap(Resources.Load<MapDefinition>(map));yield return null;yield return null;game=Object.FindFirstObjectByType<Prototype>();}
                game.StartMatch();game.Paused=true;game.SoundEnabled=false;yield return null;
                var config=game.World.Config;var signatures=new HashSet<string>();
                var stage=new GameObject("Projectile review");var origin=new Vector3(10000,0,10000);stage.transform.position=origin;
                var cameraObject=new GameObject("Projectile review camera");var camera=cameraObject.AddComponent<Camera>();camera.enabled=false;camera.cullingMask=1<<31;
                camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.04f,.065f,.08f);camera.orthographic=true;camera.orthographicSize=map=="Rimewatch"?5.5f:7.5f;camera.nearClipPlane=.1f;camera.farClipPlane=70;
                camera.transform.rotation=Quaternion.Euler(60,180,0);camera.transform.position=origin+new Vector3(2,.1f,map=="Rimewatch"?3:5)-camera.transform.forward*25;
                var ground=new GameObject("Review ground");ground.transform.SetParent(stage.transform,false);ground.transform.localPosition=new Vector3(2,-.13f,5);ground.transform.localScale=new Vector3(16,.1f,20);
                ground.AddComponent<MeshFilter>().sharedMesh=game.Models.BeveledBox;ground.AddComponent<MeshRenderer>().sharedMaterial=game.TowerPalette(0)[4];ground.layer=31;
                for(int f=0;f<config.Factions.Length;f++) {
                    var group=new GameObject("Faction projectile review");group.transform.SetParent(stage.transform,false);int row=0;
                    foreach(int design in config.Factions[f].Designs) {
                        var def=config.Catalog[design];if(def.Spec.Damage<=0)continue;
                        var style=ProjectileStyle.For(config,design);inspected++;
                        Assert.That(signatures.Add(style.Shape+"/"+ColorUtility.ToHtmlStringRGB(style.Color)),Is.True,"Duplicate signature: "+def.Name);
                        var tower=new Tower{Design=design,Name=def.Name,Spec=def.Spec,Health=def.Spec.Health};
                        var root=new GameObject(def.Name);root.transform.SetParent(group.transform,false);root.AddComponent<TowerView>().Initialize(game,tower,def,f);
                        root.transform.position=origin+new Vector3(0,0,row*1.8f);
                        var shot=new GameObject(def.Name+" projectile");shot.transform.SetParent(group.transform,false);var cue=shot.AddComponent<ProjectileCue>();
                        var from=root.transform.position+Vector3.up*1.3f;var to=origin+new Vector3(4.5f,.3f,row*1.8f);
                        cue.Initialize(design,style,from,to,false,game.MakeMaterial(style.Color,true));cue.Render(.5f);
                        Assert.That(shot.GetComponentsInChildren<Renderer>().Length,Is.EqualTo(1));Assert.That(shot.GetComponentsInChildren<Collider>().Length,Is.Zero);
                        var mesh=shot.GetComponent<MeshFilter>().sharedMesh;Assert.That(mesh.vertexCount,Is.GreaterThan(20));Assert.That(shot.GetComponent<MeshRenderer>().sharedMaterial.color,Is.EqualTo(style.Color));
                        foreach(var point in mesh.vertices)Assert.That(float.IsNaN(point.x)||float.IsNaN(point.y)||float.IsNaN(point.z),Is.False);
                        row++;
                    }
                    foreach(var t in group.GetComponentsInChildren<Transform>())t.gameObject.layer=31;
                    yield return null;Capture(camera,"/tmp/Howl-"+map+"-Projectiles-"+f+".png");group.SetActive(false);Object.Destroy(group);yield return null;
                }
                Object.Destroy(stage);Object.Destroy(cameraObject);yield return null;
                // Real paid construction and combat produce the same per-design signature.
                var w=game.World;var start=w.BuilderPosition;bool ordered=false;
                for(int y=Mathf.FloorToInt(start.Y)-2;y<=Mathf.FloorToInt(start.Y)+2&&!ordered;y++)for(int x=Mathf.FloorToInt(start.X)-2;x<=Mathf.FloorToInt(start.X)+2&&!ordered;x++)if(w.CanBuild(x,y,out _))ordered=w.OrderBuild(x,y,out _);
                Assert.That(ordered,Is.True);for(int i=0;i<300&&w.Grid.Towers.Count==0;i++)w.Step();Assert.That(w.Grid.Towers.Count,Is.EqualTo(1));
                var built=w.Grid.Towers[0];var rts=game.View.GetComponent<RtsCamera>();rts.FocusPoint(built.Center);rts.SetZoom(5,true);yield return null;
                Assert.That(w.StartWave(),Is.True);var enemy=w.Spawn(new WaveSpec{Health=500,Flying=true},built.Center+new V2(1.8f,0));w.Step();yield return null;yield return null;
                Assert.That(enemy.Health,Is.LessThan(500));var feedback=game.GetComponent<CombatFeedback>();var live=feedback.GetComponentInChildren<ProjectileCue>();Assert.That(live,Is.Not.Null);
                Assert.That(live.Design,Is.EqualTo(built.Design));Assert.That(live.Style.Color,Is.EqualTo(ProjectileStyle.For(config,built.Design).Color));Assert.That(live.To.y,Is.EqualTo(1.7f));
                var visible=live.GetComponent<MeshFilter>().sharedMesh;var positions=visible.vertices;yield return new WaitForSecondsRealtime(.25f);
                var still=visible.vertices;CollectionAssert.AreEqual(positions,still,"Pause moved a projectile");
                // Explicit valid-design chain preserves endpoints and receives the source tower's color.
                w.Shots.Add(new ShotEvent{Serial=1000,Design=built.Design,From=built.Center,To=built.Center+new V2(2,1),Chained=true,FromFlying=true});yield return null;yield return null;
                var chain=GameObject.Find("Chain arc").GetComponent<LineRenderer>();Assert.That(chain.GetPosition(0).y,Is.EqualTo(1.7f));Assert.That(chain.GetPosition(chain.positionCount-1).y,Is.EqualTo(.3f));Assert.That(chain.sharedMaterial.color,Is.EqualTo(live.Style.Color));
                w.TowersFire=false;game.Speed=3;game.Paused=false;yield return new WaitForSecondsRealtime(.14f);game.Paused=true;yield return null;
                Assert.That(live==null&&chain==null,Is.True,"Speed-scaled projectiles did not expire");
                game.StartMatch();game.Paused=true;yield return null;yield return null;
                w=game.World;long tick=w.Tick;int gold=w.Gold;var at=w.BuilderPosition;
                // Offscreen events consume no slots; a visible 100-shot burst is capped at 64 renderers.
                for(int i=1;i<=100;i++)w.Shots.Add(new ShotEvent{Serial=i,Design=0,From=at+new V2(10000,0),To=at+new V2(10001,0)});
                yield return null;Assert.That(feedback.transform.Find("Combat cues").GetComponentsInChildren<Renderer>().Length,Is.Zero);
                for(int i=101;i<=200;i++)w.Shots.Add(new ShotEvent{Serial=i,Design=0,From=at,To=at+new V2(1,0)});
                yield return null;yield return null;Assert.That(feedback.transform.Find("Combat cues").GetComponentsInChildren<Renderer>().Length,Is.EqualTo(64));Assert.That(feedback.transform.Find("Combat cues").GetComponentsInChildren<Collider>().Length,Is.Zero);
                Assert.That(w.Gold,Is.EqualTo(gold));Assert.That(w.Tick,Is.EqualTo(tick));
                game.StartMatch();game.Paused=true;yield return null;yield return null;Assert.That(feedback.transform.Find("Combat cues").GetComponentsInChildren<Renderer>().Length,Is.Zero);
            }
            Assert.That(inspected,Is.EqualTo(72));yield return new ExitPlayMode();
        }
        static void Capture(Camera camera,string path)
        {
            var rt=new RenderTexture(1440,1080,24);var old=RenderTexture.active;var image=new Texture2D(1440,1080,TextureFormat.RGB24,false);
            try{camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,1440,1080),0,0);image.Apply();System.IO.File.WriteAllBytes(path,image.EncodeToPNG());}
            finally{camera.targetTexture=null;RenderTexture.active=old;Object.DestroyImmediate(image);Object.DestroyImmediate(rt);}
        }
    }
}
#endif
