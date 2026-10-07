using System;
using FrostMaze.Simulation;
namespace FrostMaze.Tests
{
    public static class TowerCases
    {
        static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
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
            Check(w.Sell(16,14)&&w.Gold==1180,"upgrade refund incorrect");
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
