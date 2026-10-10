using UnityEngine;
namespace FrostMaze
{
    public sealed partial class Prototype
    {
        void DressCreaturePalette(Material[] p,bool iron,int faction)
        {
            Color[] bodies=iron?new[]{Color.white,new Color(.36f,.25f,.12f),new Color(.36f,.27f,.47f),new Color(.53f,.64f,.59f),new Color(.43f,.46f,.38f),new Color(.18f,.31f,.32f),new Color(.43f,.19f,.13f),new Color(.22f,.48f,.43f)}:
                new[]{new Color(.64f,.76f,.77f),Color.white,new Color(.40f,.24f,.15f),new Color(.28f,.39f,.57f)};
            Color[] trims=iron?new[]{Color.white,new Color(.70f,.57f,.32f),new Color(.80f,.73f,.58f),new Color(.83f,.83f,.66f),new Color(.67f,.64f,.48f),new Color(.77f,.71f,.53f),new Color(.76f,.58f,.33f),new Color(.79f,.76f,.57f)}:
                new[]{new Color(.89f,.90f,.80f),Color.white,new Color(.79f,.51f,.28f),new Color(.77f,.75f,.57f)};
            foreach(int i in new[]{0,1,3,4}){p[i].SetFloat("_Metallic",0);p[i].SetFloat("_Smoothness",iron&&faction==7?.30f:.17f);p[i].mainTexture=ActorTexture("Hide");}
            p[0].color=bodies[faction];p[3].color=trims[faction];p[4].color=Color.Lerp(bodies[faction],new Color(.06f,.10f,.08f),.77f);
            if(iron&&faction==4){p[0].mainTexture=ActorTexture("Stone");p[1].mainTexture=ActorTexture("Leaves");p[3].mainTexture=ActorTexture("Stone");}
            if(iron&&faction==5){p[0].mainTexture=ActorTexture("Cloth");p[3].mainTexture=ActorTexture("Wood");}
            if(iron&&faction==2){p[1].SetFloat("_Smoothness",.48f);p[0].mainTexture=ActorTexture("Cloth");}
        }
    }
}
