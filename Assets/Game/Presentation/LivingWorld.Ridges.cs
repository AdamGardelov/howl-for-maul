using UnityEngine;

namespace FrostMaze
{
    public sealed partial class LivingWorld
    {
        readonly Geometry ridgeMeadow=new Geometry();
        Texture2D ridgePaint;
        public int RidgeQuads {get;private set;}

        void BuildConnectedRidges(MapScenery scenery)
        {
            float cell=config.LayoutCellSize;
            ridgePaint=scenery.ShelfPaint;
            int columns=config.LayoutRows[0].Length,rows=config.LayoutRows.Length;
            var heights=new float[columns,rows];
            bool Solid(int x,int z)=>x>=0&&z>=0&&x<columns&&z<rows&&config.LayoutRows[rows-1-z][x]=='#';
            // Shared vertices at cell centres keep every triangle wholly inside blocked cells.
            // A broad height field joins shelves, while the original retaining edge stays exact.
            for(int z=0;z<rows;z++)for(int x=0;x<columns/2;x++){
                if(!Solid(x,z))continue;
                float px=(x+.5f)*cell,pz=(z+.5f)*cell,distance=3.5f;
                for(int dz=-7;dz<=7;dz++)for(int dx=-7;dx<=7;dx++)
                    if(!Solid(x+dx,z+dz))distance=Mathf.Min(distance,new Vector2(dx,dz).magnitude*cell);
                float lift=Mathf.SmoothStep(0,1,Mathf.Clamp01((distance-.5f)/2.2f));
                float h=lift*(1.1f+Mathf.PerlinNoise(px*.23f+19,pz*.19f+7)*1.35f);
                // Retain level working aprons around the existing inhabited landmarks and fires.
                foreach(var p in scenery.LandmarkPositions)h*=Mathf.SmoothStep(0,1,(new Vector2(px-p.x,pz-p.z).magnitude-1.65f)/1.4f);
                foreach(var p in scenery.FirePositions)h*=Mathf.SmoothStep(0,1,(new Vector2(px-p.x,pz-p.z).magnitude-.45f)/.7f);
                if(!ClearsFlight(config,px,pz,cell*1.5f))h=Mathf.Min(h,.63f);
                heights[x,z]=.616f+h;
            }
            // Meet on the exact reflection plane, without leaving a flat slot down the centre.
            for(int z=0;z<rows;z++)heights[columns/2,z]=heights[columns/2-1,z];
            Vector3 Point(int x,int z)=>new Vector3(x==columns/2?width*.5f:(x+.5f)*cell,heights[x,z],(z+.5f)*cell);
            Vector3 Normal(int x,int z){
                float l=Solid(x-1,z)?heights[x-1,z]:heights[x,z],r=x+1<columns/2&&Solid(x+1,z)?heights[x+1,z]:heights[x,z];
                float b=Solid(x,z-1)?heights[x,z-1]:heights[x,z],f=Solid(x,z+1)?heights[x,z+1]:heights[x,z];
                return new Vector3(x==columns/2?0:l-r,cell*2,b-f).normalized;
            }
            for(int z=0;z<rows-1;z++)for(int x=0;x<columns/2;x++){
                if(!Solid(x,z)||!Solid(x+1,z)||!Solid(x,z+1)||!Solid(x+1,z+1))continue;
                var a=Point(x,z);var b=Point(x,z+1);var c=Point(x+1,z+1);var d=Point(x+1,z);
                float top=Mathf.Max(a.y,b.y,c.y,d.y);
                if(top<.65f)continue;
                // Blend stone continuously per vertex; material boundaries must not reveal tiles.
                var g=ridgeMeadow;int start=g.V.Count;
                g.Quad(a,b,c,d,Color.white);
                g.N[start]=Normal(x,z);g.N[start+1]=Normal(x,z+1);g.N[start+2]=Normal(x+1,z+1);g.N[start+3]=Normal(x+1,z);
                for(int v=start;v<g.V.Count;v++){
                    g.UV[v]=new Vector2(g.V[v].x/config.Width,g.V[v].z/config.Height);
                    float slope=Mathf.SmoothStep(0,1,(1-g.N[v].y-.035f)/.22f);
                    float foot=Mathf.SmoothStep(0,1,(g.V[v].y-.616f)/.45f);
                    g.C[v]=new Color(1,1,1,slope*foot*.92f);
                }
                RidgeQuads++;
            }
        }
    }
}
