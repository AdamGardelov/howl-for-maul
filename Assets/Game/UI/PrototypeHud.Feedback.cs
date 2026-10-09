using UnityEngine;
namespace FrostMaze
{
    public sealed partial class PrototypeHud
    {
        AudioClip uiClick;AudioSource uiAudio;float nextClick;
        void ClickFeedback()
        {
            if(!game.SoundEnabled||game.EffectsVolume<=0||Time.unscaledTime<nextClick)return;
            nextClick=Time.unscaledTime+.035f;
            if(uiAudio==null){uiAudio=gameObject.AddComponent<AudioSource>();uiAudio.playOnAwake=false;uiAudio.spatialBlend=0;uiAudio.priority=32;}
            if(uiClick==null){
                const int rate=24000;var samples=new float[1920];uint seed=421;float smooth=0;
                for(int i=0;i<samples.Length;i++){
                    float t=i/(float)rate;seed=1664525*seed+1013904223;
                    float noise=(seed&65535)/32767.5f-1;smooth=Mathf.Lerp(smooth,noise,.24f);
                    samples[i]=(.65f*smooth*Mathf.Exp(-t*110)+.2f*Mathf.Sin(t*2*Mathf.PI*180)*Mathf.Exp(-t*75))*Mathf.Min(1,i/12f);
                }
                uiClick=AudioClip.Create("Original soft metal latch",samples.Length,1,rate,false);uiClick.SetData(samples,0);
            }
            uiAudio.PlayOneShot(uiClick,.30f*game.EffectsVolume);
        }
        bool HudButton(string text,GUIStyle style,params GUILayoutOption[] options)
        {
            var content=new GUIContent(text);return HudButton(GUILayoutUtility.GetRect(content,style,options),content,style);
        }
        bool HudButton(Rect rect,string text,GUIStyle style)=>HudButton(rect,new GUIContent(text),style);
        bool HudButton(Rect rect,GUIContent content,GUIStyle style)
        {
            var offset=style.contentOffset;
            if(GUI.enabled&&GUIUtility.hotControl!=0&&rect.Contains(Event.current.mousePosition))style.contentOffset=offset+Vector2.one;
            bool activated=GUI.Button(rect,content,style);style.contentOffset=offset;
            if(activated){game.GuardWorldInput();ClickFeedback();}return activated;
        }
    }
}
