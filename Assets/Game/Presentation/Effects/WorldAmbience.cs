using System;
using UnityEngine;
using FrostMaze.Simulation;
namespace FrostMaze
{
    // Original noise-based environmental audio. Four voices, no simulation randomness or timers.
    // Focus-plane attenuation keeps ambience grounded when the RTS camera is high above the lane.
    public sealed class WorldAmbience : MonoBehaviour
    {
        Prototype game;World observed;MapScenery scenery;RtsCamera cameraRig;
        readonly System.Collections.Generic.List<Vector3> hearths=new System.Collections.Generic.List<Vector3>();
        AudioSource wind,fire,place,feet;AudioClip[] clips;
        V2 previous;long previousTick;int previousPlayer=-1;float distanceWalked,nextStep;
        public int Footsteps {get;private set;}
        public float FireLevel=>fire==null?0:fire.volume;
        public float LandmarkLevel=>place==null?0:place.volume;
        public int VoiceCount=>clips==null?0:4;
        public void Initialize(Prototype prototype) {
            game=prototype;scenery=game.GetComponentInChildren<MapScenery>();cameraRig=game.View.GetComponent<RtsCamera>();
            if(scenery!=null)hearths.AddRange(scenery.FirePositions);
            bool ice=game.World.Config.Theme!="iron";clips=new[]{CreateLoop(0,ice),CreateLoop(1,ice),CreateLoop(2,ice),CreateLoop(3,ice)};
            wind=Voice("Sheltered wind",clips[0],true);fire=Voice("Nearby hearth",clips[1],true);
            place=Voice(ice?"Timber and boughs":"Distant foundry",clips[2],true);feet=Voice(ice?"Boots on stone":"Artisan hover movement",clips[3],false);
        }
        AudioSource Voice(string label,AudioClip clip,bool loop) {
            var root=new GameObject(label);root.transform.SetParent(transform,false);
            var source=root.AddComponent<AudioSource>();source.clip=clip;source.playOnAwake=false;source.loop=loop;source.volume=0;source.spatialBlend=0;source.priority=190;
            if(loop)source.Play();return source;
        }
        // Soft stochastic friction rather than oscillating arcade tones. Seeded only for repeatable assets.
        static AudioClip CreateLoop(int kind,bool ice) {
            const int rate=24000;int length=kind==3?rate/4:rate*8;var data=new float[length*2];var rng=new System.Random(1307+kind*97+(ice?0:31));
            float low=0,soft=0,crackle=0,other=0;
            for(int i=0;i<length;i++) {
                float n=(float)rng.NextDouble()*2-1,t=i/(float)rate;
                low+=(n-low)*(kind==0?.006f:kind==2?.012f:.07f);soft+=(n-soft)*.045f;
                other+=((float)rng.NextDouble()*2-1-other)*.018f;
                if(kind==1&&rng.NextDouble()<.0013)crackle=.25f+(float)rng.NextDouble()*.7f;crackle*=.991f;
                float wave;
                if(kind==0)wave=low*(1.6f+.7f*Mathf.Sin(t*1.1f)+.3f*Mathf.Sin(t*2.7f));
                else if(kind==1)wave=soft*.28f+crackle*n*.30f;
                else if(kind==2) {
                    float envelope=Mathf.Pow(Mathf.Max(0,Mathf.Sin(t*(ice?1.6f:2.3f)) ),ice?7:2);
                    wave=low*(ice?.8f:1.9f)+soft*envelope*.45f;
                    if(ice)wave+=Mathf.Sin(t*219+Mathf.Sin(t*17)*2)*envelope*.014f;
                } else {
                    float env=Mathf.Exp(-t*(ice?26:17))*Mathf.Clamp01(t/.006f);
                    wave=(soft*.85f+low*1.6f)*env;
                }
                data[i*2]=Mathf.Clamp(wave*.9f,-.7f,.7f);data[i*2+1]=Mathf.Clamp((wave*.86f+(kind==3?0:other*.14f))*.9f,-.7f,.7f);
            }
            if(kind!=3) {
                // Fade the last quarter-second into the opening samples, with a zero-slope seam.
                int blend=rate/4;for(int i=0;i<blend;i++){float f=Mathf.SmoothStep(0,1,i/(float)(blend-1));for(int ch=0;ch<2;ch++)data[(length-blend+i)*2+ch]=Mathf.Lerp(data[(length-blend+i)*2+ch],data[i*2+ch],f);}
                // Both loop endpoints are silent. The long slow envelope conceals the wrap.
                for(int i=0;i<rate/12;i++){float f=Mathf.SmoothStep(0,1,i/(rate/12f));for(int ch=0;ch<2;ch++){data[i*2+ch]*=f;data[(length-1-i)*2+ch]*=f;}}
            } else for(int i=0;i<rate/100;i++)for(int ch=0;ch<2;ch++)data[(length-1-i)*2+ch]*=i/(rate/100f);
            var clip=AudioClip.Create("Original environment "+kind+(ice?" winter":" foundry"),length,2,rate,false);clip.SetData(data,0);return clip;
        }
        void Localize(AudioSource source,System.Collections.Generic.IReadOnlyList<Vector3> anchors,float maximum,float dt) {
            float nearest=float.MaxValue;Vector3 selected=cameraRig.Focus;
            if(anchors!=null)for(int i=0;i<anchors.Count;i++){var p=anchors[i];float d=(new Vector2(p.x-cameraRig.Focus.x,p.z-cameraRig.Focus.z)).sqrMagnitude;if(d<nearest){nearest=d;selected=p;}}
            float gain=Mathf.Pow(Mathf.Clamp01(1-Mathf.Sqrt(nearest)/18),2);
            source.volume=Mathf.MoveTowards(source.volume,maximum*gain,dt*.18f);
            source.panStereo=Mathf.Clamp(Vector3.Dot(selected-cameraRig.Focus,game.View.transform.right)/14,-.7f,.7f);
        }
        void Update() {
            if(game==null||clips==null)return;float dt=Time.unscaledDeltaTime;
            if(observed!=game.World||previousPlayer!=game.World.ActivePlayer){observed=game.World;previous=observed.BuilderPosition;previousTick=observed.Tick;previousPlayer=observed.ActivePlayer;distanceWalked=0;Footsteps=0;feet.Stop();}
            bool audible=game.SoundEnabled&&game.EffectsVolume>0;
            if(!audible){wind.volume=fire.volume=place.volume=feet.volume=0;feet.Stop();}
            else {
                float gain=Mathf.Clamp01(game.EffectsVolume)*(game.MenuOpen||game.SetupOpen?.22f:game.Paused?.65f:1);
                wind.volume=Mathf.MoveTowards(wind.volume,gain*.055f,dt*.06f);
                Localize(fire,hearths,gain*.24f,dt);Localize(place,scenery?.LandmarkPositions,gain*.10f,dt);
            }
            var at=observed.BuilderPosition;
            if(observed.Tick!=previousTick) {
                float travelled=(at-previous).Length;
                // Low frame rates and faster match speeds can legitimately move several units
                // between rendered frames. Compare with elapsed simulation travel, not a fixed cutoff.
                float allowed=observed.Config.BuilderSpeed*(observed.Tick-previousTick)*World.FixedDelta+.05f;
                if(travelled>.001f&&travelled<=allowed)distanceWalked+=travelled;else if(travelled>allowed)distanceWalked=0;
                if(audible&&!game.Paused&&!game.MenuOpen&&!game.SetupOpen&&distanceWalked>=.8f&&Time.unscaledTime>=nextStep) {
                    float near=Mathf.Clamp01(1-new Vector2(at.X-cameraRig.Focus.x,at.Y-cameraRig.Focus.z).magnitude/18);
                    if(near>0){feet.volume=.24f*game.EffectsVolume*near;feet.panStereo=Mathf.Clamp(Vector3.Dot(new Vector3(at.X,0,at.Y)-cameraRig.Focus,game.View.transform.right)/14,-.7f,.7f);feet.Play();Footsteps++;}
                    distanceWalked=0;nextStep=Time.unscaledTime+.24f;
                }
                previous=at;previousTick=observed.Tick;
            }
            if(game.Paused||game.MenuOpen||game.SetupOpen)feet.Stop();
        }
        void OnDestroy(){if(clips!=null)foreach(var clip in clips)if(clip!=null)Destroy(clip);}
    }
}
