#if UNITY_EDITOR
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEditor.SceneManagement;
using FrostMaze.Simulation;
using Object=UnityEngine.Object;
namespace FrostMaze.Tests
{
    public sealed class ConstructionHitchTests
    {
        [Serializable] public class Sample { public string Map;public int Towers,Enemies;public double StepMilliseconds,ViewMilliseconds;public long GeometryChecks; }
        [Serializable] public class Report { public string Scope="Paid live-wave construction, Unity Mono, software render; CPU timings are not hardware FPS";public List<Sample> Samples=new List<Sample>(); }
        [UnityTest,Category("ConstructionHitch"),Timeout(300000)]
        public IEnumerator PaidConstructionDuringLiveWavesKeepsRoutesAndViewsWorking()
        {
            EditorSceneManager.OpenScene("Assets/Game/Maps/MazeLab.unity");yield return new EnterPlayMode();yield return null;
            var report=new Report();
            foreach(var name in new[]{"Ironfold","Rimewatch"}) {
                Object.FindFirstObjectByType<Prototype>().ChooseMap(Resources.Load<MapDefinition>(name));yield return null;yield return null;
                var game=Object.FindFirstObjectByType<Prototype>();game.SetupOptions.AutomaticWaves=false;game.StartMatch();game.Paused=true;game.SoundEnabled=false;game.MoveMode=true;
                var w=game.World;int initial=name=="Ironfold"?48:8,edits=name=="Ironfold"?8:3;
                var sites=new List<V2>();
                // Reserve valid, separated cells before dispatching the paid builder.
                for(int y=5;y<28;y+=3)for(int x=3;x<61;x+=3)if(w.CanBuild(x,y,out _))sites.Add(new V2(x,y));
                Assert.That(sites.Count,Is.GreaterThan(initial+edits));
                for(int i=0;i<initial;i++) {
                    var p=sites[i];Assert.That(w.OrderBuild(p.X,p.Y,out string reason),Is.True,reason);
                    int ticks=0;while(w.HasBuildOrder&&ticks++<1000)w.Step();
                    Assert.That(w.Grid.At(p.X,p.Y),Is.Not.Null);
                }
                yield return null;yield return null;
                Assert.That(w.StartWave(),Is.True);for(int i=0;i<240;i++)w.Step();yield return null;
                Assert.That(w.Enemies.Count,Is.GreaterThan(8));
                var sync=(Action)Delegate.CreateDelegate(typeof(Action),game,typeof(Prototype).GetMethod("SyncViewsProfiled",BindingFlags.Instance|BindingFlags.NonPublic));
                var watch=new System.Diagnostics.Stopwatch();
                for(int i=initial;i<initial+edits;i++) {
                    var p=sites[i];Assert.That(w.WaveActive,Is.True);
                    int gold=w.Gold,cost=w.BuildCost,earned=w.TotalGoldEarned;
                    Assert.That(w.OrderBuild(p.X,p.Y,out string reason),Is.True,reason);
                    int ticks=0;Sample sample=null;
                    while(w.HasBuildOrder&&ticks++<1000) {
                        int version=w.Grid.Version;long geometry=w.Navigation.GeometryChecks;
                        watch.Restart();w.Step();watch.Stop();double elapsed=watch.Elapsed.TotalMilliseconds;
                        if(version!=w.Grid.Version) {
                            sample=new Sample{Map=name,Towers=w.Grid.Towers.Count,Enemies=w.Enemies.Count,StepMilliseconds=elapsed,GeometryChecks=w.Navigation.GeometryChecks-geometry};
                            watch.Restart();sync();watch.Stop();sample.ViewMilliseconds=watch.Elapsed.TotalMilliseconds;
                        }
                    }
                    Assert.That(sample,Is.Not.Null,"Paid order did not finish during the wave");
                    Assert.That(w.Grid.At(p.X,p.Y),Is.Not.Null);
                    Assert.That(w.Gold,Is.EqualTo(gold-cost+w.TotalGoldEarned-earned),"Construction charge plus earned combat gold differs");
                    Assert.That(sample.GeometryChecks,Is.LessThan(20000),"Construction rechecked whole-map collision geometry");
                    foreach(var enemy in w.Enemies)if(!enemy.Spec.Flying)Assert.That(w.Grid.Clear(enemy.Position,enemy.Position,enemy.Spec.Radius),Is.True,"Live edit clipped an enemy");
                    report.Samples.Add(sample);yield return null;
                }
                var rendered=new HashSet<int>();
                foreach(var view in Object.FindObjectsByType<TowerView>(FindObjectsSortMode.None))if(view.Subject.Id>0) {
                    Assert.That(ReferenceEquals(view.Subject,w.Grid.Find(view.Subject.Id)),Is.True);
                    Assert.That(rendered.Add(view.Subject.Id),Is.True,"Duplicate live tower view");
                }
                Assert.That(rendered.Count,Is.EqualTo(w.Grid.Towers.Count));
            }
            string folder=Path.Combine(Application.dataPath,"../Logs/ConstructionHitch");Directory.CreateDirectory(folder);
            File.WriteAllText(Path.Combine(folder,"report.json"),JsonUtility.ToJson(report,true));
            yield return new ExitPlayMode();
        }
    }
}
#endif
