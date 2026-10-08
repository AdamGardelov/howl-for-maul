using System;
using System.Collections.Generic;
using System.Linq;
using FrostMaze.Simulation;

static partial class BalanceSweep
{
    // A separate diagnostic: once every remaining wave flies, liquidate only this
    // player's ground-only weapons, then buy/upgrade against actual flight samples.
    static void SellGroundOnly(World w,Result result)
    {
        foreach(var tower in Owned(w,result).Where(t=>!t.Spec.TargetsAir).ToArray()) {
            int owner=w.TowerOwner(tower.Id),refund=w.SaleRefund(tower.Id);
            var before=w.Players.Select(p=>p.Gold).ToArray();
            if(owner!=w.ActivePlayer||!w.Sell(tower.CellX,tower.CellY)||w.Grid.Find(tower.Id)!=null)
                throw new Exception("Final-flight sale failed ownership/removal validation.");
            for(int player=0;player<w.Players.Length;player++)
                if(w.Players[player].Gold!=before[player]+(player==owner?refund:0))
                    throw new Exception("Final-flight sale credited the wrong wallet.");
            result.Refunded+=refund;
            result.Sales.Add(new SaleResult{Player=owner+1,TowerId=tower.Id,Tower=tower.Name,Level=tower.Level,Refund=refund,BeforeWave=w.WaveIndex+2});
        }
    }
    // A diagnostic alternative: a paid upgrade competes with a new footprint on every decision.
    // No gameplay limit or stats are changed, and all transactions use the normal wallet ledger.
    static void SpendInvest(World w,List<Sample> samples,Result result,int limit)
    {
        for(int decision=0;decision<200;decision++) {
            foreach(var sample in samples) {
                sample.Coverage=0;sample.Slow=0;
                foreach(var t in w.Grid.Towers)
                    if((sample.Air?t.Spec.TargetsAir:t.Spec.TargetsGround)&&V2.Distance(t.Center,sample.Position)<t.Spec.Range) {
                        sample.Coverage+=ScoredDps(t.Spec,true);sample.Slow=Math.Max(sample.Slow,t.Spec.SlowFraction);
                    }
            }
            double Marginal(Sample sample,float power,float slow) {
                power+=sample.Coverage*Math.Max(0,slow-sample.Slow)*.5f/(1-slow);
                // A route-sample weight, not a damage multiplier; compare with the saved 0.65 / 1.5 diagnostics.
                return (sample.Air?.9:1)*Math.Log(1+power/(10+sample.Coverage));
            }
            double best=0;int design=-1,x=-1,y=-1;Tower upgrade=null;
            if(Owned(w,result).Count()<limit)
                foreach(int choice in w.Config.Factions[w.Players[w.ActivePlayer].Faction].Designs) {
                    var d=w.Config.Catalog[choice];if(d.Cost>w.Gold||d.Spec.Damage<=0||!w.RequirementsMet(choice))continue;
                    w.SelectedDesign=choice;var weights=new double[samples.Count];
                    for(int i=0;i<samples.Count;i++)weights[i]=Marginal(samples[i],ScoredDps(d.Spec,true),d.Spec.SlowFraction);
                    foreach(var candidate in Candidates(w,d.Spec,samples)) {
                        if(!w.CanBuild(candidate.X,candidate.Y,out _))continue;
                        double value=0;foreach(int index in candidate.Samples)value+=weights[index];
                        value/=Math.Sqrt(d.Cost);
                        if(value>best){best=value;design=choice;x=candidate.X;y=candidate.Y;}
                    }
                }
            foreach(var tower in Owned(w,result)) {
                int cost=w.UpgradeCost(tower);if(tower.Level>=3||tower.Spec.Damage<=0||cost>w.Gold)continue;
                double value=0;var spec=tower.Spec;
                foreach(var sample in samples) {
                    if(!(sample.Air?spec.TargetsAir:spec.TargetsGround))continue;
                    float distance=V2.Distance(tower.Center,sample.Position);
                    if(distance>=spec.Range+.35f)continue;
                    float power=ScoredDps(spec,true)*(distance<spec.Range?.6f:1.6f);
                    value+=Marginal(sample,power,spec.SlowFraction);
                }
                value/=Math.Sqrt(cost);
                if(value>best){best=value;upgrade=tower;}
            }
            if(upgrade!=null) {
                int cost=w.UpgradeCost(upgrade),before=w.Gold;
                if(!w.Upgrade(upgrade.Id,out string reason)||before-w.Gold!=cost)throw new Exception("Investment upgrade accounting failed: "+reason);
                result.Spent+=cost;result.Upgrades.Add(new UpgradeResult{Player=w.ActivePlayer+1,TowerId=upgrade.Id,Tower=upgrade.Name,Level=upgrade.Level,Cost=cost,BeforeWave=w.WaveIndex+2});
            } else if(design<0||!Purchase(w,x,y,design,result))break;
        }
    }
}
