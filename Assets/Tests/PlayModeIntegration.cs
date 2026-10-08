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
                Assert.That(view.GetComponentsInChildren<Collider>().Length,Is.Zero);
                Assert.That(view.transform.Find("Upgrade tier 2").gameObject.activeSelf,Is.False);
            }
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
