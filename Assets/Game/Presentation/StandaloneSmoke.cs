using System;
using UnityEngine;
using FrostMaze.Simulation;
namespace FrostMaze
{
    // Explicit CI switch. Normal launches never enter this path.
    public static class StandaloneSmoke
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Run()
        {
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"--howl-smoke-test")<0)return;
            try {
                foreach(string name in new[]{"Rimewatch","Ironfold"}) {
                    var asset=Resources.Load<MapDefinition>(name);
                    if(asset==null)throw new Exception("Missing packaged map "+name);
                    var world=new World(JsonUtility.FromJson<Scenario>(JsonUtility.ToJson(asset.Settings)));world.TowersFire=false;
                    for(int lane=0;lane<world.LaneCount;lane++) {
                        if(world.Spawn(new WaveSpec(),world.LaneSpawn(lane),lane)==null)throw new Exception("Blocked spawn");
                        world.Spawn(new WaveSpec{Flying=true},world.LaneSpawn(lane),lane);
                    }
                    for(int i=0;i<9000&&world.Enemies.Count>0;i++)world.Step();
                    if(world.Leaked!=world.LaneCount*2)throw new Exception("Packaged map route stalled: "+name);
                    Debug.Log("HOWL_SMOKE_PASS "+name+" lanes="+world.LaneCount+" factions="+world.Config.Factions.Length+" towers="+world.Config.Catalog.Length);
                }
                Debug.Log("HOWL_SMOKE_COMPLETE");Application.Quit(0);
            }catch(Exception e){Debug.LogException(e);Application.Quit(1);}
        }
    }
}
