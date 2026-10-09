using System.Collections.Generic;
using UnityEngine;
namespace FrostMaze
{
    public sealed partial class WorldBackdrop
    {
        readonly struct HousePlan {
            public readonly float X,Z,W,D,H;
            public HousePlan(float x,float z,float w,float d,float h){X=x;Z=z;W=w;D=d;H=h;}
        }
        readonly struct TrailPiece {
            public readonly Vector2 A,B;public readonly float Width;
            public TrailPiece(Vector2 a,Vector2 b,float width){A=a;B=b;Width=width;}
            public float Distance(Vector2 p){var d=B-A;return (p-A-d*Mathf.Clamp01(Vector2.Dot(p-A,d)/d.sqrMagnitude)).magnitude;}
        }
        readonly List<HousePlan> houses=new List<HousePlan>();
        readonly List<Rect> settlementBounds=new List<Rect>();
        readonly List<TrailPiece> trails=new List<TrailPiece>();
        float exteriorHeight;
        public IReadOnlyList<Rect> SettlementBounds=>settlementBounds;
        void PrepareLayout(float width,float height,bool ice) {
            mirrorWidth=width;exteriorHeight=height;houses.Clear();settlementBounds.Clear();trails.Clear();
            houses.Add(new HousePlan(10,-9,3.7f,3.4f,2.6f));
            houses.Add(new HousePlan(18,-14,4.5f,4,3.1f));
            houses.Add(new HousePlan(-8,18,3.4f,4.3f,3));
            // Envelopes include eaves, firewood, steps and pennants, not just wall centres.
            foreach(var h in houses)settlementBounds.Add(new Rect(h.X-1.1f,h.Z-.6f,h.W+2.3f,h.D+1.2f));
            settlementBounds.Add(new Rect(width*.5f-10.3f,-6.6f,8.2f,5.8f));
            if(!ice)settlementBounds.Add(new Rect(17.2f,-25.3f,4.3f,3.8f));
            void Curve(Vector2 a,Vector2 b,Vector2 c,Vector2 d,float wide) {
                var previous=a;const int steps=24;
                for(int i=1;i<=steps;i++){float t=i/(float)steps,u=1-t;var p=u*u*u*a+3*u*u*t*b+3*u*t*t*c+t*t*t*d;trails.Add(new TrailPiece(previous,p,wide));previous=p;}
            }
            float mid=width*.5f;
            Curve(new Vector2(mid,0),new Vector2(mid,-10),new Vector2(mid,-24),new Vector2(mid,-38),.95f);
            Curve(new Vector2(mid,-19),new Vector2(23,-23),new Vector2(9,-20),new Vector2(11.85f,-9.4f),.68f);
            Curve(new Vector2(19,-19),new Vector2(19,-18),new Vector2(20.25f,-16),new Vector2(20.25f,-14.4f),.53f);
            Curve(new Vector2(mid,-8),new Vector2(mid-1,-9),new Vector2(mid-5,-9),new Vector2(mid-6.7f,-6.4f),.6f);
        }
        static float DistanceTo(Rect r,float x,float z) {
            float dx=Mathf.Max(r.xMin-x,0,x-r.xMax),dz=Mathf.Max(r.yMin-z,0,z-r.yMax);return Mathf.Sqrt(dx*dx+dz*dz);
        }
        bool SceneryFits(float x,float z,float radius) {
            x=Mathf.Min(x,mirrorWidth-x);
            // Full envelope clearance also keeps roots/canopies out of the playfield.
            if(x+radius>0&&x-radius<mirrorWidth&&z+radius>0&&z-radius<exteriorHeight)return false;
            foreach(var r in settlementBounds)if(DistanceTo(r,x,z)<radius+.6f)return false;
            if(z<1&&z>-42)foreach(var path in trails)if(path.Distance(new Vector2(x,z))<radius+path.Width+.65f)return false;
            return true;
        }
        float ExteriorHeight(float x,float z) {
            x=Mathf.Min(x,mirrorWidth-x);
            float d=Mathf.Max(Mathf.Max(-x,x-mirrorWidth),Mathf.Max(-z,z-exteriorHeight));
            float y=Mathf.SmoothStep(0,1,Mathf.Clamp01((d-5)/35))*Mathf.PerlinNoise((x+321)*.021f,(z+157)*.021f)*3;
            // Foundations sit in sheltered, graded ground. Blend into hills beyond their yards.
            foreach(var r in settlementBounds)y*=Mathf.SmoothStep(0,1,Mathf.Clamp01(DistanceTo(r,x,z)/5));
            return -.16f+y;
        }
        public bool PlantFits(float x,float z,float radius)=>SceneryFits(x,z,radius);
        public float SurfaceY(float x,float z)=>GroundY(x,z);
        float GroundY(float x,float z) {
            const float step=4;float x0=Mathf.Floor(x/step)*step,z0=Mathf.Floor(z/step)*step,u=(x-x0)/step,v=(z-z0)/step;
            float a=ExteriorHeight(x0,z0),b=ExteriorHeight(x0,z0+step),c=ExteriorHeight(x0+step,z0+step),d=ExteriorHeight(x0+step,z0);
            return v>=u?a+(c-b)*u+(b-a)*v:a+(d-a)*u+(c-d)*v;
        }
        Color ExteriorPigment(float x,float z,bool ice) {
            var color=MapScenery.RaisedSurfaceColor(x,z,ice);
            float broad=Mathf.PerlinNoise(x*.13f+11,z*.13f+23);
            color=Color.Lerp(color,ice?new Color(.55f,.65f,.67f):new Color(.31f,.43f,.24f),ice?.12f:.14f);
            if(z>1||z<-42)return color;
            float distance=100;
            foreach(var path in trails)distance=Mathf.Min(distance,path.Distance(new Vector2(x,z))-path.Width);
            // Worn pigment is painted into the terrain itself: no floating polygon or hard edge.
            float edge=(Mathf.PerlinNoise(x*1.7f+31,z*1.7f+8)-.5f)*.30f;
            float wear=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(-.15f,.85f,distance+edge));
            var dirt=ice?new Color(.38f,.45f,.44f):new Color(.38f,.32f,.23f);
            return Color.Lerp(color,dirt*(.9f+broad*.18f),wear*.80f);
        }
    }
}
