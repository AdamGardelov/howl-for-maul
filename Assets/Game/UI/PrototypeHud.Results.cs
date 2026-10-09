using UnityEngine;
namespace FrostMaze
{
    public sealed partial class PrototypeHud
    {
        GUIStyle resultHeading,resultCopy;
        void DrawResult() {
            if(!game.ResultOpen)return;Styles();
            var w=game.World;var matrix=GUI.matrix;float scale=game.UiScale;GUI.matrix=Matrix4x4.Scale(Vector3.one*scale);
            float width=Screen.width/scale,height=Screen.height/scale;
            var old=GUI.color;GUI.color=new Color(0,0,0,.48f);GUI.DrawTexture(new Rect(0,0,width,height),Texture2D.whiteTexture);GUI.color=old;
            var box=new Rect((width-Mathf.Min(568,width-40))*.5f,(height-400)*.5f,Mathf.Min(568,width-40),400);Frame(box);
            if(resultHeading==null){resultHeading=new GUIStyle(title){fontSize=26,alignment=TextAnchor.MiddleCenter,wordWrap=true};resultCopy=new GUIStyle(label){alignment=TextAnchor.MiddleCenter,wordWrap=true};}
            resultHeading.normal.textColor=w.Won?new Color(.95f,.84f,.53f):new Color(1,.63f,.40f);
            GUILayout.BeginArea(new Rect(box.x+28,box.y+24,box.width-56,box.height-48));
            GUILayout.Label(w.Won?"VICTORY":"DEFEAT",new GUIStyle(section){alignment=TextAnchor.MiddleCenter});GUILayout.Space(10);
            GUILayout.Label(w.Won?"THE HEARTH STILL BURNS":"THE HOWL PREVAILED",resultHeading);GUILayout.Space(10);
            GUILayout.Label(w.Won?"You held the roads. The refuge survives another night.":"The last ward has fallen. The Howl has reached the refuge.",resultCopy);GUILayout.Space(14);
            GUILayout.Label(w.Config.Name+" · "+w.Difficulty+" · Wave "+Mathf.Max(1,w.WaveIndex+1)+" / "+w.Config.Waves.Length,resultCopy);
            GUILayout.Label(w.Killed+" defeated  ·  "+w.Leaked+" leaked  ·  "+w.Lives+" lives remain",resultCopy);GUILayout.Space(18);
            if(HudButton("INSPECT DEFENSE",primary))game.InspectResult();
            if(HudButton(game.ChatAvailable?"LEAVE MATCH / MAIN MENU":"MAIN MENU",button)){game.CloseChat();game.OpenMainMenu();}
            GUILayout.EndArea();GUI.matrix=matrix;
        }
    }
}
