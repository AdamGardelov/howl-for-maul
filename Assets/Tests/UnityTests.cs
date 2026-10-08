#if UNITY_EDITOR
using NUnit.Framework;
namespace FrostMaze.Tests
{
    public sealed class UnityTests
    {
        [Test]
        public void CameraGesturesHandleEdgesFocusAndDragTransitions()
        {
            var input=new DesktopInput();var screen=new UnityEngine.Vector2(1440,900);var center=screen*.5f;
            CameraIntent Sample(UnityEngine.Vector2 mouse,bool focus=true,bool middle=false,bool left=false,bool space=false)
                =>input.ReadSample(mouse,screen,UnityEngine.Vector2.zero,1,focus,middle,left,space,false);
            Assert.That(Sample(new UnityEngine.Vector2(0,450)).Pan.x,Is.EqualTo(-1));
            Assert.That(Sample(new UnityEngine.Vector2(1439,899)).Pan,Is.EqualTo(UnityEngine.Vector2.one));
            Assert.That(Sample(new UnityEngine.Vector2(-1,450)).Pan,Is.EqualTo(UnityEngine.Vector2.zero));
            Assert.That(Sample(new UnityEngine.Vector2(1440,450)).Pan,Is.EqualTo(UnityEngine.Vector2.zero));
            Assert.That(Sample(new UnityEngine.Vector2(0,450),left:true).Pan,Is.EqualTo(UnityEngine.Vector2.zero),"Clicking UI must not edge-scroll");
            Assert.That(Sample(center,space:true).Dragging,Is.False);
            var first=Sample(center,left:true,space:true);Assert.That(first.DragStarted,Is.True);Assert.That(first.Drag,Is.EqualTo(UnityEngine.Vector2.zero));
            var moved=Sample(center+new UnityEngine.Vector2(80,40),left:true,space:true);
            Assert.That(moved.DragStarted,Is.False);Assert.That(moved.Drag,Is.EqualTo(new UnityEngine.Vector2(-80,-40)));
            var unfocused=Sample(new UnityEngine.Vector2(0,450),focus:false,left:true,space:true);
            Assert.That(unfocused.Pan,Is.EqualTo(UnityEngine.Vector2.zero));Assert.That(unfocused.Dragging,Is.False);Assert.That(unfocused.Zoom,Is.Zero);
            Assert.That(Sample(center,left:true,space:true).Drag,Is.EqualTo(UnityEngine.Vector2.zero),"Refocus must not jump");
            Sample(center);
            Assert.That(Sample(center,middle:true).DragStarted,Is.True,"Middle drag remains supported");
        }
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
