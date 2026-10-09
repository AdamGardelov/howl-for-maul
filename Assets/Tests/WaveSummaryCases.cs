using System;
using FrostMaze.Simulation;
namespace FrostMaze.Tests
{
    public static class WaveSummaryCases
    {
        static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
        static Scenario Config() => new Scenario {
            Economy=true,StartingGold=1200,KillReward=2,WaveReward=7,Width=12,Height=8,
            Spawn=new V2(1.5f,4.5f),GroundRoute=new[]{new V2(10.5f,4.5f)},FlightRoute=new[]{new V2(10.5f,4.5f)},
            Tower=new TowerSpec{Damage=100,Range=20,Interval=.1f},
            Waves=new[]{new WaveSpec{Count=3,Health=1,SpawnInterval=.2f},new WaveSpec{Count=3,Health=1,SpawnInterval=.2f}}
        };
        static void Finish(World world)
        {
            for(int tick=0;tick<3000&&world.WaveActive&&!world.Defeated;tick++)world.Step();
            Check(world.LastWaveSummary!=null,"Missing wave summary");
        }
        public static void IncomeIgnoresSpendingAndSnapshots()
        {
            for(int players=1;players<=4;players++) {
                var w=new World(Config(),new MatchOptions{PlayerCount=players});
                Check(w.LastWaveSummary==null,"Starting gold produced a wave summary");
                Check(w.Build(2,2,out _),"Paid defense failed");
                int[] before=new int[players];for(int p=0;p<players;p++)before[p]=w.Players[p].Gold;
                Check(w.StartWave(),"Wave failed to start");
                Check(w.LastWaveSummary==null,"Summary exists during new wave");
                // Paid construction, an upgrade and a refund must not count as wave income.
                Check(w.Build(4,2,out _),"Mid-wave construction failed");
                Check(w.Upgrade(w.Grid.At(4,2).Id,out _),"Mid-wave upgrade failed");
                Check(w.Sell(4,2),"Mid-wave sale failed");
                w.SelectPlayer(players-1);Finish(w);
                var first=w.LastWaveSummary;
                Check(first.WaveNumber==1&&first.Cleared&&first.Killed==3&&first.Leaked==0,"Wrong first-wave totals");
                Check(first.TeamGold==13,"Bounty or completion reward missing");
                int sum=0;for(int p=0;p<players;p++) {
                    int earned=w.Players[p].Gold-before[p]+(p==0?10:0);
                    Check(first.GoldForPlayer(p)==earned,"Income confused with wallet balance or spending");sum+=earned;
                    before[p]=w.Players[p].Gold;
                }
                Check(sum==13,"Team reward lost integer remainder");
                int firstShare=first.GoldForPlayer(0);
                Check(w.StartWave()&&w.LastWaveSummary==null,"Next launch did not reset current summary");
                Check(!w.StartWave(),"Active wave accepted another launch");Finish(w);
                Check(w.Won&&w.LastWaveSummary.WaveNumber==2&&w.LastWaveSummary.TeamGold==13,"Final wave summary wrong");
                for(int p=0;p<players;p++)Check(w.LastWaveSummary.GoldForPlayer(p)==w.Players[p].Gold-before[p],"Second wave inherited previous income");
                Check(first.GoldForPlayer(0)==firstShare&&first.WaveNumber==1,"Previous snapshot changed after next wave");
                var final=w.LastWaveSummary;int gold=w.Gold;w.Step();w.Step();
                Check(w.LastWaveSummary==final&&w.Gold==gold,"Completed wave was rewarded/reported twice");
                Check(w.Restart().LastWaveSummary==null,"Restart retained previous result");
            }
        }
        public static void LeaksAndDefeat()
        {
            var c=Config();c.StartingLives=2;foreach(var wave in c.Waves)wave.Count=1;
            var w=new World(c);w.TowersFire=false;w.StartWave();Finish(w);
            Check(w.LastWaveSummary.Cleared&&w.LastWaveSummary.Leaked==1&&w.LastWaveSummary.Killed==0&&w.LastWaveSummary.TeamGold==7,"Survived leaking wave report wrong");
            w.StartWave();Finish(w);
            Check(w.Defeated&&!w.LastWaveSummary.Cleared&&w.LastWaveSummary.Leaked==1&&w.LastWaveSummary.TeamGold==0,"Defeat granted a completion bonus or used cumulative leaks");
            c=Config();c.StartingLives=1;c.Waves[0].Count=100;
            w=new World(c);w.TowersFire=false;w.StartWave();Finish(w);
            Check(w.Defeated&&w.Pending>0&&!w.LastWaveSummary.Cleared,"Defeat must report even while enemies await spawning");
            Check(w.LastWaveSummary.Leaked==w.Leaked&&w.LastWaveSummary.TeamGold==0,"Incomplete wave report wrong");
        }
        public static void AutomaticWaves()
        {
            var c=Config();c.Waves=new[]{new WaveSpec{Count=1,Health=1,Speed=.1f},new WaveSpec{Count=1,Health=1,Speed=.1f},new WaveSpec{Count=1,Health=1,Speed=.1f}};
            var w=new World(c);Check(w.Build(2,2,out _),"Defense purchase failed");
            for(int i=0;i<1200;i++)w.Step();
            Check(w.WaveIndex==-1&&!w.CountingDown,"First wave started without consent");
            Check(w.StartWave(),"First manual launch failed");Finish(w);
            Check(w.CountingDown&&w.NextWaveSeconds==30,"Missing full intermission after clear");
            long due=w.NextWaveTick;int gold=w.Gold;var summary=w.LastWaveSummary;
            w.TowersFire=false;
            for(int i=0;i<World.IntermissionTicks-1;i++)w.Step();
            Check(w.WaveIndex==0&&w.NextWaveSeconds==1&&w.Gold==gold&&w.LastWaveSummary==summary,"Early launch or repeated completion reward");
            w.Step();Check(w.Tick==due&&w.WaveIndex==1&&w.WaveActive&&!w.CountingDown&&w.NextWaveTick==-1,"Wave did not launch on exact fixed tick");
            w.TowersFire=true;Finish(w);Check(w.CountingDown&&w.NextWaveSeconds==30,"Second timer missing");
            Check(w.StartWave()&&!w.CountingDown,"Early send did not cancel timer");Finish(w);
            Check(w.Won&&!w.CountingDown&&w.NextWaveTick==-1,"Final wave scheduled another wave");
            var reset=w.Restart();for(int i=0;i<1200;i++)reset.Step();Check(reset.WaveIndex==-1&&!reset.CountingDown,"Restart inherited timer");
            c=Config();c.StartingLives=1;w=new World(c);w.TowersFire=false;w.StartWave();Finish(w);
            Check(w.Defeated&&!w.CountingDown,"Defeat scheduled another wave");
            w=new World(Config(),new MatchOptions{AutomaticWaves=false});w.Build(2,2,out _);w.StartWave();Finish(w);
            for(int i=0;i<1200;i++)w.Step();Check(w.WaveIndex==0&&!w.CountingDown,"Historical diagnostic mode launched automatically");
        }
        public static void FreeBuildAwardsNoIncome()
        {
            var c=Config();c.Economy=false;var w=new World(c);w.Build(2,2,out _);w.StartWave();Finish(w);
            Check(w.LastWaveSummary.Cleared&&w.LastWaveSummary.Killed==3&&w.LastWaveSummary.TeamGold==0,"Free-build summary invented paid rewards");
        }
    }
}
