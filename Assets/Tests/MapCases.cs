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
  public static void WallSeams() {
   foreach(bool iron in new[]{false,true}) {
    var w=new World(Load(iron));float step=w.PlacementStep;int seams=0,halfSeams=0;var radius=w.Config.Waves[0].Radius;
    for(float y=0;y<w.Config.Height;y+=step)for(float x=0;x<w.Config.Width;x+=step) {
     if(!w.CanBuild(x,y,out _))continue;
     foreach(var terrain in w.Grid.Terrain) {
      float lo=Math.Max(y,terrain.Y),hi=Math.Min(y+1,terrain.Y+terrain.Height);
      V2 seam=default;bool touching=false;
      if(hi-lo>.01f&&terrain.X+terrain.Width==x){seam=new V2(x+.035f,(lo+hi)*.5f);touching=true;}
      if(hi-lo>.01f&&terrain.X==x+1){seam=new V2(x+1-.035f,(lo+hi)*.5f);touching=true;}
      lo=Math.Max(x,terrain.X);hi=Math.Min(x+1,terrain.X+terrain.Width);
      if(hi-lo>.01f&&terrain.Y+terrain.Height==y){seam=new V2((lo+hi)*.5f,y+.035f);touching=true;}
      if(hi-lo>.01f&&terrain.Y==y+1){seam=new V2((lo+hi)*.5f,y+1-.035f);touching=true;}
      if(!touching)continue;
      var t=w.Grid.Build(x,y,w.BuildSpec.Copy());Check(t!=null,"flush placement rejected");
      Check(!w.Grid.Clear(seam,seam,radius),"enemy fits between tower and source wall");
      Check(!w.Grid.TerrainOverlaps(x,y,1,1),"flush tower overlaps terrain");
      w.Grid.Remove(t.Id);seams++;if(x%1!=0||y%1!=0)halfSeams++;
     }
    }
    Check(seams>100,"insufficient real-map wall/corner coverage");Check(!iron||halfSeams>100,"missing Ironfold half-cell edge coverage");
   }
  }
  public static void FractionalPaidOrders() {
   var w=new World(Load(true));float bx=-1,by=-1,cx=-1,cy=-1;
   for(float y=1;y<63&&bx<0;y+=.5f)for(float x=1;x<63&&bx<0;x+=.5f)
    if((x%1!=0||y%1!=0)&&w.CanBuild(x,y,out _)&&w.CanBuild(x+2,y,out _)){bx=x;by=y;cx=x+2;cy=y;}
   Check(bx>=0,"no fractional fixture");int gold=w.Gold,cost=w.BuildCost;
   Check(w.OrderBuild(bx,by,out _),"paid fractional order");Check(!w.OrderBuild(bx,by,out _,true),"duplicate order accepted");
   Check(w.OrderBuild(cx,cy,out _,true),"queued fractional order");
   for(int i=0;i<1000&&w.QueuedBuilds>0;i++)w.Step();
   var first=w.Grid.At(bx+.2f,by+.2f);var second=w.Grid.At(cx+.2f,cy+.2f);
   Check(first!=null&&first.CellX==bx&&first.CellY==by&&second!=null&&second.CellX==cx&&second.CellY==cy,"builder truncated half cells");
   Check(w.Gold==gold-2*cost,"fractional purchase wallet");
   Check(!w.CanBuild(bx+.5f,by,out _),"partial footprint overlap accepted");Check(!w.CanBuild(bx+.25f,by,out _),"off-grid placement accepted");
   Check(w.Upgrade(first.Id,out _),"fractional upgrade");Check(w.Sell(bx+.2f,by+.2f),"fractional point selection/sale");
   Check(w.Grid.At(cx+.2f,cy+.2f)==second,"sale removed neighbor");
   var snap=w.SnapBuildOrigin(new V2(12.8f,14.6f));Check(snap.X==12.5f&&snap.Y==14.5f,"cursor snapping");
  }
  public static void FractionalWallSiege() {
   var c=new Scenario{Width=12,Height=8,Economy=true,BuilderEnabled=true,LayoutRows=new[]{"."},LayoutCellSize=.5f,
    Spawn=new V2(1.5f,3),GroundRoute=new[]{new V2(10.5f,3)},FlightRoute=new[]{new V2(10.5f,3)},
    Tower=new TowerSpec{Damage=0,Health=10000},Waves=new[]{new WaveSpec{Count=1,Health=1000}},
    Terrain=new[]{new TerrainBlock{X=0,Y=0,Width=12,Height=2.5f},new TerrainBlock{X=0,Y=3.5f,Width=12,Height=4.5f}}};
   var w=new World(c);Check(w.OrderBuild(5,2.5f,out _),"flush paid seal");for(int i=0;i<200;i++)w.Step();
   var tower=w.Grid.At(5,2.5f);Check(tower!=null,"seal not built");Check(w.StartWave(),"start siege fixture");bool siege=false;
   for(int i=0;i<400;i++){w.Step();foreach(var enemy in w.Enemies){siege|=enemy.Blocked;Check(enemy.Position.X<6,"enemy slipped through flush seal");Check(w.Grid.Clear(enemy.Position,enemy.Position,enemy.Spec.Radius),"enemy penetrated seal");}}
   Check(siege&&tower.Health<10000&&w.Leaked==0,"sealed route did not trigger siege");
   Check(w.Sell(5,2.5f),"sell seal");for(int i=0;i<1000&&w.Leaked==0;i++)w.Step();Check(w.Leaked==1,"sold seal did not reopen route");
  }
  public static void ExtendedCampaign() {
   foreach(bool iron in new[]{false,true}) {
    var c=Load(iron);Check(c.Waves.Length==20,"campaign length");
    var opening=Scenario.SharedDefense();
    for(int i=0;i<20;i++) {
     Check(c.Waves[i].Flying==((i+1)%5==0),"air warning cadence");
     if(i<10)Check(c.Waves[i].Health==opening.Waves[i].Health&&c.Waves[i].Count==opening.Waves[i].Count,"opening changed");
     var w=new World(c,new MatchOptions{Difficulty=Difficulty.Hard});var preview=w.PreviewWave(i);
     Check(Math.Abs(preview.Health-c.Waves[i].Health*1.4f)<.001f,"late difficulty preview");
    }
    Check(c.Waves[11].Speed>c.Waves[10].Speed&&c.Waves[12].SpawnInterval<c.Waves[10].SpawnInterval,"rush/swarm variety");
    Check(c.Waves[18].Damage>c.Waves[10].Damage&&c.Waves[18].Health>c.Waves[10].Health,"siege escalation");
    Check(c.StartingGold==(iron?2200:240)&&c.Waves[0].ClearGold==56,"economy changed");
   }
  }
  public static void RobotIdentity() {
   var c=Load(true);
   Check(c.Catalog[8].Spec.Interval>c.Catalog[1].Spec.Interval,"pulse fire rate");
   Check(c.Catalog[9].Spec.SplashRadius>c.Catalog[2].Spec.SplashRadius,"blast area");
   Check(c.Catalog[15].Spec.ChainTargets==3,"prism chain");
   Check(c.Catalog[25].Spec.Range>c.Catalog[4].Spec.Range,"horizon air reach");
   Check(c.Catalog[29].Spec.SlowFraction==.4f&&c.Catalog[29].Spec.SlowDuration==3,"gravity control");
   var w=new World(c,new MatchOptions{Factions=new[]{5,0,0,0}});w.SelectedDesign=36;
   int bx=-1,by=-1;for(int y=0;y<c.Height&&bx<0;y++)for(int x=0;x<c.Width&&bx<0;x++)if(w.CanBuild(x,y,out _)){bx=x;by=y;}
   int gold=w.Gold;Check(bx>=0&&w.OrderBuild(bx,by,out _),"scrap order");for(int i=0;i<500;i++)w.Step();
   Check(w.Gold==gold-30&&w.Sell(bx,by)&&w.Gold==gold-3,"scrap paid refund");
   foreach(var d in c.Catalog)Check(d.Spec.TargetsAir||d.Spec.TargetsGround,"unarmed robot");
  }
  public static void DenseIronfoldCorners() {
   // Captured live positions from the wave-13 Prism/Horizon paid campaign stall.
   // Recreate geometry and surviving units, with weapons disabled to isolate movement.
   var c=Load(true);c.Economy=false;var w=new World(c);w.TowersFire=false;
   w.Grid.Build(31,5,c.Catalog[14].Spec.Copy());
   w.Grid.Build(33,5,c.Catalog[15].Spec.Copy());
   w.Grid.Build(33,3,c.Catalog[16].Spec.Copy());
   w.Grid.Build(33,7,c.Catalog[17].Spec.Copy());
   w.Grid.Build(29,7,c.Catalog[18].Spec.Copy());
   w.Grid.Build(31,3,c.Catalog[19].Spec.Copy());
   w.Grid.Build(35,5,c.Catalog[21].Spec.Copy());
   w.Grid.Build(35,7,c.Catalog[22].Spec.Copy());
   w.Grid.Build(29,9,c.Catalog[23].Spec.Copy());
   w.Grid.Build(35,9,c.Catalog[24].Spec.Copy());
   w.Grid.Build(27,7,c.Catalog[25].Spec.Copy());
   w.Grid.Build(27,5,c.Catalog[26].Spec.Copy());
   w.Grid.Build(33,1,c.Catalog[20].Spec.Copy());
   w.Grid.Build(27,9,c.Catalog[27].Spec.Copy());
   w.Enemies.Add(new Enemy{Id=923,Position=new V2(37.801567f,15.970833f),Health=97.5f,Spec=c.Waves[12],Lane=2,Checkpoint=2});
   w.Enemies.Add(new Enemy{Id=927,Position=new V2(37.799763f,16.373245f),Health=97.5f,Spec=c.Waves[12],Lane=2,Checkpoint=2});
   w.Enemies.Add(new Enemy{Id=931,Position=new V2(37.500004f,15.704073f),Health=97.5f,Spec=c.Waves[12],Lane=2,Checkpoint=2});
   w.Enemies.Add(new Enemy{Id=934,Position=new V2(27.178057f,15.886239f),Health=97.5f,Spec=c.Waves[12],Lane=1,Checkpoint=2});
   w.Enemies.Add(new Enemy{Id=935,Position=new V2(37.420155f,16.097921f),Health=97.5f,Spec=c.Waves[12],Lane=2,Checkpoint=2});
   w.Enemies.Add(new Enemy{Id=938,Position=new V2(27.449364f,16.179422f),Health=97.5f,Spec=c.Waves[12],Lane=1,Checkpoint=2});
   w.Enemies.Add(new Enemy{Id=939,Position=new V2(37.238144f,16.454094f),Health=97.5f,Spec=c.Waves[12],Lane=2,Checkpoint=2});
   w.Enemies.Add(new Enemy{Id=942,Position=new V2(27.531744f,15.701534f),Health=97.5f,Spec=c.Waves[12],Lane=1,Checkpoint=2});
   w.Enemies.Add(new Enemy{Id=943,Position=new V2(36.988308f,16.767675f),Health=97.5f,Spec=c.Waves[12],Lane=2,Checkpoint=2});
   w.Enemies.Add(new Enemy{Id=946,Position=new V2(27.724037f,16.469297f),Health=97.5f,Spec=c.Waves[12],Lane=1,Checkpoint=2});
   w.Enemies.Add(new Enemy{Id=947,Position=new V2(36.700714f,17.048931f),Health=97.5f,Spec=c.Waves[12],Lane=2,Checkpoint=2});
   w.Enemies.Add(new Enemy{Id=950,Position=new V2(27.929472f,16.812792f),Health=97.5f,Spec=c.Waves[12],Lane=1,Checkpoint=2});
   w.Enemies.Add(new Enemy{Id=951,Position=new V2(36.483795f,17.383898f),Health=97.5f,Spec=c.Waves[12],Lane=2,Checkpoint=2});
   w.Enemies.Add(new Enemy{Id=954,Position=new V2(28.219242f,17.091143f),Health=97.5f,Spec=c.Waves[12],Lane=1,Checkpoint=2});
   w.Enemies.Add(new Enemy{Id=955,Position=new V2(36.24832f,17.70655f),Health=97.5f,Spec=c.Waves[12],Lane=2,Checkpoint=2});
   w.Enemies.Add(new Enemy{Id=958,Position=new V2(28.49379f,17.381811f),Health=97.5f,Spec=c.Waves[12],Lane=1,Checkpoint=2});
   w.Enemies.Add(new Enemy{Id=959,Position=new V2(35.953358f,17.97664f),Health=97.5f,Spec=c.Waves[12],Lane=2,Checkpoint=2});
   w.Enemies.Add(new Enemy{Id=962,Position=new V2(28.785292f,17.654657f),Health=97.5f,Spec=c.Waves[12],Lane=1,Checkpoint=2});
   w.Enemies.Add(new Enemy{Id=963,Position=new V2(35.73161f,18.31161f),Health=97.5f,Spec=c.Waves[12],Lane=2,Checkpoint=2});
   w.Enemies.Add(new Enemy{Id=966,Position=new V2(29.00551f,17.988424f),Health=97.5f,Spec=c.Waves[12],Lane=1,Checkpoint=2});
   w.Enemies.Add(new Enemy{Id=967,Position=new V2(35.496178f,18.63401f),Health=97.5f,Spec=c.Waves[12],Lane=2,Checkpoint=2});
   w.Enemies.Add(new Enemy{Id=970,Position=new V2(29.245441f,18.30807f),Health=97.5f,Spec=c.Waves[12],Lane=1,Checkpoint=2});
   w.Enemies.Add(new Enemy{Id=971,Position=new V2(35.207737f,18.912344f),Health=97.5f,Spec=c.Waves[12],Lane=2,Checkpoint=2});
   w.Enemies.Add(new Enemy{Id=974,Position=new V2(29.533485f,18.58828f),Health=97.5f,Spec=c.Waves[12],Lane=1,Checkpoint=2});
   w.Enemies.Add(new Enemy{Id=975,Position=new V2(34.978138f,19.23926f),Health=97.5f,Spec=c.Waves[12],Lane=2,Checkpoint=2});
   w.Enemies.Add(new Enemy{Id=978,Position=new V2(29.756245f,18.923216f),Health=97.5f,Spec=c.Waves[12],Lane=1,Checkpoint=2});
   w.Enemies.Add(new Enemy{Id=979,Position=new V2(34.760956f,19.576443f),Health=97.5f,Spec=c.Waves[12],Lane=2,Checkpoint=2});
   w.Enemies.Add(new Enemy{Id=982,Position=new V2(29.991634f,19.246323f),Health=97.5f,Spec=c.Waves[12],Lane=1,Checkpoint=2});
   w.Enemies.Add(new Enemy{Id=983,Position=new V2(34.596424f,19.93996f),Health=97.5f,Spec=c.Waves[12],Lane=2,Checkpoint=2});
   w.Enemies.Add(new Enemy{Id=986,Position=new V2(30.264519f,19.5385f),Health=97.5f,Spec=c.Waves[12],Lane=1,Checkpoint=2});
   w.Enemies.Add(new Enemy{Id=987,Position=new V2(34.413895f,20.294973f),Health=97.5f,Spec=c.Waves[12],Lane=2,Checkpoint=2});
   w.Enemies.Add(new Enemy{Id=990,Position=new V2(30.424482f,19.905457f),Health=97.5f,Spec=c.Waves[12],Lane=1,Checkpoint=2});
   w.Enemies.Add(new Enemy{Id=991,Position=new V2(34.30419f,20.679222f),Health=97.5f,Spec=c.Waves[12],Lane=2,Checkpoint=2});
   w.Enemies.Add(new Enemy{Id=994,Position=new V2(30.53649f,20.288736f),Health=97.5f,Spec=c.Waves[12],Lane=1,Checkpoint=2});
   w.Enemies.Add(new Enemy{Id=995,Position=new V2(34.237106f,21.07262f),Health=97.5f,Spec=c.Waves[12],Lane=2,Checkpoint=2});
   w.Enemies.Add(new Enemy{Id=998,Position=new V2(30.672506f,20.66388f),Health=97.5f,Spec=c.Waves[12],Lane=1,Checkpoint=2});
   w.Enemies.Add(new Enemy{Id=999,Position=new V2(34.045723f,21.424053f),Health=97.5f,Spec=c.Waves[12],Lane=2,Checkpoint=2});
   w.Enemies.Add(new Enemy{Id=1002,Position=new V2(30.818369f,21.03633f),Health=97.5f,Spec=c.Waves[12],Lane=1,Checkpoint=2});
   w.Enemies.Add(new Enemy{Id=1003,Position=new V2(33.93092f,21.806938f),Health=97.5f,Spec=c.Waves[12],Lane=2,Checkpoint=2});
   w.Enemies.Add(new Enemy{Id=1006,Position=new V2(30.933199f,21.418486f),Health=97.5f,Spec=c.Waves[12],Lane=1,Checkpoint=2});
   w.Enemies.Add(new Enemy{Id=1007,Position=new V2(33.864326f,22.200838f),Health=97.5f,Spec=c.Waves[12],Lane=2,Checkpoint=2});
   w.Enemies.Add(new Enemy{Id=1010,Position=new V2(31.041851f,21.80292f),Health=97.5f,Spec=c.Waves[12],Lane=1,Checkpoint=2});
   w.Enemies.Add(new Enemy{Id=1011,Position=new V2(33.67389f,22.552397f),Health=97.5f,Spec=c.Waves[12],Lane=2,Checkpoint=2});
   w.Enemies.Add(new Enemy{Id=1014,Position=new V2(31.209562f,22.165289f),Health=97.5f,Spec=c.Waves[12],Lane=1,Checkpoint=2});
   w.Enemies.Add(new Enemy{Id=1015,Position=new V2(33.558006f,22.934484f),Health=97.5f,Spec=c.Waves[12],Lane=2,Checkpoint=2});
   w.Enemies.Add(new Enemy{Id=1018,Position=new V2(31.323503f,22.548117f),Health=97.5f,Spec=c.Waves[12],Lane=1,Checkpoint=2});
   w.Enemies.Add(new Enemy{Id=1019,Position=new V2(33.45167f,23.319107f),Health=97.5f,Spec=c.Waves[12],Lane=2,Checkpoint=2});
   w.Enemies.Add(new Enemy{Id=1022,Position=new V2(31.38664f,22.94219f),Health=97.5f,Spec=c.Waves[12],Lane=1,Checkpoint=2});
   w.Enemies.Add(new Enemy{Id=1023,Position=new V2(33.282288f,23.680544f),Health=97.5f,Spec=c.Waves[12],Lane=2,Checkpoint=2});
   w.Enemies.Add(new Enemy{Id=1026,Position=new V2(31.577343f,23.293692f),Health=97.5f,Spec=c.Waves[12],Lane=1,Checkpoint=2});
   w.Enemies.Add(new Enemy{Id=1027,Position=new V2(33.168724f,24.063345f),Health=97.5f,Spec=c.Waves[12],Lane=2,Checkpoint=2});
   w.Enemies.Add(new Enemy{Id=1030,Position=new V2(31.693464f,23.675875f),Health=97.5f,Spec=c.Waves[12],Lane=1,Checkpoint=2});
   w.Enemies.Add(new Enemy{Id=1031,Position=new V2(33.089867f,24.455084f),Health=97.5f,Spec=c.Waves[12],Lane=2,Checkpoint=2});
   w.Enemies.Add(new Enemy{Id=1034,Position=new V2(31.800274f,24.060385f),Health=97.5f,Spec=c.Waves[12],Lane=1,Checkpoint=2});
   w.Enemies.Add(new Enemy{Id=1035,Position=new V2(32.89557f,24.80367f),Health=97.5f,Spec=c.Waves[12],Lane=2,Checkpoint=2});
   w.Enemies.Add(new Enemy{Id=1038,Position=new V2(31.972261f,24.421062f),Health=97.5f,Spec=c.Waves[12],Lane=1,Checkpoint=2});
   w.Enemies.Add(new Enemy{Id=1039,Position=new V2(32.695652f,25.151785f),Health=97.5f,Spec=c.Waves[12],Lane=2,Checkpoint=1});
   w.Enemies.Add(new Enemy{Id=1042,Position=new V2(32.04574f,24.813583f),Health=97.5f,Spec=c.Waves[12],Lane=1,Checkpoint=2});
   w.Enemies.Add(new Enemy{Id=1043,Position=new V2(33.05912f,25.31658f),Health=97.5f,Spec=c.Waves[12],Lane=2,Checkpoint=1});
   w.Enemies.Add(new Enemy{Id=1046,Position=new V2(32.194588f,25.183794f),Health=97.5f,Spec=c.Waves[12],Lane=1,Checkpoint=1});
   w.Enemies.Add(new Enemy{Id=1047,Position=new V2(32.62454f,25.544432f),Health=97.5f,Spec=c.Waves[12],Lane=2,Checkpoint=1});
   for(int tick=0;tick<3600&&w.Enemies.Count>0;tick++) {
    w.Step();foreach(var e in w.Enemies) {
     Check(!e.Blocked,"crowding triggered siege");Check(w.Grid.Clear(e.Position,e.Position,e.Spec.Radius),"crowd crossed terrain/tower");
     foreach(var other in w.Enemies)if(other.Id>e.Id)Check(V2.Distance(e.Position,other.Position)>=e.Spec.Radius+other.Spec.Radius-.002f,"crowd overlap");
    }
   }
   Check(w.Enemies.Count==0&&w.Leaked==61,"dense corner crowd stalled: "+w.Enemies.Count+(w.Enemies.Count>0?" first "+w.Enemies[0].Position:""));
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
  // Shared six-cell exit neck: two alternating arms, with one-cell openings.
  // Test/planner fixture only; never a terrain change or a player building restriction.
  public static int[,] SharedExitMazeCells() {
   var cells=new int[10,2];int at=0;
   for(int row=0;row<2;row++)for(int x=28;x<=33;x++) {
    if(x==(row==0?33:28))continue;
    cells[at,0]=x;cells[at++,1]=12-row*2;
   }
   return cells;
  }
  public static void PaidSharedExitMaze() {
   var c=Load(false);var baseline=Traverse(new World(c));
   foreach(int first in new[]{0,3}) {
    var w=new World(c,new MatchOptions{PlayerCount=3,Factions=new[]{first,(first+1)%4,(first+2)%4,0},StartingPositions=new[]{7,0,4,1}});
    var cells=SharedExitMazeCells();var spent=new int[3];var refunds=new int[3];
    for(int cell=0;cell<cells.GetLength(0);cell++) {
     int owner=cell%3;w.SelectPlayer(owner);w.SelectedDesign=MazeDesign(w);int cost=w.BuildCost;
     Check(w.OrderBuild(cells[cell,0],cells[cell,1],out string reason),"shared exit maze rejected: "+reason);
     int ticks=0;while(w.HasBuildOrder&&ticks++<1000)w.Step();
     var tower=w.Grid.At(cells[cell,0],cells[cell,1]);
     Check(ticks>0&&!w.HasBuildOrder&&tower!=null&&w.TowerOwner(tower.Id)==owner,"shared maze paid travel/owner");
     spent[owner]+=cost;
     for(int p=0;p<3;p++)Check(w.Players[p].Gold==c.StartingGold/3-spent[p],"shared maze charged wrong wallet");
    }
    var detour=Traverse(w);
    for(int lane=0;lane<w.LaneCount;lane++) {
     Check(detour[lane*2]>baseline[lane*2]+30,"shared maze failed to delay lane "+lane);
     Check(detour[lane*2+1]==baseline[lane*2+1],"shared maze changed flyer route");
    }
    foreach(var tower in w.Grid.Towers.ToArray()) {
     int owner=w.TowerOwner(tower.Id);w.SelectPlayer((owner+1)%3);
     Check(!w.Sell(tower.CellX,tower.CellY),"teammate removed shared maze piece");
     w.SelectPlayer(owner);refunds[owner]+=w.SaleRefund(tower.Id);
     Check(w.Sell(tower.CellX,tower.CellY),"shared maze owner cannot reopen path");
    }
    for(int p=0;p<3;p++)Check(w.Players[p].Gold==c.StartingGold/3-spent[p]+refunds[p],"shared maze refund went to wrong wallet");
    var reopened=Traverse(w);for(int i=0;i<baseline.Length;i++)Check(reopened[i]==baseline[i],"shared maze sale left stale navigation");
   }
  }
  public static int MazeDesign(World w) {
   int best=w.Config.Factions[w.Players[w.ActivePlayer].Faction].Designs[0];
   foreach(int design in w.Config.Factions[w.Players[w.ActivePlayer].Faction].Designs)
    if(w.Config.Catalog[design].Cost<w.Config.Catalog[best].Cost)best=design;
   return best;
  }
  public static void OccupiedIntermediateCheckpoints() {
   foreach(bool iron in new[]{false,true}) {
    var c=Load(iron);var baseline=Traverse(new World(c));var w=new World(c);w.SelectedDesign=MazeDesign(w);
    int purchased=0,cost=w.BuildCost;
    for(int lane=0;lane<w.LaneCount;lane++) {
     var route=w.LaneRoute(lane,false);
     for(int i=0;i<route.Length-1;i++) {
      var point=route[i];if(w.Grid.At(point.X,point.Y)!=null)continue;
      bool built=false;float step=w.PlacementStep;
      for(float y=(float)Math.Floor(point.Y/step)*step;y>point.Y-1&&!built;y-=step)
       for(float x=(float)Math.Floor(point.X/step)*step;x>point.X-1&&!built;x-=step) {
        if(!w.CanBuild(x,y,out _))continue;
        Check(w.OrderBuild(x,y,out _),"checkpoint paid order rejected");
        for(int tick=0;tick<1000&&w.HasBuildOrder;tick++)w.Step();
        Check(w.Grid.At(point.X,point.Y)!=null,"checkpoint construction failed");purchased++;built=true;
       }
     }
     var exit=route[route.Length-1];var snapped=w.SnapBuildOrigin(exit);
     Check(!w.CanBuild(snapped.X,snapped.Y,out string reason)&&reason.Contains("exit"),"exit lost protection");
    }
    Check(purchased>=6,c.Name+" lacks checkpoint-placement coverage: "+purchased);
    Check(w.Gold==c.StartingGold-purchased*cost,"checkpoint builds were not paid");
    // Real lanes must pass occupied hints without false siege, clipping or skipping flight rules.
    w.TowersFire=false;var units=new Enemy[w.LaneCount];
    for(int lane=0;lane<w.LaneCount;lane++)units[lane]=w.Spawn(new WaveSpec(),w.LaneSpawn(lane),lane);
    for(int tick=0;tick<9000&&w.Enemies.Count>0;tick++) {
     w.Step();foreach(var e in w.Enemies)Check(!e.Blocked&&w.Grid.Clear(e.Position,e.Position,e.Spec.Radius),c.Name+" occupied hint caused siege/clipping");
    }
    Check(w.Leaked==w.LaneCount,c.Name+" occupied hint stalled");
    foreach(var e in units)Check(e.Checkpoint==w.LaneRoute(e.Lane,false).Length,"route hints skipped");
    foreach(var tower in new System.Collections.Generic.List<Tower>(w.Grid.Towers))Check(w.Sell(tower.CellX,tower.CellY),"checkpoint tower sale failed");
    var reopened=Traverse(w);for(int i=0;i<baseline.Length;i++)Check(reopened[i]==baseline[i],"sold checkpoint did not restore exact route");
   }
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
   var rime=Load(false);var exit=rime.GroundRoute[rime.GroundRoute.Length-1];
   int row=rime.LayoutRows.Length-1-(int)exit.Y,first=(int)exit.X,last=first;
   while(first>0&&rime.WalkableSymbols.IndexOf(rime.LayoutRows[row][first-1])>=0)first--;
   while(last+1<rime.LayoutRows[row].Length&&rime.WalkableSymbols.IndexOf(rime.LayoutRows[row][last+1])>=0)last++;
   Check(exit.X==(first+last+1)*.5f,"exit target is not centered within its actual corridor");
   foreach(var lane in rime.Lanes)foreach(var route in new[]{lane.GroundRoute,lane.FlightRoute})Check(V2.Distance(route[route.Length-1],exit)<.001f,"lane exit disagrees with centered marker");

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
  public static void PaidChampionQueueRecovery() {
   // Late-game queue regression on real terrain with explicit resource fixtures.
   // Campaign sweeps separately verify resources earned through normal play.
   foreach(bool destroyed in new[]{false,true})for(int faction=0;faction<8;faction++) {
    var c=Load(true);var w=new World(c,new MatchOptions{Factions=new[]{faction,0,0,0}});
    c.StartingGold=8000;w.Players[0].Gold=8000;w.Players[0].Wood=4;
    var cells=MazeCells(true);int spent=0,refund=0;string context=c.Factions[faction].Name+(destroyed?" destroyed":" sold");
    void Purchase(int design,int cell) {
     w.SelectedDesign=design;int before=w.Gold,cost=w.BuildCost;
     Check(w.OrderBuild(cells[cell,0],cells[cell,1],out string reason),context+": "+reason);
     for(int tick=0;tick<1000&&w.HasBuildOrder;tick++)w.Step();
     var built=w.Grid.At(cells[cell,0],cells[cell,1]);
     Check(!w.HasBuildOrder&&built!=null&&built.Design==design,context+": paid order failed");
     Check(w.Gold==before-cost,context+": incorrect purchase debit");spent+=cost;
    }
    int first=faction*7,champion=first+6;
    for(int design=0;design<6;design++)Purchase(first+design,design);
    Purchase(champion,6);var standing=w.Grid.At(cells[6,0],cells[6,1]);
    int beforeQueue=w.Gold;
    Check(w.OrderBuild(cells[12,0],cells[12,1],out _),context+": second champion order rejected");
    w.SelectedDesign=first;
    Check(w.OrderBuild(cells[13,0],cells[13,1],out _,true),context+": recovery order rejected");
    Check(w.Gold==beforeQueue&&w.QueuedBuilds==2,context+": queue reserved money");
    var prerequisite=w.Grid.At(cells[0,0],cells[0,1]);
    if(destroyed) {
     // Inject damage only to trigger the same removal path as siege; no claim of a played siege battle.
     w.Grid.Damage(prerequisite.Id,prerequisite.Health);
     Check(!w.Sell(cells[0,0],cells[0,1])&&w.Gold==beforeQueue,context+": destroyed prerequisite refunded");
    } else {
     refund=w.SaleRefund(prerequisite.Id);
     Check(w.Sell(cells[0,0],cells[0,1])&&w.Gold==beforeQueue+refund,context+": wrong sale refund");
    }
    Check(!w.RequirementsMet(champion)&&w.Grid.Find(standing.Id)==standing,context+": existing champion removed or unlock stale");
    bool skipped=false;
    for(int tick=0;tick<1000&&w.QueuedBuilds>0;tick++) {
     w.Step();skipped|=w.BuilderNotice.StartsWith("Skipped order:");
     Check(w.Grid.At(cells[12,0],cells[12,1])==null,context+": queued champion bypassed lost prerequisite");
    }
    spent+=c.Catalog[first].Cost;
    Check(skipped&&w.QueuedBuilds==0&&w.Gold==c.StartingGold-spent+refund,context+": skipped order charged or recovery stalled");
    Check(w.Grid.At(cells[13,0],cells[13,1])?.Design==first&&w.SelectedDesign==first,context+": later design/toolbar changed");
    Check(w.RequirementsMet(champion),context+": replacement did not restore unlock");
    for(int tick=0;tick<30;tick++)w.Step();
    Check(w.Grid.At(cells[12,0],cells[12,1])==null,context+": skipped champion retried automatically");
    Purchase(champion,12);
    Check(w.Grid.Find(standing.Id)==standing&&w.Grid.Towers.Count==8,context+": champion retention/rebuild count");
    Check(w.Gold==c.StartingGold-spent+refund&&w.Gold>=0,context+": final paid ledger mismatch");
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
