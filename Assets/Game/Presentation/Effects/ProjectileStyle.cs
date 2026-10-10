using UnityEngine;
using FrostMaze.Simulation;
namespace FrostMaze
{
    public enum ProjectileShape { Shard, Orb, Shell, Spear, Ember, Ring, Star, Bolt, Seed, ThornCluster, Wingseed, Sprout }
    // Cosmetic signatures only. Colors vary by weapon while retaining the faction's hue family.
    public readonly struct ProjectileStyle
    {
        public readonly ProjectileShape Shape;
        public readonly Color Color;
        public readonly float Size,Duration,Arc;
        public ProjectileStyle(ProjectileShape shape,Color color,float size,float duration,float arc)
        {Shape=shape;Color=color;Size=size;Duration=duration;Arc=arc;}
        static readonly ProjectileShape[] WinterShapes={ProjectileShape.Shard,ProjectileShape.Shard,ProjectileShape.Ring,ProjectileShape.Shell,ProjectileShape.Spear,
                    ProjectileShape.Seed,ProjectileShape.Seed,ProjectileShape.ThornCluster,ProjectileShape.Wingseed,ProjectileShape.Sprout,
                    ProjectileShape.Ember,ProjectileShape.Ember,ProjectileShape.Shell,ProjectileShape.Spear,ProjectileShape.Star,
                    ProjectileShape.Bolt,ProjectileShape.Bolt,ProjectileShape.Star,ProjectileShape.Spear,ProjectileShape.Orb};
        static readonly ProjectileShape[] IronShapes={ProjectileShape.Bolt,ProjectileShape.Orb,ProjectileShape.Shell,ProjectileShape.Shard,ProjectileShape.Spear,ProjectileShape.Ring,ProjectileShape.Star};
        public static float LaunchHeight(Scenario config,int design)
        {
            float height=TowerReadability.Height(config,design);
            var origins=config.Theme=="iron"?IronOrigins:WinterOrigins;
            return (design>=0&&design<origins.Length?origins[design]:1.3f)*height;
        }
        static readonly float[] WinterOrigins={.76f,.49f,1.02f,.53f,1.28f, .98f,.6f,1.20f,1.42f,1.15f, .68f,.56f,.53f,1.0f,1.28f, .76f,.71f,1.02f,1.28f,1.0f};
        static readonly float[] IronOrigins={1.3f,1.3f,1.3f,1.3f,1.3f,1.3f,1.3f,
            .61f,.61f,.84f,1.33f,1.48f,1.11f,.87f,
            .82f,1.25f,1.25f,1.27f,1.25f,1.28f,.82f,
            .76f,1.25f,.82f,1.28f,.76f,1.45f,.82f,
            .56f,1.13f,1.28f,1.25f,1.02f,.71f,.71f,
            .64f,1.28f,1.03f,1.03f,1.25f,1.28f,1.03f,
            .95f,.90f,.95f,1.18f,.95f,1.28f,.95f,
            .56f,.43f,1.27f,.53f,.76f,.91f,.56f};
        public static ProjectileStyle For(Scenario config,int design)
        {
            int faction=0,slot=0;
            for(int f=0;f<config.Factions.Length;f++) {
                int found=System.Array.IndexOf(config.Factions[f].Designs,design);
                if(found>=0){faction=f;slot=found;break;}
            }
            var spec=config.Catalog[design].Spec;
            bool iron=config.Theme=="iron";
            ProjectileShape shape=iron?IronShapes[slot%7]:WinterShapes[design%WinterShapes.Length];
            if(iron&&faction!=0){
                // The same shot pipeline carries shells, spells, gusts and thrown stone; damage stays authoritative.
                if(faction==1)shape=slot==4?ProjectileShape.Wingseed:slot==2||slot==5?ProjectileShape.ThornCluster:ProjectileShape.Seed;
                else if(faction==2)shape=slot%3==0?ProjectileShape.Star:slot%3==1?ProjectileShape.Ring:ProjectileShape.Shard;
                else if(faction==3)shape=slot==1||slot==5?ProjectileShape.Ring:ProjectileShape.Wingseed;
                else if(faction==4)shape=slot==4?ProjectileShape.Spear:ProjectileShape.Shell;
                else if(faction==5)shape=slot==4?ProjectileShape.Wingseed:slot%2==0?ProjectileShape.Orb:ProjectileShape.Ring;
                else if(faction==6)shape=slot==1?ProjectileShape.Shard:ProjectileShape.Ember;
                else shape=slot==4?ProjectileShape.Spear:slot%2==0?ProjectileShape.Orb:ProjectileShape.Ring;
            }
            // Vary each weapon within its order's actual core color, not an unrelated rainbow.
            Color.RGBToHSV(OrderColors.Glow(iron,faction),out float hue,out float saturation,out _);
            hue=Mathf.Repeat(hue+(slot-(iron?3:2))*(iron?.01f:.015f),1);
            Color color=Color.HSVToRGB(hue,Mathf.Clamp(saturation+.12f-(slot%3)*.06f,.25f,.85f),1);
            if(!iron&&faction==1){
                // Physical seeds keep earthy surfaces rather than a bright energy-bolt palette.
                switch(slot){case 0:color=new Color(.64f,.42f,.18f);break;case 2:color=new Color(.68f,.43f,.23f);break;case 3:color=new Color(.73f,.64f,.36f);break;case 4:color=new Color(.36f,.66f,.23f);break;}
            }
            float size=slot==(iron?6:4)?.24f:.15f+(slot%3)*.025f;
            return new ProjectileStyle(shape,color,size,spec.SplashRadius>0?.24f:.18f,spec.SplashRadius>0?.55f:0);
        }
    }
}
