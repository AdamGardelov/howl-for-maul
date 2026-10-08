using System;
using System.IO;
using FrostMaze.Simulation;
namespace FrostMaze.Tests
{
 public static class MapCases
 {
  static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
  public static Scenario Load(bool iron) {
#if UNITY_EDITOR
   string root=UnityEngine.Application.dataPath+"/Game/Maps/LayoutSources/";
#else
   string root=AppContext.BaseDirectory+"Layouts/";
#endif
   string text=File.ReadAllText(root+(iron?"Ironfold":"Rimewatch")+".txt");return iron?ReferenceMaps.Ironfold(text):ReferenceMaps.Rimewatch(text);
  }
  // Three alternating arms in the supplied upper-left corridor, leaving one-unit gaps.
  // This is a test strategy, never generated terrain or a restriction on player building.
  public static int[,] MazeCells(bool iron) {
   int left=iron?8:6,right=iron?14:12,top=iron?56:50;
   var cells=new int[3*(right-left),2];int at=0;
   for(int row=0;row<3;row++)for(int x=left;x<=right;x++) {
    if(x==(row%2==0?right:left))continue;
    cells[at,0]=x;cells[at++,1]=top-row*4;
   }
   return cells;
  }
  public static int MazeDesign(World w) {
   int best=w.Config.Factions[w.Players[w.ActivePlayer].Faction].Designs[0];
   foreach(int design in w.Config.Factions[w.Players[w.ActivePlayer].Faction].Designs)
    if(w.Config.Catalog[design].Cost<w.Config.Catalog[best].Cost)best=design;
   return best;
  }
  public static void PaidReferenceMazes() {
   foreach(bool iron in new[]{false,true}) {
    var c=Load(iron);var empty=new World(c);var maze=new World(c);maze.SelectedDesign=MazeDesign(maze);
    int cost=maze.BuildCost;var cells=MazeCells(iron);
    for(int cell=0;cell<cells.GetLength(0);cell++) {
     Check(maze.OrderBuild(cells[cell,0],cells[cell,1],out string reason),"maze purchase rejected: "+reason);
     for(int tick=0;tick<1000&&maze.HasBuildOrder;tick++)maze.Step();
     Check(!maze.HasBuildOrder&&maze.Grid.Towers.Count==cell+1,"paid maze order did not complete");
    }
    Check(maze.Gold==c.StartingGold-cost*cells.GetLength(0),"maze spending was not paid exactly");
    var baseline=Traverse(empty);var detour=Traverse(maze);
    Check(detour[0]>baseline[0]+60,c.Name+" maze failed to lengthen ground traversal");
    for(int lane=0;lane<maze.LaneCount;lane++) {
     Check(detour[lane*2+1]==baseline[lane*2+1],"maze changed flight traversal");
     if(lane>0)Check(detour[lane*2]==baseline[lane*2],"left maze changed another lane");
    }
    // Selling opens the shortcut and invalidates its cached navigation fields.
    foreach(var tower in maze.Grid.Towers.ToArray())Check(maze.Sell(tower.CellX,tower.CellY),"owned maze sale failed");
    var reopened=Traverse(maze);
    Check(reopened[0]==baseline[0],"selling maze did not restore original traversal");
   }
  }
  static int[] Traverse(World w) {
   w.TowersFire=false;var units=new Enemy[w.LaneCount*2];var ticks=new int[units.Length];
   for(int lane=0;lane<w.LaneCount;lane++)for(int air=0;air<2;air++) {
    int index=lane*2+air;units[index]=w.Spawn(new WaveSpec{Flying=air==1},w.LaneSpawn(lane),lane);Check(units[index]!=null,"test spawn blocked");
   }
   for(int tick=1;tick<=9000&&w.Enemies.Count>0;tick++) {
    w.Step();
    for(int i=0;i<units.Length;i++) {
     Check(!units[i].Blocked,"open zig-zag triggered siege");
     if(units[i].Exited&&ticks[i]==0)ticks[i]=tick;
    }
   }
   foreach(int tick in ticks)Check(tick>0,"maze traversal stalled");
   return ticks;
  }
  public static void Masks() {
   foreach(bool iron in new[]{false,true}) {var c=Load(iron);var w=new World(c);Check(w.LaneCount==(iron?4:3),"lane count");
    for(int y=0;y<c.LayoutRows.Length;y++)for(int x=0;x<c.LayoutRows[y].Length;x++) {
     var p=new V2((x+.5f)*c.LayoutCellSize,(c.LayoutRows.Length-y-.5f)*c.LayoutCellSize);
     Check(w.Grid.TerrainClear(p,p,.01f)==(c.WalkableSymbols.IndexOf(c.LayoutRows[y][x])>=0),$"mask mismatch {c.Name} {x},{y}");
    }
    var random=new Random(42);for(int n=0;n<300;n++) {
     var a=new V2((float)random.NextDouble()*64,(float)random.NextDouble()*64);var b=a+new V2((float)random.NextDouble()*8-4,(float)random.NextDouble()*8-4);
     bool clear=w.Grid.InBounds(a,.2f)&&w.Grid.InBounds(b,.2f);foreach(var t in c.Terrain)if(Geometry.SweepBox(a,b,t.Center,t.Half,.2f))clear=false;
     Check(clear==w.Grid.TerrainClear(a,b,.2f),"terrain bucket disagrees with exhaustive collision");
    }
   }
  }
  public static void Routes() {
   foreach(bool iron in new[]{false,true}) {var w=new World(Load(iron));w.TowersFire=false;
    for(int lane=0;lane<w.LaneCount;lane++)Check(w.Spawn(new WaveSpec(),w.LaneSpawn(lane),lane)!=null,"spawn blocked");
    for(int i=0;i<9000&&w.Enemies.Count>0;i++) {w.Step();foreach(var e in w.Enemies)Check(!e.Blocked&&w.Grid.TerrainClear(e.Position,e.Position,e.Spec.Radius),$"{w.Config.Name} lane {e.Lane} blocked at {e.Position}");}
    Check(w.Leaked==w.LaneCount,$"{w.Config.Name} route stalled {w.Leaked}/{w.LaneCount}");
   }
  }
  public static void FactionSelection() {
   var c=Load(false);var o=new MatchOptions{PlayerCount=4,Factions=new[]{0,1,2,3}};var w=new World(c,o);
   for(int i=0;i<4;i++){w.SelectPlayer(i);Check(w.SelectedDesign==i*5&&w.DesignAvailable(i*5+4),"wrong roster");bool rejected=false;try{w.SelectedDesign=((i+1)%4)*5;}catch(ArgumentOutOfRangeException){rejected=true;}Check(rejected,"foreign faction tower accepted");}
   o.Factions[3]=0;Check(w.Restart().Players[3].Faction==3,"options not copied");
  }
  public static void RobotProgression() {
   var c=Scenario.SharedDefense();RobotFactions.Apply(c);c.BuilderEnabled=false;
   Check(c.Factions.Length==8&&c.Catalog.Length==56,"robot roster size");
   for(int faction=0;faction<8;faction++) {
    var w=new World(c,new MatchOptions{PlayerCount=2,Factions=new[]{faction,faction,0,0}});w.Players[0].Gold=2000;w.Players[1].Gold=2000;
    w.SelectedDesign=faction*7+6;Check(!w.Build(3,28,out _),"champion unlocked without prerequisites");
    for(int t=0;t<6;t++){w.SelectedDesign=faction*7+t;Check(w.Build(3,23+t,out _),"regular tower failed");}
    w.SelectedDesign=faction*7+6;Check(w.Build(4,28,out _),"champion stayed locked");
    w.SelectPlayer(1);w.SelectedDesign=faction*7+6;Check(!w.RequirementsMet(w.SelectedDesign),"other player unlocked champion");
    w.SelectPlayer(0);w.Sell(3,23);Check(!w.RequirementsMet(faction*7+6),"sold prerequisite still counted");
   }
  }
  public static void WavePreviews() {
   foreach(Difficulty difficulty in new[]{Difficulty.Relaxed,Difficulty.Normal,Difficulty.Hard}) {
    var c=Scenario.SharedDefense();var w=new World(c,new MatchOptions{Difficulty=difficulty});
    var preview=w.PreviewWave(0);float health=preview.Health;preview.Health=9999;
    Check(w.PreviewWave(0).Health==health&&c.Waves[0].Health==35,"preview modified live data");
    w.StartWave();w.Step();Check(w.Enemies.Count==4&&w.Enemies[0].Spec.Health==health,"preview disagrees with spawn");
    Check(w.PreviewWave(4).Flying,"air forecast wrong");
   }
   var map=Scenario.SharedDefense();RobotFactions.Apply(map);map.BuilderEnabled=false;
   var world=new World(map,new MatchOptions{PlayerCount=2});world.Players[0].Gold=2000;
   int missing=0;foreach(int design in world.MissingPrerequisites(6))missing++;Check(missing==6,"missing prerequisites incomplete");
   world.Build(3,23,out _);missing=0;foreach(int design in world.MissingPrerequisites(6)){Check(design!=0,"owned prerequisite still missing");missing++;}Check(missing==5,"prerequisite not removed");
   world.SelectPlayer(1);missing=0;foreach(int design in world.MissingPrerequisites(6))missing++;Check(missing==6,"teammate ownership leaked into preview");
  }
  public static void StatusCombat() {
   var c=Scenario.SharedDefense();Factions.Apply(c);c.BuilderEnabled=false;var w=new World(c);w.SelectedDesign=2;Check(w.Build(16,14,out _),"slow tower");
   var e=w.Spawn(new WaveSpec{Health=1000},new V2(18,15));w.Step();Check(e.SlowRemaining>0&&e.SlowFraction==.3f,"slow missing");w.TowersFire=false;for(int i=0;i<70;i++)w.Step();Check(e.SlowRemaining==0,"slow never expires");
   w=new World(c,new MatchOptions{Factions=new[]{3,0,0,0}});w.SelectedDesign=17;w.Build(16,14,out _);
   for(int i=0;i<4;i++)w.Spawn(new WaveSpec{Health=1000},new V2(18+i*.5f,15));w.Step();int hit=0;foreach(var enemy in w.Enemies)if(enemy.Health<1000)hit++;Check(hit==3,"chain target count wrong");
  }
 }
}
