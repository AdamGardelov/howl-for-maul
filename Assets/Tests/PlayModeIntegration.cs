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
            Assert.That(game.World.BuilderPosition.X,Is.EqualTo(31.5f));
            Assert.That(game.World.Config.Catalog.Length,Is.EqualTo(3));
            Assert.That(game.World.Difficulty,Is.EqualTo(FrostMaze.Simulation.Difficulty.Hard));
            Assert.That(GameObject.Find("Player 2 builder"),Is.Not.Null);
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
