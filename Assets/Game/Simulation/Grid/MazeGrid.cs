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
        public void AddTerrain(TerrainBlock block) { Terrain.Add(block); Version++; }
        public bool TerrainClear(V2 a,V2 b,float radius)
        {
            if(!InBounds(a,radius)||!InBounds(b,radius))return false;
            foreach(var t in Terrain)if(Geometry.SweepBox(a,b,t.Center,t.Half,radius))return false;
            return true;
        }
        public bool TerrainOverlaps(int x,int y,int width,int height)
        {
            foreach(var t in Terrain)if(x<t.X+t.Width&&x+width>t.X&&y<t.Y+t.Height&&y+height>t.Y)return true;
            return false;
        }
        int nextId = 1;
        public MazeGrid(int width, int height)
        {
            Width = width;
            Height = height;
        }
        public Tower Build(int x, int y, TowerSpec spec)
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
            Version++;
            return tower;
        }
        public bool Remove(int id)
        {
            int i = Towers.FindIndex(t => t.Id == id);
            if (i < 0)
                return false;
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
        public Tower At(int x, int y) => Towers.Find(t => x >= t.CellX && x < t.CellX + t.Spec.Width && y >= t.CellY && y < t.CellY + t.Spec.Height);
        public bool InBounds(V2 p, float radius) => p.X >= radius && p.Y >= radius && p.X <= Width - radius && p.Y <= Height - radius;
        public bool Clear(V2 a, V2 b, float radius)
        {
            if (!TerrainClear(a,b,radius))
                return false;
            foreach (var t in Towers)
                if (Geometry.SweepBox(a, b, t.Center, t.Half, radius))
                    return false;
            return true;
        }
        public Tower FirstHit(V2 a, V2 b, float radius)
        {
            Tower best = null;
            float distance = float.PositiveInfinity;
            foreach (var t in Towers)
                if (Geometry.SweepBox(a, b, t.Center, t.Half, radius))
                {
                    float d = Geometry.PointBox(a, t.Center, t.Half);
                    if (d < distance || (d == distance && t.Id < best.Id))
                    {
                        distance = d;
                        best = t;
                    }
                }
            return best;
        }
    }
}
