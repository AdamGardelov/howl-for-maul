using System.Collections.Generic;
using UnityEngine;
using FrostMaze.Simulation;
namespace FrostMaze
{
    // Original deterministic layered synthesis: distinct register, rhythm and timbre per design.
    public sealed class TowerSoundBank : System.IDisposable
    {
        readonly Dictionary<int,AudioClip> clips=new Dictionary<int,AudioClip>();
        readonly Scenario config;
        public TowerSoundBank(Scenario scenario){config=scenario;}
        public int Count=>clips.Count;
        public AudioClip Get(int design){
            if(design<0||design>=config.Catalog.Length)return null;
            if(clips.TryGetValue(design,out var clip))return clip;
            var d=config.Catalog[design];var s=d.Spec;int family=0;
            for(int i=0;i<config.Factions.Length;i++)if(System.Array.IndexOf(config.Factions[i].Designs,design)>=0)family=i;
            const int rate=22050;float length=s.Damage<=0?.1f:s.SplashRadius>0?.28f:s.SlowFraction>0?.25f:.16f;
            var data=new float[Mathf.RoundToInt(rate*length)];uint random=(uint)(design+1)*747796405u;
            float fundamental=(s.SplashRadius>0?90:s.SlowFraction>0?580:s.ChainTargets>0?320:210)*Mathf.Pow(1.059463f,(design*7+family*3)%19);
            float phase=0,filtered=0;
            for(int i=0;i<data.Length;i++){
                float t=i/(float)rate,u=t/length,attack=Mathf.Min(1,t/.006f),envelope=attack*Mathf.Pow(1-u,2.5f);
                random^=random<<13;random^=random>>17;random^=random<<5;float noise=(random&65535)/32767.5f-1;
                filtered=Mathf.Lerp(filtered,noise,s.SplashRadius>0?.07f:.55f);
                phase+=Mathf.PI*2*fundamental*(1-u*(s.SlowFraction>0?-.15f:.65f))/rate;
                float tone=Mathf.Sin(phase)+.28f*Mathf.Sin(phase*(2+(family%3)*.17f));
                if(s.ChainTargets>0)tone=Mathf.Sin(phase+2*Mathf.Sin(phase*3.07f));
                if(s.SlowFraction>0)tone+=.35f*Mathf.Sin(phase*2.76f)*Mathf.Exp(-t*5);
                float grit=s.SplashRadius>0?.65f:.08f+(family%4)*.08f;
                float pulse=(design%3==0)?(.72f+.28f*Mathf.Cos(t*95)):1;
                data[i]=Mathf.Clamp((tone*(1-grit)+filtered*grit*2)*envelope*pulse*.48f,-.85f,.85f);
            }
            clip=AudioClip.Create(d.Name+" · original weapon",data.Length,1,rate,false);clip.SetData(data,0);clips.Add(design,clip);return clip;
        }
        public void Dispose(){foreach(var clip in clips.Values)if(clip!=null)Object.Destroy(clip);clips.Clear();}
    }
}
