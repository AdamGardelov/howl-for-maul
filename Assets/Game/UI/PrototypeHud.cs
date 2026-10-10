using UnityEngine;
using FrostMaze.Simulation;
namespace FrostMaze
{
    public sealed partial class PrototypeHud : MonoBehaviour
    {
        Prototype game; CombatFeedback feedback;
        GUIStyle title, small, label, button, section, mapLabel, card, selectedCard, primary, badge, number, alertButton, alertNumber, placementHint;
        readonly System.Collections.Generic.List<Texture2D> textures=new System.Collections.Generic.List<Texture2D>();
        Texture2D panel;
        readonly MinimapTerrain minimapTerrain=new MinimapTerrain();
        Vector2 scroll; bool wasSetup,showTools,showForecast;
        int lastSelectedTower;
        public void Initialize(Prototype prototype)
        {
            game = prototype;
        }
        Texture2D Swatch(Color color)
        {
            var texture=new Texture2D(1,1);texture.SetPixel(0,0,color);texture.Apply();textures.Add(texture);return texture;
        }
        void Styles()
        {
            if(title!=null)return;
            var displayFont=Resources.Load<Font>("Fonts/Cinzel-Bold");
            var bodyFont=Resources.Load<Font>("Fonts/AlegreyaSans-Medium");
            title=new GUIStyle(GUI.skin.label){font=displayFont,fontSize=21,fontStyle=FontStyle.Bold};title.normal.textColor=new Color(.9f,.91f,.83f);
            label=new GUIStyle(GUI.skin.label){font=bodyFont,fontSize=15,wordWrap=true};label.normal.textColor=new Color(.8f,.85f,.83f);
            small=new GUIStyle(label){fontSize=13};small.normal.textColor=new Color(.64f,.72f,.64f);
            section=new GUIStyle(label){font=displayFont,fontSize=11,fontStyle=FontStyle.Bold};section.normal.textColor=new Color(.54f,.81f,.71f);
            mapLabel=new GUIStyle(small){fontStyle=FontStyle.Bold,alignment=TextAnchor.MiddleCenter,wordWrap=false,padding=new RectOffset()};mapLabel.normal.textColor=Color.white;
            var surface=Swatch(new Color(.085f,.13f,.15f));var hover=Swatch(new Color(.14f,.22f,.23f));var active=Swatch(new Color(.18f,.32f,.29f));
            button=new GUIStyle(GUI.skin.button){font=bodyFont,fontSize=14,fixedHeight=30,border=new RectOffset(),padding=new RectOffset(8,8,5,5),margin=new RectOffset(2,2,3,3)};
            button.normal.background=surface;button.hover.background=hover;button.active.background=active;
            button.onNormal.background=active;button.onHover.background=hover;button.onActive.background=active;
            button.normal.textColor=button.hover.textColor=button.active.textColor=new Color(.86f,.91f,.88f);
            button.onNormal.textColor=button.onHover.textColor=button.onActive.textColor=new Color(.72f,1,.86f);
            primary=new GUIStyle(button){fixedHeight=34,fontStyle=FontStyle.Bold};primary.normal.background=active;
            card=new GUIStyle(button){fixedHeight=47,alignment=TextAnchor.MiddleLeft,richText=true,padding=new RectOffset(12,10,5,5)};
            selectedCard=new GUIStyle(card);selectedCard.normal.background=active;selectedCard.normal.textColor=new Color(.83f,1,.9f);
            badge=new GUIStyle(GUI.skin.box){normal={background=surface},padding=new RectOffset(8,8,6,6),margin=new RectOffset(2,2,3,3)};
            number=new GUIStyle(title){fontSize=20};number.normal.textColor=new Color(.92f,.81f,.52f);
            alertButton=new GUIStyle(primary){fixedHeight=40};
            alertButton.normal.background=Swatch(new Color(.27f,.095f,.12f));
            alertButton.normal.textColor=new Color(1,.77f,.7f);
            alertNumber=new GUIStyle(number);alertNumber.normal.textColor=new Color(1,.43f,.4f);
            panel=Swatch(new Color(.035f,.049f,.056f,.98f));
            section.normal.textColor=new Color(.79f,.73f,.55f);
            title.normal.textColor=new Color(.95f,.9f,.75f);
            button.border=primary.border=card.border=selectedCard.border=new RectOffset(8,8,8,8);
            var idle=Beveled(new Color(.115f,.145f,.123f),new Color(.42f,.43f,.32f));
            var lit=Beveled(new Color(.21f,.26f,.19f),new Color(.88f,.71f,.42f));
            var down=Beveled(new Color(.045f,.065f,.066f),new Color(.62f,.48f,.25f),true);
            foreach(var style in new[]{button,primary,card,selectedCard}){
                style.normal.background=idle;style.hover.background=lit;style.active.background=down;
                style.focused.background=lit;style.onNormal.background=lit;style.onHover.background=lit;style.onActive.background=down;
                style.normal.textColor=new Color(.88f,.85f,.74f);style.hover.textColor=new Color(1,.94f,.75f);
                style.active.textColor=new Color(.85f,.77f,.57f);style.focused.textColor=new Color(1,.94f,.75f);
            }
            primary.normal.background=Beveled(new Color(.23f,.27f,.18f),new Color(.83f,.65f,.33f));
            primary.normal.textColor=new Color(1,.92f,.69f);
            card.normal.background=Beveled(new Color(.066f,.095f,.078f),new Color(.28f,.33f,.26f));
            selectedCard.normal.background=lit;selectedCard.normal.textColor=new Color(1,.91f,.64f);
            alertButton.normal.background=Beveled(new Color(.23f,.065f,.05f),new Color(.66f,.32f,.17f));
            alertButton.hover.background=lit;alertButton.active.background=down;alertButton.border=new RectOffset(8,8,8,8);
            placementHint=new GUIStyle(label){fontSize=14,border=new RectOffset(8,8,8,8),padding=new RectOffset(12,12,9,9),normal={background=card.normal.background}};
            HearthControls();
        }
        void Rule()
        {
            GUILayout.Space(6);var r=GUILayoutUtility.GetRect(1,1);var old=GUI.color;GUI.color=new Color(.23f,.34f,.34f);GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=old;GUILayout.Space(8);
        }
        void Resource(string name,string value,bool danger=false)
        {
            GUILayout.BeginVertical(badge);GUILayout.Label(name,small);GUILayout.Label(value,danger?alertNumber:number);GUILayout.EndVertical();
        }
        static string Role(TowerSpec spec)
        {
            if(spec.Damage<=0)return "MAZE WALL";
            string role=spec.ChainTargets>0?"CHAIN":spec.SlowFraction>0?"CONTROL":spec.SplashRadius>0?"SPLASH":"DIRECT";
            return role+" / "+(spec.TargetsGround?(spec.TargetsAir?"GROUND + AIR":"GROUND"):"AIR");
        }
        static readonly Unity.Profiling.ProfilerMarker PhaseProfile=new Unity.Profiling.ProfilerMarker("Howl.HUD");
        void OnGUI() { using(PhaseProfile.Auto()) {
            if(game==null)return;
            Styles();var previousSkin=GUI.skin;GUI.skin=hearthSkin;
            // A full-screen event shield prevents click-through even in legacy draw branches
            // which locally re-enable controls. Modal controls receive the original event.
            var e=Event.current;bool shield=(game.ResultOpen||game.ChatOpen)&&(e.isMouse||e.isKey);
            EventType saved=e.type;if(shield)e.type=EventType.Ignore;
            try{DrawHud();if(shield)e.type=saved;DrawResult();DrawChat();}
            finally{GUI.skin=previousSkin;}
        } }
        void DrawHud()
        {
            if(game==null||game.World==null)return;
            Styles();
            if(game.SetupOpen&&game.MainMenuOpen&&!game.LobbyOpen){DrawTitleScreen();return;}
            if(game.LobbyOpen){DrawLobby();return;}
            if(feedback==null)feedback=game.GetComponent<CombatFeedback>();
            if(wasSetup!=game.SetupOpen){scroll=Vector2.zero;wasSetup=game.SetupOpen;}
            if(lastSelectedTower!=game.SelectedTowerId){if(game.SelectedTowerId!=0)scroll=Vector2.zero;lastSelectedTower=game.SelectedTowerId;}
            if(game.MenuOpen){DrawPauseMenu();return;}
            if(!game.SetupOpen&&!game.DetailsOpen){DrawCompact();DrawVoteStatus();return;}
            var w=game.World;var previousMatrix=GUI.matrix;float scale=game.UiScale;
            GUI.matrix=Matrix4x4.Scale(new Vector3(scale,scale,1));
            Frame(new Rect(18,18,324,Screen.height/scale-36));
            GUILayout.BeginArea(new Rect(32,28,296,Screen.height/scale-52));
            if(!game.SetupOpen&&HudButton("CLOSE DETAILS [TAB]",button))game.DetailsOpen=false;
            if(game.SetupOpen)BrandHeading(130);else GUILayout.Label("HOWL FOR MAUL",title);
            GUILayout.Label(w.Config.Name.ToUpperInvariant()+"  /  "+(game.SetupOpen?"MATCH SETUP":w.FactionName.ToUpperInvariant()),small);
            if(game.SetupOpen) {
                scroll=GUILayout.BeginScrollView(scroll);if(!onlineForm)DrawSetup();GUILayout.Space(12);DrawOnlineEntry();GUILayout.EndScrollView();
                if(game.CanReturnToMatch&&HudButton("RETURN TO MATCH",button))game.ReturnToMatch();
                if(!onlineForm&&HudButton(game.SetupOptions.PlayerCount==1?"PLAY SOLO":"START LOCAL SLOT TEST",primary)){if(game.SetupOptions.PlayerCount==1)game.BeginSolo();else game.StartMatch();}
                if(HudButton("MAIN MENU",button)){onlineForm=false;game.OpenMainMenu();}
                GUILayout.EndArea();GUI.matrix=previousMatrix;DrawMapLabels();return;
            }
            GUILayout.BeginHorizontal();Resource("YOUR GOLD",w.Gold.ToString());Resource("WOOD",w.Wood.ToString());Resource("TEAM LIVES",w.Lives.ToString(),w.Lives<=5||(feedback!=null&&feedback.RecentLeaks>0));Resource("WAVE",Mathf.Max(0,w.WaveIndex+1)+" / "+w.Config.Waves.Length);GUILayout.EndHorizontal();
            GUILayout.Label(w.Finished?(w.Won?"VICTORY — all waves cleared":"DEFEAT — the crossing fell"):$"{w.LaneCount} lanes active  ·  {w.Difficulty}  ·  {w.Enemies.Count} enemies",section);
            GUI.enabled=!w.Finished&&!w.WaveActive&&w.WaveIndex+1<w.Config.Waves.Length;
            if(HudButton(w.Finished?"MATCH COMPLETE":w.WaveActive?"WAVE IN PROGRESS":w.CountingDown?(game.ChatAvailable?"SEND NOW · ":"SEND NOW [ENTER] · ")+w.NextWaveSeconds+"s":(game.ChatAvailable?"START WAVE 1":"START WAVE 1     [ENTER]"),primary))game.Launch();
            GUI.enabled=true;
            if(w.CountingDown)GUILayout.Label("Next wave starts automatically · "+w.NextWaveSeconds+"s · countdown follows game speed",small);
            GUILayout.BeginHorizontal();if(HudButton(game.Paused?"Resume [P]":"Pause [P]",button))game.VotePause();
            if(HudButton(w.Finished?"New match":"Setup",button))game.OpenSetup();GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();DrawSpeedControls();GUILayout.EndHorizontal();
            if(feedback!=null&&feedback.RecentLeaks>0) {
                if(HudButton($"EXIT BREACHED · {feedback.RecentLeaks} leaked\nView exit",alertButton))game.FocusExit();
            } else GUILayout.Label(game.Notice,small);
            if(w.LastWaveSummary!=null) {
                var result=w.LastWaveSummary;
                GUILayout.BeginVertical(badge);
                GUILayout.Label(w.Won?"VICTORY":!result.Cleared?"DEFENSE LOST — WAVE "+result.WaveNumber:"WAVE "+result.WaveNumber+(result.Leaked==0?" CLEARED":" FINISHED"),section);
                GUILayout.Label($"{result.Killed} defeated · {result.Leaked} leaked",label);
                if(w.Config.Economy) {
                    GUILayout.Label(w.Players.Length==1?$"Earned {result.GoldForPlayer(0)} gold":$"Your income: {result.GoldForPlayer(w.ActivePlayer)}g · team: {result.TeamGold}g",label);
                    GUILayout.Label(result.Cleared?"Kill bounty + wave bonus":"Kill bounty; no completion bonus",small);
                }
                GUILayout.EndVertical();
            }
            Rule();
            scroll=GUILayout.BeginScrollView(scroll);
            if(!game.NetworkMatch&&w.Players.Length>1){GUILayout.Label("LOCAL PLAYER",section);GUILayout.BeginHorizontal();for(int i=0;i<w.Players.Length;i++)if(HudButton($"{(i==w.ActivePlayer?"• ":"")}P{i+1}  {w.Players[i].Gold}g",button))w.SelectPlayer(i);GUILayout.EndHorizontal();}
            if(!w.Finished) {
                int index=Mathf.Clamp(w.WaveIndex+(w.WaveActive?0:1),0,w.Config.Waves.Length-1);var preview=w.PreviewWave(index);
                GUILayout.Label((w.WaveActive?"CURRENT: ":"NEXT: ")+preview.Name+" · "+EnemyIdentity.Name(EnemyIdentity.For(preview)),section);
                GUILayout.Label($"{(preview.Flying?"AIR · ignores mazes":"GROUND")}  /  {preview.Count*w.LaneCount} enemies  /  {preview.Health:0.#} HP",label);
                for(int i=index;i<w.Config.Waves.Length;i++)if(w.Config.Waves[i].Flying){GUILayout.Label(i==index?"Prepare towers that can hit air.":$"Next flying attack: wave {i+1}",small);break;}
                if(HudButton(showForecast?"Hide wave details":"Wave details",button))showForecast=!showForecast;
                int defenses=w.DefensesFor(preview);
                GUILayout.Label(defenses==0?$"NO TEAM TOWERS CAN HIT {(preview.Flying?"AIR":"GROUND")}":$"Team defense: {defenses} towers can hit {(preview.Flying?"air":"ground")}",defenses==0?section:small);
                if(showForecast)GUILayout.Label(w.WaveActive?w.WaveAdvice(preview):w.WaveAdvice(index),label);
                if(showForecast)GUILayout.Label($"{preview.Count} per lane · speed {preview.Speed:0.0}\nSiege hit {preview.Damage:0.#} · spawn every {preview.SpawnInterval:0.0}s\n{w.Pending} awaiting spawn · {w.Killed} defeated · {w.Leaked} leaked",small);
            }
            var selected=w.Grid.Find(game.SelectedTowerId);
            if(selected!=null) {
                Rule();GUILayout.Label("SELECTED TOWER",section);GUILayout.Label(selected.Name+"  /  LEVEL "+selected.Level,label);
                int owner=w.TowerOwner(selected.Id);bool own=owner==w.ActivePlayer;
                GUILayout.Label(owner<0?"Unclaimed tower":own?$"YOUR TOWER · P{owner+1}":$"PLAYER {owner+1} · switch to P{owner+1} to manage",section);
                DrawTowerStats(selected.Spec);GUILayout.Label($"Current health {selected.Health:0}/{selected.Spec.Health:0}",small);
                if(selected.Level<3) {
                    GUILayout.Label($"Next: {selected.Spec.Damage*1.6f:0.#} damage · {selected.Spec.Range+.35f:0.0} range · {selected.Spec.Health*1.5f:0} HP",small);
                    int price=w.UpgradeCost(selected);
                    bool affordable=!w.Config.Economy||w.Gold>=price;
                    GUI.enabled=own&&affordable&&!w.Finished;
                    if(HudButton($"UPGRADE [U]   /   {price} GOLD",primary))game.UpgradeTower(selected.Id);
                    GUI.enabled=true;
                    if(own&&!affordable)GUILayout.Label($"Need {price-w.Gold} more gold to upgrade.",small);
                } else GUILayout.Label("MAXIMUM LEVEL",section);
                GUILayout.BeginHorizontal();GUI.enabled=!w.Finished&&(own||owner<0);
                if(HudButton(w.Config.Economy?$"Sell / {w.SaleRefund(selected.Id)} gold":"Remove tower",button))game.Notice=game.SellTower(selected.CellX,selected.CellY)?"Sold.":"Select one of your own towers.";
                GUI.enabled=true;
                if(HudButton("Deselect",button))game.SelectedTowerId=0;GUILayout.EndHorizontal();
            }
            Rule();GUILayout.Label("WOOD / FACTIONS",section);
            GUILayout.Label(w.Config.FactionWoodUnlocks?"Clear wave 9: wood unlocks another faction. Switching between unlocked rosters is free.":"Clear wave 14: wood buys champions. Each needs 750g, 1 wood and six owned prerequisites. Selling returns its wood.",small);
            if(w.Config.FactionWoodUnlocks)for(int f=0;f<w.Config.Factions.Length;f++) {
                GUI.enabled=!w.Finished&&(w.FactionUnlocked(f)||w.Wood>0||!w.Config.Economy);
                if(HudButton(w.Config.Factions[f].Name+(w.Players[w.ActivePlayer].Faction==f?" · ACTIVE":w.FactionUnlocked(f)?" · SWITCH":" · 1 WOOD"),button))game.ChooseFaction(f);
            }
            GUI.enabled=true;
            Rule();GUILayout.Label("BUILD  /  "+w.FactionName.ToUpperInvariant(),section);
            GUILayout.BeginHorizontal();
            if(HudButton(!game.SellMode&&!game.MoveMode?"• Build [B]":"Build [B]",button)){game.SellMode=false;game.MoveMode=false;}
            if(HudButton(game.SellMode?"• Sell [X]":"Sell [X]",button)){game.SellMode=true;game.MoveMode=false;}
            if(w.Config.BuilderEnabled&&HudButton(game.MoveMode?"• Move [M]":"Move [M]",button)){game.MoveMode=true;game.SellMode=false;}
            GUILayout.EndHorizontal();
            if(w.Config.Catalog.Length>0) {
                int shortcut=0;for(int i=0;i<w.Config.Catalog.Length;i++) {
                    if(!w.RosterVisible(i))continue;shortcut++;var design=w.Config.Catalog[i];bool ready=w.RequirementsMet(i);
                    string availability=!ready?" · LOCKED":w.Config.Economy&&w.Gold<design.Cost?" · NEED GOLD":w.Config.Economy&&w.Wood<design.WoodCost?" · NEED WOOD":"";
                    string text=$"<b>{shortcut}  {design.Name}</b>    {design.Cost}g{(design.WoodCost>0?" + 1 wood":"")}\n<size=10>{Role(design.Spec)}{availability}</size>";
                    if(HudButton(text,w.SelectedDesign==i?selectedCard:card)){w.SelectedDesign=i;game.SellMode=false;game.MoveMode=false;}
                }
                GUILayout.Space(5);GUILayout.Label(w.Config.Catalog[w.SelectedDesign].Description,small);DrawTowerStats(w.BuildSpec);
                foreach(int missing in w.MissingPrerequisites(w.SelectedDesign))GUILayout.Label("Requires: "+w.Config.Catalog[missing].Name,small);
            }
            if(w.Config.BuilderEnabled)GUILayout.Label($"Orders: {w.QueuedBuilds} · {w.BuilderNotice}",small);

            Rule();GUILayout.Label("OPTIONS & CONTROLS",section);
            game.SoundEnabled=GUILayout.Toggle(game.SoundEnabled,"Combat sound",button);game.ShowGrid=GUILayout.Toggle(game.ShowGrid,"Placement grid [G]",button);
            GUILayout.Label("Click to build · right click to move\nShift + click queues · Cancel button clears orders · Esc: menu\nSelect a tower to upgrade · U upgrades\nEdges / WASD: pan · Shift: faster\nSpace + left drag / middle drag: pan · wheel: zoom\nHome: builder · End: overview\nHold Alt: all health bars",small);
            showTools=GUILayout.Toggle(showTools,"Advanced inspection",button);
            if(showTools) {
                game.ShowRoutes=GUILayout.Toggle(game.ShowRoutes,"Lane and flight route guides",button);
                game.ShowNavigation=GUILayout.Toggle(game.ShowNavigation,"Clearance + low towers [F]",button);
                game.ShowDirections=GUILayout.Toggle(game.ShowDirections,"Enemy intent + siege target",button);
                game.ShowValues=GUILayout.Toggle(game.ShowValues,"Distance at hovered cell",button);
                if(!w.Config.Economy)w.TowersFire=GUILayout.Toggle(w.TowersFire,"Tower weapons enabled",button);
                if(game.HasHover&&game.ShowValues){var f=w.Navigation.Get(w.Config.GroundRoute[0],w.Config.Waves[0].Radius);GUILayout.Label($"Cell {game.Hover.X:0.#}, {game.Hover.Y:0.#} · distance {f.Distance[f.Index(game.Hover+new V2(.5f,.5f))]:0.00}",small);}
                var e=w.Enemies.Find(enemy=>enemy.Id==game.SelectedId);
                GUILayout.Label(e==null?"Ctrl + click an enemy to inspect it.":$"{EnemyIdentity.Name(EnemyIdentity.For(e.Spec))} #{e.Id} · {(e.Spec.Flying?"AIR":"GROUND")} · HP {e.Health:0}\n{(e.Blocked?"SIEGE":"ROUTE OPEN")} · speed {e.Velocity.Length:0.00}",small);
                GUILayout.Label($"Tick {w.Tick} · fields {w.Navigation.Rebuilds} · 30 Hz simulation",small);
                if(!w.Config.Economy&&HudButton("Load zig-zag maze",button))game.DemoMaze();
                if(!game.NetworkMatch&&HudButton("Reset map + waves",button))game.ResetSimulation();
                if(!w.Config.Economy&&HudButton("Play Howl for Maul",button))game.SwitchMap(true);
            }
            GUILayout.EndScrollView();GUILayout.EndArea();GUI.matrix=previousMatrix;DrawHealth();DrawMapLabels();DrawMinimap();DrawBuildFeedback();DrawPlacementHint();if(game.MenuOpen)DrawPauseMenu();
        }
        public static string TowerStatsText(TowerSpec spec)
        {
            if(spec.Damage<=0)return $"Maze piece · {spec.Health:0} HP · no weapon";
            string targets=spec.TargetsGround?(spec.TargetsAir?"Ground + air":"Ground only"):"Air only";
            string text=$"{targets} · {spec.Health:0} HP\n{spec.Damage:0.#} damage every {spec.Interval:0.00}s\n{spec.Damage/spec.Interval:0.#} direct DPS · range {spec.Range:0.0}";
            if(spec.SplashRadius>0)text+=$"\nSplash radius {spec.SplashRadius:0.0}";
            if(spec.SlowFraction>0)text+=$"\nSlow {spec.SlowFraction*100:0}% for {spec.SlowDuration:0.#}s · strongest slow wins";
            if(spec.ChainTargets>0)text+=$"\nChains to {spec.ChainTargets} extra targets within 2 units";
            return text;
        }
        void DrawTowerStats(TowerSpec spec) => GUILayout.Label(TowerStatsText(spec),small);
        void DrawSetup()
        {
            GUILayout.Space(14);
            GUILayout.Label("MATCH SETUP",section);
            GUILayout.Label($"{game.World.LaneCount} upper lanes. One bottom exit. Every lane stays active at every player count.",label);
            if(game.AvailableMaps.Length>1) {
                GUILayout.Label("MAP",section);
                foreach(var map in game.AvailableMaps)if(HudButton((map==game.Map?"✓ ":"")+map.Settings.Name+"\n"+(map.Settings.Theme=="iron"?"Four lanes · defend the Anvilheart":"Three lanes · keep the Hearthward lit"),map==game.Map?selectedCard:card)&&map!=game.Map)game.ChooseMap(map);
                if(game.CanReturnToMatch)GUILayout.Label("Changing maps closes the current match. Other choices apply when you start a new match.",small);
            }
            GUILayout.Space(10);
            GUILayout.Label("OFFLINE PLAY",section);
            game.SetupOptions.PlayerCount=GUILayout.SelectionGrid(game.SetupOptions.PlayerCount-1,new[]{"Solo","2","3","4"},4,button)+1;
            GUILayout.Label("Solo works offline. For friends on separate computers, choose Multiplayer / LAN below.",small);
            GUILayout.Space(10);
            if(game.SetupOptions.PlayerCount==1){GUILayout.Label("Play Solo to choose your faction and difficulty. You start at Last Stand with the full team gold budget; every lane stays active.",label);return;}
            GUILayout.Label("LOCAL SLOT TEST · one keyboard and mouse. Switch control with P1–P4; these slots are not separate players joining your game.",small);
            GUILayout.Label("DIFFICULTY",section);
            game.SetupOptions.Difficulty=(Difficulty)GUILayout.SelectionGrid((int)game.SetupOptions.Difficulty,new[]{"Relaxed","Normal","Hard"},1,button);
            GUILayout.Label("Enemy health and siege damage: 70% / 100% / 140%. Lane counts stay unchanged.",small);
            GUILayout.Space(10);
            int n=game.SetupOptions.PlayerCount,total=game.World.Config.StartingGold;
            GUILayout.Label($"Team starting gold: {total}\nPer player: {total/n}  ·  Team income split evenly",label);
            if(n>1) {
                GUILayout.Label("STARTING POSITIONS",section);
                for(int i=0;i<n;i++) {
                    GUILayout.Label("Player "+(i+1),small);
                    int choice=GUILayout.SelectionGrid(game.SetupOptions.StartingPositions[i],game.World.Config.StartNames,2,button);
                    if(choice!=game.SetupOptions.StartingPositions[i])game.ChooseStart(i,choice);
                }
                GUILayout.Label("Choosing an occupied start swaps the players. You can build anywhere on open terrain.",small);
            } else GUILayout.Label("Solo builder starts at the shared junction with the full team budget.",small);
            if(game.World.Config.Factions.Length>0) {
                GUILayout.Space(12);GUILayout.Label("FACTION / BUILDER",section);
                var factions=game.World.Config.Factions;var names=new string[factions.Length];for(int j=0;j<names.Length;j++)names[j]=factions[j].Name;
                for(int player=0;player<n;player++) {
                    GUILayout.Label("Player "+(player+1),small);
                    game.SetupOptions.Factions[player]=GUILayout.SelectionGrid(game.SetupOptions.Factions[player],names,2,button);
                    GUILayout.Label(factions[game.SetupOptions.Factions[player]].Description,small);
                }
            }
            GUILayout.Space(14);
            GUILayout.Label($"{game.World.Config.Waves.Length} waves · flying attacks every fifth wave. Build during combat; prepare dedicated air defense.",small);
        }
        GUIStyle minimapCaption,minimapNorth;
        void DrawMinimap()
        {
            var w=game.World;if(w.Config.Lanes.Length==0||game.SetupOpen||game.DetailsOpen)return;
            var r=game.MinimapRect;
            if(minimapCaption==null){minimapCaption=new GUIStyle(small){wordWrap=false,padding=new RectOffset()};minimapNorth=new GUIStyle(button){padding=new RectOffset(),wordWrap=false};}
            minimapCaption.fontSize=Mathf.RoundToInt(13*game.UiScale);minimapNorth.fontSize=Mathf.RoundToInt(12*game.UiScale);
            if(Event.current.type==EventType.Repaint){
            GUI.color=Color.white;GUI.Label(new Rect(r.x,r.y-21*game.UiScale,r.width-56*game.UiScale,18*game.UiScale),"Tactical map",minimapCaption);
            GUI.color=Color.white;GUI.DrawTexture(r,renderedMinimap.Texture!=null?renderedMinimap.Texture:minimapTerrain.Get(w.Grid,w.PlacementStep));
            foreach(var tower in w.Grid.Towers)MiniDot(r,tower.Center,new Color(.1f,.95f,.8f),2);
            foreach(var enemy in w.Enemies)MiniDot(r,enemy.Position,enemy.Spec.Flying?new Color(.85f,.4f,1):new Color(1,.48f,.2f),2);
            for(int i=0;i<w.Players.Length;i++)MiniDot(r,w.Players[i].Position,i==w.ActivePlayer?Color.white:Color.cyan,4);
            // North-up map with the actual rotated ground-plane camera footprint.
            var plane=new Plane(Vector3.up,Vector3.zero);var corners=new Vector2[4];bool complete=true;
            var viewport=new[]{new Vector2(0,0),new Vector2(1,0),new Vector2(1,1),new Vector2(0,1)};
            for(int i=0;i<4;i++) {
                var ray=game.View.ViewportPointToRay(viewport[i]);
                if(!plane.Raycast(ray,out float distance)){complete=false;break;}
                var p=ray.GetPoint(distance);corners[i]=new Vector2(r.x+p.x/w.Config.Width*r.width,r.yMax-p.z/w.Config.Height*r.height);
            }
            if(complete)for(int i=0;i<4;i++)DrawMinimapEdge(corners[i],corners[(i+1)%4],r);
            var focus=game.View.GetComponent<RtsCamera>().Focus;
            MiniDot(r,new V2(focus.x,focus.z),Color.yellow,3);
            GUI.color=Color.white;
            }
            if(HudButton(new Rect(r.xMax-50*game.UiScale,r.y-25*game.UiScale,50*game.UiScale,22*game.UiScale),new GUIContent("N  [R]","Face north and center the map horizontally. Keeps your zoom and distance along the map."),minimapNorth))game.ResetView();
            GUI.Label(new Rect(r.x,r.yMax+5*game.UiScale,r.width,18*game.UiScale),"Click or drag to travel",minimapCaption);
        }
        void DrawMinimapEdge(Vector2 a,Vector2 b,Rect rect)
        {
            var d=b-a;float lo=0,hi=1;
            bool Clip(float p,float q){if(Mathf.Abs(p)<.0001f)return q>=0;float t=q/p;if(p<0)lo=Mathf.Max(lo,t);else hi=Mathf.Min(hi,t);return lo<=hi;}
            if(!Clip(-d.x,a.x-rect.xMin)||!Clip(d.x,rect.xMax-a.x)||!Clip(-d.y,a.y-rect.yMin)||!Clip(d.y,rect.yMax-a.y))return;
            b=a+d*hi;a+=d*lo;var old=GUI.matrix;var color=GUI.color;
            GUI.color=new Color(1,.94f,.72f,.95f);GUIUtility.RotateAroundPivot(Mathf.Atan2(b.y-a.y,b.x-a.x)*Mathf.Rad2Deg,a);
            GUI.DrawTexture(new Rect(a.x,a.y,(b-a).magnitude,1),Texture2D.whiteTexture);GUI.matrix=old;GUI.color=color;
        }
        void MiniDot(Rect rect,V2 point,Color color,float size)
        {
            GUI.color=color;GUI.DrawTexture(new Rect(rect.x+point.X/game.World.Config.Width*rect.width-size*.5f,rect.y+(1-point.Y/game.World.Config.Height)*rect.height-size*.5f,size,size),Texture2D.whiteTexture);
        }
        void DrawMapLabels()
        {
            if (!game.World.Config.BuilderEnabled) return;
            for (int i = 0; i < game.World.LaneCount; i++) {
                var spawn=game.World.LaneSpawn(i);
                MapTag(new Vector3(spawn.X,.05f,spawn.Y),"LANE "+(i+1),false);
            }
            var route=game.World.Config.GroundRoute;
            for(int i=0;i<route.Length;i++) {
                bool exit=i==route.Length-1;
                if(exit||game.ShowRoutes||game.SetupOpen)MapTag(new Vector3(route[i].X,.1f,route[i].Y),exit?"REFUGE · EXIT":(i+1).ToString(),exit);
            }
        }
        void MapTag(Vector3 position,string text,bool exit)
        {
            var p=game.View.WorldToScreenPoint(position);
            float width=mapLabel.CalcSize(new GUIContent(text)).x+12;
            var rect=new Rect(p.x-width*.5f,Screen.height-p.y-24,width,18);
            if(p.z<=0||rect.xMin<=game.Sidebar.xMax||rect.xMax>=Screen.width||rect.yMin<0||rect.yMax>=Screen.height||rect.Overlaps(game.MinimapRect)||CoversCompactHud(rect))return;
            GUI.color=new Color(.035f,.065f,.079f,.9f);
            GUI.DrawTexture(rect,Texture2D.whiteTexture);
            GUI.color=exit?new Color(.97f,.83f,.39f):new Color(.78f,.91f,.88f);
            GUI.Label(rect,text,mapLabel);
            GUI.color=Color.white;
        }
        void DrawPlacementHint()
        {
            if(Event.current.type!=EventType.Repaint||!game.HasHover||game.SetupOpen||game.MenuOpen||game.World.Finished||string.IsNullOrEmpty(game.HoverHint))return;
            float scale=game.UiScale;
            var p=game.View.WorldToScreenPoint(new Vector3(game.Hover.X+.5f,0,game.Hover.Y+.5f));
            if(p.z<=0)return;
            var content=new GUIContent(game.HoverHint);
            float width=Mathf.Min(270,Screen.width/scale-game.Sidebar.xMax/scale-24);
            if(width<100)return;
            float height=placementHint.CalcHeight(content,width);
            float x=Mathf.Clamp((p.x+16)/scale,game.Sidebar.xMax/scale+8,Screen.width/scale-width-8);
            float y=Mathf.Clamp((Screen.height-p.y+20)/scale,8,Screen.height/scale-height-8);
            var rect=new Rect(x,y,width,height);
            if(new Rect(x*scale,y*scale,width*scale,height*scale).Overlaps(game.MinimapRect))rect.y=game.MinimapRect.yMin/scale-height-8;
            if(!game.DetailsOpen) {
                float limit=(game.World.Grid.Find(game.SelectedTowerId)!=null&&rect.Overlaps(Logical(game.SelectionHud))?game.SelectionHud.yMin:game.DockHud.yMin)/scale;
                if(rect.Overlaps(Logical(game.BuildHud))||rect.Overlaps(Logical(game.MinimapPanel))||(game.World.Grid.Find(game.SelectedTowerId)!=null&&rect.Overlaps(Logical(game.SelectionHud))))rect.y=Mathf.Min(rect.y,limit-height-8);
                rect.y=Mathf.Max(rect.y,game.TopHud.yMax/scale+8);
            }
            var previous=GUI.matrix;GUI.matrix=Matrix4x4.Scale(new Vector3(scale,scale,1));
            GUI.Label(rect,content,placementHint);GUI.matrix=previous;
        }
        void OnDestroy()
        {
            renderedMinimap.Dispose();
            portraits.Dispose();
            minimapTerrain.Dispose();
            foreach(var texture in textures)if(texture!=null)Destroy(texture);
            if(hearthSkin!=null)Destroy(hearthSkin);
            if(uiClick!=null)Destroy(uiClick);
        }
    }
}
