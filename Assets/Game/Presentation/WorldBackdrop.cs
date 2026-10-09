using System.Collections.Generic;
using UnityEngine;
namespace FrostMaze
{
    // Exterior terrain is cosmetic and never enters navigation, picking or the north-up minimap.
    public sealed class WorldBackdrop : MonoBehaviour
    {
        Texture2D surface;
        readonly List<Mesh> meshes=new List<Mesh>();
        sealed class Batch {
            public readonly List<Vector3> V=new List<Vector3>();public readonly List<int> T=new List<int>();
            public void Triangle(Vector3 a,Vector3 b,Vector3 c){int n=V.Count;V.Add(a);V.Add(b);V.Add(c);T.Add(n);T.Add(n+1);T.Add(n+2);}
            public void Quad(Vector3 a,Vector3 b,Vector3 c,Vector3 d){Triangle(a,b,c);Triangle(a,c,d);}
            public void Peak(float x,float z,float radius,float y,float height){for(int i=0;i<7;i++){float a=i*Mathf.PI*2/7,b=(i+1)*Mathf.PI*2/7;Triangle(new Vector3(x,y+height,z),new Vector3(x+Mathf.Cos(b)*radius,y,z+Mathf.Sin(b)*radius),new Vector3(x+Mathf.Cos(a)*radius,y,z+Mathf.Sin(a)*radius));}}
        }
        public void Build(Prototype game)
        {
            float w=game.World.Config.Width,h=game.World.Config.Height;bool ice=game.World.Config.Theme!="iron";
            var ground=new Batch();var rock=new Batch();var leaves=new Batch();var snow=new Batch();
            float Distance(float x,float z)=>Mathf.Max(Mathf.Max(-x,x-w),Mathf.Max(-z,z-h));
            float Height(float x,float z)=>-.16f+Mathf.SmoothStep(0,1,Mathf.Clamp01((Distance(x,z)-5)/35))*Mathf.PerlinNoise((x+321)*.021f,(z+157)*.021f)*5;
            // Four strips extend well beyond all permitted camera ground intersections.
            ground.Quad(new Vector3(-512,-.19f,-512),new Vector3(-512,-.19f,h+512),new Vector3(0,-.19f,h+512),new Vector3(0,-.19f,-512));
            ground.Quad(new Vector3(w,-.19f,-512),new Vector3(w,-.19f,h+512),new Vector3(w+512,-.19f,h+512),new Vector3(w+512,-.19f,-512));
            ground.Quad(new Vector3(0,-.19f,-512),new Vector3(0,-.19f,0),new Vector3(w,-.19f,0),new Vector3(w,-.19f,-512));
            ground.Quad(new Vector3(0,-.19f,h),new Vector3(0,-.19f,h+512),new Vector3(w,-.19f,h+512),new Vector3(w,-.19f,h));
            for(float x=-128;x<w+128;x+=8)for(float z=-128;z<h+128;z+=8){
                if(x>=0&&x<w&&z>=0&&z<h)continue;
                ground.Quad(new Vector3(x,Height(x,z),z),new Vector3(x,Height(x,z+8),z+8),new Vector3(x+8,Height(x+8,z+8),z+8),new Vector3(x+8,Height(x+8,z),z));
                float cx=x+3+Mathf.PerlinNoise(x*.8f+17,z*.6f+41)*2,cz=z+3+Mathf.PerlinNoise(x*.6f+72,z*.7f+33)*2,d=Distance(cx,cz),n=Mathf.PerlinNoise(cx*.37f+53,cz*.29f+87);
                if(d<12||d>85||n<.43f)continue;
                float y=Height(cx,cz),height=2+n*6;
                rock.Peak(cx,cz,2+n*2,y,height*(ice?.55f:1));
                if(ice){for(int j=0;j<3;j++){float px=cx+j*1.7f-2,pz=cz-j*1.6f,py=Height(px,pz),size=.9f+n*.7f;for(int tier=0;tier<3;tier++){float radius=(1.25f-tier*.3f)*size,bottom=py+.35f+tier*1.05f*size,tip=(2.2f-tier*.35f)*size;leaves.Peak(px,pz,radius,bottom,tip);snow.Peak(px,pz,radius*.77f,bottom+tip*.28f,tip*.74f);}}}
                else if(n>.57f){snow.Peak(cx,cz,1.8f,y+height*.65f,height*.35f);leaves.Peak(cx+2,cz-2,.7f,y,2.5f);}
            }
            var material=game.MakeMaterial(Color.white);surface=Paint(ice);material.mainTexture=surface;Save("Outer terrain",ground,material);
            Save("Distant ridges",rock,game.MakeMaterial(ice?new Color(.24f,.36f,.4f):new Color(.20f,.21f,.24f)));
            Save(ice?"Frost pines":"Copper outcrops",leaves,game.MakeMaterial(ice?new Color(.12f,.27f,.28f):new Color(.52f,.31f,.12f)));
            Save(ice?"Snow crowns":"Foundry peaks",snow,game.MakeMaterial(ice?new Color(.69f,.81f,.81f):new Color(.36f,.31f,.29f)));
        }
        void Save(string name,Batch b,Material material){var mesh=new Mesh{name=name,indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};mesh.SetVertices(b.V);mesh.SetTriangles(b.T,0);var uv=new List<Vector2>();foreach(var vertex in b.V)uv.Add(new Vector2(vertex.x/64,vertex.z/64));mesh.SetUVs(0,uv);mesh.RecalculateNormals();mesh.RecalculateBounds();meshes.Add(mesh);var go=new GameObject(name);go.transform.SetParent(transform,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;var renderer=go.AddComponent<MeshRenderer>();renderer.sharedMaterial=material;renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;}
        Texture2D Paint(bool ice){const int size=512;var pixels=new Color[size*size];for(int y=0;y<size;y++)for(int x=0;x<size;x++){float wx=x*64f/size,wz=y*64f/size;pixels[y*size+x]=MapScenery.RaisedSurfaceColor(wx,wz,ice);}var texture=new Texture2D(size,size,TextureFormat.RGB24,true){name="Original exterior terrain wash",wrapMode=TextureWrapMode.Repeat,filterMode=FilterMode.Trilinear,anisoLevel=4};texture.SetPixels(pixels);texture.Apply(true,true);return texture;}
        void OnDestroy(){if(surface!=null)Destroy(surface);foreach(var mesh in meshes)if(mesh!=null)Destroy(mesh);}
    }
}
