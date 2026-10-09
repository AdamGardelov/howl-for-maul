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
        int observedLeaks, recentLeaks;
        float alertTime, leakAlertUntil;
        // UI alerts use real play time, independent of simulation speed and camera visibility.
        public int RecentLeaks => alertTime<leakAlertUntil ? recentLeaks : 0;
        Material bolt,ember,defeat,airDefeat,leak,rubble; AudioSource sound; AudioClip boltClip,emberClip,leakClip;
        readonly Dictionary<int,Material> weaponColors=new Dictionary<int,Material>();
        Material WeaponColor(ShotEvent shot){if(shot.Design<0||shot.Design>=game.World.Config.Catalog.Length)return shot.Splash>0?ember:bolt;if(weaponColors.TryGetValue(shot.Design,out var color))return color;color=game.MakeMaterial(ProjectileStyle.For(game.World.Config,shot.Design).Color,true);weaponColors[shot.Design]=color;return color;}
        float nextShotSound,nextLeakSound;TowerSoundBank soundBank;
        public int SoundDispatches {get;private set;}
        public string LastSound {get;private set;}
        readonly List<Flash> flashes=new List<Flash>();
        readonly Plane[] shotFrustum=new Plane[6];
        sealed class Flash { public GameObject Object; public float Until,Start,Duration,Scale; public Vector3 Origin; public bool Pulse; public ProjectileCue Projectile; }
        public void Initialize(Prototype prototype)
        {
            effectsRoot=new GameObject("Combat cues").transform;effectsRoot.SetParent(transform,false);
            game=prototype;bolt=game.MakeMaterial(new Color(.2f,1,.85f),true);ember=game.MakeMaterial(new Color(1,.5f,.12f),true);
            defeat=game.MakeMaterial(new Color(1,.73f,.28f),true);airDefeat=game.MakeMaterial(new Color(.8f,.6f,1),true);leak=game.MakeMaterial(new Color(1,.16f,.22f),true);
            rubble=game.MakeMaterial(new Color(.65f,.7f,.75f));
            sound=gameObject.AddComponent<AudioSource>();sound.spatialBlend=0;sound.volume=.12f;sound.playOnAwake=false;
            boltClip=Tone("Original bolt",900,.055f);emberClip=Tone("Original cannon",130,.14f);leakClip=Tone("Original breach",660,.22f,1000);
        }
        AudioClip Tone(string name,float frequency,float length,float fall=120)
        {
            const int rate=22050;var data=new float[(int)(length*rate)];
            for(int i=0;i<data.Length;i++){float t=(float)i/rate,envelope=1-(float)i/data.Length;data[i]=Mathf.Sin(2*Mathf.PI*(frequency*t-fall*t*t))*envelope*envelope*.5f;}
            var clip=AudioClip.Create(name,data.Length,1,rate,false);clip.SetData(data,0);return clip;
        }
        void ObserveWorld()
        {
            if(observed==game.World)return;
            soundBank?.Dispose();soundBank=new TowerSoundBank(game.World.Config);
            observed=game.World;serial=0;effectTime=0;
            observedLeaks=0;recentLeaks=0;alertTime=0;leakAlertUntil=0;
            nextShotSound=nextLeakSound=0;SoundDispatches=0;LastSound=null;sound.Stop();
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
        public void TowerStruck(Tower tower,bool removed)
        {
            if(tower==null||(removed&&tower.Health>0))return; // A sale is not destruction.
            ObserveWorld();
            if(flashes.Count>=64) {
                if(!removed)return;
                Destroy(flashes[0].Object);flashes.RemoveAt(0);
            }
            var obj=new GameObject(removed?"Tower destroyed":"Tower struck");obj.transform.SetParent(effectsRoot,false);
            var origin=new Vector3(tower.Center.X,.15f,tower.Center.Y);obj.transform.position=origin;
            float duration=removed?.55f:.2f;
            if(removed) {
                obj.AddComponent<MeshFilter>().sharedMesh=game.Models.Rubble;
                var renderer=obj.AddComponent<MeshRenderer>();renderer.sharedMaterial=rubble;
                renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;renderer.receiveShadows=false;
            } else {
                var ring=obj.AddComponent<LineRenderer>();ring.sharedMaterial=leak;ring.useWorldSpace=false;ring.positionCount=5;ring.startWidth=ring.endWidth=.08f;
                float x=tower.Spec.Width*.48f,z=tower.Spec.Height*.48f;
                ring.SetPositions(new[]{new Vector3(-x,0,-z),new Vector3(-x,0,z),new Vector3(x,0,z),new Vector3(x,0,-z),new Vector3(-x,0,-z)});
                ring.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            flashes.Add(new Flash{Object=obj,Origin=origin,Start=effectTime,Until=effectTime+duration,Duration=duration,Scale=1,Pulse=true});
        }
        bool InView(V2 point,float height)
        {
            var screen=game.View.WorldToViewportPoint(new Vector3(point.X,height,point.Y));
            return screen.z>0&&screen.x>=0&&screen.x<=1&&screen.y>=0&&screen.y<=1;
        }
        bool ShotInView(ShotEvent shot,Vector3 from,Vector3 to)
        {
            // Conservative bounds keep beams crossing the screen and splashes reaching its edge.
            // Testing endpoints alone would incorrectly hide both cases at close zoom.
            var bounds=new Bounds(from,Vector3.zero);bounds.Encapsulate(to);
            if(shot.Splash>0)bounds.Encapsulate(new Bounds(
                new Vector3(shot.To.X,shot.Flying?1.7f:.12f,shot.To.Y),
                new Vector3(shot.Splash*2,.05f,shot.Splash*2)));
            bounds.Expand(1.6f); // Includes projectile tips, shell arcs and impact width at screen edges.
            return GeometryUtility.TestPlanesAABB(shotFrustum,bounds);
        }
        void DispatchSound(AudioClip clip)
        {
            sound.PlayOneShot(clip);SoundDispatches++;LastSound=clip.name;
        }
        static readonly Unity.Profiling.ProfilerMarker PhaseProfile=new Unity.Profiling.ProfilerMarker("Howl.Effects");
        void Update() { using(PhaseProfile.Auto()) UpdateEffects(); }
        void UpdateEffects()
        {
            if(game==null||game.World==null)return;
            ObserveWorld();
            // Cosmetic clock follows pause and speed, but may finish fading after victory.
            if(!game.Paused&&!game.SetupOpen&&!game.MenuOpen) {
                effectTime+=Time.unscaledDeltaTime*game.Speed;
                alertTime+=Time.unscaledDeltaTime;
            }
            bool audioActive=game.SoundEnabled&&!game.Paused&&!game.SetupOpen&&!game.MenuOpen;
            sound.mute=!audioActive;sound.volume=.12f*Mathf.Clamp01(game.EffectsVolume);
            if(observed.Leaked>observedLeaks) {
                if(alertTime>=leakAlertUntil)recentLeaks=0;
                recentLeaks+=observed.Leaked-observedLeaks;
                observedLeaks=observed.Leaked;leakAlertUntil=alertTime+4;
                if(audioActive&&alertTime>=nextLeakSound) {
                    sound.Stop();DispatchSound(leakClip);
                    nextLeakSound=alertTime+.6f;nextShotSound=alertTime+.22f;
                }
            }
            for(int i=flashes.Count-1;i>=0;i--) {
                var f=flashes[i];
                if(effectTime>=f.Until){Destroy(f.Object);flashes.RemoveAt(i);continue;}
                if(f.Projectile!=null)f.Projectile.Render((effectTime-f.Start)/f.Duration);
                if(f.Pulse) {
                    float t=Mathf.Clamp01((effectTime-f.Start)/f.Duration);
                    f.Object.transform.localScale=Vector3.one*(f.Scale*(1+2*t)*(1-t));
                    f.Object.transform.position=f.Origin+Vector3.up*(t*.25f);
                }
            }
            // One camera-local weapon cue per 120 ms, regardless of frame rate or speed.
            bool weaponReady=audioActive&&alertTime>=nextShotSound;
            ShotEvent audible=null;bool frustumReady=false;
            foreach(var shot in observed.Shots)if(shot.Serial>serial) {
                serial=shot.Serial;
                if(!frustumReady){GeometryUtility.CalculateFrustumPlanes(game.View,shotFrustum);frustumReady=true;}
                var from=new Vector3(shot.From.X,shot.Chained?(shot.FromFlying?1.7f:.3f):1.3f,shot.From.Y);
                var to=new Vector3(shot.To.X,shot.Flying?1.7f:.3f,shot.To.Y);
                // Consume unseen events without spending the shared visible-effect budget.
                if(flashes.Count<64&&ShotInView(shot,from,to)) {
                    var obj=new GameObject(shot.Chained?"Chain arc":"Tower shot");obj.transform.SetParent(effectsRoot,false);
                    bool styled=shot.Design>=0&&shot.Design<observed.Config.Catalog.Length;
                    if(styled) {
                        var style=ProjectileStyle.For(observed.Config,shot.Design);var cue=obj.AddComponent<ProjectileCue>();
                        cue.Initialize(shot.Design,style,from,to,shot.Chained,WeaponColor(shot));
                        flashes.Add(new Flash{Object=obj,Start=effectTime,Duration=style.Duration,Until=effectTime+style.Duration,Projectile=cue});
                    } else {
                        // Legacy/debug events without a design retain the generic tracer.
                        var line=obj.AddComponent<LineRenderer>();line.sharedMaterial=WeaponColor(shot);line.positionCount=2;line.startWidth=.055f;line.endWidth=.02f;
                        line.SetPosition(0,from);line.SetPosition(1,to);
                        line.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;line.receiveShadows=false;
                        flashes.Add(new Flash{Object=obj,Until=effectTime+.12f});
                    }
                    if(shot.Splash>0 && flashes.Count<64) {
                        var ring=new GameObject("Splash impact");ring.transform.SetParent(effectsRoot,false);var r=ring.AddComponent<LineRenderer>();r.sharedMaterial=WeaponColor(shot);r.positionCount=25;r.startWidth=r.endWidth=.05f;
                        r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;r.receiveShadows=false;
                        for(int i=0;i<25;i++){float a=i*Mathf.PI*2/24;r.SetPosition(i,new Vector3(shot.To.X+Mathf.Cos(a)*shot.Splash,shot.Flying?1.7f:.12f,shot.To.Y+Mathf.Sin(a)*shot.Splash));}
                        flashes.Add(new Flash{Object=ring,Until=effectTime+.22f});
                    }
                }
                if(weaponReady&&!shot.Chained&&(audible==null||shot.Splash>audible.Splash)&&
                    (InView(shot.From,1.3f)||InView(shot.To,shot.Flying?1.7f:.3f)))audible=shot;
            }
            if(audible!=null){DispatchSound(soundBank.Get(audible.Design)??(audible.Splash>0?emberClip:boltClip));nextShotSound=alertTime+.12f;}
        }
        void OnDestroy(){soundBank?.Dispose();if(leakClip!=null)Destroy(leakClip);if(boltClip!=null)Destroy(boltClip);if(emberClip!=null)Destroy(emberClip);}
    }
}
