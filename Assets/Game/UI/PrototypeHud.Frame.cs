using UnityEngine;
namespace FrostMaze
{
    public sealed partial class PrototypeHud
    {
        Texture2D stone;
        Texture2D Beveled(Color body,Color edge)
        {
            var texture=new Texture2D(32,32,TextureFormat.RGBA32,false){filterMode=FilterMode.Bilinear};
            for(int y=0;y<32;y++)for(int x=0;x<32;x++) {
                bool border=x<2||y<2||x>29||y>29;
                texture.SetPixel(x,y,border?Color.Lerp(edge,Color.black,y<2?.45f:0):Color.Lerp(body,Color.white,y/31f*.035f));
            }
            texture.Apply();textures.Add(texture);return texture;
        }
        void Fill(Rect rect,Color color){var old=GUI.color;GUI.color=color;GUI.DrawTexture(rect,Texture2D.whiteTexture);GUI.color=old;}
        void Frame(Rect rect)
        {
            if(stone==null) {
                stone=new Texture2D(128,64,TextureFormat.RGB24,false){wrapMode=TextureWrapMode.Repeat,filterMode=FilterMode.Bilinear};
                for(int y=0;y<64;y++)for(int x=0;x<128;x++) {
                    int hash=(x*73856093)^(y*19349663);float noise=(hash&255)/255f;
                    bool seam=y%32==0||(x+(y/32)*64)%128==0;
                    float shade=seam?.68f:.93f+noise*.12f;
                    stone.SetPixel(x,y,new Color(.11f,.135f,.145f)*shade);
                }
                stone.Apply();textures.Add(stone);
            }
            Fill(new Rect(rect.x-2,rect.y-2,rect.width+4,rect.height+4),new Color(.018f,.025f,.029f,.95f));
            GUI.DrawTextureWithTexCoords(rect,stone,new Rect(0,0,rect.width/128,rect.height/64));
            Fill(new Rect(rect.x+5,rect.y+5,rect.width-10,rect.height-10),new Color(.025f,.04f,.047f,.94f));
            var trim=new Color(.40f,.36f,.25f);
            Fill(new Rect(rect.x+3,rect.y+3,rect.width-6,1),trim);
            Fill(new Rect(rect.x+3,rect.yMax-4,rect.width-6,1),new Color(.22f,.22f,.19f));
            foreach(float x in new[]{rect.x+2,rect.xMax-6})foreach(float y in new[]{rect.y+2,rect.yMax-6})Fill(new Rect(x,y,4,4),new Color(.55f,.48f,.32f));
        }
        void DrawRemoveIcon(Rect rect)
        {
            var old=GUI.matrix;var color=new Color(.88f,.43f,.32f);
            GUIUtility.RotateAroundPivot(45,rect.center);
            Fill(new Rect(rect.center.x-3,rect.y+3,6,rect.height-6),color);
            Fill(new Rect(rect.x+3,rect.center.y-3,rect.width-6,6),color);
            GUI.matrix=old;
        }
    }
}
