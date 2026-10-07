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
            Add("Tower collision footprints", new Color(0.04f, 0.12f, 0.18f));
            Add("Enemy collision radii", new Color(0.9f, 0.24f, 0.05f));
            Add("Checkpoint guide", new Color(.55f,.38f,.08f));
            Add("Construction range", new Color(.05f,.65f,.5f));
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
                foreach (var tower in w.Grid.Towers)
                {
                    var min = tower.Center - tower.Half;
                    var max = tower.Center + tower.Half;
                    layers[7].Line(min, new V2(max.X, min.Y), 0.14f);
                    layers[7].Line(new V2(max.X, min.Y), max, 0.14f);
                    layers[7].Line(max, new V2(min.X, max.Y), 0.14f);
                    layers[7].Line(new V2(min.X, max.Y), min, 0.14f);
                }
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
            if (w.Config.BuilderEnabled)
            {
                for(int lane=0;lane<w.LaneCount;lane++) {
                    previous=w.LaneSpawn(lane);
                    foreach(var goal in w.LaneRoute(lane,false)) {
                        var delta=goal-previous;
                        for(float d=0;d<delta.Length;d+=1.1f)layers[9].Line(previous+delta.Normalized*d,previous+delta.Normalized*Mathf.Min(d+.45f,delta.Length),.04f);
                        previous=goal;
                    }
                    previous=w.LaneSpawn(lane);
                    foreach(var goal in w.LaneRoute(lane,true)) {layers[6].Line(previous,goal,.05f);previous=goal;}
                }
            }
            foreach (var layer in layers)
                layer.Upload();
        }
        // Keep radius outlines aligned with this rendered frame, independently of cached flow overlays.
        void LateUpdate()
        {
            if (game == null || game.World == null || layers.Count < 9) return;
            layers[8].Points.Clear();
            if (game.ShowNavigation)
            {
                foreach (var enemy in game.World.Enemies)
                {
                    if (enemy.Spec.Flying) continue;
                    for (int segment = 0; segment < 24; segment++)
                    {
                        float a = segment * Mathf.PI * 2 / 24;
                        float b = (segment + 1) * Mathf.PI * 2 / 24;
                        layers[8].Line(enemy.Position + new V2(Mathf.Cos(a), Mathf.Sin(a)) * enemy.Spec.Radius,
                            enemy.Position + new V2(Mathf.Cos(b), Mathf.Sin(b)) * enemy.Spec.Radius, 0.03f);
                    }
                }
            }
            layers[8].Upload();
            layers[10].Points.Clear();
            var selectedTower=game.World.Grid.Find(game.SelectedTowerId);
            if (game.World.Config.BuilderEnabled && (game.HasHover||selectedTower!=null) && !game.SellMode && !game.MoveMode)
            {
                var center = selectedTower!=null?selectedTower.Center:game.Hover + new V2(game.World.BuildSpec.Width * .5f, game.World.BuildSpec.Height * .5f);
                float range=selectedTower!=null?selectedTower.Spec.Range:game.World.BuildSpec.Range;
                for (int segment=0;segment<48;segment++)
                {
                    float a=segment*Mathf.PI*2/48, b=(segment+1)*Mathf.PI*2/48;
                    layers[10].Line(center+new V2(Mathf.Cos(a),Mathf.Sin(a))*range,center+new V2(Mathf.Cos(b),Mathf.Sin(b))*range,.06f);
                }
            }
            layers[10].Upload();
        }
        void OnDestroy()
        {
            foreach (var layer in layers)
                if (layer.Mesh != null)
                    Destroy(layer.Mesh);
        }
    }
}
