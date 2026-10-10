using System;
using FrostMaze.Simulation;
namespace FrostMaze.Tests
{
    public static class NavigationCases
    {
        static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
        // Independent, deliberately slow oracle: sorted Dijkstra selection and full
        // tower scans, with no shared edge cache, spatial buckets or production heap.
        static FlowField Reference(MazeGrid grid,V2 goal,float radius,float step,bool breach,float region)
        {
            var f=new FlowField((int)Math.Ceiling(grid.Width/step),(int)Math.Ceiling(grid.Height/step),step,radius,goal,breach,region);
            var done=new bool[f.Next.Length];
            for(int i=0;i<f.Next.Length;i++)if(V2.Distance(f.Point(i),goal)<=(region>0?region:step*1.5f)
                &&(region>0?grid.TerrainClear(f.Point(i),goal,radius)&&(breach||grid.Clear(f.Point(i),f.Point(i),radius)):grid.Clear(f.Point(i),goal,radius)))
                f.Distance[i]=V2.Distance(f.Point(i),goal);
            while(true) {
                int at=-1;float distance=float.PositiveInfinity;
                for(int i=0;i<f.Next.Length;i++)if(!done[i]&&f.Distance[i]<distance){at=i;distance=f.Distance[i];}
                if(at<0)break;done[at]=true;
                int x=at%f.Columns,y=at/f.Columns;var a=f.Point(at);
                for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++) {
                    if(dx==0&&dy==0)continue;
                    int nx=x+dx,ny=y+dy;if(nx<0||ny<0||nx>=f.Columns||ny>=f.Rows)continue;
                    int n=nx+ny*f.Columns;var b=f.Point(n);if(!grid.TerrainClear(a,b,radius))continue;
                    float cost=V2.Distance(a,b);bool blocked=false;
                    foreach(var t in grid.Towers)if(Geometry.SweepBox(a,b,t.Center,t.Half,radius)) {
                        if(!breach){blocked=true;break;}cost+=12;
                    }
                    if(blocked)continue;float candidate=distance+cost;
                    if(candidate+.00001f<f.Distance[n]){f.Distance[n]=candidate;f.Next[n]=at;}
                }
            }
            return f;
        }
        static void Compare(MazeGrid grid,FlowNavigation nav)
        {
            foreach(float radius in new[]{.2f,.3f,.6f})foreach(bool breach in new[]{false,true})foreach(float region in new[]{0f,1.45f}) {
                var goal=new V2(9.5f,5.5f);var expected=Reference(grid,goal,radius,nav.Step,breach,region);var actual=nav.Get(goal,radius,breach,region);
                for(int i=0;i<expected.Next.Length;i++) {
                    Check(actual.Distance[i]==expected.Distance[i],"Cost differs at "+i+" radius="+radius+" breach="+breach+" region="+region);
                    Check(actual.Next[i]==expected.Next[i],"Path tie-break differs at "+i);
                }
            }
        }
        public static void IncrementalFieldsMatchFullScan()
        {
            foreach(float step in new[]{.5f,.25f}) {
                var grid=new MazeGrid(12,8);var nav=new FlowNavigation(grid,step);
                grid.AddTerrain(new TerrainBlock{X=0,Y=0,Width=2,Height=2});
                // Multi-bucket footprints, narrow diagonal apertures and covered checkpoints.
                var large=grid.Build(3.5f,2,new TowerSpec{Width=3,Height=2,Fill=.86f});
                grid.Build(7,4,new TowerSpec{Fill=.6f});grid.Build(8,3,new TowerSpec{Fill=.6f});
                Compare(grid,nav);
                var covered=grid.Build(9,5,new TowerSpec{Fill=1});Compare(grid,nav);
                grid.Remove(large.Id);Compare(grid,nav);
                grid.Damage(covered.Id,1000);Compare(grid,nav);
                grid.Build(3.5f,2,new TowerSpec{Width=3,Height=2,Fill=1});
                grid.Build(1,6,new TowerSpec{Width=2});Compare(grid,nav);
                // Permanent terrain edits must also invalidate previously cached edges.
                grid.AddTerrain(new TerrainBlock{X=6.5f,Y=0,Width=.5f,Height=7});Compare(grid,nav);
            }
        }
        public static void LocalEditsReuseCollisionWork()
        {
            var grid=new MazeGrid(64,64);var nav=new FlowNavigation(grid);var goal=new V2(60.5f,60.5f);
            var field=nav.Get(goal,.2f);var distances=field.Distance;var next=field.Next;
            long full=nav.GeometryChecks;Check(full>100000,"Full map fixture did not evaluate edges");
            var tower=grid.Build(30,30,new TowerSpec{Fill=1});var rebuilt=nav.Get(goal,.2f);
            long local=nav.GeometryChecks-full;Check(local>0&&local<2000,"Single build rechecked the whole map");
            Check(ReferenceEquals(field,rebuilt)&&ReferenceEquals(distances,rebuilt.Distance)&&ReferenceEquals(next,rebuilt.Next),"Route arrays reallocated after build");
            long checks=nav.GeometryChecks;
            nav.Get(new V2(3,60),.2f,true);Check(nav.GeometryChecks==checks,"Another destination/breach mode repeated collision work");
            for(int i=0;i<100;i++)nav.Get(goal,.2f);
            long before=GC.GetAllocatedBytesForCurrentThread();for(int i=0;i<10000;i++)nav.Get(goal,.2f);
            Check(GC.GetAllocatedBytesForCurrentThread()==before,"Warm route queries allocate per enemy");
            grid.Remove(tower.Id);nav.Get(goal,.2f);Check(nav.GeometryChecks-checks<2000,"Sale rechecked the whole map");
            Check(nav.Anchor(field,new V2(30.5f,30.5f))>=0,"Sold footprint remained blocked");
        }
    }
}
