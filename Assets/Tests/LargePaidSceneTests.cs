#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TestTools;
using UnityEditor.SceneManagement;
using FrostMaze.Simulation;
using Object=UnityEngine.Object;
namespace FrostMaze.Tests
{
    public sealed class LargePaidSceneTests
    {
        [Serializable] public sealed class Purchase { public int TowerId,TravelTicks,BeforeWave,Player,X,Y,Cost;public string Tower; }
        [Serializable] public sealed class Upgrade { public int Player,TowerId,Level,Cost,BeforeWave; }
        // Saved ledgers use Wave as the field name; it cannot match this class's name.
        [Serializable] public sealed class Round { public int Wave,Killed,Leaked,Ticks;public int[] PlayerGold; }
        [Serializable] public sealed class Campaign { public string Map,Difficulty;public int StartingTeamGold;public int[] StartingWallets,KillRewards,ClearRewards;public string[] Factions;public int[] StartingPositions;public int Lives;public Purchase[] Placements;public Upgrade[] Upgrades;public Round[] Waves; }
        [Serializable] public sealed class Campaigns { public Campaign[] Runs; }
        [Serializable] public sealed class Measurement {
            public string Map,Device,Api;public int Towers,Enemies,ActiveTowerRenderers,UniqueTowerMeshes;
            public long SyncAllocatedBytes;public double SyncMillisecondsPerCall,RenderMedianMs,RenderMaxMs;
            public int Width=1280,Height=720;public string Capture;
        }
        [Serializable] public sealed class Report { public List<Measurement> Scenes=new List<Measurement>(); }
        static int SavedFaction(string theme,string name)
        {
            string[] legacy=theme=="iron"?new[]{"Pulse Foundry","Blast Circuit","Prism Division","Horizon Guild","Gravity Works","Scrap Frontier","Overdrive Order","Tidal Array"}:new[]{"Rime Covenant","Rootbound","Ember Assembly","Volt Vanguard"};
            for(int i=0;i<legacy.Length;i++)if(name==legacy[i]||name==FactionIdentity.Theme(theme,i))return i;
            throw new InvalidDataException("Unknown saved faction: "+name);
        }
        static int SavedDesign(Scenario config,string name)
        {
            int current=Array.FindIndex(config.Catalog,d=>d.Name==name);if(current>=0)return current;
            // Historical ledgers identify purchases by display name. Preserve their original design index.
            string[] legacy=config.Theme=="iron"?new[]{"Fuse Cadet","Ironhand","Shear Sentinel","Arc Ranger","Flare Keeper","Rime Runner","Echo Champion","Alloy Cadet","Crashbreaker","Gale Pilot","Heatkeeper","Quicksilver","Rootguard","Citadel Champion","Shade Cadet","Spark Herald","Lodestone","Coil Serpent","Prism Twin","Needle Guard","Shell Champion","Glimmer Cadet","Sun Regent","Dustkeeper","Boneplate","Deepdiver","Drillwarden","Crawler Champion","Gyro Cadet","Gravity Anchor","Starcaller","Wavekeeper","Chargeguard","Granite Sentinel","Eclipse Champion","Strider Cadet","Lance Knight","Windkeeper","Bloomguard","Whiteout","Hatchet Herald","Fossil Champion","Junk Cadet","Freeze Warden","Splashguard","Spring Striker","Duskkeeper","Turbo Sentinel","Mask Champion","Grenade Cadet","Kite Ranger","Jester Coil","Aquaguard","Blade Keeper","Orbit Herald","Verdant Champion"}:new[]{"Shard Sentry","Snow Cairn","Rime Binder","Hail Bell","Aurora Needle","Seedling Warden","Oldbark","Briar Elder","Skybough","Worldroot","Cinder Watch","Coal Bastion","Furnace Mouth","Flare Lance","Meteor Crucible","Pulse Cadet","Scrap Bulwark","Arc Relay","Skyrail","Nova Marshal"};
            int index=Array.IndexOf(legacy,name);
            if(index<0)throw new InvalidDataException("Unknown saved defender: "+name);
            return index;
        }
        static string Output=>Path.Combine(Application.dataPath,"../Logs/LargePaidScenes");
        static void Capture(Camera source,Measurement report)
        {
            var go=new GameObject("Paid scene diagnostic camera");var camera=go.AddComponent<Camera>();camera.CopyFrom(source);camera.enabled=false;
            camera.transform.SetPositionAndRotation(source.transform.position,source.transform.rotation);
            var target=RenderTexture.GetTemporary(report.Width,report.Height,24,RenderTextureFormat.ARGB32);
            var previous=RenderTexture.active;Texture2D image=null;
            try {
                camera.targetTexture=target;camera.aspect=report.Width/(float)report.Height;
                var request=new RenderPipeline.StandardRequest{destination=target};
                void Render(){if(GraphicsSettings.currentRenderPipeline==null)camera.Render();else {Assert.That(RenderPipeline.SupportsRenderRequest(camera,request),Is.True);RenderPipeline.SubmitRenderRequest(camera,request);}}
                for(int i=0;i<3;i++)Render();var samples=new double[9];var watch=new System.Diagnostics.Stopwatch();
                for(int i=0;i<samples.Length;i++){watch.Restart();Render();watch.Stop();samples[i]=watch.Elapsed.TotalMilliseconds;}
                Array.Sort(samples);report.RenderMedianMs=samples[samples.Length/2];report.RenderMaxMs=samples[samples.Length-1];
                RenderTexture.active=target;image=new Texture2D(report.Width,report.Height,TextureFormat.RGB24,false);
                image.ReadPixels(new Rect(0,0,report.Width,report.Height),0,0);image.Apply();
                report.Capture=report.Map+"-paid.png";File.WriteAllBytes(Path.Combine(Output,report.Capture),image.EncodeToPNG());
            }finally {RenderTexture.active=previous;camera.targetTexture=null;Object.Destroy(go);if(image!=null)Object.Destroy(image);RenderTexture.ReleaseTemporary(target);}
        }
        [UnityTest,Category("LargePaidScene"),Category("WorldCohesionFinal"),Timeout(600000)]
        public IEnumerator PaidCampaignsRenderLateDefensesAndReleaseViews()
        {
            EditorSceneManager.OpenScene("Assets/Game/Maps/MazeLab.unity");yield return new EnterPlayMode();yield return null;
            var path=Path.Combine(Application.dataPath,"../Docs/Balance/WORLD-IDENTITY-HARD-CAMPAIGNS.json");
            var ledger=JsonUtility.FromJson<Campaigns>("{\"Runs\":"+File.ReadAllText(path)+"}");
            var report=new Report();Directory.CreateDirectory(Output);
            foreach(int selected in new[]{0,1}) {
                var saved=ledger.Runs[selected];var game=Object.FindFirstObjectByType<Prototype>();
                game.ChooseMap(Resources.Load<MapDefinition>(saved.Map));yield return null;yield return null;
                game=Object.FindFirstObjectByType<Prototype>();game.SetupOptions.PlayerCount=2;game.SetupOptions.Difficulty=Difficulty.Hard;
                game.SetupOptions.StartingPositions=(int[])saved.StartingPositions.Clone();
                for(int p=0;p<2;p++)game.SetupOptions.Factions[p]=SavedFaction(game.Map.Settings.Theme,saved.Factions[p]);
                game.SetupOptions.AutomaticWaves=false; // Reproduce the saved, manually paced historical ledger.
                game.StartMatch();game.Paused=true;game.SoundEnabled=false;var w=game.World;
                Assert.That(saved.Difficulty,Is.EqualTo("Hard"));Assert.That(w.Config.StartingGold,Is.EqualTo(saved.StartingTeamGold),"Replay ledger is from a different economy");
                for(int p=0;p<2;p++)Assert.That(w.Players[p].Gold,Is.EqualTo(saved.StartingWallets[p]),"Stale starting wallet fixture");
                for(int wave=0;wave<20;wave++){Assert.That(w.KillGold(wave),Is.EqualTo(saved.KillRewards[wave]));Assert.That(w.ClearGold(wave),Is.EqualTo(saved.ClearRewards[wave]));}
                for(int round=1;round<=20;round++) {
                    for(int player=1;player<=2;player++) {
                        w.SelectPlayer(player-1);
                        foreach(var buy in saved.Placements.Where(b=>b.BeforeWave==round&&b.Player==player)) {
                            w.SelectedDesign=SavedDesign(w.Config,buy.Tower);int before=w.Gold;
                            Assert.That(w.OrderBuild(buy.X,buy.Y,out string reason),Is.True,reason);
                            int travel=0;while(w.HasBuildOrder&&travel<1000){w.Step();travel++;}
                            var tower=w.Grid.At(buy.X,buy.Y);Assert.That(tower,Is.Not.Null);
                            Assert.That(tower.Id,Is.EqualTo(buy.TowerId));Assert.That(before-w.Gold,Is.EqualTo(buy.Cost));
                            Assert.That(travel,Is.EqualTo(buy.TravelTicks),"Paid builder travel differs from saved campaign");
                        }
                        foreach(var upgrade in saved.Upgrades.Where(u=>u.BeforeWave==round&&u.Player==player)) {
                            int before=w.Gold;Assert.That(w.Upgrade(upgrade.TowerId,out string reason),Is.True,reason);
                            Assert.That(before-w.Gold,Is.EqualTo(upgrade.Cost));Assert.That(w.Grid.Find(upgrade.TowerId).Level,Is.EqualTo(upgrade.Level));
                        }
                    }
                    Assert.That(w.StartWave(),Is.True);int ticks=0;
                    while(w.WaveActive&&!w.Finished&&ticks<18000) {
                        w.Step();ticks++;
                        if(round==20&&ticks==450) {
                            yield return null;yield return null;var camera=game.View.GetComponent<RtsCamera>();camera.Overview();yield return null;
                            var views=Object.FindObjectsByType<TowerView>(FindObjectsSortMode.None);
                            Assert.That(views.Length,Is.EqualTo(w.Grid.Towers.Count));
                            Assert.That(Object.FindObjectsByType<EnemyView>(FindObjectsSortMode.None).Length,Is.EqualTo(w.Enemies.Count));
                            foreach(var view in views){Assert.That(view.VisibleLevel,Is.EqualTo(view.Subject.Level));Assert.That(view.GetComponentsInChildren<Collider>().Length,Is.Zero);}
                            var method=typeof(Prototype).GetMethod("SyncViewsProfiled",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance);
                            var sync=(Action)Delegate.CreateDelegate(typeof(Action),game,method);for(int i=0;i<20;i++)sync();
                            var watch=new System.Diagnostics.Stopwatch();long before=GC.GetAllocatedBytesForCurrentThread();watch.Start();for(int i=0;i<100;i++)sync();watch.Stop();
                            long bytes=GC.GetAllocatedBytesForCurrentThread()-before;
                            var m=new Measurement{Map=saved.Map,Device=SystemInfo.graphicsDeviceName,Api=SystemInfo.graphicsDeviceType.ToString(),Towers=views.Length,Enemies=w.Enemies.Count,SyncAllocatedBytes=bytes,SyncMillisecondsPerCall=watch.Elapsed.TotalMilliseconds/100};
                            m.ActiveTowerRenderers=views.Sum(v=>v.GetComponentsInChildren<Renderer>().Count(r=>r.enabled));
                            m.UniqueTowerMeshes=views.SelectMany(v=>v.GetComponentsInChildren<MeshFilter>()).Select(f=>f.sharedMesh).Distinct().Count();
                            Capture(game.View,m);report.Scenes.Add(m);File.WriteAllText(Path.Combine(Output,"report.json"),JsonUtility.ToJson(report,true));
                            Assert.That(bytes,Is.Zero,"Stable large paid scene view sync allocates");
                        }
                    }
                    var expected=saved.Waves[round-1];Assert.That(w.LastWaveSummary.Killed,Is.EqualTo(expected.Killed));Assert.That(w.LastWaveSummary.Leaked,Is.EqualTo(expected.Leaked));
                    for(int p=0;p<2;p++)Assert.That(w.Players[p].Gold,Is.EqualTo(expected.PlayerGold[p]),"Replay wallet differs");
                    yield return null;
                }
                Assert.That(w.Won&&w.Lives==saved.Lives,Is.True);game.StartMatch();game.Paused=true;yield return null;yield return null;
                Assert.That(Object.FindObjectsByType<TowerView>(FindObjectsSortMode.None).Length,Is.Zero);Assert.That(Object.FindObjectsByType<EnemyView>(FindObjectsSortMode.None).Length,Is.Zero);
            }
            Assert.That(report.Scenes.Count,Is.EqualTo(2));
            var last=Object.FindFirstObjectByType<Prototype>();last.ChooseMap(Resources.Load<MapDefinition>("Rimewatch"));yield return null;
            yield return new ExitPlayMode();
        }
    }
}
#endif
