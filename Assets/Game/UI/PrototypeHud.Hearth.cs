using UnityEngine;
namespace FrostMaze
{
    public sealed partial class PrototypeHud
    {
        GUISkin hearthSkin;
        Texture2D[] resourceGlyphs;
        GUIStyle fieldStyle,keyStyle,quietAction,heroAction;
        void HearthControls()
        {
            hearthSkin=Instantiate(GUI.skin);hearthSkin.name="Howl hearth controls";
            hearthSkin.font=label.font;
            fieldStyle=new GUIStyle(hearthSkin.textField){font=label.font,fontSize=16,fixedHeight=30,
                padding=new RectOffset(10,10,5,5),border=new RectOffset(8,8,8,8)};
            fieldStyle.normal.background=Beveled(new Color(.033f,.052f,.048f),new Color(.29f,.35f,.29f),true);
            fieldStyle.focused.background=Beveled(new Color(.048f,.08f,.067f),new Color(.74f,.60f,.35f),true);
            fieldStyle.normal.textColor=fieldStyle.focused.textColor=label.normal.textColor;
            hearthSkin.textField=fieldStyle;hearthSkin.textArea=new GUIStyle(fieldStyle){fixedHeight=0,wordWrap=true};
            hearthSkin.settings.cursorColor=new Color(.98f,.83f,.52f);
            hearthSkin.settings.selectionColor=new Color(.36f,.51f,.37f,.75f);
            var track=new GUIStyle(hearthSkin.horizontalSlider){fixedHeight=8,border=new RectOffset(3,3,3,3),margin=new RectOffset(2,2,9,12)};
            track.normal.background=Beveled(new Color(.035f,.05f,.041f),new Color(.31f,.34f,.27f),true);
            hearthSkin.horizontalSlider=track;
            var thumb=new GUIStyle(hearthSkin.horizontalSliderThumb){fixedWidth=14,fixedHeight=18,border=new RectOffset(5,5,5,5),overflow=new RectOffset(0,0,5,5)};
            thumb.normal.background=Beveled(new Color(.48f,.43f,.27f),new Color(.75f,.65f,.41f));
            thumb.hover.background=thumb.active.background=button.hover.background;hearthSkin.horizontalSliderThumb=thumb;
            var scrollThumb=new GUIStyle(hearthSkin.verticalScrollbarThumb){border=new RectOffset(4,4,4,4)};
            scrollThumb.normal.background=Beveled(new Color(.20f,.26f,.20f),new Color(.42f,.44f,.32f));
            scrollThumb.hover.background=scrollThumb.active.background=button.hover.background;
            hearthSkin.verticalScrollbarThumb=scrollThumb;
            keyStyle=new GUIStyle(small){fontSize=10,alignment=TextAnchor.MiddleCenter,padding=new RectOffset(),wordWrap=false};
            keyStyle.normal.textColor=new Color(.70f,.73f,.61f);
            quietAction=new GUIStyle(button){fontSize=14,alignment=TextAnchor.MiddleLeft,padding=new RectOffset(10,34,4,4)};
            heroAction=new GUIStyle(quietAction);heroAction.normal.background=primary.normal.background;heroAction.normal.textColor=primary.normal.textColor;
        }
        bool TopAction(string text,string key,float width,bool prominent=false)
        {
            var rect=GUILayoutUtility.GetRect(width,30,GUILayout.Width(width),GUILayout.Height(30));
            bool result=HudButton(rect,new GUIContent(text),prominent?heroAction:quietAction);
            if(key.Length>0){var r=new Rect(rect.xMax-30,rect.y+7,25,16);
                Fill(r,new Color(.02f,.035f,.029f,.65f));GUI.Label(r,key,keyStyle);}
            return result;
        }
        bool SettingToggle(string text,bool value)
        {
            var row=GUILayoutUtility.GetRect(1,32,GUILayout.ExpandWidth(true));
            GUI.Label(new Rect(row.x,row.y+5,row.width-65,24),text,label);
            if(HudButton(new Rect(row.xMax-58,row.y+3,58,27),value?"ON":"OFF",value?primary:button))return !value;
            return value;
        }
        void DrawResourceGlyph(Rect rect,int kind)
        {
            if(resourceGlyphs==null){
                resourceGlyphs=new Texture2D[4];
                var colors=new[]{new Color(.94f,.72f,.31f),new Color(.64f,.73f,.40f),new Color(.75f,.84f,.64f),new Color(.58f,.77f,.77f)};
                for(int k=0;k<4;k++){
                    var tex=new Texture2D(48,48,TextureFormat.RGBA32,false){filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};
                    for(int y=0;y<48;y++)for(int x=0;x<48;x++){
                        float u=(x-23.5f)/24,v=(y-23.5f)/24;bool inside=false;float shade=.8f+v*.16f;
                        if(k==0){for(int coin=0;coin<3;coin++){float cy=-.36f+coin*.30f,cx=(coin==1?-.13f:.07f);
                            if((u-cx)*(u-cx)/.38f+(v-cy)*(v-cy)/.06f<1){inside=true;shade=1f-coin*.04f;}
                        }}
                        if(k==1){float a=u*.9f-v*.4f,b=u*.4f+v*.9f;
                            inside=Mathf.Abs(b)<.69f&&(Mathf.Abs(a-.3f)<.14f||Mathf.Abs(a+.12f)<.14f);shade=Mathf.Abs(v)<.1f?.52f:1f;}
                        if(k==2){float half=.60f*Mathf.Clamp01((v+.78f)*1.25f);
                            inside=v>-.78f&&v<.66f&&Mathf.Abs(u)<half;shade=Mathf.Abs(u)<.12f||Mathf.Abs(v-.15f)<.1f?1.15f:.67f;}
                        if(k==3){inside=Mathf.Abs(u+.43f)<.065f&&Mathf.Abs(v)<.77f||u>-.43f&&u<.65f&&v>.05f+u*.12f&&v<.69f-u*.2f;shade=.85f+u*.20f;}
                        tex.SetPixel(x,y,inside?new Color(colors[k].r*shade,colors[k].g*shade,colors[k].b*shade,1):Color.clear);
                    }
                    tex.Apply();textures.Add(tex);resourceGlyphs[k]=tex;
                }
            }
            GUI.DrawTexture(rect,resourceGlyphs[kind]);
        }
    }
}
