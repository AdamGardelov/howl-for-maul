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
        // Shared world-space pigments keep inner shelves and the surrounding landscape coherent.
        // Large material patches carry the shape; small grain is restrained at gameplay zoom.
        internal static Color RaisedSurfaceColor(float wx,float wz,bool ice)
        {
            float field=SnowNoise(wx,wz,.16f,41,7),breakup=SnowNoise(wx,wz,.62f,13,29);
            float grain=SnowNoise(wx,wz,3.5f,9,3),strata=SnowNoise(wx,wz,1.3f,51,19);
            float exposed=PaintMask(.43f,.62f,field*.8f+breakup*.2f);
            float seam=(1-PaintMask(.01f,.045f,Mathf.Abs(strata-.5f)))*PaintMask(.48f,.65f,breakup);
            if(ice) {
                var snow=Color.Lerp(new Color(.44f,.55f,.59f),new Color(.57f,.66f,.68f),SnowNoise(wx,wz,.075f,41,7));
                var slate=Color.Lerp(new Color(.26f,.34f,.37f),new Color(.38f,.44f,.45f),breakup);
                slate=Color.Lerp(slate,new Color(.12f,.22f,.25f),seam*.12f);
                // Sheltered lichen, bluish scoured ice, and a narrow pale frost rim.
                float iceMask=PaintMask(.57f,.7f,SnowNoise(wx,wz,.3f,87,43));
                var glazed=Color.Lerp(new Color(.23f,.43f,.48f),new Color(.38f,.57f,.61f),strata);
                glazed=Color.Lerp(glazed,new Color(.64f,.75f,.75f),seam*.24f);
                slate=Color.Lerp(slate,glazed,iceMask*.8f);
                float lichen=PaintMask(.57f,.75f,SnowNoise(wx,wz,1.7f,77,13))*(1-iceMask);
                slate=Color.Lerp(slate,new Color(.37f,.41f,.29f),lichen*.38f);
                var result=Color.Lerp(snow,slate,exposed*.88f);
                float rim=PaintMask(.30f,.46f,exposed)*(1-PaintMask(.47f,.65f,exposed));
                result=Color.Lerp(result,new Color(.58f,.69f,.7f),rim*.08f);
                return result*(.965f+grain*.07f);
            }
            var rock=Color.Lerp(new Color(.24f,.27f,.27f),new Color(.40f,.40f,.35f),breakup);
            rock=Color.Lerp(rock,new Color(.15f,.19f,.19f),seam*.12f);
            var grass=Color.Lerp(new Color(.19f,.27f,.20f),new Color(.35f,.39f,.25f),strata);
            float fibers=SnowNoise(wx,wz,5,31,61);
            grass*=.94f+fibers*.12f;
            var earth=Color.Lerp(rock,grass,exposed*.88f);
            float rust=PaintMask(.57f,.73f,SnowNoise(wx,wz,.38f,63,83))*(1-exposed);
            earth=Color.Lerp(earth,new Color(.40f,.29f,.19f),rust*.32f);
            float ash=PaintMask(.64f,.8f,breakup);
            return Color.Lerp(earth,new Color(.17f,.20f,.20f),ash*.28f)*(.96f+grain*.08f);
        }
        internal static Color CliffSurfaceColor(float u,float v,bool ice)
        {
            float grain=Mathf.PerlinNoise(u*83+7,v*41+3),wash=Mathf.PerlinNoise(u*7+31,v*4+9);
            if(ice) {
                float bend=Mathf.Sin(u*Mathf.PI*8)*.055f+Mathf.Sin(u*Mathf.PI*18)*.025f;
                float level=v*4+bend,band=Mathf.Repeat(level,1);
                var rock=Color.Lerp(new Color(.14f,.24f,.28f),new Color(.34f,.44f,.46f),wash*.6f+grain*.4f);
                rock*=.78f+PaintMask(.02f,.22f,band)*.22f;
                float split=(1-PaintMask(.015f,.055f,Mathf.Abs(Mathf.Sin(u*Mathf.PI*18+v*2))))*PaintMask(.3f,.7f,grain);
                rock=Color.Lerp(rock,new Color(.08f,.17f,.21f),split*.5f);
                float iceSheet=PaintMask(.62f,.78f,Mathf.PerlinNoise(u*15+83,v*.8f+17));
                rock=Color.Lerp(rock,new Color(.32f,.52f,.58f),iceSheet*.38f);
                float frost=PaintMask(.78f,.99f,v)*(1-PaintMask(.45f,.7f,grain));
                return Color.Lerp(rock,new Color(.59f,.70f,.72f),frost*.7f);
            }
            int course=Mathf.FloorToInt(v*3);float block=u*4+(course%2)*.5f;int bx=Mathf.FloorToInt(block);
            float joint=Mathf.Min(Mathf.Min(Mathf.Repeat(block,1),1-Mathf.Repeat(block,1)),Mathf.Min(Mathf.Repeat(v*3,1),1-Mathf.Repeat(v*3,1)));
            var masonry=Color.Lerp(new Color(.16f,.18f,.19f),new Color(.37f,.34f,.28f),Hash(bx,course)*.45f+wash*.35f+grain*.2f);
            masonry*=.77f+.23f*PaintMask(0,.22f,Mathf.Repeat(v*3,1));
            masonry=Color.Lerp(masonry,new Color(.08f,.12f,.12f),(1-PaintMask(.009f,.04f,joint))*.75f);
            float moss=PaintMask(.45f,.7f,Mathf.PerlinNoise(u*18+3,v*3+13))*(1-PaintMask(.2f,.8f,v));
            masonry=Color.Lerp(masonry,new Color(.20f,.29f,.18f),moss*.62f);
            float stain=PaintMask(.56f,.74f,Mathf.PerlinNoise(u*25+59,v*1.2f+31));
            return Color.Lerp(masonry,new Color(.43f,.25f,.13f),stain*.32f);
        }
        // Match the exterior texture's 64-unit repeat without a visible tile boundary.
        static float SnowNoise(float wx,float wz,float frequency,float ox,float oz)
        {
            // Ease the small join band to zero slope, so fine mineral seams cannot pop at repeats.
            float Wrap(float value) {
                float t=Mathf.Repeat(value,64),edge=Mathf.Min(t,64-t);
                if(edge<.35f){float f=edge/.35f;edge=.35f*f*f*(2-f);}
                return t<32?edge:64-edge;
            }
            float x=Wrap(wx),z=Wrap(wz);
            float u=Mathf.SmoothStep(0,1,x/64),v=Mathf.SmoothStep(0,1,z/64);
            float Noise(float dx,float dz)=>Mathf.PerlinNoise(dx*frequency+ox,dz*frequency+oz);
            return Mathf.Lerp(Mathf.Lerp(Noise(x,z),Noise(x-64,z),u),Mathf.Lerp(Noise(x,z-64),Noise(x-64,z-64),u),v);
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
            var ground=new Color[size*size];var cap=new Color[size*size];var water=new Color[size*size];
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
                // Subtle route wear and damp moss break up the uniform tiled floor at play zoom.
                float travel=PaintMask(.45f,.8f,Mathf.PerlinNoise(wx*.08f+12,wz*.035f+4))*(1-edge*.7f);
                stone=Color.Lerp(stone,ice?new Color(.36f,.43f,.40f):new Color(.36f,.31f,.23f),travel*.15f);
                float moss=PaintMask(.58f,.76f,Mathf.PerlinNoise(wx*.38f+8,wz*.38f+20))*edge*(1-drift);
                stone=Color.Lerp(stone,ice?new Color(.20f,.32f,.27f):new Color(.21f,.30f,.23f),moss*.26f);
                ground[y*size+x]=stone;
                cap[y*size+x]=RaisedSurfaceColor(wx,wz,ice);
                float current=Mathf.Sin(wz*2.4f+Mathf.Sin(wx*.63f)*1.7f)*.5f+.5f;
                float pool=Mathf.PerlinNoise(wx*.12f+7,wz*.15f+11);
                water[y*size+x]=Color.Lerp(ice?new Color(.045f,.13f,.18f):new Color(.045f,.085f,.085f),ice?new Color(.10f,.25f,.29f):new Color(.105f,.18f,.15f),pool*.8f+current*.12f);
            }
            waterTexture=PaintedTexture("Original glacial pools and foundry channels",size,size,water);
            groundTexture=PaintedTexture("Original worn flagstone and edge wash",size,size,ground);
            capTexture=PaintedTexture(ice?"Original snow over blue slate":"Original weathered foundry slate",size,size,cap);
            const int sideW=512,sideH=128;var faces=new Color[sideW*sideH];
            for(int y=0;y<sideH;y++)for(int x=0;x<sideW;x++) {
                float u=x/(float)sideW,v=y/(float)(sideH-1);
                faces[y*sideW+x]=CliffSurfaceColor(u,v,ice);
            }
            wallTexture=PaintedTexture(ice?"Original frozen stratified slate":"Original mossed and oxidized masonry",sideW,sideH,faces,true);
            wallTexture.wrapModeV=TextureWrapMode.Clamp;
        }
    }
}
