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
            title = new GUIStyle(GUI.skin.label) { fontSize = 25, fontStyle = FontStyle.Bold };
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
            GUILayout.Label("HOWL FOR MAUL", title);
            GUILayout.Label(w.Config.Name.ToUpperInvariant(), small);
            if(game.SetupOpen) {
                DrawSetup();
                GUILayout.EndScrollView();
                if(GUILayout.Button("START MATCH",button))game.StartMatch();
                GUILayout.EndArea();GUI.matrix=previousMatrix;DrawMapLabels();return;
            }
            GUILayout.Space(18);
            GUILayout.Label("SHARED DEFENSE", section);
            if(w.Config.Lanes.Length>0) {
                GUILayout.Label($"{w.LaneCount} lanes active  ·  {w.Difficulty}",small);
                if(w.Players.Length>1) {
                    GUILayout.Label("Local player controls",small);
                    GUILayout.BeginHorizontal();
                    for(int i=0;i<w.Players.Length;i++)if(GUILayout.Button($"P{i+1}: {w.Players[i].Gold}g"))w.SelectPlayer(i);
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.Label($"Wave {Mathf.Max(0, w.WaveIndex + 1):00} / {w.Config.Waves.Length:00}     •     {w.Enemies.Count} active", label);
            if(w.Config.Lanes.Length>0 && !w.Finished) {
                int previewIndex=Mathf.Clamp(w.WaveIndex+(w.WaveActive?0:1),0,w.Config.Waves.Length-1);
                var preview=w.PreviewWave(previewIndex);
                GUILayout.Label($"{(preview.Flying?"AIR — ignores mazes":"GROUND")}  ·  {preview.Count}/lane  ·  {preview.Count*w.LaneCount} total",small);
                GUILayout.Label($"Health {preview.Health:0.#}  ·  Speed {preview.Speed:0.0}\nSiege hit {preview.Damage:0.#}  ·  Spawn every {preview.SpawnInterval:0.0}s",small);
                for(int upcoming=previewIndex;upcoming<w.Config.Waves.Length;upcoming++)if(w.Config.Waves[upcoming].Flying) {
                    GUILayout.Label(upcoming==previewIndex?"AIR WAVE — prepare towers that can hit air":$"Next air wave: {upcoming+1}",section);break;
                }
            }
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
            if(w.Config.Catalog.Length>0) {
                int shortcut=0;
                GUILayout.Label(w.FactionName,section);
                for(int i=0;i<w.Config.Catalog.Length;i++) {
                    if(!w.DesignAvailable(i))continue;shortcut++;
                    var design=w.Config.Catalog[i];
                    if(GUILayout.Button($"{(w.SelectedDesign==i?"● ":"")}{shortcut}. {design.Name}  {design.Cost}g{(w.RequirementsMet(i)?"":" [locked]")}",button)){w.SelectedDesign=i;game.SellMode=false;game.MoveMode=false;}
                }
                GUILayout.Label(w.Config.Catalog[w.SelectedDesign].Description,small);
                DrawTowerStats(w.BuildSpec);
                foreach(int missing in w.MissingPrerequisites(w.SelectedDesign))
                    GUILayout.Label("Requires your "+w.Config.Catalog[missing].Name,small);
            }
            GUILayout.BeginHorizontal();
            if (GUILayout.Button((game.SellMode || game.MoveMode) ? "Build [B]" : "● Build [B]", button))
                { game.SellMode = false; game.MoveMode = false; }
            if (GUILayout.Button(game.SellMode ? "● Sell [X]" : "Sell [X]", button))
                { game.SellMode = true; game.MoveMode = false; }
            GUILayout.EndHorizontal();
            if (w.Config.BuilderEnabled)
            {
                if (GUILayout.Button(game.MoveMode ? "● Move builder [M]" : "Move builder [M]", button)) { game.MoveMode = true; game.SellMode = false; }
                GUILayout.Label($"Tower: {w.BuildCost}g  ·  Select a tower to upgrade\nKill: +{w.Config.KillReward}g  ·  Wave: +{w.Config.WaveReward}g", small);
                GUILayout.Label("Click: build order  ·  Right click: move\nShift + click: queue builds  ·  Esc: cancel\nCtrl + click: inspect an enemy", small);
                GUILayout.Label($"Orders: {w.QueuedBuilds}  ·  {w.BuilderNotice}", small);
            }
            else GUILayout.Label("Click: place   /   Right click: sell\nCtrl + click: inspect an enemy", small);
            GUILayout.Space(8);
            GUILayout.Label(game.Notice, label);
            var selected=w.Grid.Find(game.SelectedTowerId);
            if(selected!=null) {
                GUILayout.Label($"{selected.Name}  ·  LEVEL {selected.Level}",section);
                GUILayout.Label($"HP {selected.Health:0}/{selected.Spec.Health:0}",small);
                DrawTowerStats(selected.Spec);
                if(selected.Level<3)GUILayout.Label($"Next level: damage {selected.Spec.Damage*1.6f:0.#} · range {selected.Spec.Range+.35f:0.0} · max HP {selected.Spec.Health*1.5f:0}",small);
                if(selected.Level<3&&GUILayout.Button($"Upgrade [U]  {w.UpgradeCost(selected)}g",button)){w.Upgrade(selected.Id,out string message);game.Notice=message;}
                if(GUILayout.Button("Sell selected tower",button))game.Notice=w.Sell(selected.CellX,selected.CellY)?"Sold.":"Select one of your own towers.";
            }
            GUILayout.Space(16);
            GUILayout.Label("NAVIGATION OVERLAY", section);
            game.SoundEnabled=GUILayout.Toggle(game.SoundEnabled,"Combat sound");
            game.ShowRoutes=GUILayout.Toggle(game.ShowRoutes,"Lane and flight route guides");
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
                GUILayout.Label("Ctrl + click an enemy to inspect it.", small);
            GUILayout.Space(16);
            GUILayout.Label("EXPERIMENTS", section);
            if (!w.Config.Economy && GUILayout.Button("Load zig-zag maze", button))
                game.DemoMaze();
            if(w.Config.Lanes.Length>0 && GUILayout.Button("New match / setup",button))game.SetupOpen=true;
            if (GUILayout.Button("Reset map + waves", button))
                game.ResetSimulation();
            if (GUILayout.Button(w.Config.Economy ? "Switch to Maze Lab" : "Play Howl for Maul", button))
                game.SwitchMap(!w.Config.Economy);
            GUILayout.Space(14);
            GUILayout.Label("WASD / arrows: pan\nWheel: zoom   ·   Middle drag: pan\nHome: focus selected builder", small);
            GUILayout.Label($"Tick {w.Tick}  ·  Fields built {w.Navigation.Rebuilds}\nNavigation step {w.Config.NavigationStep:0.00}  ·  30 Hz simulation", small);
            GUILayout.EndScrollView();
            GUILayout.EndArea();
            GUI.matrix = previousMatrix;
            DrawHealth();
            DrawMapLabels();
            DrawMinimap();
        }
        void DrawTowerStats(TowerSpec spec)
        {
            if(spec.Damage<=0){GUILayout.Label($"Maze piece · {spec.Health:0} HP · no weapon",small);return;}
            string targets=spec.TargetsGround?(spec.TargetsAir?"Ground + air":"Ground only"):"Air only";
            GUILayout.Label($"{targets} · {spec.Health:0} HP\n{spec.Damage:0.#} damage every {spec.Interval:0.00}s\n{spec.Damage/spec.Interval:0.#} direct DPS · range {spec.Range:0.0}",small);
            if(spec.SplashRadius>0)GUILayout.Label($"Splash radius {spec.SplashRadius:0.0}",small);
            if(spec.SlowFraction>0)GUILayout.Label($"Slow {spec.SlowFraction*100:0}% for {spec.SlowDuration:0.#}s · strongest slow wins",small);
            if(spec.ChainTargets>0)GUILayout.Label($"Chains to {spec.ChainTargets} extra targets within 2 units",small);
        }
        void DrawSetup()
        {
            GUILayout.Space(14);
            GUILayout.Label("MATCH SETUP",section);
            GUILayout.Label($"{game.World.LaneCount} upper lanes. One bottom exit. Every lane stays active at every player count.",label);
            if(game.AvailableMaps.Length>1) {
                GUILayout.Label("MAP",section);
                foreach(var map in game.AvailableMaps)if(GUILayout.Button(map.Settings.Name,button)&&map!=game.Map)game.ChooseMap(map);
            }
            GUILayout.Space(10);
            GUILayout.Label("PLAYERS",section);
            game.SetupOptions.PlayerCount=GUILayout.SelectionGrid(game.SetupOptions.PlayerCount-1,new[]{"1","2","3","4"},4)+1;
            GUILayout.Label("Solo or local control of multiple players. Online play is not available yet.",small);
            GUILayout.Space(10);
            GUILayout.Label("DIFFICULTY",section);
            game.SetupOptions.Difficulty=(Difficulty)GUILayout.SelectionGrid((int)game.SetupOptions.Difficulty,new[]{"Relaxed","Normal","Hard"},1);
            GUILayout.Label("Enemy health and siege damage: 70% / 100% / 140%. Lane counts stay unchanged.",small);
            GUILayout.Space(10);
            int n=game.SetupOptions.PlayerCount,total=game.World.Config.StartingGold;
            GUILayout.Label($"Team starting gold: {total}\nPer player: {total/n}  ·  Team income split evenly",label);
            if(n>1) {
                GUILayout.Label("STARTING POSITIONS",section);
                for(int i=0;i<n;i++) {
                    GUILayout.Label("Player "+(i+1),small);
                    int choice=GUILayout.SelectionGrid(game.SetupOptions.StartingPositions[i],game.World.Config.StartNames,2);
                    if(choice!=game.SetupOptions.StartingPositions[i])game.ChooseStart(i,choice);
                }
                GUILayout.Label("Choosing an occupied start swaps the players. You can build anywhere on open terrain.",small);
            } else GUILayout.Label("Solo builder starts at the shared junction with the full team budget.",small);
            if(game.World.Config.Factions.Length>0) {
                GUILayout.Space(12);GUILayout.Label("FACTION / BUILDER",section);
                var factions=game.World.Config.Factions;var names=new string[factions.Length];for(int j=0;j<names.Length;j++)names[j]=factions[j].Name;
                for(int player=0;player<n;player++) {
                    GUILayout.Label("Player "+(player+1),small);
                    game.SetupOptions.Factions[player]=GUILayout.SelectionGrid(game.SetupOptions.Factions[player],names,2);
                    GUILayout.Label(factions[game.SetupOptions.Factions[player]].Description,small);
                }
            }
            GUILayout.Space(14);
            if(GUILayout.Button("Open Maze Lab",button))game.SwitchMap(false);
        }
        void DrawMinimap()
        {
            var w=game.World;if(w.Config.Lanes.Length==0)return;
            var r=game.MinimapRect;
            GUI.color=new Color(.035f,.075f,.09f,.95f);GUI.DrawTexture(new Rect(r.x-4,r.y-18,r.width+8,r.height+22),Texture2D.whiteTexture);
            GUI.color=Color.white;GUI.Label(new Rect(r.x,r.y-18,r.width,18),"MAP · click to pan",small);
            GUI.color=new Color(.4f,.57f,.6f);GUI.DrawTexture(r,Texture2D.whiteTexture);
            foreach(var b in w.Grid.Terrain){GUI.color=new Color(.08f,.17f,.2f);GUI.DrawTexture(new Rect(r.x+b.X*r.width/w.Config.Width,r.y+(w.Config.Height-b.Y-b.Height)*r.height/w.Config.Height,b.Width*r.width/w.Config.Width,b.Height*r.height/w.Config.Height),Texture2D.whiteTexture);}
            foreach(var tower in w.Grid.Towers)MiniDot(r,tower.Center,new Color(.1f,.95f,.8f),2);
            foreach(var enemy in w.Enemies)MiniDot(r,enemy.Position,enemy.Spec.Flying?new Color(.85f,.4f,1):new Color(1,.48f,.2f),2);
            for(int i=0;i<w.Players.Length;i++)MiniDot(r,w.Players[i].Position,i==w.ActivePlayer?Color.white:Color.cyan,4);
            var focus=game.View.GetComponent<RtsCamera>().Focus;
            MiniDot(r,new V2(focus.x,focus.z),Color.yellow,3);
            GUI.color=Color.white;
        }
        void MiniDot(Rect rect,V2 point,Color color,float size)
        {
            GUI.color=color;GUI.DrawTexture(new Rect(rect.x+point.X/game.World.Config.Width*rect.width-size*.5f,rect.y+(1-point.Y/game.World.Config.Height)*rect.height-size*.5f,size,size),Texture2D.whiteTexture);
        }
        void DrawMapLabels()
        {
            if (!game.World.Config.BuilderEnabled) return;
            for (int i = 0; i < game.World.LaneCount; i++)
            {
                var spawn=game.World.LaneSpawn(i);
                var p = game.View.WorldToScreenPoint(new Vector3(spawn.X, .05f, spawn.Y));
                var rect = new Rect(p.x - 25, Screen.height - p.y - 22, 80, 24);
                if (p.z > 0 && rect.x > game.Sidebar.xMax) GUI.Label(rect, "LANE "+(i+1), mapLabel);
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
