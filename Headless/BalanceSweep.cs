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
    sealed class Placement { public int TowerId{get;set;} public int BeforeWave{get;set;} public int Player{get;set;} public int X{get;set;} public int Y{get;set;} public string Tower{get;set;} public int Cost{get;set;} }
    sealed class UpgradeResult { public int Player{get;set;} public int TowerId{get;set;} public string Tower{get;set;} public int Level{get;set;} public int Cost{get;set;} public int BeforeWave{get;set;} }
    sealed class WaveResult { public int[] PlayerGold{get;set;} public int Wave{get;set;} public bool Flying{get;set;} public int Killed{get;set;} public int Leaked{get;set;} public int Ticks{get;set;} public int Gold{get;set;} }
    sealed class Result { public int[] FinalWallets{get;set;} public int[] PlayerSpending{get;set;} public string Strategy{get;set;} public string[] Factions{get;set;} public List<UpgradeResult> Upgrades{get;set;}=new List<UpgradeResult>(); public int PlayerCount{get;set;} public string Map{get;set;} public string Faction{get;set;} public string Difficulty{get;set;} public bool Won{get;set;} public bool Stalled{get;set;} public int Lives{get;set;} public int Gold{get;set;} public int Spent{get;set;} public List<Placement> Placements{get;set;}=new List<Placement>(); public List<WaveResult> Waves{get;set;}=new List<WaveResult>(); }
    sealed class Candidate { public int X,Y; public int[] Samples; }
    static readonly Dictionary<TowerSpec,List<Candidate>> influence=new Dictionary<TowerSpec,List<Candidate>>();
    static List<Candidate> Candidates(World w,TowerSpec spec,List<Sample> samples)
    {
        if(influence.TryGetValue(spec,out var cached))return cached;
        cached=new List<Candidate>();
        for(int y=1;y<w.Config.Height-1;y+=2)for(int x=1;x<w.Config.Width-1;x+=2) {
            if(w.Grid.TerrainOverlaps(x,y,spec.Width,spec.Height))continue;
            var affected=new List<int>();var p=new V2(x+.5f,y+.5f);
            for(int i=0;i<samples.Count;i++)if((samples[i].Air?spec.TargetsAir:spec.TargetsGround)&&V2.Distance(p,samples[i].Position)<spec.Range)affected.Add(i);
            cached.Add(new Candidate{X=x,Y=y,Samples=affected.ToArray()});
        }
        influence.Add(spec,cached);return cached;
    }
    static List<Sample> Samples(Scenario c)
    {
        influence.Clear();
        var result=new List<Sample>();
        for(int lane=0;lane<c.Lanes.Length;lane++)foreach(bool air in new[]{false,true}) {
            var w=new World(c);w.TowersFire=false;w.Spawn(new WaveSpec{Flying=air},w.LaneSpawn(lane),lane);
            for(int tick=0;tick<9000&&w.Enemies.Count>0;tick++) {
                if(tick%30==0)result.Add(new Sample{Position=w.Enemies[0].Position,Air=air});w.Step();
            }
            if(w.Leaked!=1)throw new Exception("Cannot sample route");
        }
        return result;
    }
    static bool Purchase(World w,int x,int y,int design,Result result)
    {
        w.SelectedDesign=design;int before=w.Gold,count=w.Grid.Towers.Count;
        if(!w.OrderBuild(x,y,out _))return false;
        for(int tick=0;tick<1000&&w.HasBuildOrder;tick++)w.Step();
        if(w.HasBuildOrder||w.Grid.Towers.Count!=count+1)throw new Exception("Builder failed a legal purchase");
        int cost=before-w.Gold;if(cost!=w.Config.Catalog[design].Cost)throw new Exception("Purchase accounting mismatch");
        result.Spent+=cost;result.Placements.Add(new Placement{TowerId=w.Grid.Towers[w.Grid.Towers.Count-1].Id,BeforeWave=w.WaveIndex+2,Player=w.ActivePlayer+1,X=x,Y=y,Tower=w.BuildName,Cost=cost});return true;
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
        // This driver never sells, so all debits are recorded paid placements/upgrades.
        int grants=w.Config.StartingGold+w.Killed*w.Config.KillReward+completedWaves*w.Config.WaveReward;
        var wallets=new int[w.Players.Length];var spending=new int[wallets.Length];
        for(int player=0;player<wallets.Length;player++) {
            spending[player]=result.Placements.Where(p=>p.Player==player+1).Sum(p=>p.Cost)+result.Upgrades.Where(u=>u.Player==player+1).Sum(u=>u.Cost);
            int expected=grants/wallets.Length+(player<grants%wallets.Length?1:0)-spending[player];
            wallets[player]=w.Players[player].Gold;
            if(wallets[player]<0||wallets[player]!=expected)throw new Exception("Player "+(player+1)+" wallet ledger mismatch");
        }
        result.PlayerSpending=spending;return wallets;
    }
    static int TeamGold(World w) { int total=0;foreach(var p in w.Players)total+=p.Gold;return total; }
    public static int Run(string path,Difficulty difficulty,int players,string strategy="coverage",bool mixed=false,string mapFilter="",int factionFilter=-1)
    {
        if(strategy!="coverage"&&strategy!="roster"&&strategy!="maze"&&strategy!="adaptive"&&strategy!="compact"&&strategy!="compact-value"&&strategy!="compact-roles"&&strategy!="compact-support"&&strategy!="compact-invest")throw new ArgumentException("Strategy must be coverage, roster, maze, adaptive, compact, compact-value, compact-roles, compact-support or compact-invest.");
        if(players<1||players>4)throw new ArgumentException("Player count must be 1–4.");
        var results=new List<Result>();
        foreach(bool iron in new[]{false,true}) {
            var c=MapCases.Load(iron);if(mapFilter.Length>0&&c.Name!=mapFilter)continue;var samples=Samples(c);
            for(int faction=0;faction<c.Factions.Length;faction++) {
                if(factionFilter>=0&&faction!=factionFilter)continue;
                var factions=new int[4];for(int player=0;player<4;player++)factions[player]=mixed?(faction+player)%c.Factions.Length:faction;
                var w=new World(c,new MatchOptions{PlayerCount=players,Difficulty=difficulty,Factions=factions});
                var r=new Result{Strategy=strategy,Factions=factions.Take(players).Select(f=>c.Factions[f].Name).ToArray(),PlayerCount=players,Map=c.Name,Faction=c.Factions[faction].Name,Difficulty=difficulty.ToString()};
                if(strategy=="maze") {
                    int design=MapCases.MazeDesign(w);var cells=MapCases.MazeCells(iron);
                    for(int cell=0;cell<cells.GetLength(0);cell++)
                        if(!Purchase(w,cells[cell,0],cells[cell,1],design,r))throw new Exception("Paid maze fixture could not be built");
                }
                for(int wave=0;wave<c.Waves.Length&&!w.Finished;wave++) {
                    for(int player=0;player<players;player++){w.SelectPlayer(player);if(strategy=="roster")SpendRoster(w,samples,r);else if(strategy=="compact-invest")SpendInvest(w,samples,r,48/players);else if(strategy=="adaptive"||strategy.StartsWith("compact"))SpendAdaptive(w,samples,r,strategy.StartsWith("compact")?48/players:int.MaxValue,strategy=="compact-value"||strategy=="compact-roles"||strategy=="compact-support",strategy=="compact-roles"||strategy=="compact-support",strategy=="compact-support");else Spend(w,samples,r);}int killed=w.Killed,leaked=w.Leaked;
                    if(!w.StartWave())throw new Exception("Wave failed to start");int ticks=0;
                    while(w.WaveActive&&!w.Finished&&ticks<18000){w.Step();ticks++;}
                    if(strategy.StartsWith("compact")&&w.Grid.Towers.Count>48)throw new Exception("Compact diagnostic exceeded its 48-tower team limit");
                    r.Waves.Add(new WaveResult{PlayerGold=AuditWallets(w,r,wave+(!w.Defeated&&!w.WaveActive?1:0)),Wave=wave+1,Flying=c.Waves[wave].Flying,Killed=w.Killed-killed,Leaked=w.Leaked-leaked,Ticks=ticks,Gold=TeamGold(w)});
                    if(ticks>=18000){
                        r.Stalled=true;
                        var diagnostic=new {Map=c.Name,Faction=faction,Wave=wave+1,Enemies=w.Enemies.Select(e=>new {e.Id,Position=new{e.Position.X,e.Position.Y},Velocity=new{e.Velocity.X,e.Velocity.Y},e.Health,e.Blocked,e.BlockerId,e.Checkpoint,Destination=new{e.Destination.X,e.Destination.Y},e.Lane}).ToArray(),Towers=w.Grid.Towers.Select(t=>new {t.Id,t.CellX,t.CellY,t.Design,t.Health}).ToArray()};
                        File.WriteAllText(path+"."+c.Name+"-"+faction+".stall.json",JsonSerializer.Serialize(diagnostic,new JsonSerializerOptions{WriteIndented=true,IncludeFields=true}));break;
                    }
                }
                r.Won=w.Won;r.Lives=w.Lives;r.Gold=TeamGold(w);
                int completed=r.Waves.Count-(w.Defeated||r.Stalled?1:0);
                r.FinalWallets=AuditWallets(w,r,completed);
                if(r.Spent+r.Gold!=c.StartingGold+w.Killed*c.KillReward+completed*c.WaveReward)throw new Exception("Team budget was not conserved");
                results.Add(r);
                Console.Error.WriteLine($"{r.Map} / {r.Faction}: {(r.Won?"WIN":r.Stalled?"STALL":"LOSS")} lives={r.Lives} waves={r.Waves.Count} spent={r.Spent} upgrades={r.Upgrades.Count}");
                File.WriteAllText(path,JsonSerializer.Serialize(results,new JsonSerializerOptions{WriteIndented=true}));
            }
        }
        return results.Exists(r=>r.Stalled)?1:0;
    }
}
