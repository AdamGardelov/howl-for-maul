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
        public const int IntermissionSeconds=30,IntermissionTicks=30*IntermissionSeconds;
        public long NextWaveTick { get; private set; } = -1;
        public bool CountingDown => !Finished&&!WaveActive&&NextWaveTick>=0;
        public int NextWaveSeconds => CountingDown?(int)Math.Max(0,(NextWaveTick-Tick+29)/30):0;
        public bool TowersFire = true;
        public readonly PlayerState[] Players;
        public int ActivePlayer { get; private set; }
        public Difficulty Difficulty { get; }
        PlayerState Player => Players[ActivePlayer];
        public void SelectPlayer(int index) { if(index<0||index>=Players.Length)throw new ArgumentOutOfRangeException(nameof(index)); ActivePlayer=index; }
        public int Gold { get=>Player.Gold; private set=>Player.Gold=value; }
        public int Wood => Player.Wood;
        public int TotalGoldEarned { get; private set; }
        public bool FactionUnlocked(int faction) => faction>=0&&faction<Config.Factions.Length&&(Player.UnlockedFactions&(1<<faction))!=0;
        public bool RosterVisible(int design) => Config.Factions.Length==0||Array.IndexOf(Config.Factions[Player.Faction].Designs,design)>=0;
        public bool ChooseFaction(int faction,out string reason)
        {
            if(Finished||!Config.FactionWoodUnlocks||faction<0||faction>=Config.Factions.Length){reason="Faction choice unavailable.";return false;}
            if(!FactionUnlocked(faction)) {
                if(Config.Economy&&Wood<1){reason="Need 1 wood. Clear wave 9 to earn wood.";return false;}
                if(Config.Economy)Player.Wood--;
                Player.UnlockedFactions|=1<<faction;
            }
            Player.Faction=faction;Player.SelectedDesign=Config.Factions[faction].Designs[0];
            reason="Builder roster: "+FactionName+". Existing towers and queued orders are retained.";return true;
        }
        public int KillGold(int wave) => Config.Waves[wave].KillGold<0?Config.KillReward:Config.Waves[wave].KillGold;
        public int ClearGold(int wave) => Config.Waves[wave].ClearGold<0?Config.WaveReward:Config.Waves[wave].ClearGold;
        public readonly List<ShotEvent> Shots=new List<ShotEvent>();
        long nextShot;
        public bool DesignAvailable(int design) {
            if(Config.Factions.Length==0)return true;
            for(int f=0;f<Config.Factions.Length;f++)if(FactionUnlocked(f)&&Array.IndexOf(Config.Factions[f].Designs,design)>=0)return true;
            return false;
        }
        public string FactionName => Config.Factions.Length==0?"Free build":Config.Factions[Player.Faction].Name;
        public int SelectedDesign {get=>Player.SelectedDesign;set {if(value<0||value>=Math.Max(1,Config.Catalog.Length)||!DesignAvailable(value))throw new ArgumentOutOfRangeException();Player.SelectedDesign=value;} }
        public TowerSpec BuildSpec => Config.Catalog.Length==0?Config.Tower:Config.Catalog[SelectedDesign].Spec;
        public int BuildCost => Config.Catalog.Length==0?Config.TowerCost:Config.Catalog[SelectedDesign].Cost;
        public string BuildName => Config.Catalog.Length==0?"Bolt Spire":Config.Catalog[SelectedDesign].Name;
        public int BuildWoodCost => Config.Catalog.Length==0?0:Config.Catalog[SelectedDesign].WoodCost;
        int BuildRefund => Config.Catalog.Length==0?Config.SaleRefund:Config.Catalog[SelectedDesign].Refund;
        public int DefensesFor(WaveSpec wave)
        {
            int count=0;foreach(var tower in Grid.Towers)
                if(tower.Spec.Damage>0&&(wave.Flying?tower.Spec.TargetsAir:tower.Spec.TargetsGround))count++;
            return count;
        }
        public string WaveAdvice(int index)
        {
            var wave=PreviewWave(index);string advice=WaveAdvice(wave);
            for(int i=index;i<Config.Waves.Length;i++)if(!Config.Waves[i].Flying)return advice;
            int count=0,refund=0;
            foreach(var tower in Grid.Towers)
                if(TowerOwner(tower.Id)==ActivePlayer&&tower.Spec.Damage>0&&!tower.Spec.TargetsAir) {
                    count++;refund+=SaleRefund(tower.Id);
                }
            if(count==0)return advice;
            advice+=" Only flying waves remain. Your "+count+" ground-only weapons cannot fire; selling them would return "+refund+"g. Check flight-route coverage before launching.";
            foreach(var design in Config.Catalog)if(design.Requires!=null&&design.Requires.Length>0) {
                advice+=" Selling prerequisites can lock new champions.";break;
            }
            return advice;
        }
        public string WaveAdvice(WaveSpec wave)
        {
            string advice=wave.Flying?"Flying enemies ignore your maze.":wave.Speed>=2.6f?"Fast rush: lengthen the route and keep repeated firing coverage.":wave.SpawnInterval<=.45f&&wave.Count>=30?"Dense swarm: splash and chain attacks benefit from clustered enemies.":wave.Damage>=30?"Heavy siege: leave a route through your maze; sealed paths invite tower attacks.":"Keep an open winding route through overlapping tower ranges.";
            int choice=-1;float best=0;
            for(int i=0;i<Config.Catalog.Length;i++) {
                var d=Config.Catalog[i];var spec=d.Spec;
                if(!DesignAvailable(i)||spec.Damage<=0||!RequirementsMet(i)||!(wave.Flying?spec.TargetsAir:spec.TargetsGround))continue;
                float score=wave.Flying?(!spec.TargetsGround?100:0)+spec.Range:wave.Speed>=2.6f?spec.SlowFraction:wave.SpawnInterval<=.45f&&wave.Count>=30?spec.SplashRadius+spec.ChainTargets*.5f:0;
                if(score>best){choice=i;best=score;}
            }
            return choice<0?advice:advice+" Faction option: "+Config.Catalog[choice].Name+" ("+Config.Catalog[choice].Cost+"g).";
        }
        public int UpgradeCost(Tower tower) => (Config.Catalog.Length==0?Config.TowerCost:Config.Catalog[tower.Design].Cost)*tower.Level;
        // Read-only inspector queries ignore records retained after siege destruction.
        public int TowerOwner(int id) => Grid.Find(id)!=null&&owners.TryGetValue(id,out int owner)?owner:-1;
        public int SaleRefund(int id) => Grid.Find(id)!=null&&paidTowers.TryGetValue(id,out int refund)?refund:0;
        public bool Upgrade(int id,out string reason)
        {
            var tower=Grid.Find(id);
            if(Finished||tower==null||!owners.TryGetValue(id,out int owner)||owner!=ActivePlayer){reason="Select one of your towers.";return false;}
            if(tower.Level>=3){reason="Maximum level reached.";return false;}
            int cost=UpgradeCost(tower);
            if(Config.Economy&&Gold<cost){reason="Not enough gold.";return false;}
            if(Config.Economy){Gold-=cost;paidTowers[id]+=cost*3/4;}
            float oldHealth=tower.Spec.Health;
            tower.Spec=tower.Spec.Copy();tower.Spec.Health*=1.5f;tower.Health+=tower.Spec.Health-oldHealth;
            tower.Spec.Damage*=1.6f;tower.Spec.Range+=.35f;tower.Level++;
            reason=tower.Name+" upgraded to level "+tower.Level;return true;
        }
        int rewardCursor;
        readonly int[] waveGold, waveWood;
        int waveKilledStart,waveLeakedStart;
        public WaveSummary LastWaveSummary { get; private set; }
        void Reward(int amount) {
            if(WaveIndex>=0)TotalGoldEarned+=amount;
            for(int i=0;i<amount;i++) {
                Players[rewardCursor].Gold++;
                if(WaveIndex>=0&&LastWaveSummary==null)waveGold[rewardCursor]++;
                rewardCursor=(rewardCursor+1)%Players.Length;
            }
        }
        void SummarizeWave(bool cleared) { LastWaveSummary=new WaveSummary(WaveIndex+1,Killed-waveKilledStart,Leaked-waveLeakedStart,cleared,waveGold,waveWood); }
        public int LaneCount => Math.Max(1,Config.Lanes.Length);
        public V2 LaneSpawn(int lane) => Config.Lanes.Length==0?Config.Spawn:Config.Lanes[lane].Spawn;
        public V2[] LaneRoute(int lane,bool flying) => Config.Lanes.Length==0?(flying?Config.FlightRoute:Config.GroundRoute):(flying?Config.Lanes[lane].FlightRoute:Config.Lanes[lane].GroundRoute);
        public V2[] RouteFor(Enemy enemy) => LaneRoute(enemy.Lane,enemy.Spec.Flying);
        public int Lives => Math.Max(0, Config.StartingLives - Leaked);
        public bool Defeated => Config.Economy && Lives == 0;
        public bool Won => !Defeated && WaveIndex == Config.Waves.Length - 1 && !WaveActive && rewardedWave == WaveIndex;
        public bool Finished => Defeated || Won;
        public V2 BuilderPosition { get=>Player.Position; private set=>Player.Position=value; }
        public V2 BuilderDestination { get=>Player.Destination; private set=>Player.Destination=value; }
        public int QueuedBuilds => Player.Queue.Count+(HasBuildOrder?1:0);
        public bool HasBuildOrder { get=>Player.HasBuildOrder; private set=>Player.HasBuildOrder=value; }
        public V2 BuildOrder { get=>Player.BuildOrder; private set=>Player.BuildOrder=value; }
        public string BuilderNotice { get=>Player.Notice; private set=>Player.Notice=value; }
        int rewardedWave = -1;
        readonly Dictionary<int, int> paidTowers = new Dictionary<int, int>();
        public void MoveBuilder(V2 destination)
        {
            if (Finished) return;
            HasBuildOrder = false;Player.Queue.Clear();
            BuilderDestination = new V2(Geometry.Clamp(destination.X, .5f, Config.Width - .5f), Geometry.Clamp(destination.Y, .5f, Config.Height - .5f));
            BuilderNotice = "Moving. Previous build order cancelled.";
        }
        public bool OrderBuild(float x, float y, out string reason, bool append=false)
        {
            if (!Config.BuilderEnabled) return Build(x, y, out reason);
            if (!CanBuild(x, y, out reason)) return false;
            if(append&&HasBuildOrder) {
                if(BuildOrder.X==x&&BuildOrder.Y==y){reason="That cell is already ordered.";return false;}
                foreach(var task in Player.Queue)if(task.X==x&&task.Y==y){reason="That cell is already queued.";return false;}
                if(Player.Queue.Count>=128){reason="Build queue is full.";return false;}
                Player.Queue.Enqueue(new BuildTask{X=x,Y=y,Design=SelectedDesign});
                BuilderNotice=reason="Build queued. Orders are paid on completion.";return true;
            }
            Player.Queue.Clear();
            Player.OrderedDesign=SelectedDesign;
            BuildOrder = new V2(x, y);
            HasBuildOrder = true;
            BuilderDestination = new V2(x + BuildSpec.Width * .5f, y + BuildSpec.Height * .5f);
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
                int selected=SelectedDesign;SelectedDesign=Player.OrderedDesign;
                bool built=Build(BuildOrder.X, BuildOrder.Y, out string message);
                SelectedDesign=selected;
                BuilderNotice = built ? message : "Skipped order: " + message;
                BuilderDestination = BuilderPosition;
                // Advance without reordering or changing the user's toolbar selection.
                if(Player.Queue.Count>0) {
                    var task=Player.Queue.Dequeue();Player.OrderedDesign=task.Design;BuildOrder=new V2(task.X,task.Y);HasBuildOrder=true;
                    var spec=Config.Catalog.Length==0?Config.Tower:Config.Catalog[task.Design].Spec;
                    BuilderDestination=new V2(task.X+spec.Width*.5f,task.Y+spec.Height*.5f);
                }
            }
        }
        readonly float[] spawnTimers;
        readonly int[] lanePending;
        readonly Dictionary<int,int> owners=new Dictionary<int,int>();
        readonly MatchOptions options;
        WaveSpec currentWave;
        public World Restart() => new World(Config,options);
        int nextEnemy = 1;
        public World(Scenario config, MatchOptions matchOptions=null)
        {
            config.Validate();
            Config = config;
            var selected=matchOptions??new MatchOptions(); selected.Validate(config.BuilderStarts.Length==0?4:config.BuilderStarts.Length);
            options=new MatchOptions {PlayerCount=selected.PlayerCount,AutomaticWaves=selected.AutomaticWaves,UseSelectedSoloStart=selected.UseSelectedSoloStart,Difficulty=selected.Difficulty,StartingPositions=(int[])selected.StartingPositions.Clone(),Factions=(int[])selected.Factions.Clone()};
            Difficulty=options.Difficulty;
            Players=new PlayerState[options.PlayerCount];
            for(int i=0;i<Players.Length;i++) {
                var pos=config.Lanes.Length==0?config.Spawn:Players.Length==1&&!options.UseSelectedSoloStart?config.SoloBuilderStart:config.BuilderStarts[options.StartingPositions[i]];
                int faction=options.Factions[i];
                if(config.Factions.Length>0&&(faction<0||faction>=config.Factions.Length))throw new ArgumentException("Invalid faction.");
                Players[i]=new PlayerState {Position=pos,Destination=pos,Faction=faction,UnlockedFactions=1<<faction,SelectedDesign=config.Factions.Length==0?0:config.Factions[faction].Designs[0]};
            }
            waveGold=new int[Players.Length];waveWood=new int[Players.Length];
            Reward(config.StartingGold);
            spawnTimers=new float[LaneCount]; lanePending=new int[LaneCount];
            Grid = new MazeGrid(config.Width, config.Height);
            foreach(var block in config.Terrain)Grid.AddTerrain(block);
            Navigation = new FlowNavigation(Grid, config.NavigationStep, config.BreachCost);
        }
        public bool StartWave()
        {
            if (Finished || WaveActive || WaveIndex + 1 >= Config.Waves.Length)
                return false;
            NextWaveTick=-1;
            WaveIndex++;
            LastWaveSummary=null;Array.Clear(waveGold,0,waveGold.Length);Array.Clear(waveWood,0,waveWood.Length);
            waveKilledStart=Killed;waveLeakedStart=Leaked;
            currentWave=PreviewWave(WaveIndex);
            Pending=currentWave.Count*LaneCount;
            for(int lane=0;lane<LaneCount;lane++){lanePending[lane]=currentWave.Count;spawnTimers[lane]=0;}
            return true;
        }
        // A fresh copy keeps previews, difficulty scaling and actual spawns in agreement.
        public WaveSpec PreviewWave(int index)
        {
            if(index<0||index>=Config.Waves.Length)throw new ArgumentOutOfRangeException(nameof(index));
            var source=Config.Waves[index];
            float factor=Difficulty==Difficulty.Relaxed?.7f:Difficulty==Difficulty.Hard?1.4f:1f;
            return new WaveSpec {KillGold=source.KillGold,ClearGold=source.ClearGold,WoodReward=source.WoodReward,Name=source.Name,Count=source.Count,Health=source.Health*factor,Damage=source.Damage*factor,Speed=source.Speed,Radius=source.Radius,SpawnInterval=source.SpawnInterval,AttackInterval=source.AttackInterval,Flying=source.Flying};
        }
        public IEnumerable<int> MissingPrerequisites(int design)
        {
            if(Config.Catalog.Length==0)yield break;
            foreach(int required in Config.Catalog[design].Requires??Array.Empty<int>()) {
                bool found=false;
                foreach(var tower in Grid.Towers)
                    if(tower.Design==required&&owners.TryGetValue(tower.Id,out int owner)&&owner==ActivePlayer){found=true;break;}
                if(!found)yield return required;
            }
        }
        public bool RequirementsMet(int design)
        {
            foreach(int missing in MissingPrerequisites(design))return false;
            return true;
        }
        public float PlacementStep => Config.LayoutRows.Length>0?Config.LayoutCellSize:1f;
        public V2 SnapBuildOrigin(V2 point) => new V2((float)Math.Floor(point.X/PlacementStep)*PlacementStep,(float)Math.Floor(point.Y/PlacementStep)*PlacementStep);
        public bool CanBuild(float x, float y, out string reason)
        {
            if(float.IsNaN(x)||float.IsNaN(y)||float.IsInfinity(x)||float.IsInfinity(y)||Math.Abs(x/PlacementStep-Math.Round(x/PlacementStep))>.0001||Math.Abs(y/PlacementStep-Math.Round(y/PlacementStep))>.0001){reason="Align the tower to the placement grid.";return false;}
            if (Finished) { reason = "Match finished. Reset to play again."; return false; }
            if(!RequirementsMet(SelectedDesign)){reason="Build each regular tower in your faction before its champion.";return false;}
            if(Config.Economy&&Wood<BuildWoodCost){reason="Need "+BuildWoodCost+" wood. Clear wave 14 to earn wood.";return false;}
            if (Config.Economy && Gold < BuildCost) { reason = "Not enough gold."; return false; }
            var spec = BuildSpec;
            var center = new V2(x + spec.Width * 0.5f, y + spec.Height * 0.5f);
            var half = new V2(spec.Width * 0.5f - (1 - spec.Fill) * 0.5f, spec.Height * 0.5f - (1 - spec.Fill) * 0.5f);
            float protection = Config.NavigationStep * 1.5f;
            foreach (var wave in Config.Waves)
                protection = Math.Max(protection, wave.Radius);
            for(int lane=0;lane<LaneCount;lane++)
            if (Geometry.PointBox(LaneSpawn(lane), center, half) < protection)
            {
                reason = "Keep the spawn clear.";
                return false;
            }
            for(int lane=0;lane<LaneCount;lane++) {
                var route=LaneRoute(lane,false);
                if (Geometry.PointBox(route[route.Length-1], center, half) < protection) {
                    reason = "Keep the exit clear.";
                    return false;
                }
            }
            foreach (var e in Enemies)
                if (!e.Spec.Flying && Geometry.PointBox(e.Position, center, half) < e.Spec.Radius)
                {
                    reason = "An enemy occupies this footprint.";
                    return false;
                }
            if(Grid.TerrainOverlaps(x,y,spec.Width,spec.Height)){reason="Terrain cannot be built on.";return false;}
            bool occupied = x < 0 || y < 0 || x + spec.Width > Grid.Width || y + spec.Height > Grid.Height;
            foreach(var tower in Grid.Towers)occupied |= x<tower.CellX+tower.Spec.Width&&x+spec.Width>tower.CellX&&y<tower.CellY+tower.Spec.Height&&y+spec.Height>tower.CellY;
            if (occupied)
            {
                reason = "Outside map or occupied footprint.";
                return false;
            }
            reason = "Placement valid.";
            return true;
        }
        public bool Build(float x, float y, out string reason)
        {
            if (!CanBuild(x, y, out reason)) return false;
            var center = new V2(x + BuildSpec.Width * .5f, y + BuildSpec.Height * .5f);
            if (Config.BuilderEnabled && V2.Distance(BuilderPosition, center) > Config.BuildRange)
            { reason = "Builder out of range. Issue a build order."; return false; }
            var tower = Grid.Build(x, y, BuildSpec.Copy());
            if (tower == null) { reason = "Invalid tower footprint."; return false; }
            tower.Design=SelectedDesign;tower.Name=BuildName;owners[tower.Id]=ActivePlayer;
            if (Config.Economy) { Gold -= BuildCost; Player.Wood-=BuildWoodCost; paidTowers[tower.Id] = BuildRefund; }
            reason = "Tower built. Complete route blockage is allowed.";
            return true;
        }
        public bool Sell(float x, float y)
        {
            if (Finished) return false;
            var t = Grid.At(x, y);
            if (t == null || owners.TryGetValue(t.Id,out int owner)&&owner!=ActivePlayer || !Grid.Remove(t.Id)) return false;
            if (paidTowers.TryGetValue(t.Id, out int refund)) { Gold += refund; if(Config.Catalog.Length>0)Player.Wood+=Config.Catalog[t.Design].WoodCost; paidTowers.Remove(t.Id); }
            return true;
        }
        public Enemy Spawn(WaveSpec spec, V2 position, int lane=0)
        {
            if (!spec.Flying && !Grid.Clear(position, position, spec.Radius))
                return null;
            foreach (var other in Enemies)
                if (other.Spec.Flying == spec.Flying && V2.Distance(position, other.Position) < spec.Radius + other.Spec.Radius + 0.02f)
                    return null;
            var e = new Enemy { Id = nextEnemy++, Position = position, Lane=lane, Spec = spec, Health = spec.Health };
            Enemies.Add(e);
            return e;
        }
        float CheckpointRegion(Enemy enemy,V2[] route)
        {
            // Intermediate route hints are areas when a tower covers their exact point.
            // Terrain still separates areas; the terminal exit remains exact and protected.
            return !enemy.Spec.Flying && enemy.Checkpoint<route.Length-1
                && !Grid.Clear(enemy.Destination,enemy.Destination,enemy.Spec.Radius)
                ? 1.25f+enemy.Spec.Radius : 0;
        }
        public void Step()
        {
            if (Finished) return;
            Tick++;
            if(CountingDown&&Tick>=NextWaveTick)StartWave();
            int active=ActivePlayer;
            for(int i=0;i<Players.Length;i++){ActivePlayer=i;StepBuilder();}
            ActivePlayer=active;
            for(int lane=0;lane<LaneCount;lane++)
                if(lanePending[lane]>0) {
                    spawnTimers[lane]-=FixedDelta;
                    if(spawnTimers[lane]<=0&&Spawn(currentWave,LaneSpawn(lane),lane)!=null) {
                        lanePending[lane]--;Pending--;spawnTimers[lane]=currentWave.SpawnInterval;
                    }
                }
            foreach (var tower in Grid.Towers)
            {
                tower.Cooldown = Math.Max(0, tower.Cooldown - FixedDelta);
                tower.LastTarget = 0;
                if (!TowersFire || tower.Spec.Damage<=0 || tower.Cooldown > 0)
                    continue;
                foreach (var e in Enemies)
                    if (e.Health > 0 && (e.Spec.Flying ? tower.Spec.TargetsAir : tower.Spec.TargetsGround) && V2.Distance(tower.Center, e.Position) <= tower.Spec.Range)
                    {
                        Hit(e,tower.Spec);
                        if(tower.Spec.SplashRadius>0)foreach(var other in Enemies)
                            if(other!=e&&other.Health>0&&other.Spec.Flying==e.Spec.Flying&&V2.Distance(other.Position,e.Position)<=tower.Spec.SplashRadius)Hit(other,tower.Spec);
                        if(tower.Spec.ChainTargets>0) {
                            int left=tower.Spec.ChainTargets;
                            foreach(var other in Enemies)if(other!=e&&other.Health>0&&(other.Spec.Flying?tower.Spec.TargetsAir:tower.Spec.TargetsGround)&&V2.Distance(other.Position,e.Position)<=2) {
                                Hit(other,tower.Spec);Shots.Add(new ShotEvent{Serial=++nextShot,Design=tower.Design,From=e.Position,To=other.Position,Flying=other.Spec.Flying,Chained=true,FromFlying=e.Spec.Flying});if(--left==0)break;
                            }
                        }
                        Shots.Add(new ShotEvent {Serial=++nextShot,Design=tower.Design,From=tower.Center,To=e.Position,Splash=tower.Spec.SplashRadius,Flying=e.Spec.Flying});
                        if(Shots.Count>128)Shots.RemoveRange(0,Shots.Count-128);
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
                var route = RouteFor(e);
                e.Destination = route[e.Checkpoint];
                float goalRegion=CheckpointRegion(e,route);
                if (V2.Distance(e.Position, e.Destination) < (goalRegion>0?goalRegion:Config.CheckpointRadius+e.Spec.Radius)
                    && (e.Spec.Flying || (goalRegion>0?Grid.TerrainClear(e.Position,e.Destination,e.Spec.Radius):Grid.Clear(e.Position,e.Destination,e.Spec.Radius))))
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
                    goalRegion=CheckpointRegion(e,route);
                }
                V2 aim = e.Destination;
                e.Blocked = false;
                e.BlockerId = 0;
                if (!e.Spec.Flying)
                {
                    var field = Navigation.Get(e.Destination, e.Spec.Radius, false, goalRegion);
                    aim = Navigation.Waypoint(field, e.Position, out bool reachable, out _);
                    if (!reachable)
                    {
                        e.Blocked = true;
                        var breach = Navigation.Get(e.Destination, e.Spec.Radius, true, goalRegion);
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
                                    e.LastAttackTick=Tick;
                                    e.AttackDirection=(blocker.Center-e.Position).Normalized;
                                    Grid.Damage(blocker.Id, e.Spec.Damage);
                                    e.AttackCooldown = e.Spec.AttackInterval;
                                }
                            }
                        }
                    }
                }
                var toAim = aim - e.Position;
                e.IntendedDirection = toAim.Normalized;
                e.SlowRemaining=Math.Max(0,e.SlowRemaining-FixedDelta);
                float speed=e.Spec.Speed*(e.SlowRemaining>0?1-e.SlowFraction:1);
                V2 desired = e.IntendedDirection * speed;
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
                if (desired.Length > speed)
                    desired = desired.Normalized * speed;
                if(e.DetourTicks>0&&!e.Blocked){desired=e.DetourDirection*speed;e.DetourTicks--;}
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
                        if (Config.Economy) Reward(WaveIndex>=0?KillGold(WaveIndex):Config.KillReward);
                    }
                    Enemies.RemoveAt(i);
                }
            if (!WaveActive && WaveIndex > rewardedWave && !Defeated)
            {
                rewardedWave = WaveIndex;
                if (Config.Economy) {
                    Reward(ClearGold(WaveIndex));
                    for(int i=0;i<Config.Waves[WaveIndex].WoodReward;i++) {int p=i%Players.Length;Players[p].Wood++;waveWood[p]++;}
                }
                SummarizeWave(true);
                if(options.AutomaticWaves&&WaveIndex+1<Config.Waves.Length)NextWaveTick=Tick+IntermissionTicks;
            }
            if(Defeated&&WaveIndex>=0&&LastWaveSummary==null)SummarizeWave(false);
            if (Finished) foreach(var player in Players){player.HasBuildOrder=false;player.Queue.Clear();}
        }
        static void Hit(Enemy enemy,TowerSpec spec)
        {
            enemy.Health-=spec.Damage;
            if(spec.SlowFraction>0&&(enemy.SlowRemaining<=0||spec.SlowFraction>=enemy.SlowFraction)) {enemy.SlowFraction=spec.SlowFraction;enemy.SlowRemaining=spec.SlowDuration;}
        }
        void Move(Enemy e, V2 delta)
        {
            // Short substeps prevent tunnelling even at high configured speed.
            int steps = Math.Max(1, (int)Math.Ceiling(delta.Length / Math.Max(0.02f, e.Spec.Radius * 0.5f)));
            var step = delta / steps;
            var start = e.Position;
            for (int n = 0; n < steps; n++)
            {
                var before=e.Position;
                if (CanMove(e, e.Position + step))
                    e.Position += step;
                else if (CanMove(e, e.Position + new V2(step.X, 0)))
                    e.Position += new V2(step.X, 0);
                else if (CanMove(e, e.Position + new V2(0, step.Y)))
                    e.Position += new V2(0, step.Y);
                // Contact with a neighbour at a corner can defeat both axis slides.
                // Try deterministic side steps and brief backsteps, using swept collision checks.
                // A truly blocked route retains the normal siege behaviour.
                if(!e.Blocked&&V2.Distance(before,e.Position)<.000001f&&step.Length>.000001f) {
                    var tangent=new V2(-step.Y,step.X);int preferred=e.Id%2==0?1:-1;
                    for(int attempt=0;attempt<7;attempt++) {
                        int side=attempt%2==0?preferred:-preferred;
                        var candidate=attempt<2?step*.5f+tangent*(side*.8660254f):attempt<4?tangent*side:attempt<6?step*-.5f+tangent*(side*.8660254f):step*-1;
                        if(CanMove(e,e.Position+candidate)&&CanMove(e,e.Position+candidate.Normalized*(e.Spec.Radius*.5f))){e.Position+=candidate;e.DetourDirection=candidate.Normalized;e.DetourTicks=20;break;}
                    }
                }
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
