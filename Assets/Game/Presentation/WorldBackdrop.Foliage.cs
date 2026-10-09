using System.Collections.Generic;
using UnityEngine;
namespace FrostMaze
{
    public sealed partial class WorldBackdrop
    {
        sealed class WindMesh {
            public Mesh Mesh;public Vector3[] Rest,Moved;
        }
        readonly List<WindMesh> foliage=new List<WindMesh>();
        void RememberFoliage(Mesh mesh){var rest=mesh.vertices;foliage.Add(new WindMesh{Mesh=mesh,Rest=rest,Moved=(Vector3[])rest.Clone()});mesh.MarkDynamic();}
        void MoveFoliage() {
            foreach(var wind in foliage){for(int i=0;i<wind.Rest.Length;i++){var p=wind.Rest[i];float mx=Mathf.Min(p.x,mirrorWidth-p.x);float sway=Mathf.Sin(breeze*1.1f+mx*.37f+p.z*.21f+p.y*.25f)*.045f;wind.Moved[i]=p+new Vector3(p.x>mirrorWidth*.5f?-sway:sway,0,sway*.35f);}wind.Mesh.vertices=wind.Moved;}
        }
        static void SmoothNormals(Mesh mesh) {
            var v=mesh.vertices;var n=mesh.normals;var totals=new Dictionary<Vector3Int,Vector3>();
            Vector3Int Key(Vector3 p)=>new Vector3Int(Mathf.RoundToInt(p.x*10000),Mathf.RoundToInt(p.y*10000),Mathf.RoundToInt(p.z*10000));
            for(int i=0;i<v.Length;i++){var k=Key(v[i]);totals.TryGetValue(k,out var normal);totals[k]=normal+n[i];}
            for(int i=0;i<v.Length;i++)n[i]=totals[Key(v[i])].normalized;mesh.normals=n;
        }
        static void Tree(Batch trunk,Batch leaves,Batch crown,float x,float z,float y,float size,bool ice) {
            trunk.Box(x-.14f,z-.13f,.28f,.26f,y,y+2.8f*size);
            if(ice) {
                for(int tier=0;tier<4;tier++){
                    float r=(1.45f-tier*.28f)*size,bottom=y+.6f+tier*.72f*size;
                    leaves.Bough(x,z,bottom,r,1.5f*size,tier);
                    crown.Bough(x,z,bottom+.36f*size,r*.79f,1.18f*size,tier);
                }
            } else {
                for(int lobe=0;lobe<4;lobe++) {
                    float a=lobe*2.2f,spread=lobe==3?0:.68f*size;
                    var p=new Vector3(x+Mathf.Cos(a)*spread,y+(lobe==3?3.6f:2.8f)*size,z+Mathf.Sin(a)*spread);
                    leaves.Blob(p,new Vector3(1.16f,1.16f,.98f)*size,lobe*7+x);
                    crown.Blob(p+new Vector3(-.1f,.6f,-.12f)*size,new Vector3(.83f,.59f,.72f)*size,lobe*7+x);
                }
            }
        }
    }
}
