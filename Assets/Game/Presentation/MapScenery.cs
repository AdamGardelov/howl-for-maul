using System.Collections.Generic;
using UnityEngine;
namespace FrostMaze
{
    // Original scenery is generated separately from the authoritative navigation mask.
    public sealed class MapScenery : MonoBehaviour
    {
        readonly List<Mesh> meshes=new List<Mesh>();
        sealed class Batch {
            public readonly List<Vector3> V=new List<Vector3>();public readonly List<int> T=new List<int>();
            void Quad(Vector3 a,Vector3 b,Vector3 c,Vector3 d){int n=V.Count;V.AddRange(new[]{a,b,c,d});T.AddRange(new[]{n,n+1,n+2,n,n+2,n+3});}
            public void Box(float x,float z,float w,float d,float bottom,float top){
                var a=new Vector3(x,bottom,z);var b=new Vector3(x+w,bottom,z);var c=new Vector3(x+w,bottom,z+d);var e=new Vector3(x,bottom,z+d);var u=Vector3.up*(top-bottom);
                Quad(a+u,e+u,c+u,b+u);Quad(a,b,b+u,a+u);Quad(b,c,c+u,b+u);Quad(c,e,e+u,c+u);Quad(e,a,a+u,e+u);
            }
            public void Peak(float x,float z,float radius,float bottom,float height) {
                for(int i=0;i<6;i++){float a=i*Mathf.PI/3,b=(i+1)*Mathf.PI/3;int n=V.Count;V.Add(new Vector3(x,bottom+height,z));V.Add(new Vector3(x+Mathf.Cos(b)*radius,bottom,z+Mathf.Sin(b)*radius));V.Add(new Vector3(x+Mathf.Cos(a)*radius,bottom,z+Mathf.Sin(a)*radius));T.AddRange(new[]{n,n+1,n+2});}
            }
        }
        public void Build(Prototype game) {
            var c=game.World.Config;bool ice=c.Theme!="iron";var batches=new Batch[8];for(int i=0;i<batches.Length;i++)batches[i]=new Batch();
            foreach(var t in c.Terrain) {
                bool voidArea=t.Kind=="D"||t.Kind=="W"||t.Kind=="p";
                float h=voidArea?.04f:t.Kind=="#"?.5f:1.05f;
                batches[voidArea?2:0].Box(t.X,t.Y,t.Width,t.Height,-.04f,h);
                if(!voidArea)batches[1].Box(t.X+.035f,t.Y+.035f,Mathf.Max(.05f,t.Width-.07f),Mathf.Max(.05f,t.Height-.07f),h,h+.09f);
            }
            // Shallow walkable surface panels add scale without changing navigation or colliders.
            for(int row=0;row<c.LayoutRows.Length;row++)for(int col=0;col<c.LayoutRows[row].Length;col++) {
                if(c.WalkableSymbols.IndexOf(c.LayoutRows[row][col])<0)continue;
                float cell=c.LayoutCellSize,x=col*cell,z=(c.LayoutRows.Length-row-1)*cell;
                int variant=((row*17+col*31)%11)==0?7:6;
                float seam=ice?.008f:.025f;
                batches[variant].Box(x+seam,z+seam,cell-2*seam,cell-2*seam,-.018f,-.006f);
            }
            // Sparse landmarks occupy only the interiors of blocked source cells.
            for(int row=2;row<c.LayoutRows.Length-2;row+=ice?4:7)for(int col=2;col<c.LayoutRows[row].Length-2;col+=ice?4:7) {
                char k=c.LayoutRows[row][col];if(k!=(ice?'T':'#'))continue;
                bool interior=true;for(int y=-1;y<=1;y++)for(int x=-1;x<=1;x++)if(c.LayoutRows[row+y][col+x]!=k)interior=false;if(!interior)continue;
                float x0=(col+.5f)*c.LayoutCellSize,z=(c.LayoutRows.Length-row-.5f)*c.LayoutCellSize;
                if(ice){batches[3].Box(x0-.09f,z-.09f,.18f,.18f,1,1.8f);batches[4].Peak(x0,z,.6f,1.3f,1.5f);batches[1].Peak(x0,z,.43f,1.85f,1.05f);}
                else {batches[3].Box(x0-.25f,z-.25f,.5f,.5f,.5f,2);batches[5].Box(x0-.28f,z-.28f,.56f,.56f,1.6f,1.72f);}
            }
            foreach(var lane in c.Lanes) {
                float x=lane.Spawn.X,z=lane.Spawn.Y;
                for(int side=-1;side<=1;side+=2){batches[3].Box(x+side*1.1f-.15f,z-.15f,.3f,.3f,0,1.9f);batches[5].Peak(x+side*1.1f,z,.24f,1.9f,.5f);}
            }
            var exit=c.GroundRoute[c.GroundRoute.Length-1];
            for(int side=-1;side<=1;side+=2){batches[3].Box(exit.X+side*1.25f-.25f,exit.Y-.25f,.5f,.5f,0,2.8f);batches[5].Box(exit.X+side*1.25f-.12f,exit.Y-.27f,.24f,.54f,1,2.5f);}
            batches[3].Box(exit.X-1.5f,exit.Y-.25f,3,.5f,2.6f,2.9f);
            Color[] colors=ice?new[]{new Color(.12f,.3f,.38f),new Color(.82f,.92f,.94f),new Color(.035f,.12f,.2f),new Color(.22f,.32f,.35f),new Color(.06f,.24f,.22f),new Color(.2f,.95f,.85f)}:new[]{new Color(.19f,.17f,.23f),new Color(.4f,.32f,.28f),new Color(.065f,.05f,.09f),new Color(.33f,.24f,.16f),new Color(.2f,.22f,.25f),new Color(1,.52f,.12f)};
            var palette=new List<Color>(colors);
            palette.Add(ice?new Color(.48f,.65f,.72f):new Color(.24f,.28f,.31f));
            palette.Add(ice?new Color(.54f,.7f,.76f):new Color(.28f,.32f,.35f));
            for(int i=0;i<batches.Length;i++) {
                var b=batches[i];if(b.V.Count==0)continue;var mesh=new Mesh{name="Original terrain batch "+i,indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};mesh.SetVertices(b.V);mesh.SetTriangles(b.T,0);mesh.RecalculateNormals();mesh.RecalculateBounds();meshes.Add(mesh);
                var obj=new GameObject("Scenery "+i);obj.transform.SetParent(transform,false);obj.AddComponent<MeshFilter>().sharedMesh=mesh;obj.AddComponent<MeshRenderer>().sharedMaterial=game.MakeMaterial(palette[i],i==5);
            }
        }
        void OnDestroy(){foreach(var mesh in meshes)Destroy(mesh);}
    }
}
