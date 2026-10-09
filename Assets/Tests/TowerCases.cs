using System;
using FrostMaze.Simulation;
namespace FrostMaze.Tests
{
    public static class TowerCases
    {
        static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
        public static void WavePlanning()
        {
            var map=MapCases.Load(false);map.BuilderEnabled=false;
            var w=new World(map,new MatchOptions{PlayerCount=2});
            var air=w.PreviewWave(4);var rush=w.PreviewWave(17);var swarm=w.PreviewWave(12);
            Check(w.DefensesFor(air)==0,"empty air coverage");
            Check(w.WaveAdvice(air).Contains("Aurora Needle")&&w.WaveAdvice(rush).Contains("Rime Binder")&&w.WaveAdvice(swarm).Contains("Hail Bell"),"Rime advice ignored wave roles");
            w.SelectedDesign=3;Check(w.Build(28,11,out _),"ground artillery fixture");
            Check(w.DefensesFor(air)==0&&w.DefensesFor(swarm)==1,"ground-only artillery advertised as anti-air");
            w.SelectPlayer(1);w.SelectedDesign=4;Check(w.Build(30,11,out _),"second-owner anti-air fixture");
            Check(w.DefensesFor(air)==1&&w.DefensesFor(swarm)==1,"team readiness ignored another owner or air-only targeting");
            w.SelectedDesign=1;Check(w.Build(32,11,out _),"wall fixture");Check(w.DefensesFor(air)==1,"unarmed wall counted as anti-air");
            Check(w.WaveAdvice(w.PreviewWave(13)).Contains("sealed paths"),"siege advice missing");
            Check(w.Players[0].Gold==map.StartingGold/2-65&&w.Players[1].Gold==map.StartingGold/2-60,"read-only advice changed wallets");
            foreach(bool iron in new[]{false,true}) {
                var c=MapCases.Load(iron);for(int f=0;f<c.Factions.Length;f++) {
                    var team=new World(c,new MatchOptions{Factions=new[]{f,0,0,0}});
                    string hint=team.WaveAdvice(team.PreviewWave(4));bool match=false;
                    foreach(int d in c.Factions[f].Designs)if(c.Catalog[d].Spec.TargetsAir&&hint.Contains(c.Catalog[d].Name))match=true;
                    Check(match,"air advice did not name a tower from the selected faction");
                }
            }
        }
        public static void FinalAirPlanning()
        {
            var map=MapCases.Load(false);map.BuilderEnabled=false;map.StartingGold=600; // Late-game refund fixture, not an opening.
            var w=new World(map,new MatchOptions{PlayerCount=2});
            w.SelectedDesign=3;Check(w.Build(28,11,out _),"paid ground fixture");
            int tower=w.Grid.At(28,11).Id;Check(w.Upgrade(tower,out _),"paid upgraded ground fixture");
            w.SelectPlayer(1);w.SelectedDesign=3;Check(w.Build(30,11,out _),"other-owner ground fixture");
            w.SelectedDesign=1;Check(w.Build(32,11,out _),"unarmed fixture");
            w.SelectPlayer(0);long tick=w.Tick;int gold=w.Gold;
            Check(!w.WaveAdvice(4).Contains("selling"),"early flying wave suggested dismantling ground defense");
            Check(w.WaveAdvice(19).Contains("Your 1 ground-only")&&w.WaveAdvice(19).Contains("96g"),"final advice ignored owner or upgrade refund");
            Check(w.Gold==gold&&w.Tick==tick&&w.Grid.Towers.Count==3,"forecast mutated match");
            w.SelectPlayer(1);Check(w.WaveAdvice(19).Contains("Your 1 ground-only")&&w.WaveAdvice(19).Contains("48g"),"wall or other owner entered refund suggestion");
            map.Waves[19].Flying=false;Check(!w.WaveAdvice(14).Contains("selling"),"future ground wave ignored");map.Waves[19].Flying=true;
            w.SelectPlayer(0);Check(w.Sell(28,11),"sale fixture");Check(!w.WaveAdvice(19).Contains("selling"),"sold tower persisted in advice");
            var iron=MapCases.Load(true);iron.BuilderEnabled=false;var robot=new World(iron);
            int choice=-1;foreach(int d in iron.Factions[0].Designs)if(iron.Catalog[d].Spec.Damage>0&&!iron.Catalog[d].Spec.TargetsAir){choice=d;break;}
            Check(choice>=0,"robot ground design fixture");robot.SelectedDesign=choice;
            bool built=false;for(int y=1;y<63&&!built;y++)for(int x=1;x<63&&!built;x++)built=robot.Build(x,y,out _);
            Check(built&&robot.WaveAdvice(19).Contains("lock new champions"),"prerequisite warning missing");
        }
        public static void EveryArmedDesignHasTargets()
        {
            foreach (bool iron in new[] {false, true})
                foreach (var design in MapCases.Load(iron).Catalog)
                    Check(design.Spec.Damage <= 0 || design.Spec.TargetsGround || design.Spec.TargetsAir, design.Name + " has a weapon but cannot target anything");
            var c = Scenario.SharedDefense(); RobotFactions.Apply(c); c.BuilderEnabled = false;
            var w = new World(c, new MatchOptions { Factions = new[] {1, 0, 0, 0} });
            w.SelectedDesign = 11;
            Check(w.Build(16, 14, out _), "air splash purchase failed");
            var first = w.Spawn(new WaveSpec {Flying = true, Health = 1000}, new V2(18, 15));
            var second = w.Spawn(new WaveSpec {Flying = true, Health = 1000}, new V2(18.6f, 15));
            var ground = w.Spawn(new WaveSpec {Health = 1000}, new V2(18, 15));
            w.Step();
            Check(first.Health < 1000 && second.Health == first.Health && ground.Health == 1000, "air splash must hit flyers and spare ground units");
        }
        public static void ShotHistoryBounded()
        {
            var c = new Scenario {Width = 12, Height = 10, Spawn = new V2(1, 5), GroundRoute = new[] {new V2(10, 5)}, FlightRoute = new[] {new V2(10, 5)}};
            var w = new World(c);
            w.Grid.Build(4, 3, new TowerSpec {Damage = 1, ChainTargets = 2, Interval = World.FixedDelta, Range = 8});
            for (int i = 0; i < 3; i++) w.Spawn(new WaveSpec {Health = 10000, Speed = 0}, new V2(6 + i * .6f, 5));
            for (int tick = 0; tick < 200; tick++) {
                w.Step();
                Check(w.Shots.Count <= 128, "chain attacks grew the cosmetic shot history beyond its bound");
                for (int shot = 1; shot < w.Shots.Count; shot++) Check(w.Shots[shot].Serial > w.Shots[shot - 1].Serial, "retained shots lost serial order");
            }
            Check(w.Shots.Count == 128 && w.Shots[0].Serial > 1, "history should retain the latest events");
        }
        public static void ChainFeedbackOrigins()
        {
            foreach(bool sourceAir in new[]{false,true}) {
                var c=new Scenario{Width=12,Height=10,Spawn=new V2(1,5),GroundRoute=new[]{new V2(10,5)},FlightRoute=new[]{new V2(10,5)}};
                var w=new World(c);
                w.Grid.Build(4,3,new TowerSpec{Damage=1,ChainTargets=1,Range=8});
                w.Spawn(new WaveSpec{Flying=sourceAir,Health=1000,Speed=0},new V2(6,5));
                w.Spawn(new WaveSpec{Flying=!sourceAir,Health=1000,Speed=0},new V2(6.6f,5));
                w.Step();
                Check(w.Shots.Count==2,"expected a chain arc and tower shot");
                Check(w.Shots[0].Chained&&w.Shots[0].FromFlying==sourceAir&&w.Shots[0].Flying!=sourceAir,"mixed-flight chain lost source/target identity");
                Check(!w.Shots[1].Chained&&w.Shots[1].Flying==sourceAir,"direct shot mistaken for chain");
            }
        }
        public static void RolesAndOrders()
        {
            var w=new World(Scenario.SharedDefense());w.SelectedDesign=1;w.OrderBuild(3,30,out _);w.SelectedDesign=2;
            for(int i=0;i<300;i++)w.Step();
            var wall=w.Grid.At(3,30);
            Check(wall!=null&&wall.Design==1&&wall.Spec.Damage==0&&w.Gold==1195,"changing toolbar changed pending construction");
            Check(w.SelectedDesign==2,"completion changed selected design");
        }
        public static void Upgrades()
        {
            var c=Scenario.SharedDefense();var w=new World(c);Check(w.Build(16,14,out _),"setup tower rejected");var t=w.Grid.At(16,14);
            Check(w.Upgrade(t.Id,out _)&&w.Upgrade(t.Id,out _),"upgrade failed");
            Check(t.Level==3&&t.Spec.Damage>35&&c.Catalog[0].Spec.Damage==14,"upgrade mutated blueprint");
            Check(!w.Upgrade(t.Id,out _)&&w.Gold==1120,"max-level upgrade charged");
            Check(w.TowerOwner(t.Id)==0&&w.SaleRefund(t.Id)==60,"inspector must report exact invested refund and owner");
            Check(w.Sell(16,14)&&w.Gold==1180,"upgrade refund incorrect");
            Check(w.TowerOwner(t.Id)==-1&&w.SaleRefund(t.Id)==0,"sold tower inspector retained stale information");
            Check(w.Build(16,14,out _),"second purchase failed");var destroyed=w.Grid.At(16,14);
            w.Grid.Damage(destroyed.Id,destroyed.Health);
            Check(w.TowerOwner(destroyed.Id)==-1&&w.SaleRefund(destroyed.Id)==0,"destroyed tower must not advertise a reclaimable refund");
        }
        public static void GroundSplash()
        {
            var w=new World(Scenario.SharedDefense());w.SelectedDesign=2;Check(w.Build(16,14,out _),"cannon placement failed");
            var a=w.Spawn(new WaveSpec{Health=100},new V2(18,15));
            var b=w.Spawn(new WaveSpec{Health=100},new V2(18.7f,15));
            var air=w.Spawn(new WaveSpec{Health=100,Flying=true},new V2(18.4f,15));
            w.Step();Check(a.Health==68&&b.Health==68&&air.Health==100,"splash targeting wrong");
            Check(w.Shots.Count==1&&w.Shots[0].Splash>0,"shot feedback missing");
        }
        public static void QueuedConstruction()
        {
            var w=new World(Scenario.SharedDefense());w.SelectedDesign=1;
            w.OrderBuild(3,30,out _);w.OrderBuild(3,31,out _,true);w.SelectedDesign=2;w.OrderBuild(3,32,out _,true);
            Check(w.QueuedBuilds==3&&!w.OrderBuild(3,31,out _,true),"queue duplicate accepted");
            for(int i=0;i<400;i++)w.Step();
            Check(w.Grid.Towers.Count==3&&w.Gold==1130&&w.Grid.At(3,32).Design==2&&w.QueuedBuilds==0,"queued designs or charges wrong");
            w.OrderBuild(20,30,out _);w.OrderBuild(20,31,out _,true);w.MoveBuilder(w.BuilderPosition);
            for(int i=0;i<300;i++)w.Step();
            Check(w.QueuedBuilds==0&&w.Grid.Towers.Count==3,"move did not cancel queue");
        }
        public static void BlockedQueueContinues()
        {
            var w=new World(Scenario.SharedDefense());w.SelectedDesign=1;
            // Farther first, nearer last: completion must follow clicks, not travel distance.
            Check(w.OrderBuild(3,32,out _),"first order rejected");
            Check(w.OrderBuild(3,31,out _,true),"middle order rejected");
            Check(w.OrderBuild(3,30,out _,true),"last order rejected");
            var blocker=w.Spawn(new WaveSpec{Speed=0,Health=10000},new V2(3.5f,31.5f));
            Check(blocker!=null,"blocker fixture missing");
            bool skipped=false;
            for(int i=0;i<600&&w.QueuedBuilds>0;i++) {
                w.Step();skipped|=w.BuilderNotice.StartsWith("Skipped order:");
                if(w.Grid.At(3,30)!=null)Check(w.Grid.At(3,32)!=null,"queue reordered by distance");
            }
            Check(w.QueuedBuilds==0&&skipped,"blocked order stalled queue or lacked feedback");
            Check(w.Grid.At(3,31)==null&&w.Grid.At(3,32)!=null&&w.Grid.At(3,30)!=null,"blocked footprint built or later order lost");
            Check(w.Grid.Towers.Count==2&&w.Gold==1190,"skipped order charged gold");
            // Skipping discards the order; clearing the enemy later must not retry it.
            w.Enemies.Clear();for(int i=0;i<100;i++)w.Step();
            Check(w.Grid.At(3,31)==null&&w.Gold==1190,"skipped order retried");

            // Orders are paid at completion; accepting a queue does not reserve gold.
            var c=Scenario.SharedDefense();c.StartingGold=10;var poor=new World(c);
            poor.SelectedDesign=1;Check(poor.OrderBuild(3,32,out _),"cheap first order");
            poor.SelectedDesign=1;Check(poor.OrderBuild(3,31,out _,true),"cheap middle order");
            Check(poor.OrderBuild(3,30,out _,true),"cheap final order");
            for(int i=0;i<600&&poor.QueuedBuilds>0;i++)poor.Step();
            Check(poor.Gold==0&&poor.Grid.Towers.Count==2&&poor.Grid.At(3,30)==null&&poor.QueuedBuilds==0,"unfunded order charged or stalled");
        }
        public static void WallNoWeapon()
        {
            var w=new World(Scenario.SharedDefense());w.SelectedDesign=1;w.Build(16,14,out _);
            var e=w.Spawn(new WaveSpec{Health=100},new V2(18,15));w.Step();
            Check(e.Health==100&&w.Shots.Count==0,"barricade fired a weapon");
        }
    }
}
