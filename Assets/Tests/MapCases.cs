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
