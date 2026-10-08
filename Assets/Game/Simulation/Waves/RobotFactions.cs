using System.Collections.Generic;
namespace FrostMaze.Simulation
{
    // Original names and tuning; seven-slot builder progression follows the researched maul structure.
    public static class RobotFactions
    {
        public static void Apply(Scenario c)
        {
            string[] names={"Pulse Foundry","Blast Circuit","Prism Division","Horizon Guild","Gravity Works","Scrap Frontier","Overdrive Order","Tidal Array"};
            string[] roles={"Rapid fire and 30% frost control. Build long firing corridors; individual hits are light.","Wide ground blasts and an air-splash interceptor. Cluster enemies; slow fire struggles against scattered targets.","Three-link chain weapons punish compact groups. Pair them with precision anti-air for isolated flyers.","The longest firing lanes and sky coverage. Exploit bends; limited crowd control rewards careful placement.","40% gravity control and broad impacts. Expensive towers need time on target; protect their maze.","Seven-gold armed walls and 90% base refunds. Rebuild flexible mazes; modest damage needs investment.","Heavy short-range hits and a freezing opener. Fold the route close to weapons; fast enemies punish open layouts.","Water control, splash and chains. Layer complementary specialists instead of relying on one weapon."};
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
                    int cost=costs[t];if(f==5&&t==0)cost=7;if(f==4)cost=cost*6/5;
                    bool splash=(t==2&&(f==0||f==1||f==4||f==6||f==7))||t==4&&f==1;
                    bool slow=(t==5&&(f==0||f==5))||t==1&&(f==4||f==6)||t==3&&f==7;
                    bool chain=t==1&&(f==2||f==7);
                    bool airOnly=t==4;
                    string role=airOnly?(splash?"Air only · splash interceptor":"Air only · interceptor"):splash?"Ground only · splash":"Ground + air";
                    if(slow)role+=" · 25% slow";if(chain)role+=" · chains to two enemies";
                    if(t==6)role+=" · requires all six regular designs standing";
                    if(slow)role=role.Replace("25%",f==4?"40%":f==0?"30%":"25%");
                    if(chain&&f==2)role=role.Replace("two enemies","three enemies");
                    catalog.Add(new TowerDesign{Name=towers[f][t],Description=role,Cost=cost,Refund=f==5?cost*9/10:cost*3/4,VisualStyle=3,
                        Requires=t==6?new[]{f*7,f*7+1,f*7+2,f*7+3,f*7+4,f*7+5}:new int[0],
                        Spec=new TowerSpec{Damage=damage,Interval=interval,Range=range+(airOnly?(f==3?1.5f:1f):0),Health=t==6?240:100,TargetsAir=airOnly||!splash,TargetsGround=!airOnly,SplashRadius=splash?(f==1?1.6f:f==4?1.4f:1.2f):0,SlowFraction=slow?(f==4?.4f:f==0?.3f:.25f):0,SlowDuration=slow?(f==4?3:2):0,ChainTargets=chain?(f==2?3:2):0}});
                }
            }
            c.Catalog=catalog.ToArray();
        }
    }
}
