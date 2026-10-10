using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using FrostMaze.Simulation;
static class NavigationBenchmark {
 public static int Run() {
  foreach(var name in new[]{"Ironfold","Rimewatch"}) {
   var text=File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"Layouts",name+".txt"));
   var c=name=="Ironfold"?ReferenceMaps.Ironfold(text):ReferenceMaps.Rimewatch(text);
   var grid=new MazeGrid(c.Width,c.Height);foreach(var t in c.Terrain)grid.AddTerrain(t);
   var nav=new FlowNavigation(grid,c.NavigationStep,c.BreachCost);var edits=new List<Tower>();
   for(int y=2;y<c.Height-2&&grid.Towers.Count<160;y+=2)for(int x=2;x<c.Width-2&&grid.Towers.Count<160;x+=2) {
    var t=grid.Build(x,y,new TowerSpec{Damage=0});if(t!=null)edits.Add(t);
   }
   var goals=new List<V2>();foreach(var lane in c.Lanes)foreach(var p in lane.GroundRoute)if(!goals.Exists(q=>q.X==p.X&&q.Y==p.Y))goals.Add(p);
   float radius=c.Waves[0].Radius;
   Action rebuild=()=>{foreach(var goal in goals)nav.Get(goal,radius);};rebuild();
   var samples=new List<double>();long allocated=0;var watch=new Stopwatch();
   for(int i=0;i<8;i++) {
    var t=edits[i];grid.Remove(t.Id);grid.Build(t.CellX,t.CellY,t.Spec);
    long before=GC.GetAllocatedBytesForCurrentThread();watch.Restart();rebuild();watch.Stop();allocated+=GC.GetAllocatedBytesForCurrentThread()-before;samples.Add(watch.Elapsed.TotalMilliseconds);
   }
   samples.Sort();Console.WriteLine(name+" towers="+grid.Towers.Count+" fields="+goals.Count+" radius="+radius+" medianMs="+samples[4].ToString("F2")+" maxMs="+samples[7].ToString("F2")+" bytesPerEdit="+allocated/8);
  }
  return 0;
 }
}
