using System.Collections.Generic;
using UnityEngine;
namespace FrostMaze
{
    // Exterior terrain is cosmetic and never enters navigation, picking or the north-up minimap.
    public sealed partial class WorldBackdrop : MonoBehaviour
    {
        Texture2D surface,ridgeTexture;
        bool bakedSurface;
        readonly List<Mesh> meshes=new List<Mesh>();
        sealed class Batch {
            public readonly List<Vector3> V=new List<Vector3>();public readonly List<int> T=new List<int>();
            public void Triangle(Vector3 a,Vector3 b,Vector3 c){int n=V.Count;V.Add(a);V.Add(b);V.Add(c);T.Add(n);T.Add(n+1);T.Add(n+2);}
            public void Quad(Vector3 a,Vector3 b,Vector3 c,Vector3 d){Triangle(a,b,c);Triangle(a,c,d);}
            public void Box(float x,float z,float w,float d,float y,float top) {
                var a=new Vector3(x,y,z);var b=new Vector3(x+w,y,z);var c=new Vector3(x+w,y,z+d);var e=new Vector3(x,y,z+d);var up=Vector3.up*(top-y);
                Quad(a+up,e+up,c+up,b+up);Quad(a,a+up,b+up,b);Quad(b,b+up,c+up,c);Quad(c,c+up,e+up,e);Quad(e,e+up,a+up,a);
            }
            public void Blob(Vector3 centre,Vector3 size,float seed) {
                const int sides=12,rings=6;var points=new Vector3[rings+1,sides];
                for(int j=0;j<=rings;j++)for(int i=0;i<sides;i++){
                    float latitude=j*Mathf.PI/rings,angle=i*2*Mathf.PI/sides;
                    float r=Mathf.Sin(latitude)*(1+.06f*Mathf.Sin(i*3+seed)+.035f*Mathf.Cos(i*5+j+seed));
                    points[j,i]=centre+Vector3.Scale(new Vector3(Mathf.Cos(angle)*r,Mathf.Cos(latitude),Mathf.Sin(angle)*r),size);
                }
                for(int j=0;j<rings;j++)for(int i=0;i<sides;i++){int n=(i+1)%sides;Quad(points[j,i],points[j,n],points[j+1,n],points[j+1,i]);}
            }
            public void Bough(float x,float z,float y,float radius,float height,int seed) {
                const int sides=12;var rings=new Vector3[4,sides];
                for(int r=0;r<4;r++)for(int i=0;i<sides;i++){
                    float angle=i*Mathf.PI*2/sides,wide=radius*(r==0?.91f:r==1?1:r==2?.56f:.08f)*(1+.065f*Mathf.Sin(i*3+seed));
                    rings[r,i]=new Vector3(x+Mathf.Cos(angle)*wide,y+height*(r==0?0:r==1?.16f:r==2?.62f:1),z+Mathf.Sin(angle)*wide);
                }
                for(int r=0;r<3;r++)for(int i=0;i<sides;i++){int n=(i+1)%sides;Quad(rings[r,i],rings[r+1,i],rings[r+1,n],rings[r,n]);}
                for(int i=0;i<sides;i++)Triangle(rings[3,i],new Vector3(x,y+height*1.025f,z),rings[3,(i+1)%sides]);
            }
            public void Peak(float x,float z,float radius,float y,float height){for(int i=0;i<7;i++){float a=i*Mathf.PI*2/7,b=(i+1)*Mathf.PI*2/7;Triangle(new Vector3(x,y+height,z),new Vector3(x+Mathf.Cos(b)*radius,y,z+Mathf.Sin(b)*radius),new Vector3(x+Mathf.Cos(a)*radius,y,z+Mathf.Sin(a)*radius));}}
        }
        float mirrorWidth;
        public void Build(Prototype game)
        {
            float w=game.World.Config.Width,h=game.World.Config.Height;bool ice=game.World.Config.Theme!="iron";PrepareLayout(w,h,ice);
            var ground=new Batch();var rock=new Batch();var leaves=new Batch();var snow=new Batch();var trunks=new Batch();var garden=new Batch();
            float Distance(float x,float z)=>Mathf.Max(Mathf.Max(-x,x-w),Mathf.Max(-z,z-h));
            float Height(float x,float z)=>ExteriorHeight(x,z);
            // Four strips extend well beyond all permitted camera ground intersections.
            ground.Quad(new Vector3(-512,-.19f,-512),new Vector3(-512,-.19f,h+512),new Vector3(0,-.19f,h+512),new Vector3(0,-.19f,-512));
            ground.Quad(new Vector3(w,-.19f,-512),new Vector3(w,-.19f,h+512),new Vector3(w+512,-.19f,h+512),new Vector3(w+512,-.19f,-512));
            ground.Quad(new Vector3(0,-.19f,-512),new Vector3(0,-.19f,0),new Vector3(w,-.19f,0),new Vector3(w,-.19f,-512));
            ground.Quad(new Vector3(0,-.19f,h),new Vector3(0,-.19f,h+512),new Vector3(w,-.19f,h+512),new Vector3(w,-.19f,h));
            for(float x=-128;x<w+128;x+=4)for(float z=-128;z<h+128;z+=4){
                if(x>=0&&x<w&&z>=0&&z<h)continue;
                ground.Quad(new Vector3(x,Height(x,z),z),new Vector3(x,Height(x,z+4),z+4),new Vector3(x+4,Height(x+4,z+4),z+4),new Vector3(x+4,Height(x+4,z),z));
            }
            for(float x=-88;x<w*.5f;x+=7)for(float z=-88;z<h+88;z+=7){
                float cx=x+1+Mathf.PerlinNoise(x*.8f+17,z*.6f+41)*4,cz=z+1+Mathf.PerlinNoise(x*.6f+72,z*.7f+33)*4;
                float distance=Distance(cx,cz),n=Mathf.PerlinNoise(cx*.37f+53,cz*.29f+87);
                if(distance<5||distance>80||n<.40f)continue;
                float radius=1.6f+n*1.4f;
                if(!SceneryFits(cx,cz,radius+1.2f))continue;
                float y=GroundY(cx,cz);
                // Broad shoulders, blunt stone and clustered crowns replace isolated cone peaks.
                if(n<.52f)rock.Blob(new Vector3(cx,y+.5f,cz),new Vector3(radius,.7f+n,radius*.8f),n*31);
                else Tree(trunks,leaves,snow,cx,cz,y,1+n*.65f,ice);
            }
            // A few deliberately placed low garden clusters soften the settlement's yards.
            foreach(var p in new[]{new Vector2(6,-10),new Vector2(17,-4.3f),new Vector2(27.5f,-12),new Vector2(6,-19),new Vector2(26,-27)}) {
                if(!SceneryFits(p.x,p.y,1.25f))continue;
                float y=GroundY(p.x,p.y);
                rock.Blob(new Vector3(p.x,y+.025f,p.y),new Vector3(1.10f,.07f,.79f),p.x);
                for(int lobe=0;lobe<3;lobe++) {
                    float angle=lobe*2.1f;var centre=new Vector3(p.x+Mathf.Cos(angle)*.35f,y+.33f+lobe*.10f,p.y+Mathf.Sin(angle)*.28f);
                    garden.Blob(centre,new Vector3(.56f,.40f,.48f),p.x+lobe*7);
                    snow.Blob(centre+new Vector3(-.07f,.24f,-.05f),new Vector3(.39f,.22f,.34f),p.y+lobe*7);
                }
            }
            BuildLandmarks(game,ice,w,h);
            BuildRefuge(game,ice,w);
            var material=game.MakeMaterial(Color.white);var baked=WorldSurfaceSet.Find(game.World.Config);bakedSurface=baked!=null;surface=bakedSurface?baked.Exterior:Paint(ice);material.mainTexture=surface;Save("Outer terrain",ground,material);
            var ridgeMaterial=game.MakeMaterial(Color.white);ridgeTexture=PaintRidges(ice);ridgeMaterial.mainTexture=ridgeTexture;
            Save("Distant ridges",rock,ridgeMaterial);
            Save("Sheltered tree trunks",trunks,game.MakeMaterial(new Color(.25f,.20f,.14f)));
            Save("Sheltered canopy",leaves,game.MakeMaterial(ice?new Color(.12f,.29f,.27f):new Color(.19f,.35f,.18f)));
            Save("Sheltered crown",snow,game.MakeMaterial(ice?new Color(.70f,.81f,.80f):new Color(.35f,.45f,.22f)));
            Save("Sheltered garden",garden,game.MakeMaterial(ice?new Color(.17f,.34f,.30f):new Color(.25f,.39f,.19f)));
            RememberFoliage(meshes[meshes.Count-1]);
        }
        void Save(string name,Batch b,Material material){MirroredGeometry.Apply(b.V,b.T,mirrorWidth);var mesh=new Mesh{name=name,indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};mesh.SetVertices(b.V);mesh.SetTriangles(b.T,0);var uv=new List<Vector2>();foreach(var vertex in b.V) {
                float mx=Mathf.Min(vertex.x,mirrorWidth-vertex.x);
                bool roof=name.Contains("roof")||name.Contains(" refuge ")&&name.EndsWith("3");
                uv.Add(name.Contains(" refuge ")&&name.EndsWith("6")?new Vector2((mx-(mirrorWidth*.5f-6.7f))/1.74f+.5f,vertex.y/2.6f):roof?new Vector2(vertex.z/5.5f,mx/6):(name.Contains(" refuge ")||name=="Village foundations"||name=="Foundry brickwork")?new Vector2((mx+vertex.z)/4,vertex.y/3):name=="Distant ridges"?new Vector2((mx+vertex.z)/8,vertex.y/8):name=="Outer terrain"?new Vector2((mx+128)/(mirrorWidth*.5f+128),(vertex.z+128)/(exteriorHeight+256)):new Vector2(mx/64,vertex.z/64));
            };mesh.SetUVs(0,uv);mesh.RecalculateNormals();if(name=="Outer terrain"||name=="Distant ridges"||name.StartsWith("Sheltered"))SmoothNormals(mesh);mesh.RecalculateBounds();meshes.Add(mesh);var go=new GameObject(name);go.transform.SetParent(transform,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;var renderer=go.AddComponent<MeshRenderer>();renderer.sharedMaterial=material;renderer.shadowCastingMode=name.StartsWith("Sheltered")||name=="Distant ridges"?UnityEngine.Rendering.ShadowCastingMode.On:UnityEngine.Rendering.ShadowCastingMode.Off;}
        Texture2D Paint(bool ice){const int size=1024;var pixels=new Color[size*size];for(int y=0;y<size;y++)for(int x=0;x<size;x++){float wx=Mathf.Lerp(-128,mirrorWidth*.5f,x/(size-1f)),wz=Mathf.Lerp(-128,exteriorHeight+128,y/(size-1f));pixels[y*size+x]=ExteriorPigment(wx,wz,ice);}var texture=new Texture2D(size,size,TextureFormat.RGB24,true){name="Original sheltered terrain and worn trails",wrapMode=TextureWrapMode.Clamp,filterMode=FilterMode.Trilinear,anisoLevel=4};texture.SetPixels(pixels);texture.Apply(true,!paintingForBake);return texture;}
        bool paintingForBake;
        public Texture2D PaintExteriorForBake(FrostMaze.Simulation.Scenario config){paintingForBake=true;PrepareLayout(config.Width,config.Height,config.Theme!="iron");return Paint(config.Theme!="iron");}
        Texture2D PaintRidges(bool ice)
        {
            const int w=512,h=128;var pixels=new Color[w*h];
            for(int y=0;y<h;y++)for(int x=0;x<w;x++) {
                float u=x/(float)w,v=y/(float)(h-1),wash=Mathf.PerlinNoise(u*9+21,v*3+17);
                float stratum=.93f+.07f*Mathf.Sin(v*24+wash*3);
                pixels[y*w+x]=Color.Lerp(ice?new Color(.22f,.32f,.36f):new Color(.20f,.23f,.22f),ice?new Color(.36f,.44f,.47f):new Color(.35f,.34f,.28f),wash)*stratum;
            }
            var texture=new Texture2D(w,h,TextureFormat.RGB24,true){name="Original weathered mountain strata",wrapModeU=TextureWrapMode.Repeat,wrapModeV=TextureWrapMode.Clamp,filterMode=FilterMode.Trilinear,anisoLevel=4};
            texture.SetPixels(pixels);texture.Apply(true,true);return texture;
        }
        void OnDestroy(){if(refugeEmbers!=null)Destroy(refugeEmbers);if(refugeSurface!=null)Destroy(refugeSurface);if(ridgeTexture!=null)Destroy(ridgeTexture);if(smokeTexture!=null)Destroy(smokeTexture);if(surface!=null&&!bakedSurface)Destroy(surface);foreach(var mesh in meshes)if(mesh!=null)Destroy(mesh);}
    }
}
