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
            Assert.That(input.ReadSample(center,screen,UnityEngine.Vector2.zero,0,true,false,false,false,false,1).Rotate,Is.EqualTo(1));
            Assert.That(input.ReadSample(center,screen,UnityEngine.Vector2.zero,0,false,false,false,false,false,1).Rotate,Is.Zero);
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
        [Test, Category("TerrainMaterials")]
        public void SnowSurfaceRepeatsContinuouslyAcrossExteriorTiles()
        {
            var method=typeof(MapScenery).GetMethod("RaisedSurfaceColor",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static);
            Assert.That(method,Is.Not.Null);
            bool ice=true;UnityEngine.Color Sample(float x,float z)=>(UnityEngine.Color)method.Invoke(null,new object[]{x,z,ice});
            void Near(UnityEngine.Color a,UnityEngine.Color b,float tolerance)
            {
                for(int channel=0;channel<3;channel++)Assert.That(a[channel],Is.EqualTo(b[channel]).Within(tolerance),"Snow tile color discontinuity");
            }
            foreach(bool theme in new[]{true,false}){ice=theme;
            foreach(float coordinate in new[]{-127.3f,-64f,-.1f,0f,7.5f,31.7f,63.9f,64f,140.2f}) {
                Near(Sample(0,coordinate),Sample(64,coordinate),.0001f);
                Near(Sample(coordinate,0),Sample(coordinate,64),.0001f);
                Near(Sample(coordinate,17),Sample(coordinate+64,17),.0001f);
                Near(Sample(-.001f,coordinate),Sample(.001f,coordinate),.001f);
                Near(Sample(coordinate,63.999f),Sample(coordinate,64.001f),.001f);
            }}
        }
        [Test]
        public void CompactTowerTooltipsTrackOwnedRequirementsAndExactShortfall()
        {
            foreach(bool iron in new[]{false,true}) {
                var config=MapCases.Load(iron);config.BuilderEnabled=false;
                for(int faction=0;faction<config.Factions.Length;faction++) {
                    var world=new FrostMaze.Simulation.World(config,new FrostMaze.Simulation.MatchOptions{PlayerCount=2,Factions=new[]{faction,faction,0,0}});
                    int gold=world.Gold,selected=world.SelectedDesign;long tick=world.Tick;
                    foreach(int design in config.Factions[faction].Designs) {
                        var tower=config.Catalog[design];string tip=PrototypeHud.BuildTooltip(world,design);
                        Assert.That(tip,Does.Contain(tower.Name));
                        Assert.That(tip,Does.Contain(PrototypeHud.TowerStatsText(tower.Spec)));
                        if(tower.Spec.Damage==0)Assert.That(tip,Does.Contain("no weapon").And.Not.Contain("direct DPS"));
                        else Assert.That(tip,Does.Contain(tower.Spec.TargetsGround?(tower.Spec.TargetsAir?"Ground + air":"Ground only"):"Air only"));
                        foreach(int required in tower.Requires)Assert.That(tip,Does.Contain("• "+config.Catalog[required].Name));
                    }
                    Assert.That(world.Gold,Is.EqualTo(gold));Assert.That(world.SelectedDesign,Is.EqualTo(selected));Assert.That(world.Tick,Is.EqualTo(tick));Assert.That(world.Grid.Towers.Count,Is.Zero);
                    if(!iron)continue;
                    var roster=config.Factions[faction].Designs;int champion=roster[roster.Length-1];
                    FrostMaze.Simulation.Tower Buy(int design) {
                        world.SelectedDesign=design;
                        for(int y=1;y<63;y++)for(int x=1;x<63;x++)if(world.Build(x,y,out _))return world.Grid.At(x,y);
                        Assert.Fail("Paid tooltip prerequisite fixture could not build");return null;
                    }
                    world.SelectPlayer(1);Buy(roster[0]);world.SelectPlayer(0);
                    Assert.That(PrototypeHud.BuildTooltip(world,champion),Does.Contain("• "+config.Catalog[roster[0]].Name),"Teammate tower must not satisfy owned requirement");
                    var first=Buy(roster[0]);
                    Assert.That(PrototypeHud.BuildTooltip(world,champion),Does.Not.Contain("• "+config.Catalog[roster[0]].Name));
                    for(int d=1;d<roster.Length-1;d++)Buy(roster[d]);
                    string unlocked=PrototypeHud.BuildTooltip(world,champion);
                    Assert.That(unlocked,Does.Not.Contain("Missing owned towers"));
                    Assert.That(unlocked,Does.Contain("Need "+(config.Catalog[champion].Cost-world.Gold)+"g more."));
                    Assert.That(world.Sell(first.CellX,first.CellY),Is.True);
                    Assert.That(PrototypeHud.BuildTooltip(world,champion),Does.Contain("• "+config.Catalog[roster[0]].Name),"Sold prerequisite remained marked owned");
                }
            }
        }
        public static System.Collections.IEnumerable Cases {get{foreach(var test in SimulationCases.All)yield return new TestCaseData(test).SetName(test.Name);}}
        [TestCaseSource(nameof(Cases))] public void Simulation(Case test){test.Run();}
    }
}
#endif
