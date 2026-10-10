using UnityEngine;
namespace FrostMaze
{
    public sealed partial class Prototype
    {
        public HowlModels HowlModels {get;private set;}
        public Material[] HowlPalette {get;private set;}
        void PrepareHowl()
        {
            bool winter=World.Config.Theme!="iron";
            HowlPalette=new[]{
                MakeMaterial(winter?new Color(.26f,.30f,.38f):new Color(.27f,.20f,.18f)),
                MakeMaterial(winter?new Color(.74f,.78f,.73f):new Color(.65f,.57f,.39f)),
                MakeMaterial(winter?new Color(.40f,.48f,.58f):new Color(.42f,.36f,.22f)),
                MakeMaterial(new Color(1,.38f,.13f),true),
                MakeMaterial(new Color(.075f,.055f,.085f))};
            HowlPalette[0].mainTexture=ActorTexture("Hide");HowlPalette[1].mainTexture=ActorTexture("Stone");HowlPalette[2].mainTexture=ActorTexture("Cloth");
            foreach(var p in HowlPalette){if(p.HasProperty("_Smoothness"))p.SetFloat("_Smoothness",.11f);}
            HowlModels=new HowlModels(Models,winter);HowlModels.Warm();
        }
    }
}
