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
        readonly Dictionary<string, FlowField> cache = new Dictionary<string, FlowField>();
        int version = -1;
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
        public FlowField Get(V2 goal, float radius, bool breach = false, float goalRegion = 0)
        {
            if (version != grid.Version)
            {
                cache.Clear();
                version = grid.Version;
            }
            string key = goal.X.ToString("R", System.Globalization.CultureInfo.InvariantCulture) + "," + goal.Y.ToString("R", System.Globalization.CultureInfo.InvariantCulture) + "," + radius.ToString("R", System.Globalization.CultureInfo.InvariantCulture) + "," + breach + "," + goalRegion.ToString("R", System.Globalization.CultureInfo.InvariantCulture);
            if (!cache.TryGetValue(key, out var field))
            {
                field = Build(goal, radius, breach, goalRegion);
                cache.Add(key, field);
                Rebuilds++;
            }
            return field;
        }
        FlowField Build(V2 goal, float radius, bool breach, float goalRegion)
        {
            var f = new FlowField((int)Math.Ceiling(grid.Width / Step), (int)Math.Ceiling(grid.Height / Step), Step, radius, goal, breach, goalRegion);
            var heap = new MinHeap();
            // Multiple seeds avoid making an otherwise reachable checkpoint depend on one sample.
            for (int i = 0; i < f.Next.Length; i++)
                if (V2.Distance(f.Point(i), goal) <= (goalRegion>0?goalRegion:Step*1.5f)
                    && (goalRegion>0
                        ? grid.TerrainClear(f.Point(i),goal,radius) && (breach||grid.Clear(f.Point(i),f.Point(i),radius))
                        : grid.Clear(f.Point(i),goal,radius)))
                {
                    f.Distance[i] = V2.Distance(f.Point(i), goal);
                    heap.Push(i, f.Distance[i]);
                }
            while (heap.Count > 0)
            {
                var item = heap.Pop();
                int at = item.Index;
                if (item.Cost > f.Distance[at])
                    continue;
                int x = at % f.Columns, y = at / f.Columns;
                var a = f.Point(at);
                for (int dy = -1; dy <= 1; dy++)
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        if (dx == 0 && dy == 0)
                            continue;
                        int nx = x + dx, ny = y + dy;
                        if (nx < 0 || ny < 0 || nx >= f.Columns || ny >= f.Rows)
                            continue;
                        int n = nx + ny * f.Columns;
                        var b = f.Point(n);
                        if (!grid.TerrainClear(a,b,radius))
                            continue;
                        float cost = V2.Distance(a, b);
                        bool blocked = false;
                        foreach (var t in grid.Towers)
                            if (Geometry.SweepBox(a, b, t.Center, t.Half, radius))
                            {
                                if (!breach)
                                {
                                    blocked = true;
                                    break;
                                }
                                cost += BreachCost;
                            }
                        if (blocked)
                            continue;
                        float candidate = item.Cost + cost;
                        if (candidate + 0.00001f < f.Distance[n])
                        {
                            f.Distance[n] = candidate;
                            f.Next[n] = at;
                            heap.Push(n, candidate);
                        }
                    }
            }
            return f;
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
