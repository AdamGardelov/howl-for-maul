using UnityEngine;
namespace FrostMaze
{
    public sealed partial class PrototypeHud
    {
        GUIStyle frameStyle;
        Texture2D Beveled(Color body,Color edge,bool pressed=false)
        {
            const int size=64;
            var texture=new Texture2D(size,size,TextureFormat.RGBA32,false){filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};
            for(int y=0;y<size;y++)for(int x=0;x<size;x++) {
                int dx=Mathf.Min(x,size-1-x),dy=Mathf.Min(y,size-1-y),d=Mathf.Min(dx,dy);
                float grain=((x*73+y*151+x*y*7)&63)/63f;
                bool light=pressed?y<size/2:y>=size/2;
                float bloom=Mathf.Sin(x/(float)(size-1)*Mathf.PI)*Mathf.Sin(y/(float)(size-1)*Mathf.PI);
                Color c=body*(.82f+grain*.06f+y/(float)size*.18f+bloom*.16f);
                if(d==0)c=new Color(.015f,.020f,.022f,.95f);
                else if(d==1)c=edge*(light?1.13f:.53f);
                else if(d==2)c=edge*(light?.61f:.36f);
                else if(d<6)c=body*(light?1.24f:.60f);
                // Slim metal rim; the broad interior is slate rather than stacked gold outlines.
                if(dx+dy<5)c=Color.clear;
                else if(dx+dy<7)c=edge*(light?.86f:.47f);
                if(dx+dy>=5)c.a=1; // Texture shading must not make text panels translucent.
                texture.SetPixel(x,y,c);
            }
            texture.Apply();textures.Add(texture);return texture;
        }
        void Fill(Rect rect,Color color){var old=GUI.color;GUI.color=color;GUI.DrawTexture(rect,Texture2D.whiteTexture);GUI.color=old;}
        void Frame(Rect rect)
        {
            if(frameStyle==null) {
                frameStyle=new GUIStyle{border=new RectOffset(8,8,8,8)};
                frameStyle.normal.background=Beveled(new Color(.068f,.086f,.076f),new Color(.40f,.40f,.30f));
            }
            GUI.Box(rect,GUIContent.none,frameStyle);
            var trim=new Color(.62f,.51f,.31f);
            for(int side=0;side<2;side++)for(int end=0;end<2;end++){
                float x=side==0?rect.x+4:rect.xMax-18,y=end==0?rect.y+4:rect.yMax-5;
                Fill(new Rect(x,y,14,1),trim);
                Fill(new Rect(side==0?x:x+13,end==0?y:y-9,1,10),trim*.8f);
            }
            if(rect.height>100){float cx=rect.center.x;
                Fill(new Rect(cx-28,rect.y+2,56,1),trim*.8f);
                var matrix=GUI.matrix;GUIUtility.RotateAroundPivot(45,new Vector2(cx,rect.y+3));
                Fill(new Rect(cx-2,rect.y+1,4,4),new Color(.80f,.66f,.39f));GUI.matrix=matrix;
            }
        }
        GUIStyle resourceValue,resourceCaption;
        void DrawResourceCounters(FrostMaze.Simulation.World world,bool narrow)
        {
            if(resourceValue==null){resourceValue=new GUIStyle(number){fontSize=15,alignment=TextAnchor.MiddleLeft,padding=new RectOffset(),wordWrap=false};resourceCaption=new GUIStyle(small){fontSize=10,alignment=TextAnchor.MiddleLeft,padding=new RectOffset(),wordWrap=false};}
            float width=narrow?314:338;var box=GUILayoutUtility.GetRect(width,32,GUILayout.Width(width),GUILayout.Height(32));
            string[] names={"GOLD","WOOD","LIVES","WAVE"};string[] values={world.Gold.ToString(),world.Wood.ToString(),world.Lives.ToString(),Mathf.Max(0,world.WaveIndex+1)+"/"+world.Config.Waves.Length};
            for(int i=0;i<4;i++){
                float w=width/4;var tile=new Rect(box.x+i*w+2,box.y,w-4,32);
                if(i>0)Fill(new Rect(tile.x-3,tile.y+5,1,24),new Color(.3f,.33f,.27f,.7f));
                DrawResourceGlyph(new Rect(tile.x+2,tile.y+9,19,19),i);
                GUI.Label(new Rect(tile.x+25,tile.y,tile.width-25,12),names[i],resourceCaption);
                var old=resourceValue.normal.textColor;resourceValue.normal.textColor=i==2&&world.Lives<=5?new Color(1,.48f,.35f):i==1?new Color(.68f,.81f,.57f):new Color(.94f,.83f,.58f);
                GUI.Label(new Rect(tile.x+25,tile.y+11,tile.width-25,21),values[i],resourceValue);resourceValue.normal.textColor=old;
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
