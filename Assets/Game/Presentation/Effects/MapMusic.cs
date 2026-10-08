using UnityEngine;
namespace FrostMaze
{
    public sealed class MapMusic : MonoBehaviour
    {
        Prototype game;AudioSource source;
        public string Credit=>game.World.Config.Theme=="iron"?"Signal to Noise":"Snowfall";
        public void Initialize(Prototype prototype){game=prototype;source=gameObject.AddComponent<AudioSource>();source.playOnAwake=false;source.loop=true;source.spatialBlend=0;source.volume=0;source.clip=Resources.Load<AudioClip>("Music/"+(game.World.Config.Theme=="iron"?"signal-to-noise":"snowfall"));if(source.clip!=null)source.Play();}
        void Update(){if(game==null||source==null)return;float target=Mathf.Clamp01(game.MusicVolume)*.45f*(game.Paused||game.MenuOpen?.5f:1);source.volume=Mathf.MoveTowards(source.volume,target,Time.unscaledDeltaTime*.2f);}
    }
}
