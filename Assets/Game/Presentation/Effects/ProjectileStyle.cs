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
        static readonly Color[] WinterColors={new Color(.25f,.87f,1),new Color(.5f,.8f,.9f),new Color(.38f,.58f,1),new Color(.72f,.93f,1),new Color(.62f,1,.77f),
                    new Color(.79f,.65f,.37f),new Color(.5f,.6f,.4f),new Color(1,.77f,.34f),new Color(.78f,.91f,.52f),new Color(.32f,.88f,.44f),
                    new Color(1,.43f,.12f),new Color(.8f,.4f,.2f),new Color(1,.72f,.18f),new Color(1,.91f,.53f),new Color(1,.23f,.31f),
                    new Color(.69f,.43f,1),new Color(.6f,.5f,.8f),new Color(.4f,.71f,1),new Color(.94f,.68f,1),new Color(1,.4f,.85f)};
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
            ProjectileShape shape;
            Color color;
            if(!iron) {
                shape=WinterShapes[design%WinterShapes.Length];color=WinterColors[design%WinterColors.Length];
            } else {
                // Seven distinct silhouettes per roster; faction hue and slot offset identify the design.
                shape=IronShapes[slot%7];
                float hue=Mathf.Repeat(.54f+faction*.113f+(slot-3)*.012f,1);
                color=Color.HSVToRGB(hue,.76f-(slot%3)*.13f,1);
            }
            float size=slot==(iron?6:4)?.24f:.15f+(slot%3)*.025f;
            return new ProjectileStyle(shape,color,size,spec.SplashRadius>0?.24f:.18f,spec.SplashRadius>0?.55f:0);
        }
    }
}
