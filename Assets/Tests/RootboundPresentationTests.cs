#if UNITY_EDITOR
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEditor.SceneManagement;
using FrostMaze.Simulation;
namespace FrostMaze.Tests
{
    public sealed class RootboundPresentationTests
    {
        sealed class Still:ICameraInput { public CameraIntent Read()=>default; }
        [UnityTest]
        public IEnumerator PaidGroveKeepsRootsCellsPauseAndDistinctLivingAttacks()
        {
            EditorSceneManager.OpenScene("Assets/Game/Maps/MazeLab.unity");yield return new EnterPlayMode();yield return null;
            var game=Object.FindFirstObjectByType<Prototype>();game.ChooseMap(Resources.Load<MapDefinition>("Rimewatch"));yield return null;yield return null;
            game=Object.FindFirstObjectByType<Prototype>();game.SetupOptions.Factions[0]=1;game.StartMatch();game.Paused=true;game.MoveMode=true;
            var world=game.World;int gold=world.Gold;string mask=string.Join("\n",world.Config.LayoutRows);int cost=0;
            Assert.That(world.Config.Factions[1].Name,Is.EqualTo("Nature"));
            for(int i=0;i<5;i++){
                world.SelectedDesign=5+i;cost+=world.BuildCost;Assert.That(world.OrderBuild(16+i,14,out _),Is.True);
                for(int tick=0;tick<240;tick++)world.Step();
            }
            yield return null;yield return null;
            Assert.That(world.Grid.Towers.Count,Is.EqualTo(5));Assert.That(world.Gold,Is.EqualTo(gold-cost));Assert.That(cost,Is.EqualTo(228));
            var views=new TowerView[5];var crowns=new Transform[5];var arms=new Transform[5];var resting=new Quaternion[5];
            for(int i=0;i<5;i++){
                var tower=world.Grid.At(16+i,14);views[i]=GameObject.Find("Tower "+tower.Id).GetComponent<TowerView>();
                crowns[i]=views[i].transform.Find(views[i].Role+" weapon/Living crown");arms[i]=views[i].transform.Find(views[i].Role+" weapon/Throwing bough");
                Assert.That(crowns[i],Is.Not.Null);Assert.That(arms[i],Is.Not.Null);Assert.That(views[i].GetComponentsInChildren<Collider>().Length,Is.Zero);
                resting[i]=crowns[i].localRotation;
            }
            yield return new WaitForSecondsRealtime(.18f);
            for(int i=0;i<5;i++)Assert.That(crowns[i].localRotation,Is.EqualTo(resting[i]),"Paused foliage must freeze");
            for(int tick=0;tick<12;tick++)world.Step();yield return null;
            for(int i=0;i<5;i++)Assert.That(Quaternion.Angle(crowns[i].localRotation,resting[i]),Is.GreaterThan(.03f),"Living crown did not breathe");
            // Explicit later-game gold fixture for all five level-three geometry checks.
            // Opening construction above still uses the real starting wallet.
            world.Players[0].Gold+=1000;
            // All five upgrades and all actual animated vertices remain inside the occupied cell.
            foreach(var view in views){Assert.That(world.Upgrade(view.Subject.Id,out var reason),Is.True,reason);Assert.That(world.Upgrade(view.Subject.Id,out reason),Is.True,reason);}
            for(int tick=0;tick<40;tick++){
                world.Step();yield return null;
                foreach(var view in views)foreach(var filter in view.GetComponentsInChildren<MeshFilter>()){
                    if(!filter.GetComponent<Renderer>().enabled)continue;
                    var matrix=view.transform.worldToLocalMatrix*filter.transform.localToWorldMatrix;
                    foreach(var v in filter.sharedMesh.vertices){var local=matrix.MultiplyPoint3x4(v);Assert.That(new Vector2(local.x,local.z).magnitude,Is.LessThanOrEqualTo(.501f),view.Subject.Name+" crossed its cell");}
                }
            }
            for(int i=0;i<5;i++)resting[i]=arms[i].localRotation;
            Assert.That(world.StartWave(),Is.True);
            var ground=world.Spawn(new WaveSpec{Health=5000,Speed=0},new V2(18.5f,12.5f));
            var sky=world.Spawn(new WaveSpec{Health=5000,Speed=0,Flying=true},new V2(18.5f,12.5f));
            world.Step();yield return null;yield return null;
            Assert.That(ground.Health,Is.LessThan(5000));Assert.That(sky.Health,Is.LessThan(5000));
            for(int i=0;i<5;i++)if(i!=1)Assert.That(Quaternion.Angle(arms[i].localRotation,resting[i]),Is.GreaterThan(10),"Tree should throw with its bough");
            Assert.That(world.Shots.Exists(s=>s.Design==6),Is.False,"Oldbark is a living maze wall, not a free weapon");
            foreach(var cue in Object.FindObjectsByType<ProjectileCue>(FindObjectsSortMode.None))if(cue.Design>=5&&cue.Design<=9)
                Assert.That(cue.From.y,Is.EqualTo(ProjectileStyle.LaunchHeight(world.Config,cue.Design)),"Seeds must start at tree height");
            foreach(var view in views)foreach(var filter in view.GetComponentsInChildren<MeshFilter>()){
                if(!filter.GetComponent<Renderer>().enabled)continue;
                var matrix=view.transform.worldToLocalMatrix*filter.transform.localToWorldMatrix;
                foreach(var v in filter.sharedMesh.vertices){var local=matrix.MultiplyPoint3x4(v);Assert.That(new Vector2(local.x,local.z).magnitude,Is.LessThanOrEqualTo(.501f),view.Subject.Name+" throws outside its cell");}
            }
            for(int i=0;i<5;i++)resting[i]=arms[i].localRotation;
            yield return new WaitForSecondsRealtime(.15f);
            for(int i=0;i<5;i++)Assert.That(arms[i].localRotation,Is.EqualTo(resting[i]),"Paused throw moved");
            Assert.That(string.Join("\n",world.Config.LayoutRows),Is.EqualTo(mask));
            world.TowersFire=false;world.MoveBuilder(new V2(23,17));for(int tick=0;tick<240;tick++)world.Step();yield return null;
            game.Paused=false;yield return new WaitForSecondsRealtime(.7f);game.Paused=true;yield return null;
            var camera=game.View.GetComponent<RtsCamera>();camera.SetInput(new Still());camera.ResetRotation();camera.FocusPoint(new V2(18.5f,16));camera.SetZoom(5,true);yield return null;
            Capture(game.View,"/tmp/Howl-Rootbound-Paid-Close.png");
            camera.SetZoom(10,true);yield return null;Capture(game.View,"/tmp/Howl-Rootbound-Paid-Normal.png");
            yield return new ExitPlayMode();
        }
        static void Capture(Camera camera,string path)
        {
            var rt=new RenderTexture(1440,900,24);var old=RenderTexture.active;var texture=new Texture2D(1440,900,TextureFormat.RGB24,false);
            try{camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;texture.ReadPixels(new Rect(0,0,1440,900),0,0);texture.Apply();System.IO.File.WriteAllBytes(path,texture.EncodeToPNG());}
            finally{camera.targetTexture=null;RenderTexture.active=old;Object.DestroyImmediate(texture);Object.DestroyImmediate(rt);}
        }
    }
}
#endif
