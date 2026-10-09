using UnityEngine;
using FrostMaze.Simulation.Online;
namespace FrostMaze
{
    public sealed partial class PrototypeHud
    {
        bool onlineForm,advancedLan;string relayCode="",retryPassword="";string playerName="Player",joinAddress="127.0.0.1",portText="27888",lobbyPassword="",lobbyError="";
        Vector2 lobbyScroll,notificationScroll;
        void DrawOnlineEntry(){
            if(HudButton(onlineForm?"CLOSE MULTIPLAYER":"MULTIPLAYER",primary))onlineForm=!onlineForm;
            if(!onlineForm)return;
            GUILayout.Label("Private games for up to four players. Share a join code with your friends.",small);
            GUILayout.Label("Name",small);playerName=GUILayout.TextField(playerName,24);
            GUILayout.Label("Password (optional)",small);lobbyPassword=GUILayout.PasswordField(lobbyPassword,'•',64);
            advancedLan=GUILayout.Toggle(advancedLan,"Advanced: direct LAN / IP");
            if(advancedLan){
                GUILayout.Label("Host address (join only)",small);joinAddress=GUILayout.TextField(joinAddress,128);
                GUILayout.Label("Port",small);portText=GUILayout.TextField(portText,5);
                GUILayout.BeginHorizontal();if(HudButton("HOST LAN",button))Connect(true);if(HudButton("JOIN LAN",button))Connect(false);GUILayout.EndHorizontal();
                GUILayout.Label("Direct connections require a reachable host address and port.",small);
            }else{
                if(HudButton("HOST ONLINE",primary))ConnectRelay(true);
                GUILayout.Label("Friend's join code",small);relayCode=GUILayout.TextField(relayCode,16).ToUpperInvariant();
                GUI.enabled=relayCode.Trim().Length>=4;if(HudButton("JOIN WITH CODE",button))ConnectRelay(false);GUI.enabled=true;
                if(!RelayGateway.Configured)GUILayout.Label("Online service setup is pending in this build. Solo and LAN are available.",small);
            }
            GUILayout.Label(lobbyError,small);
        }
        void ConnectRelay(bool host){var online=OnlineGame.Create();_ = online.ConnectRelay(host,game.Map.name,playerName,lobbyPassword,relayCode);lobbyPassword="";}
        void Connect(bool host){if(!int.TryParse(portText,out int port)||port<1024||port>65535){lobbyError="Use a port from 1024 to 65535.";return;}
            string address=joinAddress.Trim();int split=address.LastIndexOf(':');if(!host&&split>0&&int.TryParse(address.Substring(split+1),out int invitePort)&&invitePort>=1024&&invitePort<=65535){port=invitePort;address=address.Substring(0,split);}
            var online=OnlineGame.Create();if(host)online.Host(game.Map.name,playerName,lobbyPassword,port);else online.Join(address,port,playerName,lobbyPassword);lobbyPassword="";}
        void DrawLobby(){
            var online=OnlineGame.Current;var net=online.Session;var matrix=GUI.matrix;float scale=game.UiScale;GUI.matrix=Matrix4x4.Scale(new Vector3(scale,scale,1));
            float width=Screen.width/scale,height=Screen.height/scale;
            bool factions=net!=null&&net.Stage==Stage.Factions;
            float panelWidth=Mathf.Min(factions?1080:720,width-32);
            var box=new Rect((width-panelWidth)/2,24,panelWidth,height-48);Frame(box);
            GUILayout.BeginArea(new Rect(box.x+24,box.y+18,box.width-48,box.height-36));
            if(RelayNotices.Available){
                notificationScroll=GUILayout.BeginScrollView(notificationScroll,GUILayout.Height(Mathf.Min(200,box.height*.3f)));
                GUILayout.Label("ONLINE ACCOUNT NOTICE",label);GUILayout.Label(RelayNotices.Text,label);GUILayout.EndScrollView();
                if(HudButton("COPY NOTICE",button))GUIUtility.systemCopyBuffer=RelayNotices.Text;
                if(HudButton("ACKNOWLEDGE",button))RelayNotices.Acknowledge();
            }
            if(online.Pending||net==null||online.Error.Length>0){
                BrandHeading(130);GUILayout.Space(24);GUILayout.Label(online.Pending?online.Status:online.Error,label);
                if(!online.Pending&&online.IsRelay){GUILayout.Label("Lobby password (if required)",small);retryPassword=GUILayout.PasswordField(retryPassword,'•',64);if(HudButton("RETRY",primary)){online.RetryRelay(retryPassword);retryPassword="";}}
                if(HudButton(online.Pending?"CANCEL":"BACK",button))game.LeaveOnline();GUILayout.EndArea();GUI.matrix=matrix;return;
            }
            var config=game.World.Config;Member me=null;foreach(var member in net.Members)if(member.Id==net.LocalId)me=member;
            LobbyBrandHeading(box.width-48,net.Map+" · "+(online.LocalOnly?"SOLO":net.IsHost?"HOST":"CONNECTED PLAYER"),
                net.Members.Count==1?"FACTION → DIFFICULTY → DEFEND · Solo starts at Last Stand":"LOBBY → FACTION → STARTING POSITION → DIFFICULTY → DEFEND",
                factions?"CHOOSE YOUR FACTION":net.Stage.ToString().ToUpperInvariant());
            lobbyScroll=GUILayout.BeginScrollView(lobbyScroll);
            foreach(var member in net.Members){GUILayout.BeginHorizontal(badge);string faction=member.Faction>=0&&member.Faction<config.Factions.Length?config.Factions[member.Faction].Name:"Choosing faction";
                string start=member.Lane>=0&&member.Lane<config.StartNames.Length?" · "+config.StartNames[member.Lane]:"";
                GUILayout.Label(member.Name+(member.Id==net.LocalId?" (you)":"")+" · "+faction+start+(member.Vote>=0?" · "+((FrostMaze.Simulation.Difficulty)member.Vote):"")+(member.Ready?" · READY":""),label);
                if(net.IsHost&&member.Id!=0&&HudButton("KICK",button,GUILayout.Width(60)))net.Send(new Packet{Kind=Kind.Kick,A=member.Id});GUILayout.EndHorizontal();}
            if(net.IsHost&&!online.LocalOnly&&net.Stage==Stage.Lobby&&!online.IsRelay){GUILayout.Label("Share your host address and port "+net.Port+". Password is sent separately.",label);
                if(HudButton("COPY LAN INVITE",button)){string host="127.0.0.1";try{foreach(var ip in System.Net.Dns.GetHostAddresses(System.Net.Dns.GetHostName()))if(ip.AddressFamily==System.Net.Sockets.AddressFamily.InterNetwork&&!System.Net.IPAddress.IsLoopback(ip)){host=ip.ToString();break;}}catch{}GUIUtility.systemCopyBuffer=host+":"+net.Port;}}
            if(online.IsRelay&&net.Stage==Stage.Lobby){
                GUILayout.Label("JOIN CODE   "+online.JoinCode,label);
                if(HudButton("COPY JOIN CODE",primary))GUIUtility.systemCopyBuffer=online.JoinCode;
                GUILayout.Label("Share this code privately. Send any password separately. The host must keep the game open.",small);
            }
            if(me!=null){
                if(factions)DrawFactionBrowser(net,me,box.width-68);
                if(net.Stage==Stage.Lanes){GUILayout.Label("Choose a unique start. All enemy lanes remain active.",label);for(int i=0;i<config.BuilderStarts.Length;i++){bool taken=false;foreach(var m in net.Members)if(m.Id!=me.Id&&m.Lane==i)taken=true;GUI.enabled=!taken;
                    if(HudButton((me.Lane==i?"✓ ":"")+"Start "+(i+1)+(i<config.StartNames.Length?" · "+config.StartNames[i]:"")+(taken?" · taken":""),me.Lane==i?primary:button))net.Send(new Packet{Kind=Kind.Lane,A=i});GUI.enabled=true;}}
                if(net.Stage==Stage.Difficulty){GUILayout.Label(net.Members.Count==1?"Choose your challenge. You defend every lane from Last Stand.":"Most votes wins; a tied Normal vote takes priority, otherwise Relaxed wins the tie.",label);for(int i=0;i<3;i++)if(HudButton((me.Vote==i?"✓ ":"")+((FrostMaze.Simulation.Difficulty)i),button))net.Send(new Packet{Kind=Kind.Difficulty,A=i});}
            }
            GUILayout.EndScrollView();
            if(me!=null){
                if(net.Stage!=Stage.Difficulty){GUI.enabled=net.Stage==Stage.Lobby||net.Stage==Stage.Factions&&me.Faction>=0||net.Stage==Stage.Lanes&&me.Lane>=0;
                    string ready=net.Stage==Stage.Factions&&me.Faction>=0?"CONFIRM "+config.Factions[me.Faction].Name.ToUpperInvariant():"CONFIRM / READY";
                    if(HudButton(me.Ready?"NOT READY":ready,primary))net.Send(new Packet{Kind=Kind.Ready});GUI.enabled=true;}
                if(net.IsHost&&net.Stage==Stage.Lobby){bool ready=true;foreach(var m in net.Members)if(!m.Ready)ready=false;GUI.enabled=ready;if(HudButton("BEGIN FACTION SELECTION",primary))net.Send(new Packet{Kind=Kind.Begin});GUI.enabled=true;}
            }
            if(net.Notice.Length>0)GUILayout.Label(net.Notice,small);
            if(HudButton("LEAVE",button))game.LeaveOnline();GUILayout.EndArea();GUI.matrix=matrix;
        }
        void DrawVoteStatus(){if(!game.NetworkMatch)return;var net=game.Net;var r=new Rect(Screen.width*.5f-230,72,460,70);
            if(net.Failure.Length>0){GUI.Label(r,net.Failure,placementHint);return;}
            if(OnlineGame.Current.LocalOnly){if(net.Paused)GUI.Label(r,"PAUSED · [P] to resume",placementHint);return;}
            if(net.Votes>0||net.Paused)GUI.Label(r,(net.Paused?"PAUSED":"PAUSE VOTE")+$" · {net.Votes}/{net.RequiredVotes} votes · [P] to vote"+(net.Votes>0?$" · {net.VoteRemaining:0}s":""),placementHint);
        }
    }
}
