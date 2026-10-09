using UnityEngine;
using FrostMaze.Simulation;
namespace FrostMaze
{
    public sealed partial class MapScenery
    {
        // Three planes give cliff faces a grounded base, upright stone and a worn crown.
        // Every vertex stays inside its original blocked source cell.
        static void Cliff(Batch b,Vector3 a,Vector3 d,Vector3 topD,Vector3 topA)
        {
            var lowA=Vector3.Lerp(a,topA,.16f);var lowD=Vector3.Lerp(d,topD,.16f);
            var highA=Vector3.Lerp(a,topA,.3f);highA.y=topA.y*.76f;
            var highD=Vector3.Lerp(d,topD,.3f);highD.y=topD.y*.76f;
            b.Quad(a,d,lowD,lowA);b.Quad(lowA,lowD,highD,highA);b.Quad(highA,highD,topD,topA);
        }
        static float Hash(int x,int y)
        {
            unchecked {uint h=(uint)(x*374761393+y*668265263);h=(h^(h>>13))*1274126177;return (h&65535)/65535f;}
        }
        static Texture2D PaintedTexture(string name,int width,int height,Color[] pixels,bool repeat=false)
        {
            var texture=new Texture2D(width,height,TextureFormat.RGB24,true){name=name,wrapMode=repeat?TextureWrapMode.Repeat:TextureWrapMode.Clamp,filterMode=FilterMode.Trilinear,anisoLevel=4};
            texture.SetPixels(pixels);texture.Apply(true,true);return texture;
        }
        // Unity SmoothStep interpolates endpoints; threshold masks must normalize their input first.
        static float PaintMask(float low,float high,float value)=>Mathf.SmoothStep(0,1,Mathf.InverseLerp(low,high,value));
        // The inner ledges and exterior share one restrained wash. Keep the snow below
        // near-white so lane silhouettes, frost plants and tower effects remain readable.
        internal static Color RaisedSurfaceColor(float wx,float wz,bool ice)
        {
            float detail=Mathf.PerlinNoise(wx*1.2f+5,wz*1.2f+23);
            if(ice) {
                float drift=Mathf.PerlinNoise(wx*.075f+41,wz*.075f+7);
                float grain=Mathf.PerlinNoise(wx*3.5f+9,wz*3.5f+3);
                var snow=Color.Lerp(new Color(.40f,.51f,.55f),new Color(.55f,.63f,.65f),drift);
                return snow*(.98f+detail*.025f+grain*.015f);
            }
            float wash=Mathf.PerlinNoise(wx*.22f+41,wz*.22f+7);
            var rock=Color.Lerp(new Color(.27f,.245f,.35f),new Color(.44f,.4f,.5f),wash*.8f+detail*.2f);
            return Color.Lerp(rock,new Color(.33f,.31f,.25f),PaintMask(.6f,.8f,detail)*.4f);
        }
        void PaintTerrain(Scenario c,bool ice)
        {
            bool Solid(int row,int col) {
                if(row<0||row>=c.LayoutRows.Length||col<0||col>=c.LayoutRows[row].Length)return false;
                char k=c.LayoutRows[row][col];return c.WalkableSymbols.IndexOf(k)<0&&k!='D'&&k!='W'&&k!='p';
            }
            // Cache a low-frequency distance wash; detailed painting does not rescan the map mask.
            const int fieldSize=128,size=1024;
            var edges=new float[fieldSize*fieldSize];
            for(int y=0;y<fieldSize;y++)for(int x=0;x<fieldSize;x++) {
                float wx=x*c.Width/(float)(fieldSize-1),wz=y*c.Height/(float)(fieldSize-1),best=2.5f;
                int col=Mathf.FloorToInt(wx/c.LayoutCellSize),row=c.LayoutRows.Length-1-Mathf.FloorToInt(wz/c.LayoutCellSize),reach=Mathf.CeilToInt(best/c.LayoutCellSize);
                for(int yy=row-reach;yy<=row+reach;yy++)for(int xx=col-reach;xx<=col+reach;xx++) {
                    if(!Solid(yy,xx))continue;
                    float left=xx*c.LayoutCellSize,bottom=(c.LayoutRows.Length-yy-1)*c.LayoutCellSize;
                    float dx=Mathf.Max(left-wx,Mathf.Max(0,wx-left-c.LayoutCellSize)),dz=Mathf.Max(bottom-wz,Mathf.Max(0,wz-bottom-c.LayoutCellSize));
                    best=Mathf.Min(best,Mathf.Sqrt(dx*dx+dz*dz));
                }
                edges[y*fieldSize+x]=1-Mathf.SmoothStep(0,1,best/2.5f);
            }
            var ground=new Color[size*size];var cap=new Color[size*size];
            for(int y=0;y<size;y++)for(int x=0;x<size;x++) {
                float wx=x*c.Width/(float)(size-1),wz=y*c.Height/(float)(size-1);
                float fx=x*(fieldSize-1)/(float)(size-1),fy=y*(fieldSize-1)/(float)(size-1);int ix=Mathf.Min(fieldSize-2,(int)fx),iy=Mathf.Min(fieldSize-2,(int)fy);
                float edge=Mathf.Lerp(Mathf.Lerp(edges[iy*fieldSize+ix],edges[iy*fieldSize+ix+1],fx-ix),Mathf.Lerp(edges[(iy+1)*fieldSize+ix],edges[(iy+1)*fieldSize+ix+1],fx-ix),fy-iy);
                float broad=Mathf.PerlinNoise(wx*.13f+17,wz*.13f+31),grain=Mathf.PerlinNoise(wx*3.5f+9,wz*3.5f+3);
                float warp=(Mathf.PerlinNoise(wx*.55f,wz*.55f)-.5f)*.18f;
                float row=(wz+warp)/1.65f;int ry=Mathf.FloorToInt(row);
                float col=(wx+warp)/2.4f+(ry%2)*.47f;int cx=Mathf.FloorToInt(col);
                float u=Mathf.Repeat(col,1),v=Mathf.Repeat(row,1);
                float seam=Mathf.Min(Mathf.Min(u,1-u)*2.4f,Mathf.Min(v,1-v)*1.65f);
                float wear=Mathf.PerlinNoise(wx*.9f+61,wz*.9f+43);
                var stone=Color.Lerp(ice?new Color(.27f,.38f,.39f):new Color(.18f,.24f,.25f),ice?new Color(.43f,.52f,.49f):new Color(.32f,.38f,.36f),broad*.7f+Hash(cx,ry)*.3f);
                stone*=.94f+grain*.12f;
                // Broken seams are painted into flat ground, never physical cracks or build blockers.
                float cracks=(1-PaintMask(.018f,.075f,seam))*PaintMask(.19f,.49f,wear);
                stone=Color.Lerp(stone,ice?new Color(.13f,.235f,.26f):new Color(.11f,.125f,.14f),cracks*.6f);
                float lip=(1-PaintMask(.055f,.12f,seam))*(1-cracks);
                stone=Color.Lerp(stone,ice?new Color(.55f,.65f,.65f):new Color(.46f,.43f,.38f),lip*.16f);
                stone=Color.Lerp(stone,ice?new Color(.16f,.29f,.33f):new Color(.12f,.145f,.17f),edge*.42f);
                float drift=PaintMask(.58f,.79f,wear)*edge;
                stone=Color.Lerp(stone,ice?new Color(.58f,.72f,.74f):new Color(.25f,.285f,.20f),drift*(ice?.62f:.4f));
                ground[y*size+x]=stone;
                cap[y*size+x]=RaisedSurfaceColor(wx,wz,ice);
            }
            groundTexture=PaintedTexture("Original worn flagstone and edge wash",size,size,ground);
            capTexture=PaintedTexture(ice?"Original snow over blue slate":"Original weathered foundry slate",size,size,cap);
            const int sideW=512,sideH=128;var faces=new Color[sideW*sideH];
            for(int y=0;y<sideH;y++)for(int x=0;x<sideW;x++) {
                float u=x/(float)sideW,v=y/(float)sideH;int course=Mathf.FloorToInt(v*3);
                float block=u*4+(course%2)*.5f;int bx=Mathf.FloorToInt(block);
                float joint=Mathf.Min(Mathf.Min(Mathf.Repeat(block,1),1-Mathf.Repeat(block,1)),Mathf.Min(Mathf.Repeat(v*3,1),1-Mathf.Repeat(v*3,1)));
                float noise=Mathf.PerlinNoise(u*23+7,v*11+3);
                Color color=Color.Lerp(ice?new Color(.125f,.24f,.29f):new Color(.145f,.14f,.185f),ice?new Color(.32f,.47f,.5f):new Color(.32f,.29f,.33f),Hash(bx,course)*.35f+noise*.65f);
                color*=.76f+.24f*PaintMask(0,.28f,Mathf.Repeat(v*3,1));
                color=Color.Lerp(color,new Color(.075f,.11f,.14f),(1-PaintMask(.01f,.045f,joint))*.7f);
                faces[y*sideW+x]=color;
            }
            wallTexture=PaintedTexture("Original layered cliff masonry",sideW,sideH,faces,true);
        }
    }
}
