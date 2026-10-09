using UnityEngine;
namespace FrostMaze
{
    public sealed partial class PrototypeHud
    {
        readonly TowerPortraits portraits=new TowerPortraits();
        readonly RenderedMinimap renderedMinimap=new RenderedMinimap();
        GUIStyle portraitKey,portraitPrice,portraitTile,selectedPortraitTile;
        void Update()
        {
            if(game==null||game.World==null)return;
            renderedMinimap.Prepare(game);
            // Warm only the current roster, at most one portrait per frame; never render in OnGUI.
            for(int i=0;i<game.World.Config.Catalog.Length;i++)
                if(game.World.DesignAvailable(i)&&portraits.Get(i)==null){portraits.Prepare(game,i);break;}
        }
        public static string BuildTooltip(FrostMaze.Simulation.World world,int design)
        {
            var tower=world.Config.Catalog[design];
            var text=new System.Text.StringBuilder();
            text.Append(tower.Name).Append(" · ").Append(tower.Cost).Append("g\n").Append(Role(tower.Spec));
            text.Append("\n").Append(TowerStatsText(tower.Spec));
            if(!string.IsNullOrEmpty(tower.Description))text.Append("\n").Append(tower.Description);
            bool missing=false;
            foreach(int required in world.MissingPrerequisites(design)) {
                if(!missing)text.Append("\nMissing owned towers:");
                text.Append("\n• ").Append(world.Config.Catalog[required].Name);missing=true;
            }
            if(world.Finished)text.Append("\nMatch finished.");
            else if(world.Config.Economy&&world.Gold<tower.Cost)text.Append("\nNeed ").Append(tower.Cost-world.Gold).Append("g more.");
            else if(!missing)text.Append("\nClick to select · Shift + click map to queue");
            return text.ToString();
        }
        void DrawBuildGrid()
        {
            var w=game.World;var box=Logical(game.BuildHud);Frame(box);
            if(portraitKey==null) {
                portraitTile=new GUIStyle(card){fixedHeight=0,fixedWidth=0,padding=new RectOffset()};
                selectedPortraitTile=new GUIStyle(selectedCard){fixedHeight=0,fixedWidth=0,padding=new RectOffset()};
                portraitKey=new GUIStyle(section){alignment=TextAnchor.UpperLeft,padding=new RectOffset(4,0,1,0)};
                portraitPrice=new GUIStyle(small){alignment=TextAnchor.MiddleCenter};
                portraitPrice.normal.textColor=new Color(.94f,.83f,.55f);
            }
            GUI.Label(new Rect(box.x+8,box.y+5,box.width-16,18),w.FactionName.ToUpperInvariant(),section);
            GUI.Label(new Rect(box.x+8,box.y+24,box.width-88,18),$"{w.QueuedBuilds} queued · Shift + click",small);
            if(GUI.Button(new Rect(box.xMax-74,box.y+22,66,20),"CANCEL",button))game.CancelInteraction();
            int slot=0,hovered=-1;
            for(int i=0;i<w.Config.Catalog.Length;i++) {
                if(!w.DesignAvailable(i))continue;
                var d=w.Config.Catalog[i];int row=slot/game.BuildColumns,col=slot%game.BuildColumns;
                var tile=new Rect(box.x+8+col*78,box.y+48+row*80,72,76);
                bool locked=!w.RequirementsMet(i),poor=w.Config.Economy&&w.Gold<d.Cost;
                GUI.enabled=!w.Finished;
                if(GUI.Button(tile,GUIContent.none,!game.SellMode&&!game.MoveMode&&w.SelectedDesign==i?selectedPortraitTile:portraitTile)){
                    w.SelectedDesign=i;game.SellMode=false;game.MoveMode=false;game.SelectedTowerId=0;
                }
                GUI.enabled=true;
                var color=GUI.color;GUI.color=locked?new Color(.45f,.45f,.45f):poor?new Color(.7f,.7f,.7f):Color.white;
                var portrait=portraits.Get(i);
                if(portrait!=null)GUI.DrawTexture(new Rect(tile.x+4,tile.y+3,64,56),portrait,ScaleMode.ScaleToFit);
                GUI.color=color;
                GUI.Label(new Rect(tile.x,tile.y,20,18),(slot+1).ToString(),portraitKey);
                GUI.Label(new Rect(tile.x,tile.y+57,tile.width,18),locked?"LOCKED":d.Cost+"g",portraitPrice);
                if(tile.Contains(Event.current.mousePosition))hovered=i;
                slot++;
            }
            // A dedicated command slot replaces inventory: enter removal mode, then choose a tower.
            var remove=new Rect(box.x+8+(slot%game.BuildColumns)*78,box.y+48+(slot/game.BuildColumns)*80,72,76);
            GUI.enabled=!w.Finished;
            if(GUI.Button(remove,GUIContent.none,game.SellMode?selectedPortraitTile:portraitTile)){game.SellMode=!game.SellMode;game.MoveMode=false;game.SelectedTowerId=0;}
            GUI.enabled=true;
            DrawRemoveIcon(new Rect(remove.x+16,remove.y+10,40,40));
            GUI.Label(new Rect(remove.x,remove.y+56,72,18),"REMOVE [X]",portraitPrice);
            if(remove.Contains(Event.current.mousePosition))GUI.Label(new Rect(box.x-8,box.y-67,box.width+8,60),"Remove tower · [X]\nSelect this command, then click one of your towers. Its sale refund is shown before removal.",placementHint);
            int inspect=hovered>=0?hovered:w.SelectedDesign;
            if(inspect>=0&&inspect<w.Config.Catalog.Length) {
                var design=w.Config.Catalog[inspect];
                GUI.Label(new Rect(box.x+8,box.yMax-23,box.width-16,20),game.SellMode?"REMOVE MODE · click your tower":design.Name,section);
                if(hovered>=0) {
                    string text=BuildTooltip(w,inspect);
                    float width=Mathf.Min(Mathf.Max(box.width,360),Screen.width/game.UiScale-24);
                    float height=placementHint.CalcHeight(new GUIContent(text),width);
                    var tip=new Rect(box.xMax-width,Mathf.Max(8,box.y-height-8),width,height);
                    GUI.Label(tip,text,placementHint);
                }
            }
        }
    }
}
