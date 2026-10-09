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
        [UnityTest, Category("BuildFeedback")]
        public IEnumerator QueuedFootprintsTrackEveryDesignAndClearWithOrders()
        {
            EditorSceneManager.OpenScene("Assets/Game/Maps/MazeLab.unity");yield return new EnterPlayMode();yield return null;
            var game=Object.FindFirstObjectByType<Prototype>();game.StartMatch();game.Paused=true;
            var w=game.World;var view=game.GetComponent<BuildQueueView>();int gold=w.Gold;long tick=w.Tick;
            w.SelectedDesign=1;Assert.That(game.SubmitBuild(6,50,false),Is.True);
            w.SelectedDesign=0;Assert.That(game.SubmitBuild(7,50,true),Is.True);
            w.SelectedDesign=2;Assert.That(game.SubmitBuild(8,50,true),Is.True);
            view.Refresh();Assert.That(view.Orders.Count,Is.EqualTo(3));
            for(int i=0;i<3;i++){Assert.That(view.Orders[i].Number,Is.EqualTo(i+1));Assert.That(view.Orders[i].Origin.X,Is.EqualTo(6+i));}
            Assert.That(view.Orders[0].Design,Is.EqualTo(1));Assert.That(view.Orders[2].Design,Is.EqualTo(2));
            Assert.That(view.GetComponentsInChildren<Collider>().Length,Is.Zero);
            Assert.That(view.transform.Find("Current build footprints").GetComponent<MeshFilter>().sharedMesh.vertexCount,Is.EqualTo(16));
            Assert.That(view.transform.Find("Queued build footprints").GetComponent<MeshFilter>().sharedMesh.vertexCount,Is.EqualTo(32));
            game.ToggleMenu();view.Refresh();Assert.That(view.Orders.Count,Is.EqualTo(3));game.ToggleMenu();
            Assert.That(w.Gold,Is.EqualTo(gold));Assert.That(w.Tick,Is.EqualTo(tick));
            for(int i=0;i<1000&&w.QueuedBuilds==3;i++)w.Step();view.Refresh();
            Assert.That(view.Orders.Count,Is.EqualTo(2));Assert.That(view.Orders[0].Design,Is.EqualTo(0));Assert.That(view.Orders[0].Number,Is.EqualTo(1));
            game.CancelInteraction();view.Refresh();Assert.That(view.Orders.Count,Is.Zero);
            Assert.That(game.SubmitBuild(w.LaneSpawn(0).X,w.LaneSpawn(0).Y,false),Is.False);
            Assert.That(game.PlacementFailure,Is.Not.Null.And.Not.Empty);Assert.That(game.PlacementFailureUntil,Is.GreaterThan(Time.unscaledTime));
            game.StartMatch();view.Refresh();Assert.That(view.Orders.Count,Is.Zero);Assert.That(game.PlacementFailure,Is.Null);
            yield return new ExitPlayMode();
        }
        [UnityTest, Category("WaveForecast")]
        public IEnumerator PreparationForecastBlocksClicksOnlyWhileVisible()
        {
            EditorSceneManager.OpenScene("Assets/Game/Maps/MazeLab.unity");yield return new EnterPlayMode();yield return null;
            var game=Object.FindFirstObjectByType<Prototype>();Assert.That(game.ForecastHud,Is.EqualTo(Rect.zero));
            game.StartMatch();game.Paused=true;yield return null;
            var rect=game.ForecastHud;Assert.That(rect.width,Is.GreaterThan(0));Assert.That(rect.yMin,Is.GreaterThan(game.TopHud.yMax));
            Assert.That(rect.xMin,Is.GreaterThan(game.AlertHud.xMax));Assert.That(rect.yMax,Is.LessThan(game.BuildHud.yMin));
            Assert.That(game.PointerOverHud(rect.center),Is.True,"Forecast permits build-through");
            var gold=game.World.Gold;var tick=game.World.Tick;yield return null;
            Assert.That(game.World.Gold,Is.EqualTo(gold));Assert.That(game.World.Tick,Is.EqualTo(tick));
            game.DetailsOpen=true;Assert.That(game.ForecastHud,Is.EqualTo(Rect.zero));Assert.That(game.PointerOverHud(rect.center),Is.False);
            game.DetailsOpen=false;game.ToggleMenu();Assert.That(game.ForecastHud,Is.EqualTo(Rect.zero));game.ToggleMenu();
            Assert.That(game.World.StartWave(),Is.True);yield return null;
            Assert.That(game.ForecastHud,Is.EqualTo(Rect.zero));Assert.That(game.PointerOverHud(rect.center),Is.False,"Hidden forecast still blocks map input");
            yield return new ExitPlayMode();
        }
        static int EffectRendererCount(CombatFeedback feedback)
        {
            return feedback.transform.Find("Combat cues").GetComponentsInChildren<Renderer>().Length;
        }
        sealed class CameraInputFixture : ICameraInput
        {
            public CameraIntent Intent;
            public CameraIntent Read()=>Intent;
        }
        [UnityTest, Category("DepthPresentation"), Category("FollowThrough")]
        public IEnumerator TowerPortraitsCacheActualModelsWithoutChangingMatch()
        {
            EditorSceneManager.OpenScene("Assets/Game/Maps/MazeLab.unity");
            yield return new EnterPlayMode();yield return null;
            foreach(string map in new[]{"Rimewatch","Ironfold"}) {
                var game=Object.FindFirstObjectByType<Prototype>();
                if(game.Map.name!=map){game.ChooseMap(Resources.Load<MapDefinition>(map));yield return null;yield return null;game=Object.FindFirstObjectByType<Prototype>();}
                game.StartMatch();game.Paused=true;
                var world=game.World;int gold=world.Gold,towers=world.Grid.Towers.Count;long tick=world.Tick;
                var minimap=new RenderedMinimap();minimap.Prepare(game);var terrainImage=minimap.Texture;
                Assert.That(terrainImage,Is.Not.Null);Assert.That(terrainImage.width,Is.EqualTo(512));
                var shades=new System.Collections.Generic.HashSet<Color32>(terrainImage.GetPixels32());
                Assert.That(shades.Count,Is.GreaterThan(64),"Minimap is blank or only a flat mask");
                minimap.Prepare(game);Assert.That(minimap.Texture,Is.SameAs(terrainImage),"Scenery snapshot regenerated");
                var cache=new TowerPortraits();Texture2D first=null;
                for(int i=0;i<world.Config.Catalog.Length;i++) {
                    cache.Prepare(game,i);var image=cache.Get(i);
                    Assert.That(image,Is.Not.Null,map+" design "+i);Assert.That(image.width,Is.EqualTo(160));
                    cache.Prepare(game,i);Assert.That(cache.Get(i),Is.SameAs(image),"Portrait regenerated");
                    if(first==null)first=image;
                    yield return null;
                }
                Assert.That(world.Gold,Is.EqualTo(gold));Assert.That(world.Grid.Towers.Count,Is.EqualTo(towers));
                Assert.That(world.Tick,Is.EqualTo(tick));Assert.That(world.QueuedBuilds,Is.Zero);
                Assert.That(GameObject.Find("Tower portrait preview"),Is.Null,"Temporary preview leaked");
                Assert.That(GameObject.Find("Tower portrait camera"),Is.Null,"Temporary camera leaked");
                cache.Dispose();minimap.Dispose();yield return null;Assert.That(first==null,Is.True,"Cached texture leaked");Assert.That(terrainImage==null,Is.True,"Scenery texture leaked");
            }
            Object.FindFirstObjectByType<Prototype>().ChooseMap(Resources.Load<MapDefinition>("Rimewatch"));yield return null;
            yield return new ExitPlayMode();
        }
        [UnityTest, Category("DepthPresentation"), Category("FollowThrough")]
        public IEnumerator CameraDragRejectsUiOriginsAndFreezesInSetup()
        {
            EditorSceneManager.OpenScene("Assets/Game/Maps/MazeLab.unity");
            yield return new EnterPlayMode();yield return null;
            var game=Object.FindFirstObjectByType<Prototype>();game.StartMatch();game.Paused=true;
            var camera=game.View.GetComponent<RtsCamera>();var input=new CameraInputFixture();camera.SetInput(input);
            camera.Overview();
            var bottomPoint=game.View.WorldToScreenPoint(new Vector3(game.World.Config.Width*.5f,0,0));
            var topPoint=game.View.WorldToScreenPoint(new Vector3(game.World.Config.Width*.5f,0,game.World.Config.Height));
            Assert.That(game.PointerOverHud(new Vector2(bottomPoint.x,Screen.height-bottomPoint.y)),Is.False,"Overview exit is covered by HUD");
            Assert.That(bottomPoint.y,Is.GreaterThan(0),"Overview exit is off screen");
            Assert.That(game.BuildHud.width,Is.LessThan(340*game.UiScale));
            Assert.That(game.PointerOverHud(game.SelectionHud.center),Is.False,"Empty centre still blocks world clicks");
            Assert.That(game.MinimapRect.xMax,Is.LessThan(Screen.width*.5f),"Minimap belongs on the left");
            Assert.That(game.BuildHud.xMin,Is.GreaterThan(Screen.width*.5f),"Command grid belongs on the right");
            Assert.That(Screen.height-topPoint.y,Is.GreaterThan(game.TopHud.yMax),"Overview top is covered by status bar");
            Assert.That(game.View.rect,Is.EqualTo(new Rect(0,0,1,1)));Assert.That(game.Sidebar,Is.EqualTo(Rect.zero));
            Assert.That(game.PointerOverHud(game.TopHud.center),Is.True);Assert.That(game.PointerOverHud(game.BuildHud.center),Is.True);
            Assert.That(game.PointerOverHud(new Vector2(50,Screen.height*.5f)),Is.False,"Hidden sidebar still blocks the map");
            game.Paused=false;game.ToggleMenu();long tick=game.World.Tick;var menuFocus=camera.Focus;
            input.Intent=new CameraIntent{Pointer=Vector2.one*400,Pan=Vector2.one,Zoom=5,Rotate=1};
            yield return null;yield return null;yield return null;
            Assert.That(camera.Yaw,Is.Zero,"Menu allowed rotation");Assert.That(game.World.Tick,Is.EqualTo(tick),"Menu failed to pause simulation");Assert.That(camera.Focus,Is.EqualTo(menuFocus));
            Assert.That(game.PointerOverHud(Vector2.zero),Is.True,"Modal menu permits world input");
            game.ToggleMenu();Assert.That(game.Paused,Is.False,"Closing menu changed prior pause state");
            game.Paused=true;game.ToggleMenu();game.ToggleMenu();Assert.That(game.Paused,Is.True);
            game.DetailsOpen=true;Assert.That(game.Sidebar.width,Is.GreaterThan(0));
            Assert.That(game.PointerOverHud(new Vector2(Screen.width*.5f,Screen.height-40)),Is.False,"Hidden tower controls still block map in Details view");
            game.DetailsOpen=false;
            input.Intent=new CameraIntent{Pointer=new Vector2(Screen.width*.5f,Screen.height*.5f),Rotate=1};
            for(int i=0;i<12;i++)yield return null;
            Assert.That(camera.Yaw,Is.GreaterThan(0),"Rotation input ignored");
            camera.FocusPoint(new FrostMaze.Simulation.V2(32,32));var rotatedFocus=camera.Focus;var screenRight=camera.transform.right;
            input.Intent=new CameraIntent{Pointer=new Vector2(Screen.width*.5f,Screen.height*.5f),Dragging=true,DragStarted=true};yield return null;
            input.Intent=new CameraIntent{Pointer=new Vector2(Screen.width*.5f,Screen.height*.5f),Dragging=true,Drag=new Vector2(80,0)};yield return null;
            var dragged=camera.Focus-rotatedFocus;
            Assert.That(Vector3.Dot(dragged,screenRight),Is.GreaterThan(0),"Rotated drag went the wrong way");
            Assert.That(Vector3.Cross(dragged,screenRight).magnitude,Is.LessThan(.01f),"Drag no longer follows screen horizontal");
            camera.Overview();
            for(int x=0;x<=1;x++)for(int z=0;z<=1;z++) {
                var corner=game.View.WorldToViewportPoint(new Vector3(x*game.World.Config.Width,0,z*game.World.Config.Height));
                Assert.That(corner.x,Is.InRange(0f,1f));Assert.That(corner.y,Is.InRange(0f,1f));
            }
            camera.ResetRotation();Assert.That(camera.Yaw,Is.Zero);

            camera.FocusPoint(new FrostMaze.Simulation.V2(32,32));var start=camera.Focus;
            var ui=new Vector2(game.TopHud.center.x,Screen.height-game.TopHud.center.y);var world=new Vector2(Screen.width*.6f,Screen.height*.5f);
            input.Intent=new CameraIntent{Pointer=ui,Dragging=true,DragStarted=true};yield return null;
            input.Intent=new CameraIntent{Pointer=world,Dragging=true,Drag=new Vector2(100,50)};yield return null;
            Assert.That(camera.Focus,Is.EqualTo(start),"Drag starting over compact HUD leaked into camera");
            input.Intent=new CameraIntent{Pointer=world};yield return null;
            input.Intent=new CameraIntent{Pointer=world,Dragging=true,DragStarted=true};yield return null;
            Assert.That(camera.Focus,Is.EqualTo(start),"First drag frame jumped");
            camera.GroundPoint(world+new Vector2(100,50),out var grabbed);camera.GroundPoint(world,out var underPointer);
            input.Intent=new CameraIntent{Pointer=world,Dragging=true,Drag=new Vector2(100,50)};yield return null;
            Assert.That(Vector3.Distance(camera.Focus-start,grabbed-underPointer),Is.LessThan(.01f),"Perspective drag lost its ground anchor");
            input.Intent=new CameraIntent{Pointer=world};yield return null;
            game.OpenSetup();start=camera.Focus;float zoom=game.View.orthographicSize;
            input.Intent=new CameraIntent{Pointer=world,Pan=Vector2.one,Zoom=5};yield return null;yield return null;
            Assert.That(camera.Focus,Is.EqualTo(start));Assert.That(game.View.orthographicSize,Is.EqualTo(zoom));
            game.ReturnToMatch();input.Intent=new CameraIntent{Pointer=world,Dragging=true,DragStarted=true};yield return null;
            input.Intent=new CameraIntent{Pointer=world,Dragging=true,Drag=Vector2.one*100000};for(int i=0;i<20;i++)yield return null;
            Assert.That(camera.Focus.x,Is.EqualTo(camera.BoundsMax.x));Assert.That(camera.Focus.z,Is.EqualTo(camera.BoundsMax.y));
            Assert.That(game.World.Gold,Is.EqualTo(1200));Assert.That(game.World.Grid.Towers.Count,Is.Zero);
            input.Intent=new CameraIntent{Pointer=world};yield return null;
            camera.FocusPoint(new FrostMaze.Simulation.V2(32,32));
            float previousPitch=0;var scenery=Object.FindFirstObjectByType<MapScenery>();var terrainPosition=scenery.transform.position;var terrainRotation=scenery.transform.rotation;
            foreach(float zoomLevel in new[]{5f,11f,24f}) {
                camera.SetZoom(zoomLevel,true);
                Assert.That(game.View.orthographic,Is.False,"Close inspection requires actual perspective");
                Assert.That(camera.Pitch,Is.GreaterThan(previousPitch));previousPitch=camera.Pitch;
                foreach(var ground in new[]{new Vector3(31.5f,0,31.5f),new Vector3(33,0,33),camera.Focus}) {
                    var screen=game.View.WorldToScreenPoint(ground);
                    Assert.That(camera.GroundPoint(screen,out var hit),Is.True);
                    Assert.That(Vector3.Distance(hit,ground),Is.LessThan(.002f),"Placement ray drifted at zoom "+zoomLevel);
                }
                for(int x=0;x<2;x++)for(int y=0;y<2;y++)Assert.That(camera.GroundPoint(new Vector2(x*Screen.width,y*Screen.height),out _),Is.True,"Viewport crossed horizon");
            }
            camera.SetZoom(5);float previousZoom=camera.Zoom;
            for(int i=0;i<20;i++) {
                yield return null;Assert.That(camera.Zoom,Is.InRange(5f,previousZoom));previousZoom=camera.Zoom;
            }
            Assert.That(camera.Zoom,Is.LessThan(24),"Smooth zoom did not progress");
            Assert.That(scenery.transform.position,Is.EqualTo(terrainPosition));Assert.That(scenery.transform.rotation,Is.EqualTo(terrainRotation));
            camera.FocusPoint(new FrostMaze.Simulation.V2(32,32));camera.Overview();
            yield return new ExitPlayMode();
        }
        [UnityTest, Category("OnlineClient")]
        public IEnumerator RemoteClientChangesMapKeepsItsSlotAndUsesHostTicks()
        {
            EditorSceneManager.OpenScene("Assets/Game/Maps/MazeLab.unity");yield return new EnterPlayMode();yield return null;
            FrostMaze.Simulation.Scenario Resolve(string name)=>JsonUtility.FromJson<FrostMaze.Simulation.Scenario>(JsonUtility.ToJson(Resources.Load<MapDefinition>(name).Settings));
            using(var host=new FrostMaze.Simulation.Online.Session(Resolve,FrostMaze.Simulation.Online.StateDigest.Scenario)){
                host.Host("Ironfold","Host","fixture",0);var online=OnlineGame.Create();online.Join("127.0.0.1",host.Port,"Client","fixture");var client=online.Session;
                for(int i=0;i<600&&(host.Members.Count<2||!client.IsConnected||Object.FindFirstObjectByType<Prototype>().Map.name!="Ironfold");i++){host.Update(Time.unscaledDeltaTime);yield return null;}
                var game=Object.FindFirstObjectByType<Prototype>();Assert.That(game.Map.name,Is.EqualTo("Ironfold"));Assert.That(OnlineGame.Current,Is.SameAs(online));Assert.That(client.LocalSlot,Is.EqualTo(1));
                void Send(FrostMaze.Simulation.Online.Session session,FrostMaze.Simulation.Online.Kind kind,int value=0)=>session.Send(new FrostMaze.Simulation.Online.Packet{Kind=kind,A=value});
                Send(host,FrostMaze.Simulation.Online.Kind.Ready);Send(client,FrostMaze.Simulation.Online.Kind.Ready);
                for(int i=0;i<60;i++){host.Update(Time.unscaledDeltaTime);yield return null;}
                Send(host,FrostMaze.Simulation.Online.Kind.Begin);
                for(int i=0;i<60;i++){host.Update(Time.unscaledDeltaTime);yield return null;}
                Send(host,FrostMaze.Simulation.Online.Kind.Faction,0);Send(client,FrostMaze.Simulation.Online.Kind.Faction,3);Send(host,FrostMaze.Simulation.Online.Kind.Ready);Send(client,FrostMaze.Simulation.Online.Kind.Ready);
                for(int i=0;i<60;i++){host.Update(Time.unscaledDeltaTime);yield return null;}
                Send(host,FrostMaze.Simulation.Online.Kind.Lane,0);Send(client,FrostMaze.Simulation.Online.Kind.Lane,6);Send(host,FrostMaze.Simulation.Online.Kind.Ready);Send(client,FrostMaze.Simulation.Online.Kind.Ready);
                for(int i=0;i<60;i++){host.Update(Time.unscaledDeltaTime);yield return null;}
                Send(host,FrostMaze.Simulation.Online.Kind.Difficulty,1);Send(client,FrostMaze.Simulation.Online.Kind.Difficulty,1);
                for(int i=0;i<120&&client.World==null;i++){host.Update(Time.unscaledDeltaTime);yield return null;}
                yield return null;Assert.That(game.World,Is.SameAs(client.World));Assert.That(game.World.ActivePlayer,Is.EqualTo(1));Assert.That(game.World.Players[1].Faction,Is.EqualTo(3));Assert.That(game.World.Gold,Is.EqualTo(600));Assert.That(game.World.BuilderPosition,Is.EqualTo(game.World.Config.BuilderStarts[6]));
                for(int i=0;i<10;i++)yield return null;long tick=game.World.Tick;yield return new WaitForSecondsRealtime(.1f);Assert.That(game.World.Tick,Is.EqualTo(tick),"Client advanced without host frames");
                game.ToggleMenu();for(int i=0;i<30;i++){host.Update(Time.unscaledDeltaTime);yield return null;}Assert.That(game.World.Tick,Is.GreaterThan(tick),"Client menu paused the shared match");
                Send(host,FrostMaze.Simulation.Online.Kind.PauseVote);game.VotePause();for(int i=0;i<30;i++){host.Update(Time.unscaledDeltaTime);yield return null;}Assert.That(host.Paused&&game.Paused,Is.True);Assert.That(client.Failure,Is.Empty);
                game.LeaveOnline();yield return null;Assert.That(OnlineGame.Current,Is.Null);
            }
            yield return new ExitPlayMode();
        }
        [UnityTest, Category("OnlineSetup")]
        public IEnumerator SoloStagedSetupUsesChosenStartMusicAndAuthoritativeTicks()
        {
            EditorSceneManager.OpenScene("Assets/Game/Maps/MazeLab.unity");yield return new EnterPlayMode();yield return null;
            var game=Object.FindFirstObjectByType<Prototype>();game.BeginSolo();var net=OnlineGame.Current.Session;
            Assert.That(net.Stage,Is.EqualTo(FrostMaze.Simulation.Online.Stage.Factions));
            net.Send(new FrostMaze.Simulation.Online.Packet{Kind=FrostMaze.Simulation.Online.Kind.Faction,A=2});net.Send(new FrostMaze.Simulation.Online.Packet{Kind=FrostMaze.Simulation.Online.Kind.Ready});
            Assert.That(net.Stage,Is.EqualTo(FrostMaze.Simulation.Online.Stage.Lanes));
            net.Send(new FrostMaze.Simulation.Online.Packet{Kind=FrostMaze.Simulation.Online.Kind.Lane,A=4});net.Send(new FrostMaze.Simulation.Online.Packet{Kind=FrostMaze.Simulation.Online.Kind.Ready});
            Assert.That(net.Stage,Is.EqualTo(FrostMaze.Simulation.Online.Stage.Difficulty));
            net.Send(new FrostMaze.Simulation.Online.Packet{Kind=FrostMaze.Simulation.Online.Kind.Difficulty,A=1});yield return null;yield return null;
            Assert.That(game.World,Is.SameAs(net.World));Assert.That(game.World.Players[0].Faction,Is.EqualTo(2));
            Assert.That(game.World.BuilderPosition,Is.EqualTo(game.World.Config.BuilderStarts[4]));Assert.That(game.World.Gold,Is.EqualTo(1200));
            Assert.That(game.World.LaneCount,Is.EqualTo(3));Assert.That(game.SetupOpen,Is.False);
            Assert.That(game.Speed,Is.EqualTo(1));game.SetSpeedIndex(3);Assert.That(game.Speed,Is.EqualTo(3));
            long fastStart=game.World.Tick;yield return new WaitForSecondsRealtime(.25f);Assert.That(game.World.Tick-fastStart,Is.GreaterThan(8));
            game.VotePause();long stopped=game.World.Tick;game.SetSpeedIndex(0);yield return new WaitForSecondsRealtime(.15f);
            Assert.That(game.World.Tick,Is.EqualTo(stopped));Assert.That(game.Speed,Is.EqualTo(.5f));game.VotePause();
            game.ChangeSpeed(1);Assert.That(game.Speed,Is.EqualTo(1));Assert.That(Time.timeScale,Is.EqualTo(1));

            var music=Object.FindFirstObjectByType<MapMusic>();var source=music.GetComponent<AudioSource>();
            Assert.That(source.pitch,Is.EqualTo(1));Assert.That(source.clip,Is.Not.Null);Assert.That(source.clip.length,Is.GreaterThan(60));Assert.That(source.clip.loadType,Is.EqualTo(AudioClipLoadType.Streaming));
            game.MusicVolume=0;yield return new WaitForSecondsRealtime(1);Assert.That(source.volume,Is.Zero);game.MusicVolume=.5f;yield return new WaitForSecondsRealtime(.3f);Assert.That(source.volume,Is.GreaterThan(0));
            game.VotePause();yield return null;long tick=game.World.Tick;yield return new WaitForSecondsRealtime(.15f);Assert.That(game.World.Tick,Is.EqualTo(tick));
            game.VotePause();yield return new WaitForSecondsRealtime(.15f);Assert.That(game.World.Tick,Is.GreaterThan(tick));
            tick=game.World.Tick;game.ToggleMenu();yield return new WaitForSecondsRealtime(.15f);Assert.That(game.World.Tick,Is.EqualTo(tick),"Solo menu must pause");
            game.ToggleMenu();game.LeaveOnline();yield return null;Assert.That(OnlineGame.Current,Is.Null);Assert.That(game.SetupOpen,Is.True);
            yield return new ExitPlayMode();
        }
        [UnityTest, Category("WorldAtmosphere")]
        public IEnumerator ExteriorAndSoundIdentityPreservePlayfield()
        {
            EditorSceneManager.OpenScene("Assets/Game/Maps/MazeLab.unity");yield return new EnterPlayMode();yield return null;
            var game=Object.FindFirstObjectByType<Prototype>();game.StartMatch();game.Paused=true;
            var camera=game.View.GetComponent<RtsCamera>();var input=new CameraInputFixture();camera.SetInput(input);
            camera.FocusPoint(new FrostMaze.Simulation.V2(22,29));camera.SetZoom(8,true);
            input.Intent=new CameraIntent{Pointer=new Vector2(Screen.width*.5f,Screen.height*.5f),Rotate=1};yield return null;yield return null;
            input.Intent=default;var focus=camera.Focus;float zoom=camera.Zoom;game.ResetView();
            Assert.That(camera.Yaw,Is.Zero);Assert.That(camera.Focus,Is.EqualTo(focus));Assert.That(camera.Zoom,Is.EqualTo(zoom));
            foreach(var name in new[]{"Rimewatch","Ironfold"}){
                if(game.Map.name!=name){game.ChooseMap(Resources.Load<MapDefinition>(name));yield return null;yield return null;game=Object.FindFirstObjectByType<Prototype>();}
                var backdrop=Object.FindFirstObjectByType<WorldBackdrop>();Assert.That(backdrop,Is.Not.Null);
                Assert.That(backdrop.GetComponentsInChildren<Collider>().Length,Is.Zero);
                foreach(var filter in backdrop.GetComponentsInChildren<MeshFilter>()){
                    Assert.That(filter.gameObject.layer,Is.Not.EqualTo(30),"Exterior must not pollute minimap");
                    var mesh=filter.sharedMesh;var vertices=mesh.vertices;var triangles=mesh.triangles;
                    for(int i=0;i<triangles.Length;i+=3){var a=vertices[triangles[i]];var b=vertices[triangles[i+1]];var c=vertices[triangles[i+2]];
                        Assert.That(Mathf.Max(a.x,b.x,c.x)<=0||Mathf.Min(a.x,b.x,c.x)>=game.World.Config.Width||Mathf.Max(a.z,b.z,c.z)<=0||Mathf.Min(a.z,b.z,c.z)>=game.World.Config.Height,Is.True,"Exterior crosses playable rectangle");}
                }
                using(var bank=new TowerSoundBank(game.World.Config)){
                    var hashes=new System.Collections.Generic.HashSet<long>();
                    for(int i=0;i<game.World.Config.Catalog.Length;i++){var clip=bank.Get(i);Assert.That(bank.Get(i),Is.SameAs(clip));var samples=new float[clip.samples];clip.GetData(samples,0);long hash=17;float peak=0;
                        foreach(float sample in samples){Assert.That(float.IsNaN(sample),Is.False);peak=Mathf.Max(peak,Mathf.Abs(sample));unchecked{hash=hash*31+Mathf.RoundToInt(sample*100000);}}
                        Assert.That(peak,Is.InRange(.01f,.9f));Assert.That(hashes.Add(hash),Is.True,"Repeated tower sound");}
                    Assert.That(bank.Count,Is.EqualTo(game.World.Config.Catalog.Length));
                }
            }
            yield return new ExitPlayMode();
        }
        [UnityTest, Category("CrowdedPresentation")]
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
        [UnityTest, Category("CrowdedPresentation")]
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
        [UnityTest, Category("CrowdedPresentation")]
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
        [UnityTest, Category("DepthPresentation"), Category("FollowThrough")]
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
                foreach(string batch in new[]{"Scenery 3","Scenery 4","Scenery 5","Scenery 11","Scenery 12","Scenery 13","Scenery 14","Scenery 15","Scenery 16","Scenery 17","Scenery 18","Scenery 19"}) {
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
        [UnityTest, Category("CrowdedPresentation")]
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
        [UnityTest, Category("CrowdedPresentation")]
        public IEnumerator OffscreenVolleysKeepVisibleBeamsAndEdgeSplashes()
        {
            EditorSceneManager.OpenScene("Assets/Game/Maps/MazeLab.unity");
            yield return new EnterPlayMode();yield return null;
            var game=Object.FindFirstObjectByType<Prototype>();game.StartMatch();game.Paused=true;game.SoundEnabled=false;
            var camera=game.View.GetComponent<RtsCamera>();camera.FocusPoint(new FrostMaze.Simulation.V2(32,32));camera.SetZoom(5,true);camera.enabled=false;
            yield return null;yield return null;
            var w=game.World;var feedback=game.GetComponent<CombatFeedback>();int gold=w.Gold,lives=w.Lives;long tick=w.Tick,serial=0;
            FrostMaze.Simulation.V2 At(float x,float y,float height) {
                var ray=game.View.ViewportPointToRay(new Vector3(x,y,0));
                var plane=new Plane(Vector3.up,new Vector3(0,height,0));Assert.That(plane.Raycast(ray,out float distance),Is.True);
                var hit=ray.GetPoint(distance);return new FrostMaze.Simulation.V2(hit.x,hit.z);
            }
            void Shot(FrostMaze.Simulation.V2 from,FrostMaze.Simulation.V2 to,bool chain=false,float splash=0) {
                w.Shots.Add(new FrostMaze.Simulation.ShotEvent{Serial=++serial,From=from,To=to,Chained=chain,Splash=splash});
            }
            var centre=At(.5f,.5f,.3f);var off=centre+new FrostMaze.Simulation.V2(10000,0);
            for(int i=0;i<100;i++)Shot(off,off+new FrostMaze.Simulation.V2(1,0));
            Shot(centre,centre+new FrostMaze.Simulation.V2(.5f,0));yield return null;yield return null;
            Assert.That(EffectRendererCount(feedback),Is.EqualTo(1),"Unseen volley consumed the visible shot budget");
            var beam=feedback.transform.Find("Combat cues").GetComponentInChildren<LineRenderer>();
            Assert.That(beam.GetPosition(1).x,Is.EqualTo(centre.X+.5f).Within(.001f));
            // Moving the camera must not replay the hundred already-consumed events.
            var cameraPosition=game.View.transform.position;game.View.transform.position+=new Vector3(10000,0,0);
            yield return null;yield return null;Assert.That(EffectRendererCount(feedback),Is.EqualTo(1));game.View.transform.position=cameraPosition;
            var left=At(-.1f,.5f,.3f);var right=At(1.1f,.5f,.3f);
            Shot(left,right,true);yield return null;yield return null;
            Assert.That(EffectRendererCount(feedback),Is.EqualTo(2),"Beam crossing the screen was culled with its endpoints");
            var outside=At(1.15f,.5f,.12f);float radius=FrostMaze.Simulation.V2.Distance(outside,At(.96f,.5f,.12f));
            Shot(outside,outside,false,radius);yield return null;yield return null;
            Assert.That(EffectRendererCount(feedback),Is.EqualTo(4),"Offscreen impact reaching the viewport was hidden");
            foreach(var line in feedback.transform.Find("Combat cues").GetComponentsInChildren<LineRenderer>()) {
                Assert.That(line.shadowCastingMode,Is.EqualTo(UnityEngine.Rendering.ShadowCastingMode.Off));
                Assert.That(line.receiveShadows,Is.False);
            }
            yield return new WaitForSecondsRealtime(.15f);Assert.That(EffectRendererCount(feedback),Is.EqualTo(4),"Pause expired combat visuals");
            for(int i=0;i<100;i++)Shot(centre,centre+new FrostMaze.Simulation.V2(.5f,0));yield return null;yield return null;
            Assert.That(EffectRendererCount(feedback),Is.EqualTo(64),"Onscreen volley exceeded the shared cap");
            Assert.That(feedback.GetComponentsInChildren<Collider>().Length,Is.Zero);
            Assert.That(w.Gold,Is.EqualTo(gold));Assert.That(w.Lives,Is.EqualTo(lives));Assert.That(w.Tick,Is.EqualTo(tick));
            game.StartMatch();game.Paused=true;yield return null;yield return null;
            Assert.That(EffectRendererCount(feedback),Is.Zero,"New match kept old volley visuals");
            yield return new ExitPlayMode();
        }
        [UnityTest, Category("CrowdedPresentation")]
        public IEnumerator ReusedViewBuffersKeepSalesDeathsAndRestartAccurate()
        {
            EditorSceneManager.OpenScene("Assets/Game/Maps/MazeLab.unity");
            yield return new EnterPlayMode();yield return null;
            var game=Object.FindFirstObjectByType<Prototype>();
            if(game.Map.name!="Rimewatch") {game.ChooseMap(Resources.Load<MapDefinition>("Rimewatch"));yield return null;yield return null;game=Object.FindFirstObjectByType<Prototype>();}
            game.StartMatch();game.Paused=true;game.SoundEnabled=false;var w=game.World;w.TowersFire=false;
            Assert.That(w.OrderBuild(16,14,out _),Is.True);for(int i=0;i<150;i++)w.Step();
            Assert.That(w.OrderBuild(16,16,out _),Is.True);for(int i=0;i<150;i++)w.Step();
            Assert.That(w.Grid.Towers.Count,Is.EqualTo(2));var sold=w.Grid.At(16,14);var kept=w.Grid.At(16,16);
            var removed=w.Spawn(new FrostMaze.Simulation.WaveSpec{Flying=true,Health=100},new FrostMaze.Simulation.V2(20,20));
            var survivor=w.Spawn(new FrostMaze.Simulation.WaveSpec{Flying=true,Health=100},new FrostMaze.Simulation.V2(22,20));
            Assert.That(removed,Is.Not.Null);Assert.That(survivor,Is.Not.Null);yield return null;yield return null;
            var method=typeof(Prototype).GetMethod("SyncViewsProfiled",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance);
            var sync=(System.Action)System.Delegate.CreateDelegate(typeof(System.Action),game,method);
            for(int i=0;i<20;i++)sync();
            long before=System.GC.GetAllocatedBytesForCurrentThread();for(int i=0;i<100;i++)sync();
            long allocated=System.GC.GetAllocatedBytesForCurrentThread()-before;
            Assert.That(allocated,Is.Zero,"Warmed stable view updates should not allocate managed memory");
            Assert.That(w.Sell(16,14),Is.True);removed.Health=0;w.Step();yield return null;yield return null;
            Assert.That(GameObject.Find("Tower "+sold.Id),Is.Null);Assert.That(GameObject.Find("Tower "+kept.Id),Is.Not.Null);
            Assert.That(GameObject.Find("Enemy "+removed.Id),Is.Null);Assert.That(GameObject.Find("Enemy "+survivor.Id),Is.Not.Null);
            Assert.That(Object.FindObjectsByType<TowerView>(FindObjectsSortMode.None).Length,Is.EqualTo(1));
            Assert.That(Object.FindObjectsByType<EnemyView>(FindObjectsSortMode.None).Length,Is.EqualTo(1));
            game.ResetSimulation();game.Paused=true;yield return null;yield return null;
            Assert.That(Object.FindObjectsByType<TowerView>(FindObjectsSortMode.None).Length,Is.Zero);
            Assert.That(Object.FindObjectsByType<EnemyView>(FindObjectsSortMode.None).Length,Is.Zero);
            Assert.That(game.World.OrderBuild(16,14,out _),Is.True);for(int i=0;i<150;i++)game.World.Step();yield return null;yield return null;
            Assert.That(Object.FindObjectsByType<TowerView>(FindObjectsSortMode.None).Length,Is.EqualTo(1),"Reused model IDs lost their new views");
            // The active builder is reused when a new match chooses another faction.
            game.SetupOptions.Factions[0]=1;game.StartMatch();game.Paused=true;yield return null;yield return null;
            var builder=typeof(Prototype).GetField("builder",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).GetValue(game) as GameObject;
            foreach(var renderer in builder.GetComponentsInChildren<Renderer>())if(renderer.name.StartsWith("Faction"))
                Assert.That(renderer.sharedMaterial,Is.SameAs(game.TowerPalette(1)[1]),"Cached builder tint ignored a new faction");
            yield return new ExitPlayMode();
        }
        [UnityTest, Category("FollowThrough")]
        public IEnumerator HealthBarsPrioritizeSelectionDeclutterAndRevealOnDemand()
        {
            EditorSceneManager.OpenScene("Assets/Game/Maps/MazeLab.unity");yield return new EnterPlayMode();yield return null;
            var game=Object.FindFirstObjectByType<Prototype>();
            if(game.Map.name!="Rimewatch"){game.ChooseMap(Resources.Load<MapDefinition>("Rimewatch"));yield return null;yield return null;game=Object.FindFirstObjectByType<Prototype>();}
            game.StartMatch();game.Paused=true;game.SoundEnabled=false;var w=game.World;w.TowersFire=false;
            Assert.That(w.OrderBuild(16,14,out _),Is.True);for(int i=0;i<150;i++)w.Step();
            var tower=w.Grid.At(16,14);Assert.That(tower,Is.Not.Null);
            var camera=game.View.GetComponent<RtsCamera>();camera.FocusPoint(tower.Center);camera.SetZoom(Mathf.Max(11,Screen.height/60f),true);camera.enabled=false;
            var hud=game.GetComponent<PrototypeHud>();game.SelectedTowerId=tower.Id;yield return null;
            hud.PrepareHealthBars(false);Assert.That(hud.HealthBars.Count,Is.EqualTo(1),"Selected healthy tower needs a bar");
            Assert.That(hud.HealthBars[0].Selected,Is.True);Assert.That(hud.HealthBars[0].TowerId,Is.EqualTo(tower.Id));
            game.SelectedTowerId=0;tower.Health*=.5f;
            FrostMaze.Simulation.Enemy selected=null;
            for(int i=0;i<8;i++) {
                var e=w.Spawn(new FrostMaze.Simulation.WaveSpec{Flying=true,Health=100},tower.Center+new FrostMaze.Simulation.V2(-2+i*.55f,1.5f));
                Assert.That(e,Is.Not.Null);e.Health=50;if(i==0)selected=e;
            }
            var healthy=w.Spawn(new FrostMaze.Simulation.WaveSpec{Flying=true,Health=100},tower.Center+new FrostMaze.Simulation.V2(3,0));Assert.That(healthy,Is.Not.Null);
            game.SelectedId=selected.Id;yield return null;hud.PrepareHealthBars(false);
            Assert.That(hud.HealthBars[0].EnemyId,Is.EqualTo(selected.Id));Assert.That(hud.HealthBars[0].Selected,Is.True);
            Assert.That(hud.SuppressedHealthBars,Is.GreaterThanOrEqualTo(1),"Crowd bars overlap instead of being decluttered");
            foreach(var bar in hud.HealthBars)Assert.That(bar.EnemyId,Is.Not.EqualTo(healthy.Id),"Healthy unselected unit should stay quiet");
            for(int i=0;i<hud.HealthBars.Count;i++)for(int j=i+1;j<hud.HealthBars.Count;j++)Assert.That(hud.HealthBars[i].Rect.Overlaps(hud.HealthBars[j].Rect),Is.False);
            hud.PrepareHealthBars(true);Assert.That(hud.HealthBars.Count,Is.EqualTo(10),"Reveal must include healthy and overlapping units");
            Assert.That(hud.HealthBars[0].EnemyId,Is.EqualTo(selected.Id),"Reveal lost selected unit priority");
            foreach(var bar in hud.HealthBars) {
                Assert.That(bar.Rect.Overlaps(game.TopHud)||bar.Rect.Overlaps(game.MinimapRect)||bar.Rect.Overlaps(game.BuildHud),Is.False);
                Assert.That(bar.Fraction,Is.InRange(0,1));
            }
            for(int i=0;i<20;i++)hud.PrepareHealthBars(false);
            long before=System.GC.GetAllocatedBytesForCurrentThread();for(int i=0;i<100;i++)hud.PrepareHealthBars(false);
            Assert.That(System.GC.GetAllocatedBytesForCurrentThread()-before,Is.Zero,"Stable health layout allocated per frame");
            camera.FocusPoint(new FrostMaze.Simulation.V2(50,50));hud.PrepareHealthBars(true);
            Assert.That(hud.HealthBars.Count,Is.Zero,"Offscreen health bars leaked over the HUD");
            Assert.That(w.Grid.Towers.Count,Is.EqualTo(1));Assert.That(w.Enemies.Count,Is.EqualTo(9));
            yield return new ExitPlayMode();
        }
        [UnityTest, Category("InspectionPicking")]
        public IEnumerator PerspectiveInspectionPicksAirAndGroundModelsWithoutBuilding()
        {
            EditorSceneManager.OpenScene("Assets/Game/Maps/MazeLab.unity");yield return new EnterPlayMode();yield return null;
            var game=Object.FindFirstObjectByType<Prototype>();
            if(game.Map.name!="Rimewatch"){game.ChooseMap(Resources.Load<MapDefinition>("Rimewatch"));yield return null;yield return null;game=Object.FindFirstObjectByType<Prototype>();}
            game.StartMatch();game.Paused=true;game.SoundEnabled=false;var world=game.World;world.TowersFire=false;
            Assert.That(world.OrderBuild(16,14,out _),Is.True);for(int i=0;i<150;i++)world.Step();
            var tower=world.Grid.At(16,14);Assert.That(tower,Is.Not.Null);
            var air=world.Spawn(new FrostMaze.Simulation.WaveSpec{Flying=true},tower.Center);Assert.That(air,Is.Not.Null);
            var ground=world.Spawn(new FrostMaze.Simulation.WaveSpec(),tower.Center+new FrostMaze.Simulation.V2(2,0));Assert.That(ground,Is.Not.Null);
            var camera=game.View.GetComponent<RtsCamera>();var input=new CameraInputFixture();camera.SetInput(input);camera.FocusPoint(tower.Center);
            var scenery=Object.FindFirstObjectByType<MapScenery>();var position=scenery.transform.position;var rotation=scenery.transform.rotation;
            int gold=world.Gold;long tick=world.Tick;
            for(int angle=0;angle<2;angle++) {
                foreach(float zoom in new[]{5f,11f,24f}) {
                    camera.SetZoom(zoom,true);
                    var airScreen=game.View.WorldToScreenPoint(new Vector3(air.Position.X,1.7f,air.Position.Y));
                    var groundScreen=game.View.WorldToScreenPoint(new Vector3(ground.Position.X,ground.Spec.Radius*.65f,ground.Position.Y));
                    Assert.That(game.EnemyAtScreenPoint(airScreen),Is.EqualTo(air.Id),"Flying model over a tower missed at zoom "+zoom);
                    Assert.That(game.EnemyAtScreenPoint(groundScreen),Is.EqualTo(ground.Id),"Ground model missed at zoom "+zoom);
                    Assert.That(game.EnemyAtScreenPoint(new Vector2(-1,-1)),Is.Zero);
                    if(angle==0&&zoom==5) {
                        Assert.That(camera.GroundPoint(airScreen,out var floor),Is.True);
                        Assert.That(Vector2.Distance(new Vector2(floor.x,floor.z),new Vector2(air.Position.X,air.Position.Y)),Is.GreaterThan(1),"Fixture must expose the former ground-plane miss");
                    }
                }
                input.Intent=new CameraIntent{Pointer=new Vector2(Screen.width*.5f,Screen.height*.5f),Rotate=1};
                for(int i=0;i<12;i++)yield return null;
                input.Intent=default;
            }
            Assert.That(camera.Yaw,Is.GreaterThan(0));Assert.That(scenery.transform.position,Is.EqualTo(position));Assert.That(scenery.transform.rotation,Is.EqualTo(rotation));
            Assert.That(world.Gold,Is.EqualTo(gold));Assert.That(world.Tick,Is.EqualTo(tick));Assert.That(world.Grid.Towers.Count,Is.EqualTo(1));Assert.That(world.QueuedBuilds,Is.Zero);
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
