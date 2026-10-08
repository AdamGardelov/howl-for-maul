using System;
using System.Collections.Generic;
namespace FrostMaze.Simulation
{
    public sealed class MazeGrid
    {
        public readonly int Width, Height;
        public readonly List<Tower> Towers = new List<Tower>();
        public int Version
        {
            get; private set;
        }
        public readonly List<TerrainBlock> Terrain = new List<TerrainBlock>();
        const int TerrainBucketSize=4;
        readonly Dictionary<int,List<TerrainBlock>> terrainBuckets=new Dictionary<int,List<TerrainBlock>>();
        int BucketColumns => (Width+TerrainBucketSize-1)/TerrainBucketSize;
        public void AddTerrain(TerrainBlock block)
        {
            Terrain.Add(block);Version++;
            for(int y=(int)(block.Y/TerrainBucketSize);y<Math.Ceiling((block.Y+block.Height)/TerrainBucketSize);y++)
                for(int x=(int)(block.X/TerrainBucketSize);x<Math.Ceiling((block.X+block.Width)/TerrainBucketSize);x++) {
                    int key=x+y*BucketColumns;
                    if(!terrainBuckets.TryGetValue(key,out var list)){list=new List<TerrainBlock>();terrainBuckets[key]=list;}
                    list.Add(block);
                }
        }
        public bool TerrainClear(V2 a,V2 b,float radius)
        {
            if(!InBounds(a,radius)||!InBounds(b,radius))return false;
            int minX=Math.Max(0,(int)Math.Floor((Math.Min(a.X,b.X)-radius)/TerrainBucketSize));
            int maxX=Math.Min(BucketColumns-1,(int)Math.Floor((Math.Max(a.X,b.X)+radius)/TerrainBucketSize));
            int minY=Math.Max(0,(int)Math.Floor((Math.Min(a.Y,b.Y)-radius)/TerrainBucketSize));
            int maxY=Math.Min((Height-1)/TerrainBucketSize,(int)Math.Floor((Math.Max(a.Y,b.Y)+radius)/TerrainBucketSize));
            for(int y=minY;y<=maxY;y++)for(int x=minX;x<=maxX;x++)
                if(terrainBuckets.TryGetValue(x+y*BucketColumns,out var blocks))foreach(var t in blocks)
                    if(Geometry.SweepBox(a,b,t.Center,t.Half,radius))return false;
            return true;
        }
        public bool TerrainOverlaps(float x,float y,int width,int height)
        {
            foreach(var t in Terrain)if(x<t.X+t.Width&&x+width>t.X&&y<t.Y+t.Height&&y+height>t.Y)return true;
            return false;
        }
        // Broad phase only: exact rounded-disc geometry still decides collision.
        // Tower footprints stay fixed for their lifetime; upgrades change combat stats only.
        const int TowerBucketSize=2;
        readonly Dictionary<int,List<Tower>> towerBuckets=new Dictionary<int,List<Tower>>();
        int TowerBucketColumns => (Width+TowerBucketSize-1)/TowerBucketSize;
        void TowerBounds(V2 a,V2 b,float radius,out int minX,out int maxX,out int minY,out int maxY)
        {
            minX=Math.Max(0,(int)Math.Floor((Math.Min(a.X,b.X)-radius)/TowerBucketSize));
            maxX=Math.Min(TowerBucketColumns-1,(int)Math.Floor((Math.Max(a.X,b.X)+radius)/TowerBucketSize));
            minY=Math.Max(0,(int)Math.Floor((Math.Min(a.Y,b.Y)-radius)/TowerBucketSize));
            maxY=Math.Min((Height-1)/TowerBucketSize,(int)Math.Floor((Math.Max(a.Y,b.Y)+radius)/TowerBucketSize));
        }
        void IndexTower(Tower tower,bool add)
        {
            TowerBounds(tower.Center-tower.Half,tower.Center+tower.Half,0,out int minX,out int maxX,out int minY,out int maxY);
            for(int y=minY;y<=maxY;y++)for(int x=minX;x<=maxX;x++) {
                int key=x+y*TowerBucketColumns;
                if(add) {
                    if(!towerBuckets.TryGetValue(key,out var bucket)){bucket=new List<Tower>();towerBuckets.Add(key,bucket);}
                    bucket.Add(tower);
                } else if(towerBuckets.TryGetValue(key,out var bucket)) {
                    bucket.Remove(tower);
                    if(bucket.Count==0)towerBuckets.Remove(key);
                }
            }
        }
        int nextId = 1;
        public MazeGrid(int width, int height)
        {
            Width = width;
            Height = height;
        }
        public Tower Build(float x, float y, TowerSpec spec)
        {
            if (spec.Width < 1 || spec.Height < 1 || spec.Fill <= 0 || spec.Fill > 1 || spec.Health <= 0)
                return null;
            if (x < 0 || y < 0 || x + spec.Width > Width || y + spec.Height > Height)
                return null;
            if(TerrainOverlaps(x,y,spec.Width,spec.Height))return null;
            foreach (var t in Towers)
                if (x < t.CellX + t.Spec.Width && x + spec.Width > t.CellX && y < t.CellY + t.Spec.Height && y + spec.Height > t.CellY)
                    return null;
            var tower = new Tower { Id = nextId++, CellX = x, CellY = y, Spec = spec, Health = spec.Health };
            Towers.Add(tower);
            IndexTower(tower,true);
            Version++;
            return tower;
        }
        public bool Remove(int id)
        {
            int i = Towers.FindIndex(t => t.Id == id);
            if (i < 0)
                return false;
            IndexTower(Towers[i],false);
            Towers.RemoveAt(i);
            Version++;
            return true;
        }
        public void Damage(int id, float amount)
        {
            var t = Find(id);
            if (t == null)
                return;
            t.Health -= amount;
            if (t.Health <= 0)
                Remove(id);
        }
        public Tower Find(int id) => Towers.Find(t => t.Id == id);
        public Tower At(float x, float y) => Towers.Find(t => x >= t.CellX && x < t.CellX + t.Spec.Width && y >= t.CellY && y < t.CellY + t.Spec.Height);
        public bool InBounds(V2 p, float radius) => p.X >= radius && p.Y >= radius && p.X <= Width - radius && p.Y <= Height - radius;
        public bool Clear(V2 a, V2 b, float radius)
        {
            if (!TerrainClear(a,b,radius)) return false;
            TowerBounds(a,b,radius,out int minX,out int maxX,out int minY,out int maxY);
            for(int y=minY;y<=maxY;y++)for(int x=minX;x<=maxX;x++)
                if(towerBuckets.TryGetValue(x+y*TowerBucketColumns,out var bucket))
                    foreach(var t in bucket)
                        if(Geometry.SweepBox(a,b,t.Center,t.Half,radius))return false;
            return true;
        }
        public Tower FirstHit(V2 a, V2 b, float radius)
        {
            Tower best=null;float distance=float.PositiveInfinity;
            TowerBounds(a,b,radius,out int minX,out int maxX,out int minY,out int maxY);
            for(int y=minY;y<=maxY;y++)for(int x=minX;x<=maxX;x++)
                if(towerBuckets.TryGetValue(x+y*TowerBucketColumns,out var bucket))
                    foreach(var t in bucket)
                        if(Geometry.SweepBox(a,b,t.Center,t.Half,radius)) {
                            float d=Geometry.PointBox(a,t.Center,t.Half);
                            if(d<distance||(d==distance&&(best==null||t.Id<best.Id))){distance=d;best=t;}
                        }
            return best;
        }
    }
}
