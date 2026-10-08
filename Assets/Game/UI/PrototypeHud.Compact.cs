using UnityEngine;
using FrostMaze.Simulation;
namespace FrostMaze
{
    public sealed partial class PrototypeHud
    {
        bool CoversCompactHud(Rect r) => !game.SetupOpen&&!game.DetailsOpen&&(r.Overlaps(game.TopHud)||(r.Overlaps(game.BuildHud)||r.Overlaps(game.MinimapPanel)||(game.World.Grid.Find(game.SelectedTowerId)!=null&&r.Overlaps(game.SelectionHud)))||r.Overlaps(game.AlertHud));
        Rect Logical(Rect r) => new Rect(r.x/game.UiScale,r.y/game.UiScale,r.width/game.UiScale,r.height/game.UiScale);
        void DrawCompact()
        {
            var w=game.World;var matrix=GUI.matrix;float scale=game.UiScale;
            GUI.matrix=Matrix4x4.Scale(new Vector3(scale,scale,1));
            var top=Logical(game.TopHud);Frame(top);
            GUILayout.BeginArea(new Rect(top.x+10,top.y+8,top.width-20,top.height-12));GUILayout.BeginHorizontal();
            GUILayout.Label("HOWL FOR MAUL",section,GUILayout.Width(138));
            if(GUILayout.Button("MENU [ESC]",button,GUILayout.Width(104)))game.ToggleMenu();
            if(GUILayout.Button("DETAILS [TAB]",button,GUILayout.Width(108)))game.DetailsOpen=true;
            if(GUILayout.Button(game.Paused?"RESUME [P]":"PAUSE [P]",button,GUILayout.Width(100)))game.Paused=!game.Paused;
            GUI.enabled=!w.Finished&&!w.WaveActive;
            if(GUILayout.Button(w.Finished?(w.Won?"VICTORY":"DEFEAT"):w.WaveActive?"WAVE ACTIVE":"NEXT WAVE [ENTER]",button,GUILayout.Width(148)))game.Launch();
            GUI.enabled=true;GUILayout.FlexibleSpace();
            if(w.Players.Length>1&&GUILayout.Button($"P{w.ActivePlayer+1}",button,GUILayout.Width(40)))w.SelectPlayer((w.ActivePlayer+1)%w.Players.Length);
            GUILayout.Label($"{w.Gold} GOLD    ·    {w.Lives} LIVES    ·    WAVE {Mathf.Max(0,w.WaveIndex+1)}/{w.Config.Waves.Length}",section,GUILayout.Width(310));
            GUILayout.EndHorizontal();GUILayout.EndArea();
            Frame(Logical(game.MinimapPanel));
            DrawCommandDetails();DrawBuildGrid();
            var alert=new Rect(20,68,330,48);
            if(feedback!=null&&feedback.RecentLeaks>0){if(GUI.Button(alert,$"EXIT BREACHED · {feedback.RecentLeaks} leaked · View exit",alertButton))game.FocusExit();}
            else if(w.Finished)GUI.Label(alert,w.Won?"VICTORY — all waves cleared":"DEFEAT — open Menu for a new game",section);
            else if(!w.WaveActive&&w.LastWaveSummary!=null)GUI.Label(alert,$"Wave {w.LastWaveSummary.WaveNumber}: {w.LastWaveSummary.Killed} defeated · {w.LastWaveSummary.Leaked} leaked\nIncome: {w.LastWaveSummary.GoldForPlayer(w.ActivePlayer)}g",small);
            GUI.matrix=matrix;DrawHealth();DrawMapLabels();DrawMinimap();DrawPlacementHint();
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
            if(GUI.Button(new Rect(box.x+12,box.y+56,width,28),tower.Level>=3?"MAX LEVEL":$"UPGRADE [U] · {price}g",button)){w.Upgrade(tower.Id,out string message);game.Notice=message;}
            GUI.enabled=!w.Finished&&(own||owner<0);
            if(GUI.Button(new Rect(box.x+18+width,box.y+56,width,28),$"REMOVE · +{w.SaleRefund(tower.Id)}g",button))w.Sell(tower.CellX,tower.CellY);
            GUI.enabled=true;if(GUI.Button(new Rect(box.xMax-30,box.y+7,22,22),"×",button))game.SelectedTowerId=0;
        }
        void DrawPauseMenu()
        {
            var matrix=GUI.matrix;float scale=game.UiScale;
            GUI.matrix=Matrix4x4.Scale(new Vector3(scale,scale,1));
            float width=Screen.width/scale,height=Screen.height/scale;
            var old=GUI.color;GUI.color=new Color(0,0,0,.65f);GUI.DrawTexture(new Rect(0,0,width,height),Texture2D.whiteTexture);GUI.color=old;
            var box=new Rect((width-380)/2,(height-470)/2,380,470);Frame(box);
            GUILayout.BeginArea(new Rect(box.x+24,box.y+18,332,434));
            GUILayout.Label("HOWL FOR MAUL",title);GUILayout.Label("GAME MENU · match paused",section);GUILayout.Space(12);
            if(GUILayout.Button("RETURN TO GAME [ESC]",primary))game.ToggleMenu();
            if(GUILayout.Button("NEW GAME",button))game.OpenSetup();
            GUILayout.Space(12);GUILayout.Label("SETTINGS",section);
            game.SoundEnabled=GUILayout.Toggle(game.SoundEnabled,"Combat sound",button);
            game.ShowGrid=GUILayout.Toggle(game.ShowGrid,"Placement grid",button);
            if(GUILayout.Button(game.Speed==1?"Game speed: 1×":"Game speed: 2×",button))game.Speed=game.Speed==1?2:1;
            if(!Application.isEditor)Screen.fullScreen=GUILayout.Toggle(Screen.fullScreen,"Fullscreen window",button);
            GUILayout.Space(8);GUILayout.Label("Details [Tab]: wave advice, tower stats and inspection tools.\nEdges / WASD: pan · Space + drag: pan · wheel: zoom\nQ / E: rotate · Home: default view at builder",small);
            GUILayout.EndArea();GUI.matrix=matrix;
        }
    }
}
