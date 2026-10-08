using System.Collections.Generic;
using UnityEngine;
using FrostMaze.Simulation;
namespace FrostMaze
{
    // Cosmetic effects consume a bounded event history; damage remains authoritative in World.
    public sealed class CombatFeedback : MonoBehaviour
    {
        Transform effectsRoot;
        Prototype game; World observed; long serial; float effectTime;
        Material bolt,ember,defeat,airDefeat,leak; AudioSource sound; AudioClip boltClip,emberClip;
        readonly List<Flash> flashes=new List<Flash>();
        sealed class Flash { public GameObject Object; public float Until,Start,Duration,Scale; public Vector3 Origin; public bool Pulse; }
        public void Initialize(Prototype prototype)
        {
            effectsRoot=new GameObject("Combat cues").transform;effectsRoot.SetParent(transform,false);
            game=prototype;bolt=game.MakeMaterial(new Color(.2f,1,.85f),true);ember=game.MakeMaterial(new Color(1,.5f,.12f),true);
            defeat=game.MakeMaterial(new Color(1,.73f,.28f),true);airDefeat=game.MakeMaterial(new Color(.8f,.6f,1),true);leak=game.MakeMaterial(new Color(1,.16f,.22f),true);
            sound=gameObject.AddComponent<AudioSource>();sound.spatialBlend=0;sound.volume=.12f;
            boltClip=Tone("Original bolt",900,.055f);emberClip=Tone("Original cannon",130,.14f);
        }
        AudioClip Tone(string name,float frequency,float length)
        {
            const int rate=22050;var data=new float[(int)(length*rate)];
            for(int i=0;i<data.Length;i++){float t=(float)i/rate,envelope=1-(float)i/data.Length;data[i]=Mathf.Sin(2*Mathf.PI*(frequency*t-120*t*t))*envelope*envelope*.5f;}
            var clip=AudioClip.Create(name,data.Length,1,rate,false);clip.SetData(data,0);return clip;
        }
        void ObserveWorld()
        {
            if(observed==game.World)return;
            observed=game.World;serial=0;effectTime=0;
            foreach(var f in flashes)Destroy(f.Object);flashes.Clear();
        }
        public void EnemyRemoved(Enemy enemy)
        {
            if(enemy==null||enemy.Health>0)return;
            ObserveWorld();
            // Leaks must remain visible even when a volley has filled the shared budget.
            if(flashes.Count>=64) {
                if(!enemy.Exited)return;
                Destroy(flashes[0].Object);flashes.RemoveAt(0);
            }
            var obj=new GameObject(enemy.Exited?"Enemy leaked":"Enemy defeated");obj.transform.SetParent(effectsRoot,false);
            var origin=new Vector3(enemy.Position.X,enemy.Spec.Flying?1.7f:.3f,enemy.Position.Y);obj.transform.position=origin;
            float duration=enemy.Exited?.35f:.24f,scale=enemy.Exited?1:Mathf.Max(.6f,enemy.Spec.Radius*3);
            if(enemy.Exited) {
                var ring=obj.AddComponent<LineRenderer>();ring.sharedMaterial=leak;ring.useWorldSpace=false;ring.positionCount=25;ring.startWidth=ring.endWidth=.065f;
                for(int i=0;i<25;i++){float a=i*Mathf.PI*2/24;ring.SetPosition(i,new Vector3(Mathf.Cos(a)*.5f,0,Mathf.Sin(a)*.5f));}
                ring.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            } else {
                obj.AddComponent<MeshFilter>().sharedMesh=game.Models.DefeatBurst;
                var renderer=obj.AddComponent<MeshRenderer>();renderer.sharedMaterial=enemy.Spec.Flying?airDefeat:defeat;
                renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;renderer.receiveShadows=false;
                obj.transform.rotation=Quaternion.Euler(0,enemy.Id*47,0);
            }
            obj.transform.localScale=Vector3.one*scale;
            flashes.Add(new Flash{Object=obj,Origin=origin,Start=effectTime,Until=effectTime+duration,Duration=duration,Scale=scale,Pulse=true});
        }
        static readonly Unity.Profiling.ProfilerMarker PhaseProfile=new Unity.Profiling.ProfilerMarker("Howl.Effects");
        void Update() { using(PhaseProfile.Auto()) UpdateEffects(); }
        void UpdateEffects()
        {
            if(game==null||game.World==null)return;
            ObserveWorld();
            // Cosmetic clock follows pause and speed, but may finish fading after victory.
            if(!game.Paused&&!game.SetupOpen)effectTime+=Time.unscaledDeltaTime*game.Speed;
            for(int i=flashes.Count-1;i>=0;i--) {
                var f=flashes[i];
                if(effectTime>=f.Until){Destroy(f.Object);flashes.RemoveAt(i);continue;}
                if(f.Pulse) {
                    float t=Mathf.Clamp01((effectTime-f.Start)/f.Duration);
                    f.Object.transform.localScale=Vector3.one*(f.Scale*(1+2*t)*(1-t));
                    f.Object.transform.position=f.Origin+Vector3.up*(t*.25f);
                }
            }
            int sounds=0;
            foreach(var shot in observed.Shots)if(shot.Serial>serial) {
                serial=shot.Serial;
                if(flashes.Count<64) {
                    var obj=new GameObject(shot.Chained?"Chain arc":"Tower shot");obj.transform.SetParent(effectsRoot,false);
                    var line=obj.AddComponent<LineRenderer>();line.sharedMaterial=shot.Splash>0?ember:bolt;line.positionCount=2;line.startWidth=.055f;line.endWidth=.02f;
                    line.SetPosition(0,new Vector3(shot.From.X,shot.Chained?(shot.FromFlying?1.7f:.3f):1.3f,shot.From.Y));line.SetPosition(1,new Vector3(shot.To.X,shot.Flying?1.7f:.3f,shot.To.Y));
                    flashes.Add(new Flash{Object=obj,Until=effectTime+.12f});
                    if(shot.Splash>0 && flashes.Count<64) {
                        var ring=new GameObject("Splash impact");ring.transform.SetParent(effectsRoot,false);var r=ring.AddComponent<LineRenderer>();r.sharedMaterial=ember;r.positionCount=25;r.startWidth=r.endWidth=.05f;
                        for(int i=0;i<25;i++){float a=i*Mathf.PI*2/24;r.SetPosition(i,new Vector3(shot.To.X+Mathf.Cos(a)*shot.Splash,shot.Flying?1.7f:.12f,shot.To.Y+Mathf.Sin(a)*shot.Splash));}
                        flashes.Add(new Flash{Object=ring,Until=effectTime+.22f});
                    }
                }
                if(game.SoundEnabled&&sounds++<2)sound.PlayOneShot(shot.Splash>0?emberClip:boltClip);
            }
        }
        void OnDestroy(){if(boltClip!=null)Destroy(boltClip);if(emberClip!=null)Destroy(emberClip);}
    }
}
