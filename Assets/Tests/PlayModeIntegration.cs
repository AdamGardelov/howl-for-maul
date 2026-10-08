#if UNITY_EDITOR
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEditor.SceneManagement;
namespace FrostMaze.Tests
{
    public sealed class PlayModeIntegration
    {
        static int EffectRendererCount(CombatFeedback feedback)
        {
            return feedback.transform.Find("Combat cues").GetComponentsInChildren<Renderer>().Length;
        }
        sealed class CameraInputFixture : ICameraInput
        {
            public CameraIntent Intent;
            public CameraIntent Read()=>Intent;
        }
        [UnityTest]
        public IEnumerator CameraDragRejectsUiOriginsAndFreezesInSetup()
        {
            EditorSceneManager.OpenScene("Assets/Game/Maps/MazeLab.unity");
            yield return new EnterPlayMode();yield return null;
            var game=Object.FindFirstObjectByType<Prototype>();game.StartMatch();game.Paused=true;
            var camera=game.View.GetComponent<RtsCamera>();var input=new CameraInputFixture();camera.SetInput(input);
            camera.Overview();
            var bottomPoint=game.View.WorldToScreenPoint(new Vector3(game.World.Config.Width*.5f,0,0));
            var topPoint=game.View.WorldToScreenPoint(new Vector3(game.World.Config.Width*.5f,0,game.World.Config.Height));
            Assert.That(Screen.height-bottomPoint.y,Is.LessThan(game.BuildHud.yMin),"Overview bottom is covered by tower bar");
            Assert.That(Screen.height-topPoint.y,Is.GreaterThan(game.TopHud.yMax),"Overview top is covered by status bar");
            Assert.That(game.View.rect,Is.EqualTo(new Rect(0,0,1,1)));Assert.That(game.Sidebar,Is.EqualTo(Rect.zero));
            Assert.That(game.PointerOverHud(game.TopHud.center),Is.True);Assert.That(game.PointerOverHud(game.BuildHud.center),Is.True);
            Assert.That(game.PointerOverHud(new Vector2(50,Screen.height*.5f)),Is.False,"Hidden sidebar still blocks the map");
            game.Paused=false;game.ToggleMenu();long tick=game.World.Tick;var menuFocus=camera.Focus;
            input.Intent=new CameraIntent{Pointer=Vector2.one*400,Pan=Vector2.one,Zoom=5};
            yield return null;yield return null;yield return null;
            Assert.That(game.World.Tick,Is.EqualTo(tick),"Menu failed to pause simulation");Assert.That(camera.Focus,Is.EqualTo(menuFocus));
            Assert.That(game.PointerOverHud(Vector2.zero),Is.True,"Modal menu permits world input");
            game.ToggleMenu();Assert.That(game.Paused,Is.False,"Closing menu changed prior pause state");
            game.Paused=true;game.ToggleMenu();game.ToggleMenu();Assert.That(game.Paused,Is.True);
            game.DetailsOpen=true;Assert.That(game.Sidebar.width,Is.GreaterThan(0));
            Assert.That(game.PointerOverHud(game.BuildHud.center),Is.False,"Hidden tower bar still blocks map in Details view");
            game.DetailsOpen=false;

            camera.FocusPoint(new FrostMaze.Simulation.V2(32,32));var start=camera.Focus;
            var ui=new Vector2(game.TopHud.center.x,Screen.height-game.TopHud.center.y);var world=new Vector2(Screen.width*.6f,Screen.height*.5f);
            input.Intent=new CameraIntent{Pointer=ui,Dragging=true,DragStarted=true};yield return null;
            input.Intent=new CameraIntent{Pointer=world,Dragging=true,Drag=new Vector2(100,50)};yield return null;
            Assert.That(camera.Focus,Is.EqualTo(start),"Drag starting over compact HUD leaked into camera");
            input.Intent=new CameraIntent{Pointer=world};yield return null;
            input.Intent=new CameraIntent{Pointer=world,Dragging=true,DragStarted=true};yield return null;
            Assert.That(camera.Focus,Is.EqualTo(start),"First drag frame jumped");
            input.Intent=new CameraIntent{Pointer=world,Dragging=true,Drag=new Vector2(100,50)};yield return null;
            float scale=game.View.orthographicSize*2/Screen.height;
            Assert.That(camera.Focus.x-start.x,Is.EqualTo(100*scale).Within(.01f));
            Assert.That(camera.Focus.z-start.z,Is.EqualTo(50*scale/Mathf.Sin(55*Mathf.Deg2Rad)).Within(.01f));
            input.Intent=new CameraIntent{Pointer=world};yield return null;
            game.OpenSetup();start=camera.Focus;float zoom=game.View.orthographicSize;
            input.Intent=new CameraIntent{Pointer=world,Pan=Vector2.one,Zoom=5};yield return null;yield return null;
            Assert.That(camera.Focus,Is.EqualTo(start));Assert.That(game.View.orthographicSize,Is.EqualTo(zoom));
            game.ReturnToMatch();input.Intent=new CameraIntent{Pointer=world,Dragging=true,DragStarted=true};yield return null;
            input.Intent=new CameraIntent{Pointer=world,Dragging=true,Drag=Vector2.one*100000};yield return null;
            Assert.That(camera.Focus.x,Is.EqualTo(camera.BoundsMax.x));Assert.That(camera.Focus.z,Is.EqualTo(camera.BoundsMax.y));
            Assert.That(game.World.Gold,Is.EqualTo(1200));Assert.That(game.World.Grid.Towers.Count,Is.Zero);
            input.Intent=new CameraIntent{Pointer=world};yield return new ExitPlayMode();
        }
        [UnityTest]
        public IEnumerator PaidWallSiegeShowsStrikesDestructionAndRouteOpening()
        {
            EditorSceneManager.OpenScene("Assets/Game/Maps/MazeLab.unity");
            yield return new EnterPlayMode();yield return null;
            var game=Object.FindFirstObjectByType<Prototype>();
            if(game.Map.name!="Rimewatch"){game.ChooseMap(Resources.Load<MapDefinition>("Rimewatch"));yield return null;yield return null;game=Object.FindFirstObjectByType<Prototype>();}
            game.SetupOptions=new FrostMaze.Simulation.MatchOptions();game.StartMatch();game.Paused=true;game.SoundEnabled=false;
            var w=game.World;w.SelectedDesign=1;
            for(int x=28;x<=33;x++) {
                Assert.That(w.OrderBuild(x,11,out var reason),Is.True,reason);
                for(int i=0;i<180&&w.Grid.At(x,11)==null;i++)w.Step();
                Assert.That(w.Grid.At(x,11),Is.Not.Null);
            }
            Assert.That(w.Gold,Is.EqualTo(1170),"The seal must be paid for");
            var enemy=w.Spawn(new FrostMaze.Simulation.WaveSpec{Health=1000,Speed=1.55f,Damage=60,AttackInterval=.6f},new FrostMaze.Simulation.V2(31.5f,12.4f));
            Assert.That(enemy,Is.Not.Null);enemy.Checkpoint=w.LaneRoute(0,false).Length-1;
            yield return null;yield return null;
            for(int i=0;i<100&&enemy.LastAttackTick<0;i++)w.Step();
            Assert.That(enemy.Blocked,Is.True);Assert.That(enemy.LastAttackTick,Is.GreaterThan(0));
            var attacked=w.Grid.Find(enemy.BlockerId);
            Assert.That(attacked,Is.Not.Null);Assert.That(attacked.Health,Is.LessThan(attacked.Spec.Health));
            yield return null;yield return null;
            var cue=GameObject.Find("Tower struck");Assert.That(cue,Is.Not.Null);
            var body=GameObject.Find("Enemy "+enemy.Id).transform.Find("Armored crawler");var pose=body.localRotation;
            Assert.That(Quaternion.Angle(pose,Quaternion.identity),Is.GreaterThan(5));
            Assert.That(cue.GetComponentsInChildren<Collider>().Length,Is.Zero);
            yield return new WaitForSecondsRealtime(.1f);Assert.That(body.localRotation,Is.EqualTo(pose));
            game.OpenSetup();yield return new WaitForSecondsRealtime(.1f);Assert.That(body.localRotation,Is.EqualTo(pose));game.ReturnToMatch();
            for(int i=0;i<120&&attacked.Health>0;i++)w.Step();
            Assert.That(attacked.Health,Is.LessThanOrEqualTo(0));yield return null;yield return null;
            var rubble=GameObject.Find("Tower destroyed");Assert.That(rubble,Is.Not.Null);
            Assert.That(rubble.GetComponent<MeshFilter>().sharedMesh,Is.SameAs(game.Models.Rubble));
            Assert.That(rubble.GetComponentsInChildren<Collider>().Length,Is.Zero);
            for(int i=0;i<7;i++)w.Step();yield return null;
            Assert.That(enemy.Blocked,Is.False,"Destroying the seal should reopen the route");
            Assert.That(w.Grid.Clear(enemy.Position,enemy.Position,enemy.Spec.Radius),Is.True,"Enemy penetrated a remaining wall");
            Assert.That(Quaternion.Angle(body.localRotation,Quaternion.identity),Is.LessThan(.01f));
            int destroyed=0;foreach(var t in game.GetComponent<CombatFeedback>().transform.Find("Combat cues").GetComponentsInChildren<Transform>())if(t.name=="Tower destroyed")destroyed++;
            var sold=w.Grid.At(28,11)??w.Grid.At(33,11);Assert.That(sold,Is.Not.Null);Assert.That(w.Sell(sold.CellX,sold.CellY),Is.True);
            yield return null;yield return null;
            int afterSale=0;foreach(var t in game.GetComponent<CombatFeedback>().transform.Find("Combat cues").GetComponentsInChildren<Transform>())if(t.name=="Tower destroyed")afterSale++;
            Assert.That(afterSale,Is.EqualTo(destroyed),"Selling a healthy tower must not show a destruction cue");
            game.Paused=false;yield return new WaitForSecondsRealtime(.7f);Assert.That(rubble==null,Is.True);
            game.Paused=true;var feedback=game.GetComponent<CombatFeedback>();
            for(int i=0;i<100;i++)feedback.TowerStruck(attacked,true);
            yield return null;Assert.That(EffectRendererCount(feedback),Is.LessThanOrEqualTo(64));
            game.StartMatch();game.Paused=true;yield return null;yield return null;
            Assert.That(EffectRendererCount(feedback),Is.Zero,"Restart left stale siege effects");
            yield return new ExitPlayMode();
        }
        [UnityTest]
        public IEnumerator HitDefeatAndLeakCuesRespectPauseResetAndBudget()
        {
            EditorSceneManager.OpenScene("Assets/Game/Maps/MazeLab.unity");
            yield return new EnterPlayMode();yield return null;
            var game=Object.FindFirstObjectByType<Prototype>();game.StartMatch();game.Paused=true;game.SoundEnabled=false;
            var w=game.World;Assert.That(w.OrderBuild(16,14,out _),Is.True);
            for(int tick=0;tick<150;tick++)w.Step();
            var tower=w.Grid.At(16,14);Assert.That(tower,Is.Not.Null);
            var enemy=w.Spawn(new FrostMaze.Simulation.WaveSpec{Health=20,Speed=.05f},tower.Center+new FrostMaze.Simulation.V2(1.8f,0));
            Assert.That(enemy,Is.Not.Null);yield return null;yield return null;
            var crest=GameObject.Find("Enemy "+enemy.Id).transform.Find("Armored crawler/Signal crest").GetComponent<Renderer>();
            var normal=crest.sharedMaterial;w.Step();yield return null;
            Assert.That(enemy.Health,Is.LessThan(20));Assert.That(enemy.Health,Is.GreaterThan(0));
            var hit=crest.sharedMaterial;Assert.That(hit,Is.Not.SameAs(normal),"Actual damage needs visible feedback");
            yield return null;yield return null;Assert.That(crest.sharedMaterial,Is.SameAs(hit),"Paused hit flash changed");
            w.TowersFire=false;for(int tick=0;tick<4;tick++)w.Step();yield return null;
            Assert.That(crest.sharedMaterial,Is.SameAs(normal),"Hit flash did not recover on simulation time");
            w.TowersFire=true;for(int tick=0;tick<40&&w.Killed==0;tick++)w.Step();yield return null;yield return null;
            Assert.That(w.Killed,Is.EqualTo(1));var burst=GameObject.Find("Enemy defeated");Assert.That(burst,Is.Not.Null);
            Assert.That(burst.GetComponent<MeshFilter>().sharedMesh,Is.SameAs(game.Models.DefeatBurst));
            Assert.That(burst.GetComponentsInChildren<Collider>().Length,Is.Zero);
            var route=w.LaneRoute(0,true);var exit=w.Spawn(new FrostMaze.Simulation.WaveSpec{Flying=true},route[route.Length-1]);
            Assert.That(exit,Is.Not.Null);exit.Checkpoint=route.Length-1;yield return null;yield return null;
            w.Step();yield return null;yield return null;
            Assert.That(w.Leaked,Is.EqualTo(1));var leak=GameObject.Find("Enemy leaked");Assert.That(leak,Is.Not.Null);
            Assert.That(leak.GetComponent<LineRenderer>(),Is.Not.Null);Assert.That(leak.transform.position.y,Is.EqualTo(1.7f));
            var scale=burst.transform.localScale;var position=burst.transform.position;
            yield return new WaitForSecondsRealtime(.1f);
            Assert.That(burst.transform.localScale,Is.EqualTo(scale));Assert.That(burst.transform.position,Is.EqualTo(position));
            game.Paused=false;game.OpenSetup();yield return new WaitForSecondsRealtime(.1f);
            Assert.That(burst.transform.localScale,Is.EqualTo(scale),"Setup must pause cosmetic pulses");
            game.ReturnToMatch();yield return new WaitForSecondsRealtime(.5f);
            Assert.That(burst==null&&leak==null,Is.True,"Transient cues did not expire");
            game.Paused=true;w.TowersFire=false;
            var crowd=new System.Collections.Generic.List<FrostMaze.Simulation.Enemy>();
            for(int i=0;i<100;i++) {
                var e=w.Spawn(new FrostMaze.Simulation.WaveSpec{Flying=true,Health=1},new FrostMaze.Simulation.V2(4+i%10*.5f,4+i/10*.5f));
                Assert.That(e,Is.Not.Null);crowd.Add(e);
            }
            yield return null;yield return null;
            foreach(var e in crowd)e.Health=0;w.Step();yield return null;yield return null;
            var feedback=game.GetComponent<CombatFeedback>();Assert.That(EffectRendererCount(feedback),Is.EqualTo(64),"Defeats exceeded shared effect budget");
            exit=w.Spawn(new FrostMaze.Simulation.WaveSpec{Flying=true},route[route.Length-1]);exit.Checkpoint=route.Length-1;
            yield return null;yield return null;w.Step();yield return null;yield return null;
            Assert.That(GameObject.Find("Enemy leaked"),Is.Not.Null,"A full volley must not hide a leak");
            Assert.That(EffectRendererCount(feedback),Is.EqualTo(64),"Leak priority exceeded shared effect budget");
            game.StartMatch();game.Paused=true;yield return null;yield return null;
            Assert.That(EffectRendererCount(feedback),Is.Zero,"New match retained old defeat/leak cues");
            yield return new ExitPlayMode();
        }
        [UnityTest]
        public IEnumerator OffscreenLeakAlertAggregatesPausesExpiresAndFocusesExit()
        {
            EditorSceneManager.OpenScene("Assets/Game/Maps/MazeLab.unity");
            yield return new EnterPlayMode();yield return null;
            var game=Object.FindFirstObjectByType<Prototype>();game.StartMatch();game.Paused=true;game.SoundEnabled=false;
            var w=game.World;var feedback=game.GetComponent<CombatFeedback>();
            var route=w.LaneRoute(0,true);var end=route[route.Length-1];
            // Each unit exits before presentation has a chance to create its view.
            for(int i=0;i<2;i++) {
                var e=w.Spawn(new FrostMaze.Simulation.WaveSpec{Flying=true},end);Assert.That(e,Is.Not.Null);
                e.Checkpoint=route.Length-1;w.Step();
            }
            yield return null;yield return null;
            Assert.That(w.Leaked,Is.EqualTo(2));Assert.That(feedback.RecentLeaks,Is.EqualTo(2));
            Assert.That(GameObject.Find("Enemy leaked"),Is.Null,"Fixture should have no rendered removal to depend on");
            var camera=game.View.GetComponent<RtsCamera>();camera.Focus=new Vector3(5,0,50);game.View.orthographicSize=25;
            int gold=w.Gold,lives=w.Lives;game.FocusExit();yield return null;
            var groundExit=w.Config.GroundRoute[w.Config.GroundRoute.Length-1];
            Assert.That(camera.Focus,Is.EqualTo(new Vector3(groundExit.X,0,groundExit.Y)));
            Assert.That(game.View.orthographicSize,Is.EqualTo(11));Assert.That(game.Paused,Is.True);
            Assert.That(w.Gold,Is.EqualTo(gold));Assert.That(w.Lives,Is.EqualTo(lives));
            game.Speed=2;game.Paused=false;yield return new WaitForSecondsRealtime(2.1f);
            Assert.That(feedback.RecentLeaks,Is.EqualTo(2),"Double speed must not halve the UI alert duration");
            game.Paused=true;
            var another=w.Spawn(new FrostMaze.Simulation.WaveSpec{Flying=true},end);another.Checkpoint=route.Length-1;w.Step();
            yield return null;yield return null;Assert.That(feedback.RecentLeaks,Is.EqualTo(3));
            yield return new WaitForSecondsRealtime(4.1f);
            Assert.That(feedback.RecentLeaks,Is.EqualTo(3),"Pause expired the alert");
            game.Paused=false;game.OpenSetup();yield return new WaitForSecondsRealtime(4.1f);
            Assert.That(feedback.RecentLeaks,Is.EqualTo(3),"Setup expired the alert");
            game.ReturnToMatch();yield return new WaitForSecondsRealtime(4.1f);
            Assert.That(feedback.RecentLeaks,Is.Zero,"Alert failed to expire after real play time");
            game.Paused=true;another=w.Spawn(new FrostMaze.Simulation.WaveSpec{Flying=true},end);another.Checkpoint=route.Length-1;w.Step();
            yield return null;yield return null;Assert.That(feedback.RecentLeaks,Is.EqualTo(1),"Expired bursts must not accumulate forever");
            game.StartMatch();game.Paused=true;yield return null;yield return null;
            Assert.That(feedback.RecentLeaks,Is.Zero);Assert.That(game.World.Leaked,Is.Zero);
            yield return new ExitPlayMode();
        }
        [UnityTest]
        public IEnumerator CombatAudioLimitsBurstsFollowsCameraAndPrioritizesLeaks()
        {
            EditorSceneManager.OpenScene("Assets/Game/Maps/MazeLab.unity");
            yield return new EnterPlayMode();yield return null;
            var game=Object.FindFirstObjectByType<Prototype>();game.StartMatch();game.SoundEnabled=true;
            var w=game.World;var feedback=game.GetComponent<CombatFeedback>();var source=game.GetComponent<AudioSource>();
            var point=w.BuilderPosition;game.View.GetComponent<RtsCamera>().FocusPoint(point);
            yield return null;yield return null;
            long serial=0;
            void Shot(FrostMaze.Simulation.V2 p,bool chain=false,float splash=0) {
                w.Shots.Add(new FrostMaze.Simulation.ShotEvent{Serial=++serial,From=p,To=p+new FrostMaze.Simulation.V2(.5f,0),Chained=chain,Splash=splash});
            }
            for(int i=0;i<100;i++)Shot(point,false,i==99?1:0);
            yield return null;yield return null;
            Assert.That(feedback.SoundDispatches,Is.EqualTo(1),"A volley must not create one voice per shot");
            Assert.That(feedback.LastSound,Is.EqualTo("Original cannon"),"A visible impact should take priority within the volley");
            Assert.That(source.mute,Is.False);
            int before=feedback.SoundDispatches;float start=Time.realtimeSinceStartup;
            while(Time.realtimeSinceStartup-start<.5f){w.Shots.Clear();Shot(point);yield return null;}
            float elapsed=Time.realtimeSinceStartup-start;
            Assert.That(feedback.SoundDispatches-before,Is.InRange(1,Mathf.CeilToInt(elapsed/.12f)+1),"Weapon cadence depends on rendered frame rate");
            w.Shots.Clear();yield return new WaitForSecondsRealtime(.16f);before=feedback.SoundDispatches;
            Shot(new FrostMaze.Simulation.V2(1,63));yield return null;yield return null;
            Assert.That(feedback.SoundDispatches,Is.EqualTo(before),"Off-camera weapons should be quiet");
            Shot(point,true);yield return null;yield return null;
            Assert.That(feedback.SoundDispatches,Is.EqualTo(before),"Chain arcs must not multiply weapon voices");
            game.Paused=true;Shot(point);yield return null;yield return null;
            Assert.That(source.mute,Is.True);Assert.That(feedback.SoundDispatches,Is.EqualTo(before));
            game.Paused=false;yield return new WaitForSecondsRealtime(.16f);
            Assert.That(feedback.SoundDispatches,Is.EqualTo(before),"Unpausing replayed an old shot");
            game.OpenSetup();Shot(point);yield return null;yield return null;
            Assert.That(source.mute,Is.True);Assert.That(feedback.SoundDispatches,Is.EqualTo(before));
            game.ReturnToMatch();game.SoundEnabled=false;Shot(point);yield return null;yield return null;
            Assert.That(source.mute,Is.True);Assert.That(feedback.SoundDispatches,Is.EqualTo(before));
            game.SoundEnabled=true;yield return new WaitForSecondsRealtime(.16f);
            Assert.That(feedback.SoundDispatches,Is.EqualTo(before),"Unmuting replayed old shots");
            // Real exits outside the camera must still produce a single global warning.
            game.View.GetComponent<RtsCamera>().FocusPoint(new FrostMaze.Simulation.V2(8,50));
            var route=w.LaneRoute(0,true);
            for(int i=0;i<3;i++){var e=w.Spawn(new FrostMaze.Simulation.WaveSpec{Flying=true},route[route.Length-1]);e.Checkpoint=route.Length-1;w.Step();}
            Shot(new FrostMaze.Simulation.V2(8,50),false,1);yield return null;yield return null;
            Assert.That(w.Leaked,Is.EqualTo(3));Assert.That(feedback.SoundDispatches,Is.EqualTo(before+1));
            Assert.That(feedback.LastSound,Is.EqualTo("Original breach"),"Breach warning lost priority to weapon sound");
            game.StartMatch();game.Paused=true;yield return null;yield return null;
            Assert.That(feedback.SoundDispatches,Is.Zero);Assert.That(feedback.LastSound,Is.Null);Assert.That(source.mute,Is.True);
            yield return new ExitPlayMode();
        }
        [UnityTest]
        public IEnumerator MapLandmarksNeverCoverWalkableCells()
        {
            EditorSceneManager.OpenScene("Assets/Game/Maps/MazeLab.unity");
            yield return new EnterPlayMode();yield return null;
            foreach(string map in new[]{"Rimewatch","Ironfold"}) {
                var game=Object.FindFirstObjectByType<Prototype>();
                if(game.Map.name!=map){game.ChooseMap(Resources.Load<MapDefinition>(map));yield return null;yield return null;game=Object.FindFirstObjectByType<Prototype>();}
                game.Paused=true;
                var scenery=Object.FindFirstObjectByType<MapScenery>();Assert.That(scenery,Is.Not.Null);
                Assert.That(scenery.GetComponentsInChildren<Collider>().Length,Is.Zero,"Scenery cannot add physical blockers");
                Assert.That(scenery.PlantClusters,Is.InRange(1,90));Assert.That(scenery.Braziers,Is.InRange(1,24));
                var config=game.World.Config;float cell=config.LayoutCellSize;
                foreach(string batch in new[]{"Scenery 3","Scenery 4","Scenery 5","Scenery 11","Scenery 12","Scenery 13","Scenery 14","Scenery 15","Scenery 16","Scenery 17","Scenery 18"}) {
                    var prop=scenery.transform.Find(batch);
                    if(batch=="Scenery 12"||batch=="Scenery 13"||int.Parse(batch.Substring(8))>=14)Assert.That(prop,Is.Not.Null,"Map must retain its landmark silhouettes");
                    if(prop==null)continue; // Theme-specific trees/rocks are optional.
                    var mesh=prop.GetComponent<MeshFilter>().sharedMesh;var vertices=mesh.vertices;var triangles=mesh.triangles;
                    // Check the projected triangle bounds, not only endpoints: a bridge can have
                    // both ends on blocked cells while still hiding the playable corridor.
                    for(int i=0;i<triangles.Length;i+=3) {
                        Vector3 a=prop.TransformPoint(vertices[triangles[i]]),b=prop.TransformPoint(vertices[triangles[i+1]]),c=prop.TransformPoint(vertices[triangles[i+2]]);
                        int minX=Mathf.FloorToInt(Mathf.Min(a.x,b.x,c.x)/cell),maxX=Mathf.FloorToInt(Mathf.Max(a.x,b.x,c.x)/cell);
                        int minZ=Mathf.FloorToInt(Mathf.Min(a.z,b.z,c.z)/cell),maxZ=Mathf.FloorToInt(Mathf.Max(a.z,b.z,c.z)/cell);
                        for(int z=minZ;z<=maxZ;z++)for(int x=minX;x<=maxX;x++) {
                            int row=config.LayoutRows.Length-1-z;
                            Assert.That(row,Is.InRange(0,config.LayoutRows.Length-1),map+" landmark outside source mask");
                            Assert.That(x,Is.InRange(0,config.LayoutRows[row].Length-1),map+" landmark outside source mask");
                            Assert.That(config.WalkableSymbols.IndexOf(config.LayoutRows[row][x]),Is.LessThan(0),map+" landmark visually covers buildable cell "+x+","+z);
                        }
                    }
                }
            }
            // Restore the default map for subsequent scene-based integration cases.
            var last=Object.FindFirstObjectByType<Prototype>();last.ChooseMap(Resources.Load<MapDefinition>("Rimewatch"));yield return null;
            yield return new ExitPlayMode();
        }
        [UnityTest]
        public IEnumerator TowerFireFeedbackTracksShotsAndPauses()
        {
            EditorSceneManager.OpenScene("Assets/Game/Maps/MazeLab.unity");
            yield return new EnterPlayMode();yield return null;
            var game=Object.FindFirstObjectByType<Prototype>();game.StartMatch();game.Paused=true;
            Assert.That(game.World.OrderBuild(16,14,out _),Is.True);
            for(int i=0;i<150;i++)game.World.Step();
            yield return null;yield return null;
            var tower=game.World.Grid.At(16,14);
            var weapon=GameObject.Find("Tower "+tower.Id).transform.Find("Sentry weapon");
            Assert.That(weapon.localPosition,Is.EqualTo(Vector3.zero),"Idle tower must not replay old shots");
            Assert.That(game.World.StartWave(),Is.True);
            var enemy=game.World.Spawn(new FrostMaze.Simulation.WaveSpec{Health=500},tower.Center+new FrostMaze.Simulation.V2(1.8f,0));
            Assert.That(enemy,Is.Not.Null);game.World.Step();yield return null;
            Assert.That(enemy.Health,Is.LessThan(500),"Fixture must use a real combat hit");
            Assert.That(weapon.forward.x,Is.GreaterThan(.99f),"Weapon did not face its actual shot");
            Assert.That(weapon.localPosition.x,Is.LessThan(-.08f),"Missing firing recoil");
            var recoil=weapon.localPosition;var facing=weapon.localRotation;
            yield return null;yield return null;
            Assert.That(weapon.localPosition,Is.EqualTo(recoil));Assert.That(weapon.localRotation,Is.EqualTo(facing));
            game.Paused=false;game.OpenSetup();long tick=game.World.Tick;
            yield return null;yield return null;
            Assert.That(game.World.Tick,Is.EqualTo(tick));Assert.That(weapon.localPosition,Is.EqualTo(recoil));
            game.Paused=true;game.ReturnToMatch();game.World.TowersFire=false;
            for(int i=0;i<8;i++)game.World.Step();yield return null;
            Assert.That(weapon.localPosition,Is.EqualTo(Vector3.zero),"Recoil did not recover after simulation resumed");
            var shots=game.World.Shots;
            shots.Add(new FrostMaze.Simulation.ShotEvent{Serial=shots[shots.Count-1].Serial+1,From=tower.Center,To=tower.Center+new FrostMaze.Simulation.V2(0,3),Chained=true});
            yield return null;
            Assert.That(weapon.localRotation,Is.EqualTo(facing),"A chain bounce must not turn the firing tower");
            Assert.That(weapon.localPosition,Is.EqualTo(Vector3.zero));
            yield return new ExitPlayMode();
        }
        [UnityTest]
        public IEnumerator TowerRolesUpgradesAndCamera()
        {
            EditorSceneManager.OpenScene("Assets/Game/Maps/MazeLab.unity");
            yield return new EnterPlayMode();yield return null;
            var game=Object.FindFirstObjectByType<Prototype>();game.StartMatch();game.Paused=true;
            Assert.That(game.View.orthographicSize,Is.EqualTo(11));
            Assert.That(game.View.GetComponent<RtsCamera>().Focus.z,Is.EqualTo(game.World.BuilderPosition.Y));
            string[] roles={"Sentry","Wall","Control","Artillery","Interceptor"};
            string[] signatures={"Sentry weapon/Shard launcher","Cairn stone","Control weapon/Rime heart","Artillery weapon/Dark basin","Interceptor weapon/Aurora spire"};
            for(int i=0;i<5;i++) {
                game.World.SelectedDesign=i;
                Assert.That(game.World.OrderBuild(16+i,14,out _),Is.True);
                for(int tick=0;tick<180;tick++)game.World.Step();
            }
            yield return null;yield return null;
            for(int i=0;i<5;i++) {
                var tower=game.World.Grid.At(16+i,14);Assert.That(tower,Is.Not.Null);
                var view=GameObject.Find("Tower "+tower.Id).GetComponent<TowerView>();
                Assert.That(view.Role,Is.EqualTo(roles[i]));
                Assert.That(view.transform.Find(signatures[i]),Is.Not.Null,"Missing distinct Rime model");
                Assert.That(view.transform.Find("Foundation").GetComponent<MeshFilter>().sharedMesh,Is.SameAs(game.Models.Column),"Tower meshes must be shared, not allocated per tower");
                Assert.That(view.transform.Find("Foundation").GetComponent<Renderer>().enabled,Is.False,"Rigid source pieces must not double-render");
                Assert.That(view.transform.Find("Combined geometry 0").GetComponent<Renderer>().enabled,Is.True);
                int renderedTriangles=0,sourceTriangles=0;
                foreach(var filter in view.GetComponentsInChildren<MeshFilter>()) {
                    if(filter.name.StartsWith("Upgrade tier"))continue;
                    if(filter.name.StartsWith("Combined geometry"))renderedTriangles+=filter.sharedMesh.triangles.Length;
                    else sourceTriangles+=filter.sharedMesh.triangles.Length;
                }
                Assert.That(renderedTriangles,Is.EqualTo(sourceTriangles),"Batching must preserve every rigid triangle exactly once");
                Assert.That(view.GetComponentsInChildren<Collider>().Length,Is.Zero);
                Assert.That(view.transform.Find("Upgrade tier 2").gameObject.activeSelf,Is.False);
            }
            game.World.SelectedDesign=0;
            Assert.That(game.World.OrderBuild(21,14,out _),Is.True);
            for(int tick=0;tick<180;tick++)game.World.Step();
            yield return null;yield return null;
            var firstView=GameObject.Find("Tower "+game.World.Grid.At(16,14).Id).transform;
            var secondView=GameObject.Find("Tower "+game.World.Grid.At(21,14).Id).transform;
            var firstCombined=firstView.Find("Sentry weapon/Combined geometry 0").GetComponent<MeshFilter>().sharedMesh;
            Assert.That(secondView.Find("Sentry weapon/Combined geometry 0").GetComponent<MeshFilter>().sharedMesh,Is.SameAs(firstCombined),"Do not allocate combined geometry per instance");
            var warden=GameObject.Find("Builder drone");
            Assert.That(warden.transform.Find("Faction mantle"),Is.Not.Null);
            Assert.That(warden.GetComponentsInChildren<Collider>().Length,Is.Zero);
            Assert.That(warden.transform.Find("Hood").GetComponent<Renderer>().sharedMaterial,Is.Not.SameAs(warden.transform.Find("Faction mantle").GetComponent<Renderer>().sharedMaterial),"Faction tint must preserve the dark hood");
            var sentry=game.World.Grid.At(16,14);
            Assert.That(game.World.Upgrade(sentry.Id,out _),Is.True);
            yield return null;
            var upgraded=GameObject.Find("Tower "+sentry.Id).GetComponent<TowerView>();
            Assert.That(upgraded.VisibleLevel,Is.EqualTo(2));
            Assert.That(upgraded.transform.Find("Upgrade tier 2").gameObject.activeSelf,Is.True);
            Assert.That(upgraded.transform.Find("Upgrade tier 3").gameObject.activeSelf,Is.False);
            game.ShowNavigation=true;yield return null;
            Assert.That(upgraded.transform.localScale.y,Is.EqualTo(.08f));
            Assert.That(game.World.Grid.At(16,14),Is.SameAs(sentry));
            game.View.GetComponent<RtsCamera>().Overview();
            Assert.That(game.View.orthographicSize,Is.GreaterThan(11));
            game.SetupOptions.Factions[0]=1;game.StartMatch();game.Paused=true;
            string[] stoneSignatures={"Artillery weapon/Pebble hopper","Basalt slab","Artillery weapon/Quake monolith","Interceptor weapon/Sky cradle","Control weapon/Worldroot trunk"};
            for(int i=0;i<5;i++) {
                game.World.SelectedDesign=5+i;
                Assert.That(game.World.OrderBuild(16+i,14,out _),Is.True);
                for(int tick=0;tick<180;tick++)game.World.Step();
            }
            yield return null;yield return null;
            for(int i=0;i<5;i++) {
                var tower=game.World.Grid.At(16+i,14);Assert.That(tower,Is.Not.Null);
                var view=GameObject.Find("Tower "+tower.Id).GetComponent<TowerView>();
                Assert.That(view.transform.Find(stoneSignatures[i]),Is.Not.Null,"Missing Stonebound silhouette");
                Assert.That(view.GetComponentsInChildren<Collider>().Length,Is.Zero,"Scenery must not add physical blockers");
            }
            Assert.That(game.World.Gold,Is.EqualTo(959),"Models must retain actual paid Stonebound costs");
            game.SetupOptions.Factions[0]=2;game.StartMatch();game.Paused=true;game.ShowNavigation=false;
            string[] emberSignatures={"Sentry weapon/Cinder drum","Coal bunker","Artillery weapon/Furnace chimney","Interceptor weapon/Flare spear","Artillery weapon/Crucible bowl"};
            for(int i=0;i<5;i++) {
                game.World.SelectedDesign=10+i;
                Assert.That(game.World.OrderBuild(16+i,14,out _),Is.True);
                for(int tick=0;tick<180;tick++)game.World.Step();
            }
            yield return null;yield return null;
            for(int i=0;i<5;i++) {
                var tower=game.World.Grid.At(16+i,14);Assert.That(tower,Is.Not.Null);
                var view=GameObject.Find("Tower "+tower.Id).GetComponent<TowerView>();
                Assert.That(view.transform.Find(emberSignatures[i]),Is.Not.Null,"Missing Ember silhouette");
                Assert.That(view.GetComponentsInChildren<Collider>().Length,Is.Zero);
                Assert.That(game.World.Upgrade(tower.Id,out _),Is.True);
            }
            yield return null;
            for(int i=0;i<5;i++) {
                var tower=game.World.Grid.At(16+i,14);
                var view=GameObject.Find("Tower "+tower.Id).GetComponent<TowerView>();
                Assert.That(view.VisibleLevel,Is.EqualTo(2));
                Assert.That(view.transform.Find(emberSignatures[i]),Is.Not.Null,"Upgrade changed the model identity");
            }
            Assert.That(game.World.Gold,Is.EqualTo(710),"Five paid builds and upgrades must cost 490");
            game.SetupOptions.Factions[0]=3;game.StartMatch();game.Paused=true;
            string[] voltSignatures={"Sentry weapon/Cadet chest","Scrap barricade","Relay weapon/Induction ring","Interceptor weapon/Skyrail conductor","Sentry weapon/Marshal crest"};
            for(int i=0;i<5;i++) {
                game.World.SelectedDesign=15+i;
                Assert.That(game.World.OrderBuild(16+i,14,out _),Is.True);
                for(int tick=0;tick<180;tick++)game.World.Step();
            }
            yield return null;yield return null;
            for(int i=0;i<5;i++) {
                var tower=game.World.Grid.At(16+i,14);Assert.That(tower,Is.Not.Null);
                var view=GameObject.Find("Tower "+tower.Id).GetComponent<TowerView>();
                Assert.That(view.transform.Find(voltSignatures[i]),Is.Not.Null,"Missing Volt silhouette");
                Assert.That(view.GetComponentsInChildren<Collider>().Length,Is.Zero);
            }
            Assert.That(game.World.Gold,Is.EqualTo(956),"Volt paid roster must cost 244");
            Assert.That(firstCombined==null,Is.False,"Restarting the same map should reuse its model cache");
            game.ChooseMap(Resources.Load<MapDefinition>("Ironfold"));
            yield return null;yield return null;
            Assert.That(firstCombined==null,Is.True,"Unloading the map must dispose combined meshes");
            yield return new ExitPlayMode();
        }
        [UnityTest]
        public IEnumerator BuilderViewsAndMapSwitching()
        {
            EditorSceneManager.OpenScene("Assets/Game/Maps/MazeLab.unity");
            yield return new EnterPlayMode();
            yield return null;
            var game=Object.FindFirstObjectByType<Prototype>();
            Assert.That(game, Is.Not.Null);
            Assert.That(game.World.Config.Economy, Is.True);
            Assert.That(game.SetupOpen,Is.True);
            game.StartMatch();game.Paused=true;
            Assert.That(game.World.OrderBuild(16,14,out _),Is.True);
            for(int i=0;i<120;i++)game.World.Step();
            yield return null;
            Assert.That(game.World.Gold,Is.EqualTo(1180));
            Assert.That(GameObject.Find("Tower 1"),Is.Not.Null);
            var drone=GameObject.Find("Builder drone");
            Assert.That(drone,Is.Not.Null);
            Assert.That(drone.transform.position.x,Is.EqualTo(game.World.BuilderPosition.X).Within(.001f));
            game.SwitchMap(false);
            yield return null; yield return null;
            game=Object.FindFirstObjectByType<Prototype>();
            Assert.That(game,Is.Not.Null,"Map switch lost runtime bootstrap");
            Assert.That(game.World.Config.Economy,Is.False);
            game.Paused=true;game.DemoMaze();
            yield return null;
            Assert.That(game.World.Grid.Towers.Count,Is.GreaterThan(0));
            Assert.That(GameObject.Find("Builder drone"),Is.Null);
            game.SwitchMap(true);
            yield return null; yield return null;
            game=Object.FindFirstObjectByType<Prototype>();
            Assert.That(game.World.Config.Economy,Is.True);
            Assert.That(game.World.Gold,Is.EqualTo(1200));
            Assert.That(game.World.Grid.Towers.Count,Is.Zero);
            Assert.That(Object.FindObjectsByType<Prototype>(FindObjectsSortMode.None).Length,Is.EqualTo(1));
            game.SetupOptions.PlayerCount=2;game.SetupOptions.Difficulty=FrostMaze.Simulation.Difficulty.Hard;game.ChooseStart(0,3);game.StartMatch();game.Paused=true;
            yield return null;
            Assert.That(game.World.Players.Length,Is.EqualTo(2));
            Assert.That(game.World.Gold,Is.EqualTo(600));
            Assert.That(game.World.BuilderPosition.X,Is.EqualTo(game.World.Config.BuilderStarts[3].X));
            Assert.That(game.World.Config.Catalog.Length,Is.EqualTo(20));
            Assert.That(game.World.Difficulty,Is.EqualTo(FrostMaze.Simulation.Difficulty.Hard));
            Assert.That(GameObject.Find("Player 2 builder"),Is.Not.Null);
            var oldModel=game.Models.Column;
            game.ChooseMap(Resources.Load<MapDefinition>("Ironfold"));
            yield return null;yield return null;
            game=Object.FindFirstObjectByType<Prototype>();
            Assert.That(oldModel==null,Is.True,"Map switch leaked owned model meshes");
            Assert.That(GameObject.Find("Builder drone").transform.Find("Chassis"),Is.Not.Null);
            Assert.That(game.World.LaneCount,Is.EqualTo(4));
            Assert.That(game.World.Config.Waves.Length,Is.EqualTo(20));
            Assert.That(game.World.Config.Catalog[29].Spec.SlowFraction,Is.EqualTo(.4f));
            foreach(var design in game.World.Config.Catalog) Assert.That(design.Spec.Damage<=0||design.Spec.TargetsAir||design.Spec.TargetsGround, Is.True, design.Name+" has no targets in the packaged map");
            Assert.That(game.World.Config.Catalog[11].Spec.TargetsAir && !game.World.Config.Catalog[11].Spec.TargetsGround, Is.True);
            game.SetupOptions.Factions[0]=3;game.StartMatch();game.Paused=true;
            Assert.That(game.World.SelectedDesign,Is.EqualTo(21));
            int bx=-1,by=-1;
            for(int y=0;y<64&&bx<0;y++)for(int x=0;x<64&&bx<0;x++)
                if(FrostMaze.Simulation.V2.Distance(game.World.BuilderPosition,new FrostMaze.Simulation.V2(x+.5f,y+.5f))<2.5f&&game.World.CanBuild(x,y,out _)){bx=x;by=y;}
            Assert.That(bx,Is.GreaterThanOrEqualTo(0));
            Assert.That(game.World.Build(bx,by,out _),Is.True);
            yield return null;
            Assert.That(GameObject.Find("Tower "+game.World.Grid.Towers[0].Id).GetComponent<TowerView>().Role,Is.EqualTo("Sentry"));
            game.SetupOptions.Factions[0]=0;game.StartMatch();game.Paused=true;
            Assert.That(game.World.Build(bx,by,out _),Is.True);
            yield return null;yield return null;
            Assert.That(game.World.Grid.Towers.Count,Is.EqualTo(1));
            Assert.That(game.World.Grid.Towers[0].Design,Is.EqualTo(0));
            yield return new ExitPlayMode();
        }
        [UnityTest]
        public IEnumerator PulseModelsFollowPaidChampionProgression()
        {
            EditorSceneManager.OpenScene("Assets/Game/Maps/MazeLab.unity");
            yield return new EnterPlayMode();yield return null;
            var game=Object.FindFirstObjectByType<Prototype>();
            game.ChooseMap(Resources.Load<MapDefinition>("Ironfold"));
            yield return null;yield return null;
            game=Object.FindFirstObjectByType<Prototype>();game.StartMatch();game.Paused=true;
            game.World.SelectedDesign=6;
            Assert.That(game.World.OrderBuild(26,6,out _),Is.False,"Champion must remain locked before its six prerequisites");
            string[] paths={"Sentry weapon/Fuse barrel","Sentry weapon/Iron gauntlet","Artillery weapon/Shear blade","Sentry weapon/Ranger rifle","Interceptor weapon/Flare rocket pod","Control weapon/Cryo reservoir","Champion weapon/Echo crest"};
            for(int d=0;d<7;d++) {
                game.World.SelectedDesign=d;bool built=false;
                for(int y=6;y<20&&!built;y++)for(int x=26+d;x<45&&!built;x++)if(game.World.CanBuild(x,y,out _)) {
                    Assert.That(game.World.OrderBuild(x,y,out _),Is.True);
                    for(int tick=0;tick<300;tick++)game.World.Step();
                    Assert.That(game.World.Grid.At(x,y),Is.Not.Null);built=true;
                }
                Assert.That(built,Is.True,"No paid placement found");
            }
            yield return null;yield return null;
            Assert.That(game.World.Gold,Is.EqualTo(505));
            for(int i=0;i<7;i++) {
                var tower=game.World.Grid.Towers[i];var view=GameObject.Find("Tower "+tower.Id).GetComponent<TowerView>();
                Assert.That(tower.Design,Is.EqualTo(i));
                Assert.That(view.transform.Find(paths[i]),Is.Not.Null,"Missing distinct Pulse model");
                Assert.That(view.GetComponentsInChildren<Collider>().Length,Is.Zero);
            }
            var champion=game.World.Grid.Towers[6];
            Assert.That(game.World.Upgrade(champion.Id,out _),Is.True);
            yield return null;
            var championView=GameObject.Find("Tower "+champion.Id).GetComponent<TowerView>();
            Assert.That(championView.VisibleLevel,Is.EqualTo(2));
            Assert.That(championView.transform.Find(paths[6]),Is.Not.Null);
            Assert.That(game.World.Gold,Is.EqualTo(245));
            game.SetupOptions.Factions[0]=1;game.StartMatch();game.Paused=true;
            string[] blastPaths={"Sentry weapon/Alloy dome","Sentry weapon/Crash hammer","Artillery weapon/Gale vane","Sentry weapon/Heatkeeper boiler","Interceptor weapon/Quicksilver wing","Sentry weapon/Rootguard barrel","Champion weapon/Citadel keep"};
            for(int d=0;d<7;d++) {
                game.World.SelectedDesign=7+d;bool built=false;
                for(int y=6;y<20&&!built;y++)for(int x=26+d;x<45&&!built;x++)if(game.World.CanBuild(x,y,out _)) {
                    Assert.That(game.World.OrderBuild(x,y,out _),Is.True);
                    for(int tick=0;tick<300;tick++)game.World.Step();
                    Assert.That(game.World.Grid.At(x,y),Is.Not.Null);built=true;
                }
                Assert.That(built,Is.True);
            }
            yield return null;yield return null;
            for(int i=0;i<7;i++) {
                var tower=game.World.Grid.Towers[i];var view=GameObject.Find("Tower "+tower.Id).GetComponent<TowerView>();
                Assert.That(view.transform.Find(blastPaths[i]),Is.Not.Null,"Missing Blast model");
                Assert.That(view.GetComponentsInChildren<Collider>().Length,Is.Zero);
            }
            Assert.That(game.World.Gold,Is.EqualTo(505));
            var air=game.World.Grid.Towers[4];
            Assert.That(air.Spec.TargetsAir&&!air.Spec.TargetsGround&&air.Spec.SplashRadius>0,Is.True,"Quicksilver's aircraft model must retain air-only splash");
            game.SetupOptions.Factions[0]=2;game.StartMatch();game.Paused=true;
            string[] prismPaths={"Sentry weapon/Shade hood","Relay weapon/Spark core","Sentry weapon/Magnet bridge","Sentry weapon/Serpent head","Interceptor weapon/Twin sky lance","Sentry weapon/Needle rack","Champion weapon/Champion carapace"};
            for(int d=0;d<7;d++) {
                game.World.SelectedDesign=14+d;bool built=false;
                for(int y=6;y<20&&!built;y++)for(int x=26+d;x<45&&!built;x++)if(game.World.CanBuild(x,y,out _)) {
                    Assert.That(game.World.OrderBuild(x,y,out _),Is.True);
                    for(int tick=0;tick<300;tick++)game.World.Step();
                    Assert.That(game.World.Grid.At(x,y),Is.Not.Null);built=true;
                }
                Assert.That(built,Is.True);
            }
            yield return null;yield return null;
            for(int i=0;i<7;i++) {
                var tower=game.World.Grid.Towers[i];var view=GameObject.Find("Tower "+tower.Id).GetComponent<TowerView>();
                Assert.That(view.transform.Find(prismPaths[i]),Is.Not.Null,"Missing Prism model");
                Assert.That(view.GetComponentsInChildren<Collider>().Length,Is.Zero);
            }
            Assert.That(game.World.Gold,Is.EqualTo(505));
            Assert.That(game.World.Grid.Towers[1].Spec.ChainTargets,Is.EqualTo(3));
            Assert.That(game.World.Grid.Towers[4].Spec.TargetsGround,Is.False);
            var prismChampion=game.World.Grid.Towers[6];
            Assert.That(game.World.Upgrade(prismChampion.Id,out _),Is.True);
            yield return null;
            Assert.That(GameObject.Find("Tower "+prismChampion.Id).GetComponent<TowerView>().VisibleLevel,Is.EqualTo(2));
            Assert.That(game.World.Gold,Is.EqualTo(245));
            game.SetupOptions.Factions[0]=3;game.StartMatch();game.Paused=true;
            string[] horizonPaths={"Sentry weapon/Glimmer scope","Sentry weapon/Solar crown","Sentry weapon/Dust intake","Sentry weapon/Boneplate rib","Interceptor weapon/Sky harpoon","Sentry weapon/Warden drill","Champion weapon/Crawler hull"};
            for(int d=0;d<7;d++) {
                game.World.SelectedDesign=21+d;bool built=false;
                for(int y=6;y<20&&!built;y++)for(int x=26+d;x<45&&!built;x++)if(game.World.CanBuild(x,y,out _)) {
                    Assert.That(game.World.OrderBuild(x,y,out _),Is.True);
                    for(int tick=0;tick<300;tick++)game.World.Step();
                    Assert.That(game.World.Grid.At(x,y),Is.Not.Null);built=true;
                }
                Assert.That(built,Is.True);
            }
            yield return null;yield return null;
            for(int i=0;i<7;i++) {
                var tower=game.World.Grid.Towers[i];var view=GameObject.Find("Tower "+tower.Id).GetComponent<TowerView>();
                Assert.That(view.transform.Find(horizonPaths[i]),Is.Not.Null,"Missing Horizon model");
                Assert.That(view.GetComponentsInChildren<Collider>().Length,Is.Zero);
            }
            Assert.That(game.World.Gold,Is.EqualTo(505));
            Assert.That(game.World.Grid.Towers[0].Spec.Range,Is.EqualTo(5.5f));
            Assert.That(game.World.Grid.Towers[4].Spec.TargetsGround,Is.False);
            var horizonChampion=game.World.Grid.Towers[6];
            Assert.That(game.World.Upgrade(horizonChampion.Id,out _),Is.True);
            yield return null;
            Assert.That(GameObject.Find("Tower "+horizonChampion.Id).GetComponent<TowerView>().VisibleLevel,Is.EqualTo(2));
            Assert.That(game.World.Gold,Is.EqualTo(245));
            string[][] remainingPaths={
                new[]{"Sentry weapon/Gyro blade","Control weapon/Anchor hook","Artillery weapon/Starcaller orb","Sentry weapon/Wave resonator","Interceptor weapon/Sky electrode","Sentry weapon/Granite shoulder","Champion weapon/Eclipse rim"},
                new[]{"Sentry weapon/Strider backpack","Sentry weapon/Knight lance","Sentry weapon/Windkeeper vane","Sentry weapon/Bloom petal","Interceptor weapon/Whiteout missile","Control weapon/Hatchet blade","Champion weapon/Fossil skull"},
                new[]{"Sentry weapon/Junk crusher","Control weapon/Freeze prong","Artillery weapon/Splash pressure tank","Sentry weapon/Spring winding","Interceptor weapon/Dusk sky dart","Sentry weapon/Turbo rotor","Champion weapon/Champion mask"},
                new[]{"Sentry weapon/Grenade drum","Relay weapon/Kite sail","Artillery weapon/Jester cap","Control weapon/Aqua reservoir","Interceptor weapon/Keeper sky blade","Sentry weapon/Orbit satellite","Champion weapon/Verdant crown"}
            };
            for(int faction=4;faction<8;faction++) {
                game.SetupOptions.Factions[0]=faction;game.StartMatch();game.Paused=true;
                int spent=0;
                game.World.SelectedDesign=faction*7+6;
                Assert.That(game.World.OrderBuild(26,6,out _),Is.False,"Champion requires the complete faction roster");
                for(int d=0;d<7;d++) {
                    game.World.SelectedDesign=faction*7+d;bool built=false;
                    for(int y=6;y<20&&!built;y++)for(int x=26+d;x<45&&!built;x++)if(game.World.CanBuild(x,y,out _)) {
                        Assert.That(game.World.OrderBuild(x,y,out _),Is.True);
                        for(int tick=0;tick<300;tick++)game.World.Step();
                        Assert.That(game.World.Grid.At(x,y),Is.Not.Null);built=true;
                    }
                    Assert.That(built,Is.True);spent+=game.World.Config.Catalog[faction*7+d].Cost;
                }
                yield return null;yield return null;
                Assert.That(game.World.Gold,Is.EqualTo(1200-spent));
                for(int i=0;i<7;i++) {
                    var tower=game.World.Grid.Towers[i];var view=GameObject.Find("Tower "+tower.Id).GetComponent<TowerView>();
                    Assert.That(view.transform.Find(remainingPaths[faction-4][i]),Is.Not.Null,"Missing faction model");
                    Assert.That(view.GetComponentsInChildren<Collider>().Length,Is.Zero);
                }
                var antiAir=game.World.Grid.Towers[4];
                Assert.That(antiAir.Spec.TargetsAir&&!antiAir.Spec.TargetsGround,Is.True);
                var finalTower=game.World.Grid.Towers[6];int price=game.World.UpgradeCost(finalTower);
                Assert.That(game.World.Upgrade(finalTower.Id,out _),Is.True);
                yield return null;
                Assert.That(GameObject.Find("Tower "+finalTower.Id).GetComponent<TowerView>().VisibleLevel,Is.EqualTo(2));
                Assert.That(game.World.Gold,Is.EqualTo(1200-spent-price));
            }
            yield return new ExitPlayMode();
        }
        [UnityTest]
        public IEnumerator EnemyPresentationTracksSimulationAndResets()
        {
            EditorSceneManager.OpenScene("Assets/Game/Maps/MazeLab.unity");
            yield return new EnterPlayMode();
            yield return null;
            var game = Object.FindFirstObjectByType<Prototype>();
            game.StartMatch(); game.Paused = true;
            Assert.That(game.World.StartWave(), Is.True);
            var ground = game.World.Spawn(new FrostMaze.Simulation.WaveSpec(), game.World.LaneSpawn(0));
            var air = game.World.Spawn(new FrostMaze.Simulation.WaveSpec { Flying = true }, game.World.LaneSpawn(0));
            Assert.That(ground, Is.Not.Null); Assert.That(air, Is.Not.Null);
            // Relaxed difficulty reduces siege damage but must retain the heavy silhouette.
            var heavy=game.World.Spawn(new FrostMaze.Simulation.WaveSpec{Damage=21,Speed=1.55f},game.World.LaneSpawn(1),1);
            var runner=game.World.Spawn(new FrostMaze.Simulation.WaveSpec{Speed=2.7f},game.World.LaneSpawn(2),2);
            Assert.That(heavy,Is.Not.Null);Assert.That(runner,Is.Not.Null);
            ground.Velocity = new FrostMaze.Simulation.V2(1, 0);
            ground.SlowRemaining = 2;
            yield return null; yield return null;
            var groundView = GameObject.Find("Enemy " + ground.Id);
            var airView = GameObject.Find("Enemy " + air.Id);
            Assert.That(GameObject.Find("Enemy "+heavy.Id).transform.Find("Armored crawler/Siege shield"),Is.Not.Null);
            Assert.That(GameObject.Find("Enemy "+runner.Id).transform.Find("Armored crawler/Runner fin"),Is.Not.Null);
            Assert.That(groundView.transform.Find("Armored crawler"), Is.Not.Null);
            var wings = airView.transform.Find("Winged drifter/Left wing");
            Assert.That(wings, Is.Not.Null);
            Assert.That(groundView.transform.forward.x, Is.EqualTo(1).Within(.001f));
            Assert.That(groundView.transform.Find("Frost status").gameObject.activeSelf, Is.True);
            Assert.That(groundView.GetComponentsInChildren<Collider>().Length, Is.Zero, "Cosmetics must not add physics blockers");
            Assert.That(airView.GetComponentsInChildren<Collider>().Length, Is.Zero);
            Assert.That(GameObject.Find("Enemy "+heavy.Id).GetComponentsInChildren<Collider>().Length,Is.Zero);
            Assert.That(GameObject.Find("Enemy "+runner.Id).GetComponentsInChildren<Collider>().Length,Is.Zero);
            Assert.That(wings.Find("Wing vane").GetComponent<MeshFilter>().sharedMesh,Is.SameAs(game.Models.Wing(-1)));
            var foot=groundView.transform.Find("Armored crawler/Crawler foot");var pausedFoot=foot.localRotation;
            var pausedWing = wings.localRotation;
            var pausedBody = wings.parent.localPosition;
            yield return null; yield return null;
            Assert.That(wings.localRotation, Is.EqualTo(pausedWing), "Paused animation must use simulation time");
            Assert.That(wings.parent.localPosition, Is.EqualTo(pausedBody));
            Assert.That(foot.localRotation,Is.EqualTo(pausedFoot),"Paused walking animation moved");
            ground.SlowRemaining = 0;
            for (int step = 0; step < 6; step++) game.World.Step();
            yield return null;
            Assert.That(wings.localRotation, Is.Not.EqualTo(pausedWing));
            Assert.That(foot.localRotation,Is.Not.EqualTo(pausedFoot),"Walking animation did not follow simulation ticks");
            Assert.That(groundView.transform.Find("Frost status").gameObject.activeSelf, Is.False);
            Assert.That(airView.transform.position.y, Is.EqualTo(1.7f));
            Assert.That(groundView.transform.position.x, Is.EqualTo(ground.Position.X));
            game.StartMatch(); game.Paused = true;
            yield return null; yield return null;
            Assert.That(Object.FindObjectsByType<EnemyView>(FindObjectsSortMode.None).Length, Is.Zero, "New matches must remove all old enemy parts");
            yield return new ExitPlayMode();
        }
        [UnityTest]
        public IEnumerator CombatEffectsRespectFlightPauseAndBudget()
        {
            EditorSceneManager.OpenScene("Assets/Game/Maps/MazeLab.unity");
            yield return new EnterPlayMode(); yield return null;
            var game=Object.FindFirstObjectByType<Prototype>();game.StartMatch();game.Paused=true;game.SoundEnabled=false;
            game.World.Shots.Add(new FrostMaze.Simulation.ShotEvent{Serial=1,From=new FrostMaze.Simulation.V2(14,15),To=new FrostMaze.Simulation.V2(16,15),Flying=true,Splash=1});
            game.World.Shots.Add(new FrostMaze.Simulation.ShotEvent{Serial=2,From=new FrostMaze.Simulation.V2(16,15),To=new FrostMaze.Simulation.V2(17,15),Chained=true,FromFlying=true});
            yield return null; yield return null;
            var ring=GameObject.Find("Splash impact").GetComponent<LineRenderer>();
            var arc=GameObject.Find("Chain arc").GetComponent<LineRenderer>();
            Assert.That(ring.GetPosition(0).y,Is.EqualTo(1.7f));
            Assert.That(arc.GetPosition(0).y,Is.EqualTo(1.7f));
            Assert.That(arc.GetPosition(1).y,Is.EqualTo(.3f));
            yield return new WaitForSecondsRealtime(.3f);
            Assert.That(ring!=null&&arc!=null,Is.True,"Paused effects expired in real time");
            game.Paused=false;game.OpenSetup();
            yield return new WaitForSecondsRealtime(.3f);
            Assert.That(ring!=null&&arc!=null,Is.True,"Setup did not freeze combat effects");
            game.ReturnToMatch();
            yield return new WaitForSecondsRealtime(.35f);yield return null;
            Assert.That(ring==null&&arc==null,Is.True,"Effects failed to expire after resuming");
            game.StartMatch();game.Paused=true;
            // 63 beams followed by a splash must not allocate a 65th effect object.
            for(int i=1;i<=64;i++)game.World.Shots.Add(new FrostMaze.Simulation.ShotEvent{Serial=i,Splash=i==64?1:0});
            yield return null;yield return null;
            Assert.That(game.GetComponent<CombatFeedback>().GetComponentsInChildren<LineRenderer>().Length,Is.EqualTo(64));
            game.StartMatch();game.Paused=true;yield return null;yield return null;
            Assert.That(game.GetComponent<CombatFeedback>().GetComponentsInChildren<LineRenderer>().Length,Is.Zero,"New match retained old effects");
            yield return new ExitPlayMode();
        }
        [UnityTest]
        public IEnumerator SetupCanReturnAndNewMatchesClearInteraction()
        {
            EditorSceneManager.OpenScene("Assets/Game/Maps/MazeLab.unity");
            yield return new EnterPlayMode();yield return null;
            var game=Object.FindFirstObjectByType<Prototype>();
            Assert.That(game.CanReturnToMatch,Is.False);game.ReturnToMatch();Assert.That(game.SetupOpen,Is.True);
            game.StartMatch();game.Paused=true;
            Assert.That(game.World.OrderBuild(16,14,out _),Is.True);
            for(int i=0;i<150;i++)game.World.Step();
            Assert.That(game.World.Gold,Is.EqualTo(1180));
            Assert.That(game.World.StartWave(),Is.True);game.World.Step();
            var world=game.World;long tick=world.Tick;
            game.Paused=false;game.OpenSetup();game.SetupOptions.Factions[0]=1;
            yield return new WaitForSecondsRealtime(.2f);
            Assert.That(world.Tick,Is.EqualTo(tick),"Setup allowed hidden combat to continue");
            game.ReturnToMatch();game.Paused=true;yield return null;
            Assert.That(game.SetupOpen,Is.False);Assert.That(game.World,Is.SameAs(world));
            Assert.That(world.Gold,Is.EqualTo(1180));Assert.That(world.Grid.Towers.Count,Is.EqualTo(1));
            Assert.That(world.Players[0].Faction,Is.Zero,"Unconfirmed setup edits changed current faction");
            game.MoveMode=true;game.SellMode=true;game.SelectedId=42;game.SelectedTowerId=1;game.HasHover=true;
            game.StartMatch();game.Paused=true;
            Assert.That(game.World.Players[0].Faction,Is.EqualTo(1));
            Assert.That(game.SellMode||game.MoveMode||game.HasHover,Is.False);
            Assert.That(game.SelectedId+game.SelectedTowerId,Is.Zero);
            game.SellMode=true;game.MoveMode=true;game.SelectedId=42;
            game.ResetSimulation();game.Paused=true;
            Assert.That(game.SellMode||game.MoveMode,Is.False);Assert.That(game.SelectedId,Is.Zero);
            Assert.That(game.World.OrderBuild(16,14,out _),Is.True);
            game.SellMode=true;game.MoveMode=true;game.SelectedId=42;game.SelectedTowerId=1;
            game.CancelInteraction();
            Assert.That(game.World.QueuedBuilds,Is.Zero);Assert.That(game.SellMode||game.MoveMode,Is.False);
            Assert.That(game.SelectedId+game.SelectedTowerId,Is.Zero);
            yield return new ExitPlayMode();
        }
        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            if (UnityEditor.EditorApplication.isPlaying) yield return new ExitPlayMode();
        }
    }
}
#endif
