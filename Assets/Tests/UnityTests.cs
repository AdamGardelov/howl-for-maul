#if UNITY_EDITOR
using NUnit.Framework;
namespace FrostMaze.Tests
{
    public sealed class UnityTests
    {
        [Test]
        public void MinimapTerrainMatchesBothSourceGridsAndCachesOnlyPermanentCells()
        {
            using(var cache=new MinimapTerrain())foreach(bool iron in new[]{false,true}) {
                var world=new FrostMaze.Simulation.World(MapCases.Load(iron));float step=world.PlacementStep;
                var texture=cache.Get(world.Grid,step);
                Assert.That(texture.width,Is.EqualTo(iron?128:64));
                for(int y=0;y<texture.height;y++)for(int x=0;x<texture.width;x++) {
                    bool blocked=world.Config.WalkableSymbols.IndexOf(world.Config.LayoutRows[texture.height-1-y][x])<0;
                    Assert.That((UnityEngine.Color32)texture.GetPixel(x,y),Is.EqualTo(blocked?MinimapTerrain.Wall:MinimapTerrain.Ground),"Minimap mask at "+x+","+y);
                }
                Assert.That(cache.Get(world.Grid,step),Is.SameAs(texture));
                bool built=false;
                for(int y=1;y<world.Grid.Height-1&&!built;y++)for(int x=1;x<world.Grid.Width-1&&!built;x++)
                    if(world.Grid.Build(x,y,new FrostMaze.Simulation.TowerSpec())!=null)built=true;
                Assert.That(built,Is.True);
                Assert.That(cache.Get(world.Grid,step),Is.SameAs(texture),"Building must not regenerate permanent terrain");
                var old=texture;
                var other=new FrostMaze.Simulation.MazeGrid(4,4);cache.Get(other,1);
                Assert.That(old==null,Is.True,"Replacing a map must release its texture");
            }
        }
        public static System.Collections.IEnumerable Cases {get{foreach(var test in SimulationCases.All)yield return new TestCaseData(test).SetName(test.Name);}}
        [TestCaseSource(nameof(Cases))] public void Simulation(Case test){test.Run();}
    }
}
#endif
