using UnityEngine;
using FrostMaze.Simulation.Online;
namespace FrostMaze
{
    public sealed partial class PrototypeHud
    {
        bool onlineForm;string playerName="Player",joinAddress="127.0.0.1",portText="27888",lobbyPassword="",lobbyError="";
        Vector2 lobbyScroll;
        void DrawOnlineEntry(){
            if(GUILayout.Button(onlineForm?"HIDE ONLINE":"PLAY ONLINE",primary))onlineForm=!onlineForm;
            if(!onlineForm)return;
            GUILayout.Label("Name",small);playerName=GUILayout.TextField(playerName,24);
            GUILayout.Label("Host address (join only)",small);joinAddress=GUILayout.TextField(joinAddress,128);
            GUILayout.Label("Port",small);portText=GUILayout.TextField(portText,5);
            GUILayout.Label("Password (optional)",small);lobbyPassword=GUILayout.PasswordField(lobbyPassword,'•',64);
            GUILayout.BeginHorizontal();
            if(GUILayout.Button("HOST",button))Connect(true);
            if(GUILayout.Button("JOIN",button))Connect(false);
            GUILayout.EndHorizontal();GUILayout.Label(lobbyError,small);
            GUILayout.Label("Direct connection · up to 4 players. Host must be reachable at the shared address and TCP port.",small);
        }
        void Connect(bool host){if(!int.TryParse(portText,out int port)||port<1024||port>65535){lobbyError="Use a port from 1024 to 65535.";return;}
            string address=joinAddress.Trim();int split=address.LastIndexOf(':');if(!host&&split>0&&int.TryParse(address.Substring(split+1),out int invitePort)&&invitePort>=1024&&invitePort<=65535){port=invitePort;address=address.Substring(0,split);}
            var online=OnlineGame.Create();if(host)online.Host(game.Map.name,playerName,lobbyPassword,port);else online.Join(address,port,playerName,lobbyPassword);lobbyPassword="";}
        void DrawLobby(){
            var online=OnlineGame.Current;var net=online.Session;var matrix=GUI.matrix;float scale=game.UiScale;GUI.matrix=Matrix4x4.Scale(new Vector3(scale,scale,1));
            float width=Screen.width/scale,height=Screen.height/scale;var box=new Rect((width-680)/2,35,680,height-70);Frame(box);
            GUILayout.BeginArea(new Rect(box.x+24,box.y+20,632,box.height-40));GUILayout.Label("HOWL FOR MAUL",title);
            if(net==null||online.Error.Length>0){GUILayout.Label(online.Error,label);if(GUILayout.Button("BACK",button))game.LeaveOnline();GUILayout.EndArea();GUI.matrix=matrix;return;}
            GUILayout.Label(net.Map+" · "+(online.LocalOnly?"SOLO":net.IsHost?"HOST":"CONNECTED PLAYER"),section);
            GUILayout.Label("LOBBY → FACTION → STARTING POSITION → DIFFICULTY → DEFEND",small);
            GUILayout.Label(net.Stage.ToString().ToUpperInvariant(),title);
            if(net.IsHost&&!online.LocalOnly&&net.Stage==Stage.Lobby){GUILayout.Label("Share your host address and port "+net.Port+". Password is sent separately.",label);
                if(GUILayout.Button("COPY LAN INVITE",button)){string host="127.0.0.1";try{foreach(var ip in System.Net.Dns.GetHostAddresses(System.Net.Dns.GetHostName()))if(ip.AddressFamily==System.Net.Sockets.AddressFamily.InterNetwork&&!System.Net.IPAddress.IsLoopback(ip)){host=ip.ToString();break;}}catch{}GUIUtility.systemCopyBuffer=host+":"+net.Port;}}
            lobbyScroll=GUILayout.BeginScrollView(lobbyScroll);
            var config=game.World.Config;
            foreach(var member in net.Members){GUILayout.BeginHorizontal(badge);string faction=member.Faction>=0&&member.Faction<config.Factions.Length?config.Factions[member.Faction].Name:"Choosing faction";
                GUILayout.Label(member.Name+(member.Id==net.LocalId?" (you)":"")+" · "+faction+(member.Lane>=0?" · Start "+(member.Lane+1):"")+(member.Vote>=0?" · "+((FrostMaze.Simulation.Difficulty)member.Vote):"")+(member.Ready?" · READY":""),label);
                if(net.IsHost&&member.Id!=0&&GUILayout.Button("KICK",button,GUILayout.Width(60)))net.Send(new Packet{Kind=Kind.Kick,A=member.Id});GUILayout.EndHorizontal();}
            Member me=null;foreach(var member in net.Members)if(member.Id==net.LocalId)me=member;
            if(me!=null){
                if(net.Stage==Stage.Factions)for(int i=0;i<config.Factions.Length;i++)if(GUILayout.Button((me.Faction==i?"✓ ":"")+config.Factions[i].Name,me.Faction==i?primary:button))net.Send(new Packet{Kind=Kind.Faction,A=i});
                if(net.Stage==Stage.Factions&&me.Faction>=0)GUILayout.Label(config.Factions[me.Faction].Description,label);
                if(net.Stage==Stage.Lanes){GUILayout.Label("Choose a unique start. All enemy lanes remain active.",label);for(int i=0;i<config.BuilderStarts.Length;i++){bool taken=false;foreach(var m in net.Members)if(m.Id!=me.Id&&m.Lane==i)taken=true;GUI.enabled=!taken;
                    if(GUILayout.Button((me.Lane==i?"✓ ":"")+"Start "+(i+1)+(i<config.StartNames.Length?" · "+config.StartNames[i]:"")+(taken?" · taken":""),me.Lane==i?primary:button))net.Send(new Packet{Kind=Kind.Lane,A=i});GUI.enabled=true;}}
                if(net.Stage==Stage.Difficulty){GUILayout.Label("Most votes wins; a tied Normal vote takes priority, otherwise Relaxed wins the tie.",small);for(int i=0;i<3;i++)if(GUILayout.Button((me.Vote==i?"✓ ":"")+((FrostMaze.Simulation.Difficulty)i),button))net.Send(new Packet{Kind=Kind.Difficulty,A=i});}
                if(net.Stage!=Stage.Difficulty){GUI.enabled=net.Stage==Stage.Lobby||net.Stage==Stage.Factions&&me.Faction>=0||net.Stage==Stage.Lanes&&me.Lane>=0;
                    if(GUILayout.Button(me.Ready?"NOT READY":"CONFIRM / READY",primary))net.Send(new Packet{Kind=Kind.Ready});GUI.enabled=true;}
                if(net.IsHost&&net.Stage==Stage.Lobby){bool ready=true;foreach(var m in net.Members)if(!m.Ready)ready=false;GUI.enabled=ready;if(GUILayout.Button("BEGIN FACTION SELECTION",primary))net.Send(new Packet{Kind=Kind.Begin});GUI.enabled=true;}
            }
            GUILayout.EndScrollView();GUILayout.Label(net.Notice,label);if(GUILayout.Button("LEAVE",button))game.LeaveOnline();GUILayout.EndArea();GUI.matrix=matrix;
        }
        void DrawVoteStatus(){if(!game.NetworkMatch)return;var net=game.Net;var r=new Rect(Screen.width*.5f-230,72,460,70);
            if(net.Failure.Length>0){GUI.Label(r,net.Failure,placementHint);return;}
            if(OnlineGame.Current.LocalOnly){if(net.Paused)GUI.Label(r,"PAUSED · [P] to resume",placementHint);return;}
            if(net.Votes>0||net.Paused)GUI.Label(r,(net.Paused?"PAUSED":"PAUSE VOTE")+$" · {net.Votes}/{net.RequiredVotes} votes · [P] to vote"+(net.Votes>0?$" · {net.VoteRemaining:0}s":""),placementHint);
        }
    }
}
