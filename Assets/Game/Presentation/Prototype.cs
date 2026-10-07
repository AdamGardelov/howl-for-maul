using System.Collections.Generic;
using UnityEngine;
using FrostMaze.Simulation;
namespace FrostMaze
{
    public sealed class Prototype : MonoBehaviour
    {
        public MapDefinition Map;
        static string requestedMap = "Frostfall";
        public bool MoveMode;
        public bool SetupOpen;
        public MapDefinition[] AvailableMaps;
        public void ChooseMap(MapDefinition map)
        {
            requestedMap=map.name;
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }
        public MatchOptions SetupOptions = new MatchOptions();
        public void StartMatch()
        {
            World=new World(JsonUtility.FromJson<Scenario>(JsonUtility.ToJson(Map.Settings)),SetupOptions);
            SetupOpen=false; Paused=false; accumulator=0; SelectedId=0;SelectedTowerId=0;
            Notice="All lanes active. Build your maze, then launch the first wave.";
        }
        public void ChooseStart(int player,int position)
        {
            int previous=SetupOptions.StartingPositions[player];
            for(int i=0;i<SetupOptions.StartingPositions.Length;i++)if(i!=player&&SetupOptions.StartingPositions[i]==position)SetupOptions.StartingPositions[i]=previous;
            SetupOptions.StartingPositions[player]=position;
        }
        readonly Dictionary<int,GameObject> extraBuilders=new Dictionary<int,GameObject>();
        public void SwitchMap(bool shared)
        {
            requestedMap = shared ? "Frostfall" : "TestMap";
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }
        public World World
        {
            get; private set;
        }
        public Camera View
        {
            get; private set;
        }
        public bool ShowGrid = true, ShowNavigation, ShowDirections = true, Paused, ShowValues;
        public string Notice = "Build a maze, then launch a wave. Blocking every route is allowed.";
        public float Speed = 1;
        public int SelectedId, SelectedTowerId;
        public bool SoundEnabled=true;
        public bool SellMode;
        public V2 Hover;
        public bool HasHover;
        public float UiScale => Mathf.Clamp(Mathf.Min(Screen.width / 1200f, Screen.height / 800f), 0.65f, 1f);
        public Rect MinimapRect => new Rect(Screen.width-156*UiScale,Screen.height-184*UiScale,140*UiScale,156*UiScale);
        public Rect Sidebar => new Rect(18 * UiScale, 18 * UiScale, 292 * UiScale, Screen.height - 36 * UiScale);
        void SetViewport()
        {
            float inset = Mathf.Clamp((Sidebar.xMax + 12 * UiScale) / Screen.width, 0, 0.48f);
            View.rect = new Rect(inset, 0, 1 - inset, 1);
        }
        readonly Dictionary<int, GameObject> towers = new Dictionary<int, GameObject>();
        readonly Dictionary<int, GameObject> enemies = new Dictionary<int, GameObject>();
        readonly List<Material> materials = new List<Material>();
        Material barricadeMaterial,cannonMaterial;
        Material towerMaterial, enemyMaterial, airMaterial, blockedMaterial, ghostMaterial;
        GameObject ghost, worldRoot, builder, orderMarker;
        float accumulator;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void InstallBootstrap()
        {
            requestedMap = "Frostfall";
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }
        static void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            Boot();
        }
        static void Boot()
        {
            if (FindFirstObjectByType<Prototype>() == null)
                new GameObject("Howl for Maul").AddComponent<Prototype>();
        }
        void Awake()
        {
            if (Map == null)
                Map = Resources.Load<MapDefinition>(requestedMap);
            World = new World(Map != null ? JsonUtility.FromJson<Scenario>(JsonUtility.ToJson(Map.Settings)) : Scenario.SharedDefense());
            SetupOpen=World.Config.Lanes.Length>0;
            var maps=new List<MapDefinition>();
            foreach(var candidate in Resources.LoadAll<MapDefinition>(""))if(candidate.Settings.Lanes.Length>0)maps.Add(candidate);
            AvailableMaps=maps.ToArray();
            worldRoot = new GameObject("Procedural map");
            worldRoot.transform.SetParent(transform);
            towerMaterial = MakeMaterial(new Color(0.10f, 0.32f, 0.40f));
            barricadeMaterial=MakeMaterial(new Color(.3f,.39f,.43f));
            cannonMaterial=MakeMaterial(new Color(.75f,.28f,.10f));
            enemyMaterial = MakeMaterial(new Color(1f, 0.48f, 0.23f));
            airMaterial = MakeMaterial(new Color(0.67f, 0.38f, 0.96f));
            blockedMaterial = MakeMaterial(new Color(1f, 0.17f, 0.24f));
            ghostMaterial = MakeMaterial(new Color(0.24f, 0.9f, 0.74f));
            var floor = Primitive("Snowfield", PrimitiveType.Cube, new Vector3(World.Config.Width / 2f, -0.15f, World.Config.Height / 2f), new Vector3(World.Config.Width, 0.25f, World.Config.Height), MakeMaterial(new Color(0.72f, 0.83f, 0.86f), true));
            if (World.Config.BuilderEnabled)
            {
                foreach(var block in World.Config.Terrain)
                    Primitive("Frozen ridge",PrimitiveType.Cube,new Vector3(block.Center.X,.25f,block.Center.Y),new Vector3(block.Width,.6f,block.Height),MakeMaterial(new Color(.10f,.22f,.27f)));
                builder = Primitive("Builder drone", PrimitiveType.Capsule, Vector3.zero, new Vector3(.65f,.35f,.65f), MakeMaterial(new Color(.1f,.95f,.8f)));
                var wing = Primitive("Builder wings", PrimitiveType.Cube, Vector3.zero, new Vector3(1.2f,.12f,.22f), builder.GetComponent<Renderer>().sharedMaterial);
                wing.transform.SetParent(builder.transform, false);
                wing.transform.localPosition = Vector3.zero;
                orderMarker = Primitive("Builder destination", PrimitiveType.Cylinder, Vector3.zero, new Vector3(.6f,.02f,.6f), MakeMaterial(new Color(.1f,.8f,.65f)));
                Notice = "Build near the route or across the flight corridor. The drone travels to your build orders.";
            }
            var oldCamera = Camera.main;
            if (oldCamera != null)
                Destroy(oldCamera.gameObject);
            var cameraObject = new GameObject("RTS Camera");
            View = cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
            cameraObject.tag = "MainCamera";
            View.backgroundColor = new Color(0.035f, 0.065f, 0.09f);
            View.clearFlags = CameraClearFlags.SolidColor;
            View.nearClipPlane = 0.1f;
            View.farClipPlane = 150;
            SetViewport();
            cameraObject.AddComponent<RtsCamera>().Initialize(World.Config.Width, World.Config.Height);
            var lightObject = new GameObject("Winter sun");
            var sun = lightObject.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 1.8f;
            sun.shadows = LightShadows.Soft;
            lightObject.transform.rotation = Quaternion.Euler(50, -30, 0);
            RenderSettings.ambientLight = new Color(0.6f, 0.7f, 0.8f);
            for(int lane=0;lane<World.LaneCount;lane++) {
                Marker(World.LaneSpawn(lane),new Color(.1f,.85f,.68f),"Spawn lane "+(lane+1));
                if(World.Config.Lanes.Length>0)Marker(World.LaneRoute(lane,false)[0],new Color(.95f,.74f,.25f),"Lane merge");
            }
            foreach (var p in World.Config.GroundRoute)
                Marker(p, new Color(0.95f, 0.74f, 0.25f), "Ground checkpoint");
            foreach (var p in World.Config.FlightRoute)
                Marker(p, new Color(0.63f, 0.5f, 0.91f), "Flight checkpoint", 0.22f);
            ghost = Primitive("Placement preview", PrimitiveType.Cube, Vector3.zero, new Vector3(World.Config.Tower.Width - 0.1f, 0.08f, World.Config.Tower.Height - 0.1f), ghostMaterial);
            gameObject.AddComponent<MazeDebug>().Initialize(this);
            gameObject.AddComponent<PrototypeHud>().Initialize(this);
            gameObject.AddComponent<CombatFeedback>().Initialize(this);
        }
        void Marker(V2 p, Color color, string name, float radius = 0.55f)
        {
            Primitive(name, PrimitiveType.Cylinder, new Vector3(p.X, 0.015f, p.Y), new Vector3(radius, 0.025f, radius), MakeMaterial(color));
        }
        public Material MakeMaterial(Color color, bool unlit = false)
        {
            var template = Resources.Load<Material>(unlit ? "DebugMaterial" : "PrototypeMaterial");
            var shader = Shader.Find(unlit ? "Universal Render Pipeline/Unlit" : "Universal Render Pipeline/Lit");
            var material = template != null ? new Material(template) : new Material(shader);
            material.color = color;
            materials.Add(material);
            return material;
        }
        GameObject Primitive(string name, PrimitiveType kind, Vector3 position, Vector3 scale, Material material)
        {
            var obj = GameObject.CreatePrimitive(kind);
            obj.name = name;
            obj.transform.SetParent(worldRoot.transform);
            obj.transform.position = position;
            obj.transform.localScale = scale;
            obj.GetComponent<Renderer>().sharedMaterial = material;
            var collider = obj.GetComponent<Collider>();
            if (collider != null)
                Destroy(collider);
            return obj;
        }
        void Update()
        {
            // Unity does not serialize the authoritative simulation across an in-Play script reload.
            if (World == null)
            {
                enabled = false;
                Debug.LogWarning("Howl for Maul simulation was reset by script reload. Exit and re-enter Play mode.");
                return;
            }
            SetViewport();
            if(!SetupOpen)ReadBuildInput();
            else {HasHover=false;ghost.SetActive(false);}
            if (!Paused && !SetupOpen)
            {
                accumulator += Time.deltaTime * Speed;
                int steps = 0;
                while (accumulator >= FrostMaze.Simulation.World.FixedDelta && steps++ < 12)
                {
                    World.Step();
                    accumulator -= FrostMaze.Simulation.World.FixedDelta;
                }
            }
            else
                accumulator = 0;
            SyncViews();
        }
        void ReadBuildInput()
        {
            for(int i=0;i<World.Config.Catalog.Length;i++)if(UnityEngine.Input.GetKeyDown(KeyCode.Alpha1+i)){World.SelectedDesign=i;SellMode=false;MoveMode=false;}
            if(UnityEngine.Input.GetKeyDown(KeyCode.U)&&SelectedTowerId>0){World.Upgrade(SelectedTowerId,out string message);Notice=message;}
            if(UnityEngine.Input.GetKeyDown(KeyCode.Home))View.GetComponent<RtsCamera>().Focus=new Vector3(World.BuilderPosition.X,0,World.BuilderPosition.Y);
            if (UnityEngine.Input.GetKeyDown(KeyCode.Space))
                Launch();
            if (UnityEngine.Input.GetKeyDown(KeyCode.P))
                Paused = !Paused;
            if (UnityEngine.Input.GetKeyDown(KeyCode.G))
                ShowGrid = !ShowGrid;
            if (UnityEngine.Input.GetKeyDown(KeyCode.F))
                ShowNavigation = !ShowNavigation;
            if (UnityEngine.Input.GetKeyDown(KeyCode.B))
                { SellMode = false; MoveMode = false; }
            if (UnityEngine.Input.GetKeyDown(KeyCode.X))
                { SellMode = true; MoveMode = false; }
            if (UnityEngine.Input.GetKeyDown(KeyCode.M)) { MoveMode = true; SellMode = false; }
            if (UnityEngine.Input.GetKeyDown(KeyCode.Escape)) { World.MoveBuilder(World.BuilderPosition); MoveMode = false; SelectedTowerId=0; }
            var mouse = UnityEngine.Input.mousePosition;
            var uiPoint = new Vector2(mouse.x, Screen.height - mouse.y);
            HasHover = false;
            ghost.SetActive(false);
            if(World.Config.Lanes.Length>0&&MinimapRect.Contains(uiPoint)) {
                if(UnityEngine.Input.GetMouseButton(0)) {
                    float mx=(uiPoint.x-MinimapRect.x)/MinimapRect.width,my=1-(uiPoint.y-MinimapRect.y)/MinimapRect.height;
                    View.GetComponent<RtsCamera>().Focus=new Vector3(mx*World.Config.Width,0,my*World.Config.Height);
                }
                return;
            }
            if (Sidebar.Contains(uiPoint))
                return;
            var plane = new Plane(Vector3.up, Vector3.zero);
            if (!plane.Raycast(View.ScreenPointToRay(mouse), out float distance))
                return;
            var point = View.ScreenPointToRay(mouse).GetPoint(distance);
            int x = Mathf.FloorToInt(point.x), y = Mathf.FloorToInt(point.z);
            if (x < 0 || y < 0 || x >= World.Grid.Width || y >= World.Grid.Height)
                return;
            Hover = new V2(x, y);
            HasHover = true;
            ghost.SetActive(true);
            ghost.transform.position = new Vector3(x + World.BuildSpec.Width * 0.5f, 0.04f, y + World.BuildSpec.Height * 0.5f);
            ghost.transform.localScale=new Vector3(World.BuildSpec.Width-.1f,.08f,World.BuildSpec.Height-.1f);
            ghostMaterial.color = SellMode || (!MoveMode && !World.CanBuild(x, y, out _)) ? new Color(1, 0.3f, 0.3f) : new Color(0.24f, 0.9f, 0.74f);
            if (World.Config.BuilderEnabled && (UnityEngine.Input.GetMouseButtonDown(1) || MoveMode && UnityEngine.Input.GetMouseButtonDown(0)))
            {
                World.MoveBuilder(new V2(point.x, point.z));
                Notice = "Builder moving. Select Build [B] to construct.";
                return;
            }
            if (UnityEngine.Input.GetMouseButtonDown(1) || UnityEngine.Input.GetMouseButtonDown(0) && SellMode)
            {
                Notice = World.Sell(x, y) ? "Tower sold. Navigation updated." : "No owned tower at this cell.";
            }
            else if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                var tower=World.Grid.At(x,y);
                if(tower!=null){SelectedTowerId=tower.Id;SelectedId=0;Notice=tower.Name+" selected. U upgrades.";return;}
                SelectedTowerId=0;
                if (UnityEngine.Input.GetKey(KeyCode.LeftControl) || UnityEngine.Input.GetKey(KeyCode.RightControl))
                {
                    SelectedId = 0;
                    float best = 1;
                    foreach (var e in World.Enemies)
                    {
                        float d = V2.Distance(e.Position, new V2(point.x, point.z));
                        if (d < best)
                        {
                            SelectedId = e.Id;
                            best = d;
                        }
                    }
                }
                else
                {
                    World.OrderBuild(x, y, out string reason,UnityEngine.Input.GetKey(KeyCode.LeftShift)||UnityEngine.Input.GetKey(KeyCode.RightShift));
                    Notice = reason;
                }
            }
        }
        public void Launch()
        {
            Notice = World.StartWave() ? "Wave launched. You can edit the maze during combat." : World.WaveActive ? "Finish the current wave first." : World.Defeated ? "Defense lost. Reset for a new match." : "All waves complete. Reset to play again.";
        }
        public void ResetSimulation()
        {
            World = World.Restart();
            accumulator = 0;
            SelectedId = 0;SelectedTowerId=0;
            Paused = false;
            Notice = "Map reset. Build a new experiment.";
        }
        public void DemoMaze()
        {
            if (World.Config.Economy) { Notice = "Sample mazes are available in Maze Lab. Build your own defense here."; return; }
            if (World.WaveActive)
            {
                Notice = "Reset or finish the wave before loading the sample maze.";
                return;
            }
            foreach (var t in new List<Tower>(World.Grid.Towers))
                World.Grid.Remove(t.Id);
            for (int x = 6; x < World.Grid.Width - 3; x += 6)
                for (int y = 0; y < World.Grid.Height; y++)
                {
                    int gap = (x / 6) % 2 == 0 ? 2 : World.Grid.Height - 3;
                    if (y == gap || y == gap + 1)
                        continue;
                    World.Build(x, y, out _);
                }
            Notice = "Zig-zag loaded. Close an opening to test siege behavior.";
        }
        void SyncViews()
        {
            if (builder != null)
            {
                builder.transform.position = new Vector3(World.BuilderPosition.X, 1.1f, World.BuilderPosition.Y);
                orderMarker.SetActive(V2.Distance(World.BuilderPosition, World.BuilderDestination) > .1f);
                orderMarker.transform.position = new Vector3(World.BuilderDestination.X,.03f,World.BuilderDestination.Y);
            }
            for(int i=0;i<World.Players.Length;i++) {
                if(i==World.ActivePlayer)continue;
                if(!extraBuilders.TryGetValue(i,out var drone)) {
                    drone=Primitive("Player "+(i+1)+" builder",PrimitiveType.Capsule,Vector3.zero,new Vector3(.65f,.35f,.65f),MakeMaterial(Color.HSVToRGB(i*.23f,.7f,.85f)));
                    extraBuilders.Add(i,drone);
                }
                drone.SetActive(World.Config.BuilderEnabled);
                var pos=World.Players[i].Position;drone.transform.position=new Vector3(pos.X,1.1f,pos.Y);
            }
            foreach(var pair in extraBuilders)if(pair.Key>=World.Players.Length||pair.Key==World.ActivePlayer)pair.Value.SetActive(false);
            var live = new HashSet<int>();
            foreach (var t in World.Grid.Towers)
            {
                live.Add(t.Id);
                if (!towers.TryGetValue(t.Id, out var obj))
                {
                    var material=t.Design==1?barricadeMaterial:t.Design==2?cannonMaterial:towerMaterial;
                    obj = Primitive("Tower " + t.Id, PrimitiveType.Cube, new Vector3(t.Center.X, 0.6f, t.Center.Y), new Vector3(t.Half.X * 2, 1.2f, t.Half.Y * 2), material);
                    towers.Add(t.Id, obj);
                    var crown = Primitive("Crown", t.Design==2?PrimitiveType.Sphere:PrimitiveType.Cylinder, new Vector3(t.Center.X, 1.3f, t.Center.Y), new Vector3(0.4f, 0.13f, 0.4f), material);
                    if(t.Spec.Damage<=0)crown.SetActive(false);
                    crown.transform.SetParent(obj.transform, true);
                }
                float health = t.Health / t.Spec.Health;
                // Lower only the presentation during clearance inspection; collision stays unchanged.
                float height = ShowNavigation ? 0.12f : t.Design==1?.45f:0.65f + 0.35f * health + .15f*t.Level;
                obj.transform.position = new Vector3(t.Center.X, height * 0.5f, t.Center.Y);
                obj.transform.localScale = new Vector3(t.Half.X * 2, height, t.Half.Y * 2);
            }
            foreach (var id in new List<int>(towers.Keys))
                if (!live.Contains(id))
                {
                    Destroy(towers[id]);
                    towers.Remove(id);
                }
            live.Clear();
            foreach (var e in World.Enemies)
            {
                live.Add(e.Id);
                if (!enemies.TryGetValue(e.Id, out var obj))
                {
                    obj = Primitive("Enemy " + e.Id, PrimitiveType.Sphere, Vector3.zero, Vector3.one * e.Spec.Radius * 2, enemyMaterial);
                    enemies.Add(e.Id, obj);
                }
                obj.transform.position = new Vector3(e.Position.X, e.Spec.Flying ? 1.7f : e.Spec.Radius, e.Position.Y);
                obj.GetComponent<Renderer>().sharedMaterial = e.Blocked ? blockedMaterial : e.Spec.Flying ? airMaterial : enemyMaterial;
            }
            foreach (var id in new List<int>(enemies.Keys))
                if (!live.Contains(id))
                {
                    Destroy(enemies[id]);
                    enemies.Remove(id);
                }
        }
        void OnDestroy()
        {
            foreach (var material in materials)
                if (material != null)
                    Destroy(material);
        }
    }
}
