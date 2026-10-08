using System.Collections.Generic;
using UnityEngine;
namespace FrostMaze
{
    public sealed partial class PrototypeHud
    {
        public readonly struct HealthOverlay
        {
            public readonly Rect Rect;
            public readonly float Fraction;
            public readonly Color Color;
            public readonly bool Selected;
            public readonly int EnemyId,TowerId;
            public HealthOverlay(Rect rect,float fraction,Color color,bool selected,int enemyId,int towerId)
            {Rect=rect;Fraction=fraction;Color=color;Selected=selected;EnemyId=enemyId;TowerId=towerId;}
        }
        readonly List<HealthOverlay> healthBars=new List<HealthOverlay>(256);
        public IReadOnlyList<HealthOverlay> HealthBars=>healthBars;
        public int SuppressedHealthBars {get;private set;}

        // Build once per repaint. Priority is selection, damaged towers, siege, then ordinary enemies.
        // Reused storage avoids adding per-frame collection garbage in a crowded battlefield.
        public void PrepareHealthBars(bool revealAll)
        {
            healthBars.Clear();SuppressedHealthBars=0;
            var world=game.World;
            foreach(var t in world.Grid.Towers)if(t.Id==game.SelectedTowerId)
                AddHealthBar(new Vector3(t.Center.X,1.7f,t.Center.Y),t.Health/t.Spec.Health,new Color(.28f,.9f,.73f),36,true,0,t.Id,revealAll);
            foreach(var e in world.Enemies)if(e.Id==game.SelectedId)EnemyHealthBar(e,true,revealAll);
            foreach(var t in world.Grid.Towers)if(t.Id!=game.SelectedTowerId&&(revealAll||t.Health<t.Spec.Health))
                AddHealthBar(new Vector3(t.Center.X,1.7f,t.Center.Y),t.Health/t.Spec.Health,new Color(.28f,.9f,.73f),36,false,0,t.Id,revealAll);
            for(int priority=0;priority<2;priority++)foreach(var e in world.Enemies)
                if(e.Id!=game.SelectedId&&(revealAll||e.Health<e.Spec.Health)&&e.Blocked==(priority==0))EnemyHealthBar(e,false,revealAll);
        }
        void EnemyHealthBar(FrostMaze.Simulation.Enemy enemy,bool selected,bool revealAll)
        {
            AddHealthBar(new Vector3(enemy.Position.X,enemy.Spec.Flying?2.5f:enemy.Spec.Radius*2+.25f,enemy.Position.Y),
                enemy.Health/enemy.Spec.Health,enemy.Blocked?new Color(1,.23f,.27f):enemy.Spec.Flying?new Color(.8f,.55f,1):new Color(1,.65f,.25f),28,selected,enemy.Id,0,revealAll);
        }
        void AddHealthBar(Vector3 position,float fraction,Color color,float width,bool selected,int enemyId,int towerId,bool revealAll)
        {
            var p=game.View.WorldToScreenPoint(position);
            if(!selected) {
                var edge=game.View.WorldToScreenPoint(position+game.View.transform.right*.75f);
                width=Mathf.Clamp(Mathf.Abs(edge.x-p.x),10,width);
            }
            var rect=new Rect(p.x-width*.5f,Screen.height-p.y,width,width<18?2:4);
            var padded=new Rect(rect.x-2,rect.y-2,rect.width+4,rect.height+4);
            if(p.z<=0||padded.xMin<=game.Sidebar.xMax||padded.xMax>=Screen.width||padded.yMin<0||padded.yMax>=Screen.height||padded.Overlaps(game.MinimapRect)||CoversCompactHud(padded))return;
            if(!revealAll&&!selected)foreach(var existing in healthBars) {
                var reserved=new Rect(existing.Rect.x-2,existing.Rect.y-2,existing.Rect.width+4,existing.Rect.height+4);
                if(padded.Overlaps(reserved)){SuppressedHealthBars++;return;}
            }
            healthBars.Add(new HealthOverlay(rect,Mathf.Clamp01(fraction),color,selected,enemyId,towerId));
        }
        void DrawHealth()
        {
            if(Event.current.type!=EventType.Repaint)return;
            PrepareHealthBars(Input.GetKey(KeyCode.LeftAlt)||Input.GetKey(KeyCode.RightAlt));
            // Selection was reserved first and is painted last, including when Alt reveals every bar.
            for(int i=healthBars.Count-1;i>=0;i--) {
                var bar=healthBars[i];var rect=bar.Rect;
                if(bar.Selected) {
                    GUI.color=new Color(.94f,.97f,.87f);
                    GUI.DrawTexture(new Rect(rect.x-2,rect.y-2,rect.width+4,rect.height+4),Texture2D.whiteTexture);
                }
                GUI.color=new Color(.07f,.1f,.14f);
                GUI.DrawTexture(new Rect(rect.x-1,rect.y-1,rect.width+2,rect.height+2),Texture2D.whiteTexture);
                rect.width*=bar.Fraction;GUI.color=bar.Color;GUI.DrawTexture(rect,Texture2D.whiteTexture);
            }
            GUI.color=Color.white;
        }
    }
}
