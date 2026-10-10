#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEditor.SceneManagement;

namespace FrostMaze.Tests
{
    public sealed class WallRimTests
    {
        static Vector3Int Key(Vector3 p)=>new Vector3Int(Mathf.RoundToInt(p.x*10000),Mathf.RoundToInt(p.y*10000),Mathf.RoundToInt(p.z*10000));
        [UnityTest]
        public IEnumerator RoundedEdgesPreserveEveryBlockedCellAndSealingFoot()
        {
            EditorSceneManager.OpenScene("Assets/Game/Maps/MazeLab.unity");yield return new EnterPlayMode();yield return null;
            foreach(var name in new[]{"Ironfold","Rimewatch"}){
                var game=Object.FindFirstObjectByType<Prototype>();game.ChooseMap(Resources.Load<MapDefinition>(name));yield return null;yield return null;
                game=Object.FindFirstObjectByType<Prototype>();var config=game.World.Config;string mask=string.Join("\n",config.LayoutRows);
                var scenery=game.GetComponentInChildren<MapScenery>();
                Assert.That(scenery.GetComponentsInChildren<Collider>(),Is.Empty);
                Assert.That(scenery.transform.Find("Scenery 19"),Is.Null,"Do not restore the repeated square crown trim");
                var cap=scenery.transform.Find("Scenery 1").GetComponent<MeshFilter>();
                var mesh=cap.sharedMesh;var v=mesh.vertices;var t=mesh.triangles;var normals=mesh.normals;
                Assert.That(mesh.vertexCount,Is.LessThan(400000));Assert.That(scenery.RoundedShelfQuads,Is.GreaterThan(100));
                Assert.That(cap.GetComponent<Renderer>().sharedMaterial.shader.name,Is.EqualTo("Howl/Painted ridge"));
                var shared=new Dictionary<Vector3Int,Vector3>();int sloping=0;
                for(int i=0;i<v.Length;i++){
                    var k=Key(v[i]);if(shared.TryGetValue(k,out var normal)&&v[i].y<.73f)Assert.That(Vector3.Distance(normal,normals[i]),Is.LessThan(.0001f),"Adjacent rim faces have a lighting seam");
                    shared[k]=normals[i];if(normals[i].y<.95f)sloping++;
                }
                Assert.That(sloping,Is.GreaterThan(100));
                foreach(var p in v)Assert.That(shared.ContainsKey(Key(new Vector3(config.Width-p.x,p.y,p.z))),Is.True,"Unmirrored rim vertex");
                float cell=config.LayoutCellSize;
                bool Solid(int x,int z){
                    int row=config.LayoutRows.Length-1-z;if(row<0||row>=config.LayoutRows.Length||x<0||x>=config.LayoutRows[row].Length)return false;
                    char k=config.LayoutRows[row][x];return config.WalkableSymbols.IndexOf(k)<0&&k!='p'&&k!='D'&&k!='W';
                }
                for(int i=0;i<t.Length;i+=3){
                    var a=v[t[i]];var b=v[t[i+1]];var c=v[t[i+2]];
                    float x0=Mathf.Min(a.x,b.x,c.x),x1=Mathf.Max(a.x,b.x,c.x),z0=Mathf.Min(a.z,b.z,c.z),z1=Mathf.Max(a.z,b.z,c.z);
                    // Exact shared boundaries belong to both adjacent cells; test positive-area coverage.
                    for(int z=Mathf.FloorToInt((z0+.00001f)/cell);z<=Mathf.FloorToInt((z1-.00001f)/cell);z++)
                    for(int x=Mathf.FloorToInt((x0+.00001f)/cell);x<=Mathf.FloorToInt((x1-.00001f)/cell);x++)Assert.That(Solid(x,z),Is.True,"Rounded wall covers lane or water");
                }
                var foot=scenery.transform.Find("Scenery 0").GetComponent<MeshFilter>().sharedMesh;
                var points=new HashSet<Vector3Int>();foreach(var p in foot.vertices)points.Add(Key(p));int edges=0;
                void Edge(float ax,float az,float bx,float bz){
                    foreach(var p in new[]{new Vector3(ax,0,az),new Vector3(bx,0,bz),new Vector3(ax,.035f,az),new Vector3(bx,.035f,bz)})Assert.That(points.Contains(Key(p)),Is.True,"Missing exact tower-to-wall sealing foot");edges++;
                }
                for(int z=0;z<config.LayoutRows.Length;z++)for(int x=0;x<config.LayoutRows[0].Length;x++){
                    if(!Solid(x,z))continue;float wx=x*cell,wz=z*cell;
                    if(!Solid(x-1,z))Edge(wx,wz,wx,wz+cell);
                    if(!Solid(x+1,z))Edge(wx+cell,wz,wx+cell,wz+cell);
                    if(!Solid(x,z-1))Edge(wx,wz,wx+cell,wz);
                    if(!Solid(x,z+1))Edge(wx,wz+cell,wx+cell,wz+cell);
                }
                Assert.That(edges,Is.GreaterThan(100));Assert.That(string.Join("\n",config.LayoutRows),Is.EqualTo(mask));
                Debug.Log($"HOWL_ROUNDED_WALLS {name} vertices={v.Length} rimQuads={scenery.RoundedShelfQuads} sealingEdges={edges}");
            }
            yield return new ExitPlayMode();
        }
    }
}
#endif
