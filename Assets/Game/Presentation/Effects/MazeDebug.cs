using System.Collections.Generic;
using UnityEngine;
using FrostMaze.Simulation;
namespace FrostMaze
{
    public sealed class MazeDebug : MonoBehaviour
    {
        Prototype game;
        readonly List<Layer> layers = new List<Layer>();
        float timer;
        sealed class Layer
        {
            public GameObject Object; public Mesh Mesh; public List<Vector3> Points = new List<Vector3>();
            public void Line(V2 a, V2 b, float height = 0.08f)
            {
                Points.Add(new Vector3(a.X, height, a.Y));
                Points.Add(new Vector3(b.X, height, b.Y));
            }
            public void Arrow(V2 p, V2 direction, float height = 0.08f)
            {
                var end = p + direction;
                Line(p, end, height);
                var d = direction.Normalized;
                var side = new V2(-d.Y, d.X);
                Line(end, end - d * 0.13f + side * 0.09f, height);
                Line(end, end - d * 0.13f - side * 0.09f, height);
            }
            public void Upload()
            {
                Mesh.Clear();
                Mesh.SetVertices(Points);
                var indices = new int[Points.Count];
                for (int i = 0; i < indices.Length; i++)
                    indices[i] = i;
                Mesh.SetIndices(indices, MeshTopology.Lines, 0);
                Mesh.RecalculateBounds();
            }
        }
        public void Initialize(Prototype prototype)
        {
            game = prototype;
            Add("Placement grid", new Color(0.36f, 0.55f, 0.61f));
            Add("Reachable flow", new Color(0.12f, 0.55f, 0.46f));
            Add("No clearance", new Color(0.87f, 0.22f, 0.29f));
            Add("Unreachable", new Color(0.92f, 0.54f, 0.16f));
            Add("Enemy intent", new Color(0.08f, 0.32f, 0.85f));
            Add("Siege target", new Color(0.95f, 0.05f, 0.2f));
            Add("Route", new Color(0.53f, 0.22f, 0.8f));
        }
        void Add(string name, Color color)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(transform);
            var mesh = new Mesh { name = name };
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            obj.AddComponent<MeshFilter>().sharedMesh = mesh;
            obj.AddComponent<MeshRenderer>().sharedMaterial = game.MakeMaterial(color, true);
            layers.Add(new Layer { Object = obj, Mesh = mesh });
        }
        void Update()
        {
            timer -= Time.unscaledDeltaTime;
            if (timer > 0 || game == null || game.World == null)
                return;
            timer = 0.12f;
            foreach (var layer in layers)
                layer.Points.Clear();
            var w = game.World;
            if (game.ShowGrid)
            {
                for (int x = 0; x <= w.Grid.Width; x++)
                    layers[0].Line(new V2(x, 0), new V2(x, w.Grid.Height), 0.015f);
                for (int y = 0; y <= w.Grid.Height; y++)
                    layers[0].Line(new V2(0, y), new V2(w.Grid.Width, y), 0.015f);
            }
            if (game.ShowNavigation)
            {
                float radius = w.Config.Waves[0].Radius;
                V2 goal = w.Config.GroundRoute[0];
                var selected = w.Enemies.Find(e => e.Id == game.SelectedId);
                if (selected != null && !selected.Spec.Flying)
                {
                    radius = selected.Spec.Radius;
                    goal = selected.Destination;
                }
                var f = w.Navigation.Get(goal, radius);
                for (int i = 0; i < f.Next.Length; i++)
                {
                    var p = f.Point(i);
                    if (!w.Grid.Clear(p, p, radius))
                    {
                        layers[2].Line(p - new V2(0.07f, 0.07f), p + new V2(0.07f, 0.07f));
                        layers[2].Line(p - new V2(0.07f, -0.07f), p + new V2(0.07f, -0.07f));
                    }
                    else if (float.IsPositiveInfinity(f.Distance[i]))
                        layers[3].Line(p - new V2(0.06f, 0), p + new V2(0.06f, 0));
                    else
                        layers[1].Arrow(p, ((f.Next[i] >= 0 ? f.Point(f.Next[i]) : goal) - p).Normalized * f.Step * 0.65f);
                }
            }
            if (game.ShowDirections)
                foreach (var e in w.Enemies)
                {
                    float h = e.Spec.Flying ? 1.8f : 0.5f;
                    layers[4].Arrow(e.Position, e.IntendedDirection * 0.8f, h);
                    var target = w.Grid.Find(e.BlockerId);
                    if (target != null)
                        layers[5].Line(e.Position, target.Center, h);
                    if (e.Id == game.SelectedId)
                        layers[6].Line(e.Position, e.Destination, h);
                }
            V2 previous = w.Config.Spawn;
            foreach (var goal in w.Config.FlightRoute)
            {
                if (game.ShowNavigation)
                    layers[6].Line(previous, goal, 0.1f);
                previous = goal;
            }
            foreach (var layer in layers)
                layer.Upload();
        }
        void OnDestroy()
        {
            foreach (var layer in layers)
                if (layer.Mesh != null)
                    Destroy(layer.Mesh);
        }
    }
}
