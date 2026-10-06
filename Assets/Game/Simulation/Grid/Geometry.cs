using System;
namespace FrostMaze.Simulation
{
    [Serializable]
    public struct V2
    {
        public float X, Y;
        public V2(float x, float y)
        {
            X = x;
            Y = y;
        }
        public float Length => (float)Math.Sqrt(X * X + Y * Y);
        public float LengthSquared => X * X + Y * Y;
        public V2 Normalized => Length > 0.00001f ? this / Length : new V2();
        public static V2 operator +(V2 a, V2 b) => new V2(a.X + b.X, a.Y + b.Y);
        public static V2 operator -(V2 a, V2 b) => new V2(a.X - b.X, a.Y - b.Y);
        public static V2 operator *(V2 a, float n) => new V2(a.X * n, a.Y * n);
        public static V2 operator /(V2 a, float n) => new V2(a.X / n, a.Y / n);
        public static float Dot(V2 a, V2 b) => a.X * b.X + a.Y * b.Y;
        public static float Distance(V2 a, V2 b) => (a - b).Length;
        public override string ToString() => $"({X:0.00}, {Y:0.00})";
    }
    public static class Geometry
    {
        public static float Clamp(float x, float a, float b) => Math.Max(a, Math.Min(b, x));
        public static float PointSegment(V2 p, V2 a, V2 b)
        {
            var d = b - a;
            return V2.Distance(p, a + d * Clamp(V2.Dot(p - a, d) / Math.Max(0.000001f, d.LengthSquared), 0, 1));
        }
        static float Cross(V2 a, V2 b) => a.X * b.Y - a.Y * b.X;
        static float SegmentDistance(V2 a, V2 b, V2 c, V2 d)
        {
            var r = b - a;
            var s = d - c;
            float cross = Cross(r, s);
            if (Math.Abs(cross) > 0.000001f)
            {
                float t = Cross(c - a, s) / cross, u = Cross(c - a, r) / cross;
                if (t >= 0 && t <= 1 && u >= 0 && u <= 1)
                    return 0;
            }
            return Math.Min(Math.Min(PointSegment(a, c, d), PointSegment(b, c, d)), Math.Min(PointSegment(c, a, b), PointSegment(d, a, b)));
        }
        public static float PointBox(V2 p, V2 center, V2 half)
        {
            float x = Math.Max(0, Math.Abs(p.X - center.X) - half.X), y = Math.Max(0, Math.Abs(p.Y - center.Y) - half.Y);
            return (float)Math.Sqrt(x * x + y * y);
        }
        // Exact swept disc against an axis-aligned rectangle, including rounded clearance at corners.
        public static bool SweepBox(V2 a, V2 b, V2 center, V2 half, float radius)
        {
            // Conservative broad phase: distant rectangles cannot intersect the swept disc.
            // This keeps the exact rounded-corner test affordable with many towers.
            if (Math.Max(a.X, b.X) + radius <= center.X - half.X ||
                Math.Min(a.X, b.X) - radius >= center.X + half.X ||
                Math.Max(a.Y, b.Y) + radius <= center.Y - half.Y ||
                Math.Min(a.Y, b.Y) - radius >= center.Y + half.Y)
                return false;
            if (PointBox(a, center, half) < radius || PointBox(b, center, half) < radius)
                return true;
            var p = new V2(center.X - half.X, center.Y - half.Y);
            var q = new V2(center.X + half.X, center.Y - half.Y);
            var r = new V2(center.X + half.X, center.Y + half.Y);
            var s = new V2(center.X - half.X, center.Y + half.Y);
            return Math.Min(Math.Min(SegmentDistance(a, b, p, q), SegmentDistance(a, b, q, r)), Math.Min(SegmentDistance(a, b, r, s), SegmentDistance(a, b, s, p))) < radius;
        }
    }
}
