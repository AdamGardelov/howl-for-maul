using System.Collections.Generic;
using UnityEngine;
using FrostMaze.Simulation;
namespace FrostMaze
{
    // Original material-based Foley synthesis: transients, filtered texture and short inharmonic bodies.
    // No imported recordings, sustained oscillators or arcade pitch sweeps.
    public sealed class TowerSoundBank : System.IDisposable
    {
        public const int Variations=3,SampleRate=48000;
        readonly Dictionary<int,AudioClip> clips=new Dictionary<int,AudioClip>();
        readonly Scenario config;
        public TowerSoundBank(Scenario scenario){config=scenario;}
        public int Count=>clips.Count;
        static float Noise(ref uint state){state^=state<<13;state^=state>>17;state^=state<<5;return (state&65535)/32767.5f-1;}
        public AudioClip Get(int design,int variation=0)
        {
            if(design<0||design>=config.Catalog.Length)return null;
            variation=((variation%Variations)+Variations)%Variations;int key=design*Variations+variation;
            if(clips.TryGetValue(key,out var clip))return clip;
            var d=config.Catalog[design];var spec=d.Spec;int family=0,slot=0;
            for(int i=0;i<config.Factions.Length;i++){int at=System.Array.IndexOf(config.Factions[i].Designs,design);if(at>=0){family=i;slot=at;break;}}
            bool iron=config.Theme=="iron",metal=iron&&family==0,heavy=spec.SplashRadius>0;
            float length=spec.Damage<=0?.18f:heavy?.42f:spec.SlowFraction>0?.34f:.25f;
            length+=(slot%3)*.027f;int count=Mathf.RoundToInt(SampleRate*length);var mono=new float[count];
            uint random=(uint)(design+1)*747796405u+(uint)(variation+1)*2891336453u+(metal?91u:17u);
            float detune=1+(variation-1)*.025f+((design*17)%11-5)*.007f;
            float low=0,mid=0,air=0,crackle=0;
            // Physical families have very different noise balance and resonance duration.
            float bodyFrequency=metal?190+family*37+slot*29:family==0?1550+slot*170:family==1?115+slot*26:family==2?85+slot*19:530+slot*73;
            float resonanceDecay=metal?42+family*4:family==0?58:family==1?35:family==2?48:65;
            float lowMix=metal?.42f:family==1?.85f:family==2?.70f:.12f;
            float gritMix=metal?.65f:family==0?.43f:family==1?.36f:family==2?.74f:.62f;
            float airMix=metal?.16f:family==0?.36f:family==1?.045f:family==2?.10f:.24f;
            if(iron&&!metal){
                // Chitin, glass, wind, stone, cloth spirits, drake breath and water have different bodies.
                float[] frequencies={190,235,1320,740,105,610,92,340};
                float[] lows={.42f,.55f,.10f,.06f,.94f,.20f,.78f,.48f};
                float[] grits={.65f,.73f,.28f,.24f,.72f,.24f,.81f,.22f};
                float[] airs={.16f,.08f,.31f,.68f,.035f,.39f,.16f,.39f};
                bodyFrequency=frequencies[family]+slot*(family==4?14:27);lowMix=lows[family];gritMix=grits[family];airMix=airs[family];
                resonanceDecay=family==2?38:family==4?52:65;
            }
            for(int i=0;i<count;i++) {
                float t=i/(float)SampleRate,u=t/length,n=Noise(ref random);
                low+=.025f*(n-low);mid+=(metal?.21f:.13f)*(n-mid);air+=.62f*(n-air);
                float high=n-air,band=mid-low;
                if(i%(113+(design%7)*37)==0)crackle=Noise(ref random)>.4f?1:0;else crackle*=.991f;
                float attack=1-Mathf.Exp(-t/ .0015f),release=Mathf.Clamp01((length-t)/.018f);
                float strike=Mathf.Exp(-t*(heavy?34:62)),tail=Mathf.Exp(-t*(heavy?11:18));
                float burst=1;
                if(spec.ChainTargets>0)burst=.58f+.42f*Mathf.Pow(Mathf.Cos(t*(170+family*13)),2);
                if(spec.SlowFraction>0)burst=.62f+.38f*Mathf.Sin(Mathf.PI*Mathf.Clamp01(u*1.4f));
                float texture=(low*lowMix*4+band*gritMix*2+high*airMix)*tail*burst;
                float snap=(band*.75f+high*.24f)*strike;
                float body=0;
                // Inharmonic partials decay quickly into noise, avoiding a pitched laser tail.
                for(int partial=0;partial<4;partial++) {
                    float ratio=partial==0?1:partial==1?1.467f:partial==2?2.731f:4.113f;
                    float frequency=bodyFrequency*ratio*detune;
                    body+=Mathf.Sin(t*frequency*Mathf.PI*2)*Mathf.Exp(-t*(resonanceDecay+partial*23))/(partial+1);
                }
                float thump=Mathf.Sin(t*(heavy?73:125+slot*9)*Mathf.PI*2)*Mathf.Exp(-t*(heavy?32:58));
                float detail=metal?band*Mathf.Exp(-Mathf.Abs(t-(.025f+slot*.003f))*130)*.32f:
                    family==2?band*crackle*tail*.65f:family==0?high*crackle*tail*.25f:band*crackle*tail*.12f;
                mono[i]=(texture+snap+body*(metal?.16f:family==0?.12f:.10f)+thump*(heavy?.34f:.13f)+detail)*attack*release;
            }
            // Remove DC and normalize perceived energy, with headroom for overlapping weapons.
            double sum=0;foreach(float v in mono)sum+=v;float mean=(float)(sum/count);double energy=0;float peak=0;
            for(int i=0;i<count;i++){mono[i]-=mean;mono[i]*=Mathf.Min(1,i/(SampleRate*.002f))*Mathf.Min(1,(count-1-i)/(SampleRate*.01f));energy+=mono[i]*mono[i];peak=Mathf.Max(peak,Mathf.Abs(mono[i]));}
            float gain=Mathf.Min((heavy?.15f:.115f)/Mathf.Max(.001f,Mathf.Sqrt((float)(energy/count))),.76f/Mathf.Max(.001f,peak));
            var stereo=new float[count*2];int leftDelay=SampleRate*(9+variation)/1000,rightDelay=SampleRate*(16+family%3)/1000;
            for(int i=0;i<count;i++){
                float fade=Mathf.Clamp01((count-1-i)/(SampleRate*.012f));
                stereo[i*2]=(mono[i]+(i>=leftDelay?mono[i-leftDelay]*.095f:0))*gain*fade;
                stereo[i*2+1]=(mono[i]+(i>=rightDelay?mono[i-rightDelay]*.085f:0))*gain*fade;
            }
            clip=AudioClip.Create(d.Name+" · material shot "+(variation+1),count,2,SampleRate,false);clip.SetData(stereo,0);clips.Add(key,clip);return clip;
        }
        public void Dispose(){foreach(var clip in clips.Values)if(clip!=null){if(Application.isPlaying)Object.Destroy(clip);else Object.DestroyImmediate(clip);}clips.Clear();}
    }
}
