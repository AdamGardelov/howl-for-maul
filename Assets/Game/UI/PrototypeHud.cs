using UnityEngine;
using FrostMaze.Simulation;
namespace FrostMaze
{
    public sealed class PrototypeHud : MonoBehaviour
    {
        Prototype game;
        GUIStyle title, small, label, button, section;
        Texture2D panel;
        Vector2 scroll;
        public void Initialize(Prototype prototype)
        {
            game = prototype;
        }
        void Styles()
        {
            if (title != null)
                return;
            title = new GUIStyle(GUI.skin.label) { fontSize = 29, fontStyle = FontStyle.Bold };
            title.normal.textColor = new Color(0.85f, 0.96f, 0.97f);
            label = new GUIStyle(GUI.skin.label) { fontSize = 13, wordWrap = true };
            label.normal.textColor = new Color(0.78f, 0.86f, 0.9f);
            small = new GUIStyle(label) { fontSize = 11 };
            small.normal.textColor = new Color(0.49f, 0.67f, 0.73f);
            section = new GUIStyle(label) { fontStyle = FontStyle.Bold, fontSize = 11 };
            section.normal.textColor = new Color(0.26f, 0.87f, 0.74f);
            button = new GUIStyle(GUI.skin.button) { fontSize = 13, fixedHeight = 32 };
            panel = new Texture2D(1, 1);
            panel.SetPixel(0, 0, new Color(0.025f, 0.055f, 0.075f, 0.97f));
            panel.Apply();
        }
        void OnGUI()
        {
            if (game == null || game.World == null)
                return;
            Styles();
            var w = game.World;
            var previousMatrix = GUI.matrix;
            float scale = game.UiScale;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1));
            GUI.DrawTexture(new Rect(18, 18, 292, Screen.height / scale - 36), panel);
            GUILayout.BeginArea(new Rect(34, 30, 260, Screen.height / scale - 62));
            scroll = GUILayout.BeginScrollView(scroll);
            GUILayout.Label("FROSTMAZE", title);
            GUILayout.Label("MAZE LAB   /   VERTICAL SLICE 01", small);
            GUILayout.Space(18);
            GUILayout.Label("SHARED DEFENSE", section);
            GUILayout.Label($"Wave {Mathf.Max(0, w.WaveIndex + 1):00} / {w.Config.Waves.Length:00}     •     {w.Enemies.Count} active", label);
            GUILayout.Label($"{w.Pending} awaiting spawn   ·   {w.Killed} defeated   ·   {w.Leaked} leaked", small);
            GUILayout.Space(10);
            GUI.enabled = !w.WaveActive && w.WaveIndex + 1 < w.Config.Waves.Length;
            if (GUILayout.Button("LAUNCH WAVE     [SPACE]", button))
                game.Launch();
            GUI.enabled = true;
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(game.Paused ? "Resume [P]" : "Pause [P]", button))
                game.Paused = !game.Paused;
            if (GUILayout.Button(game.Speed == 1 ? "Speed  1×" : "Speed  2×", button))
                game.Speed = game.Speed == 1 ? 2 : 1;
            GUILayout.EndHorizontal();
            GUILayout.Space(16);
            GUILayout.Label("CONSTRUCTION", section);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(game.SellMode ? "Build [B]" : "● Build [B]", button))
                game.SellMode = false;
            if (GUILayout.Button(game.SellMode ? "● Sell [X]" : "Sell [X]", button))
                game.SellMode = true;
            GUILayout.EndHorizontal();
            GUILayout.Label("Click: place   /   Right click: sell\nShift + click: inspect an enemy", small);
            GUILayout.Space(8);
            GUILayout.Label(game.Notice, label);
            GUILayout.Space(16);
            GUILayout.Label("NAVIGATION OVERLAY", section);
            game.ShowGrid = GUILayout.Toggle(game.ShowGrid, "Placement grid [G]");
            game.ShowNavigation = GUILayout.Toggle(game.ShowNavigation, "Clearance + flow field [F]");
            game.ShowDirections = GUILayout.Toggle(game.ShowDirections, "Enemy intent + siege target");
            game.ShowValues = GUILayout.Toggle(game.ShowValues, "Distance at hovered cell");
            w.TowersFire = GUILayout.Toggle(w.TowersFire, "Tower weapons enabled");
            GUILayout.Label("Green: route   ·   Red: no clearance\nAmber: unreachable   ·   Purple: flight route\nRed enemies: route blocked, seeking breach", small);
            if (game.HasHover && game.ShowValues)
            {
                var f = w.Navigation.Get(w.Config.GroundRoute[0], w.Config.Waves[0].Radius);
                int at = f.Index(game.Hover + new V2(0.5f, 0.5f));
                GUILayout.Label($"Cell {game.Hover.X:0}, {game.Hover.Y:0}  |  Distance {f.Distance[at]:0.00}", label);
            }
            var e = w.Enemies.Find(enemy => enemy.Id == game.SelectedId);
            GUILayout.Space(12);
            GUILayout.Label("ENEMY INSPECTOR", section);
            if (e != null)
                GUILayout.Label($"#{e.Id}  {(e.Spec.Flying ? "AIR" : "GROUND")}   HP {e.Health:0}\nDestination {e.Destination}\n{(e.Blocked ? "BLOCKED" : "ROUTE OPEN")}  /  Tower {(e.BlockerId == 0 ? "—" : e.BlockerId.ToString())}\nRadius {e.Spec.Radius:0.00}  ·  Speed {e.Velocity.Length:0.00}", label);
            else
                GUILayout.Label("Shift + click an enemy to inspect it.", small);
            GUILayout.Space(16);
            GUILayout.Label("EXPERIMENTS", section);
            if (GUILayout.Button("Load zig-zag maze", button))
                game.DemoMaze();
            if (GUILayout.Button("Reset map + waves", button))
                game.ResetSimulation();
            GUILayout.Space(14);
            GUILayout.Label("WASD / arrows: pan\nWheel: zoom   ·   Middle drag: pan", small);
            GUILayout.Label($"Tick {w.Tick}  ·  Fields built {w.Navigation.Rebuilds}\nNavigation step {w.Config.NavigationStep:0.00}  ·  30 Hz simulation", small);
            GUILayout.EndScrollView();
            GUILayout.EndArea();
            GUI.matrix = previousMatrix;
            DrawHealth();
        }
        void DrawHealth()
        {
            foreach (var tower in game.World.Grid.Towers)
            {
                if (tower.Health >= tower.Spec.Health)
                    continue;
                var p = game.View.WorldToScreenPoint(new Vector3(tower.Center.X, 1.7f, tower.Center.Y));
                if (p.z <= 0)
                    continue;
                var rect = new Rect(p.x - 18, Screen.height - p.y, 36, 4);
                GUI.color = new Color(0.12f, 0.18f, 0.22f);
                GUI.DrawTexture(rect, Texture2D.whiteTexture);
                rect.width *= Mathf.Clamp01(tower.Health / tower.Spec.Health);
                GUI.color = new Color(0.28f, 0.9f, 0.73f);
                GUI.DrawTexture(rect, Texture2D.whiteTexture);
            }
            GUI.color = Color.white;
        }
        void OnDestroy()
        {
            if (panel != null)
                Destroy(panel);
        }
    }
}
