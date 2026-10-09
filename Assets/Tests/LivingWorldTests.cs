#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEditor.SceneManagement;
using FrostMaze.Simulation;
namespace FrostMaze.Tests
{
    public sealed class LivingWorldTests
    {
        [UnityTest]
        public IEnumerator GardensKeepBuildCellsAirRoutesAndHouseRoofsClear()
        {
            EditorSceneManager.OpenScene("Assets/Game/Maps/MazeLab.unity");yield return new EnterPlayMode();yield return null;
            foreach(var name in new[]{"Ironfold","Rimewatch"}){
                var game=Object.FindFirstObjectByType<Prototype>();game.ChooseMap(Resources.Load<MapDefinition>(name));yield return null;yield return null;
                game=Object.FindFirstObjectByType<Prototype>();game.StartMatch();game.Paused=true;
                var config=game.World.Config;string mask=string.Join("\n",config.LayoutRows);
                var worlds=game.GetComponentsInChildren<LivingWorld>();Assert.That(worlds.Length,Is.EqualTo(2));
                var oldMaterials=new List<Material>();
                foreach(var world in worlds){
                    Assert.That(world.GetComponentsInChildren<Collider>(),Is.Empty);
                    Assert.That(world.Trees,Is.GreaterThan(0),"At least one safe canopy group per half-map");Assert.That(world.Gardens,Is.GreaterThan(20));
                    foreach(var site in world.Sites)if(!world.Exterior)Assert.That(LivingWorld.FitsMap(config,site.x,site.z,site.w-LivingWorld.WindEnvelope),Is.True);
                    foreach(var filter in world.GetComponentsInChildren<MeshFilter>()){
                        Assert.That(filter.gameObject.layer,Is.EqualTo(world.Exterior?0:30));
                        var mesh=filter.sharedMesh;var vertices=mesh.vertices;var triangles=mesh.triangles;
                        var renderer=filter.GetComponent<Renderer>();oldMaterials.Add(renderer.sharedMaterial);
                        Assert.That(renderer.sharedMaterial.shader.isSupported,Is.True);
                        Assert.That(renderer.sharedMaterial.mainTexture,Is.Not.Null);
                        // Includes all leaf-card corners, stems, petal tips, foundations and roofline details.
                        for(int i=0;i<triangles.Length;i+=3){
                            var a=vertices[triangles[i]];var b=vertices[triangles[i+1]];var c=vertices[triangles[i+2]];
                            float x0=Mathf.Min(a.x,b.x,c.x),x1=Mathf.Max(a.x,b.x,c.x),z0=Mathf.Min(a.z,b.z,c.z),z1=Mathf.Max(a.z,b.z,c.z);
                            if(world.Exterior){
                                Assert.That(x1<=0||x0>=config.Width||z1<=0||z0>=config.Height,Is.True,"Exterior reaches gameplay: "+filter.name);
                                if(filter.name.Contains("crowns")||filter.name=="Living branches")foreach(var house in game.GetComponentInChildren<WorldBackdrop>().SettlementBounds)for(int side=0;side<2;side++){
                                    float left=side==0?house.xMin:config.Width-house.xMax,right=side==0?house.xMax:config.Width-house.xMin;
                                    Assert.That(x1<left||x0>right||z1<house.yMin||z0>house.yMax,Is.True,"Tree penetrates a roof: "+filter.name);
                                }
                            } else {
                                float wind=renderer.sharedMaterial.GetFloat("_Wind")>0?LivingWorld.WindEnvelope:0;
                                for(int z=Mathf.FloorToInt((z0-wind)/config.LayoutCellSize);z<=Mathf.FloorToInt((z1+wind)/config.LayoutCellSize);z++)
                                for(int x=Mathf.FloorToInt((x0-wind)/config.LayoutCellSize);x<=Mathf.FloorToInt((x1+wind)/config.LayoutCellSize);x++){
                                    int row=config.LayoutRows.Length-1-z;Assert.That(row,Is.InRange(0,config.LayoutRows.Length-1));Assert.That(x,Is.InRange(0,config.LayoutRows[row].Length-1));
                                    Assert.That(config.WalkableSymbols.IndexOf(config.LayoutRows[row][x]),Is.LessThan(0),"Windy foliage reaches a build cell: "+filter.name);
                                }
                                if(Mathf.Max(a.y,b.y,c.y)>1.35f){
                                    // A triangle's enclosing circle includes its complete horizontal footprint.
                                    float radius=new Vector2(x1-x0,z1-z0).magnitude*.5f;
                                    Assert.That(LivingWorld.ClearsFlight(config,(x0+x1)*.5f,(z0+z1)*.5f,radius),Is.True,"Tall foliage enters an air route");
                                }
                            }
                        }
                        // Every vertex has a partner. UVs stay attached when the winding is reflected.
                        int half=vertices.Length/2;Assert.That(vertices.Length%2,Is.Zero);
                        for(int i=0;i<half;i++)Assert.That(Vector3.Distance(vertices[i+half],new Vector3(config.Width-vertices[i].x,vertices[i].y,vertices[i].z)),Is.LessThan(.001f));
                    }
                }
                // Use the actual builder and wallet, with no free-money / instant-build shortcuts.
                int gold=game.World.Gold,cost=game.World.BuildCost;bool ordered=false;var start=game.World.BuilderPosition;
                for(int z=(int)start.Y-2;z<start.Y+5&&!ordered;z++)for(int x=(int)start.X-4;x<start.X+5&&!ordered;x++)if(game.World.CanBuild(x,z,out _))ordered=game.World.OrderBuild(x,z,out _);
                Assert.That(ordered,Is.True);for(int i=0;i<500&&game.World.Grid.Towers.Count==0;i++)game.World.Step();
                Assert.That(game.World.Grid.Towers.Count,Is.EqualTo(1));Assert.That(game.World.Gold,Is.EqualTo(gold-cost));Assert.That(string.Join("\n",config.LayoutRows),Is.EqualTo(mask));
                Assert.That(game.GetComponentInChildren<MapScenery>().UsesBakedSurfaces,Is.True);
                game.OpenSetup();game.ChooseMap(Resources.Load<MapDefinition>(name=="Ironfold"?"Rimewatch":"Ironfold"));yield return null;yield return null;
                foreach(var material in oldMaterials)Assert.That(material==null,Is.True,"Changing maps leaked a living-world material");
            }
            yield return new ExitPlayMode();
        }
    }
}
#endif
