using System.Collections.Generic;
using UnityEngine;
using FrostMaze.Simulation;
namespace FrostMaze
{
    public sealed partial class MapScenery
    {
        readonly List<Vector4> scenerySpaces=new List<Vector4>();
        readonly List<Vector3> landmarks=new List<Vector3>();
        public IReadOnlyList<Vector3> LandmarkPositions=>landmarks;
        public IReadOnlyList<Vector3> FirePositions=>fireAnchors;
        public int Groves {get;private set;}
        readonly List<Vector4> paintedTrees=new List<Vector4>();
        public IReadOnlyList<Vector4> PaintedTrees=>paintedTrees;
        public bool PlantSpaceFree(float x,float z,float radius)=>SpaceFree(x,z,radius);
        bool SpaceFree(float x,float z,float radius) {
            foreach(var s in scenerySpaces)if(new Vector2(s.x-x,s.z-z).magnitude<radius+s.w+.3f)return false;
            return true;
        }
        static bool FlightClear(Scenario c,float x,float z,float radius) {
            // Tall props stay outside the swept flight corridor. Low plants can sit underneath it.
            float clearance=radius*1.415f+.75f;
            foreach(var lane in c.Lanes) {
                var from=lane.Spawn;
                foreach(var to in lane.FlightRoute) {
                    float dx=to.X-from.X,dz=to.Y-from.Y;
                    float t=Mathf.Clamp01(((x-from.X)*dx+(z-from.Y)*dz)/Mathf.Max(.0001f,dx*dx+dz*dz));
                    if(new Vector2(x-from.X-dx*t,z-from.Y-dz*t).magnitude<clearance)return false;
                    from=to;
                }
            }
            return true;
        }
        static void Beam(Batch b,Vector3 a,Vector3 end,float width) {
            var d=(end-a).normalized;var s=Vector3.Cross(d,Vector3.forward).normalized*width;var t=Vector3.Cross(d,s).normalized*width;
            b.Quad(a-s-t,a+s-t,end+s-t,end-s-t);b.Quad(a+s+t,a-s+t,end-s+t,end+s+t);
            b.Quad(a-s+t,a-s-t,end-s-t,end-s+t);b.Quad(a+s-t,a+s+t,end+s+t,end+s-t);
        }
        // Low, rounded shelves with irregular outlines, rather than rows of pointed cones.
        static void Mound(Batch b,float x,float z,float y,float radius,float height,int seed) {
            const int sides=9;var rings=new Vector3[3,sides];
            for(int r=0;r<3;r++)for(int i=0;i<sides;i++) {
                float a=i*Mathf.PI*2/sides,spread=(r==0?1:r==1?.81f:.37f)*( .89f+.10f*Mathf.Sin(i*3+seed));
                rings[r,i]=new Vector3(x+Mathf.Cos(a)*radius*spread,y+height*(r==0?0:r==1?.6f:1),z+Mathf.Sin(a)*radius*spread);
            }
            for(int i=0;i<sides;i++){int n=(i+1)%sides;for(int r=0;r<2;r++)b.Quad(rings[r,i],rings[r+1,i],rings[r+1,n],rings[r,n]);b.Quad(rings[2,i],new Vector3(x,y+height*1.06f,z),new Vector3(x,y+height*1.06f,z),rings[2,n]);}
        }
        void BuildComposition(Batch[] b,Scenario c,bool ice) {
            float y=ice?.72f:.6f;
            // Ironfold has narrower half-cell shelves; fit whole compositions to those shelves.
            float landmarkScale=ice?1:.72f,groveScale=ice?1:.65f;
            int[] Starts(){var counts=new int[b.Length];for(int i=0;i<b.Length;i++)counts[i]=b[i].V.Count;return counts;}
            void Fit(int[] starts,float x,float z,float scale) {
                for(int i=0;i<b.Length;i++)for(int v=starts[i];v<b[i].V.Count;v++){var p=b[i].V[v];p.x=x+(p.x-x)*scale;p.z=z+(p.z-z)*scale;b[i].V[v]=p;}
            }
            // Select broad blocked shelves near important bends; the source mask remains untouched.
            var targets=ice?new[]{new Vector2(.48f,.38f),new Vector2(.22f,.7f),new Vector2(.79f,.58f)}:new[]{new Vector2(.49f,.18f),new Vector2(.27f,.52f),new Vector2(.70f,.63f)};
            foreach(var target in targets) {
                Vector3 best=default;float score=float.MaxValue;
                for(float z=3;z<c.Height-3;z+=c.LayoutCellSize)for(float x=3;x<c.Width-3;x+=c.LayoutCellSize) {
                    if(!ScenicFootprint(c,x,z,2.25f*landmarkScale)||!SpaceFree(x,z,3*landmarkScale)||!FlightClear(c,x,z,2.25f*landmarkScale))continue;
                    float distance=(new Vector2(x/c.Width,z/c.Height)-target).sqrMagnitude;
                    if(distance<score){score=distance;best=new Vector3(x,y,z);}
                }
                if(score==float.MaxValue)continue;
                landmarks.Add(best);scenerySpaces.Add(new Vector4(best.x,y,best.z,2.5f*landmarkScale));
                var starts=Starts();
                float x0=best.x,z0=best.z;
                Mound(b[24],x0,z0,y+.015f,2.12f,.16f,landmarks.Count);
                if(ice)WayshrineLandmark(b,x0,z0,y,landmarks.Count);
                else ForgeLandmark(b,x0,z0,y,landmarks.Count);
                // One warm hearth per landmark becomes a spatial sound/light anchor too.
                float hx=x0+1.55f,hz=z0+1.25f;
                Ring(b[16],hx,hz,y,.45f,.24f,.32f);b[17].Peak(hx,hz,.20f,y+.45f,.65f);b[18].Peak(hx,hz,.10f,y+.46f,.4f);
                fireAnchors.Add(new Vector3(x0+(hx-x0)*landmarkScale,y+.45f,z0+(hz-z0)*landmarkScale));Braziers++;
                Fit(starts,x0,z0,landmarkScale);
            }
            float spacing=ice?4:2.25f;
            for(int iz=0;iz<Mathf.CeilToInt((c.Height-4)/spacing);iz++)for(int ix=0;ix<Mathf.CeilToInt((c.Width-4)/spacing);ix++) {
                int seed=ix*37+iz*71;float x=2+ix*spacing+Mathf.Sin(seed)*.8f,z=2+iz*spacing+Mathf.Cos(seed)*.8f;
                if(!ScenicFootprint(c,x,z,1.8f*groveScale)||!SpaceFree(x,z,1.8f*groveScale))continue;
                bool border=!ScenicFootprint(c,x,z,3.6f);if(!border&&seed%3!=0)continue;
                scenerySpaces.Add(new Vector4(x,y,z,1.8f*groveScale));Groves++;
                var starts=Starts();
                if(!FlightClear(c,x,z,1.8f*groveScale)) {
                    // Groundcover and stones beneath air routes, with no compressed tree silhouettes.
                    Mound(b[24],x,z,y+.01f,1.43f,.06f,seed);
                    Mound(b[25],x+.55f,z-.35f,y+.02f,.5f,.28f,seed);
                    if(ice)Mound(b[28],x+.55f,z-.35f,y+.22f,.35f,.08f,seed);
                    for(int shrub=0;shrub<3;shrub++) {
                        float a=seed+shrub*2.1f,sx=x+Mathf.Cos(a)*.7f,sz=z+Mathf.Sin(a)*.7f;
                        Mound(b[27],sx,sz,y+.04f,.4f,.18f,seed+shrub);
                        for(int leaf=0;leaf<5;leaf++) {
                            float angle=a+leaf*1.25f;
                            var root=new Vector3(sx,y+.08f,sz);
                            Leaf(b[27],root,root+new Vector3(Mathf.Cos(angle)*.42f,.3f,Mathf.Sin(angle)*.42f),.11f);
                        }
                    }
                    Fit(starts,x,z,groveScale);continue;
                }
                Mound(b[24],x,z,y+.01f,1.43f,.35f,seed);
                Mound(b[25],x+.5f,z-.35f,y+.05f,.62f,.78f,seed);
                if(ice)Mound(b[28],x+.5f,z-.35f,y+.62f,.46f,.26f,seed);
                // Asymmetric branching, layered crowns, two heights per group.
                paintedTrees.Add(new Vector4(x,y+(ice?3.5f:3.2f),z,1.6f*groveScale));
                for(int tree=0;ice&&tree<2;tree++) {
                    float tx=x-.35f+tree*.73f,tz=z+.25f+tree*.3f,height=(tree==0?3.1f:2.05f)+(seed%4)*.16f;
                    Beam(b[26],new Vector3(tx,y,tz),new Vector3(tx+.13f,y+height,tz),.08f);
                    for(int tier=0;tier<3;tier++) {
                        float h=y+.85f+tier*height*.23f,rad=(.78f-tier*.16f)*(tree==0?1:.72f);
                        for(int branch=0;branch<3;branch++) {
                            float a=seed+branch*2.1f+tier*.7f;var end=new Vector3(tx+Mathf.Cos(a)*rad*.35f,h+.12f,tz+Mathf.Sin(a)*rad*.35f);
                            Beam(b[26],new Vector3(tx,h-.15f,tz),end,.034f);
                            Mound(b[27],end.x,end.z,h,rad*.47f,ice?.20f:.22f,seed+tier+branch);
                            if(ice)Mound(b[28],end.x,end.z,h+.16f,rad*.39f,.12f,seed+tier+branch);
                        }
                    }
                    b[27].Peak(tx+.13f,tz,.28f*(tree==0?1:.8f),y+height-.45f,.62f);
                    if(ice)b[28].Peak(tx+.13f,tz,.21f*(tree==0?1:.8f),y+height-.24f,.43f);
                }
                for(int i=0;i<5;i++) {
                    float a=seed+i*2.4f;var root=new Vector3(x+Mathf.Cos(a),y+.03f,z+Mathf.Sin(a));
                    Leaf(b[27],root,root+new Vector3(Mathf.Cos(a)*.27f,.45f,Mathf.Sin(a)*.27f),.08f);
                }
                Fit(starts,x,z,groveScale);
            }
        }
    }
}
