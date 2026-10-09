using System;
using System.Linq;
using UnityEngine;
using FrostMaze.Simulation;
using FrostMaze.Simulation.Online;
namespace FrostMaze
{
    // Explicit opt-in process integration probe: actual packaged maps, transport and simulation.
    public sealed class StandaloneNetworkSmoke : MonoBehaviour
    {
        static bool Flag(string name)=>Array.IndexOf(Environment.GetCommandLineArgs(),name)>=0;
        public static bool Requested=>Flag("--howl-network-host")||Flag("--howl-network-client")||Flag("--howl-utp-host")||Flag("--howl-utp-client")||Flag("--howl-relay-host")||Flag("--howl-relay-client");
        readonly System.Threading.CancellationTokenSource cancellation=new System.Threading.CancellationTokenSource();

        Session session;Stage lastStage=(Stage)(-1);float started;bool built,launched,voted,resumed,seenPause;int held;long pausedTick;bool finished;
        static string Argument(string key,string fallback){var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,key);return i>=0&&i+1<args.Length?args[i+1]:fallback;}
        static Scenario Resolve(string name){var asset=Resources.Load<MapDefinition>(name);return JsonUtility.FromJson<Scenario>(JsonUtility.ToJson(asset.Settings));}
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Run(){if(Requested)new GameObject("Packaged network smoke").AddComponent<StandaloneNetworkSmoke>();}
        async void Start(){try{
            if(FindFirstObjectByType<Prototype>()!=null||FindFirstObjectByType<AudioSource>()!=null)throw new Exception("Unexpected presentation in data-only network probe");
            Application.targetFrameRate=60;started=Time.realtimeSinceStartup;session=new Session(Resolve,StateDigest.Scenario);
            bool host=Flag("--howl-network-host")||Flag("--howl-utp-host")||Flag("--howl-relay-host");string map=Argument("--howl-network-map","Rimewatch");
            if(Flag("--howl-relay-host")||Flag("--howl-relay-client")){
                var ticket=await new RelayGateway().Connect(host,Argument("--howl-relay-code",""),cancellation.Token,s=>Debug.Log("HOWL_RELAY_PROGRESS "+s));
                if(finished){ticket.Dispose();return;}
                if(host){session.HostOver(map,"Relay host","smoke",ticket.Transport);Debug.Log("HOWL_RELAY_INVITE "+ticket.Code);}
                else session.JoinOver("Relay client","smoke",ticket.Transport);
            }else if(Flag("--howl-utp-host")||Flag("--howl-utp-client")){
                var transport=UtpPacketTransport.Local(host,ushort.Parse(Argument("--howl-network-port",host?"0":"27888")));
                if(host){session.HostOver(map,"UTP host","smoke",transport);Debug.Log("HOWL_NETWORK_LISTEN "+transport.Port);}
                else session.JoinOver("UTP client","smoke",transport);
            }else if(host){session.Host(map,"Packaged host","smoke",int.Parse(Argument("--howl-network-port","0")));Debug.Log("HOWL_NETWORK_LISTEN "+session.Port);}
            else session.Join("127.0.0.1",int.Parse(Argument("--howl-network-port","27888")),"Packaged client","smoke");
        }catch(Exception e){Fail(e);}}
        void Update(){if(finished||session==null)return;try{
            if(Time.realtimeSinceStartup-started>90)throw new Exception("Packaged network probe timed out");
            session.Update(Time.unscaledDeltaTime);if(session.Failure.Length>0)throw new Exception(session.Failure);
            var me=session.Members.FirstOrDefault(m=>m.Id==session.LocalId);if(me==null)return;
            if(session.IsHost&&session.Stage==Stage.Lobby&&session.Members.Count==2&&session.Members.All(m=>m.Ready))session.Send(new Packet{Kind=Kind.Begin});
            if(session.Stage!=lastStage){lastStage=session.Stage;
                if(lastStage==Stage.Lobby)session.Send(new Packet{Kind=Kind.Ready});
                if(lastStage==Stage.Factions){session.Send(new Packet{Kind=Kind.Faction,A=session.IsHost?0:1});session.Send(new Packet{Kind=Kind.Ready});}
                if(lastStage==Stage.Lanes){session.Send(new Packet{Kind=Kind.Lane,A=session.IsHost?0:1});session.Send(new Packet{Kind=Kind.Ready});}
                if(lastStage==Stage.Difficulty)session.Send(new Packet{Kind=Kind.Difficulty,A=1});
            }
            var w=session.World;if(w==null)return;
            if(!built){int design=w.Config.Factions[w.Players[w.ActivePlayer].Faction].Designs[0];w.SelectedDesign=design;var origin=w.SnapBuildOrigin(w.BuilderPosition);bool found=false;
                for(float y=origin.Y-3;y<origin.Y+3&&!found;y+=w.PlacementStep)for(float x=origin.X-3;x<origin.X+3&&!found;x+=w.PlacementStep)if(w.CanBuild(x,y,out _)){session.Submit(new Order{Kind=ActionKind.Build,Design=design,X=x,Y=y});found=true;}
                if(!found)throw new Exception("No legal paid footprint");built=true;
            }
            if(w.Tick>180&&!launched){if(w.Grid.Towers.Count!=2)throw new Exception("Missing network purchase");foreach(int owner in new[]{0,1}){var tower=w.Grid.Towers.Single(t=>w.TowerOwner(t.Id)==owner);if(w.Players[owner].Gold!=w.Config.StartingGold/2-w.Config.Catalog[tower.Design].Cost)throw new Exception("Incorrect paid wallet");}if(session.IsHost)session.Submit(new Order{Kind=ActionKind.Launch});launched=true;}
            if(!voted&&(session.IsHost?w.Tick>240:session.Votes>0)){session.Send(new Packet{Kind=Kind.PauseVote});voted=true;}
            if(session.Paused&&!resumed){if(!seenPause){seenPause=true;pausedTick=w.Tick;}if(w.Tick!=pausedTick)throw new Exception("Paused ticks advanced");held++;
                if(!resumed&&((!session.IsHost&&held>30)||(session.IsHost&&session.Votes>0))){session.Send(new Packet{Kind=Kind.PauseVote});resumed=true;}
            }
            if(!session.IsHost&&w.Tick>=420){if(!seenPause||!resumed)throw new Exception("Missing pause cycle");Debug.Log("HOWL_NETWORK_CLIENT_PASS "+w.Config.Name+" tick="+w.Tick+" hash="+StateDigest.Of(w));Finish();}
            if(session.IsHost&&seenPause&&resumed&&session.Members.Count(m=>m.Connected)==1){if(!session.Paused)throw new Exception("Disconnect failed to pause");session.Send(new Packet{Kind=Kind.PauseVote});if(session.Paused)throw new Exception("Remaining player cannot resume");Debug.Log("HOWL_NETWORK_HOST_PASS "+w.Config.Name+" lanes="+w.LaneCount+" paid=2");Finish();}
        }catch(Exception e){Fail(e);}}
        void Finish(){finished=true;cancellation.Cancel();session?.Dispose();Application.Quit(0);}
        void Fail(Exception error){finished=true;cancellation.Cancel();Debug.LogError("HOWL_NETWORK_FAILURE "+error.GetType().Name+": "+(error is Unity.Services.Core.RequestFailedException?"Online service request failed":error.Message));session?.Dispose();Application.Quit(1);}
        void OnDestroy(){cancellation.Cancel();session?.Dispose();}
    }
}
