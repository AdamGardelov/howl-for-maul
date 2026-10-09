using System.Collections.Generic;
using FrostMaze.Simulation;
using UnityEngine;
using UnityEngine.Rendering;

namespace FrostMaze
{
    // Presentation only. The complete windy silhouette must fit its reserved scenic footprint.
    // Cards are bent meshes in world space, not camera-facing billboards or navigable objects.
    public sealed partial class LivingWorld : MonoBehaviour
    {
        sealed class Geometry
        {
            public readonly List<Vector3> V=new List<Vector3>(),N=new List<Vector3>();
            public readonly List<Vector2> UV=new List<Vector2>();
            public readonly List<Color> C=new List<Color>();
            public readonly List<int> T=new List<int>();
            public void Quad(Vector3 a,Vector3 b,Vector3 c,Vector3 d,Color color,Vector3 normal=default)
            {
                int n=V.Count;V.AddRange(new[]{a,b,c,d});
                if(normal==default)normal=Vector3.Cross(b-a,c-a).normalized;
                for(int i=0;i<4;i++){N.Add(normal);C.Add(color);}
                UV.AddRange(new[]{new Vector2(0,0),new Vector2(0,1),new Vector2(1,1),new Vector2(1,0)});
                T.AddRange(new[]{n,n+1,n+2,n,n+2,n+3});
            }
            public void Mirror(float width)
            {
                int count=V.Count,triangles=T.Count;
                for(int i=0;i<count;i++){var p=V[i];V.Add(new Vector3(width-p.x,p.y,p.z));var n=N[i];N.Add(new Vector3(-n.x,n.y,n.z));UV.Add(UV[i]);C.Add(C[i]);}
                for(int i=0;i<triangles;i+=3){T.Add(T[i]+count);T.Add(T[i+2]+count);T.Add(T[i+1]+count);}
            }
        }
        readonly Geometry canopy=new Geometry(),blossom=new Geometry(),grass=new Geometry(),bark=new Geometry(),stone=new Geometry(),brass=new Geometry(),rocks=new Geometry();
        readonly List<Mesh> ownedMeshes=new List<Mesh>();
        readonly List<Material> ownedMaterials=new List<Material>();
        readonly List<Vector4> sites=new List<Vector4>();
        Scenario config;bool ice;float width;
        public int Trees {get;private set;}
        public int Gardens {get;private set;}
        public int Cards {get;private set;}
        public IReadOnlyList<Vector4> Sites=>sites;
        public bool Exterior {get;private set;}
        readonly List<Vector3> hearths=new List<Vector3>();
        public IReadOnlyList<Vector3> HearthPositions=>hearths;
        public const float WindEnvelope=.085f;

        public static LivingWorld Create(Prototype game,Transform parent,bool exterior)
        {
            var root=new GameObject(exterior?"Living refuge landscape":"Living lane gardens");
            root.transform.SetParent(parent,false);root.layer=exterior?0:30;
            var world=root.AddComponent<LivingWorld>();world.config=game.World.Config;world.ice=world.config.Theme!="iron";
            world.width=world.config.Width;world.Exterior=exterior;
            if(exterior)world.BuildOutside(parent.GetComponent<WorldBackdrop>());else world.BuildInside(parent.GetComponent<MapScenery>());
            world.Finish();return world;
        }
        static float Noise(float x,float z)=>Mathf.PerlinNoise(x*.73f+113,z*.67f+71);
        static Color Tint(float r,float g,float b,float wind=1)=>new Color(r,g,b,wind);
        bool Clear(float x,float z,float radius)
        {
            foreach(var p in sites)if(new Vector2(p.x-x,p.z-z).magnitude<p.w+radius+.16f)return false;
            return true;
        }
        void Site(float x,float y,float z,float r){sites.Add(new Vector4(x,y,z,r+WindEnvelope));}
        public static bool FitsMap(Scenario c,float x,float z,float radius)
        {
            float cell=c.LayoutCellSize;radius+=WindEnvelope+.035f;
            for(int zz=Mathf.FloorToInt((z-radius)/cell);zz<=Mathf.FloorToInt((z+radius)/cell);zz++)
            for(int xx=Mathf.FloorToInt((x-radius)/cell);xx<=Mathf.FloorToInt((x+radius)/cell);xx++){
                int row=c.LayoutRows.Length-1-zz;
                if(row<0||row>=c.LayoutRows.Length||xx<0||xx>=c.LayoutRows[row].Length)return false;
                char k=c.LayoutRows[row][xx];if(c.WalkableSymbols.IndexOf(k)>=0||k=='D'||k=='W'||k=='p')return false;
            }
            return true;
        }
        public static bool ClearsFlight(Scenario c,float x,float z,float radius)
        {
            foreach(var lane in c.Lanes){var a=lane.Spawn;foreach(var b in lane.FlightRoute){
                var d=new Vector2(b.X-a.X,b.Y-a.Y);float t=Mathf.Clamp01(Vector2.Dot(new Vector2(x-a.X,z-a.Y),d)/Mathf.Max(.001f,d.sqrMagnitude));
                if((new Vector2(x-a.X,z-a.Y)-d*t).magnitude<radius*1.415f+.8f)return false;a=b;
            }}return true;
        }
        void BuildInside(MapScenery scenery)
        {
            float y=ice?.74f:.62f;
            // The large existing grove cores become the shaded interior of painted leaf crowns.
            foreach(var p in scenery.PaintedTrees){
                if(p.x>width*.5f||!FitsMap(config,p.x,p.z,p.w)||!ClearsFlight(config,p.x,p.z,p.w))continue;
                Tree(new Vector3(p.x,y,p.z),p.w,p.y,Mathf.RoundToInt(p.x*13+p.z*7),false);Site(p.x,y,p.z,p.w);
            }
            for(float z=1.2f;z<config.Height-1;z+=1.35f)for(float x=1.2f;x<width*.5f-1;x+=1.35f){
                float n=Noise(x,z),px=x+(n-.5f)*.65f,pz=z+(Noise(z,x)-.5f)*.6f;
                float r=.36f+n*.17f;
                if(!FitsMap(config,px,pz,r)||!Clear(px,pz,r)||!scenery.PlantSpaceFree(px,pz,r))continue;
                bool edge=!FitsMap(config,px,pz,2.2f);
                if(Mathf.PerlinNoise(px*.22f+5,pz*.22f+17)<(edge?.32f:.53f))continue;
                if(n>.65f&&FitsMap(config,px,pz,1.02f)&&scenery.PlantSpaceFree(px,pz,1.14f)&&Clear(px,pz,1.14f)&&ClearsFlight(config,px,pz,1.02f)){
                    Tree(new Vector3(px,y,pz),.98f,ice?3.1f:2.9f,(int)(px*13+pz*3),false);Site(px,y,pz,.98f);
                } else {Garden(new Vector3(px,y,pz),r,(int)(px*11+pz*17));Site(px,y,pz,r);}
            }
        }
        void BuildOutside(WorldBackdrop backdrop)
        {
            BuildSanctuary(backdrop);
            foreach(var p in backdrop.PaintedTrees){
                if(p.x>width*.5f||!backdrop.PlantFits(p.x,p.z,p.w+WindEnvelope)||!Clear(p.x,p.z,p.w))continue;
                Tree(new Vector3(p.x,backdrop.SurfaceY(p.x,p.z),p.z),p.w,3.5f+p.w*.65f,(int)(p.x*7+p.z*13),p.z<0&&Mathf.Abs(p.x-width*.5f)<24);
                Site(p.x,p.y,p.z,p.w);
            }
            // Hand-composed Last Stand gardens, safely south of all gameplay and house envelopes.
            foreach(var p in new[]{new Vector3(width*.5f-15,-1.8f,1.45f),new Vector3(9,-2.7f,1.5f),new Vector3(4,-13,2.2f),new Vector3(width*.5f-3.5f,-12,1.8f)}){
                float x=p.x,z=p.y,r=p.z;
                if(!backdrop.PlantFits(x,z,r+WindEnvelope)||!Clear(x,z,r))continue;
                Tree(new Vector3(x,backdrop.SurfaceY(x,z),z),r,ice?5.5f:4.9f,(int)(x*19),!ice);Site(x,0,z,r);
            }
            // Low gardens follow the paths and foundations, rather than a regular prop grid.
            for(float z=-35;z<-1;z+=1.25f)for(float x=2;x<width*.5f-1;x+=1.25f){
                float n=Noise(x,z),px=x+(n-.5f)*.7f,pz=z+(Noise(z,x)-.5f)*.7f;
                float r=.4f+n*.23f;
                if(Mathf.PerlinNoise(px*.21f+19,pz*.21f+3)<.43f||!backdrop.PlantFits(px,pz,r+WindEnvelope)||!Clear(px,pz,r))continue;
                Garden(new Vector3(px,backdrop.SurfaceY(px,pz)+.035f,pz),r,(int)(px*11-pz*17));Site(px,0,pz,r);
            }
        }
        void Tree(Vector3 root,float radius,float height,int seed,bool flowering)
        {
            Trees++;var trunk=Tint(.47f,.36f,.23f,0);
            var bend=root+new Vector3(radius*.12f,height*.59f,radius*.06f);
            Tube(bark,root,bend,radius*.12f,radius*.06f,trunk,9);
            for(int i=0;i<4;i++){
                float angle=i*2.4f+seed;var end=root+new Vector3(Mathf.Cos(angle)*radius*.50f,height*(.61f+i*.055f),Mathf.Sin(angle)*radius*.50f);
                Tube(bark,Vector3.Lerp(root,bend,.5f),end,radius*.065f,.024f,trunk,6);
                Tube(bark,root+Vector3.up*.35f,root+new Vector3(Mathf.Cos(angle)*radius*.42f,.025f,Mathf.Sin(angle)*radius*.42f),radius*.075f,.01f,trunk,6);
            }
            var target=flowering?blossom:canopy;
            int layers=ice?4:3;
            for(int layer=0;layer<layers;layer++){
                int fans=layer==layers-1?3:5;
                float spread=radius*(ice?.51f:.48f)*(1-layer/(float)layers*.63f);
                for(int fan=0;fan<fans;fan++){
                    float a=seed+fan*Mathf.PI*2/fans+layer*.8f;
                    var center=root+new Vector3(Mathf.Cos(a)*spread,height*(ice?.28f+layer*.18f:.55f+layer*.15f),Mathf.Sin(a)*spread);
                    float size=radius*(ice?.66f:.70f)*(1-layer/(float)layers*(ice?.45f:.18f));
                    var tint=ice?Tint(.88f,1.07f,1.09f):Tint(.76f+layer*.13f,.92f+layer*.08f,.78f+layer*.07f);
                    if(flowering)tint=Tint(1.06f+layer*.035f,.82f+layer*.045f,.87f+layer*.035f);
                    LeafCard(target,center,size,a,ice?-.38f:.20f,tint,root,radius);
                }
            }
            Garden(root,radius*.6f,seed);
        }
        void LeafCard(Geometry g,Vector3 centre,float size,float turn,float tilt,Color color,Vector3 root,float radius)
        {
            Cards++;var right=new Vector3(Mathf.Cos(turn),0,Mathf.Sin(turn));var forward=Vector3.Cross(right,Vector3.up);
            Vector3 P(float u,float v){var p=centre+right*(u*size)+forward*(v*size)+Vector3.up*(size*(.28f*(1-u*u-v*v)+v*tilt));
                p.x=Mathf.Clamp(p.x,root.x-radius,root.x+radius);p.z=Mathf.Clamp(p.z,root.z-radius,root.z+radius);return p;}
            for(int z=0;z<2;z++)for(int x=0;x<2;x++){
                float u=x-1,v=z-1;int start=g.V.Count;
                var n=(Vector3.up+right*(u+.5f)*.55f+forward*(v+.5f)*.55f).normalized;
                g.Quad(P(u,v),P(u,v+1),P(u+1,v+1),P(u+1,v),color,n);
                g.UV[start]=new Vector2(x*.5f,z*.5f);g.UV[start+1]=new Vector2(x*.5f,(z+1)*.5f);
                g.UV[start+2]=new Vector2((x+1)*.5f,(z+1)*.5f);g.UV[start+3]=new Vector2((x+1)*.5f,z*.5f);
            }
        }
        void Garden(Vector3 root,float radius,int seed)
        {
            Gardens++;float turn=seed*.79f;
            if(seed%7==0)RockShoulder(root,radius*.78f,seed);
            // Small bent blades, fern fronds and light flower heads create three height layers.
            for(int i=0;i<7;i++){
                float a=turn+i*2.4f,spread=radius*(.2f+(i%3)*.15f);
                var foot=root+new Vector3(Mathf.Cos(a)*spread,.015f,Mathf.Sin(a)*spread);
                var direction=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a));var side=Vector3.Cross(direction,Vector3.up);
                float h=radius*(.35f+(i%4)*.16f),wide=radius*.07f;
                var middle=foot+Vector3.up*h*.64f+direction*radius*.1f;var tip=foot+Vector3.up*h+direction*radius*.3f;
                var c=ice?Tint(.37f,.55f,.48f,.4f):Tint(.35f+(i%3)*.08f,.51f+(i%3)*.09f,.18f,.4f);
                grass.Quad(foot-side*wide,middle-side*wide*.7f,middle+side*wide*.7f,foot+side*wide,c,Vector3.up);
                c.a=1;c*=1.12f;c.a=1;grass.Quad(middle-side*wide*.7f,tip,tip,middle+side*wide*.7f,c,Vector3.up);
            }
            if(seed%4!=0){
                var c=ice?Tint(.68f,.85f,.82f):Tint(.85f,.98f,.75f);
                LeafCard(canopy,root+Vector3.up*radius*.50f,radius*.92f,turn,.5f,c,root,radius);
                LeafCard(canopy,root+Vector3.up*radius*.60f,radius*.82f,turn+1.5f,.5f,c,root,radius);
            }
            if(seed%5==0)for(int f=0;f<3;f++){
                float a=turn+f*2.1f;var p=root+new Vector3(Mathf.Cos(a)*radius*.42f,radius*.62f,Mathf.Sin(a)*radius*.42f);
                Tube(grass,p-Vector3.up*radius*.6f,p,.011f,.008f,Tint(.28f,.47f,.25f,.6f),4);
                for(int petal=0;petal<5;petal++){
                    float t=petal*Mathf.PI*2/5;var end=p+new Vector3(Mathf.Cos(t)*.075f,.025f,Mathf.Sin(t)*.075f);
                    var side=new Vector3(-Mathf.Sin(t),0,Mathf.Cos(t))*.032f;
                    grass.Quad(p,end-side,end+Vector3.up*.035f,end+side,ice?Tint(.71f,.84f,1):Tint(1,.87f,.60f),Vector3.up);
                }
            }
        }
        void RockShoulder(Vector3 root,float radius,int seed)
        {
            const int sides=9;var ring=new Vector3[3,sides];
            for(int level=0;level<3;level++)for(int i=0;i<sides;i++){
                float a=i*Mathf.PI*2/sides,variation=.88f+.09f*Mathf.Sin(seed+i*2.3f);
                float r=radius*variation*(level==0?1:level==1?.88f:.55f);
                ring[level,i]=root+new Vector3(Mathf.Cos(a)*r+level*.035f*radius,level*radius*.40f,Mathf.Sin(a)*r);
            }
            var color=ice?Tint(.39f,.49f,.50f,0):Tint(.43f,.48f,.39f,0);
            for(int i=0;i<sides;i++){
                int n=(i+1)%sides;
                for(int level=0;level<2;level++)rocks.Quad(ring[level,i],ring[level+1,i],ring[level+1,n],ring[level,n],color*(.85f+(i%3)*.07f));
                rocks.Quad(ring[2,i],root+Vector3.up*radius*.85f,root+Vector3.up*radius*.85f,ring[2,n],color*1.1f,Vector3.up);
            }
        }
        static void Tube(Geometry b,Vector3 a,Vector3 end,float r0,float r1,Color color,int sides=12)
        {
            var axis=(end-a).normalized;var right=Vector3.Cross(axis,Vector3.forward).normalized;
            if(right.sqrMagnitude<.01f)right=Vector3.right;var up=Vector3.Cross(axis,right).normalized;
            for(int i=0;i<sides;i++){
                float t=i*2*Mathf.PI/sides,n=(i+1)*2*Mathf.PI/sides;
                var va=right*Mathf.Cos(t)+up*Mathf.Sin(t);var vb=right*Mathf.Cos(n)+up*Mathf.Sin(n);
                b.Quad(a+va*r0,end+va*r1,end+vb*r1,a+vb*r0,color,(va+vb).normalized);
                b.Quad(end,end+vb*r1,end+va*r1,end,color,axis);
            }
        }
        void Finish()
        {
            Save("Painted crowns",canopy,Resources.Load<Texture2D>(ice?"World/HearthSpruce":"World/HearthLeaves"),true);
            Save("Hearthblossom crowns",blossom,Resources.Load<Texture2D>("World/HearthLeaves"),true,true);
            Save("Meadow undergrowth",grass,Texture2D.whiteTexture,true);
            Save("Living branches",bark,Texture2D.whiteTexture,false);
            Save("Weathered shoulders",rocks,Texture2D.whiteTexture,false);
            Save("Sanctuary stone",stone,Resources.Load<Texture2D>("World/HearthMasonry"),false);
            Save("Sanctuary bronze",brass,Texture2D.whiteTexture,false);
        }
        void Save(string name,Geometry b,Texture2D texture,bool windy,bool desaturate=false)
        {
            if(b.V.Count==0)return;b.Mirror(width);
            var mesh=new Mesh{name=name,indexFormat=IndexFormat.UInt32};mesh.SetVertices(b.V);mesh.SetNormals(b.N);mesh.SetUVs(0,b.UV);mesh.SetColors(b.C);mesh.SetTriangles(b.T,0);mesh.RecalculateBounds();
            var bounds=mesh.bounds;bounds.Expand(WindEnvelope*2);mesh.bounds=bounds;ownedMeshes.Add(mesh);
            var material=new Material(Resources.Load<Shader>("World/HearthFoliage")){name=name};material.SetTexture("_BaseMap",texture);
            material.SetFloat("_Wind",windy?.045f:0);material.SetFloat("_MapWidth",width);material.SetFloat("_Desaturate",desaturate?1:0);ownedMaterials.Add(material);
            var go=new GameObject(name);go.layer=gameObject.layer;go.transform.SetParent(transform,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;
            var renderer=go.AddComponent<MeshRenderer>();renderer.sharedMaterial=material;renderer.shadowCastingMode=ShadowCastingMode.TwoSided;
        }
        void OnDestroy(){foreach(var mesh in ownedMeshes)Destroy(mesh);foreach(var material in ownedMaterials)Destroy(material);}
    }
}
