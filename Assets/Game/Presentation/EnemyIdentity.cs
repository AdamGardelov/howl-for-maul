using FrostMaze.Simulation;
namespace FrostMaze
{
    public enum HowlForm { Hearthgnawer, Thornrunner, Ashling, Cairnback, HollowWarden, Gloamwing, Gatebreaker, StormHerald }
    // Presentation only: names/shape never change wave stats, fingerprints or routing.
    public static class EnemyIdentity
    {
        static readonly string[] names={"Hearthgnawer","Thornrunner","Ashling swarm","Cairnback","Hollow Warden","Gloamwing","Gatebreaker","Storm Herald"};
        public static string Name(HowlForm form)=>names[(int)form];
        public static HowlForm For(WaveSpec wave)
        {
            string name=wave.Name??"";
            if(wave.Flying)return name.StartsWith("Finale",System.StringComparison.Ordinal)?HowlForm.StormHerald:HowlForm.Gloamwing;
            if(name.Contains("Gatebreakers"))return HowlForm.Gatebreaker;
            if(name.Contains("Siege wardens")||name.Contains("Iron procession"))return HowlForm.Cairnback;
            if(name.Contains("Reinforced patrol")||name.Contains("Veteran vanguard"))return HowlForm.HollowWarden;
            if(name.Contains("Swarm column"))return HowlForm.Ashling;
            if(wave.Speed>=2.6f)return HowlForm.Thornrunner;
            if(wave.Speed<=1.6f||wave.Damage>=30)return HowlForm.Cairnback;
            return HowlForm.Hearthgnawer;
        }
    }
}
