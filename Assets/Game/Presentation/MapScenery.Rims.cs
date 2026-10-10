using System.Collections.Generic;
using FrostMaze.Simulation;
using UnityEngine;

namespace FrostMaze
{
    public sealed partial class MapScenery
    {
        const int RimSteps=4;
        const float RimFoot=.035f;
        float[,] shelfHeights;
        float rimStep,shelfTop;
        public int RoundedShelfQuads {get;private set;}

        void PrepareShelfEdges(Scenario config,bool ice)
        {
            int columns=config.LayoutRows[0].Length,rows=config.LayoutRows.Length;
            float cell=config.LayoutCellSize;
            rimStep=cell/RimSteps;shelfTop=ice?.72f:.6f;
            var solid=new bool[columns,rows];
            for(int z=0;z<rows;z++)for(int x=0;x<columns;x++){
                char k=config.LayoutRows[rows-1-z][x];
                solid[x,z]=config.WalkableSymbols.IndexOf(k)<0&&k!='p'&&k!='W'&&k!='D';
            }
            bool Solid(int x,int z)=>x>=0&&z>=0&&x<columns&&z<rows&&solid[x,z];
            var cuts=new Vector4[columns,rows];
            for(int z=0;z<rows;z++)for(int x=0;x<columns;x++){
                if(!Solid(x,z))continue;
                for(int i=0;i<4;i++){
                    int sx=(i&1)==0?-1:1,sz=i<2?-1:1;
                    if(Solid(x+sx,z)||Solid(x,z+sz))continue;
                    bool diagonal=Solid(x-sx,z+sz)&&Solid(x+sx,z-sz);
                    var cut=cuts[x,z];cut[i]=cell*(diagonal?.94f:.42f);cuts[x,z]=cut;
                }
            }
            shelfHeights=new float[columns*RimSteps+1,rows*RimSteps+1];
            for(int iz=0;iz<shelfHeights.GetLength(1);iz++)for(int ix=0;ix<shelfHeights.GetLength(0);ix++){
                float x=ix*rimStep,z=iz*rimStep;int cx=ix/RimSteps,cz=iz/RimSteps;
                if(!Solid(cx,cz)){shelfHeights[ix,iz]=RimFoot;continue;}
                float distance=cell*.46f;
                // Distance to the exact mask boundary rounds the upper edge inward only.
                // The low foot still reaches the original wall/tower sealing line.
                for(int dz=-1;dz<=1;dz++)for(int dx=-1;dx<=1;dx++){
                    int qx=cx+dx,qz=cz+dz;
                    if(Solid(qx,qz)){
                        var cut=cuts[qx,qz];
                        for(int i=0;i<4;i++){
                            float leg=cut[i];if(leg<=0)continue;
                            int sx=(i&1)==0?-1:1,sz=i<2?-1:1;
                            float cornerX=(qx+(sx>0?1:0))*cell,cornerZ=(qz+(sz>0?1:0))*cell;
                            float u=(cornerX-x)*sx,v=(cornerZ-z)*sz;
                            if(u>=0&&v>=0&&u+v<=leg){distance=0;continue;}
                            // Distance to a chamfer shared along a diagonal run, rather than
                            // a separate rounded bump at each square mask corner.
                            var a=new Vector2(cornerX-sx*leg,cornerZ);var b=new Vector2(cornerX,cornerZ-sz*leg);
                            var delta=b-a;float t=Mathf.Clamp01(Vector2.Dot(new Vector2(x,z)-a,delta)/delta.sqrMagnitude);
                            distance=Mathf.Min(distance,(new Vector2(x,z)-a-delta*t).magnitude);
                        }
                        continue;
                    }
                    float ex=Mathf.Max(qx*cell-x,0,x-(qx+1)*cell);
                    float ez=Mathf.Max(qz*cell-z,0,z-(qz+1)*cell);
                    distance=Mathf.Min(distance,Mathf.Sqrt(ex*ex+ez*ez));
                }
                shelfHeights[ix,iz]=Mathf.Lerp(RimFoot,shelfTop,Mathf.SmoothStep(0,1,distance/(cell*.46f)));
            }
        }
        void RoundedShelf(Batch batch,int col,int zCell)
        {
            int x=col*RimSteps,z=zCell*RimSteps;
            Vector3 Point(int dx,int dz)=>new Vector3((x+dx)*rimStep,shelfHeights[x+dx,z+dz],(z+dz)*rimStep);
            bool flat=true;
            for(int dz=0;dz<=RimSteps;dz++)for(int dx=0;dx<=RimSteps;dx++)
                if(shelfHeights[x+dx,z+dz]<shelfTop-.001f)flat=false;
            if(flat){batch.Quad(Point(0,0),Point(0,RimSteps),Point(RimSteps,RimSteps),Point(RimSteps,0));return;}
            for(int dz=0;dz<RimSteps;dz++)for(int dx=0;dx<RimSteps;dx++){
                batch.Quad(Point(dx,dz),Point(dx,dz+1),Point(dx+1,dz+1),Point(dx+1,dz));RoundedShelfQuads++;
            }
        }
        static void ShelfFoot(Batch batch,Vector3 a,Vector3 b)
        {
            batch.Quad(a,b,b+Vector3.up*RimFoot,a+Vector3.up*RimFoot);
        }
        void ShadeShelf(Mesh mesh,Material material,Scenario config,bool ice)
        {
            var normals=new List<Vector3>();var colors=new List<Color>();
            var originalNormals=mesh.normals;
            int maxX=shelfHeights.GetLength(0)-1,maxZ=shelfHeights.GetLength(1)-1;
            foreach(var p in mesh.vertices){
                int x=Mathf.Clamp(Mathf.RoundToInt(p.x/rimStep),0,maxX),z=Mathf.Clamp(Mathf.RoundToInt(p.z/rimStep),0,maxZ);
                float l=shelfHeights[Mathf.Max(0,x-1),z],r=shelfHeights[Mathf.Min(maxX,x+1),z];
                float b=shelfHeights[x,Mathf.Max(0,z-1)],f=shelfHeights[x,Mathf.Min(maxZ,z+1)];
                var n=p.y>shelfTop+.001f?originalNormals[normals.Count]:new Vector3(l-r,rimStep*2,b-f).normalized;normals.Add(n);
                colors.Add(new Color(1,1,1,Mathf.SmoothStep(0,1,(1-n.y)/.32f)*.94f));
            }
            mesh.SetNormals(normals);mesh.SetColors(colors);
            material.shader=Resources.Load<Shader>("World/HearthRidge");
            material.SetTexture("_BaseMap",capTexture);material.SetTexture("_RockMap",Resources.Load<Texture2D>("World/HearthBedrock"));
            material.SetFloat("_Wind",0);material.SetFloat("_MapWidth",config.Width);material.SetFloat("_MapHeight",config.Height);
            material.SetColor("_RockTint",ice?new Color(.75f,.92f,1.02f):new Color(.80f,.86f,.81f));
        }
    }
}
