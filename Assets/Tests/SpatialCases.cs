using System;
using FrostMaze.Simulation;
namespace FrostMaze.Tests
{
    public static class SpatialCases
    {
        static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
        static void Compare(MazeGrid grid,V2 a,V2 b,float radius)
        {
            bool clear=grid.TerrainClear(a,b,radius);Tower hit=null;float distance=float.PositiveInfinity;
            // Independent oracle: original full-list query, without any bucket logic.
            foreach(var tower in grid.Towers)if(Geometry.SweepBox(a,b,tower.Center,tower.Half,radius)) {
                clear=false;float d=Geometry.PointBox(a,tower.Center,tower.Half);
                if(d<distance||(d==distance&&(hit==null||tower.Id<hit.Id))){distance=d;hit=tower;}
            }
            Check(grid.Clear(a,b,radius)==clear,"Indexed clearance differs from full scan");
            Check(grid.FirstHit(a,b,radius)==hit,"Indexed blocker differs from full scan");
        }
        static void SweepQueries(MazeGrid grid,Random random)
        {
            float[] radii={0,.1f,.2f,.49f,.75f};
            for(int i=0;i<4000;i++) {
                var a=new V2((float)random.NextDouble()*34-1,(float)random.NextDouble()*34-1);
                var b=i%5==0?new V2((float)random.NextDouble()*34-1,(float)random.NextDouble()*34-1):i%5==1?a:new V2(a.X+(float)random.NextDouble()*3-1.5f,a.Y+(float)random.NextDouble()*3-1.5f);
                Compare(grid,a,b,radii[i%radii.Length]);
            }
            // Exact cell/bucket edges and tangent paths complement non-boundary random samples.
            for(int y=0;y<=32;y+=2)for(int x=0;x<=32;x+=2)foreach(float radius in radii) {
                Compare(grid,new V2(x,y),new V2(x+2,y),radius);
                Compare(grid,new V2(x,y),new V2(x,y+2),radius);
            }
        }
        public static void TowerQueriesMatchFullScan()
        {
            var grid=new MazeGrid(32,32);var random=new Random(77194);
            grid.AddTerrain(new TerrainBlock{X=12,Y=0,Width=.5f,Height=20});
            for(int i=0;i<350;i++)grid.Build(random.Next(0,61)*.5f,random.Next(0,61)*.5f,new TowerSpec{Width=random.Next(1,4),Height=random.Next(1,4),Fill=new[]{.6f,.86f,1f}[i%3]});
            Check(grid.Towers.Count>40,"Insufficient varied footprints");SweepQueries(grid,random);
            var snapshot=grid.Towers.ToArray();
            for(int i=0;i<snapshot.Length;i+=2) {
                if(i%4==0)Check(grid.Remove(snapshot[i].Id),"Removal failed");
                else grid.Damage(snapshot[i].Id,1000);
            }
            SweepQueries(grid,random);
            for(int i=0;i<snapshot.Length;i+=2) {
                var old=snapshot[i];Check(grid.Build(old.CellX,old.CellY,old.Spec.Copy())!=null,"Removed bucket footprint did not reopen");
            }
            SweepQueries(grid,random);
            var tie=new MazeGrid(8,8);var first=tie.Build(2,2,new TowerSpec{Fill=1});var second=tie.Build(1,2,new TowerSpec{Fill=1});
            var from=new V2(2,1);var to=new V2(2,4);
            Check(tie.FirstHit(from,to,.2f)==first,"Bucket iteration changed equal-distance blocker priority");
            tie.Remove(first.Id);Check(tie.FirstHit(from,to,.2f)==second,"Removed tower remains indexed");
            tie.Damage(second.Id,1000);Check(tie.FirstHit(from,to,.2f)==null,"Destroyed tower remains indexed");
        }
    }
}
