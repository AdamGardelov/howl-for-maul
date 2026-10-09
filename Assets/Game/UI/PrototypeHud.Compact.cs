using UnityEngine;
using FrostMaze.Simulation;
namespace FrostMaze
{
    public sealed partial class PrototypeHud
    {
        bool CoversCompactHud(Rect r) => !game.SetupOpen&&!game.DetailsOpen&&(r.Overlaps(game.TopHud)||r.Overlaps(game.ForecastHud)||(r.Overlaps(game.BuildHud)||r.Overlaps(game.MinimapPanel)||(r.Overlaps(game.SelectionHud)&&HasSelectedTower()))||r.Overlaps(game.AlertHud));
        bool HasSelectedTower()
        {
            if(game.SelectedTowerId==0)return false;
            foreach(var tower in game.World.Grid.Towers)if(tower.Id==game.SelectedTowerId)return true;
            return false;
        }
        Rect Logical(Rect r) => new Rect(r.x/game.UiScale,r.y/game.UiScale,r.width/game.UiScale,r.height/game.UiScale);
        void DrawCompact()
        {
            var w=game.World;var matrix=GUI.matrix;float scale=game.UiScale;
            GUI.matrix=Matrix4x4.Scale(new Vector3(scale,scale,1));
            var top=Logical(game.TopHud);Frame(top);bool narrow=top.width<1260;
            GUILayout.BeginArea(new Rect(top.x+10,top.y+8,top.width-20,top.height-12));GUILayout.BeginHorizontal();
            GUILayout.Label(narrow?"HOWL":"HOWL FOR MAUL",section,GUILayout.Width(narrow?66:138));
            if(GUILayout.Button("MENU [ESC]",button,GUILayout.Width(narrow?88:104)))game.ToggleMenu();
            if(GUILayout.Button("DETAILS [TAB]",button,GUILayout.Width(narrow?94:108)))game.DetailsOpen=true;
            if(GUILayout.Button(game.Paused?"RESUME [P]":"PAUSE [P]",button,GUILayout.Width(100)))game.VotePause();
            GUI.enabled=!w.Finished&&!w.WaveActive;
            if(GUILayout.Button(w.Finished?(w.Won?"VICTORY":"DEFEAT"):w.WaveActive?"WAVE ACTIVE":"NEXT WAVE [ENTER]",button,GUILayout.Width(narrow?134:148)))game.Launch();
            GUI.enabled=true;DrawSpeedControls();GUILayout.FlexibleSpace();
            if(!game.NetworkMatch&&w.Players.Length>1&&GUILayout.Button($"P{w.ActivePlayer+1}",button,GUILayout.Width(40)))w.SelectPlayer((w.ActivePlayer+1)%w.Players.Length);
            DrawResourceCounters(w,narrow);
            GUILayout.EndHorizontal();GUILayout.EndArea();
            DrawForecast();
            Frame(Logical(game.MinimapPanel));
            DrawCommandDetails();DrawBuildGrid();
            var alert=new Rect(20,68,330,48);
            if(feedback!=null&&feedback.RecentLeaks>0){if(GUI.Button(alert,$"EXIT BREACHED · {feedback.RecentLeaks} leaked · View exit",alertButton))game.FocusExit();}
            else if(w.Finished)GUI.Label(alert,w.Won?"VICTORY — all waves cleared":"DEFEAT — open Menu for a new game",section);
            else if(!w.WaveActive&&w.LastWaveSummary!=null)GUI.Label(alert,$"Wave {w.LastWaveSummary.WaveNumber}: {w.LastWaveSummary.Killed} defeated · {w.LastWaveSummary.Leaked} leaked\nIncome: {w.LastWaveSummary.GoldForPlayer(w.ActivePlayer)}g · {w.LastWaveSummary.WoodForPlayer(w.ActivePlayer)} wood",small);
            GUI.matrix=matrix;DrawHealth();DrawMapLabels();DrawMinimap();DrawBuildFeedback();DrawPlacementHint();
        }
        void DrawForecast()
        {
            if(game.ForecastHud==Rect.zero)return;
            var w=game.World;int index=w.WaveIndex+1;var wave=w.PreviewWave(index);
            var box=Logical(game.ForecastHud);Frame(box);
            string kind=wave.Flying?"AIR":"GROUND";
            GUI.Label(new Rect(box.x+12,box.y+8,box.width-24,18),$"{(index==w.Config.Waves.Length-1?"FINAL":"NEXT")} {index+1}/{w.Config.Waves.Length} · {kind}",section);
            GUI.Label(new Rect(box.x+12,box.y+27,box.width-24,18),$"{wave.Count*w.LaneCount} enemies · {wave.Health:0.#} HP · {w.LaneCount} lanes",label);
            int defenses=w.DefensesFor(wave);
            GUI.Label(new Rect(box.x+12,box.y+47,box.width-24,20),defenses==0?$"No team weapons hit {kind.ToLowerInvariant()} · Details [TAB]":$"{defenses} team {kind.ToLowerInvariant()} weapons · Check coverage [TAB]",defenses==0?section:small);
            if(GUI.Button(box,GUIContent.none,GUIStyle.none)){game.DetailsOpen=true;showForecast=true;}
        }
        void DrawCommandDetails()
        {
            var w=game.World;var tower=w.Grid.Find(game.SelectedTowerId);if(tower==null)return;
            var box=Logical(game.SelectionHud);Frame(box);
            int owner=w.TowerOwner(tower.Id);bool own=owner==w.ActivePlayer;int price=w.UpgradeCost(tower);
            GUI.Label(new Rect(box.x+12,box.y+9,box.width-44,20),$"{tower.Name} · LEVEL {tower.Level} · P{owner+1}",section);
            GUI.Label(new Rect(box.x+12,box.y+31,box.width-24,18),$"{tower.Health:0}/{tower.Spec.Health:0} HP · Damage {tower.Spec.Damage:0.#} · Range {tower.Spec.Range:0.0}",small);
            float width=(box.width-30)*.5f;
            GUI.enabled=own&&!w.Finished&&tower.Level<3&&(!w.Config.Economy||w.Gold>=price);
            if(GUI.Button(new Rect(box.x+12,box.y+56,width,28),tower.Level>=3?"MAX LEVEL":$"UPGRADE [U] · {price}g",button))game.UpgradeTower(tower.Id);
            GUI.enabled=!w.Finished&&(own||owner<0);
            if(GUI.Button(new Rect(box.x+18+width,box.y+56,width,28),$"REMOVE · +{w.SaleRefund(tower.Id)}g",button))game.SellTower(tower.CellX,tower.CellY);
            GUI.enabled=true;if(GUI.Button(new Rect(box.xMax-30,box.y+7,22,22),"×",button))game.SelectedTowerId=0;
        }
        GUIStyle speedChoice,speedActive,speedCaption;
        void DrawSpeedControls()
        {
            if(speedChoice==null){
                speedChoice=new GUIStyle(button){fixedHeight=0,margin=new RectOffset(),padding=new RectOffset(),alignment=TextAnchor.MiddleCenter,fontSize=12};
                speedActive=new GUIStyle(speedChoice){fontStyle=FontStyle.Bold};speedActive.normal.background=selectedCard.normal.background;speedActive.normal.textColor=new Color(1,.9f,.62f);
                speedCaption=new GUIStyle(small){alignment=TextAnchor.MiddleCenter,wordWrap=false,padding=new RectOffset()};
            }
            var box=GUILayoutUtility.GetRect(204,30,GUILayout.Width(204),GUILayout.Height(30));
            GUI.Box(box,GUIContent.none,card);
            bool guest=game.NetworkMatch&&!game.Net.IsHost;
            GUI.Label(new Rect(box.x+2,box.y+1,42,28),guest?"HOST":"SPEED",speedCaption);
            bool enabled=GUI.enabled;
            for(int i=0;i<FrostMaze.Simulation.Online.MatchSpeeds.Count;i++){
                var tile=new Rect(box.x+45+i*39,box.y+3,38,24);bool selected=i==game.SpeedIndex;
                string text=FrostMaze.Simulation.Online.MatchSpeeds.Label(i);
                GUI.enabled=enabled&&game.CanChangeSpeed;
                if(guest){GUI.enabled=enabled;GUI.Box(tile,new GUIContent(text,"The host controls the shared match speed."),selected?speedActive:speedChoice);}
                else if(GUI.Button(tile,new GUIContent(text,"Set match speed to "+text+". Keyboard: − / +."),selected?speedActive:speedChoice))game.SetSpeedIndex(i);
                GUI.enabled=enabled;
                if(selected){var color=GUI.color;GUI.color=new Color(.86f,.71f,.38f);GUI.DrawTexture(new Rect(tile.x+6,tile.yMax-3,tile.width-12,2),Texture2D.whiteTexture);GUI.color=color;}
            }
            GUI.enabled=enabled;
        }
        void DrawPauseMenu()
        {
            var matrix=GUI.matrix;float scale=game.UiScale;
            GUI.matrix=Matrix4x4.Scale(new Vector3(scale,scale,1));
            float width=Screen.width/scale,height=Screen.height/scale;
            var old=GUI.color;GUI.color=new Color(0,0,0,.65f);GUI.DrawTexture(new Rect(0,0,width,height),Texture2D.whiteTexture);GUI.color=old;
            var box=new Rect((width-380)/2,(height-680)/2,380,680);Frame(box);
            GUILayout.BeginArea(new Rect(box.x+24,box.y+18,332,644));
            GUILayout.Label("HOWL FOR MAUL",title);GUILayout.Label(game.NetworkMatch&&!OnlineGame.Current.LocalOnly?"GAME MENU · [P] votes to pause":"GAME MENU · match paused",section);GUILayout.Space(12);
            if(GUILayout.Button("RETURN TO GAME [ESC]",primary))game.ToggleMenu();
            if(game.NetworkMatch&&GUILayout.Button(game.Paused?"VOTE TO RESUME":"VOTE TO PAUSE",button))game.VotePause();
            if(GUILayout.Button(game.NetworkMatch?"LEAVE MATCH":"NEW GAME",button))game.OpenSetup();
            if(GUILayout.Button("QUIT GAME",button))game.QuitGame();
            GUILayout.Space(12);GUILayout.Label("SETTINGS",section);
            game.SoundEnabled=GUILayout.Toggle(game.SoundEnabled,"Combat sound",button);
            GUILayout.Label("Effects volume",small);game.EffectsVolume=GUILayout.HorizontalSlider(game.EffectsVolume,0,1);
            GUILayout.Label("Music volume",small);game.MusicVolume=GUILayout.HorizontalSlider(game.MusicVolume,0,1);
            if(GUILayout.Button("RESET CAMERA ANGLE [R]",button))game.ResetView();
            GUILayout.Label("\""+(game.World.Config.Theme=="iron"?"Signal to Noise":"Snowfall")+"\" by Scott Buckley\nCC BY 4.0 · scottbuckley.com.au",small);
            if(GUILayout.Button("MUSIC & LICENSE CREDITS",button))Application.OpenURL("https://www.scottbuckley.com.au/library/"+(game.World.Config.Theme=="iron"?"signal-to-noise/":"snowfall/"));
            game.ShowGrid=GUILayout.Toggle(game.ShowGrid,"Placement grid",button);
            GUILayout.BeginHorizontal();DrawSpeedControls();GUILayout.EndHorizontal();
            if(game.NetworkMatch&&!game.Net.IsHost)GUILayout.Label("The host controls the shared game speed.",small);
            if(!Application.isEditor)Screen.fullScreen=GUILayout.Toggle(Screen.fullScreen,"Fullscreen window",button);
            GUILayout.Space(8);GUILayout.Label((game.NetworkMatch&&!game.Net.IsHost?"Speed: shared by everyone; controlled by the host.":"Speed [− / +]: 0.5× / 1× / 2× / 3×.")+"\nDetails [Tab]: wave advice, tower stats and inspection tools.\nEdges / WASD: pan · Space + drag: pan · wheel: zoom\nQ / E: rotate · R: reset angle · Home: builder\nHold Alt: reveal all health bars",small);
            GUILayout.EndArea();GUI.matrix=matrix;
        }
    }
}
