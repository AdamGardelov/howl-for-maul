using UnityEngine;
using FrostMaze.Simulation;
namespace FrostMaze
{
    public sealed class PrototypeHud : MonoBehaviour
    {
        Prototype game;
        GUIStyle title, small, label, button, section, mapLabel;
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
            mapLabel = new GUIStyle(small) { fontStyle = FontStyle.Bold };
            mapLabel.normal.textColor = new Color(.08f,.22f,.27f);
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
            GUILayout.Label(w.Config.Name.ToUpperInvariant(), small);
            GUILayout.Space(18);
            GUILayout.Label("SHARED DEFENSE", section);
            GUILayout.Label($"Wave {Mathf.Max(0, w.WaveIndex + 1):00} / {w.Config.Waves.Length:00}     •     {w.Enemies.Count} active", label);
            GUILayout.Label($"{w.Pending} awaiting spawn   ·   {w.Killed} defeated   ·   {w.Leaked} leaked", small);
            if (w.Config.Economy)
            {
                GUILayout.Label($"GOLD {w.Gold}    /    LIVES {w.Lives}", section);
                GUILayout.Label(w.Finished ? (w.Won ? "VICTORY — all waves cleared" : "DEFEAT — the crossing fell") : w.WaveActive ? w.Config.Waves[w.WaveIndex].Name : $"Next: {w.Config.Waves[Mathf.Min(w.WaveIndex + 1, w.Config.Waves.Length - 1)].Name}", label);
            }
            GUILayout.Space(10);
            GUI.enabled = !w.Finished && !w.WaveActive && w.WaveIndex + 1 < w.Config.Waves.Length;
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
            if (GUILayout.Button((game.SellMode || game.MoveMode) ? "Build [B]" : "● Build [B]", button))
                { game.SellMode = false; game.MoveMode = false; }
            if (GUILayout.Button(game.SellMode ? "● Sell [X]" : "Sell [X]", button))
                { game.SellMode = true; game.MoveMode = false; }
            GUILayout.EndHorizontal();
            if (w.Config.BuilderEnabled)
            {
                if (GUILayout.Button(game.MoveMode ? "● Move builder [M]" : "Move builder [M]", button)) { game.MoveMode = true; game.SellMode = false; }
                GUILayout.Label($"Tower: {w.Config.TowerCost}g  ·  Refund: {w.Config.SaleRefund}g\nKill: +{w.Config.KillReward}g  ·  Wave: +{w.Config.WaveReward}g", small);
                GUILayout.Label("Click: build order  ·  Right click: move\nOne order at a time  ·  Esc: cancel\nShift + click: inspect an enemy", small);
                GUILayout.Label(w.BuilderNotice, small);
            }
            else GUILayout.Label("Click: place   /   Right click: sell\nShift + click: inspect an enemy", small);
            GUILayout.Space(8);
            GUILayout.Label(game.Notice, label);
            GUILayout.Space(16);
            GUILayout.Label("NAVIGATION OVERLAY", section);
            game.ShowGrid = GUILayout.Toggle(game.ShowGrid, "Placement grid [G]");
            game.ShowNavigation = GUILayout.Toggle(game.ShowNavigation, "Clearance + low towers [F]");
            game.ShowDirections = GUILayout.Toggle(game.ShowDirections, "Enemy intent + siege target");
            game.ShowValues = GUILayout.Toggle(game.ShowValues, "Distance at hovered cell");
            w.TowersFire = GUILayout.Toggle(w.TowersFire, "Tower weapons enabled");
            GUILayout.Label("Low towers show collision footprints.\nOrange rings show ground-unit radii.\nGreen: route   ·   Red: no clearance\nAmber: unreachable   ·   Purple: flight route\nRed enemies: route blocked, seeking breach", small);
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
            if (!w.Config.Economy && GUILayout.Button("Load zig-zag maze", button))
                game.DemoMaze();
            if (GUILayout.Button("Reset map + waves", button))
                game.ResetSimulation();
            if (GUILayout.Button(w.Config.Economy ? "Switch to Maze Lab" : "Play Frostline Crossing", button))
                game.SwitchMap(!w.Config.Economy);
            GUILayout.Space(14);
            GUILayout.Label("WASD / arrows: pan\nWheel: zoom   ·   Middle drag: pan", small);
            GUILayout.Label($"Tick {w.Tick}  ·  Fields built {w.Navigation.Rebuilds}\nNavigation step {w.Config.NavigationStep:0.00}  ·  30 Hz simulation", small);
            GUILayout.EndScrollView();
            GUILayout.EndArea();
            GUI.matrix = previousMatrix;
            DrawHealth();
            DrawMapLabels();
        }
        void DrawMapLabels()
        {
            if (!game.World.Config.BuilderEnabled) return;
            string[] names = { "01  WESTWATCH", "02  THE CROSSING", "03  EASTWARD" };
            for (int i = 0; i < 3; i++)
            {
                var p = game.View.WorldToScreenPoint(new Vector3(7 + i * 14, .05f, 23));
                var rect = new Rect(p.x - 65, Screen.height - p.y, 150, 24);
                if (p.z > 0 && rect.x > game.Sidebar.xMax && game.View.pixelRect.Contains(new Vector2(p.x,p.y))) GUI.Label(rect, names[i], mapLabel);
            }
            var route = game.World.Config.GroundRoute;
            for (int i = 0; i < route.Length; i++)
            {
                var p = game.View.WorldToScreenPoint(new Vector3(route[i].X, .1f, route[i].Y));
                if (p.z > 0 && p.x > game.Sidebar.xMax + 20) GUI.Label(new Rect(p.x - 8, Screen.height - p.y - 24, 60, 20), i == route.Length - 1 ? "EXIT" : (i + 1).ToString(), mapLabel);
            }
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
