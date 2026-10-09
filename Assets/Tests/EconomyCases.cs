using System;
using System.Linq;
using FrostMaze.Simulation;
using FrostMaze.Simulation.Online;
namespace FrostMaze.Tests
{
    public static class EconomyCases
    {
        static void Check(bool b,string message){if(!b)throw new Exception(message);}
        public static void Milestones()
        {
            foreach(bool iron in new[]{false,true})for(int players=1;players<=4;players++) {
                var c=MapCases.Load(iron);int originalLanes=c.Lanes.Length;
                var actual=new World(c,new MatchOptions{PlayerCount=players});
                Check(actual.Players.Sum(p=>p.Gold)==(iron?2200:240)&&actual.LaneCount==originalLanes,"Opening/lane budget");
                Check(actual.Players.All(p=>p.Wood==0),"Faction selection left spendable starting wood");
                // Small combat fixture retains exact campaign reward schedules. No long path simulation needed.
                var test=new Scenario{Economy=true,StartingGold=c.StartingGold,Width=12,Height=8,Spawn=new V2(1.5f,4.5f),GroundRoute=new[]{new V2(10.5f,4.5f)},FlightRoute=new[]{new V2(10.5f,4.5f)},Waves=c.Waves};
                foreach(var wave in test.Waves){wave.Count=1;wave.Health=1;}
                var w=new World(test,new MatchOptions{PlayerCount=players});int earned=0;
                for(int wave=0;wave<20;wave++) {
                    Check(w.StartWave(),"Wave start");w.Step();w.Enemies[0].Health=0;w.Step();
                    earned+=1+wave/4+4*(14+2*wave);
                    Check(w.TotalGoldEarned==earned&&w.Players.Sum(p=>p.Gold)==test.StartingGold+earned,"Progressive gold ledger");
                    int wood=wave>=(iron?13:8)?4:0;
                    Check(w.Players.Sum(p=>p.Wood)==wood,"Wood milestone timing/budget");
                    Check(w.LastWaveSummary.TeamWood==(wave==(iron?13:8)?4:0),"Wood summary");
                    int before=w.Players.Sum(p=>p.Wood);w.Step();Check(w.Players.Sum(p=>p.Wood)==before,"Repeated milestone reward");
                }
                Check(w.Restart().Players.All(p=>p.Wood==0),"Restart retained wood");
            }
        }
        public static void Purchases()
        {
            var c=Scenario.SharedDefense();Factions.Apply(c);c.FactionWoodUnlocks=true;c.BuilderEnabled=false;
            var w=new World(c,new MatchOptions{PlayerCount=2});string reason;
            Check(!w.ChooseFaction(1,out reason),"Unlocked without wood");w.Players[0].Wood=1;
            Check(w.ChooseFaction(1,out reason)&&w.Wood==0&&w.DesignAvailable(5)&&w.DesignAvailable(0),"Faction unlock");
            Check(w.ChooseFaction(0,out reason)&&w.Wood==0&&!w.ChooseFaction(-1,out reason),"Free roster switch / invalid faction");
            w.SelectPlayer(1);Check(!w.FactionUnlocked(1),"Faction unlock leaked to teammate");w.SelectPlayer(0);
            string digest=StateDigest.Of(w);w.Players[0].Wood++;Check(StateDigest.Of(w)!=digest,"Wood missing from digest");
            var iron=MapCases.Load(true);iron.BuilderEnabled=true;
            var paid=new World(iron);paid.Players[0].Gold=10000;
            // Use paid prerequisites on known valid downstream cells.
            for(int d=0;d<6;d++) {
                paid.SelectedDesign=d;bool built=false;
                for(int y=3;y<20&&!built;y++)for(int x=10;x<55&&!built;x++)if(paid.OrderBuild(x,y,out reason)){
                    for(int i=0;i<1000&&paid.HasBuildOrder;i++)paid.Step();built=true;
                }
                Check(built,"Prerequisite placement");
            }
            paid.SelectedDesign=6;Check(paid.BuildWoodCost==1&&paid.BuildCost==750,"Reference champion cost");
            Check(paid.BuildSpec.Damage>300&&paid.BuildSpec.Health==480,"Wood champion lost its increased combat value");
            Check(!paid.OrderBuild(30,6,out reason)&&reason.Contains("wood"),"Wood gate missing");
            paid.Players[0].Wood=1;int gold=paid.Gold;int orders=0;
            for(int y=3;y<20&&orders<2;y++)for(int x=10;x<55&&orders<2;x++)if(paid.OrderBuild(x,y,out reason,orders>0))orders++;
            Check(orders==2&&paid.Wood==1&&paid.Gold==gold,"Queue reserved resources");
            for(int i=0;i<2000&&paid.QueuedBuilds>0;i++)paid.Step();
            Check(paid.Wood==0&&paid.Gold==gold-750&&paid.Grid.Towers.Count==7,"Queued overspend / failed-order charge");
            var champion=paid.Grid.Towers.Last();Check(paid.Sell(champion.CellX,champion.CellY)&&paid.Wood==1,"Sold wood not returned");
        }
    }
}
