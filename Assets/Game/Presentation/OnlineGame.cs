using System;
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
        void Awake(){if(Current!=null){Destroy(gameObject);return;}Current=this;DontDestroyOnLoad(gameObject);}
        static Scenario Resolve(string name){var map=Resources.Load<MapDefinition>(name);if(map==null||!map.Settings.SelectableMap)throw new ArgumentException("Unknown map");return JsonUtility.FromJson<Scenario>(JsonUtility.ToJson(map.Settings));}
        public static OnlineGame Create(){if(Current!=null)Current.Leave();return new GameObject("Online match").AddComponent<OnlineGame>();}
        public void Host(string map,string name,string password,int port,bool solo=false){LocalOnly=solo;Session=new Session(Resolve,StateDigest.Scenario);try{if(solo)Session.Solo(map,name);else Session.Host(map,name,password,port);}catch(Exception){Error="Could not open lobby. Check the port is available.";Session.Dispose();Session=null;}}
        public void Join(string address,int port,string name,string password){Session=new Session(Resolve,StateDigest.Scenario);Session.Join(address,port,name,password);}
        void Update(){var currentGame=FindFirstObjectByType<Prototype>();
            Session?.Update(LocalOnly&&currentGame!=null&&currentGame.MenuOpen?0:Time.unscaledDeltaTime);if(Session==null)return;
            if(Session.Failure.Length>0)Error=Session.Failure;
            var game=FindFirstObjectByType<Prototype>();if(game==null)return;
            if(Session.IsConnected&&game.Map.name!=Session.Map){game.ChooseMap(Resources.Load<MapDefinition>(Session.Map));return;}
            if(Session.World!=null&&Session.World!=game.World)game.AdoptOnlineWorld(Session.World);
        }
        public void Leave(){Session?.Dispose();if(Current==this)Current=null;Destroy(gameObject);}
        void OnDestroy(){Session?.Dispose();if(Current==this)Current=null;}
    }
}
