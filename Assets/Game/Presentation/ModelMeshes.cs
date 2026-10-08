using System.Collections.Generic;
using UnityEngine;
namespace FrostMaze
{
    // Original flat-shaded profiles, shared by all actors in one map and disposed with it.
    public sealed class ModelMeshes
    {
        readonly Dictionary<string,Mesh> meshes=new Dictionary<string,Mesh>();
        public Mesh Column => Profile("Stone column",8,new[]{-1f,-.78f,.78f,1f},new[]{.4f,.5f,.5f,.4f});
        public Mesh Crystal => Profile("Cut ice",5,new[]{-.5f,-.3f,.22f,.5f},new[]{0f,.42f,.31f,0f});
        public Mesh Shell => Profile("Carved shell",8,new[]{-.5f,-.27f,.14f,.36f,.5f},new[]{0f,.4f,.5f,.33f,0f});
        public Mesh Robe => Profile("Warden mantle",7,new[]{-.5f,-.38f,.32f,.5f},new[]{.36f,.5f,.24f,.2f});
        Mesh Profile(string name,int sides,float[] heights,float[] radii)
        {
            if(meshes.TryGetValue(name,out var mesh))return mesh;
            var v=new List<Vector3>();var t=new List<int>();
            for(int ring=0;ring<heights.Length-1;ring++)for(int side=0;side<sides;side++) {
                float a=side*Mathf.PI*2/sides,b=(side+1)*Mathf.PI*2/sides;int n=v.Count;
                v.Add(new Vector3(Mathf.Cos(a)*radii[ring],heights[ring],Mathf.Sin(a)*radii[ring]));
                v.Add(new Vector3(Mathf.Cos(a)*radii[ring+1],heights[ring+1],Mathf.Sin(a)*radii[ring+1]));
                v.Add(new Vector3(Mathf.Cos(b)*radii[ring+1],heights[ring+1],Mathf.Sin(b)*radii[ring+1]));
                v.Add(new Vector3(Mathf.Cos(b)*radii[ring],heights[ring],Mathf.Sin(b)*radii[ring]));
                if(radii[ring+1]>0)t.AddRange(new[]{n,n+1,n+2});if(radii[ring]>0)t.AddRange(new[]{n,n+2,n+3});
            }
            for(int end=0;end<2;end++){int ring=end==0?0:heights.Length-1;if(radii[ring]==0)continue;
                for(int side=0;side<sides;side++){
                    float a=side*Mathf.PI*2/sides,b=(side+1)*Mathf.PI*2/sides;int n=v.Count;float y=heights[ring],radius=radii[ring];
                    v.Add(new Vector3(0,y,0));v.Add(new Vector3(Mathf.Cos(a)*radius,y,Mathf.Sin(a)*radius));v.Add(new Vector3(Mathf.Cos(b)*radius,y,Mathf.Sin(b)*radius));
                    t.AddRange(end==0?new[]{n,n+1,n+2}:new[]{n,n+2,n+1});
                }
            }
            return Save(name,v,t);
        }
        public Mesh Wing(int side)
        {
            string name="Swept wing "+side;if(meshes.TryGetValue(name,out var mesh))return mesh;
            var outline=new[]{new Vector3(0,0,.16f),new Vector3(.42f,0,.28f),new Vector3(1.05f,0,-.32f),new Vector3(.42f,0,-.17f),new Vector3(.1f,0,-.38f)};
            var v=new List<Vector3>();var t=new List<int>();
            for(int face=0;face<2;face++)for(int i=1;i<outline.Length-1;i++){
                int n=v.Count;foreach(int j in new[]{0,i,i+1}){var p=outline[j];v.Add(new Vector3(p.x*side,face==0?.045f:-.045f,p.z));}
                t.AddRange((face==0)==(side==1)?new[]{n,n+1,n+2}:new[]{n,n+2,n+1});
            }
            for(int i=0;i<outline.Length;i++){
                var a=outline[i];var b=outline[(i+1)%outline.Length];int n=v.Count;
                v.Add(new Vector3(a.x*side,-.045f,a.z));v.Add(new Vector3(a.x*side,.045f,a.z));v.Add(new Vector3(b.x*side,.045f,b.z));v.Add(new Vector3(b.x*side,-.045f,b.z));
                t.AddRange(side==1?new[]{n,n+2,n+1,n,n+3,n+2}:new[]{n,n+1,n+2,n,n+2,n+3});
            }
            return Save(name,v,t);
        }
        public Mesh DefeatBurst {
            get {
                const string key="Enemy defeat shards";
                if(meshes.TryGetValue("Combined "+key,out var cached))return cached;
                var pieces=new List<CombineInstance>();
                for(int i=0;i<3;i++) {
                    float angle=i*120*Mathf.Deg2Rad;
                    pieces.Add(new CombineInstance {mesh=Crystal,transform=Matrix4x4.TRS(new Vector3(Mathf.Cos(angle)*.3f,0,Mathf.Sin(angle)*.3f),Quaternion.Euler(25,i*120,25),new Vector3(.4f,.7f,.4f))});
                }
                return Combine(key,pieces);
            }
        }
        Mesh Save(string name,List<Vector3> vertices,List<int> triangles){var mesh=new Mesh{name="Original "+name};mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();meshes.Add(name,mesh);return mesh;}
        public Mesh Combine(string key,List<CombineInstance> pieces)
        {
            key="Combined "+key;
            if(meshes.TryGetValue(key,out var cached))return cached;
            var mesh=new Mesh{name=key};mesh.CombineMeshes(pieces.ToArray(),true,true);mesh.RecalculateBounds();meshes.Add(key,mesh);return mesh;
        }
        public void Dispose(){foreach(var mesh in meshes.Values)Object.Destroy(mesh);meshes.Clear();}
    }
}
