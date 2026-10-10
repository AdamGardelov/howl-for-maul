#if UNITY_EDITOR
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEditor.SceneManagement;
using FrostMaze.Simulation;
namespace FrostMaze.Tests
{
    public sealed class WorldCohesionTests
    {
        sealed class StillCameraInput : ICameraInput { public CameraIntent Read()=>default; }
        [UnityTest,Category("WorldCohesion"),Category("WorldCohesionFinal")]
        public IEnumerator LandmarksAmbientVoicesAndConstructionStayCosmetic()
        {
            EditorSceneManager.OpenScene("Assets/Game/Maps/MazeLab.unity");yield return new EnterPlayMode();yield return null;
            foreach(string map in new[]{"Rimewatch","Ironfold"}) {
                var game=Object.FindFirstObjectByType<Prototype>();
                if(game.Map.name!=map){game.ChooseMap(Resources.Load<MapDefinition>(map));yield return null;yield return null;game=Object.FindFirstObjectByType<Prototype>();}
                game.StartMatch();game.Paused=true;game.MusicVolume=0;game.SoundEnabled=true;game.EffectsVolume=1;yield return null;
                var w=game.World;var scenery=game.GetComponentInChildren<MapScenery>();var ambience=game.GetComponent<WorldAmbience>();
                Assert.That(scenery.LandmarkPositions.Count,Is.GreaterThanOrEqualTo(3));
                foreach(var landmark in scenery.LandmarkPositions){bool paired=false;foreach(var other in scenery.LandmarkPositions)paired|=Vector3.Distance(other,new Vector3(w.Config.Width-landmark.x,landmark.y,landmark.z))<.001f;Assert.That(paired,Is.True,"Unpaired mirrored landmark");}Assert.That(scenery.Groves,Is.GreaterThanOrEqualTo(8));
                Assert.That(ambience.VoiceCount,Is.EqualTo(4));Assert.That(ambience.GetComponents<AudioSource>().Length,Is.EqualTo(1),"Only existing combat source belongs directly to prototype");
                CollectionAssert.AreEqual(game.Map.Settings.LayoutRows,w.Config.LayoutRows,"Art must not change the supplied mask");
                ValidateComposition(scenery,w.Config);
                var landmarkTextures=new System.Collections.Generic.List<Texture>();
                foreach(int batch in w.Config.Theme=="iron"?new[]{29,32}:new[]{32})landmarkTextures.Add(scenery.transform.Find("Scenery "+batch).GetComponent<Renderer>().sharedMaterial.mainTexture);
                var refuge=game.GetComponentInChildren<WorldBackdrop>();Assert.That(refuge,Is.Not.Null);
                Assert.That(refuge.RefugeVertices,Is.InRange(1000,20000));
                Assert.That(refuge.RefugeHearths.Count,Is.EqualTo(2));
                foreach(var filter in refuge.GetComponentsInChildren<MeshFilter>())if(filter.name.Contains(" refuge ")) {
                    Assert.That(filter.GetComponent<Collider>(),Is.Null);
                    foreach(var vertex in filter.sharedMesh.vertices)Assert.That(vertex.z,Is.LessThan(0),"Refuge geometry intrudes into the buildable map");
                }
                var slate=Resources.Load<Texture2D>("World/HearthSlate");Assert.That(slate,Is.Not.Null);Assert.That(slate.isReadable,Is.True);
                var rts=game.View.GetComponent<RtsCamera>();rts.SetInput(new StillCameraInput());
                var anchor=scenery.LandmarkPositions[0];rts.FocusPoint(new V2(anchor.x,anchor.z));rts.SetZoom(8,true);
                long frozen=w.Tick;yield return new WaitForSecondsRealtime(1);
                Assert.That(w.Tick,Is.EqualTo(frozen));Assert.That(ambience.LandmarkLevel,Is.GreaterThan(.01f));
                Assert.That(scenery.ActiveFireLights,Is.InRange(1,4));
                foreach(var light in scenery.GetComponentsInChildren<Light>()){Assert.That(light.intensity,Is.LessThan(1.5f));Assert.That(light.shadows,Is.EqualTo(LightShadows.None));}
                if(map=="Ironfold") {
                    // Ironfold no longer draws the old solid CPU-deformed grove cores.
                    // Verify the replacement painted foliage really moves in the GPU render.
                    yield return CheckPaintedWind(game,rts);
                    rts.FocusPoint(new V2(anchor.x,anchor.z));rts.SetZoom(8,true);yield return null;
                } else {
                    var canopy=scenery.transform.Find("Scenery 27").GetComponent<MeshFilter>().sharedMesh;
                    var leaves=canopy.vertices;yield return new WaitForSecondsRealtime(.35f);var moved=canopy.vertices;float sway=0;
                    for(int i=0;i<leaves.Length;i++)sway=Mathf.Max(sway,Vector3.Distance(leaves[i],moved[i]));
                    Assert.That(sway,Is.InRange(.0001f,.08f),"Foliage should move gently while combat remains paused");
                }
                Assert.That(w.Tick,Is.EqualTo(frozen));
                Capture(game.View,"/tmp/Howl-"+map+"-Cohesion-Landmark.png");
                var owned=new System.Collections.Generic.List<AudioClip>();
                foreach(var source in game.GetComponentsInChildren<AudioSource>())if(source.clip!=null&&source.clip.name.StartsWith("Original environment")) {
                    owned.Add(source.clip);Assert.That(source.clip.channels,Is.EqualTo(2));var samples=new float[source.clip.samples*2];source.clip.GetData(samples,0);
                    float peak=0;foreach(float s in samples){Assert.That(float.IsNaN(s)||float.IsInfinity(s),Is.False);peak=Mathf.Max(peak,Mathf.Abs(s));}
                    Assert.That(peak,Is.InRange(.001f,.7f));Assert.That(Mathf.Abs(samples[0]),Is.LessThan(.001f));Assert.That(Mathf.Abs(samples[samples.Length-1]),Is.LessThan(.001f));
                }
                Assert.That(owned.Count,Is.EqualTo(4));
                game.SoundEnabled=false;yield return null;Assert.That(ambience.FireLevel,Is.Zero);Assert.That(ambience.LandmarkLevel,Is.Zero);
                game.SoundEnabled=true;game.EffectsVolume=0;yield return null;Assert.That(ambience.FireLevel,Is.Zero);Assert.That(ambience.LandmarkLevel,Is.Zero);
                game.EffectsVolume=1;
                rts.FocusPoint(w.BuilderPosition);rts.SetZoom(7,true);
                int gold=w.Gold;var builder=GameObject.Find("Builder drone").GetComponent<BuilderView>();
                var start=w.BuilderPosition;bool ordered=false;
                for(int y=Mathf.FloorToInt(start.Y)-2;y<=Mathf.FloorToInt(start.Y)+2&&!ordered;y++)for(int x=Mathf.FloorToInt(start.X)-2;x<=Mathf.FloorToInt(start.X)+2&&!ordered;x++)if(w.CanBuild(x,y,out _))ordered=w.OrderBuild(x,y,out _);
                Assert.That(ordered,Is.True);
                for(int i=0;i<300&&w.Grid.Towers.Count==0;i++){w.Step();yield return null;}
                Assert.That(w.Grid.Towers.Count,Is.EqualTo(1));Assert.That(w.Gold,Is.EqualTo(gold-w.BuildCost));Assert.That(builder.Constructing,Is.True,"Actual paid completion should produce a work gesture");
                var arm=builder.transform.Find("Right arm");var pose=arm.localRotation;yield return new WaitForSecondsRealtime(.15f);Assert.That(arm.localRotation,Is.EqualTo(pose),"Paused work gesture changed");
                var tower=w.Grid.Towers[0];Assert.That(w.StartWave(),Is.True);var enemy=w.Spawn(new WaveSpec{Health=1000,Speed=0},tower.Center+new V2(1.8f,0));w.Step();yield return null;
                Assert.That(enemy.Health,Is.LessThan(1000));
                var enemyView=GameObject.Find("Enemy "+enemy.Id).GetComponent<EnemyView>();var body=enemyView.Body;Assert.That(Quaternion.Angle(body.localRotation,Quaternion.identity),Is.GreaterThan(1));
                Capture(game.View,"/tmp/Howl-"+map+"-Cohesion-Combat.png");
                w.TowersFire=false;var walkStart=w.BuilderPosition;w.MoveBuilder(walkStart+new V2(8,1));game.Paused=false;
                // Let the real update loop advance: stepping manually as well can turn a low-frame-rate
                // test into a >2-unit apparent teleport, which intentionally suppresses footsteps.
                float until=Time.realtimeSinceStartup+3;
                while(ambience.Footsteps==0&&Time.realtimeSinceStartup<until)yield return new WaitForSecondsRealtime(.03f);
                game.Paused=true;Assert.That((w.BuilderPosition-walkStart).Length,Is.GreaterThan(.8f),"Footstep fixture did not move the builder");
                Assert.That(ambience.Footsteps,Is.GreaterThan(0));
                yield return new WaitForSecondsRealtime(.3f);int beforeCatchup=ambience.Footsteps;
                var catchupStart=w.BuilderPosition;w.MoveBuilder(catchupStart+new V2(5,0));
                // Reproduce a frame containing several legitimate simulation ticks at high speed.
                game.Paused=false;int catchupTicks=Mathf.CeilToInt(3/(w.Config.BuilderSpeed*World.FixedDelta));
                for(int i=0;i<catchupTicks;i++)w.Step();yield return null;yield return null;game.Paused=true;
                Assert.That(ambience.Footsteps,Is.GreaterThan(beforeCatchup),"Valid catch-up movement was mistaken for a teleport");
                int steps=ambience.Footsteps;yield return new WaitForSecondsRealtime(.3f);Assert.That(ambience.Footsteps,Is.EqualTo(steps));
                rts.Overview();yield return null;Capture(game.View,"/tmp/Howl-"+map+"-Cohesion-Overview.png");
                rts.FocusPoint(new V2(31,4));rts.SetZoom(17,true);yield return null;Capture(game.View,"/tmp/Howl-"+map+"-Cohesion-Exterior.png");
                game.ChooseMap(Resources.Load<MapDefinition>(map=="Rimewatch"?"Ironfold":"Rimewatch"));yield return null;yield return null;
                foreach(var clip in owned)Assert.That(clip==null,Is.True,"Changing maps leaked ambient audio");
                foreach(var texture in landmarkTextures)Assert.That(texture==null,Is.True,"Changing maps leaked generated landmark paint");
            }
            yield return new ExitPlayMode();
        }
        static IEnumerator CheckPaintedWind(Prototype game,RtsCamera rts) {
            LivingWorld garden=null;
            foreach(var world in game.GetComponentsInChildren<LivingWorld>())if(!world.Exterior)garden=world;
            var crowns=garden.transform.Find("Painted crowns");var material=crowns.GetComponent<Renderer>().sharedMaterial;
            var vertices=crowns.GetComponent<MeshFilter>().sharedMesh.vertices;var point=vertices[0];
            rts.FocusPoint(new V2(point.x,point.z));rts.SetZoom(5,true);yield return null;
            var camera=game.View;int oldMask=camera.cullingMask,oldLayer=crowns.gameObject.layer;
            var oldFlags=camera.clearFlags;var oldColor=camera.backgroundColor;float wind=material.GetFloat("_Wind");
            try {
                crowns.gameObject.layer=31;camera.cullingMask=1<<31;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.black;
                Assert.That(wind,Is.InRange(.01f,LivingWorld.WindEnvelope));
                var first=WindFrame(camera);yield return new WaitForSecondsRealtime(.65f);var second=WindFrame(camera);
                Assert.That(ChangedPixels(first,second),Is.GreaterThan(5),"Painted canopy should visibly sway while combat is paused");
                material.SetFloat("_Wind",0);first=WindFrame(camera);yield return new WaitForSecondsRealtime(.35f);second=WindFrame(camera);
                Assert.That(ChangedPixels(first,second),Is.Zero,"The isolated static control should not change; other effects must not satisfy the wind check");
            } finally {material.SetFloat("_Wind",wind);crowns.gameObject.layer=oldLayer;camera.cullingMask=oldMask;camera.clearFlags=oldFlags;camera.backgroundColor=oldColor;}
        }
        static Color32[] WindFrame(Camera camera) {
            var rt=new RenderTexture(256,256,24);var old=RenderTexture.active;var oldTarget=camera.targetTexture;var image=new Texture2D(256,256,TextureFormat.RGB24,false);
            try {camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,256,256),0,0);image.Apply();return image.GetPixels32();}
            finally {camera.targetTexture=oldTarget;RenderTexture.active=old;Object.DestroyImmediate(image);Object.DestroyImmediate(rt);}
        }
        static int ChangedPixels(Color32[] a,Color32[] b) {
            int changed=0;for(int i=0;i<a.Length;i++)if(Mathf.Abs(a[i].r-b[i].r)+Mathf.Abs(a[i].g-b[i].g)+Mathf.Abs(a[i].b-b[i].b)>12)changed++;return changed;
        }
        static bool SegmentHitsRect(Vector2 a,Vector2 b,Vector2 min,Vector2 max) {
            float enter=0,exit=1;var delta=b-a;
            for(int axis=0;axis<2;axis++) {
                float at=axis==0?a.x:a.y,d=axis==0?delta.x:delta.y,lo=axis==0?min.x:min.y,hi=axis==0?max.x:max.y;
                if(Mathf.Abs(d)<.00001f){if(at<lo||at>hi)return false;continue;}
                float first=(lo-at)/d,last=(hi-at)/d;if(first>last){float swap=first;first=last;last=swap;}
                enter=Mathf.Max(enter,first);exit=Mathf.Min(exit,last);if(enter>exit)return false;
            }
            return true;
        }
        static void ValidateComposition(MapScenery scenery,Scenario config) {
            foreach(var filter in scenery.GetComponentsInChildren<MeshFilter>()) {
                if(!filter.name.StartsWith("Scenery ")||!int.TryParse(filter.name.Substring(8),out int batch)||batch<24)continue;
                var prop=filter.transform;
                var mesh=prop.GetComponent<MeshFilter>().sharedMesh;var vertices=mesh.vertices;var triangles=mesh.triangles;
                Assert.That(prop.GetComponent<Collider>(),Is.Null);
                var reflected=new System.Collections.Generic.HashSet<Vector3Int>();
                foreach(var v in vertices)reflected.Add(new Vector3Int(Mathf.RoundToInt(v.x*1000),Mathf.RoundToInt(v.y*1000),Mathf.RoundToInt(v.z*1000)));
                foreach(var v in vertices){
                    var q=new Vector3Int(Mathf.RoundToInt((config.Width-v.x)*1000),Mathf.RoundToInt(v.y*1000),Mathf.RoundToInt(v.z*1000));
                    Assert.That(reflected.Contains(q)||reflected.Contains(q+Vector3Int.right)||reflected.Contains(q-Vector3Int.right),Is.True,"Unpaired composition vertex: "+filter.name+" "+v);
                }
                if(batch>=30&&batch<=32){
                    Assert.That(prop.GetComponent<Renderer>().sharedMaterial.mainTexture,Is.Not.Null);
                    Assert.That(mesh.uv.Length,Is.EqualTo(vertices.Length));
                }
                for(int i=0;i<triangles.Length;i+=3) {
                    var a=vertices[triangles[i]];var b=vertices[triangles[i+1]];var c=vertices[triangles[i+2]];
                    var min=new Vector2(Mathf.Min(a.x,b.x,c.x),Mathf.Min(a.z,b.z,c.z));var max=new Vector2(Mathf.Max(a.x,b.x,c.x),Mathf.Max(a.z,b.z,c.z));
                    float cell=config.LayoutCellSize;
                    for(int z=Mathf.FloorToInt(min.y/cell);z<=Mathf.FloorToInt(max.y/cell);z++)for(int x=Mathf.FloorToInt(min.x/cell);x<=Mathf.FloorToInt(max.x/cell);x++) {
                        int row=config.LayoutRows.Length-1-z;Assert.That(row,Is.InRange(0,config.LayoutRows.Length-1));Assert.That(x,Is.InRange(0,config.LayoutRows[row].Length-1));
                        Assert.That(config.WalkableSymbols.IndexOf(config.LayoutRows[row][x]),Is.LessThan(0),"Composed prop covers buildable terrain");
                    }
                    if(Mathf.Max(a.y,b.y,c.y)<1.35f)continue;
                    foreach(var lane in config.Lanes) {
                        var from=new Vector2(lane.Spawn.X,lane.Spawn.Y);
                        foreach(var point in lane.FlightRoute){var to=new Vector2(point.X,point.Y);Assert.That(SegmentHitsRect(from,to,min-Vector2.one*.4f,max+Vector2.one*.4f),Is.False,"Tall composition intersects a flying unit corridor");from=to;}
                    }
                }
            }
        }
        static void Capture(Camera camera,string path) {
            var rt=new RenderTexture(1440,900,24);var old=RenderTexture.active;var image=new Texture2D(1440,900,TextureFormat.RGB24,false);
            try{camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,1440,900),0,0);image.Apply();System.IO.File.WriteAllBytes(path,image.EncodeToPNG());}
            finally{camera.targetTexture=null;RenderTexture.active=old;Object.DestroyImmediate(image);Object.DestroyImmediate(rt);}
        }
    }
}
#endif
