using System;
using FrostMaze.Simulation;
namespace FrostMaze.Tests
{
    public static class CampaignCases
    {
        static void Check(bool ok, string message) { if (!ok) throw new Exception(message); }
        static void Run(World w, int ticks) { for (int i=0;i<ticks;i++) w.Step(); }
        static World Small()
        {
            return new World(new Scenario { Economy=true, BuilderEnabled=true, StartingGold=40,
                Width=12, Height=8, Spawn=new V2(1.5f,4.5f),
                GroundRoute=new[]{new V2(10.5f,4.5f)}, FlightRoute=new[]{new V2(10.5f,4.5f)},
                Waves=new[]{new WaveSpec {Count=1, Health=7}} });
        }
        public static void Transactions()
        {
            var w=Small();
            Check(!w.Build(1,4,out _) && w.Gold==40,"spawn placement charged");
            Check(w.Build(2,3,out _) && w.Gold==20,"valid purchase failed");
            Check(!w.Build(2,3,out _) && w.Gold==20,"overlap charged");
            Check(w.Build(3,3,out _) && w.Gold==0,"second purchase failed");
            Check(!w.Build(2,2,out _) && w.Gold==0,"overspent");
            Check(w.Sell(2,3) && w.Gold==15,"refund wrong");
            Check(!w.Sell(2,3) && w.Gold==15,"duplicate refund");
            w.Grid.Damage(w.Grid.At(3,3).Id,1000);
            Check(!w.Sell(3,3) && w.Gold==15,"destroyed tower refunded");
        }
        public static void BuilderOrders()
        {
            var w=Small();
            Check(!w.Build(8,2,out _) && w.Gold==40,"out of range built");
            Check(w.OrderBuild(8,2,out _) && w.Gold==40,"order charged before arrival");
            Run(w,120);
            Check(w.Grid.At(8,2)!=null && w.Gold==20 && !w.HasBuildOrder,"drone did not complete purchase");
            Check(w.OrderBuild(2,2,out _),"second order rejected");
            w.MoveBuilder(new V2(-100,100)); Run(w,120);
            Check(w.Grid.At(2,2)==null && w.Gold==20,"cancelled order built");
            Check(w.BuilderPosition.X==.5f && w.BuilderPosition.Y==7.5f,"move not clamped");
            Check(w.OrderBuild(8,1,out _),"replacement first order failed");
            Check(w.OrderBuild(8,5,out _),"replacement failed"); Run(w,120);
            Check(w.Grid.At(8,1)==null && w.Grid.At(8,5)!=null && w.Gold==0,"replaced order built or double charged");
        }
        public static void ArrivalRevalidation()
        {
            var w=Small(); w.OrderBuild(8,2,out _);
            w.Grid.Build(8,2,w.Config.Tower); Run(w,120);
            Check(w.Gold==40 && w.Grid.Towers.Count==1 && !w.HasBuildOrder,"occupied arrival charged");
            Check(w.Sell(8,2) && w.Gold==40,"unpaid tower generated money");
        }
        public static void Rewards()
        {
            var w=Small(); w.Build(2,3,out _); w.StartWave();
            for(int i=0;i<1000&&!w.Finished;i++)w.Step();
            Check(w.Won && w.Killed==1 && w.Gold==52,"kill/wave reward or victory wrong");
            long tick=w.Tick; Run(w,100);
            Check(w.Gold==52 && w.Tick==tick && !w.StartWave(),"finished match mutated or rewarded twice");
        }
        public static void Defeat()
        {
            var c=Small().Config; c.StartingLives=1;
            var w=new World(c); w.StartWave(); Run(w,600);
            Check(w.Defeated && !w.Won && w.Lives==0 && w.Gold==40,"defeat leaked reward or wrong result");
            Check(!w.Build(2,3,out _) && !w.OrderBuild(8,2,out _) && !w.StartWave(),"defeat accepts actions");
            long tick=w.Tick;Run(w,30);Check(w.Tick==tick,"defeat not frozen");
        }
        public static void FinishedTeamQueues()
        {
            foreach(bool victory in new[]{false,true})for(int players=1;players<=4;players++) {
                var c=new Scenario {
                    Economy=true,BuilderEnabled=true,StartingGold=1200,StartingLives=1,
                    Width=64,Height=12,BuildRange=5,KillReward=3,WaveReward=7,
                    Spawn=new V2(1.5f,4.5f),GroundRoute=new[]{new V2(8.5f,4.5f)},FlightRoute=new[]{new V2(8.5f,4.5f)},
                    Tower=new TowerSpec{Damage=victory?100:0,Range=20},Waves=new[]{new WaveSpec{Count=1,Health=7,Speed=6}}
                };
                var w=new World(c,new MatchOptions{PlayerCount=players});
                var ids=new int[players];var positions=new V2[players];
                for(int p=0;p<players;p++) {
                    w.SelectPlayer(p);int x=2+p%2,y=p<2?2:6;
                    Check(w.Build(x,y,out _),"paid team tower failed");ids[p]=w.Grid.At(x,y).Id;
                    Check(w.Upgrade(ids[p],out _),"paid team upgrade failed");
                    for(int order=0;order<3;order++)Check(w.OrderBuild(50+p*2,2+order*2,out _,order>0),"pending team order failed");
                    Check(w.QueuedBuilds==3&&w.Gold==1200/players-40,"queue reserved money or lost orders");
                }
                Check(w.StartWave(),"terminal wave launch failed");
                for(int tick=0;tick<300&&!w.Finished;tick++)w.Step();
                Check(w.Finished&&w.Won==victory&&w.Defeated!=victory,"wrong terminal outcome");
                Check(w.Grid.Towers.Count==players,"distant order completed before fixture ended");
                Check(w.LastWaveSummary.Cleared==victory&&w.LastWaveSummary.TeamGold==(victory?10:0),"terminal reward incorrect");
                long finishedTick=w.Tick;
                for(int p=0;p<players;p++) {
                    w.SelectPlayer(p);positions[p]=w.BuilderPosition;
                    int expected=1200/players-40+(victory?10/players+(p<10%players?1:0):0);
                    Check(w.Gold==expected&&w.QueuedBuilds==0&&!w.HasBuildOrder,"finished team retained orders or changed wallet");
                    var t=w.Grid.Find(ids[p]);
                    Check(!w.Upgrade(ids[p],out _)&&!w.Sell(t.CellX,t.CellY)&&!w.OrderBuild(40,2,out _)&&!w.StartWave(),"terminal match accepted spending or refund");
                    w.MoveBuilder(new V2(40,2));Check(w.Gold==expected,"rejected terminal action charged");
                }
                Run(w,200);
                Check(w.Tick==finishedTick&&w.Grid.Towers.Count==players,"terminal simulation advanced");
                for(int p=0;p<players;p++){w.SelectPlayer(p);Check(V2.Distance(w.BuilderPosition,positions[p])==0,"finished builder moved");}
                var fresh=w.Restart();
                Check(!fresh.Finished&&fresh.WaveIndex==-1&&fresh.Grid.Towers.Count==0&&fresh.LastWaveSummary==null,"restart retained terminal state");
                for(int p=0;p<players;p++){fresh.SelectPlayer(p);Check(fresh.Gold==1200/players&&fresh.QueuedBuilds==0,"restart retained queue or spending");}
            }
        }
        public static void SharedRoute()
        {
            var c=Scenario.SharedDefense(); c.Economy=false;
            var w=new World(c); w.TowersFire=false;
            var e=w.Spawn(c.Waves[0],c.Spawn); int next=0;
            for(int i=0;i<2400&&w.Enemies.Count>0;i++) {w.Step(); if(e.Checkpoint>next) {Check(e.Checkpoint==next+1,"checkpoint skipped"); next++;}}
            Check(next==4 && w.Leaked==1,"connected areas route failed");
            Check(c.Waves[4].Flying && c.Waves[9].Flying && !c.Waves[3].Flying,"flight cadence wrong");
        }
        public static void Purchase(World w,int x,int y)
        {
            Check(w.OrderBuild(x,y,out var reason),reason+" at "+x+","+y);
            for(int t=0;t<600&&w.HasBuildOrder;t++)w.Step();
            Check(w.Grid.At(x,y)!=null,"build failed at "+x+","+y);
        }
        public static void FullMatch()
        {
            var w=new World(Scenario.SharedDefense());
            for(int lane=0;lane<4;lane++)for(int y=24;y<=36;y+=3)Purchase(w,3+lane*9,y);
            foreach(int y in new[]{3,6,9,14,19})foreach(int x in new[]{14,16,20,22})Purchase(w,x,y);
            Check(w.Gold==400,"starter budget wrong");
            for(int wave=0;wave<10;wave++)
            {
                Check(w.StartWave(),"wave failed to start: "+wave);
                int ticks=0;while(w.WaveActive&&!w.Finished&&ticks++<6000)w.Step();
                Check(!w.WaveActive,"wave stalled: "+wave);
                Check(!w.Defeated,"starter defense lost: "+wave+" leaks="+w.Leaked);
            }
            Check(w.Won && w.Killed+w.Leaked==680 && w.Lives>0,"complete match outcome wrong");
        }
    }
}
