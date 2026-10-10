using System.Collections.Generic;
using UnityEngine;
namespace FrostMaze
{
    public sealed partial class Prototype
    {
        readonly Dictionary<string,Texture2D> actorTextures=new Dictionary<string,Texture2D>();
        readonly Dictionary<int,Material> ownerMaterials=new Dictionary<int,Material>();
        public Material OwnerMaterial(int owner)
        {
            if(ownerMaterials.TryGetValue(owner,out var material))return material;
            Color[] colors={new Color(.25f,.85f,1),new Color(1,.66f,.2f),new Color(.67f,.47f,1),new Color(.45f,.9f,.32f)};
            material=MakeMaterial(colors[Mathf.Abs(owner)%4],true);ownerMaterials.Add(owner,material);return material;
        }
        Texture2D ActorTexture(string kind)
        {
            if(actorTextures.TryGetValue(kind,out var cached))return cached;
            const int size=128;var pixels=new Color[size*size];
            for(int y=0;y<size;y++)for(int x=0;x<size;x++) {
                float u=x/(float)size,v=y/(float)size,n=Mathf.PerlinNoise(u*19+13,v*19+7),broad=Mathf.PerlinNoise(u*4+31,v*4+9);
                float shade=.86f+n*.11f+broad*.08f;
                if(kind=="Stone") {
                    float seam=Mathf.Abs(Mathf.Sin((v*3+Mathf.Sin(u*6)*.04f)*Mathf.PI));
                    shade*=Mathf.Lerp(.79f,1,Mathf.SmoothStep(0,1,Mathf.Clamp01(seam/.12f)));
                } else if(kind=="Cloth")shade*=.96f+(((x+y)&1)==0?.035f:0);
                else if(kind=="Wood")shade*=.77f+.24f*Mathf.PerlinNoise(u*32+Mathf.Sin(v*8),v*2+12);
                else {
                    shade*=.96f+.035f*Mathf.PerlinNoise(u*2+3,v*75+41);
                    float rim=Mathf.Min(Mathf.Min(u,1-u),Mathf.Min(v,1-v));shade+=.10f*(1-Mathf.Clamp01(rim/.065f));
                }
                pixels[y*size+x]=new Color(shade,shade,shade);
            }
            var texture=new Texture2D(size,size,TextureFormat.RGB24,true){name="Original actor "+kind,wrapMode=TextureWrapMode.Repeat,filterMode=FilterMode.Trilinear,anisoLevel=2};
            texture.SetPixels(pixels);texture.Apply(true,true);actorTextures.Add(kind,texture);return texture;
        }
        void DressActorPalette(Material[] palette,bool metal,int faction)
        {
            palette[0].mainTexture=ActorTexture(metal?"Metal":"Stone");palette[1].mainTexture=ActorTexture(metal?"Metal":"Cloth");
            palette[3].mainTexture=ActorTexture("Metal");palette[4].mainTexture=ActorTexture(metal?"Metal":"Stone");
            palette[0].SetFloat("_Metallic",metal?.25f:.03f);palette[0].SetFloat("_Smoothness",metal?.30f:.16f);
            palette[1].SetFloat("_Metallic",metal?.28f:0);palette[1].SetFloat("_Smoothness",metal?.32f:.12f);
            palette[3].SetFloat("_Metallic",.6f);palette[3].SetFloat("_Smoothness",.42f);
            // Pigment stays subdued; light belongs to lenses, embers and power cores.
            // These are actor materials only. Ownership rings and attack signatures stay separate.
            palette[1].color=OrderColors.Pigment(metal,faction);
            palette[2].color=OrderColors.Glow(metal,faction);
            // Material families separate the factions even before their colored weapons are visible.
            Color[] winterShell={new Color(.46f,.53f,.54f),new Color(.43f,.43f,.32f),new Color(.40f,.30f,.24f),new Color(.37f,.39f,.49f)};
            if(!metal){palette[0].color=winterShell[faction%4];palette[3].color=faction==2?new Color(.72f,.52f,.28f):new Color(.66f,.67f,.56f);}
            else {
                // Body materials carry the order's identity, not just a thin colored band.
                Color[] bodies={new Color(.20f,.43f,.47f),new Color(.46f,.28f,.16f),new Color(.71f,.69f,.57f),new Color(.37f,.26f,.15f),new Color(.25f,.25f,.38f),new Color(.46f,.27f,.18f),new Color(.39f,.14f,.095f),new Color(.20f,.43f,.38f)};
                Color[] trims={new Color(.64f,.71f,.67f),new Color(.64f,.45f,.24f),new Color(.84f,.78f,.61f),new Color(.70f,.57f,.31f),new Color(.67f,.51f,.29f),new Color(.56f,.61f,.38f),new Color(.72f,.60f,.41f),new Color(.76f,.75f,.59f)};
                palette[0].color=bodies[faction];palette[3].color=trims[faction];
                palette[4].color=Color.Lerp(bodies[faction],new Color(.10f,.13f,.11f),.7f);
                if(faction==3||faction==5){palette[0].mainTexture=ActorTexture("Wood");palette[0].SetFloat("_Metallic",0);palette[0].SetFloat("_Smoothness",.16f);}
                if(faction==2){palette[0].SetFloat("_Metallic",.06f);palette[0].SetFloat("_Smoothness",.4f);}
            }
        }
    }
}
