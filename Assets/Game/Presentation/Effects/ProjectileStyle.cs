using UnityEngine;
using FrostMaze.Simulation;
namespace FrostMaze
{
    public enum ProjectileShape { Shard, Orb, Shell, Spear, Ember, Ring, Star, Bolt }
    // Cosmetic signatures only. Colors vary by weapon while retaining the faction's hue family.
    public readonly struct ProjectileStyle
    {
        public readonly ProjectileShape Shape;
        public readonly Color Color;
        public readonly float Size,Duration,Arc;
        public ProjectileStyle(ProjectileShape shape,Color color,float size,float duration,float arc)
        {Shape=shape;Color=color;Size=size;Duration=duration;Arc=arc;}
        static readonly ProjectileShape[] WinterShapes={ProjectileShape.Shard,ProjectileShape.Shard,ProjectileShape.Ring,ProjectileShape.Shell,ProjectileShape.Spear,
                    ProjectileShape.Orb,ProjectileShape.Orb,ProjectileShape.Shell,ProjectileShape.Spear,ProjectileShape.Star,
                    ProjectileShape.Ember,ProjectileShape.Ember,ProjectileShape.Shell,ProjectileShape.Spear,ProjectileShape.Star,
                    ProjectileShape.Bolt,ProjectileShape.Bolt,ProjectileShape.Star,ProjectileShape.Spear,ProjectileShape.Orb};
        static readonly ProjectileShape[] IronShapes={ProjectileShape.Bolt,ProjectileShape.Orb,ProjectileShape.Shell,ProjectileShape.Shard,ProjectileShape.Spear,ProjectileShape.Ring,ProjectileShape.Star};
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
            // Vary each weapon within its order's actual core color, not an unrelated rainbow.
            Color.RGBToHSV(OrderColors.Glow(iron,faction),out float hue,out float saturation,out _);
            hue=Mathf.Repeat(hue+(slot-(iron?3:2))*(iron?.01f:.015f),1);
            Color color=Color.HSVToRGB(hue,Mathf.Clamp(saturation+.12f-(slot%3)*.06f,.25f,.85f),1);
            float size=slot==(iron?6:4)?.24f:.15f+(slot%3)*.025f;
            return new ProjectileStyle(shape,color,size,spec.SplashRadius>0?.24f:.18f,spec.SplashRadius>0?.55f:0);
        }
    }
}
