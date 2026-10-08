using System;
using FrostMaze.Simulation;
namespace FrostMaze.Tests
{
    public static class TowerCases
    {
        static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
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
        public static void WallNoWeapon()
        {
            var w=new World(Scenario.SharedDefense());w.SelectedDesign=1;w.Build(16,14,out _);
            var e=w.Spawn(new WaveSpec{Health=100},new V2(18,15));w.Step();
            Check(e.Health==100&&w.Shots.Count==0,"barricade fired a weapon");
        }
    }
}
