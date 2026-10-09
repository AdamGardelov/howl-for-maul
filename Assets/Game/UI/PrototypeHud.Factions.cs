using UnityEngine;
using FrostMaze.Simulation;
using FrostMaze.Simulation.Online;
namespace FrostMaze
{
    public sealed partial class PrototypeHud
    {
        Session previewSession;
        int previewFaction,previewTower=-1;
        GUIStyle factionName,factionNote,factionTile,factionChosen,factionPreview,rosterPrice;
        void PreviewFaction(Session session,int faction)
        {
            if(previewSession==session&&previewFaction==faction)return;
            previewSession=session;previewFaction=faction;
            previewTower=game.World.Config.Factions[faction].Designs[0];
        }
        void EnsureFactionPreview(Session session)
        {
            if(previewSession==session)return;
            int selected=0;foreach(var m in session.Members)if(m.Id==session.LocalId&&m.Faction>=0)selected=m.Faction;
            PreviewFaction(session,selected);
        }
        bool WarmFactionPortraits()
        {
            var net=game.Net;if(!game.LobbyOpen||net==null||net.Stage!=Stage.Factions)return false;
            EnsureFactionPreview(net);var config=game.World.Config;
            // A portrait of every faction first; then the roster being inspected. One render per frame.
            foreach(var faction in config.Factions)if(portraits.Get(faction.Designs[0])==null){portraits.Prepare(game,faction.Designs[0]);return true;}
            foreach(int design in config.Factions[previewFaction].Designs)if(portraits.Get(design)==null){portraits.Prepare(game,design);return true;}
            return true;
        }
        public static string FactionTowerPreview(Scenario config,int design)
        {
            var tower=config.Catalog[design];string text=TowerStatsText(tower.Spec);
            if(tower.Requires.Length>0){text+="\n\nRequires your towers: ";for(int i=0;i<tower.Requires.Length;i++)text+=(i==0?"":", ")+config.Catalog[tower.Requires[i]].Name;}
            if(tower.WoodCost>0)text+="\nWood is awarded after wave 14.";
            return text;
        }
        void FactionStyles()
        {
            if(factionName!=null)return;
            factionName=new GUIStyle(label){fontSize=14,fontStyle=FontStyle.Bold,padding=new RectOffset(),alignment=TextAnchor.UpperLeft};
            factionNote=new GUIStyle(small){padding=new RectOffset(),alignment=TextAnchor.UpperLeft};
            factionTile=new GUIStyle(card){fixedHeight=0,fixedWidth=0,padding=new RectOffset(),margin=new RectOffset()};
            factionChosen=new GUIStyle(factionTile);factionChosen.normal.background=selectedCard.normal.background;
            factionPreview=new GUIStyle(factionTile);factionPreview.normal.background=button.hover.background;
            rosterPrice=new GUIStyle(section){alignment=TextAnchor.MiddleCenter,padding=new RectOffset(),wordWrap=false};
        }
        void Portrait(Rect rect,int design)
        {
            var image=portraits.Get(design);
            if(image!=null)GUI.DrawTexture(rect,image,ScaleMode.ScaleToFit);
            else GUI.Label(rect,"…",rosterPrice);
        }
        void DrawFactionBrowser(Session net,Member me,float width)
        {
            EnsureFactionPreview(net);FactionStyles();var config=game.World.Config;
            GUILayout.Label("Hover to explore · click a faction to choose · confirm when ready",small);
            bool stacked=width<800;float leftWidth=stacked?width:width*.48f,rightWidth=stacked?width:width-leftWidth-18;
            int rows=(config.Factions.Length+1)/2;float leftHeight=rows*116;
            float previewHeight=588;
            var area=GUILayoutUtility.GetRect(width,stacked?leftHeight+previewHeight+18:Mathf.Max(leftHeight,previewHeight),GUILayout.Width(width));
            for(int i=0;i<config.Factions.Length;i++){
                var faction=config.Factions[i];float tileWidth=(leftWidth-8)/2;
                var tile=new Rect(area.x+(i%2)*(tileWidth+8),area.y+(i/2)*116,tileWidth,108);
                GUI.SetNextControlName("Faction "+i);
                if(HudButton(tile,GUIContent.none,me.Faction==i?factionChosen:previewFaction==i?factionPreview:factionTile)){
                    PreviewFaction(net,i);net.Send(new Packet{Kind=Kind.Faction,A=i});
                }
                if(tile.Contains(Event.current.mousePosition))PreviewFaction(net,i);
                Portrait(new Rect(tile.x+5,tile.y+18,64,72),faction.Designs[0]);
                GUI.Label(new Rect(tile.x+76,tile.y+9,tile.width-82,34),faction.Name,factionName);
                string theme=faction.Description.Split('.')[0];
                GUI.Label(new Rect(tile.x+76,tile.y+44,tile.width-82,40),theme,factionNote);
                GUI.Label(new Rect(tile.x+76,tile.y+87,tile.width-82,18),me.Faction==i?"✓ CHOSEN":config.Catalog[faction.Designs[0]].Cost+"g opening tower",section);
            }
            var pane=new Rect(stacked?area.x:area.x+leftWidth+18,stacked?area.y+leftHeight+18:area.y,rightWidth,previewHeight);
            GUI.Box(pane,GUIContent.none,factionTile);
            var selected=config.Factions[previewFaction];float x=pane.x+14,w=pane.width-28;
            GUI.Label(new Rect(x,pane.y+12,w,28),selected.Name,title);
            GUI.Label(new Rect(x,pane.y+47,w,72),selected.Description,label);
            GUI.Label(new Rect(x,pane.y+123,w,18),"TOWER ROSTER · hover or click to inspect",small);
            for(int i=0;i<selected.Designs.Length;i++){
                int design=selected.Designs[i];var tower=config.Catalog[design];float tileWidth=(w-18)/4;
                var tile=new Rect(x+(i%4)*(tileWidth+6),pane.y+148+(i/4)*88,tileWidth,82);
                if(HudButton(tile,GUIContent.none,previewTower==design?factionChosen:factionTile)||tile.Contains(Event.current.mousePosition))previewTower=design;
                Portrait(new Rect(tile.x+3,tile.y+3,tile.width-6,58),design);
                GUI.Label(new Rect(tile.x,tile.y+61,tile.width,18),tower.Cost+"g"+(tower.WoodCost>0?" + "+tower.WoodCost+"w":""),rosterPrice);
            }
            if(System.Array.IndexOf(selected.Designs,previewTower)<0)previewTower=selected.Designs[0];
            var shown=config.Catalog[previewTower];
            GUI.Label(new Rect(x,pane.y+330,w,24),shown.Name+" · "+shown.Cost+" gold"+(shown.WoodCost>0?" + "+shown.WoodCost+" wood":""),factionName);
            GUI.Label(new Rect(x,pane.y+361,w,168),FactionTowerPreview(config,previewTower),label);
            if(me.Faction==previewFaction)GUI.Label(new Rect(x,pane.y+543,w,30),"CHOSEN · confirm below when ready",rosterPrice);
            else if(HudButton(new Rect(x,pane.y+541,w,32),"CHOOSE "+selected.Name.ToUpperInvariant(),primary))net.Send(new Packet{Kind=Kind.Faction,A=previewFaction});
        }
    }
}
