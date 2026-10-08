using System;
using FrostMaze.Simulation;
namespace FrostMaze.Tests
{
    public sealed class Case
    {
        public string Name; public Action Run; public Case(string name, Action run)
        {
            Name = name;
            Run = run;
        }
    }
    public static class SimulationCases
    {
        static void Check(bool value, string message)
        {
            if (!value)
                throw new Exception(message);
        }
        static bool Reach(MazeGrid grid, V2 a, V2 b, float radius = 0.2f, float step = 0.5f)
        {
            var nav = new FlowNavigation(grid, step);
            return nav.Anchor(nav.Get(b, radius), a) >= 0;
        }
        static TowerSpec Solid() => new TowerSpec { Fill = 1, Damage = 0 };
        static void Wall(MazeGrid grid, int x, int gap = -1)
        {
            for (int y = 0; y < grid.Height; y++)
                if (y != gap)
                    grid.Build(x, y, Solid());
        }
        static Scenario Config() => new Scenario { Width = 12, Height = 8, Spawn = new V2(1.5f, 4.5f), GroundRoute = new[] { new V2(10.5f, 4.5f) }, FlightRoute = new[] { new V2(5.5f, 1.5f), new V2(10.5f, 4.5f) }, Tower = new TowerSpec { Damage = 0 }, Waves = new[] { new WaveSpec { Count = 20, Health = 100 } } };
        static void Run(World w, int ticks)
        {
            for (int i = 0; i < ticks; i++)
                w.Step();
        }
        public static readonly Case[] All ={
            new Case("Tower spatial queries match full scans through removal and rebuild",SpatialCases.TowerQueriesMatchFullScan),
            new Case("Reference wall seams accept flush towers and exclude enemies",MapCases.WallSeams),
            new Case("Half-cell paid queues preserve position ownership and selection",MapCases.FractionalPaidOrders),
            new Case("Half-cell wall seal triggers siege and reopens after sale",MapCases.FractionalWallSiege),
            new Case("Paid reference mazes detour ground, preserve flight and reopen",MapCases.PaidReferenceMazes),
            new Case("Mixed-flight chain feedback preserves source and target",TowerCases.ChainFeedbackOrigins),
            new Case("Every armed roster design has valid targeting and air splash",TowerCases.EveryArmedDesignHasTargets),
            new Case("Chain volleys retain bounded ordered shot history",TowerCases.ShotHistoryBounded),
            new Case("Wave previews match spawns and show owned prerequisites",MapCases.WavePreviews),
            new Case("Robot champion progression and ownership",MapCases.RobotProgression),
            new Case("Wave planning respects targeting, factions and team ownership",TowerCases.WavePlanning),
            new Case("Reference map masks and spatial index",MapCases.Masks),
            new Case("Reference map lane traversal",MapCases.Routes),
            new Case("Faction selection enforces rosters",MapCases.FactionSelection),
            new Case("Slow expiry and chain target limits",MapCases.StatusCombat),
            new Case("Straight path",()=>{var g=new MazeGrid(12,8);Check(Reach(g,new V2(1.5f,4.5f),new V2(10.5f,4.5f)),"no straight route");}),
            new Case("Horizontal wall detour",()=>{var g=new MazeGrid(12,8);for(int x=0;x<10;x++)g.Build(x,4,Solid());Check(Reach(g,new V2(1.5f,1.5f),new V2(1.5f,6.5f)),"no detour");}),
            new Case("Vertical wall detour",()=>{var g=new MazeGrid(12,8);Wall(g,5,6);Check(Reach(g,new V2(1.5f,4.5f),new V2(10.5f,4.5f)),"no detour");}),
            new Case("Valid diagonal clearance",()=>{var g=new MazeGrid(4,4);g.Build(1,2,new TowerSpec{Fill=0.6f});g.Build(2,1,new TowerSpec{Fill=0.6f});Check(g.Clear(new V2(1.5f,1.5f),new V2(2.5f,2.5f),0.25f),"small disc cannot pass");}),
            new Case("Invalid diagonal clearance",()=>{var g=new MazeGrid(4,4);g.Build(1,2,new TowerSpec{Fill=0.6f});g.Build(2,1,new TowerSpec{Fill=0.6f});Check(!g.Clear(new V2(1.5f,1.5f),new V2(2.5f,2.5f),0.3f),"large disc clipped corners");}),
            new Case("Diagonal aperture changes global connectivity",()=>{var g=new MazeGrid(4,4);g.Build(0,2,Solid());g.Build(1,2,new TowerSpec{Fill=0.6f});g.Build(2,1,new TowerSpec{Fill=0.6f});g.Build(3,1,Solid());Check(Reach(g,new V2(0.5f,0.5f),new V2(3.5f,3.5f),0.25f,0.25f),"small radius disconnected");Check(!Reach(g,new V2(0.5f,0.5f),new V2(3.5f,3.5f),0.30f,0.25f),"large radius crossed aperture");}),
            new Case("Tower combat counts negative health as a kill",()=>{var c=Config();c.Tower.Damage=8;var w=new World(c);w.Grid.Build(2,3,c.Tower);w.Spawn(new WaveSpec{Health=7},c.Spawn);w.Step();Check(w.Killed==1&&w.Leaked==0,"kill mistaken for leak");}),
            new Case("Long zig-zag maze",()=>{var g=new MazeGrid(24,12);for(int x=4;x<22;x+=4)Wall(g,x,(x/4)%2==0?1:10);var nav=new FlowNavigation(g);var f=nav.Get(new V2(22.5f,6.5f),0.2f);int at=nav.Anchor(f,new V2(1.5f,6.5f));Check(at>=0&&f.Distance[at]>45,"maze shortcut or disconnected");}),
            new Case("Complete blockage",()=>{var g=new MazeGrid(12,8);Wall(g,5);Check(!Reach(g,new V2(1.5f,4.5f),new V2(10.5f,4.5f)),"wall crossed");}),
            new Case("Reopen after selling, invalidate cached field",()=>{var g=new MazeGrid(12,8);Wall(g,5);var nav=new FlowNavigation(g);var goal=new V2(10.5f,4.5f);Check(nav.Anchor(nav.Get(goal,0.2f),new V2(1.5f,4.5f))<0,"not blocked");g.Remove(g.At(5,4).Id);Check(nav.Anchor(nav.Get(goal,0.2f),new V2(1.5f,4.5f))>=0,"stale field");}),
            new Case("Reopen after destruction",()=>{var g=new MazeGrid(12,8);Wall(g,5);g.Damage(g.At(5,4).Id,101);Check(Reach(g,new V2(1.5f,4.5f),new V2(10.5f,4.5f)),"destroyed tower blocks");}),
            new Case("Enemy breaches and resumes",()=>{var w=new World(Config());Wall(w.Grid,5);w.Spawn(w.Config.Waves[0],w.Config.Spawn);bool attacked=false;for(int i=0;i<1800;i++){w.Step();foreach(var e in w.Enemies)if(e.BlockerId>0)attacked=true;}Check(attacked&&w.Grid.Towers.Count<8&&w.Leaked==1,"did not breach and finish");}),
            new Case("Congestion never triggers siege",()=>{var w=new World(Config());w.StartWave();bool blocked=false;for(int i=0;i<2400;i++){w.Step();foreach(var e in w.Enemies)blocked|=e.Blocked;}Check(!blocked&&w.Leaked==20,"congestion misclassified or stalled");}),
            new Case("Flight follows checkpoints over wall",()=>{var w=new World(Config());Wall(w.Grid,5);w.Spawn(new WaveSpec{Flying=true},w.Config.Spawn);Run(w,900);Check(w.Leaked==1&&w.Grid.Towers.Count==8,"flight blocked or attacked tower");}),
            new Case("Placement footprint and occupancy",()=>{var g=new MazeGrid(12,8);Check(g.Build(2,2,new TowerSpec{Width=2,Height=3})!=null,"footprint rejected");Check(g.Build(3,4,Solid())==null,"overlap accepted");Check(g.Build(11,7,new TowerSpec{Width=2})==null,"outside map");}),
            new Case("Reproducible fixed-step simulation",()=>{var a=new World(Config());var b=new World(Config());a.StartWave();b.StartWave();Run(a,170);Run(b,170);Check(a.Enemies.Count==b.Enemies.Count,"count differs");for(int i=0;i<a.Enemies.Count;i++)Check(V2.Distance(a.Enemies[i].Position,b.Enemies[i].Position)==0,"state differs");}),
            new Case("Shared fields reused",()=>{var g=new MazeGrid(12,8);var nav=new FlowNavigation(g);var goal=new V2(10.5f,4.5f);for(int i=0;i<100;i++)nav.Get(goal,0.2f);Check(nav.Rebuilds==1,"field rebuilt per enemy");}),
            new Case("Enemies traverse zig-zag without collision",()=>{var w=new World(Config());Wall(w.Grid,4,1);Wall(w.Grid,8,6);w.Config.Waves[0].Count=12;w.StartWave();for(int i=0;i<5000;i++){w.Step();foreach(var e in w.Enemies){Check(!e.Blocked,"open zig-zag classified blocked");Check(w.Grid.Clear(e.Position,e.Position,e.Spec.Radius),"enemy inside wall");}}Check(w.Leaked==12,"zig-zag stalled: "+w.Leaked+" finished");}),
            new Case("Dynamic blockage and sale during wave",()=>{var w=new World(Config());var e=w.Spawn(w.Config.Waves[0],w.Config.Spawn);Run(w,10);Wall(w.Grid,5);Run(w,5);Check(e.Blocked,"did not recognize new wall");w.Sell(5,4);Run(w,5);Check(!e.Blocked,"did not recognize sold opening");Run(w,600);Check(w.Leaked==1,"failed to finish reopened path");}),
            new Case("Ground checkpoints retain downstream routing",()=>{var c=Config();c.GroundRoute=new[]{new V2(5.5f,1.5f),new V2(10.5f,4.5f)};var w=new World(c);var e=w.Spawn(c.Waves[0],c.Spawn);bool passed=false;for(int i=0;i<1200;i++){w.Step();passed|=e.Checkpoint==1;}Check(passed&&w.Leaked==1,"downstream checkpoint skipped or stalled");}),
            new Case("Two hundred enemies complete a shared route",()=>{var w=new World(Config());var wave=new WaveSpec();for(int y=0;y<16;y++)for(int x=0;x<13;x++)w.Spawn(wave,new V2(0.5f+x*0.45f,0.3f+y*0.45f));Check(w.Enemies.Count>=200,"crowd setup too small");int count=w.Enemies.Count;Run(w,1800);Check(w.Leaked==count,"crowd stalled "+w.Leaked+"/"+count);Check(w.Navigation.Rebuilds==1,"crowd rebuilt shared field");}),
            new Case("No tunnelling at high speed",()=>{var w=new World(Config());Wall(w.Grid,5,1);var e=w.Spawn(new WaveSpec{Speed=60},w.Config.Spawn);for(int i=0;i<90&&w.Enemies.Count>0;i++){w.Step();Check(w.Grid.Clear(e.Position,e.Position,e.Spec.Radius),"inside tower");}})
            ,new Case("Economy transactions reject invalid purchases and refunds", CampaignCases.Transactions)
            ,new Case("Builder travel cancellation and order replacement", CampaignCases.BuilderOrders)
            ,new Case("Construction revalidates on arrival", CampaignCases.ArrivalRevalidation)
            ,new Case("Kill and wave rewards paid once with victory", CampaignCases.Rewards)
            ,new Case("Defeat freezes match and prevents new actions", CampaignCases.Defeat)
            ,new Case("Shared map traverses all defense areas", CampaignCases.SharedRoute)
            ,new Case("Complete ten-wave match using paid builder construction", CampaignCases.FullMatch)
            ,new Case("All four lanes active for every player count",LaneCases.AlwaysActive)
            ,new Case("Independent lane spawn queues",LaneCases.QueueIndependence)
            ,new Case("Permanent terrain blocks construction and siege navigation",LaneCases.Terrain)
            ,new Case("All upper lanes reach shared bottom exit",LaneCases.Routes)
            ,new Case("Fixed team economy and selected starting positions",LaneCases.EconomyAndStarts)
            ,new Case("Independent builders and owned sale refunds",LaneCases.OwnershipAndBuilders)
            ,new Case("Four builder queues skip contested and enemy-blocked cells independently",LaneCases.CompetingBuilderQueues)
            ,new Case("Difficulty scales enemies without disabling lanes",LaneCases.Difficulty)
            ,new Case("Build orders retain their tower design",TowerCases.RolesAndOrders)
            ,new Case("Tower upgrades preserve blueprints and refund investments",TowerCases.Upgrades)
            ,new Case("Cannon splash damages ground crowds but not air",TowerCases.GroundSplash)
            ,new Case("Barricades have no weapon",TowerCases.WallNoWeapon)
            ,new Case("Queued construction retains designs and cancels cleanly",TowerCases.QueuedConstruction)
            ,new Case("Blocked queued construction skips in click order without charging",TowerCases.BlockedQueueContinues)
            ,new Case("Twenty-wave escalation preserves opening and forecasts",MapCases.ExtendedCampaign)
            ,new Case("Robot faction strengths and paid reclamation",MapCases.RobotIdentity)
            ,new Case("Wave income excludes spending and preserves independent snapshots",WaveSummaryCases.IncomeIgnoresSpendingAndSnapshots)
            ,new Case("Wave results distinguish survived leaks from defeat",WaveSummaryCases.LeaksAndDefeat)
            ,new Case("Free-build wave results do not invent income",WaveSummaryCases.FreeBuildAwardsNoIncome)
            ,new Case("Captured dense Ironfold corner crowd clears without siege",MapCases.DenseIronfoldCorners)
        };
    }
}
