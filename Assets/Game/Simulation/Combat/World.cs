using System;
using System.Collections.Generic;
namespace FrostMaze.Simulation
{
    public sealed class World
    {
        public const float FixedDelta = 1f / 30f;
        public readonly Scenario Config;
        public readonly MazeGrid Grid;
        public readonly FlowNavigation Navigation;
        public readonly List<Enemy> Enemies = new List<Enemy>();
        public int WaveIndex { get; private set; } = -1;
        public int Pending
        {
            get; private set;
        }
        public int Killed
        {
            get; private set;
        }
        public int Leaked
        {
            get; private set;
        }
        public long Tick
        {
            get; private set;
        }
        public bool WaveActive => Pending > 0 || Enemies.Count > 0;
        public bool TowersFire = true;
        public int Gold { get; private set; }
        public int Lives => Math.Max(0, Config.StartingLives - Leaked);
        public bool Defeated => Config.Economy && Lives == 0;
        public bool Won => !Defeated && WaveIndex == Config.Waves.Length - 1 && !WaveActive && rewardedWave == WaveIndex;
        public bool Finished => Defeated || Won;
        public V2 BuilderPosition { get; private set; }
        public V2 BuilderDestination { get; private set; }
        public bool HasBuildOrder { get; private set; }
        public V2 BuildOrder { get; private set; }
        public string BuilderNotice { get; private set; } = "Builder ready.";
        int rewardedWave = -1;
        readonly Dictionary<int, int> paidTowers = new Dictionary<int, int>();
        public void MoveBuilder(V2 destination)
        {
            if (Finished) return;
            HasBuildOrder = false;
            BuilderDestination = new V2(Geometry.Clamp(destination.X, .5f, Config.Width - .5f), Geometry.Clamp(destination.Y, .5f, Config.Height - .5f));
            BuilderNotice = "Moving. Previous build order cancelled.";
        }
        public bool OrderBuild(int x, int y, out string reason)
        {
            if (!Config.BuilderEnabled) return Build(x, y, out reason);
            if (!CanBuild(x, y, out reason)) return false;
            BuildOrder = new V2(x, y);
            HasBuildOrder = true;
            BuilderDestination = new V2(x + Config.Tower.Width * .5f, y + Config.Tower.Height * .5f);
            BuilderNotice = reason = "Builder dispatched. Gold is charged on completion.";
            return true;
        }
        void StepBuilder()
        {
            if (!Config.BuilderEnabled) return;
            var delta = BuilderDestination - BuilderPosition;
            float travel = Config.BuilderSpeed * FixedDelta;
            BuilderPosition += delta.Length <= travel ? delta : delta.Normalized * travel;
            if (HasBuildOrder && V2.Distance(BuilderPosition, BuilderDestination) <= Config.BuildRange)
            {
                HasBuildOrder = false;
                Build((int)BuildOrder.X, (int)BuildOrder.Y, out string message);
                BuilderNotice = message;
                BuilderDestination = BuilderPosition;
            }
        }
        float spawnTimer;
        int nextEnemy = 1;
        public World(Scenario config)
        {
            config.Validate();
            Config = config;
            Gold = config.StartingGold;
            BuilderPosition = BuilderDestination = config.Spawn;
            Grid = new MazeGrid(config.Width, config.Height);
            Navigation = new FlowNavigation(Grid, config.NavigationStep, config.BreachCost);
        }
        public bool StartWave()
        {
            if (Finished || WaveActive || WaveIndex + 1 >= Config.Waves.Length)
                return false;
            WaveIndex++;
            Pending = Config.Waves[WaveIndex].Count;
            spawnTimer = 0;
            return true;
        }
        public bool CanBuild(int x, int y, out string reason)
        {
            if (Finished) { reason = "Match finished. Reset to play again."; return false; }
            if (Config.Economy && Gold < Config.TowerCost) { reason = "Not enough gold."; return false; }
            var spec = Config.Tower;
            var center = new V2(x + spec.Width * 0.5f, y + spec.Height * 0.5f);
            var half = new V2(spec.Width * 0.5f - (1 - spec.Fill) * 0.5f, spec.Height * 0.5f - (1 - spec.Fill) * 0.5f);
            float protection = Config.NavigationStep * 1.5f;
            foreach (var wave in Config.Waves)
                protection = Math.Max(protection, wave.Radius);
            if (Geometry.PointBox(Config.Spawn, center, half) < protection)
            {
                reason = "Keep the spawn clear.";
                return false;
            }
            foreach (var p in Config.GroundRoute)
                if (Geometry.PointBox(p, center, half) < protection)
                {
                    reason = "Keep route checkpoints clear.";
                    return false;
                }
            foreach (var e in Enemies)
                if (!e.Spec.Flying && Geometry.PointBox(e.Position, center, half) < e.Spec.Radius)
                {
                    reason = "An enemy occupies this footprint.";
                    return false;
                }
            bool occupied = x < 0 || y < 0 || x + spec.Width > Grid.Width || y + spec.Height > Grid.Height;
            for (int cx = x; cx < x + spec.Width && !occupied; cx++)
                for (int cy = y; cy < y + spec.Height; cy++) occupied |= Grid.At(cx, cy) != null;
            if (occupied)
            {
                reason = "Outside map or occupied footprint.";
                return false;
            }
            reason = "Placement valid.";
            return true;
        }
        public bool Build(int x, int y, out string reason)
        {
            if (!CanBuild(x, y, out reason)) return false;
            var center = new V2(x + Config.Tower.Width * .5f, y + Config.Tower.Height * .5f);
            if (Config.BuilderEnabled && V2.Distance(BuilderPosition, center) > Config.BuildRange)
            { reason = "Builder out of range. Issue a build order."; return false; }
            var tower = Grid.Build(x, y, Config.Tower);
            if (tower == null) { reason = "Invalid tower footprint."; return false; }
            if (Config.Economy) { Gold -= Config.TowerCost; paidTowers[tower.Id] = Config.SaleRefund; }
            reason = "Tower built. Complete route blockage is allowed.";
            return true;
        }
        public bool Sell(int x, int y)
        {
            if (Finished) return false;
            var t = Grid.At(x, y);
            if (t == null || !Grid.Remove(t.Id)) return false;
            if (paidTowers.TryGetValue(t.Id, out int refund)) { Gold += refund; paidTowers.Remove(t.Id); }
            return true;
        }
        public Enemy Spawn(WaveSpec spec, V2 position)
        {
            if (!spec.Flying && !Grid.Clear(position, position, spec.Radius))
                return null;
            foreach (var other in Enemies)
                if (other.Spec.Flying == spec.Flying && V2.Distance(position, other.Position) < spec.Radius + other.Spec.Radius + 0.02f)
                    return null;
            var e = new Enemy { Id = nextEnemy++, Position = position, Spec = spec, Health = spec.Health };
            Enemies.Add(e);
            return e;
        }
        public void Step()
        {
            if (Finished) return;
            Tick++;
            StepBuilder();
            if (Pending > 0)
            {
                spawnTimer -= FixedDelta;
                if (spawnTimer <= 0)
                {
                    var spec = Config.Waves[WaveIndex];
                    if (Spawn(spec, Config.Spawn) != null)
                    {
                        Pending--;
                        spawnTimer = spec.SpawnInterval;
                    }
                }
            }
            foreach (var tower in Grid.Towers)
            {
                tower.Cooldown = Math.Max(0, tower.Cooldown - FixedDelta);
                tower.LastTarget = 0;
                if (!TowersFire || tower.Cooldown > 0)
                    continue;
                foreach (var e in Enemies)
                    if (e.Health > 0 && (e.Spec.Flying ? tower.Spec.TargetsAir : tower.Spec.TargetsGround) && V2.Distance(tower.Center, e.Position) <= tower.Spec.Range)
                    {
                        e.Health -= tower.Spec.Damage;
                        tower.Cooldown = tower.Spec.Interval;
                        tower.LastTarget = e.Id;
                        break;
                    }
            }
            for (int i = 0; i < Enemies.Count; i++)
            {
                var e = Enemies[i];
                if (e.Health <= 0)
                    continue;
                var route = e.Spec.Flying ? Config.FlightRoute : Config.GroundRoute;
                e.Destination = route[e.Checkpoint];
                if (V2.Distance(e.Position, e.Destination) < Config.CheckpointRadius + e.Spec.Radius && (e.Spec.Flying || Grid.Clear(e.Position, e.Destination, e.Spec.Radius)))
                {
                    e.Checkpoint++;
                    if (e.Checkpoint >= route.Length)
                    {
                        e.Health = 0;
                        e.Exited = true;
                        Leaked++;
                        continue;
                    }
                    e.Destination = route[e.Checkpoint];
                }
                V2 aim = e.Destination;
                e.Blocked = false;
                e.BlockerId = 0;
                if (!e.Spec.Flying)
                {
                    var field = Navigation.Get(e.Destination, e.Spec.Radius);
                    aim = Navigation.Waypoint(field, e.Position, out bool reachable, out _);
                    if (!reachable)
                    {
                        e.Blocked = true;
                        var breach = Navigation.Get(e.Destination, e.Spec.Radius, true);
                        aim = Navigation.Waypoint(breach, e.Position, out _, out var blocker);
                        if (blocker != null)
                        {
                            e.BlockerId = blocker.Id;
                            // Aim at closest rectangle point so melee approaches the collision boundary without entering it.
                            aim = new V2(Geometry.Clamp(e.Position.X, blocker.Center.X - blocker.Half.X, blocker.Center.X + blocker.Half.X), Geometry.Clamp(e.Position.Y, blocker.Center.Y - blocker.Half.Y, blocker.Center.Y + blocker.Half.Y));
                            if (Geometry.PointBox(e.Position, blocker.Center, blocker.Half) <= e.Spec.Radius + Config.AttackReach)
                            {
                                aim = e.Position;
                                e.AttackCooldown -= FixedDelta;
                                if (e.AttackCooldown <= 0)
                                {
                                    Grid.Damage(blocker.Id, e.Spec.Damage);
                                    e.AttackCooldown = e.Spec.AttackInterval;
                                }
                            }
                        }
                    }
                }
                var toAim = aim - e.Position;
                e.IntendedDirection = toAim.Normalized;
                V2 desired = e.IntendedDirection * e.Spec.Speed;
                V2 separation = new V2();
                foreach (var other in Enemies)
                    if (other != e && other.Health > 0 && other.Spec.Flying == e.Spec.Flying)
                    {
                        var away = e.Position - other.Position;
                        float d = away.Length, range = (e.Spec.Radius + other.Spec.Radius) * 1.8f;
                        if (d > 0.0001f && d < range)
                            separation += away / d * ((range - d) / range * Config.Separation);
                    }
                // Bound avoidance so dense neighbours cannot create a false equilibrium on an open route.
                if (separation.Length > e.Spec.Speed * 0.5f)
                    separation = separation.Normalized * (e.Spec.Speed * 0.5f);
                desired += separation;
                if (desired.Length > e.Spec.Speed)
                    desired = desired.Normalized * e.Spec.Speed;
                var acceleration = desired - e.Velocity;
                float limit = Config.Acceleration * FixedDelta;
                if (acceleration.Length > limit)
                    acceleration = acceleration.Normalized * limit;
                e.Velocity += acceleration;
                var delta = e.Velocity * FixedDelta;
                if (toAim.Length < delta.Length && toAim.Length > 0.0001f)
                    delta = delta.Normalized * toAim.Length;
                if (toAim.Length < 0.001f)
                    delta = new V2();
                Move(e, delta);
            }
            for (int i = Enemies.Count - 1; i >= 0; i--)
                if (Enemies[i].Health <= 0)
                {
                    if (!Enemies[i].Exited)
                    {
                        Killed++;
                        if (Config.Economy) Gold += Config.KillReward;
                    }
                    Enemies.RemoveAt(i);
                }
            if (!WaveActive && WaveIndex > rewardedWave && !Defeated)
            {
                rewardedWave = WaveIndex;
                if (Config.Economy) Gold += Config.WaveReward;
            }
            if (Finished) HasBuildOrder = false;
        }
        void Move(Enemy e, V2 delta)
        {
            // Short substeps prevent tunnelling even at high configured speed.
            int steps = Math.Max(1, (int)Math.Ceiling(delta.Length / Math.Max(0.02f, e.Spec.Radius * 0.5f)));
            var step = delta / steps;
            var start = e.Position;
            for (int n = 0; n < steps; n++)
            {
                if (CanMove(e, e.Position + step))
                    e.Position += step;
                else if (CanMove(e, e.Position + new V2(step.X, 0)))
                    e.Position += new V2(step.X, 0);
                else if (CanMove(e, e.Position + new V2(0, step.Y)))
                    e.Position += new V2(0, step.Y);
            }
            e.Velocity = (e.Position - start) / FixedDelta;
        }
        bool CanMove(Enemy e, V2 to)
        {
            if (!Grid.InBounds(to, e.Spec.Radius) || (!e.Spec.Flying && !Grid.Clear(e.Position, to, e.Spec.Radius)))
                return false;
            foreach (var other in Enemies)
                if (other != e && other.Health > 0 && other.Spec.Flying == e.Spec.Flying && Geometry.PointSegment(other.Position, e.Position, to) < e.Spec.Radius + other.Spec.Radius - 0.001f)
                    return false;
            return true;
        }
    }
}
