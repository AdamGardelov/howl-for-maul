using System;
using System.Collections.Generic;
namespace FrostMaze.Simulation
{
    public sealed class FlowField
    {
        public readonly float[] Distance;
        public readonly int[] Next;
        public readonly int Columns, Rows;
        public readonly float Step, Radius, GoalRegion;
        public readonly V2 Goal;
        public readonly bool Breach;
        internal int Version = -1;
        public FlowField(int columns, int rows, float step, float radius, V2 goal, bool breach, float goalRegion = 0)
        {
            Columns = columns;
            Rows = rows;
            Step = step;
            Radius = radius;
            Goal = goal;
            Breach = breach;
            GoalRegion = goalRegion;
            Distance = new float[columns * rows];
            Next = new int[Distance.Length];
            for (int i = 0; i < Next.Length; i++)
            {
                Distance[i] = float.PositiveInfinity;
                Next[i] = -1;
            }
        }
        public V2 Point(int i) => new V2((i % Columns + 0.5f) * Step, (i / Columns + 0.5f) * Step);
        public int Index(V2 p) => Math.Max(0, Math.Min(Columns - 1, (int)(p.X / Step))) + Columns * Math.Max(0, Math.Min(Rows - 1, (int)(p.Y / Step)));
    }
    // Shared destination/radius fields. No enemy occupancy enters global topology.
    public sealed class FlowNavigation
    {
        readonly MazeGrid grid;
        public readonly float Step, BreachCost;
        readonly Dictionary<(float, float, float, bool, float), FlowField> cache = new Dictionary<(float, float, float, bool, float), FlowField>();
        readonly Dictionary<float, FlowTopology> topologies = new Dictionary<float, FlowTopology>();
        readonly MinHeap heap = new MinHeap();
        public long GeometryChecks { get { long checks=0;foreach(var topology in topologies.Values)checks+=topology.GeometryChecks;return checks; } }
        public int Rebuilds
        {
            get; private set;
        }
        public FlowNavigation(MazeGrid grid, float step = 0.5f, float breachCost = 12)
        {
            if (step <= 0 || step > 1)
                throw new ArgumentOutOfRangeException(nameof(step));
            this.grid = grid;
            Step = step;
            BreachCost = breachCost;
        }
        // Fields are borrowed buffers, refreshed in place after topology edits.
        // Simulation and debug consumers request the current field before using it.
        public FlowField Get(V2 goal, float radius, bool breach = false, float goalRegion = 0)
        {
            var key = (goal.X, goal.Y, radius, breach, goalRegion);
            if (!cache.TryGetValue(key, out var field))
            {
                field = new FlowField((int)Math.Ceiling(grid.Width / Step), (int)Math.Ceiling(grid.Height / Step), Step, radius, goal, breach, goalRegion);
                cache.Add(key, field);
            }
            if (field.Version != grid.Version)
            {
                Build(field);
                field.Version = grid.Version;
                Rebuilds++;
            }
            return field;
        }
        void Build(FlowField f)
        {
            var goal=f.Goal;float radius=f.Radius,goalRegion=f.GoalRegion;bool breach=f.Breach;
            if(!topologies.TryGetValue(radius,out var topology)) {
                topology=new FlowTopology(grid,Step,radius,f.Columns,f.Rows);
                topologies.Add(radius,topology);
            }
            topology.Refresh();
            for(int i=0;i<f.Next.Length;i++){f.Distance[i]=float.PositiveInfinity;f.Next[i]=-1;}
            heap.Clear();
            // Multiple seeds avoid making an otherwise reachable checkpoint depend on one sample.
            float seedRadius=goalRegion>0?goalRegion:Step*1.5f;
            int minX=Math.Max(0,(int)Math.Floor((goal.X-seedRadius)/Step)-1),maxX=Math.Min(f.Columns-1,(int)Math.Ceiling((goal.X+seedRadius)/Step));
            int minY=Math.Max(0,(int)Math.Floor((goal.Y-seedRadius)/Step)-1),maxY=Math.Min(f.Rows-1,(int)Math.Ceiling((goal.Y+seedRadius)/Step));
            for(int y=minY;y<=maxY;y++)for(int x=minX;x<=maxX;x++) {
                int i=x+y*f.Columns;
                if (V2.Distance(f.Point(i), goal) <= seedRadius
                    && (goalRegion>0
                        ? grid.TerrainClear(f.Point(i),goal,radius) && (breach||grid.Clear(f.Point(i),f.Point(i),radius))
                        : grid.Clear(f.Point(i),goal,radius)))
                {
                    f.Distance[i] = V2.Distance(f.Point(i), goal);
                    heap.Push(i, f.Distance[i]);
                }
            }
            while (heap.Count > 0)
            {
                var item = heap.Pop();
                int at = item.Index;
                if (item.Cost > f.Distance[at])
                    continue;
                int end=at*8+8;
                for(int edgeIndex=at*8;edgeIndex<end;edgeIndex++)
                {
                    int n=topology.Neighbors[edgeIndex];
                    if(n<0)continue;
                    if (!topology.TerrainEdges[edgeIndex])
                        continue;
                    int intersections=topology.TowerCounts[edgeIndex];
                    if(!breach&&intersections>0)continue;
                    float cost = topology.EdgeLengths[edgeIndex];
                    // Keep sequential additions, matching the original floating-point cost.
                    for(int hit=0;hit<intersections;hit++)cost+=BreachCost;
                    float candidate = item.Cost + cost;
                    if (candidate + 0.00001f < f.Distance[n])
                    {
                        f.Distance[n] = candidate;
                        f.Next[n] = at;
                        heap.Push(n, candidate);
                    }
                }
            }
        }
        public int Anchor(FlowField f, V2 position, bool requireClear = true)
        {
            int center = f.Index(position), cx = center % f.Columns, cy = center / f.Columns, best = -1;
            float score = float.PositiveInfinity;
            for (int y = Math.Max(0, cy - 2); y <= Math.Min(f.Rows - 1, cy + 2); y++)
                for (int x = Math.Max(0, cx - 2); x <= Math.Min(f.Columns - 1, cx + 2); x++)
                {
                    int i = x + y * f.Columns;
                    if (float.IsPositiveInfinity(f.Distance[i]))
                        continue;
                    var p = f.Point(i);
                    float d = V2.Distance(position, p);
                    if (requireClear && !grid.Clear(position, p, f.Radius))
                        continue;
                    float s = d + f.Distance[i];
                    if (s < score)
                    {
                        score = s;
                        best = i;
                    }
                }
            return best;
        }
        public V2 Waypoint(FlowField f, V2 position, out bool reachable, out Tower blocker)
        {
            blocker = null;
            int at = Anchor(f, position);
            reachable = at >= 0;
            if (at < 0)
                return position;
            var aim = f.Point(at);
            for (int k = 0; k < 10; k++)
            {
                int next = f.Next[at];
                var p = next < 0 ? (f.GoalRegion>0?f.Point(at):f.Goal) : f.Point(next);
                var hit = grid.FirstHit(position, p, f.Radius);
                if (hit != null)
                {
                    if (f.Breach)
                        blocker = hit;
                    break;
                }
                if (!grid.Clear(position, p, f.Radius))
                    break;
                aim = p;
                if (next < 0)
                    break;
                at = next;
            }
            // If the first blocking edge is farther away, advance normally and inspect again next tick.
            return aim;
        }
        struct Entry
        {
            public int Index; public float Cost; public Entry(int i, float c)
            {
                Index = i;
                Cost = c;
            }
        }
        sealed class MinHeap
        {
            readonly List<Entry> data = new List<Entry>(); public int Count => data.Count;
            public void Clear()=>data.Clear();
            static bool Less(Entry a, Entry b) => a.Cost < b.Cost || (a.Cost == b.Cost && a.Index < b.Index);
            public void Push(int index, float cost)
            {
                var e = new Entry(index, cost);
                data.Add(e);
                int i = data.Count - 1;
                while (i > 0)
                {
                    int p = (i - 1) / 2;
                    if (!Less(e, data[p]))
                        break;
                    data[i] = data[p];
                    i = p;
                }
                data[i] = e;
            }
            public Entry Pop()
            {
                var result = data[0];
                var end = data[data.Count - 1];
                data.RemoveAt(data.Count - 1);
                if (data.Count == 0)
                    return result;
                int i = 0;
                while (i * 2 + 1 < data.Count)
                {
                    int c = i * 2 + 1;
                    if (c + 1 < data.Count && Less(data[c + 1], data[c]))
                        c++;
                    if (!Less(data[c], end))
                        break;
                    data[i] = data[c];
                    i = c;
                }
                data[i] = end;
                return result;
            }
        }
    }
}
