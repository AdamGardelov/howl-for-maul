using UnityEngine;
namespace FrostMaze
{
    public sealed partial class PrototypeHud
    {
        Texture2D stone;
        Texture2D Beveled(Color body,Color edge,bool pressed=false)
        {
            const int size=64;
            var texture=new Texture2D(size,size,TextureFormat.RGBA32,false){filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};
            for(int y=0;y<size;y++)for(int x=0;x<size;x++) {
                int dx=Mathf.Min(x,size-1-x),dy=Mathf.Min(y,size-1-y),d=Mathf.Min(dx,dy);
                float grain=((x*73+y*151+x*y*7)&63)/63f;
                bool light=pressed?y<size/2:y>=size/2;
                Color c=body*(.91f+grain*.08f+y/(float)size*.12f);
                if(d==0)c=new Color(.015f,.020f,.022f,.95f);
                else if(d<3)c=edge*(light?1.18f:.48f);
                else if(d==3)c=Color.Lerp(edge,Color.white,light?.30f:.02f);
                else if(d<6)c=edge*(light?.56f:.30f);
                else if(d==6)c=new Color(.018f,.023f,.025f);
                else if(d==7)c=body*(light?1.7f:.65f);
                // Clipped metal corners and an inset pin stay intact with nine-slice scaling.
                if(dx+dy<5)c=Color.clear;
                if(dx>=3&&dy>=3&&dx+dy<9)c=edge*(light?1.25f:.65f);
                if(dx==10&&dy==10)c=edge*.8f;
                texture.SetPixel(x,y,c);
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
                    stone.SetPixel(x,y,new Color(.16f,.18f,.18f)*shade);
                }
                stone.Apply();textures.Add(stone);
            }
            Fill(new Rect(rect.x-2,rect.y-2,rect.width+4,rect.height+4),new Color(.018f,.025f,.029f,.95f));
            GUI.DrawTextureWithTexCoords(rect,stone,new Rect(0,0,rect.width/128,rect.height/64));
            Fill(new Rect(rect.x+8,rect.y+8,rect.width-16,rect.height-16),new Color(.025f,.04f,.047f,.94f));
            var trim=new Color(.57f,.46f,.27f);
            Fill(new Rect(rect.x+3,rect.y+3,rect.width-6,1),trim);
            Fill(new Rect(rect.x+3,rect.yMax-4,rect.width-6,1),new Color(.22f,.22f,.19f));
            Fill(new Rect(rect.x+7,rect.y+7,rect.width-14,1),new Color(.16f,.14f,.10f));
            Fill(new Rect(rect.x+15,rect.y+5,rect.width-30,1),new Color(.70f,.59f,.36f,.65f));
            // A restrained central clasp echoes the title emblem without consuming HUD space.
            float cx=rect.center.x;Fill(new Rect(cx-17,rect.y+1,34,6),new Color(.07f,.09f,.09f));
            Fill(new Rect(cx-11,rect.y+2,22,2),trim);
            var matrix=GUI.matrix;GUIUtility.RotateAroundPivot(45,new Vector2(cx,rect.y+4));Fill(new Rect(cx-2,rect.y+2,4,4),new Color(.77f,.64f,.38f));GUI.matrix=matrix;
            Fill(new Rect(rect.x+7,rect.y+7,1,rect.height-14),new Color(.16f,.14f,.10f));
            Fill(new Rect(rect.xMax-8,rect.y+7,1,rect.height-14),new Color(.46f,.39f,.25f));
            foreach(float x in new[]{rect.x+1,rect.xMax-12})foreach(float y in new[]{rect.y+1,rect.yMax-12}){
                Fill(new Rect(x,y,11,11),new Color(.055f,.065f,.06f));
                Fill(new Rect(x+1,y+1,9,9),new Color(.47f,.39f,.25f));
                Fill(new Rect(x+2,y+2,7,7),new Color(.21f,.23f,.21f));
                Fill(new Rect(x+4,y+4,3,3),new Color(.74f,.63f,.39f));
            }
        }
        GUIStyle resourceValue,resourceCaption;
        void DrawResourceCounters(FrostMaze.Simulation.World world,bool narrow)
        {
            if(resourceValue==null){resourceValue=new GUIStyle(number){fontSize=14,alignment=TextAnchor.MiddleCenter,padding=new RectOffset()};resourceCaption=new GUIStyle(small){fontSize=9,alignment=TextAnchor.MiddleCenter,padding=new RectOffset(),wordWrap=false};}
            float width=narrow?282:310;var box=GUILayoutUtility.GetRect(width,32,GUILayout.Width(width),GUILayout.Height(32));
            string[] names={"GOLD","WOOD","LIVES","WAVE"};string[] values={world.Gold.ToString(),world.Wood.ToString(),world.Lives.ToString(),Mathf.Max(0,world.WaveIndex+1)+"/"+world.Config.Waves.Length};
            for(int i=0;i<4;i++){
                float w=width/4;var tile=new Rect(box.x+i*w+2,box.y,w-4,32);
                GUI.Box(tile,GUIContent.none,factionTile??card);
                GUI.Label(new Rect(tile.x,tile.y+1,tile.width,12),names[i],resourceCaption);
                var old=resourceValue.normal.textColor;resourceValue.normal.textColor=i==2&&world.Lives<=5?new Color(1,.48f,.35f):i==1?new Color(.68f,.81f,.57f):new Color(.94f,.83f,.58f);
                GUI.Label(new Rect(tile.x,tile.y+11,tile.width,20),values[i],resourceValue);resourceValue.normal.textColor=old;
            }
        }
        Texture2D removeGlyph;
        void DrawRemoveIcon(Rect rect)
        {
            if(removeGlyph==null){
                removeGlyph=new Texture2D(32,32,TextureFormat.RGBA32,false){filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};
                for(int y=0;y<32;y++)for(int x=0;x<32;x++)removeGlyph.SetPixel(x,y,x>2&&x<29&&y>2&&y<29&&(Mathf.Abs(x-y)<3||Mathf.Abs(x+y-31)<3)?new Color(.88f,.43f,.32f):Color.clear);
                removeGlyph.Apply();textures.Add(removeGlyph);
            }
            GUI.DrawTexture(rect,removeGlyph);
        }
    }
}
