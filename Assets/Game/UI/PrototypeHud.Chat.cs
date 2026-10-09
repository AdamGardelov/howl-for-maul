using UnityEngine;
using FrostMaze.Simulation.Online;
namespace FrostMaze
{
    public sealed partial class PrototypeHud
    {
        string chatDraft="";Vector2 chatScroll;long seenChat;float chatChanged;Session chatSession;
        GUIStyle chatText,chatName,chatField;bool focusChat,wasChatOpen;
        static readonly Color[] ChatColors={new Color(.40f,.85f,1),new Color(.60f,.91f,.38f),new Color(1,.65f,.35f),new Color(.82f,.60f,1)};
        void DrawChat() {
            if(!game.ChatAvailable)return;
            var net=game.Net;Styles();
            if(chatText==null){chatText=new GUIStyle(label){richText=false,fontSize=15};chatName=new GUIStyle(small){richText=false,fontStyle=FontStyle.Bold};chatField=new GUIStyle(GUI.skin.textField){font=label.font,fontSize=16,padding=new RectOffset(8,8,6,4)};}
            if(chatSession!=net){chatSession=net;chatDraft="";seenChat=0;}
            long newest=net.Chat.Count==0?0:net.Chat[net.Chat.Count-1].Sequence;
            if(newest!=seenChat){seenChat=newest;chatChanged=Time.unscaledTime;chatScroll.y=float.MaxValue;}
            if(game.ChatOpen&&!wasChatOpen){focusChat=true;chatScroll.y=float.MaxValue;}
            wasChatOpen=game.ChatOpen;
            float s=game.UiScale;var matrix=GUI.matrix;GUI.matrix=Matrix4x4.Scale(Vector3.one*s);
            float width=Screen.width/s,height=Screen.height/s;
            var box=new Rect(20,height-476,Mathf.Min(440,width-40),222);
            if(game.LobbyOpen)box=new Rect(width-Mathf.Min(460,width-32)-16,height-304,Mathf.Min(460,width-32),250);
            if(!game.ChatOpen) {
                var trigger=Logical(game.ChatTriggerRect);
                if(HudButton(trigger,"CHAT [ENTER]",button))game.OpenChat();
                if(!game.LobbyOpen&&Time.unscaledTime-chatChanged<12&&net.Chat.Count>0) {
                    float y=box.y;int first=Mathf.Max(0,net.Chat.Count-3);
                    for(int i=first;i<net.Chat.Count;i++){var line=net.Chat[i];if(net.ChatMuted(line.PlayerId))continue;string text=line.Name+": "+line.Text;float h=chatText.CalcHeight(new GUIContent(text),box.width-16);GUI.Box(new Rect(box.x,y,box.width,h+5),GUIContent.none,placementHint);chatText.normal.textColor=ChatColors[line.Slot];GUI.Label(new Rect(box.x+8,y,box.width-16,h),text,chatText);y+=h+5;}
                }
                GUI.matrix=matrix;return;
            }
            var e=Event.current;
            if(e.type==EventType.KeyDown&&(e.keyCode==KeyCode.Return||e.keyCode==KeyCode.KeypadEnter)) {
                if(Time.frameCount>game.ChatOpenedFrame){if(net.SendChat(chatDraft))chatDraft="";game.CloseChat();GUI.FocusControl(null);}e.Use();
            } else if(e.type==EventType.KeyDown&&e.keyCode==KeyCode.Escape){game.CloseChat();GUI.FocusControl(null);e.Use();}
            Frame(box);GUI.Label(new Rect(box.x+12,box.y+8,box.width-60,22),"TEAM CHAT",section);
            if(HudButton(new Rect(box.xMax-38,box.y+5,28,24),"×",button))game.CloseChat();
            GUILayout.BeginArea(new Rect(box.x+12,box.y+34,box.width-24,box.height-94));
            chatScroll=GUILayout.BeginScrollView(chatScroll);
            if(net.Chat.Count==0)GUILayout.Label("Plan the maze together. Enter sends; Esc closes.",small);
            foreach(var line in net.Chat){if(net.ChatMuted(line.PlayerId))continue;chatName.normal.textColor=ChatColors[line.Slot];GUILayout.Label(line.Name,chatName);chatText.normal.textColor=new Color(.90f,.89f,.81f);GUILayout.Label(line.Text,chatText);}
            GUILayout.EndScrollView();GUILayout.EndArea();
            float muteX=box.x+12;
            foreach(var member in net.Members)if(member.Id!=net.LocalId&&member.Connected){bool muted=net.ChatMuted(member.Id);var r=new Rect(muteX,box.yMax-58,98,22);if(HudButton(r,(muted?"UNMUTE ":"MUTE ")+member.Name,small))net.MuteChat(member.Id,!muted);muteX+=102;}
            if(net.Notice.StartsWith("Chat is "))GUI.Label(new Rect(box.x+12,box.yMax-58,box.width-24,22),net.Notice,small);
            GUI.SetNextControlName("TeamChatInput");chatDraft=GUI.TextField(new Rect(box.x+12,box.yMax-32,box.width-94,26),chatDraft,Session.ChatLimit,chatField);
            if(focusChat&&e.type==EventType.Repaint){GUI.FocusControl("TeamChatInput");focusChat=false;}
            if(HudButton(new Rect(box.xMax-76,box.yMax-32,64,26),"SEND",button)){if(net.SendChat(chatDraft))chatDraft="";focusChat=true;}
            GUI.matrix=matrix;
        }
    }
}
