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
        public int Width = 30, Height = 20;
        public float NavigationStep = 0.5f, BreachCost = 12, Separation = 1.4f, Acceleration = 9, AttackReach = 0.16f, CheckpointRadius = 0.2f;
        public V2 Spawn = new V2(1.5f, 10.5f);
        public V2[] GroundRoute = { new V2(28.5f, 10.5f) };
        public V2[] FlightRoute = { new V2(10.5f, 5.5f), new V2(20.5f, 15.5f), new V2(28.5f, 10.5f) };
        public TowerSpec Tower = new TowerSpec();
        public WaveSpec[] Waves = { new WaveSpec(), new WaveSpec { Count = 35, Health = 65 }, new WaveSpec { Count = 45, Health = 80 }, new WaveSpec { Count = 60, Health = 100 }, new WaveSpec { Name = "Sky drifters", Flying = true, Count = 35, Health = 60, Speed = 2.2f } };
        public void Validate()
        {
            if (Width < 4 || Height < 4 || NavigationStep <= 0 || NavigationStep > 1 || GroundRoute == null || GroundRoute.Length == 0 || FlightRoute == null || FlightRoute.Length == 0 || Waves == null || Waves.Length == 0)
                throw new ArgumentException("Invalid map, routes or waves.");
            foreach (var wave in Waves)
                if (wave.Radius <= 0 || wave.Radius >= 0.5f || wave.Speed <= 0 || wave.SpawnInterval <= 0 || wave.AttackInterval <= 0 || wave.Count < 1 || wave.Health <= 0)
                    throw new ArgumentException("Invalid wave settings.");
            float clearance = 0;
            foreach (var wave in Waves)
                clearance = Math.Max(clearance, wave.Radius);
            var bounds = new MazeGrid(Width, Height);
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
