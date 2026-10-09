using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using FrostMaze.Simulation;
using FrostMaze.Simulation.Online;
namespace FrostMaze
{
    [DefaultExecutionOrder(-100)]
    public sealed class OnlineGame : MonoBehaviour
    {
        public static OnlineGame Current {get;private set;}
        public Session Session {get;private set;}
        public bool LocalOnly {get;private set;}
        public string Error="";
        public bool IsRelay {get;private set;}
        public bool Pending {get;private set;}
        public string Status {get;private set;}="";
        public string JoinCode {get;private set;}="";
        CancellationTokenSource attempt;UtpPacketTransport relayTransport;
        double deadline;bool left;
        bool retryHost;string retryMap,retryName,retryCode;
        public async Task ConnectRelay(bool host,string map,string name,string password,string code,IRelayGateway gateway=null){
            CancelAttempt();Session?.Dispose();Session=null;left=false;LocalOnly=false;IsRelay=true;Pending=true;Error="";JoinCode="";
            retryHost=host;retryMap=map;retryName=name;retryCode=(code??"").Trim().ToUpperInvariant();
            var current=new CancellationTokenSource();attempt=current;deadline=Time.realtimeSinceStartupAsDouble+45;
            try{
                if(!host&&(retryCode.Length<4||retryCode.Length>16||System.Linq.Enumerable.Any(retryCode,c=>!char.IsLetterOrDigit(c))))throw new InvalidOperationException("Enter the join code shared by your host.");
                var ticket=await (gateway??new RelayGateway()).Connect(host,retryCode,current.Token,s=>{if(!current.IsCancellationRequested)Status=s;});
                if(current.IsCancellationRequested||left){ticket.Dispose();return;}
                relayTransport=ticket.Transport;JoinCode=ticket.Code;Session=new Session(Resolve,StateDigest.Scenario);
                if(host)Session.HostOver(map,name,password,relayTransport);else Session.JoinOver(name,password,relayTransport);
                Status=host?"Opening relay connection…":"Checking lobby password and game version…";
            }catch(OperationCanceledException){}catch(Exception e){if(!current.IsCancellationRequested&&!left){Error=RelayGateway.Explain(e);Pending=false;Session?.Dispose();Session=null;relayTransport?.Dispose();relayTransport=null;}}
        }
        // Passwords are not saved for retries; protected lobbies ask for it again.
        public void RetryRelay(string password){_ = ConnectRelay(retryHost,retryMap,retryName,password,retryCode);}
        void CancelAttempt(){attempt?.Cancel();attempt=null;relayTransport?.Dispose();relayTransport=null;}

        void Awake(){if(Current!=null){Destroy(gameObject);return;}Current=this;DontDestroyOnLoad(gameObject);}
        static Scenario Resolve(string name){var map=Resources.Load<MapDefinition>(name);if(map==null||!map.Settings.SelectableMap)throw new ArgumentException("Unknown map");return JsonUtility.FromJson<Scenario>(JsonUtility.ToJson(map.Settings));}
        public static OnlineGame Create(){if(Current!=null)Current.Leave();return new GameObject("Online match").AddComponent<OnlineGame>();}
        public void Host(string map,string name,string password,int port,bool solo=false){LocalOnly=solo;Session=new Session(Resolve,StateDigest.Scenario);try{if(solo)Session.Solo(map,name);else Session.Host(map,name,password,port);}catch(Exception){Error="Could not open lobby. Check the port is available.";Session.Dispose();Session=null;}}
        public void Join(string address,int port,string name,string password){Session=new Session(Resolve,StateDigest.Scenario);Session.Join(address,port,name,password);}
        void Update(){var currentGame=FindFirstObjectByType<Prototype>();
            if(Pending&&Time.realtimeSinceStartupAsDouble>deadline){CancelAttempt();Session?.Dispose();Session=null;Pending=false;Error="Connection timed out. Check your internet connection and retry.";}
            Session?.Update(LocalOnly&&currentGame!=null&&currentGame.MenuOpen?0:Time.unscaledDeltaTime);if(Session==null)return;
            if(Session.Failure.Length>0){Error=Session.Failure;Pending=false;}
            if(Pending&&relayTransport!=null&&relayTransport.Ready&&Session.IsConnected){Pending=false;Status="Connected";}
            var game=FindFirstObjectByType<Prototype>();if(game==null)return;
            if(Session.IsConnected&&game.Map.name!=Session.Map){game.ChooseMap(Resources.Load<MapDefinition>(Session.Map));return;}
            if(Session.World!=null&&Session.World!=game.World)game.AdoptOnlineWorld(Session.World);
        }
        public void Leave(){left=true;Pending=false;CancelAttempt();Session?.Dispose();if(Current==this)Current=null;Destroy(gameObject);}
        void OnDestroy(){left=true;CancelAttempt();Session?.Dispose();if(Current==this)Current=null;}
    }
}
