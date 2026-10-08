using UnityEngine;
using FrostMaze.Simulation;
namespace FrostMaze
{
    public sealed partial class PrototypeHud
    {
        bool CoversCompactHud(Rect r) => !game.SetupOpen&&!game.DetailsOpen&&(r.Overlaps(game.TopHud)||r.Overlaps(game.BuildHud)||r.Overlaps(game.AlertHud)||(game.World.Grid.Find(game.SelectedTowerId)!=null&&r.Overlaps(game.SelectionHud)));
        Rect Logical(Rect r) => new Rect(r.x/game.UiScale,r.y/game.UiScale,r.width/game.UiScale,r.height/game.UiScale);
        void DrawCompact()
        {
            var w=game.World;var matrix=GUI.matrix;float scale=game.UiScale;
            GUI.matrix=Matrix4x4.Scale(new Vector3(scale,scale,1));
            var top=Logical(game.TopHud);GUI.DrawTexture(top,panel);
            GUILayout.BeginArea(new Rect(top.x+8,top.y+4,top.width-16,top.height-8));GUILayout.BeginHorizontal();
            GUILayout.Label($"{w.Gold} GOLD    ·    {w.Lives} LIVES    ·    WAVE {Mathf.Max(0,w.WaveIndex+1)}/{w.Config.Waves.Length}",section,GUILayout.ExpandWidth(true));
            if(w.Players.Length>1&&GUILayout.Button($"P{w.ActivePlayer+1}",button,GUILayout.Width(48)))w.SelectPlayer((w.ActivePlayer+1)%w.Players.Length);
            GUI.enabled=!w.Finished&&!w.WaveActive;
            if(GUILayout.Button(w.Finished?(w.Won?"VICTORY":"DEFEAT"):w.WaveActive?"WAVE ACTIVE":"NEXT WAVE [ENTER]",button,GUILayout.Width(156)))game.Launch();
            GUI.enabled=true;
            if(GUILayout.Button(game.Paused?"RESUME [P]":"PAUSE [P]",button,GUILayout.Width(100)))game.Paused=!game.Paused;
            if(GUILayout.Button("DETAILS [TAB]",button,GUILayout.Width(110)))game.DetailsOpen=true;
            if(GUILayout.Button("MENU [ESC]",button,GUILayout.Width(104)))game.ToggleMenu();
            GUILayout.EndHorizontal();GUILayout.EndArea();
            var build=Logical(game.BuildHud);GUI.DrawTexture(build,panel);
            GUILayout.BeginArea(new Rect(build.x+8,build.y+4,build.width-16,build.height-8));
            GUILayout.BeginHorizontal();GUILayout.Label(w.FactionName.ToUpperInvariant(),section,GUILayout.ExpandWidth(true));
            if(GUILayout.Button(game.SellMode?"SELLING [X]":game.MoveMode?"MOVING [M]":"BUILDING [B]",button,GUILayout.Width(132))){game.SellMode=false;game.MoveMode=false;}
            if(GUILayout.Button("CANCEL ORDERS",button,GUILayout.Width(126)))game.CancelInteraction();GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();int shortcut=0;
            for(int i=0;i<w.Config.Catalog.Length;i++) {
                if(!w.DesignAvailable(i))continue;shortcut++;var d=w.Config.Catalog[i];
                string note=!w.RequirementsMet(i)?"LOCKED":w.Config.Economy&&w.Gold<d.Cost?"NEED GOLD":d.Cost+"g";
                GUI.enabled=!w.Finished;
                if(GUILayout.Button($"<b>{shortcut}  {d.Name}</b>\n<size=10>{note}</size>",w.SelectedDesign==i?selectedCard:card,GUILayout.ExpandWidth(true))){w.SelectedDesign=i;game.SellMode=false;game.MoveMode=false;game.SelectedTowerId=0;}
            }
            GUI.enabled=true;GUILayout.EndHorizontal();GUILayout.EndArea();
            var selected=w.Grid.Find(game.SelectedTowerId);
            if(selected!=null) {
                var box=Logical(game.SelectionHud);GUI.DrawTexture(box,panel);
                GUILayout.BeginArea(new Rect(box.x+10,box.y+5,box.width-20,box.height-10));
                int owner=w.TowerOwner(selected.Id);bool own=owner==w.ActivePlayer;
                GUILayout.Label($"{selected.Name} · LEVEL {selected.Level} · {(owner<0?"UNCLAIMED":$"P{owner+1}")} · {selected.Health:0}/{selected.Spec.Health:0} HP",section);
                GUILayout.Label($"{Role(selected.Spec)} · damage {selected.Spec.Damage:0.#} · range {selected.Spec.Range:0.0}",small);
                GUILayout.BeginHorizontal();int price=w.UpgradeCost(selected);
                GUI.enabled=own&&!w.Finished&&selected.Level<3&&(!w.Config.Economy||w.Gold>=price);
                if(GUILayout.Button(selected.Level>=3?"MAX LEVEL":$"UPGRADE [U] · {price}g",button)){w.Upgrade(selected.Id,out string message);game.Notice=message;}
                GUI.enabled=!w.Finished&&(own||owner<0);
                if(GUILayout.Button($"SELL · {w.SaleRefund(selected.Id)}g",button))w.Sell(selected.CellX,selected.CellY);
                GUI.enabled=true;if(GUILayout.Button("CLOSE",button))game.SelectedTowerId=0;
                GUILayout.EndHorizontal();GUILayout.EndArea();
            }
            // Compact alerts retain the leak warning and income feedback without a permanent panel.
            var alert=new Rect(20,68,330,48);
            if(feedback!=null&&feedback.RecentLeaks>0){if(GUI.Button(alert,$"EXIT BREACHED · {feedback.RecentLeaks} leaked · View exit",alertButton))game.FocusExit();}
            else if(w.Finished)GUI.Label(alert,w.Won?"VICTORY — all waves cleared":"DEFEAT — open Menu for a new game",section);
            else if(!w.WaveActive&&w.LastWaveSummary!=null)GUI.Label(alert,$"Wave {w.LastWaveSummary.WaveNumber}: {w.LastWaveSummary.Killed} defeated · {w.LastWaveSummary.Leaked} leaked\nIncome: {w.LastWaveSummary.GoldForPlayer(w.ActivePlayer)}g",small);
            GUI.matrix=matrix;DrawHealth();DrawMapLabels();DrawMinimap();DrawPlacementHint();
        }
        void DrawPauseMenu()
        {
            var matrix=GUI.matrix;float scale=game.UiScale;
            GUI.matrix=Matrix4x4.Scale(new Vector3(scale,scale,1));
            float width=Screen.width/scale,height=Screen.height/scale;
            var old=GUI.color;GUI.color=new Color(0,0,0,.65f);GUI.DrawTexture(new Rect(0,0,width,height),Texture2D.whiteTexture);GUI.color=old;
            var box=new Rect((width-380)/2,(height-470)/2,380,470);GUI.DrawTexture(box,panel);
            GUILayout.BeginArea(new Rect(box.x+24,box.y+18,332,434));
            GUILayout.Label("HOWL FOR MAUL",title);GUILayout.Label("GAME MENU · match paused",section);GUILayout.Space(12);
            if(GUILayout.Button("RETURN TO GAME [ESC]",primary))game.ToggleMenu();
            if(GUILayout.Button("NEW GAME",button))game.OpenSetup();
            GUILayout.Space(12);GUILayout.Label("SETTINGS",section);
            game.SoundEnabled=GUILayout.Toggle(game.SoundEnabled,"Combat sound",button);
            game.ShowGrid=GUILayout.Toggle(game.ShowGrid,"Placement grid",button);
            if(GUILayout.Button(game.Speed==1?"Game speed: 1×":"Game speed: 2×",button))game.Speed=game.Speed==1?2:1;
            if(!Application.isEditor)Screen.fullScreen=GUILayout.Toggle(Screen.fullScreen,"Fullscreen window",button);
            GUILayout.Space(8);GUILayout.Label("Details [Tab]: wave advice, tower stats and inspection tools.\nEdges / WASD: pan · Space + drag: pan · wheel: zoom",small);
            GUILayout.EndArea();GUI.matrix=matrix;
        }
    }
}
