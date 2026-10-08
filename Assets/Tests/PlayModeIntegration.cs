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
            game.ChooseMap(Resources.Load<MapDefinition>("Ironfold"));
            yield return null;yield return null;
            game=Object.FindFirstObjectByType<Prototype>();
            Assert.That(game.World.LaneCount,Is.EqualTo(4));
            game.SetupOptions.Factions[0]=3;game.StartMatch();game.Paused=true;
            Assert.That(game.World.SelectedDesign,Is.EqualTo(21));
            int bx=-1,by=-1;
            for(int y=0;y<64&&bx<0;y++)for(int x=0;x<64&&bx<0;x++)
                if(FrostMaze.Simulation.V2.Distance(game.World.BuilderPosition,new FrostMaze.Simulation.V2(x+.5f,y+.5f))<2.5f&&game.World.CanBuild(x,y,out _)){bx=x;by=y;}
            Assert.That(bx,Is.GreaterThanOrEqualTo(0));
            Assert.That(game.World.Build(bx,by,out _),Is.True);
            yield return null;
            Assert.That(GameObject.Find("Visored sentry"),Is.Not.Null);
            game.SetupOptions.Factions[0]=0;game.StartMatch();game.Paused=true;
            Assert.That(game.World.Build(bx,by,out _),Is.True);
            yield return null;yield return null;
            Assert.That(game.World.Grid.Towers.Count,Is.EqualTo(1));
            Assert.That(game.World.Grid.Towers[0].Design,Is.EqualTo(0));
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
            ground.Velocity = new FrostMaze.Simulation.V2(1, 0);
            ground.SlowRemaining = 2;
            yield return null; yield return null;
            var groundView = GameObject.Find("Enemy " + ground.Id);
            var airView = GameObject.Find("Enemy " + air.Id);
            Assert.That(groundView.transform.Find("Armored crawler"), Is.Not.Null);
            var wings = airView.transform.Find("Winged drifter/Left wing");
            Assert.That(wings, Is.Not.Null);
            Assert.That(groundView.transform.forward.x, Is.EqualTo(1).Within(.001f));
            Assert.That(groundView.transform.Find("Frost status").gameObject.activeSelf, Is.True);
            Assert.That(groundView.GetComponentsInChildren<Collider>().Length, Is.Zero, "Cosmetics must not add physics blockers");
            Assert.That(airView.GetComponentsInChildren<Collider>().Length, Is.Zero);
            var pausedWing = wings.localRotation;
            var pausedBody = wings.parent.localPosition;
            yield return null; yield return null;
            Assert.That(wings.localRotation, Is.EqualTo(pausedWing), "Paused animation must use simulation time");
            Assert.That(wings.parent.localPosition, Is.EqualTo(pausedBody));
            ground.SlowRemaining = 0;
            for (int step = 0; step < 6; step++) game.World.Step();
            yield return null;
            Assert.That(wings.localRotation, Is.Not.EqualTo(pausedWing));
            Assert.That(groundView.transform.Find("Frost status").gameObject.activeSelf, Is.False);
            Assert.That(airView.transform.position.y, Is.EqualTo(1.7f));
            Assert.That(groundView.transform.position.x, Is.EqualTo(ground.Position.X));
            game.StartMatch(); game.Paused = true;
            yield return null; yield return null;
            Assert.That(Object.FindObjectsByType<EnemyView>(FindObjectsSortMode.None).Length, Is.Zero, "New matches must remove all old enemy parts");
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
