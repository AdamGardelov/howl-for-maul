using System;
using UnityEngine;
using FrostMaze.Simulation;
namespace FrostMaze
{
    // Explicit CI switch. Normal launches never enter this path.
    public static class StandaloneSmoke
    {
        public static bool Requested => Array.IndexOf(Environment.GetCommandLineArgs(),"--howl-smoke-test")>=0;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Run()
        {
            if(!Requested)return;
            try {
                if(UnityEngine.Object.FindFirstObjectByType<Prototype>()!=null||UnityEngine.Object.FindFirstObjectByType<AudioSource>()!=null)
                    throw new Exception("Route/data smoke unexpectedly initialized presentation or audio");
                Debug.Log("HOWL_SMOKE_DATA_ONLY");
                foreach(string name in new[]{"Rimewatch","Ironfold"}) {
                    var asset=Resources.Load<MapDefinition>(name);
                    if(asset==null)throw new Exception("Missing packaged map "+name);
                    if(asset.Settings.Waves.Length!=20||!asset.Settings.Waves[19].Flying)throw new Exception("Stale packaged campaign "+name);
                    if(name=="Ironfold"&&Math.Abs(asset.Settings.Catalog[29].Spec.SlowFraction-.4f)>.001f)throw new Exception("Stale packaged faction tuning");
                    bool iron=name=="Ironfold";
                    if(asset.Settings.StartingGold!=(iron?2200:240)||asset.Settings.Waves[0].KillGold!=1||asset.Settings.Waves[19].KillGold!=5||asset.Settings.Waves[0].ClearGold!=56||asset.Settings.Waves[iron?13:8].WoodReward!=4)
                        throw new Exception("Stale packaged gold/wood progression: "+name);
                    if(iron&&(asset.Settings.Catalog[6].Cost!=750||asset.Settings.Catalog[6].WoodCost!=1||asset.Settings.Catalog[6].Spec.Damage<300||asset.Settings.Catalog[6].Spec.Health!=480))throw new Exception("Stale champion costs");
                    if(!iron&&!asset.Settings.FactionWoodUnlocks)throw new Exception("Missing faction wood unlocks");
                    var smoke=Resources.Load<Material>("HearthSmoke");if(smoke==null||smoke.shader==null)throw new Exception("Missing packaged hearth shader");
                    var world=new World(JsonUtility.FromJson<Scenario>(JsonUtility.ToJson(asset.Settings)));world.TowersFire=false;
                    for(int lane=0;lane<world.LaneCount;lane++) {
                        if(world.Spawn(new WaveSpec(),world.LaneSpawn(lane),lane)==null)throw new Exception("Blocked spawn");
                        world.Spawn(new WaveSpec{Flying=true},world.LaneSpawn(lane),lane);
                    }
                    for(int i=0;i<9000&&world.Enemies.Count>0;i++)world.Step();
                    if(world.Leaked!=world.LaneCount*2)throw new Exception("Packaged map route stalled: "+name);
                    Debug.Log("HOWL_SMOKE_PASS "+name+" lanes="+world.LaneCount+" factions="+world.Config.Factions.Length+" towers="+world.Config.Catalog.Length+" waves="+world.Config.Waves.Length+" startingGold="+world.Config.StartingGold+" woodAfterWave="+(iron?14:9));
                }
                Debug.Log("HOWL_SMOKE_COMPLETE");ExitAfterStartup(0);
            }catch(Exception e){Debug.LogException(e);ExitAfterStartup(1);}
        }
        static void ExitAfterStartup(int result)
        {
            // Let native engine initialization finish before shutting down this data-only process.
            new GameObject("Data smoke exit").AddComponent<StandaloneSmokeExit>().Result=result;
        }
    }
}
