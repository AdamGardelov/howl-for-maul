using System.Collections.Generic;
using UnityEngine;
using FrostMaze.Simulation;
namespace FrostMaze
{
    // Cosmetic effects consume a bounded event history; damage remains authoritative in World.
    public sealed class CombatFeedback : MonoBehaviour
    {
        Prototype game; World observed; long serial;
        Material bolt,ember; AudioSource sound; AudioClip boltClip,emberClip;
        readonly List<Flash> flashes=new List<Flash>();
        sealed class Flash { public GameObject Object; public float Until; }
        public void Initialize(Prototype prototype)
        {
            game=prototype;bolt=game.MakeMaterial(new Color(.2f,1,.85f),true);ember=game.MakeMaterial(new Color(1,.5f,.12f),true);
            sound=gameObject.AddComponent<AudioSource>();sound.spatialBlend=0;sound.volume=.12f;
            boltClip=Tone("Original bolt",900,.055f);emberClip=Tone("Original cannon",130,.14f);
        }
        AudioClip Tone(string name,float frequency,float length)
        {
            const int rate=22050;var data=new float[(int)(length*rate)];
            for(int i=0;i<data.Length;i++){float t=(float)i/rate,envelope=1-(float)i/data.Length;data[i]=Mathf.Sin(2*Mathf.PI*(frequency*t-120*t*t))*envelope*envelope*.5f;}
            var clip=AudioClip.Create(name,data.Length,1,rate,false);clip.SetData(data,0);return clip;
        }
        void Update()
        {
            if(game==null||game.World==null)return;
            if(observed!=game.World){observed=game.World;serial=0;foreach(var f in flashes)Destroy(f.Object);flashes.Clear();}
            int sounds=0;
            foreach(var shot in observed.Shots)if(shot.Serial>serial) {
                serial=shot.Serial;
                if(flashes.Count<64) {
                    var obj=new GameObject("Tower shot");obj.transform.SetParent(transform);
                    var line=obj.AddComponent<LineRenderer>();line.sharedMaterial=shot.Splash>0?ember:bolt;line.positionCount=2;line.startWidth=.055f;line.endWidth=.02f;
                    line.SetPosition(0,new Vector3(shot.From.X,1.3f,shot.From.Y));line.SetPosition(1,new Vector3(shot.To.X,shot.Flying?1.7f:.3f,shot.To.Y));
                    flashes.Add(new Flash{Object=obj,Until=Time.unscaledTime+.12f});
                    if(shot.Splash>0) {
                        var ring=new GameObject("Splash impact");ring.transform.SetParent(transform);var r=ring.AddComponent<LineRenderer>();r.sharedMaterial=ember;r.positionCount=25;r.startWidth=r.endWidth=.05f;
                        for(int i=0;i<25;i++){float a=i*Mathf.PI*2/24;r.SetPosition(i,new Vector3(shot.To.X+Mathf.Cos(a)*shot.Splash,.12f,shot.To.Y+Mathf.Sin(a)*shot.Splash));}
                        flashes.Add(new Flash{Object=ring,Until=Time.unscaledTime+.22f});
                    }
                }
                if(game.SoundEnabled&&sounds++<2)sound.PlayOneShot(shot.Splash>0?emberClip:boltClip);
            }
            for(int i=flashes.Count-1;i>=0;i--)if(Time.unscaledTime>=flashes[i].Until){Destroy(flashes[i].Object);flashes.RemoveAt(i);}
        }
        void OnDestroy(){if(boltClip!=null)Destroy(boltClip);if(emberClip!=null)Destroy(emberClip);}
    }
}
