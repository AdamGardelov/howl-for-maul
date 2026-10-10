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
        public Mesh Shell => Profile("Carved shell",16,new[]{-.5f,-.27f,.14f,.36f,.5f},new[]{0f,.4f,.5f,.33f,0f},true);
        public Mesh Armor => Profile("Beveled armor",12,new[]{-.5f,-.3f,.32f,.5f},new[]{.38f,.5f,.5f,.38f});
        public Mesh Heartwood => Profile("Gnarled heartwood",14,new[]{-.5f,-.35f,-.08f,.16f,.37f,.5f},new[]{.49f,.42f,.38f,.48f,.39f,.32f},true);
        public Mesh PineCrown => Profile("Tiered pine needles",12,new[]{-.5f,-.43f,-.17f,-.13f,.16f,.20f,.5f},new[]{.43f,.50f,.20f,.38f,.09f,.23f,0f},true);
        public Mesh Thorn {
            get {
                const string key="Hooked hawthorn";if(meshes.TryGetValue(key,out var cached))return cached;
                var mesh=Profile(key,8,new[]{0f,.2f,.55f,.82f,1f},new[]{.28f,.24f,.13f,.05f,0f},true);
                var vertices=mesh.vertices;var normals=mesh.normals;
                for(int i=0;i<vertices.Length;i++){
                    float y=vertices[i].y;vertices[i].x+=.23f*y*y;
                    normals[i]=new Vector3(normals[i].x,normals[i].y-.46f*y*normals[i].x,normals[i].z).normalized;
                }
                mesh.vertices=vertices;mesh.normals=normals;mesh.RecalculateBounds();return mesh;
            }
        }
        public Mesh Robe => Profile("Warden mantle",12,new[]{-.5f,-.38f,.32f,.5f},new[]{.36f,.5f,.24f,.2f},true);
        public Mesh Bell => Profile("Ward bell",16,new[]{-.48f,-.44f,-.12f,.35f,.48f},new[]{.49f,.5f,.31f,.20f,.08f},true);
        public Mesh Halo {
            get {
                const string name="Cast bronze halo";if(meshes.TryGetValue(name,out var cached))return cached;
                var v=new List<Vector3>();var t=new List<int>();
                for(int i=0;i<32;i++)for(int j=0;j<6;j++){
                    int n=v.Count;
                    for(int c=0;c<4;c++){
                        float a=(i+(c==1||c==2?1:0))*Mathf.PI/16,b=(j+(c>=2?1:0))*Mathf.PI/3;
                        float r=.455f+Mathf.Cos(b)*.045f;v.Add(new Vector3(Mathf.Cos(a)*r,Mathf.Sin(b)*.045f,Mathf.Sin(a)*r));
                    }
                    t.AddRange(new[]{n,n+2,n+1,n,n+3,n+2});
                }
                var mesh=Save(name,v,t);
                var normals=new Vector3[v.Count];
                for(int i=0;i<v.Count;i++){var p=v[i];var center=new Vector3(p.x,0,p.z).normalized*.455f;normals[i]=(p-center).normalized;}
                mesh.normals=normals;return mesh;
            }
        }
        public Mesh BeveledBox {
            get {
                const string name="Chamfered armor block";if(meshes.TryGetValue(name,out var cached))return cached;
                var outline=new[]{new Vector2(.5f,.39f),new Vector2(.39f,.5f),new Vector2(-.39f,.5f),new Vector2(-.5f,.39f),new Vector2(-.5f,-.39f),new Vector2(-.39f,-.5f),new Vector2(.39f,-.5f),new Vector2(.5f,-.39f)};
                var v=new List<Vector3>();var t=new List<int>();float[] ys={-.5f,-.43f,.43f,.5f},rs={.85f,1,1,.85f};
                for(int ring=0;ring<3;ring++)for(int i=0;i<8;i++) {
                    int n=v.Count;var a=outline[i];var b=outline[(i+1)%8];
                    v.Add(new Vector3(a.x*rs[ring],ys[ring],a.y*rs[ring]));v.Add(new Vector3(a.x*rs[ring+1],ys[ring+1],a.y*rs[ring+1]));
                    v.Add(new Vector3(b.x*rs[ring+1],ys[ring+1],b.y*rs[ring+1]));v.Add(new Vector3(b.x*rs[ring],ys[ring],b.y*rs[ring]));
                    t.AddRange(new[]{n,n+1,n+2,n,n+2,n+3});
                }
                for(int end=0;end<2;end++)for(int i=0;i<8;i++) {
                    int n=v.Count;float y=end==0?-.5f:.5f;var a=outline[i]*.85f;var b=outline[(i+1)%8]*.85f;
                    v.Add(new Vector3(0,y,0));v.Add(new Vector3(a.x,y,a.y));v.Add(new Vector3(b.x,y,b.y));t.AddRange(end==0?new[]{n,n+1,n+2}:new[]{n,n+2,n+1});
                }
                return Save(name,v,t);
            }
        }
        public Mesh OwnerRing {
            get {
                const string name="Builder ownership ring";if(meshes.TryGetValue(name,out var cached))return cached;
                var v=new List<Vector3>();var t=new List<int>();
                for(int i=0;i<32;i++){float a=i*Mathf.PI/16,b=(i+1)*Mathf.PI/16;int n=v.Count;
                    v.Add(new Vector3(Mathf.Cos(a)*.44f,0,Mathf.Sin(a)*.44f));v.Add(new Vector3(Mathf.Cos(a)*.49f,0,Mathf.Sin(a)*.49f));
                    v.Add(new Vector3(Mathf.Cos(b)*.49f,0,Mathf.Sin(b)*.49f));v.Add(new Vector3(Mathf.Cos(b)*.44f,0,Mathf.Sin(b)*.44f));t.AddRange(new[]{n,n+2,n+1,n,n+3,n+2});}
                return Save(name,v,t);
            }
        }
        public Mesh Leaf {
            get {
                const string key="Living leaf";if(meshes.TryGetValue(key,out var cached))return cached;
                var outline=new[]{new Vector3(0,-.5f,0),new Vector3(-.26f,-.18f,0),new Vector3(-.34f,.16f,0),new Vector3(0,.5f,.04f),new Vector3(.34f,.16f,0),new Vector3(.26f,-.18f,0)};
                var vertices=new List<Vector3>();var indices=new List<int>();
                for(int face=0;face<2;face++)for(int i=0;i<outline.Length;i++){
                    int n=vertices.Count;vertices.Add(new Vector3(0,0,.12f));vertices.Add(outline[i]);vertices.Add(outline[(i+1)%outline.Length]);
                    indices.AddRange(face==0?new[]{n,n+1,n+2}:new[]{n,n+2,n+1});
                }
                return Save(key,vertices,indices);
            }
        }
        public Mesh CurvedPipe(string key,Vector3[] points,float width)
        {
            key="Cast pipe "+key;if(meshes.TryGetValue(key,out var cached))return cached;
            const int steps=6,sides=10;
            var centers=new List<Vector3>();
            for(int section=0;section<points.Length-1;section++)for(int step=0;step<steps;step++){
                float t=step/(float)steps;
                var a=points[Mathf.Max(0,section-1)];var b=points[section];var c=points[section+1];var d=points[Mathf.Min(points.Length-1,section+2)];
                centers.Add(.5f*((2*b)+(-a+c)*t+(2*a-5*b+4*c-d)*t*t+(-a+3*b-3*c+d)*t*t*t));
            }
            centers.Add(points[points.Length-1]);
            var vertices=new List<Vector3>();var triangles=new List<int>();var normals=new List<Vector3>();
            for(int ring=0;ring<centers.Count;ring++){
                var tangent=(centers[Mathf.Min(ring+1,centers.Count-1)]-centers[Mathf.Max(0,ring-1)]).normalized;
                var frame=Quaternion.FromToRotation(Vector3.up,tangent);
                for(int side=0;side<sides;side++){
                    float angle=side*Mathf.PI*2/sides;var normal=frame*new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle));
                    vertices.Add(centers[ring]+normal*width*.5f);normals.Add(normal);
                    if(ring==0)continue;
                    int a=(ring-1)*sides+side,b=ring*sides+side,c=ring*sides+(side+1)%sides,d=(ring-1)*sides+(side+1)%sides;
                    triangles.AddRange(new[]{a,b,c,a,c,d});
                }
            }
            // Closed ends prevent an open tube being visible where it meets the cast housing.
            for(int end=0;end<2;end++){
                int ring=end==0?0:centers.Count-1,index=vertices.Count;
                var normal=(centers[ring]-centers[end==0?1:centers.Count-2]).normalized;
                vertices.Add(centers[ring]);normals.Add(normal);
                for(int side=0;side<sides;side++){
                    int a=ring*sides+side,b=ring*sides+(side+1)%sides;
                    triangles.AddRange(end==0?new[]{index,a,b}:new[]{index,b,a});
                }
            }
            var mesh=Save(key,vertices,triangles);mesh.SetNormals(normals);return mesh;
        }
        Mesh Profile(string name,int sides,float[] heights,float[] radii,bool smooth=false)
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
            mesh=Save(name,v,t);
            if(smooth){
                var summed=new Dictionary<Vector3Int,Vector3>();var normals=mesh.normals;
                Vector3Int Key(Vector3 p)=>new Vector3Int(Mathf.RoundToInt(p.x*10000),Mathf.RoundToInt(p.y*10000),Mathf.RoundToInt(p.z*10000));
                for(int i=0;i<v.Count;i++){var key=Key(v[i]);summed.TryGetValue(key,out var normal);summed[key]=normal+normals[i];}
                for(int i=0;i<v.Count;i++)normals[i]=summed[Key(v[i])].normalized;
                mesh.normals=normals;
            }
            return mesh;
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
        public Mesh Rubble {
            get {
                const string key="Tower rubble";
                if(meshes.TryGetValue("Combined "+key,out var cached))return cached;
                var pieces=new List<CombineInstance>();
                for(int i=0;i<4;i++) {
                    float angle=i*90*Mathf.Deg2Rad;
                    pieces.Add(new CombineInstance{mesh=Armor,transform=Matrix4x4.TRS(new Vector3(Mathf.Cos(angle)*.3f,.05f,Mathf.Sin(angle)*.3f),Quaternion.Euler(15,i*90,25),new Vector3(.35f,.3f,.4f))});
                }
                return Combine(key,pieces);
            }
        }
        Mesh Save(string name,List<Vector3> vertices,List<int> triangles){var mesh=new Mesh{name="Original "+name};mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();
            var uv=new List<Vector2>();var normals=mesh.normals;
            for(int i=0;i<vertices.Count;i++){var p=vertices[i];var n=normals[i];uv.Add(Mathf.Abs(n.y)>.7f?new Vector2(p.x+.5f,p.z+.5f):Mathf.Abs(n.x)>Mathf.Abs(n.z)?new Vector2(p.z+.5f,p.y*.5f+.5f):new Vector2(p.x+.5f,p.y*.5f+.5f));}
            mesh.SetUVs(0,uv);mesh.RecalculateBounds();meshes.Add(name,mesh);return mesh;}
        public Mesh Combine(string key,List<CombineInstance> pieces)
        {
            key="Combined "+key;
            if(meshes.TryGetValue(key,out var cached))return cached;
            var mesh=new Mesh{name=key};mesh.CombineMeshes(pieces.ToArray(),true,true);mesh.RecalculateBounds();meshes.Add(key,mesh);return mesh;
        }
        public void Dispose(){foreach(var mesh in meshes.Values)Object.Destroy(mesh);meshes.Clear();}
    }
}
