using System.Collections.Generic;
using UnityEngine;
namespace FrostMaze
{
    public sealed partial class WorldBackdrop
    {
        static void SmoothNormals(Mesh mesh) {
            var v=mesh.vertices;var n=mesh.normals;var totals=new Dictionary<Vector3Int,Vector3>();
            Vector3Int Key(Vector3 p)=>new Vector3Int(Mathf.RoundToInt(p.x*10000),Mathf.RoundToInt(p.y*10000),Mathf.RoundToInt(p.z*10000));
            for(int i=0;i<v.Length;i++){var k=Key(v[i]);totals.TryGetValue(k,out var normal);totals[k]=normal+n[i];}
            for(int i=0;i<v.Length;i++)n[i]=totals[Key(v[i])].normalized;mesh.normals=n;
        }
    }
}
