#if UNITY_EDITOR
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEditor.SceneManagement;
using FrostMaze.Simulation;
namespace FrostMaze.Tests
{
    public sealed class ActorPresentationTests
    {
        [UnityTest,Category("WorldCohesion")]
        public IEnumerator CompleteRosterAndBuildersHaveStableDetailedPresentation()
        {
            EditorSceneManager.OpenScene("Assets/Game/Maps/MazeLab.unity");yield return new EnterPlayMode();yield return null;
            int inspected=0;
            foreach(string map in new[]{"Rimewatch","Ironfold"}) {
                var game=Object.FindFirstObjectByType<Prototype>();
                if(game.Map.name!=map){game.ChooseMap(Resources.Load<MapDefinition>(map));yield return null;yield return null;game=Object.FindFirstObjectByType<Prototype>();}
                game.StartMatch();game.Paused=true;
                int gold=game.World.Gold;long tick=game.World.Tick;var sharedBox=game.Models.BeveledBox;
                Assert.That(sharedBox.bounds.min.x,Is.EqualTo(-.5f).Within(.001f));Assert.That(sharedBox.bounds.max.x,Is.EqualTo(.5f).Within(.001f));
                Assert.That(sharedBox.uv.Length,Is.EqualTo(sharedBox.vertexCount));Assert.That(game.Models.Column.uv.Length,Is.EqualTo(game.Models.Column.vertexCount));
                var stage=new GameObject("Actor art review stage");var origin=new Vector3(10000,0,10000);stage.transform.position=origin;
                var cameraObject=new GameObject("Actor art review camera");var camera=cameraObject.AddComponent<Camera>();camera.enabled=false;camera.cullingMask=1<<31;
                camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.045f,.07f,.085f);camera.orthographic=true;camera.orthographicSize=4.4f;camera.nearClipPlane=.1f;camera.farClipPlane=50;
                camera.transform.rotation=Quaternion.Euler(35,180,0);camera.transform.position=origin+new Vector3(.1f,.9f,1.5f)-camera.transform.forward*18;
                var ground=new GameObject("Review ground");ground.transform.SetParent(stage.transform,false);ground.transform.localPosition=new Vector3(0,-.14f,1.6f);ground.transform.localScale=new Vector3(16,.1f,12);
                ground.AddComponent<MeshFilter>().sharedMesh=sharedBox;ground.AddComponent<MeshRenderer>().sharedMaterial=game.TowerPalette(0)[4];ground.layer=31;
                for(int faction=0;faction<game.World.Config.Factions.Length;faction++) {
                    var group=new GameObject("Faction review "+faction);group.transform.SetParent(stage.transform,false);
                    var hero=new GameObject("Builder review");hero.transform.SetParent(group.transform,false);var builder=hero.AddComponent<BuilderView>();builder.Configure(game,faction);
                    builder.Sync(new V2(origin.x-4.7f,origin.z+1.0f),20,faction%4);yield return null;
                    Assert.That(builder.VisibleFaction,Is.EqualTo(faction));Assert.That(builder.GetComponentsInChildren<Collider>().Length,Is.Zero);
                    Assert.That(hero.transform.Find(map=="Rimewatch"?"Hood":"Chassis"),Is.Not.Null);
                    if(map=="Ironfold")foreach(var filter in hero.GetComponentsInChildren<MeshFilter>()){
                        if(filter.name=="Ownership ring"||!filter.GetComponent<Renderer>().enabled)continue;
                        var matrix=hero.transform.worldToLocalMatrix*filter.transform.localToWorldMatrix;
                        foreach(var vertex in filter.sharedMesh.vertices){var point=matrix.MultiplyPoint3x4(vertex);
                            Assert.That(new Vector2(point.x,point.z).magnitude,Is.LessThan(.56f),"Builder hides adjacent defenses: "+faction);
                            Assert.That(point.y+1.1f,Is.LessThan(1.61f),"Builder is taller than the compact art envelope: "+faction);
                        }
                    }
                    Assert.That(hero.transform.Find("Ownership ring").position.y,Is.EqualTo(.04f).Within(.001f));
                    foreach(var renderer in builder.GetComponentsInChildren<Renderer>())if(renderer.name.StartsWith("Faction"))Assert.That(renderer.sharedMaterial,Is.SameAs(game.TowerPalette(faction)[1]));
                    builder.Sync(new V2(origin.x-4.6f,origin.z+1.0f),21,faction%4);
                    Assert.That(Vector3.Dot(builder.transform.forward,Vector3.right),Is.InRange(.01f,.99f),"First movement should turn smoothly rather than snap");
                    for(int t=22;t<=28;t++)builder.Sync(new V2(origin.x-4.6f+(t-21)*.1f,origin.z+1),t,faction%4);
                    Assert.That(Vector3.Dot(builder.transform.forward,Vector3.right),Is.GreaterThan(.99f),"Builder did not finish turning toward motion");
                    var rotation=hero.transform.Find("Left arm").localRotation;var height=hero.transform.position.y;
                    builder.Sync(new V2(origin.x-3.9f,origin.z+1.0f),28,faction%4);
                    Assert.That(hero.transform.Find("Left arm").localRotation,Is.EqualTo(rotation),"Frozen tick changed pose");Assert.That(hero.transform.position.y,Is.EqualTo(height),"Paused hovering must not jitter");
                    builder.Sync(new V2(origin.x-4.7f,origin.z+1),0,(faction+1)%4);Assert.That(hero.transform.rotation,Is.EqualTo(Quaternion.identity),"Reset/switch must not preserve another player's heading");
                    Assert.That(hero.transform.Find("Ownership ring").GetComponent<Renderer>().sharedMaterial,Is.SameAs(game.OwnerMaterial((faction+1)%4)));
                    int index=0;
                    foreach(int design in game.World.Config.Factions[faction].Designs) {
                        var def=game.World.Config.Catalog[design];var tower=new Tower{Design=design,Name=def.Name,Spec=def.Spec,Health=def.Spec.Health};
                        var root=new GameObject("Review "+def.Name);root.transform.SetParent(group.transform,false);var view=root.AddComponent<TowerView>();view.Initialize(game,tower,def,faction);
                        root.transform.position=origin+new Vector3(-2.1f+(index%4)*2.1f,0,(index/4)*3.6f);index++;inspected++;
                        Assert.That(root.transform.Find("Faction crest backing"),Is.Not.Null);Assert.That(root.transform.Find("Footing rim"),Is.Not.Null);
                        foreach(var filter in root.GetComponentsInChildren<MeshFilter>())Assert.That(filter.sharedMesh.uv.Length,Is.EqualTo(filter.sharedMesh.vertexCount),"Missing actor texture coordinates: "+filter.name);
                        Assert.That(game.TowerPalette(faction)[0].mainTexture,Is.Not.Null);
                        // Each faction can change its shape, but the visible level-three model must
                        // still fit its placement cell when aimed diagonally. Check actual vertices.
                        tower.Level=3;view.Sync(tower,false);
                        var gun=root.transform.Find(view.Role+" weapon");if(gun!=null)gun.localRotation=Quaternion.Euler(0,45,0);
                        foreach(var filter in root.GetComponentsInChildren<MeshFilter>()){
                            if(!filter.GetComponent<Renderer>().enabled)continue;
                            var toTower=root.transform.worldToLocalMatrix*filter.transform.localToWorldMatrix;
                            foreach(var vertex in filter.sharedMesh.vertices){var point=toTower.MultiplyPoint3x4(vertex);
                                Assert.That(new Vector2(point.x,point.z).magnitude,Is.LessThanOrEqualTo(.501f),def.Name+" overhangs the occupied cell");
                            }
                        }
                        tower.Level=1;view.Sync(tower,false);if(gun!=null)gun.localRotation=Quaternion.identity;
                        root.transform.position=origin+new Vector3(-2.1f+((index-1)%4)*2.1f,0,((index-1)/4)*3.6f);
                    }
                    foreach(var t in group.GetComponentsInChildren<Transform>(true))t.gameObject.layer=31;
                    yield return null;yield return null;
                    Assert.That(group.GetComponentsInChildren<Collider>().Length,Is.Zero);
                    Capture(camera,"/tmp/Howl-"+map+"-Actors-"+faction+".png");
                    group.SetActive(false);Object.Destroy(group);yield return null;
                }
                Object.Destroy(stage);Object.Destroy(cameraObject);yield return null;
                Assert.That(game.World.Gold,Is.EqualTo(gold));Assert.That(game.World.Tick,Is.EqualTo(tick));Assert.That(game.World.Grid.Towers.Count,Is.Zero);
                game.SetupOptions.Factions[0]=1;game.StartMatch();game.Paused=true;yield return null;yield return null;
                var actual=GameObject.Find("Builder drone").GetComponent<BuilderView>();
                Assert.That(actual.VisibleFaction,Is.EqualTo(1));Assert.That(actual.GetComponentsInChildren<Collider>().Length,Is.Zero);
                game.SetupOptions.Factions[0]=0;game.StartMatch();game.Paused=true;yield return null;
                var world=game.World;var start=world.BuilderPosition;int paid=world.Gold;bool built=false;
                for(int y=Mathf.FloorToInt(start.Y)-2;y<=Mathf.FloorToInt(start.Y)+2&&!built;y++)for(int x=Mathf.FloorToInt(start.X)-2;x<=Mathf.FloorToInt(start.X)+2&&!built;x++) {
                    if(!world.CanBuild(x,y,out _))continue;
                    Assert.That(world.OrderBuild(x,y,out _),Is.True);
                    for(int i=0;i<300&&world.Grid.Towers.Count==0;i++)world.Step();built=world.Grid.Towers.Count==1;
                }
                Assert.That(built,Is.True,"Live model fixture needs a real paid construction");
                Assert.That(world.Gold,Is.EqualTo(paid-world.Config.Catalog[world.SelectedDesign].Cost));
                world.MoveBuilder(start);for(int i=0;i<10;i++){world.Step();yield return null;}
                actual=GameObject.Find("Builder drone").GetComponent<BuilderView>();
                Assert.That(actual.transform.position.x,Is.EqualTo(world.BuilderPosition.X).Within(.001f));Assert.That(actual.transform.position.z,Is.EqualTo(world.BuilderPosition.Y).Within(.001f));
                var pose=actual.transform.Find("Left arm").localRotation;var location=actual.transform.position;
                yield return null;yield return null;Assert.That(actual.transform.position,Is.EqualTo(location));Assert.That(actual.transform.Find("Left arm").localRotation,Is.EqualTo(pose));
                var rts=game.View.GetComponent<RtsCamera>();rts.FocusPoint(world.BuilderPosition);rts.SetZoom(5,true);yield return null;
                Capture(game.View,"/tmp/Howl-"+map+"-Actors-In-Map.png");
            }
            Assert.That(inspected,Is.EqualTo(76));yield return new ExitPlayMode();
        }
        [UnityTest,Category("WorldCohesion")]
        public IEnumerator GroundedArtisansKeepPaidWorkPosesAndOwnerRings()
        {
            EditorSceneManager.OpenScene("Assets/Game/Maps/MazeLab.unity");yield return new EnterPlayMode();yield return null;
            var game=Object.FindFirstObjectByType<Prototype>();
            if(game.Map.name!="Ironfold"){game.ChooseMap(Resources.Load<MapDefinition>("Ironfold"));yield return null;yield return null;}
            foreach(int faction in new[]{0,1,3}){
                game.SetupOptions.Factions[0]=faction;game.StartMatch();game.Paused=true;game.MoveMode=true;yield return null;yield return null;
                var world=game.World;var builder=GameObject.Find("Builder drone").GetComponent<BuilderView>();
                var start=world.BuilderPosition;int gold=world.Gold;bool ordered=false;
                for(int y=Mathf.FloorToInt(start.Y)-2;y<=Mathf.FloorToInt(start.Y)+2&&!ordered;y++)for(int x=Mathf.FloorToInt(start.X)-2;x<=Mathf.FloorToInt(start.X)+2&&!ordered;x++)
                    if(world.CanBuild(x,y,out _))ordered=world.OrderBuild(x,y,out _);
                Assert.That(ordered,Is.True);
                for(int tick=0;tick<300&&world.Grid.Towers.Count==0;tick++){world.Step();yield return null;}
                Assert.That(world.Grid.Towers.Count,Is.EqualTo(1));Assert.That(world.Gold,Is.EqualTo(gold-world.BuildCost));
                Assert.That(builder.Constructing,Is.True);
                var arm=builder.transform.Find("Right arm");var completed=arm.localRotation;
                for(int tick=0;tick<6;tick++){world.Step();yield return null;}
                Assert.That(Quaternion.Angle(completed,arm.localRotation),Is.GreaterThan(10),"Paid completion should animate the working tool");
                var pose=arm.localRotation;var height=builder.transform.position.y;
                yield return new WaitForSecondsRealtime(.2f);
                Assert.That(arm.localRotation,Is.EqualTo(pose));Assert.That(builder.transform.position.y,Is.EqualTo(height));
                Assert.That(height,Is.EqualTo(1.1f).Within(.001f),"Grounded artisans must not hover");
                Assert.That(builder.transform.Find("Ownership ring").position.y,Is.EqualTo(.04f).Within(.001f));
                var rts=game.View.GetComponent<RtsCamera>();rts.FocusPoint(world.BuilderPosition);rts.SetZoom(5,true);yield return null;
                Capture(game.View,"/tmp/Howl-Artisan-Paid-"+faction+".png");
                var before=builder.transform.Find("Left stride").localRotation;world.MoveBuilder(start+new V2(3,1));
                for(int tick=0;tick<6;tick++){world.Step();yield return null;}
                Assert.That(builder.Walking,Is.True);Assert.That(Quaternion.Angle(before,builder.transform.Find("Left stride").localRotation),Is.GreaterThan(1));
            }
            yield return new ExitPlayMode();
        }
        static void Capture(Camera camera,string path)
        {
            var target=new RenderTexture(1440,900,24);var previous=RenderTexture.active;var image=new Texture2D(1440,900,TextureFormat.RGB24,false);
            try{camera.targetTexture=target;camera.Render();RenderTexture.active=target;image.ReadPixels(new Rect(0,0,1440,900),0,0);image.Apply();System.IO.File.WriteAllBytes(path,image.EncodeToPNG());}
            finally{camera.targetTexture=null;RenderTexture.active=previous;Object.DestroyImmediate(image);Object.DestroyImmediate(target);}
        }
    }
}
#endif
