using System;
using FrostMaze.Simulation;
namespace FrostMaze.Tests
{
    public static class LaneCases
    {
        static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
        public static void AlwaysActive()
        {
            for(int players=1;players<=4;players++) {
                var w=new World(Scenario.SharedDefense(),new MatchOptions{PlayerCount=players});
                w.StartWave();w.Step();
                Check(w.Enemies.Count==4&&w.Pending==28,"player count changed lane population");
                for(int lane=0;lane<4;lane++)Check(w.Enemies.Exists(e=>e.Lane==lane),"missing lane");
            }
        }
        public static void QueueIndependence()
        {
            var w=new World(Scenario.SharedDefense());w.StartWave();w.Spawn(w.Config.Waves[0],w.LaneSpawn(0));
            w.Step();
            Check(w.Pending==29,"occupied lane delayed other spawners");
            Check(w.Enemies.FindAll(e=>e.Lane==0).Count==1,"spawn overlapped existing unit");
        }
        public static void Terrain()
        {
            var g=new MazeGrid(12,8);g.AddTerrain(new TerrainBlock{X=5,Y=0,Width=1,Height=8});
            Check(g.Build(5,2,new TowerSpec())==null,"built on terrain");
            Check(!g.Clear(new V2(4,4),new V2(7,4),.2f),"crossed ridge");
            var nav=new FlowNavigation(g);
            Check(nav.Anchor(nav.Get(new V2(10,4),.2f,true),new V2(2,4))<0,"siege field crosses permanent terrain");
        }
        public static void Routes()
        {
            var c=Scenario.SharedDefense();c.Economy=false;
            var w=new World(c);
            for(int lane=0;lane<4;lane++)w.Spawn(c.Waves[0],w.LaneSpawn(lane),lane);
            for(int i=0;i<2400&&w.Enemies.Count>0;i++) {
                w.Step();foreach(var e in w.Enemies)Check(w.Grid.Clear(e.Position,e.Position,e.Spec.Radius),"unit crossed terrain");
            }
            Check(w.Leaked==4,"all lanes did not converge to bottom exit");
        }
        public static void EconomyAndStarts()
        {
            for(int n=1;n<=4;n++) {
                var c=Scenario.SharedDefense();var w=new World(c,new MatchOptions{PlayerCount=n,StartingPositions=new[]{3,2,1,0}});
                int total=0;foreach(var p in w.Players)total+=p.Gold;
                Check(total==1200&&w.Gold==1200/n,"team budget changed");
                if(n>1)Check(w.Players[0].Position.X==c.BuilderStarts[3].X,"chosen start ignored");
                w.Grid.Build(3,37,new TowerSpec{Damage=100,Range=4});
                w.Spawn(new WaveSpec{Health=1},w.LaneSpawn(0));w.Step();
                total=0;int min=int.MaxValue,max=0;foreach(var p in w.Players){total+=p.Gold;min=Math.Min(min,p.Gold);max=Math.Max(max,p.Gold);}
                Check(total==1202&&max-min<=1,"reward lost or split unfairly");
            }
            bool rejected=false;try{new World(Scenario.SharedDefense(),new MatchOptions{PlayerCount=2,StartingPositions=new[]{1,1}});}catch(ArgumentException){rejected=true;}
            Check(rejected,"duplicate starts accepted");
        }
        public static void OwnershipAndBuilders()
        {
            var w=new World(Scenario.SharedDefense(),new MatchOptions{PlayerCount=2});
            w.OrderBuild(3,30,out _);w.SelectPlayer(1);w.OrderBuild(12,30,out _);
            for(int i=0;i<60;i++)w.Step();
            Check(w.Grid.Towers.Count==2&&w.Players[0].Gold==580&&w.Players[1].Gold==580,"inactive builder did not execute");
            Check(!w.Sell(3,30),"sold another player's tower");
            Check(w.TowerOwner(w.Grid.At(3,30).Id)==0&&w.SaleRefund(w.Grid.At(3,30).Id)==15,"foreign inspector changed owner or refund");
            w.SelectPlayer(0);Check(w.Sell(3,30)&&w.Gold==595,"owner refund failed");
        }
        public static void CompetingBuilderQueues()
        {
            var w=new World(Scenario.SharedDefense(),new MatchOptions{PlayerCount=4});
            for(int player=0;player<4;player++) {
                w.SelectPlayer(player);w.SelectedDesign=1;
                Check(w.OrderBuild(20,30,out _),"shared order rejected before construction");
                Check(w.OrderBuild(3+player*3,32,out _,true),"individual order rejected");
                Check(w.OrderBuild(3+player*3,33,out _,true),"last order rejected");
            }
            Check(w.Spawn(new WaveSpec{Speed=0,Health=10000},new V2(3.5f,32.5f))!=null,"enemy fixture missing");
            for(int tick=0;tick<600;tick++) {
                w.Step();Check(w.ActivePlayer==3,"background builders changed local control");
            }
            var shared=w.Grid.At(20,30);Check(shared!=null,"no builder completed contested cell");
            int winner=w.TowerOwner(shared.Id),total=0;
            Check(winner>=0&&winner<4,"contested tower has no owner");
            for(int player=0;player<4;player++) {
                w.SelectPlayer(player);Check(w.QueuedBuilds==0,"queue stalled behind invalid order");
                int expected=300-5*(player==0?1:2)-(winner==player?5:0);
                Check(w.Gold==expected,"competing or blocked order charged wrong wallet");total+=w.Gold;
                var last=w.Grid.At(3+player*3,33);
                Check(last!=null&&w.TowerOwner(last.Id)==player,"later owned order was lost");
                if(player!=winner)Check(!w.Sell(20,30),"losing builder sold another player's tower");
            }
            Check(w.Grid.At(3,32)==null&&w.Grid.Towers.Count==8&&total==1160,"shared-cell or enemy overlap changed team budget");
        }
        public static void Difficulty()
        {
            var c=Scenario.SharedDefense();
            foreach(var difficulty in new[]{Simulation.Difficulty.Relaxed,Simulation.Difficulty.Normal,Simulation.Difficulty.Hard}) {
                var w=new World(c,new MatchOptions{Difficulty=difficulty});w.StartWave();w.Step();
                float factor=difficulty==Simulation.Difficulty.Relaxed?.7f:difficulty==Simulation.Difficulty.Hard?1.4f:1;
                Check(Math.Abs(w.Enemies[0].Health-35*factor)<.01f&&w.Enemies.Count==4,"difficulty altered lanes or missed scaling");
            }
            Check(c.Waves[0].Health==35,"difficulty mutated source data");
        }
    }
}
