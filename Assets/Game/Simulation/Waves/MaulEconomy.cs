namespace FrostMaze.Simulation
{
    public static class MaulEconomy
    {
        // Version-specific reference openings, four-player-equivalent shared budget.
        // Starting Winter lumber is represented by the free initial faction selection.
        public static void Apply(Scenario c)
        {
            bool iron=c.Theme=="iron";
            c.StartingGold=iron?2200:240;
            c.FactionWoodUnlocks=!iron;
            for(int i=0;i<c.Waves.Length;i++) {
                c.Waves[i].KillGold=1+i/4;
                c.Waves[i].ClearGold=4*(14+2*i);
                c.Waves[i].WoodReward=i==(iron?13:8)?4:0;
            }
            if(iron)for(int f=0;f<c.Factions.Length;f++) {
                var champion=c.Catalog[f*7+6];champion.Cost=750;champion.Refund=562;champion.WoodCost=1;
                champion.Spec.Damage*=3;champion.Spec.Health*=2;
                champion.Description+=" · 1 wood, earned after wave 14";
            }
            else for(int f=0;f<c.Factions.Length;f++) {
                var opener=c.Catalog[f*5];opener.Cost=f==1||f==2?12:10;opener.Refund=opener.Cost*3/4;
            }
        }
    }
}
