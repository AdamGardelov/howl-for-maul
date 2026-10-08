using UnityEngine;
namespace FrostMaze
{
    public sealed partial class PrototypeHud
    {
        BuildQueueView buildQueue;
        void DrawBuildFeedback() {
            if(Event.current.type!=EventType.Repaint||game.SetupOpen||game.MenuOpen)return;
            if(buildQueue==null)buildQueue=game.GetComponent<BuildQueueView>();
            if(buildQueue!=null&&!game.World.Finished)foreach(var order in buildQueue.Orders) {
                var p=game.View.WorldToScreenPoint(new Vector3(order.Origin.X+order.Spec.Width*.5f,.12f,order.Origin.Y+order.Spec.Height*.5f));
                if(p.z<=0||p.x<0||p.x>Screen.width||p.y<0||p.y>Screen.height)continue;
                var box=new Rect(p.x-(game.World.Players.Length>1?24:10),Screen.height-p.y-9,game.World.Players.Length>1?48:20,18);
                if(box.Overlaps(game.Sidebar)||CoversCompactHud(box))continue;
                var color=GUI.color;GUI.color=new Color(.03f,.05f,.06f,.9f);GUI.DrawTexture(box,Texture2D.whiteTexture);
                GUI.color=order.Number==1?new Color(1,.8f,.4f):new Color(.5f,1,.9f);GUI.Label(box,order.Label,mapLabel);GUI.color=color;
            }
            if(!string.IsNullOrEmpty(game.PlacementFailure)&&Time.unscaledTime<game.PlacementFailureUntil) {
                float width=Mathf.Min(420*game.UiScale,Screen.width-24*game.UiScale);
                var box=new Rect((Screen.width-width)*.5f,game.TopHud.yMax+8*game.UiScale,width,48*game.UiScale);
                GUI.Label(box,"CANNOT BUILD · "+game.PlacementFailure,placementHint);
            }
        }
    }
}
