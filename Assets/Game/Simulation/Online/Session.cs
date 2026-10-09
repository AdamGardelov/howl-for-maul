using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
namespace FrostMaze.Simulation.Online
{
    // Host orders all gameplay actions and fixed ticks. Clients never step independently.
    public sealed class Session : IDisposable
    {
        readonly Func<string,Scenario> resolve;
        readonly Func<Scenario,string> signature;
        readonly List<Peer> peers=new List<Peer>();
        readonly ConcurrentQueue<TcpClient> accepted=new ConcurrentQueue<TcpClient>();
        readonly List<Order> orders=new List<Order>();
        IPacketConnection server;IPacketTransport transport;TcpListener listener;volatile bool disposed;volatile string connectError;volatile TcpClient connected;
        string password="",playerName="",fingerprint="";double clock,accumulator,voteUntil;long frame;
        int nextId=1;double lastServerMessage;readonly List<Member> members=new List<Member>();
        sealed class Peer {public IPacketConnection Link;public int Id=-1;public string Nonce;public double Accepted,LastMessage;public int Rate;public double RateReset;}
        public bool IsHost {get;private set;} public bool IsConnected {get;private set;}
        public int LocalId {get;private set;}=-1;
        public string Map {get;private set;}="";public Stage Stage {get;private set;}=Stage.Lobby;
        public IReadOnlyList<Member> Members=>members;public World World {get;private set;}
        public bool Paused {get;private set;}
        public int SpeedIndex {get;private set;}=MatchSpeeds.Normal;
        public float Speed=>MatchSpeeds.At(SpeedIndex);
        public string Notice {get;private set;}="";public string Failure {get;private set;}="";
        public int Port {get;private set;}
        public int LocalSlot=>members.FindIndex(m=>m.Id==LocalId);
        public double VoteRemaining=>Math.Max(0,voteUntil-clock);
        public int Votes=>members.Count(m=>m.Connected&&m.PauseVote);
        public int RequiredVotes=>members.Count(m=>m.Connected)/2+1;
        public Session(Func<string,Scenario> maps,Func<Scenario,string> hash){resolve=maps;signature=hash;}
        static int LastStand(Scenario config){int named=Array.FindIndex(config.StartNames,n=>string.Equals(n,"Last stand",StringComparison.OrdinalIgnoreCase));return named>=0?named:config.BuilderStarts.Length-1;}
        public void Solo(string map,string name){Map=map;IsHost=IsConnected=true;LocalId=0;members.Add(new Member{Id=0,Name=Clean(name),Lane=LastStand(resolve(map))});Stage=Stage.Factions;}
        public void Host(string map,string name,string secret,int port){
            Map=map;fingerprint=signature(resolve(map));password=secret??"";playerName=Clean(name);
            listener=new TcpListener(IPAddress.Any,port);listener.Start(8);Port=((IPEndPoint)listener.LocalEndpoint).Port;
            IsHost=true;IsConnected=true;LocalId=0;members.Add(new Member{Id=0,Name=playerName});
            new Thread(()=>{try{while(!disposed){var socket=listener.AcceptTcpClient();if(accepted.Count>=8)socket.Close();else accepted.Enqueue(socket);}}catch(SocketException){}catch(ObjectDisposedException){}}){IsBackground=true,Name="Howl accept"}.Start();
            Notice="Lobby open. Share your address and port.";
        }
        public void Join(string address,int port,string name,string secret){playerName=Clean(name);password=secret??"";Notice="Connecting…";
            new Thread(()=>{TcpClient socket=null;try{socket=new TcpClient();var task=socket.ConnectAsync(address,port);if(!task.Wait(8000))throw new TimeoutException();if(disposed)socket.Close();else connected=socket;}catch(Exception){socket?.Close();connectError="Could not connect. Check the address, port and host reachability.";}}){IsBackground=true,Name="Howl connect"}.Start();}
        public void HostOver(string map,string name,string secret,IPacketTransport link){
            transport=link;Map=map;fingerprint=signature(resolve(map));password=secret??"";playerName=Clean(name);
            IsHost=IsConnected=true;LocalId=0;members.Add(new Member{Id=0,Name=playerName});Notice="Lobby open. Share your join code.";
        }
        public void JoinOver(string name,string secret,IPacketTransport link){transport=link;playerName=Clean(name);password=secret??"";Notice="Connecting to host…";}
        void Accept(IPacketConnection link){if(peers.Count>=8){link.Dispose();return;}var peer=new Peer{Link=link,Nonce=Protocol.Nonce(),Accepted=clock,LastMessage=clock};peers.Add(peer);link.Send(new Packet{Kind=Kind.Challenge,Text=peer.Nonce,Extra=Protocol.Version});}
        static string Clean(string name){var chars=(name??"").Where(c=>!char.IsControl(c)&&c!='<'&&c!='>').Take(24).ToArray();var clean=new string(chars).Trim();return clean.Length==0?"Player":clean;}
        public void Send(Packet packet){if(disposed)return;if(IsHost)Handle(LocalId,packet);else server?.Send(packet);}
        public void Submit(Order order){Send(new Packet{Kind=Kind.Command,Orders=new[]{order}});}
        public void Update(double delta){if(disposed)return;clock+=Math.Max(0,delta);
            if(transport!=null){transport.Update();if(transport.Failure.Length>0){Fail(transport.Failure);return;}
                if(!IsHost&&server==null&&transport.Server!=null){server=transport.Server;lastServerMessage=clock;}
                if(IsHost)while(transport.TryAccept(out var link))Accept(link);
            }
            if(connectError!=null){Fail(connectError);return;}
            if(connected!=null){server=new Connection(connected);connected=null;lastServerMessage=clock;}
            if(IsHost){
                while(accepted.TryDequeue(out var socket))Accept(new Connection(socket));
                foreach(var peer in peers.ToArray()){
                    if(clock-peer.RateReset>=1){peer.Rate=0;peer.RateReset=clock;}
                    int received=0;while(received++<64&&peer.Link.TryRead(out var packet)){
                        peer.LastMessage=clock;if(++peer.Rate>120){peer.Link.Dispose();break;}
                        if(peer.Id<0){if(packet.Kind==Kind.Hello)Authenticate(peer,packet);else peer.Link.Dispose();}
                        else Handle(peer.Id,packet);
                    }
                    if(peer.Link.Closed||peer.Id<0&&clock-peer.Accepted>10||clock-peer.LastMessage>20){peer.Link.Dispose();peers.Remove(peer);Disconnected(peer.Id);}
                }
                if(voteUntil>0&&clock>=voteUntil){ClearVotes();Notice="Pause vote expired.";BroadcastLobby();}
                if(Stage==Stage.Match){accumulator+=Math.Max(0,Math.Min(delta,.25))*(Paused?1:Speed);int steps=0;while(accumulator>=FrostMaze.Simulation.World.FixedDelta&&steps++<24){accumulator-=FrostMaze.Simulation.World.FixedDelta;var batch=orders.ToArray();orders.Clear();foreach(var order in batch)Apply(order);if(!Paused)World.Step();frame++;var message=new Packet{Kind=Kind.Frame,A=SpeedIndex,Tick=frame,Flag=!Paused,Orders=batch,Extra=frame%60==0?StateDigest.Of(World):""};Broadcast(message);}}
                else if(clock-lastServerMessage>=1){lastServerMessage=clock;BroadcastLobby();}
            }else if(server!=null){
                int received=0;while(received++<128&&server.TryRead(out var p)){lastServerMessage=clock;Receive(p);if(disposed)return;}
                if(clock-lastServerMessage>20||server.Closed)Fail("Host disconnected. The match has stopped.");
                if(clock-lastPing>=2){lastPing=clock;server.Send(new Packet{Kind=Kind.Ping});}
            }
        }
        double lastPing;
        void Authenticate(Peer peer,Packet packet){
            if(packet.Extra!=Protocol.Version||!Protocol.Equal(packet.Text,Protocol.Proof(password,peer.Nonce))||Stage!=Stage.Lobby||members.Count>=4){peer.Link.Send(new Packet{Kind=Kind.Notice,Flag=true,Text="Join refused: password, game version, capacity or match already started."});peer.Link.CloseAfterFlush();return;}
            peer.Id=nextId++;members.Add(new Member{Id=peer.Id,Name=Clean(packet.Members.Length==1?packet.Members[0].Name:"")});peer.Link.Send(new Packet{Kind=Kind.Welcome,A=peer.Id,Text=Map,Extra=fingerprint});Notice="Player joined.";BroadcastLobby();
        }
        void Receive(Packet p){
            switch(p.Kind){
                case Kind.Challenge:if(p.Extra!=Protocol.Version){Fail("Different game protocol. Update both games.");return;}server.Send(new Packet{Kind=Kind.Hello,Text=Protocol.Proof(password,p.Text),Extra=Protocol.Version,Members=new[]{new Member{Name=playerName}}});break;
                case Kind.Welcome:
                    Scenario config;try{config=resolve(p.Text);}catch{Fail("Host map is unavailable.");return;}
                    if(signature(config)!=p.Extra){Fail("Different map or balance data. Update both games.");return;}LocalId=p.A;Map=p.Text;IsConnected=true;password="";Notice="Connected.";break;
                case Kind.Lobby:
                    if(!IsConnected)return;members.Clear();members.AddRange(p.Members);Stage=(Stage)p.A;Paused=p.Flag;voteUntil=p.B>0?clock+p.B/1000.0:0;Notice=p.Text;
                    if(Stage==Stage.Match&&World==null)CreateWorld((Difficulty)p.C);break;
                case Kind.Speed:if(World!=null&&MatchSpeeds.Valid(p.A))SpeedIndex=p.A;break;
                case Kind.Frame:
                    if(!MatchSpeeds.Valid(p.A)){Fail("Invalid shared game speed.");return;}SpeedIndex=p.A;
                    if(World==null||p.Tick!=frame+1){Fail("Network tick mismatch. Match stopped to protect game state.");return;}
                    foreach(var order in p.Orders)Apply(order);if(p.Flag)World.Step();frame=p.Tick;Paused=!p.Flag;
                    if(p.Extra.Length>0&&p.Extra!=StateDigest.Of(World))Fail("Simulation mismatch. Match stopped; reconnect requires a new lobby.");break;
                case Kind.Notice:Notice=p.Text;if(p.Flag)Fail(p.Text);break;
            }
        }
        void Handle(int id,Packet p){
            var member=members.Find(m=>m.Id==id&&m.Connected);if(member==null)return;
            if(p.Kind==Kind.Ping)return;
            if(p.Kind==Kind.Leave){var peer=peers.Find(q=>q.Id==id);peer?.Link.Dispose();Disconnected(id);return;}
            if(p.Kind==Kind.Kick&&id==0&&p.A!=0){var peer=peers.Find(q=>q.Id==p.A);peer?.Link.Dispose();Disconnected(p.A);return;}
            if(Stage==Stage.Match){
                if(p.Kind==Kind.Speed&&id==0&&!World.Finished&&MatchSpeeds.Valid(p.A)){
                    SpeedIndex=p.A;Notice="Game speed: "+MatchSpeeds.Label(SpeedIndex)+".";BroadcastLobby();
                }
                if(p.Kind==Kind.PauseVote){member.PauseVote=!member.PauseVote;if(Votes==0)voteUntil=0;else if(voteUntil==0)voteUntil=clock+20;
                    if(Votes>=RequiredVotes){Paused=!Paused;ClearVotes();Notice=Paused?"Match paused by vote.":"Match resumed by vote.";}BroadcastLobby();}
                if(p.Kind==Kind.Command&&p.Orders.Length==1&&orders.Count<64){var order=p.Orders[0];order.Player=members.IndexOf(member);if(Valid(order))orders.Add(order);}
                return;
            }
            if(p.Kind==Kind.Faction&&Stage==Stage.Factions&&p.A>=0&&p.A<resolve(Map).Factions.Length){member.Faction=p.A;member.Ready=false;}
            if(p.Kind==Kind.Lane&&Stage==Stage.Lanes&&p.A>=0&&p.A<resolve(Map).BuilderStarts.Length&&!members.Any(m=>m.Id!=id&&m.Lane==p.A)){member.Lane=p.A;member.Ready=false;}
            if(p.Kind==Kind.Difficulty&&Stage==Stage.Difficulty&&p.A>=0&&p.A<=2){member.Vote=p.A;member.Ready=true;}
            if(p.Kind==Kind.Ready&&(Stage==Stage.Lobby||Stage==Stage.Factions&&member.Faction>=0||Stage==Stage.Lanes&&member.Lane>=0))member.Ready=!member.Ready;
            if(p.Kind==Kind.Begin&&id==0&&Stage==Stage.Lobby&&members.All(m=>m.Ready)){Stage=Stage.Factions;ResetReady();}
            if(Stage==Stage.Factions&&members.All(m=>m.Ready&&m.Faction>=0)){
                if(members.Count==1){members[0].Lane=LastStand(resolve(Map));Stage=Stage.Difficulty;}
                else Stage=Stage.Lanes;
                ResetReady();
            }
            if(Stage==Stage.Lanes&&members.All(m=>m.Ready&&m.Lane>=0)){Stage=Stage.Difficulty;ResetReady();}
            if(Stage==Stage.Difficulty&&members.All(m=>m.Vote>=0)){int[] votes=new int[3];foreach(var m in members)votes[m.Vote]++;int selected=1;foreach(int option in new[]{0,2})if(votes[option]>votes[selected])selected=option;Stage=Stage.Match;CreateWorld((Difficulty)selected);}
            BroadcastLobby();
        }
        bool Valid(Order o)=>Enum.IsDefined(typeof(ActionKind),o.Kind)&&!float.IsNaN(o.X)&&!float.IsInfinity(o.X)&&!float.IsNaN(o.Y)&&!float.IsInfinity(o.Y)&&o.X>=0&&o.Y>=0&&o.X<World.Config.Width&&o.Y<World.Config.Height&&o.Design>=0&&o.Design<World.Config.Catalog.Length;
        void Apply(Order o){int active=World.ActivePlayer;if(o.Player<0||o.Player>=World.Players.Length)return;World.SelectPlayer(o.Player);int selected=World.SelectedDesign;string message="";
            switch(o.Kind){
                case ActionKind.Build:if(World.DesignAvailable(o.Design)){World.SelectedDesign=o.Design;World.OrderBuild(o.X,o.Y,out message,o.Append);}break;
                case ActionKind.Move:World.MoveBuilder(new V2(o.X,o.Y));break;
                case ActionKind.Cancel:World.MoveBuilder(World.BuilderPosition);break;
                case ActionKind.Sell:message=World.Sell(o.X,o.Y)?"Tower removed.":"No owned tower here.";break;
                case ActionKind.ChooseFaction:World.ChooseFaction(o.Target,out message);break;
                case ActionKind.Upgrade:World.Upgrade(o.Target,out message);break;
                case ActionKind.Launch:message=World.StartWave()?"Wave launched.":"Wave cannot start yet.";break;
            }
            if(o.Kind!=ActionKind.ChooseFaction)World.SelectedDesign=selected;World.SelectPlayer(active);if(o.Player==LocalSlot&&message.Length>0)Notice=message;
        }
        void CreateWorld(Difficulty difficulty){var options=new MatchOptions{PlayerCount=members.Count,UseSelectedSoloStart=true,Difficulty=difficulty,Factions=members.Select(m=>m.Faction).ToArray(),StartingPositions=members.Select(m=>m.Lane).ToArray()};World=new World(resolve(Map),options);World.SelectPlayer(LocalSlot);frame=0;accumulator=0;SpeedIndex=MatchSpeeds.Normal;Notice="All lanes active. Start wave 1 when ready; later waves follow a 30s countdown.";}
        void Disconnected(int id){var member=members.Find(m=>m.Id==id);if(member==null||!member.Connected)return;
            if(Stage==Stage.Match){member.Connected=false;Paused=true;ClearVotes();Notice=member.Name+" disconnected. Vote to resume with the remaining players.";}
            else {members.Remove(member);Stage=Stage.Lobby;foreach(var m in members){m.Faction=m.Lane=m.Vote=-1;m.Ready=false;}Notice="Player left. Lobby setup reset.";}
            BroadcastLobby();
        }
        void ResetReady(){foreach(var m in members)m.Ready=false;}
        void ClearVotes(){foreach(var m in members)m.PauseVote=false;voteUntil=0;}
        void BroadcastLobby(){Broadcast(new Packet{Kind=Kind.Lobby,A=(int)Stage,B=(int)(VoteRemaining*1000),C=World==null?1:(int)World.Difficulty,Flag=Paused,Text=Notice,Members=members.ToArray()});if(World!=null)Broadcast(new Packet{Kind=Kind.Speed,A=SpeedIndex});}
        void Broadcast(Packet p){foreach(var peer in peers)if(peer.Id>=0&&!peer.Link.Closed)peer.Link.Send(p);}
        void Fail(string message){Paused=true;Failure=Notice=message;Dispose();}
        public void Dispose(){if(disposed)return;disposed=true;IsConnected=false;server?.Dispose();transport?.Dispose();connected?.Close();listener?.Stop();foreach(var p in peers)p.Link.Dispose();while(accepted.TryDequeue(out var s))s.Close();}
    }
}
