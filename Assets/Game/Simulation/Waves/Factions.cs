using System;
namespace FrostMaze.Simulation
{
    [Serializable] public sealed class FactionSpec
    {
        public string Name,Description;
        public int[] Designs;
    }
    public static class Factions
    {
        static TowerDesign D(string name,string description,int cost,float damage,float interval,float range,bool air=true,float splash=0,float slow=0,int chain=0)
            =>new TowerDesign{Name=name,Description=description,Cost=cost,Refund=cost*3/4,Spec=new TowerSpec{Damage=damage,Interval=interval,Range=range,TargetsAir=air,SplashRadius=splash,SlowFraction=slow,SlowDuration=slow>0?2:0,ChainTargets=chain}};
        public static void Apply(Scenario c)
        {
            c.Factions=new[]{
                new FactionSpec{Name="Rime Covenant",Description="Ice sentinels. Slow the front line and shatter clustered ground enemies.",Designs=new[]{0,1,2,3,4}},
                new FactionSpec{Name="Rootbound",Description="Ancient tree guardians. Seed volleys, durable oaks and binding roots; Skybough guards the air.",Designs=new[]{5,6,7,8,9}},
                new FactionSpec{Name="Ember Assembly",Description="Fire constructs. Rapid attacks and expensive bombardment reward compact defenses.",Designs=new[]{10,11,12,13,14}},
                new FactionSpec{Name="Volt Vanguard",Description="Arm-cannon sentries and storm machines. Affordable mazes, precise air defense and chaining bolts.",Designs=new[]{15,16,17,18,19}}
            };
            c.Catalog=new[]{
                D("Shard Sentry","Ground + air · dependable opening tower",20,12,.65f,4),
                D("Snow Cairn","Maze piece · no weapon",5,0,1,0,false),
                D("Rime Binder","Ground + air · 30% slow for 2 seconds",45,8,.9f,4.3f,true,0,.3f),
                D("Hail Bell","Ground splash · cannot hit air",65,30,1.25f,4.5f,false,1.3f),
                D("Aurora Needle","Air only · long-range interceptor",55,30,.65f,6),
                D("Seedling Warden","Ground + air · small impact splash",25,16,.9f,4,true,.55f),
                D("Oldbark","Durable maze piece · no weapon",6,0,1,0,false),
                D("Briar Elder","Ground splash · thrown thorn clusters",65,42,1.7f,4,false,1.6f),
                D("Skybough","Air only · heavy sky projectiles",55,50,1.1f,5.5f),
                D("Worldroot","Ground only · broad slowing impact",90,35,1.5f,5,false,1.8f,.2f),
                D("Cinder Watch","Ground + air · fast single-target attacks",25,10,.4f,3.8f),
                D("Coal Bastion","Maze piece · no weapon",5,0,1,0,false),
                D("Furnace Mouth","Ground splash · short-range bombardment",60,34,1.1f,3.5f,false,1.3f),
                D("Flare Lance","Air only · rapid interception",55,18,.35f,5),
                D("Meteor Crucible","Ground splash · expensive long-range artillery",100,70,2,6,false,1.8f),
                D("Pulse Cadet","Ground + air · arm-cannon sentry",20,14,.65f,4),
                D("Scrap Bulwark","Cheap maze piece · no weapon",4,0,1,0,false),
                D("Arc Relay","Ground + air · chains to two nearby enemies",65,18,1,4.3f,true,0,0,2),
                D("Skyrail","Air only · precise long-range fire",55,34,.75f,6),
                D("Nova Marshal","Ground + air · heavy arm-cannon champion",100,55,.9f,5)
            };
            for(int i=0;i<c.Catalog.Length;i++)c.Catalog[i].VisualStyle=i/5;
            for(int f=0;f<4;f++) {
                var wall=c.Catalog[f*5+1].Spec;wall.TargetsGround=false;wall.Health=f==1?300:180;
                c.Catalog[f*5+4].Spec.Health=180;
                c.Catalog[f*5+ (f==0?4:3)].Spec.TargetsGround=false;
            }
        }
    }
}
