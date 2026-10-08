namespace FrostMaze.Simulation
{
    public static class CampaignProgression
    {
        // Preserve the opening ten-wave learning curve; the second half tests different defenses.
        public static void Apply(Scenario c)
        {
            var waves=new WaveSpec[20];
            for(int i=0;i<10;i++)waves[i]=c.Waves[i];
            waves[10]=W("II · Reinforced patrol",24,180,1.9f,.75f);
            waves[11]=W("II · Skitter rush",30,155,2.7f,.45f);
            waves[12]=W("II · Swarm column",38,150,2.1f,.3f);
            waves[13]=W("II · Siege wardens",16,340,1.55f,1.05f,30);
            waves[14]=W("II · Sky lancers",28,230,2.5f,.65f,12,true);
            waves[15]=W("III · Veteran vanguard",30,310,2.1f,.65f);
            waves[16]=W("III · Iron procession",18,560,1.6f,1,36);
            waves[17]=W("III · Last rush",42,285,2.65f,.35f);
            waves[18]=W("III · Gatebreakers",12,1000,1.4f,1.4f,50);
            waves[19]=W("Finale · Storm host",30,480,2.5f,.65f,12,true);
            // Four separated approaches give Ironfold less shared firing time than Rimewatch.
            // Calibrate its late health budget; lane activity, counts and team income stay fixed.
            if(c.Theme=="iron")for(int i=10;i<waves.Length;i++)waves[i].Health*=.65f;
            c.Waves=waves;
        }
        static WaveSpec W(string name,int count,float health,float speed,float interval,float siege=12,bool flying=false)
            =>new WaveSpec{Name=name,Count=count,Health=health,Speed=speed,SpawnInterval=interval,Damage=siege,Flying=flying};
    }
}
