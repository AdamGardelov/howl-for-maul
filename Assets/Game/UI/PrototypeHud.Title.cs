using UnityEngine;
namespace FrostMaze
{
    public sealed partial class PrototypeHud
    {
        int titlePage;
        Texture2D titleShade;
        GUIStyle titleAction,titlePlay,titleCaption;
        Vector2 titleScroll;
        void DrawTitleScreen()
        {
            if(titleShade==null){
                titleShade=new Texture2D(128,1,TextureFormat.RGBA32,false){wrapMode=TextureWrapMode.Clamp};
                for(int x=0;x<128;x++)titleShade.SetPixel(x,0,new Color(.018f,.029f,.033f,Mathf.Lerp(.96f,0,Mathf.SmoothStep(0,1,x/127f))));
                titleShade.Apply();textures.Add(titleShade);
                titleAction=new GUIStyle(button){fixedHeight=46,fontSize=17,alignment=TextAnchor.MiddleLeft,padding=new RectOffset(20,16,10,10),margin=new RectOffset(0,0,4,4)};
                titlePlay=new GUIStyle(titleAction);titlePlay.normal.background=primary.normal.background;titlePlay.normal.textColor=primary.normal.textColor;titlePlay.fontStyle=FontStyle.Bold;
                titleCaption=new GUIStyle(section){fontSize=12,alignment=TextAnchor.MiddleCenter};
            }
            var matrix=GUI.matrix;float scale=game.UiScale;
            GUI.matrix=Matrix4x4.Scale(new Vector3(scale,scale,1));
            float w=Screen.width/scale,h=Screen.height/scale;
            GUI.DrawTexture(new Rect(0,0,Mathf.Min(w,780),h),titleShade);
            DrawBrand(new Rect(46,42,350,242));
            GUI.Label(new Rect(46,293,350,24),"BUILD TOGETHER. KEEP THE HEARTH LIT.",titleCaption);
            GUILayout.BeginArea(new Rect(68,340,306,h-407));
            if(titlePage==0){
                if(HudButton("◆  PLAY",titlePlay)){game.OpenSetup();onlineForm=false;}
                if(game.CanReturnToMatch&&HudButton("RESUME MATCH",titleAction))game.ReturnToMatch();
                if(HudButton("SETTINGS",titleAction))titlePage=1;
                if(HudButton("CREDITS",titleAction))titlePage=2;
                if(HudButton("QUIT",titleAction))game.QuitGame();
            }else{
                if(HudButton("‹  BACK",button))titlePage=0;
                titleScroll=GUILayout.BeginScrollView(titleScroll);
                if(titlePage==1){
                    GUILayout.Label("SETTINGS",title);
                    game.SoundEnabled=GUILayout.Toggle(game.SoundEnabled,"Combat sound",button);
                    GUILayout.Label("Effects volume",label);game.EffectsVolume=GUILayout.HorizontalSlider(game.EffectsVolume,0,1);
                    GUILayout.Label("Music volume",label);game.MusicVolume=GUILayout.HorizontalSlider(game.MusicVolume,0,1);
                    if(!Application.isEditor)Screen.fullScreen=GUILayout.Toggle(Screen.fullScreen,"Fullscreen",button);
                }else{
                    GUILayout.Label("HOWL FOR MAUL",title);
                    GUILayout.Label("An independent cooperative maze defense game. Inspired by the community spirit of classic mauls.",label);
                    GUILayout.Space(16);GUILayout.Label("MUSIC · SCOTT BUCKLEY",section);
                    GUILayout.Label("Snowfall — Rimewatch\nSignal to Noise — Ironfold\nLicensed under CC BY 4.0.\nRecordings unmodified; runtime volume and looping.",label);
                    if(HudButton("COMPOSER & MUSIC",button))Application.OpenURL("https://www.scottbuckley.com.au/library/");
                    if(HudButton("CC BY 4.0 LICENSE",button))Application.OpenURL("https://creativecommons.org/licenses/by/4.0/");
                    GUILayout.Space(12);GUILayout.Label("Full notices are included with your download in THIRD-PARTY-NOTICES.md.",small);
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndArea();
            var tint=GUI.color;GUI.color=new Color(.018f,.029f,.033f,.78f);GUI.DrawTexture(new Rect(0,h-64,w,64),Texture2D.whiteTexture);GUI.color=tint;
            GUI.Label(new Rect(68,h-47,500,24),"FRIENDS PLAYTEST · 0.1.0",small);
            var credit=new GUIStyle(small){alignment=TextAnchor.LowerRight};
            GUI.Label(new Rect(w-400,h-56,376,36),"“"+(game.World.Config.Theme=="iron"?"Signal to Noise":"Snowfall")+"” · Scott Buckley\nCC BY 4.0 · scottbuckley.com.au",credit);
            GUI.matrix=matrix;
        }
    }
}
