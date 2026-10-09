using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using FrostMaze.Simulation;
using FrostMaze.Tests;

// Reproducible baseline, not a skilled-player substitute. Uses normal builder orders and wallets.
static partial class BalanceSweep
{
    sealed class Sample { public V2 Position; public bool Air; public float Coverage,Slow; }
    sealed class Placement { public int TowerId{get;set;} public int TravelTicks{get;set;} public int BeforeWave{get;set;} public int Player{get;set;} public int X{get;set;} public int Y{get;set;} public string Tower{get;set;} public int Cost{get;set;} }
    sealed class SaleResult { public int Player{get;set;} public int TowerId{get;set;} public string Tower{get;set;} public int Level{get;set;} public int Refund{get;set;} public int BeforeWave{get;set;} }
    sealed class UpgradeResult { public int Player{get;set;} public int TowerId{get;set;} public string Tower{get;set;} public int Level{get;set;} public int Cost{get;set;} public int BeforeWave{get;set;} }
    sealed class WaveResult { public bool SummaryPresent{get;set;} public bool Cleared{get;set;} public int TeamIncome{get;set;} public int[] PlayerIncome{get;set;} public int[] PlayerGold{get;set;} public int[] PlayerWood{get;set;} public int Wave{get;set;} public bool Flying{get;set;} public int Killed{get;set;} public int Leaked{get;set;} public int Ticks{get;set;} public int Gold{get;set;} }
    sealed class Result { public int[] KillRewards{get;set;} public int[] ClearRewards{get;set;} public System.Collections.Generic.List<string> WoodPurchases{get;set;}=new System.Collections.Generic.List<string>(); public int[] OpenRouteTicks{get;set;} public int[] PreparedMazeRouteTicks{get;set;} public int InitialMazePurchases{get;set;} public int StartingTeamGold{get;set;} public int KillReward{get;set;} public int WaveReward{get;set;} public List<SaleResult> Sales{get;set;}=new List<SaleResult>(); public int Refunded{get;set;} public int[] PlayerRefunds{get;set;} public int[] StartingPositions{get;set;} public float[][] InitialBuilderPositions{get;set;} public int ActiveLanes{get;set;} public int[] StartingWallets{get;set;} public int PreparationTicks{get;set;} public int[] FinalWallets{get;set;} public int[] PlayerSpending{get;set;} public string Strategy{get;set;} public string[] Factions{get;set;} public List<UpgradeResult> Upgrades{get;set;}=new List<UpgradeResult>(); public int PlayerCount{get;set;} public string Map{get;set;} public string Faction{get;set;} public string Difficulty{get;set;} public bool Won{get;set;} public bool Stalled{get;set;} public int Lives{get;set;} public int Gold{get;set;} public int Spent{get;set;} public List<Placement> Placements{get;set;}=new List<Placement>(); public List<WaveResult> Waves{get;set;}=new List<WaveResult>(); }
    sealed class Candidate { public int X,Y; public int[] Samples; }
    static readonly HashSet<(int x,int y)> keptOpen=new HashSet<(int,int)>();
    static readonly Dictionary<TowerSpec,List<Candidate>> influence=new Dictionary<TowerSpec,List<Candidate>>();
    static List<Candidate> Candidates(World w,TowerSpec spec,List<Sample> samples)
    {
        if(influence.TryGetValue(spec,out var cached))return cached;
        cached=new List<Candidate>();
        for(int y=1;y<w.Config.Height-1;y+=2)for(int x=1;x<w.Config.Width-1;x+=2) {
            if(keptOpen.Contains((x,y))||w.Grid.TerrainOverlaps(x,y,spec.Width,spec.Height))continue;
            var affected=new List<int>();var p=new V2(x+.5f,y+.5f);
            for(int i=0;i<samples.Count;i++)if((samples[i].Air?spec.TargetsAir:spec.TargetsGround)&&V2.Distance(p,samples[i].Position)<spec.Range)affected.Add(i);
            cached.Add(new Candidate{X=x,Y=y,Samples=affected.ToArray()});
        }
        influence.Add(spec,cached);return cached;
    }
    static List<Sample> Samples(Scenario c,World defense=null,List<int> travelTicks=null)
    {
        influence.Clear();
        var result=new List<Sample>();
        for(int lane=0;lane<c.Lanes.Length;lane++)foreach(bool air in new[]{false,true}) {
            var w=new World(c);w.TowersFire=false;
            // Geometry-only probe copies, never free construction in the paid campaign.
            if(defense!=null)foreach(var tower in defense.Grid.Towers)w.Grid.Build(tower.CellX,tower.CellY,tower.Spec);
            w.Spawn(new WaveSpec{Flying=air},w.LaneSpawn(lane),lane);
            int tick=0;
            for(;tick<9000&&w.Enemies.Count>0;tick++) {
                if(tick%30==0)result.Add(new Sample{Position=w.Enemies[0].Position,Air=air});w.Step();
                if(defense!=null&&w.Enemies.Any(e=>e.Blocked))throw new Exception("Prepared shared maze triggered siege.");
            }
            if(w.Leaked!=1)throw new Exception("Cannot sample route");
            travelTicks?.Add(tick);
        }
        return result;
    }
    static bool Purchase(World w,int x,int y,int design,Result result)
    {
        w.SelectedDesign=design;int before=w.Gold,count=w.Grid.Towers.Count;
        if(!w.OrderBuild(x,y,out _))return false;
        int travelTicks=0;
        while(travelTicks<1000&&w.HasBuildOrder){w.Step();travelTicks++;}
        result.PreparationTicks+=travelTicks;
        if(w.HasBuildOrder||w.Grid.Towers.Count!=count+1)throw new Exception("Builder failed a legal purchase");
        int cost=before-w.Gold;if(cost!=w.Config.Catalog[design].Cost)throw new Exception("Purchase accounting mismatch");
        result.Spent+=cost;result.Placements.Add(new Placement{TravelTicks=travelTicks,TowerId=w.Grid.Towers[w.Grid.Towers.Count-1].Id,BeforeWave=w.WaveIndex+2,Player=w.ActivePlayer+1,X=x,Y=y,Tower=w.BuildName,Cost=cost});return true;
    }
    static float ScoredDps(TowerSpec spec,bool roles)
    {
        float direct=spec.Damage/spec.Interval;
        // Conservative bot estimates, not promised combat DPS. Crowd geometry decides real results.
        return roles?direct*(1+Math.Min(2,spec.SplashRadius)+.6f*spec.ChainTargets)/(1-.5f*spec.SlowFraction):direct;
    }
    static void Spend(World w,List<Sample> samples,Result result,int towerLimit=int.MaxValue,bool valueSlots=false,bool roles=false,bool support=false)
    {
        // Whole-map route coverage, diminishing returns. No teleporting, free towers, or balance overrides.
        // Compact-value weights scarce tower slots as well as gold; this is a bot heuristic, not game tuning.
        foreach(var sample in samples) {
            sample.Coverage=0;sample.Slow=0;
            foreach(var t in w.Grid.Towers)if((sample.Air?t.Spec.TargetsAir:t.Spec.TargetsGround)&&V2.Distance(t.Center,sample.Position)<t.Spec.Range)
                { sample.Coverage+=ScoredDps(t.Spec,roles);sample.Slow=Math.Max(sample.Slow,t.Spec.SlowFraction); }
        }
        int owned=Owned(w,result).Count();
        for(int purchase=0;purchase<100&&owned<towerLimit;purchase++) {
            double best=0;int bx=-1,by=-1,bd=-1;
            foreach(int design in w.Config.Factions[w.Players[w.ActivePlayer].Faction].Designs) {
                var d=w.Config.Catalog[design];if(d.Spec.Damage<=0||d.Cost>w.Gold||!w.RequirementsMet(design))continue;
                w.SelectedDesign=design;
                var weights=new double[samples.Count];
                for(int i=0;i<samples.Count;i++) {
                    float power=ScoredDps(d.Spec,roles);
                    // Slowing extends other towers' firing time, but repeated slows do not stack.
                    if(support)power+=samples[i].Coverage*Math.Max(0,d.Spec.SlowFraction-samples[i].Slow)*.5f/(1-d.Spec.SlowFraction);
                    weights[i]=(samples[i].Air?1.5:1)*Math.Log(1+power/(10+samples[i].Coverage));
                }
                foreach(var candidate in Candidates(w,d.Spec,samples)) {
                    int x=candidate.X,y=candidate.Y;if(!w.CanBuild(x,y,out _))continue;
                    double score=0;foreach(int index in candidate.Samples)score+=weights[index];
                    score/=valueSlots?Math.Sqrt(d.Cost):d.Cost;
                    if(score>best){best=score;bx=x;by=y;bd=design;}
                }
            }
            if(bd<0||!Purchase(w,bx,by,bd,result))break;
            owned++;var spec=w.Config.Catalog[bd].Spec;
            foreach(var sample in samples)if((sample.Air?spec.TargetsAir:spec.TargetsGround)&&V2.Distance(new V2(bx+.5f,by+.5f),sample.Position)<spec.Range){sample.Coverage+=ScoredDps(spec,roles);sample.Slow=Math.Max(sample.Slow,spec.SlowFraction);}
        }
    }
    static IEnumerable<Tower> Owned(World w,Result result)
    {
        foreach(var p in result.Placements)if(p.Player==w.ActivePlayer+1) {
            var tower=w.Grid.Find(p.TowerId);if(tower!=null)yield return tower;
        }
    }
    static void SpendRoster(World w,List<Sample> samples,Result result)
    {
        // Buy one of each design, unlock the champion, then upgrade strongest weapons first.
        // Savings are intentional: this strategy waits for its next roster purchase/upgrade.
        foreach(int design in w.Config.Factions[w.Players[w.ActivePlayer].Faction].Designs) {
            if(Owned(w,result).Any(t=>t.Design==design))continue;
            if(w.Config.Catalog[design].WoodCost>w.Wood)continue;
            var d=w.Config.Catalog[design];
            if(d.Cost>w.Gold||!w.RequirementsMet(design))return;
            w.SelectedDesign=design;double best=-1;int bx=-1,by=-1;
            for(int y=1;y<w.Config.Height-1;y+=2)for(int x=1;x<w.Config.Width-1;x+=2) {
                if(!w.CanBuild(x,y,out _))continue;
                var p=new V2(x+.5f,y+.5f);double score=0;
                foreach(var sample in samples)if((sample.Air?d.Spec.TargetsAir:d.Spec.TargetsGround)&&V2.Distance(p,sample.Position)<d.Spec.Range)
                    score+=sample.Air?1.5:1;
                if(score>best){best=score;bx=x;by=y;}
            }
            if(bx<0||!Purchase(w,bx,by,design,result))return;
        }
        foreach(var tower in Owned(w,result).OrderByDescending(t=>t.Spec.Damage/t.Spec.Interval).ToArray())
            while(tower.Level<3) {
                int cost=w.UpgradeCost(tower);if(cost>w.Gold)return;
                int before=w.Gold;
                if(!w.Upgrade(tower.Id,out string reason))throw new Exception("Roster upgrade rejected: "+reason);
                if(before-w.Gold!=cost)throw new Exception("Upgrade accounting mismatch");
                result.Spent+=cost;
                result.Upgrades.Add(new UpgradeResult{Player=w.ActivePlayer+1,TowerId=tower.Id,Tower=tower.Name,Level=tower.Level,Cost=cost,BeforeWave=w.WaveIndex+2});
            }
        Spend(w,samples,result);
    }
    static void SpendAdaptive(World w,List<Sample> samples,Result result,int towerLimit=int.MaxValue,bool valueSlots=false,bool roles=false,bool support=false)
    {
        Spend(w,samples,result,towerLimit,valueSlots,roles,support);
        // When sampled building sites fill up, reinvest leftover gold instead of hoarding it.
        // Prefer affordable upgrades with actual route exposure; never wait for one expensive item.
        while(true) {
            Tower best=null;double value=0;
            foreach(var tower in Owned(w,result)) {
                int cost=w.UpgradeCost(tower);if(tower.Level>=3||tower.Spec.Damage<=0||cost>w.Gold)continue;
                double exposure=0;foreach(var sample in samples)if((sample.Air?tower.Spec.TargetsAir:tower.Spec.TargetsGround)&&V2.Distance(tower.Center,sample.Position)<tower.Spec.Range+.35f)exposure+=sample.Air?1.5:1;
                double score=roles?exposure*ScoredDps(tower.Spec,true)*.6/cost:exposure*tower.Spec.Damage/tower.Spec.Interval*.6/cost;
                if(score>value){value=score;best=tower;}
            }
            if(best==null)break;int price=w.UpgradeCost(best),before=w.Gold;
            if(!w.Upgrade(best.Id,out string reason)||before-w.Gold!=price)throw new Exception("Adaptive upgrade failed: "+reason);
            result.Spent+=price;result.Upgrades.Add(new UpgradeResult{Player=w.ActivePlayer+1,TowerId=best.Id,Tower=best.Name,Level=best.Level,Cost=price,BeforeWave=w.WaveIndex+2});
        }
    }
    static int[] AuditWallets(World w,Result result,int completedWaves)
    {
        // Rewards rotate across wallets continuously, including the initial team grant.
        // Sales return money only to their owner; they do not advance the shared reward cursor.
        int grants=w.Config.StartingGold+w.TotalGoldEarned;
        var wallets=new int[w.Players.Length];var spending=new int[wallets.Length];var refunds=new int[wallets.Length];
        for(int player=0;player<wallets.Length;player++) {
            spending[player]=result.Placements.Where(p=>p.Player==player+1).Sum(p=>p.Cost)+result.Upgrades.Where(u=>u.Player==player+1).Sum(u=>u.Cost);
            refunds[player]=result.Sales.Where(s=>s.Player==player+1).Sum(s=>s.Refund);
            int expected=grants/wallets.Length+(player<grants%wallets.Length?1:0)-spending[player]+refunds[player];
            wallets[player]=w.Players[player].Gold;
            if(wallets[player]<0||wallets[player]!=expected)throw new Exception("Player "+(player+1)+" wallet ledger mismatch");
        }
        result.PlayerSpending=spending;result.PlayerRefunds=refunds;return wallets;
    }
    static int TeamGold(World w) { int total=0;foreach(var p in w.Players)total+=p.Gold;return total; }
    public static int Run(string path,Difficulty difficulty,int players,string strategy="coverage",bool mixed=false,string mapFilter="",int factionFilter=-1,int[] startingPositions=null)
    {
        bool woodProgression=strategy=="wood-progression";if(woodProgression)strategy="compact-transition";
        if(strategy!="compact-shared-transition"&&strategy!="compact-shared-maze"&&strategy!="coverage"&&strategy!="roster"&&strategy!="maze"&&strategy!="adaptive"&&strategy!="compact"&&strategy!="compact-value"&&strategy!="compact-roles"&&strategy!="compact-support"&&strategy!="compact-invest"&&strategy!="compact-transition")throw new ArgumentException("Strategy must be coverage, roster, maze, adaptive, compact, compact-value, compact-roles, compact-support, compact-invest, compact-transition, compact-shared-maze or compact-shared-transition.");
        if((strategy=="compact-shared-maze"||strategy=="compact-shared-transition")&&mapFilter!="Rimewatch")throw new ArgumentException("The shared exit maze diagnostic requires the Rimewatch map filter.");
        if(players<1||players>4)throw new ArgumentException("Player count must be 1–4.");
        if(!Enum.IsDefined(typeof(Difficulty),difficulty))throw new ArgumentException("Unknown difficulty.");
        if(mapFilter!=""&&mapFilter!="Rimewatch"&&mapFilter!="Ironfold")throw new ArgumentException("Unknown map filter.");
        if(factionFilter < -1)throw new ArgumentException("Invalid faction filter.");
        // Reject the complete request before running campaigns or writing a partial sweep.
        foreach(bool iron in new[]{false,true}) {
            var c=MapCases.Load(iron);if(mapFilter.Length>0&&c.Name!=mapFilter)continue;
            if(factionFilter>=c.Factions.Length)throw new ArgumentException("Faction is outside the selected map roster.");
            if(startingPositions!=null) {
                if(startingPositions.Length!=players)throw new ArgumentException("Provide exactly one start index per player.");
                new MatchOptions{PlayerCount=players,StartingPositions=startingPositions}.Validate(c.BuilderStarts.Length);
            }
        }
        var results=new List<Result>();
        foreach(bool iron in new[]{false,true}) {
            var c=MapCases.Load(iron);if(mapFilter.Length>0&&c.Name!=mapFilter)continue;var openTicks=new List<int>();var openSamples=Samples(c,null,openTicks);
            for(int faction=0;faction<c.Factions.Length;faction++) {
                if(factionFilter>=0&&faction!=factionFilter)continue;
                keptOpen.Clear();var samples=openSamples;
                influence.Clear(); // A previous faction may have sampled only the terminal flying route.
                var factions=new int[4];for(int player=0;player<4;player++)factions[player]=mixed?(faction+player)%c.Factions.Length:faction;
                var options=new MatchOptions{PlayerCount=players,Difficulty=difficulty,Factions=factions,AutomaticWaves=false};
                if(startingPositions!=null){options.StartingPositions=(int[])startingPositions.Clone();options.UseSelectedSoloStart=true;}
                var w=new World(c,options);
                var restarted=w.Restart();
                for(int player=0;player<players;player++) {
                    V2 expected=players==1&&startingPositions==null?c.SoloBuilderStart:c.BuilderStarts[options.StartingPositions[player]];
                    if(V2.Distance(w.Players[player].Position,expected)!=0||V2.Distance(restarted.Players[player].Position,expected)!=0)
                        throw new Exception("Chosen builder start was not preserved at spawn/restart.");
                    if(w.Players[player].Gold!=c.StartingGold/players+(player<c.StartingGold%players?1:0)||w.Players[player].Faction!=factions[player])
                        throw new Exception("Initial wallet or faction mismatch.");
                }
                var r=new Result{KillRewards=Enumerable.Range(0,c.Waves.Length).Select(w.KillGold).ToArray(),ClearRewards=Enumerable.Range(0,c.Waves.Length).Select(w.ClearGold).ToArray(),StartingTeamGold=c.StartingGold,KillReward=c.KillReward,WaveReward=c.WaveReward,StartingPositions=players==1&&startingPositions==null?new[]{-1}:options.StartingPositions.Take(players).ToArray(),InitialBuilderPositions=w.Players.Select(p=>new[]{p.Position.X,p.Position.Y}).ToArray(),ActiveLanes=w.LaneCount,StartingWallets=w.Players.Select(p=>p.Gold).ToArray(),Strategy=woodProgression?"wood-progression":strategy,Factions=factions.Take(players).Select(f=>c.Factions[f].Name).ToArray(),PlayerCount=players,Map=c.Name,Faction=c.Factions[faction].Name,Difficulty=difficulty.ToString()};
                if(strategy=="compact-shared-maze"||strategy=="compact-shared-transition") {
                    var cells=MapCases.SharedExitMazeCells();
                    for(int cell=0;cell<cells.GetLength(0);cell++) {
                        w.SelectPlayer(cell%players);int design=MapCases.MazeDesign(w);
                        if(!Purchase(w,cells[cell,0],cells[cell,1],design,r))throw new Exception("Paid shared exit maze could not be built.");
                    }
                    // Planner-only keep-out: preserve this fixture's neck and alternating gaps.
                    // Normal players remain free to build here; game placement rules do not change.
                    for(int y=8;y<=13;y++)for(int x=28;x<=33;x++)keptOpen.Add((x,y));
                    var mazeTicks=new List<int>();samples=Samples(c,w,mazeTicks);
                    r.InitialMazePurchases=cells.GetLength(0);r.OpenRouteTicks=openTicks.ToArray();r.PreparedMazeRouteTicks=mazeTicks.ToArray();
                    for(int lane=0;lane<w.LaneCount;lane++)
                        if(mazeTicks[lane*2]<=openTicks[lane*2]+30||mazeTicks[lane*2+1]!=openTicks[lane*2+1])throw new Exception("Shared maze failed ground detour / unchanged flight check.");
                }
                if(strategy=="maze") {
                    int design=MapCases.MazeDesign(w);var cells=MapCases.MazeCells(iron);
                    for(int cell=0;cell<cells.GetLength(0);cell++)
                        if(!Purchase(w,cells[cell,0],cells[cell,1],design,r))throw new Exception("Paid maze fixture could not be built");
                }
                for(int wave=0;wave<c.Waves.Length&&!w.Finished;wave++) {
                    bool terminalFlight=(strategy=="compact-transition"||strategy=="compact-shared-transition")&&c.Waves.Skip(wave).All(next=>next.Flying);
                    var waveSamples=samples;
                    if(terminalFlight){waveSamples=samples.Where(sample=>sample.Air).ToList();influence.Clear();}
                    for(int player=0;player<players;player++){w.SelectPlayer(player);
                        if(woodProgression&&c.FactionWoodUnlocks&&w.Wood>0) {
                            int next=(w.Players[player].Faction+1)%c.Factions.Length;
                            if(!w.FactionUnlocked(next)) {int before=w.Wood;if(!w.ChooseFaction(next,out string reason)||w.Wood!=before-1)throw new Exception("Paid faction unlock failed: "+reason);
                                r.WoodPurchases.Add("Before wave "+(wave+1)+": player "+(player+1)+" unlocked "+c.Factions[next].Name+" for 1 wood");}
                        }
                        if(strategy=="compact-shared-transition") {
                            if(terminalFlight){SellGroundOnly(w,r);keptOpen.Clear();}
                            SpendInvest(w,waveSamples,r,48/players);
                        }else if(strategy=="compact-transition") {
                            if(terminalFlight)SellGroundOnly(w,r);
                            SpendAdaptive(w,waveSamples,r,48/players,true,true);
                        }else if(strategy=="roster")SpendRoster(w,samples,r);else if(strategy=="compact-invest"||strategy=="compact-shared-maze")SpendInvest(w,samples,r,48/players);else if(strategy=="adaptive"||strategy.StartsWith("compact"))SpendAdaptive(w,samples,r,strategy.StartsWith("compact")?48/players:int.MaxValue,strategy=="compact-value"||strategy=="compact-roles"||strategy=="compact-support",strategy=="compact-roles"||strategy=="compact-support",strategy=="compact-support");else Spend(w,samples,r);}
                    int[] beforeCombat=AuditWallets(w,r,wave);int killed=w.Killed,leaked=w.Leaked;
                    if(!w.StartWave())throw new Exception("Wave failed to start");int ticks=0;
                    while(w.WaveActive&&!w.Finished&&ticks<18000){w.Step();ticks++;}
                    if(!w.Defeated&&!w.WaveActive&&w.Killed-killed+w.Leaked-leaked!=c.Waves[wave].Count*w.LaneCount)
                        throw new Exception("Completed wave did not account for all active lanes.");
                    if(strategy.StartsWith("compact")&&w.Grid.Towers.Count>48)throw new Exception("Compact diagnostic exceeded its 48-tower team limit");
                    var afterCombat=AuditWallets(w,r,wave+(!w.Defeated&&!w.WaveActive?1:0));
                    var summary=w.LastWaveSummary;
                    bool cleared=!w.Defeated&&!w.WaveActive;
                    var income=new int[players];for(int player=0;player<players;player++)income[player]=afterCombat[player]-beforeCombat[player];
                    // The planner finishes its paid orders before combat. Thus wallet deltas here
                    // independently verify the income presented in the game's end-of-wave summary.
                    int teamIncome=(w.Killed-killed)*w.KillGold(wave)+(cleared?w.ClearGold(wave):0);
                    if(income.Sum()!=teamIncome)throw new Exception("Wave wallet deltas disagree with earned rewards.");
                    if(w.Defeated||cleared) {
                        if(summary==null||summary.WaveNumber!=wave+1||summary.Cleared!=cleared||summary.Killed!=w.Killed-killed||summary.Leaked!=w.Leaked-leaked||summary.TeamGold!=teamIncome)
                            throw new Exception("Displayed wave summary disagrees with campaign outcome or rewards.");
                        for(int player=0;player<players;player++)if(summary.GoldForPlayer(player)!=income[player])
                            throw new Exception("Displayed player income disagrees with paid campaign wallet.");
                    }else if(summary!=null)throw new Exception("Unfinished wave reported a completed summary.");
                    r.Waves.Add(new WaveResult{SummaryPresent=summary!=null,Cleared=cleared,TeamIncome=teamIncome,PlayerIncome=income,PlayerGold=afterCombat,PlayerWood=w.Players.Select(p=>p.Wood).ToArray(),Wave=wave+1,Flying=c.Waves[wave].Flying,Killed=w.Killed-killed,Leaked=w.Leaked-leaked,Ticks=ticks,Gold=TeamGold(w)});
                    if(ticks>=18000){
                        r.Stalled=true;
                        var diagnostic=new {Map=c.Name,Faction=faction,Wave=wave+1,Enemies=w.Enemies.Select(e=>new {e.Id,Position=new{e.Position.X,e.Position.Y},Velocity=new{e.Velocity.X,e.Velocity.Y},e.Health,e.Blocked,e.BlockerId,e.Checkpoint,Destination=new{e.Destination.X,e.Destination.Y},e.Lane}).ToArray(),Towers=w.Grid.Towers.Select(t=>new {t.Id,t.CellX,t.CellY,t.Design,t.Health}).ToArray()};
                        File.WriteAllText(path+"."+c.Name+"-"+faction+".stall.json",JsonSerializer.Serialize(diagnostic,new JsonSerializerOptions{WriteIndented=true,IncludeFields=true}));break;
                    }
                }
                r.Won=w.Won;r.Lives=w.Lives;r.Gold=TeamGold(w);
                int completed=r.Waves.Count-(w.Defeated||r.Stalled?1:0);
                r.FinalWallets=AuditWallets(w,r,completed);
                if(r.Spent+r.Gold-r.Refunded!=c.StartingGold+w.TotalGoldEarned)throw new Exception("Team budget was not conserved");
                results.Add(r);
                Console.Error.WriteLine($"{r.Map} / {r.Faction}: {(r.Won?"WIN":r.Stalled?"STALL":"LOSS")} lives={r.Lives} waves={r.Waves.Count} spent={r.Spent} upgrades={r.Upgrades.Count}");
                File.WriteAllText(path,JsonSerializer.Serialize(results,new JsonSerializerOptions{WriteIndented=true}));
            }
        }
        return results.Exists(r=>r.Stalled)?1:0;
    }
}
