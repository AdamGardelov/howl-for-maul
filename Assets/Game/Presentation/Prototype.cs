using System.Collections.Generic;
using UnityEngine;
using FrostMaze.Simulation;
using FrostMaze.Simulation.Online;
namespace FrostMaze
{
    public sealed partial class Prototype : MonoBehaviour
    {
        static readonly Unity.Profiling.ProfilerMarker InputProfile=new Unity.Profiling.ProfilerMarker("Howl.Input");
        static readonly Unity.Profiling.ProfilerMarker SimulationProfile=new Unity.Profiling.ProfilerMarker("Howl.Simulation");
        static readonly Unity.Profiling.ProfilerMarker ViewsProfile=new Unity.Profiling.ProfilerMarker("Howl.Views");
        public MapDefinition Map;
        const string DefaultMap = "Ironfold";
        static string requestedMap = DefaultMap;
        WaveSummary observedWaveSummary;
        public bool MoveMode;
        public bool SetupOpen;
        public bool MainMenuOpen;
        readonly List<GameObject> routeMarkers=new List<GameObject>();
        static bool openingMapSelection;
        public void OpenMainMenu(){if(NetworkMatch)LeaveOnline();SetupOpen=true;MenuOpen=false;MainMenuOpen=true;}
        public bool MenuOpen, DetailsOpen;
        public void QuitGame() {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying=false;
#else
            Application.Quit();
#endif
        }
        public string PlacementFailure { get; private set; }
        public float PlacementFailureUntil { get; private set; }
        public bool SubmitBuild(float x,float y,bool append) {
            if(NetworkMatch){if(!World.CanBuild(x,y,out string invalid)){Notice=PlacementFailure=invalid;PlacementFailureUntil=Time.unscaledTime+5;return false;}Issue(new FrostMaze.Simulation.Online.Order{Kind=FrostMaze.Simulation.Online.ActionKind.Build,Design=World.SelectedDesign,X=x,Y=y,Append=append});PlacementFailure=null;return true;}
            bool accepted=World.OrderBuild(x,y,out string reason,append);Notice=reason;
            PlacementFailure=accepted?null:reason;PlacementFailureUntil=Time.unscaledTime+5;
            return accepted;
        }
        public void ToggleMenu() { MenuOpen=!MenuOpen;HasHover=false;if(ghost!=null)ghost.SetActive(false); }
        public Rect TopHud => new Rect(12*UiScale,12*UiScale,Screen.width-24*UiScale,48*UiScale);
        public int BuildColumns => World!=null&&World.Config.Theme=="iron"?4:3;
        public Rect DockHud => new Rect(12*UiScale,Screen.height-246*UiScale,Screen.width-24*UiScale,234*UiScale);
        public Rect BuildHud => new Rect(Screen.width-(BuildColumns*78+28)*UiScale,DockHud.y,(BuildColumns*78+16)*UiScale,234*UiScale);
        public Rect MinimapPanel => new Rect(12*UiScale,DockHud.y,202*UiScale,234*UiScale);
        public Rect SelectionHud { get { float width=Mathf.Min(480*UiScale,BuildHud.xMin-238*UiScale);return new Rect((Screen.width-width)*.5f,Screen.height-108*UiScale,width,96*UiScale); } }
        public Rect ForecastHud => World!=null&&!World.Finished&&!World.WaveActive&&!SetupOpen&&!DetailsOpen&&!MenuOpen&&!LobbyOpen
            ?new Rect(Screen.width-342*UiScale,68*UiScale,330*UiScale,76*UiScale):Rect.zero;
        public Rect AlertHud => (feedback!=null&&feedback.RecentLeaks>0)||(World!=null&&(World.Finished||!World.WaveActive&&World.LastWaveSummary!=null))?new Rect(12*UiScale,64*UiScale,340*UiScale,56*UiScale):Rect.zero;
        public bool PointerOverHud(Vector2 point) => MenuOpen||SetupOpen||Sidebar.Contains(point)||MinimapRect.Contains(point)||(!DetailsOpen&&(TopHud.Contains(point)||ForecastHud.Contains(point)||AlertHud.Contains(point)||(BuildHud.Contains(point)||MinimapPanel.Contains(point)||(World!=null&&World.Grid.Find(SelectedTowerId)!=null&&SelectionHud.Contains(point)))));
        bool matchStarted;
        public bool CanReturnToMatch => matchStarted;
        public void OpenSetup() { if(NetworkMatch){LeaveOnline();return;}SetupOpen=true;MenuOpen=false;MainMenuOpen=false; }
        public void ReturnToMatch() { if(CanReturnToMatch)SetupOpen=false; }
        void ClearInteraction()
        {
            MoveMode=false;SellMode=false;SelectedId=0;SelectedTowerId=0;HasHover=false;PlacementFailure=null;
            if(ghost!=null)ghost.SetActive(false);
        }
        public void CancelInteraction()
        {
            if(NetworkMatch)Issue(new FrostMaze.Simulation.Online.Order{Kind=FrostMaze.Simulation.Online.ActionKind.Cancel});else World.MoveBuilder(World.BuilderPosition);
            ClearInteraction();
        }
        public MapDefinition[] AvailableMaps;
        public void ChooseMap(MapDefinition map)
        {
            requestedMap=map.name;openingMapSelection=true;
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }
        public MatchOptions SetupOptions = new MatchOptions();
        public void StartMatch()
        {
            ClearUnitViews();
            World=new World(JsonUtility.FromJson<Scenario>(JsonUtility.ToJson(Map.Settings)),SetupOptions);
            SetupOpen=false;MenuOpen=false;DetailsOpen=false; matchStarted=true; Paused=false; localSpeedIndex=MatchSpeeds.Normal; accumulator=0; ClearInteraction();
            View.GetComponent<RtsCamera>().FocusPoint(World.BuilderPosition);
            Notice="All lanes active. Build your maze, then launch the first wave.";
        }
        public void FocusExit()
        {
            var route=World.Config.GroundRoute;
            if(route.Length>0)View.GetComponent<RtsCamera>().FocusPoint(route[route.Length-1]);
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
            requestedMap = shared ? "Rimewatch" : "TestMap";
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
        public bool ShowGrid, ShowNavigation, ShowDirections, ShowRoutes, Paused, ShowValues;
        public string Notice = "Build a maze, then launch a wave. Blocking every route is allowed.";
        int localSpeedIndex=MatchSpeeds.Normal;
        public int SpeedIndex=>NetworkMatch?Net.SpeedIndex:localSpeedIndex;
        public float Speed {
            get=>MatchSpeeds.At(SpeedIndex);
            set {for(int i=0;i<MatchSpeeds.Count;i++)if(Mathf.Approximately(value,MatchSpeeds.At(i))){SetSpeedIndex(i);return;}throw new System.ArgumentOutOfRangeException(nameof(value));}
        }
        public bool CanChangeSpeed=>World!=null&&!World.Finished&&!SetupOpen&&(!NetworkMatch||Net.IsHost);
        public void SetSpeedIndex(int index){if(!CanChangeSpeed||!MatchSpeeds.Valid(index))return;if(NetworkMatch)Net.Send(new Packet{Kind=Kind.Speed,A=index});else localSpeedIndex=index;}
        public void ChangeSpeed(int direction)=>SetSpeedIndex(Mathf.Clamp(SpeedIndex+direction,0,MatchSpeeds.Count-1));
        public int SelectedId, SelectedTowerId;
        public bool SoundEnabled=true;
        public float EffectsVolume=1, MusicVolume=.35f;
        public void ResetView()=>View.GetComponent<RtsCamera>().ResetRotation();
        public bool SellMode;
        public V2 Hover;
        public bool HasHover;
        public string HoverHint;
        public bool HoverBuildValid;
        public float UiScale => Mathf.Clamp(Mathf.Min(Screen.width / 1200f, Screen.height / 800f), 0.65f, 1f);
        public Rect MinimapRect => DetailsOpen||SetupOpen?Rect.zero:new Rect(28*UiScale,Screen.height-212*UiScale,178*UiScale,178*UiScale);
        public Rect Sidebar => !SetupOpen&&!DetailsOpen?Rect.zero:new Rect(18 * UiScale, 18 * UiScale, 324 * UiScale, Screen.height - 36 * UiScale);
        void SetViewport()
        {
            bool showMarkers=!(SetupOpen&&MainMenuOpen&&!LobbyOpen);
            foreach(var marker in routeMarkers)if(marker!=null&&marker.activeSelf!=showMarkers)marker.SetActive(showMarkers);
            View.rect = new Rect(0,0,1,1);
        }
        readonly Dictionary<int, GameObject> towers = new Dictionary<int, GameObject>();
        readonly Dictionary<int, EnemyView> enemies = new Dictionary<int, EnemyView>();
        // Reused across frames; removal happens only after dictionary enumeration finishes.
        readonly HashSet<int> liveViewIds = new HashSet<int>();
        readonly List<int> staleViewIds = new List<int>();
        readonly List<Material> materials = new List<Material>();
        public readonly ModelMeshes Models=new ModelMeshes();
        readonly Dictionary<int,Material[]> towerPalettes=new Dictionary<int,Material[]>();
        public Material[] TowerPalette(int faction)
        {
            if(towerPalettes.TryGetValue(faction,out var palette))return palette;
            Color[] winter={new Color(.3f,.82f,1),new Color(.58f,.72f,.36f),new Color(1,.4f,.12f),new Color(.62f,.47f,1)};
            Color color=World.Config.Theme=="iron"?Color.HSVToRGB((.54f+faction*.113f)%1,.68f,.95f):winter[faction%4];
            palette=new[]{MakeMaterial(World.Config.Theme=="iron"?new Color(.23f,.29f,.33f):new Color(.49f,.47f,.37f)),MakeMaterial(Color.Lerp(color,new Color(.4f,.44f,.42f),.23f)),MakeMaterial(Color.Lerp(color,Color.white,.4f),true),MakeMaterial(new Color(.68f,.7f,.64f)),MakeMaterial(new Color(.13f,.22f,.25f))};
            DressActorPalette(palette,World.Config.Theme=="iron",faction);
            towerPalettes.Add(faction,palette);return palette;
        }
        Material barricadeMaterial,cannonMaterial;
        Material[] factionMaterials;
        Material towerMaterial, enemyMaterial, airMaterial, blockedMaterial, ghostMaterial, enemyShellMaterial, slowMaterial, hitMaterial;
        CombatFeedback feedback;
        GameObject ghost, worldRoot, builder, orderMarker;
        float accumulator;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void InstallBootstrap()
        {
            requestedMap = DefaultMap;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
            // Data-only smoke exits immediately; it must not initialize transient graphics/audio.
            if(!StandaloneSmoke.Requested&&!StandaloneNetworkSmoke.Requested)UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
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
            SoundEnabled=PlayerPrefs.GetInt("Howl.Sound",1)!=0;
            EffectsVolume=Mathf.Clamp01(PlayerPrefs.GetFloat("Howl.Effects",1));
            MusicVolume=Mathf.Clamp01(PlayerPrefs.GetFloat("Howl.Music",.35f));
            if (Map == null)
                Map = Resources.Load<MapDefinition>(requestedMap);
            World = new World(Map != null ? JsonUtility.FromJson<Scenario>(JsonUtility.ToJson(Map.Settings)) : Scenario.SharedDefense());
            SetupOpen=World.Config.Lanes.Length>0;
            MainMenuOpen=SetupOpen&&!openingMapSelection;openingMapSelection=false;
            var maps=new List<MapDefinition>();
            foreach(var candidate in Resources.LoadAll<MapDefinition>(""))if(candidate.Settings.SelectableMap)maps.Add(candidate);
            // Keep the boot map first; resource enumeration order is not a menu order.
            maps.Sort((a,b)=>a.name==DefaultMap?(b.name==DefaultMap?0:-1):b.name==DefaultMap?1:System.StringComparer.OrdinalIgnoreCase.Compare(a.Settings.Name,b.Settings.Name));
            AvailableMaps=maps.ToArray();
            worldRoot = new GameObject("Procedural map");
            worldRoot.transform.SetParent(transform);
            factionMaterials=new[]{MakeMaterial(new Color(.2f,.72f,.92f)),MakeMaterial(new Color(.48f,.63f,.32f)),MakeMaterial(new Color(.95f,.34f,.12f)),MakeMaterial(new Color(.24f,.4f,.92f))};
            towerMaterial = MakeMaterial(new Color(0.10f, 0.32f, 0.40f));
            barricadeMaterial=MakeMaterial(new Color(.3f,.39f,.43f));
            cannonMaterial=MakeMaterial(new Color(.75f,.28f,.10f));
            enemyMaterial = MakeMaterial(new Color(1f, 0.48f, 0.23f));
            airMaterial = MakeMaterial(new Color(0.67f, 0.38f, 0.96f));
            blockedMaterial = MakeMaterial(new Color(1f, 0.17f, 0.24f));
            hitMaterial = MakeMaterial(new Color(1f,.92f,.65f),true);
            enemyShellMaterial = MakeMaterial(new Color(.12f, .18f, .25f));
            slowMaterial = MakeMaterial(new Color(.25f, .94f, 1f), true);
            ghostMaterial = MakeMaterial(new Color(0.24f, 0.9f, 0.74f));
            var floor = Primitive("Snowfield", PrimitiveType.Cube, new Vector3(World.Config.Width / 2f, -0.15f, World.Config.Height / 2f), new Vector3(World.Config.Width, 0.25f, World.Config.Height), MakeMaterial(World.Config.Theme=="iron"?new Color(.22f,.25f,.28f):new Color(.52f,.72f,.8f)));
            floor.layer=30;
            if (World.Config.BuilderEnabled)
            {
                if(World.Config.LayoutRows.Length>0)worldRoot.AddComponent<MapScenery>().Build(this);
                else foreach(var block in World.Config.Terrain)
                    Primitive("Frozen ridge",PrimitiveType.Cube,new Vector3(block.Center.X,.25f,block.Center.Y),new Vector3(block.Width,.6f,block.Height),MakeMaterial(new Color(.10f,.22f,.27f))).layer=30;
                builder = CreateBuilder("Builder drone");
                orderMarker = Primitive("Builder destination", PrimitiveType.Cylinder, Vector3.zero, new Vector3(.6f,.02f,.6f), MakeMaterial(new Color(.1f,.8f,.65f)));
                Notice = "Build near the route or across the flight corridor. The drone travels to your build orders.";
            }
            var exterior=new GameObject("Surrounding world");exterior.transform.SetParent(transform,false);var backdrop=exterior.AddComponent<WorldBackdrop>();backdrop.Build(this);
            GetComponentInChildren<MapScenery>()?.LightHearths(this,backdrop.RefugeHearths);
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
            sun.intensity = 1.0f;
            bool winter=World.Config.Theme!="iron";
            sun.color=winter?new Color(1,.94f,.82f):new Color(1,.84f,.65f);
            sun.shadowStrength=.58f;
            sun.shadowBias=.035f;
            sun.shadows = LightShadows.Soft;
            lightObject.transform.rotation = Quaternion.Euler(48, -35, 0);
            RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = winter?new Color(.46f,.58f,.66f):new Color(.43f,.49f,.58f);
            RenderSettings.ambientEquatorColor = winter?new Color(.34f,.43f,.47f):new Color(.38f,.37f,.30f);
            RenderSettings.ambientGroundColor = new Color(.2f,.23f,.24f);
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
            gameObject.AddComponent<BuildQueueView>().Initialize(this);
            gameObject.AddComponent<PrototypeHud>().Initialize(this);
            feedback=gameObject.AddComponent<CombatFeedback>();feedback.Initialize(this);
            gameObject.AddComponent<WorldAmbience>().Initialize(this);
            var music=new GameObject("Map soundtrack");music.transform.SetParent(transform,false);music.AddComponent<MapMusic>().Initialize(this);
        }
        void Marker(V2 p, Color color, string name, float radius = 0.55f)
        {
            routeMarkers.Add(Primitive(name, PrimitiveType.Cylinder, new Vector3(p.X, 0.015f, p.Y), new Vector3(radius, 0.025f, radius), MakeMaterial(color)));
        }
        public Material MakeMaterial(Color color, bool unlit = false)
        {
            var template = Resources.Load<Material>(unlit ? "DebugMaterial" : "PrototypeMaterial");
            var shader = Shader.Find(unlit ? "Universal Render Pipeline/Unlit" : "Universal Render Pipeline/Lit");
            var material = template != null ? new Material(template) : new Material(shader);
            material.color = color;
            if(material.HasProperty("_Smoothness"))material.SetFloat("_Smoothness",.12f);
            if(material.HasProperty("_Metallic"))material.SetFloat("_Metallic",0);
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
            if(LobbyOpen){SetupOpen=true;HasHover=false;ghost.SetActive(false);return;}
            if(NetworkMatch){Paused=Net.Paused;Notice=Net.Notice;if(MenuOpen&&UnityEngine.Input.GetKeyDown(KeyCode.P))VotePause();}
            if(UnityEngine.Input.GetKeyDown(KeyCode.Escape)) {
                if(SetupOpen){if(CanReturnToMatch)ReturnToMatch();else OpenMainMenu();}else ToggleMenu();
            }
            if(!SetupOpen&&!MenuOpen&&UnityEngine.Input.GetKeyDown(KeyCode.Tab))DetailsOpen=!DetailsOpen;
            if(!SetupOpen&&!MenuOpen)ReadBuildInput();
            else {HasHover=false;ghost.SetActive(false);}
            if (!NetworkMatch && !Paused && !SetupOpen && !MenuOpen)
            {
                accumulator += Time.deltaTime * Speed;
                int steps = 0;
                while (accumulator >= FrostMaze.Simulation.World.FixedDelta && steps++ < 12)
                {
                    using(SimulationProfile.Auto()) World.Step();
                    accumulator -= FrostMaze.Simulation.World.FixedDelta;
                }
            }
            else
                accumulator = 0;
            SyncViews();
        }
        void ReadBuildInput() { using(InputProfile.Auto()) ReadBuildInputProfiled(); }
        void ReadBuildInputProfiled()
        {
            int shortcut=0;for(int i=0;i<World.Config.Catalog.Length;i++)if(World.RosterVisible(i)){if(UnityEngine.Input.GetKeyDown(KeyCode.Alpha1+shortcut)){World.SelectedDesign=i;SellMode=false;MoveMode=false;}shortcut++;}
            if(UnityEngine.Input.GetKeyDown(KeyCode.Minus)||UnityEngine.Input.GetKeyDown(KeyCode.KeypadMinus))ChangeSpeed(-1);
            if(UnityEngine.Input.GetKeyDown(KeyCode.Equals)||UnityEngine.Input.GetKeyDown(KeyCode.KeypadPlus))ChangeSpeed(1);
            if(UnityEngine.Input.GetKeyDown(KeyCode.U)&&SelectedTowerId>0)UpgradeTower(SelectedTowerId);
            if(UnityEngine.Input.GetKeyDown(KeyCode.R))ResetView();
            if(UnityEngine.Input.GetKeyDown(KeyCode.Home)){var camera=View.GetComponent<RtsCamera>();camera.ResetRotation();camera.FocusPoint(World.BuilderPosition);}
            if(UnityEngine.Input.GetKeyDown(KeyCode.End))View.GetComponent<RtsCamera>().Overview();
            if (!UnityEngine.Input.GetKey(KeyCode.Space)&&(UnityEngine.Input.GetKeyDown(KeyCode.Return)||UnityEngine.Input.GetKeyDown(KeyCode.KeypadEnter)))
                Launch();
            if (UnityEngine.Input.GetKeyDown(KeyCode.P))
                VotePause();
            if (UnityEngine.Input.GetKeyDown(KeyCode.G))
                ShowGrid = !ShowGrid;
            if (UnityEngine.Input.GetKeyDown(KeyCode.F))
                ShowNavigation = !ShowNavigation;
            if (UnityEngine.Input.GetKeyDown(KeyCode.B))
                { SellMode = false; MoveMode = false; }
            if (UnityEngine.Input.GetKeyDown(KeyCode.X))
                { SellMode = true; MoveMode = false; }
            if (UnityEngine.Input.GetKeyDown(KeyCode.M)) { MoveMode = true; SellMode = false; }
            var mouse = UnityEngine.Input.mousePosition;
            var uiPoint = new Vector2(mouse.x, Screen.height - mouse.y);
            HasHover = false;
            ghost.SetActive(false);
            // Camera gestures must never place, sell, select or order the builder.
            if(UnityEngine.Input.GetKey(KeyCode.Space)||UnityEngine.Input.GetMouseButton(2))return;
            if(World.Config.Lanes.Length>0&&MinimapRect.Contains(uiPoint)) {
                if(UnityEngine.Input.GetMouseButton(0)) {
                    float mx=(uiPoint.x-MinimapRect.x)/MinimapRect.width,my=1-(uiPoint.y-MinimapRect.y)/MinimapRect.height;
                    View.GetComponent<RtsCamera>().Focus=new Vector3(mx*World.Config.Width,0,my*World.Config.Height);
                }
                return;
            }
            if (World.Finished||PointerOverHud(uiPoint))
                return;
            // Inspect the projected model before ground picking: a flying body can be over
            // a tower or have its ground-ray intersection outside the map at close perspective.
            if(!SellMode&&!MoveMode&&!UnityEngine.Input.GetMouseButton(1)&&
                (UnityEngine.Input.GetKey(KeyCode.LeftControl)||UnityEngine.Input.GetKey(KeyCode.RightControl))) {
                if(UnityEngine.Input.GetMouseButtonDown(0)) {
                    SelectedTowerId=0;SelectedId=EnemyAtScreenPoint(mouse);
                    Notice=SelectedId==0?"No enemy at pointer.":"Enemy selected.";
                }
                return;
            }
            var plane = new Plane(Vector3.up, Vector3.zero);
            if (!plane.Raycast(View.ScreenPointToRay(mouse), out float distance))
                return;
            var point = View.ScreenPointToRay(mouse).GetPoint(distance);
            var snapped=World.SnapBuildOrigin(new V2(point.x,point.z));
            float x=snapped.X,y=snapped.Y;
            if (x < 0 || y < 0 || x >= World.Grid.Width || y >= World.Grid.Height)
                return;
            Hover = new V2(x, y);
            HasHover = true;
            ghost.SetActive(true);
            ghost.transform.position = new Vector3(x + World.BuildSpec.Width * 0.5f, 0.04f, y + World.BuildSpec.Height * 0.5f);
            ghost.transform.localScale=new Vector3(World.BuildSpec.Width-.1f,.08f,World.BuildSpec.Height-.1f);
            HoverHint=null;HoverBuildValid=false;
            if(SellMode) {
                var target=World.Grid.At(x,y);
                HoverHint=target==null?"Remove mode · click your tower":World.TowerOwner(target.Id)!=World.ActivePlayer?"This tower belongs to another player":"Remove "+target.Name+" · refund "+World.SaleRefund(target.Id)+" gold";
            }
            if(!SellMode&&!MoveMode) {
                HoverBuildValid=World.CanBuild(x,y,out string reason);
                // Existing towers are selectable; do not label their occupied cell as a failed purchase.
                var existing=World.Grid.At(x,y);
                HoverHint=existing!=null?"Click to inspect "+existing.Name:HoverBuildValid?World.BuildName+(World.Config.Economy?" · "+World.BuildCost+" gold":" · free build"):reason;
                if(existing!=null)ghost.SetActive(false);
            }
            ghostMaterial.color = SellMode || (!MoveMode && !HoverBuildValid) ? new Color(1, 0.3f, 0.3f) : new Color(0.24f, 0.9f, 0.74f);
            if (World.Config.BuilderEnabled && (UnityEngine.Input.GetMouseButtonDown(1) || MoveMode && UnityEngine.Input.GetMouseButtonDown(0)))
            {
                MoveTo(new V2(point.x, point.z));
                Notice = "Builder moving. Select Build [B] to construct.";
                return;
            }
            if (UnityEngine.Input.GetMouseButtonDown(1) || UnityEngine.Input.GetMouseButtonDown(0) && SellMode)
            {
                Notice = SellTower(x, y) ? "Tower sold. Navigation updated." : "No owned tower at this cell.";
            }
            else if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                var tower=World.Grid.At(x,y);
                if(tower!=null){SelectedTowerId=tower.Id;SelectedId=0;Notice=tower.Name+" selected. U upgrades.";return;}
                SelectedTowerId=0;
                SubmitBuild(x,y,UnityEngine.Input.GetKey(KeyCode.LeftShift)||UnityEngine.Input.GetKey(KeyCode.RightShift));
            }
        }
        public int EnemyAtScreenPoint(Vector2 point)
        {
            if(point.x<0||point.y<0||point.x>=Screen.width||point.y>=Screen.height)return 0;
            int selected=0;float nearest=float.PositiveInfinity;
            foreach(var enemy in World.Enemies) {
                var position=new Vector3(enemy.Position.X,enemy.Spec.Flying?1.7f:enemy.Spec.Radius*.65f,enemy.Position.Y);
                var centre=View.WorldToScreenPoint(position);
                if(centre.z<=0||centre.x<0||centre.y<0||centre.x>=Screen.width||centre.y>=Screen.height)continue;
                var edge=View.WorldToScreenPoint(position+View.transform.right*Mathf.Max(.25f,enemy.Spec.Radius*1.8f));
                float radius=Mathf.Clamp(Mathf.Abs(edge.x-centre.x),10,40);
                float distance=((Vector2)centre-point).sqrMagnitude;
                if(distance<=radius*radius&&distance<nearest){nearest=distance;selected=enemy.Id;}
            }
            return selected;
        }
        public void Launch()
        {
            if(NetworkMatch){Issue(new FrostMaze.Simulation.Online.Order{Kind=FrostMaze.Simulation.Online.ActionKind.Launch});return;}
            Notice = World.StartWave() ? "Wave launched. You can edit the maze during combat." : World.WaveActive ? "Finish the current wave first." : World.Defeated ? "Defense lost. Reset for a new match." : "All waves complete. Reset to play again.";
        }
        void ClearUnitViews()
        {
            foreach(var view in towers.Values)Destroy(view);towers.Clear();
            foreach(var view in enemies.Values)Destroy(view.gameObject);enemies.Clear();
        }
        public void ResetSimulation()
        {
            if(NetworkMatch)return;
            ClearUnitViews();
            World = World.Restart();
            accumulator = 0;
            ClearInteraction();SetupOpen=false;matchStarted=true;
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
        void SyncViews() { using(ViewsProfile.Auto()) SyncViewsProfiled(); }
        void SyncViewsProfiled()
        {
            if(observedWaveSummary!=World.LastWaveSummary) {
                observedWaveSummary=World.LastWaveSummary;
                if(observedWaveSummary!=null)Notice=World.Won?"Victory! All waves survived.":World.Defeated?"Defense lost. Open Setup to start a new match.":"Wave finished. Build and upgrade before the next attack.";
            }
            if (builder != null)
            {
                TintBuilder(builder,World.Players[World.ActivePlayer].Faction);
                builder.GetComponent<BuilderView>().Sync(World.BuilderPosition,World.Tick,World.ActivePlayer);
                orderMarker.SetActive(V2.Distance(World.BuilderPosition, World.BuilderDestination) > .1f);
                orderMarker.transform.position = new Vector3(World.BuilderDestination.X,.03f,World.BuilderDestination.Y);
            }
            for(int i=0;i<World.Players.Length;i++) {
                if(i==World.ActivePlayer)continue;
                if(!extraBuilders.TryGetValue(i,out var drone)) {
                    drone=CreateBuilder("Player "+(i+1)+" builder");
                    extraBuilders.Add(i,drone);
                }
                drone.SetActive(World.Config.BuilderEnabled);
                TintBuilder(drone,World.Players[i].Faction);
                var pos=World.Players[i].Position;drone.GetComponent<BuilderView>().Sync(pos,World.Tick,i);
            }
            foreach(var pair in extraBuilders)if(pair.Key>=World.Players.Length||pair.Key==World.ActivePlayer)pair.Value.SetActive(false);
            liveViewIds.Clear();
            foreach (var t in World.Grid.Towers)
            {
                liveViewIds.Add(t.Id);
                if (!towers.TryGetValue(t.Id, out var obj))
                {
                    obj=new GameObject("Tower "+t.Id);
                    obj.transform.SetParent(worldRoot.transform,false);
                    var design=World.Config.Catalog.Length>t.Design?World.Config.Catalog[t.Design]:null;
                    int faction=0;
                    for(int f=0;f<World.Config.Factions.Length;f++)if(System.Array.IndexOf(World.Config.Factions[f].Designs,t.Design)>=0)faction=f;
                    obj.AddComponent<TowerView>().Initialize(this,t,design,faction);
                    towers.Add(t.Id,obj);
                }
                obj.GetComponent<TowerView>().Sync(t,ShowNavigation);

            }
            staleViewIds.Clear();
            foreach (var id in towers.Keys)if(!liveViewIds.Contains(id))staleViewIds.Add(id);
            foreach (var id in staleViewIds)
            {
                feedback.TowerStruck(towers[id].GetComponent<TowerView>().Subject,true);
                Destroy(towers[id]);
                towers.Remove(id);
            }
            liveViewIds.Clear();
            foreach (var e in World.Enemies)
            {
                liveViewIds.Add(e.Id);
                if (!enemies.TryGetValue(e.Id, out var obj))
                {
                    var root = new GameObject("Enemy " + e.Id);
                    root.transform.SetParent(worldRoot.transform, false);
                    obj = root.AddComponent<EnemyView>();
                    obj.Initialize(e, e.Spec.Flying ? airMaterial : enemyMaterial, blockedMaterial, enemyShellMaterial, slowMaterial,hitMaterial,Models);
                    enemies.Add(e.Id, obj);
                }
                obj.Sync(e, World.Tick);
            }
            staleViewIds.Clear();
            foreach (var id in enemies.Keys)if(!liveViewIds.Contains(id))staleViewIds.Add(id);
            foreach (var id in staleViewIds)
            {
                feedback.EnemyRemoved(enemies[id].Subject);
                Destroy(enemies[id].gameObject);
                enemies.Remove(id);
            }
        }
        GameObject CreateBuilder(string name)
        {
            var root=new GameObject(name);root.transform.SetParent(worldRoot.transform,false);
            root.AddComponent<BuilderView>().Configure(this,0);return root;
        }
        void TintBuilder(GameObject root,int faction)=>root.GetComponent<BuilderView>().Configure(this,faction);
        void OnDestroy()
        {
            PlayerPrefs.SetInt("Howl.Sound",SoundEnabled?1:0);
            PlayerPrefs.SetFloat("Howl.Effects",EffectsVolume);PlayerPrefs.SetFloat("Howl.Music",MusicVolume);PlayerPrefs.Save();
            Models.Dispose();
            foreach(var texture in actorTextures.Values)if(texture!=null)Destroy(texture);
            foreach (var material in materials)
                if (material != null)
                    Destroy(material);
        }
    }
}
