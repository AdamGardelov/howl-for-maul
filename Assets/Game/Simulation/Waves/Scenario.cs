using System;
namespace FrostMaze.Simulation
{
    [Serializable]
    public sealed class WaveSpec
    {
        public string Name = "Ground patrol";
        public int Count = 25;
        public float SpawnInterval = 0.4f, Health = 50, Speed = 1.9f, Radius = 0.2f, Damage = 12, AttackInterval = 0.6f;
        public bool Flying;
    }
    [Serializable]
    public sealed class Scenario
    {
        public string[] StartNames = new string[0];
        public V2[] BuilderStarts = new V2[0];
        public V2 SoloBuilderStart = new V2(18,14);
        public LaneSpec[] Lanes = new LaneSpec[0];
        public TerrainBlock[] Terrain = new TerrainBlock[0];
        public string Name = "Maze Lab";
        public bool Economy, BuilderEnabled;
        public int StartingGold = 300, TowerCost = 20, SaleRefund = 15, KillReward = 2, WaveReward = 30, StartingLives = 30;
        public float BuilderSpeed = 9, BuildRange = 3;
        public int Width = 30, Height = 20;
        public float NavigationStep = 0.5f, BreachCost = 12, Separation = 1.4f, Acceleration = 9, AttackReach = 0.16f, CheckpointRadius = 0.2f;
        public V2 Spawn = new V2(1.5f, 10.5f);
        public V2[] GroundRoute = { new V2(28.5f, 10.5f) };
        public V2[] FlightRoute = { new V2(10.5f, 5.5f), new V2(20.5f, 15.5f), new V2(28.5f, 10.5f) };
        public TowerDesign[] Catalog = new TowerDesign[0];
        public TowerSpec Tower = new TowerSpec();
        public WaveSpec[] Waves = { new WaveSpec(), new WaveSpec { Count = 35, Health = 65 }, new WaveSpec { Count = 45, Health = 80 }, new WaveSpec { Count = 60, Health = 100 }, new WaveSpec { Name = "Sky drifters", Flying = true, Count = 35, Health = 60, Speed = 2.2f } };
        public static Scenario SharedDefense()
        {
            var c = new Scenario {
                Name="Frostfall Maul", Width=36, Height=40, Economy=true, BuilderEnabled=true,
                StartingGold=1200, WaveReward=120,
                Spawn=new V2(4.5f,38.5f),
                GroundRoute=new[]{new V2(18,19),new V2(18,12),new V2(18,1.5f)},
                FlightRoute=new[]{new V2(18,18),new V2(18,1.5f)},
                Tower=new TowerSpec {Damage=14,Range=4,Interval=.65f},
                Catalog=new[]{
                    new TowerDesign{Name="Bolt Spire",Description="Reliable ground and air damage",Cost=20,Refund=15,Spec=new TowerSpec{Damage=14,Range=4,Interval=.65f}},
                    new TowerDesign{Name="Barricade",Description="Cheap maze walls. No weapon",Cost=5,Refund=3,Spec=new TowerSpec{Damage=0,Health=180,TargetsAir=false,TargetsGround=false}},
                    new TowerDesign{Name="Ember Cannon",Description="Ground splash. Cannot hit flying enemies",Cost=60,Refund=45,Spec=new TowerSpec{Damage=32,Interval=1.3f,Range=4.5f,SplashRadius=1.25f,TargetsAir=false}}
                },
                Lanes=new LaneSpec[4], BuilderStarts=new V2[8], StartNames=new[]{"Upper 1","Upper 2","Upper 3","Upper 4","Junction L","Junction R","Last stand L","Last stand R"}, Waves=new WaveSpec[10],
                Terrain=new[]{
                    new TerrainBlock{X=8,Y=21,Width=1,Height=19},
                    new TerrainBlock{X=17,Y=21,Width=1,Height=19},
                    new TerrainBlock{X=26,Y=21,Width=1,Height=19},
                    new TerrainBlock{X=0,Y=16,Width=12,Height=2},
                    new TerrainBlock{X=24,Y=16,Width=12,Height=2},
                    new TerrainBlock{X=0,Y=0,Width=12,Height=10},
                    new TerrainBlock{X=24,Y=0,Width=12,Height=10}
                }
            };
            for(int lane=0;lane<4;lane++) {
                float x=4.5f+9*lane;
                c.BuilderStarts[lane]=new V2(x,30);
                c.Lanes[lane]=new LaneSpec {Spawn=new V2(x,38.5f),
                    GroundRoute=new[]{new V2(x,22.5f),new V2(18,19),new V2(18,12),new V2(18,1.5f)},
                    FlightRoute=new[]{new V2(x,25),new V2(18,18),new V2(18,1.5f)}};
            }
            c.BuilderStarts[4]=new V2(14,19);c.BuilderStarts[5]=new V2(22,19);c.BuilderStarts[6]=new V2(14,7);c.BuilderStarts[7]=new V2(22,7);
            for(int i=0;i<10;i++)c.Waves[i]=new WaveSpec {
                Name=(i+1)%5==0 ? "Sky drifters" : "Frostbound patrol",
                Count=8+i*2,Health=35+i*12,Speed=(i+1)%5==0?2.2f:1.9f,
                SpawnInterval=.8f,Flying=(i+1)%5==0};
            return c;
        }
        public void Validate()
        {
            if (Width < 4 || Height < 4 || NavigationStep <= 0 || NavigationStep > 1 || GroundRoute == null || GroundRoute.Length == 0 || FlightRoute == null || FlightRoute.Length == 0 || Waves == null || Waves.Length == 0)
                throw new ArgumentException("Invalid map, routes or waves.");
            foreach (var wave in Waves)
                if (wave.Radius <= 0 || wave.Radius >= 0.5f || wave.Speed <= 0 || wave.SpawnInterval <= 0 || wave.AttackInterval <= 0 || wave.Count < 1 || wave.Health <= 0)
                    throw new ArgumentException("Invalid wave settings.");
            if (StartingGold < 0 || TowerCost <= 0 || SaleRefund < 0 || SaleRefund > TowerCost || KillReward < 0 || WaveReward < 0 || StartingLives < 1 || BuilderSpeed <= 0 || BuildRange <= 0)
                throw new ArgumentException("Invalid economy or builder settings.");
            float clearance = 0;
            foreach (var wave in Waves)
                clearance = Math.Max(clearance, wave.Radius);
            var bounds = new MazeGrid(Width, Height);
            foreach(var block in Terrain) {
                if(block.Width<1||block.Height<1||block.X<0||block.Y<0||block.X+block.Width>Width||block.Y+block.Height>Height)throw new ArgumentException("Invalid terrain block.");
                bounds.AddTerrain(block);
            }
            if(Lanes.Length>0&&BuilderStarts.Length<4)throw new ArgumentException("Maps require four selectable builder starts.");
            if(Lanes.Length>0&&StartNames.Length!=BuilderStarts.Length)throw new ArgumentException("Builder starts need matching names.");
            foreach(var start in BuilderStarts)if(!bounds.InBounds(start,.25f))throw new ArgumentException("Builder start outside map.");
            foreach(var design in Catalog)if(design.Cost<1||design.Refund<0||design.Refund>design.Cost||design.Spec==null)throw new ArgumentException("Invalid tower catalog.");
            foreach(var lane in Lanes) {
                if(lane.GroundRoute==null||lane.GroundRoute.Length==0||lane.FlightRoute==null||lane.FlightRoute.Length==0||!bounds.TerrainClear(lane.Spawn,lane.Spawn,clearance))throw new ArgumentException("Invalid lane.");
                foreach(var p in lane.GroundRoute)if(!bounds.TerrainClear(p,p,clearance))throw new ArgumentException("Ground checkpoint intersects terrain.");
                foreach(var p in lane.FlightRoute)if(!bounds.InBounds(p,clearance))throw new ArgumentException("Flight checkpoint outside map.");
            }
            if (!bounds.InBounds(Spawn, clearance))
                throw new ArgumentException("Spawn outside map clearance.");
            foreach (var p in GroundRoute)
                if (!bounds.InBounds(p, clearance))
                    throw new ArgumentException("Ground checkpoint outside map clearance.");
            foreach (var p in FlightRoute)
                if (!bounds.InBounds(p, clearance))
                    throw new ArgumentException("Flight checkpoint outside map clearance.");
        }
    }
}
