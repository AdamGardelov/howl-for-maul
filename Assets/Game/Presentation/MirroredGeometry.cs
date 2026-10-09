using System.Collections.Generic;
using UnityEngine;
namespace FrostMaze
{
    // Clip triangles at the centre plane before reflecting: centre-spanning surfaces stay closed.
    static class MirroredGeometry
    {
        public static void Apply(List<Vector3> vertices,List<int> triangles,float width)
        {
            var output=new List<Vector3>();var indices=new List<int>();float centre=width*.5f;
            for(int t=0;t<triangles.Count;t+=3){
                var polygon=new List<Vector3>{vertices[triangles[t]],vertices[triangles[t+1]],vertices[triangles[t+2]]};
                var clipped=new List<Vector3>();
                for(int i=0;i<3;i++){
                    var a=polygon[i];var b=polygon[(i+1)%3];bool inside=a.x<=centre,next=b.x<=centre;
                    if(inside)clipped.Add(a);
                    if(inside!=next)clipped.Add(Vector3.Lerp(a,b,(centre-a.x)/(b.x-a.x)));
                }
                for(int i=1;i+1<clipped.Count;i++){
                    var a=clipped[0];var b=clipped[i];var c=clipped[i+1];if(Vector3.Cross(b-a,c-a).sqrMagnitude<.00000001f)continue;
                    int n=output.Count;output.Add(a);output.Add(b);output.Add(c);indices.Add(n);indices.Add(n+1);indices.Add(n+2);
                    if(Mathf.Abs(a.x-centre)+Mathf.Abs(b.x-centre)+Mathf.Abs(c.x-centre)<.0001f)continue;
                    output.Add(new Vector3(width-a.x,a.y,a.z));output.Add(new Vector3(width-c.x,c.y,c.z));output.Add(new Vector3(width-b.x,b.y,b.z));indices.Add(n+3);indices.Add(n+4);indices.Add(n+5);
                }
            }
            vertices.Clear();vertices.AddRange(output);triangles.Clear();triangles.AddRange(indices);
        }
        public static void Points(List<Vector3> points,float width){
            for(int i=points.Count-1;i>=0;i--)if(points[i].x>width*.5f+.001f)points.RemoveAt(i);
            int count=points.Count;for(int i=0;i<count;i++)if(points[i].x<width*.5f-.001f)points.Add(new Vector3(width-points[i].x,points[i].y,points[i].z));
        }
    }
}
