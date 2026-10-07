using System.Collections.Generic;
namespace FrostMaze.Simulation
{
    // Original names and tuning; seven-slot builder progression follows the researched maul structure.
    public static class RobotFactions
    {
        public static void Apply(Scenario c)
        {
            string[] names={"Pulse Foundry","Blast Circuit","Prism Division","Horizon Guild","Gravity Works","Scrap Frontier","Overdrive Order","Tidal Array"};
            string[] roles={"Fast arm-cannon sentries and a slowing specialist.","Explosive attacks with steady firing speed.","Balanced precision weapons and chained energy.","Long-range specialists cover wide approaches.","Costly specialists mix control and area attacks.","The cheapest armed maze pieces and adaptable support.","Heavy damage trades away range and firing speed.","A versatile collection of splash, control and precision."};
            string[][] towers={
                new[]{"Fuse Cadet","Ironhand","Shear Sentinel","Arc Ranger","Flare Keeper","Rime Runner","Echo Champion"},
                new[]{"Alloy Cadet","Crashbreaker","Gale Pilot","Heatkeeper","Quicksilver","Rootguard","Citadel Champion"},
                new[]{"Shade Cadet","Spark Herald","Lodestone","Coil Serpent","Prism Twin","Needle Guard","Shell Champion"},
                new[]{"Glimmer Cadet","Sun Regent","Dustkeeper","Boneplate","Deepdiver","Drillwarden","Crawler Champion"},
                new[]{"Gyro Cadet","Gravity Anchor","Starcaller","Wavekeeper","Chargeguard","Granite Sentinel","Eclipse Champion"},
                new[]{"Strider Cadet","Lance Knight","Windkeeper","Bloomguard","Whiteout","Hatchet Herald","Fossil Champion"},
                new[]{"Junk Cadet","Freeze Warden","Splashguard","Spring Striker","Duskkeeper","Turbo Sentinel","Mask Champion"},
                new[]{"Grenade Cadet","Kite Ranger","Jester Coil","Aquaguard","Blade Keeper","Orbit Herald","Verdant Champion"}
            };
            c.Factions=new FactionSpec[8];var catalog=new List<TowerDesign>();
            int[] costs={10,30,55,80,110,150,260};
            for(int f=0;f<8;f++) {
                var designs=new int[7];for(int t=0;t<7;t++)designs[t]=f*7+t;
                c.Factions[f]=new FactionSpec{Name=names[f],Description=roles[f]+" Build all six regular designs to unlock the champion.",Designs=designs};
                for(int t=0;t<7;t++) {
                    float damage=new[]{6f,16,26,38,52,75,140}[t];float interval=.8f;float range=4;
                    if(f==0){interval=.55f;damage*=.8f;}
                    if(f==1){interval=1;damage*=1.1f;}
                    if(f==3)range=5.5f;
                    if(f==4){damage*=1.2f;interval=1;}
                    if(f==6){damage*=1.65f;interval=1.2f;range=3.5f;}
                    int cost=costs[t];if(f==5&&t==0)cost=7;if(f==4)cost=cost*7/5;
                    bool splash=(t==2&&(f==0||f==1||f==4||f==6||f==7))||t==4&&f==1;
                    bool slow=(t==5&&(f==0||f==5))||t==1&&(f==4||f==6)||t==3&&f==7;
                    bool chain=t==1&&(f==2||f==7);
                    bool airOnly=t==4;
                    string role=airOnly?"Air only · interceptor":splash?"Ground only · splash":"Ground + air";
                    if(slow)role+=" · 25% slow";if(chain)role+=" · chains to two enemies";
                    if(t==6)role+=" · requires all six regular designs standing";
                    catalog.Add(new TowerDesign{Name=towers[f][t],Description=role,Cost=cost,Refund=cost*3/4,VisualStyle=3,
                        Requires=t==6?new[]{f*7,f*7+1,f*7+2,f*7+3,f*7+4,f*7+5}:new int[0],
                        Spec=new TowerSpec{Damage=damage,Interval=interval,Range=range,Health=t==6?240:100,TargetsAir=!splash,TargetsGround=!airOnly,SplashRadius=splash?1.2f:0,SlowFraction=slow?.25f:0,SlowDuration=slow?2:0,ChainTargets=chain?2:0}});
                }
            }
            c.Catalog=catalog.ToArray();
        }
    }
}
