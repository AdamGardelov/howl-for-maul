#if UNITY_EDITOR
using System;
using System.Collections;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using FrostMaze.Simulation;
using FrostMaze.Simulation.Online;
namespace FrostMaze.Tests
{
    public sealed class RelayTransportTests
    {
        static Scenario Map(string name)=>JsonUtility.FromJson<Scenario>(JsonUtility.ToJson(Resources.Load<MapDefinition>(name).Settings));
        static Session New()=>new Session(Map,StateDigest.Scenario);
        static bool Before(double deadline)=>Time.realtimeSinceStartupAsDouble<deadline;
        [UnityTest,Category("Relay")]
        public IEnumerator ReliableFragmentsSurviveDelayLossAndBackpressure()
        {
            using(var host=UtpPacketTransport.Local(true,0,25,3))using(var client=UtpPacketTransport.Local(false,host.Port,25,3)){
                IPacketConnection peer=null;double until=Time.realtimeSinceStartupAsDouble+12;
                while(Before(until)&&(peer==null||client.Server==null)){host.Update();client.Update();if(peer==null)host.TryAccept(out peer);yield return null;}
                Assert.That(peer,Is.Not.Null);Assert.That(client.Server,Is.Not.Null);
                const int count=160;int received=0,replies=0;
                for(int i=0;i<count;i++)client.Server.Send(new Packet{Kind=Kind.Notice,A=i,Text=new string('x',2000),Extra=new string('y',2000)});
                until=Time.realtimeSinceStartupAsDouble+35;
                while(Before(until)&&replies<count){host.Update();client.Update();
                    while(peer.TryRead(out var p)){Assert.That(p.A,Is.EqualTo(received++),"Lost, duplicated or reordered fragmented message");Assert.That(p.Text,Is.EqualTo(new string('x',2000)));Assert.That(p.Extra.Length,Is.EqualTo(2000));peer.Send(new Packet{Kind=Kind.Ping,A=p.A});}
                    while(client.Server.TryRead(out var p))Assert.That(p.A,Is.EqualTo(replies++));
                    Assert.That(peer.Closed||client.Server.Closed,Is.False);yield return null;
                }
                Assert.That(received,Is.EqualTo(count));Assert.That(replies,Is.EqualTo(count));
                for(int i=0;i<257;i++)client.Server.Send(new Packet{Kind=Kind.Ping});
                Assert.That(client.Server.Closed,Is.True,"Unbounded outgoing queue");Assert.That(client.Server.Error,Does.Contain("slow"));
            }
        }
        [UnityTest,Category("Relay")]
        public IEnumerator FourPlayersShareSetupPaidCommandsSpeedAndPause()
        {
            foreach(string map in new[]{"Rimewatch","Ironfold"}){
                using(var host=New())using(var a=New())using(var b=New())using(var c=New()){
                    var transport=UtpPacketTransport.Local(true);host.HostOver(map,"Host","secret",transport);
                    var sessions=new[]{host,a,b,c};foreach(var s in sessions.Skip(1))s.JoinOver("Friend","secret",UtpPacketTransport.Local(false,transport.Port));
                    double until=Time.realtimeSinceStartupAsDouble+12;
                    while(Before(until)&&sessions.Any(s=>s.Members.Count!=4)){foreach(var s in sessions)s.Update(.01);yield return null;}
                    Assert.That(sessions.All(s=>s.Members.Count==4),Is.True);
                    using(var full=New()){
                        full.JoinOver("Fifth","secret",UtpPacketTransport.Local(false,transport.Port));
                        until=Time.realtimeSinceStartupAsDouble+8;
                        while(Before(until)&&full.Failure.Length==0){foreach(var s in sessions)s.Update(.01);full.Update(.01);yield return null;}
                        Assert.That(full.Failure,Does.Contain("Join refused"));Assert.That(host.Members.Count,Is.EqualTo(4));
                    }
                    foreach(var s in sessions)s.Send(new Packet{Kind=Kind.Ready});
                    yield return Pump(sessions,30);host.Send(new Packet{Kind=Kind.Begin});yield return Pump(sessions,30);
                    for(int i=0;i<4;i++){sessions[i].Send(new Packet{Kind=Kind.Faction,A=i%Map(map).Factions.Length});sessions[i].Send(new Packet{Kind=Kind.Ready});}
                    yield return Pump(sessions,30);Assert.That(sessions.All(s=>s.Stage==Stage.Lanes),Is.True);
                    a.Send(new Packet{Kind=Kind.Lane,A=0});b.Send(new Packet{Kind=Kind.Lane,A=0});yield return Pump(sessions,30);
                    Assert.That(host.Members.Count(m=>m.Lane==0),Is.EqualTo(1),"Duplicate starts accepted");
                    // Reset the occupied choice, then assign four distinct starts.
                    a.Send(new Packet{Kind=Kind.Lane,A=1});b.Send(new Packet{Kind=Kind.Lane,A=2});yield return Pump(sessions,20);
                    for(int i=0;i<4;i++){sessions[i].Send(new Packet{Kind=Kind.Lane,A=i});sessions[i].Send(new Packet{Kind=Kind.Ready});}
                    yield return Pump(sessions,30);Assert.That(sessions.All(s=>s.Stage==Stage.Difficulty),Is.True);
                    foreach(var s in sessions)s.Send(new Packet{Kind=Kind.Difficulty,A=1});yield return Pump(sessions,60);
                    Assert.That(sessions.All(s=>s.World!=null),Is.True);
                    for(int i=0;i<4;i++){
                        var s=sessions[i];var w=s.World;int design=w.Config.Factions[w.Players[i].Faction].Designs[0];w.SelectedDesign=design;
                        var at=w.SnapBuildOrigin(w.BuilderPosition);bool built=false;
                        for(float y=at.Y-3;y<at.Y+3&&!built;y+=w.PlacementStep)for(float x=at.X-3;x<at.X+3&&!built;x+=w.PlacementStep)if(w.CanBuild(x,y,out _)){s.Submit(new Order{Kind=ActionKind.Build,Player=0,Design=design,X=x,Y=y});built=true;}
                        Assert.That(built,Is.True);
                    }
                    yield return Pump(sessions,500);Assert.That(host.World.Grid.Towers.Count,Is.EqualTo(4));
                    foreach(var tower in host.World.Grid.Towers){int owner=host.World.TowerOwner(tower.Id);Assert.That(host.World.Players[owner].Gold,Is.EqualTo(host.World.Config.StartingGold/4-host.World.Config.Catalog[tower.Design].Cost));}
                    a.Send(new Packet{Kind=Kind.Speed,A=3});yield return Pump(sessions,30);Assert.That(host.Speed,Is.EqualTo(1));
                    host.Send(new Packet{Kind=Kind.Speed,A=3});host.Submit(new Order{Kind=ActionKind.Launch});yield return Pump(sessions,90);Assert.That(sessions.All(s=>s.Speed==3),Is.True);
                    host.Send(new Packet{Kind=Kind.PauseVote});a.Send(new Packet{Kind=Kind.PauseVote});yield return Pump(sessions,30);Assert.That(host.Paused,Is.False);
                    b.Send(new Packet{Kind=Kind.PauseVote});yield return Pump(sessions,50);Assert.That(sessions.All(s=>s.Paused),Is.True);
                    long frozen=host.World.Tick;string hash=StateDigest.Of(host.World);foreach(var s in sessions)Assert.That(StateDigest.Of(s.World),Is.EqualTo(hash));
                    yield return Pump(sessions,30);Assert.That(host.World.Tick,Is.EqualTo(frozen));
                    foreach(var s in sessions.Take(3))s.Send(new Packet{Kind=Kind.PauseVote});yield return Pump(sessions,50);Assert.That(host.World.Tick,Is.GreaterThan(frozen));
                    c.Dispose();yield return Pump(new[]{host,a,b},60);Assert.That(host.Paused,Is.True);Assert.That(host.Members.Count(m=>m.Connected),Is.EqualTo(3));
                    host.Dispose();for(int i=0;i<120&&a.Failure.Length==0;i++){a.Update(.02);b.Update(.02);yield return null;}Assert.That(a.Failure,Does.Contain("Host disconnected"));
                }
            }
        }
        static IEnumerator Pump(Session[] sessions,int frames){for(int i=0;i<frames;i++){foreach(var s in sessions){s.Update(.02);Assert.That(s.Failure,Is.Empty);}yield return null;}}
        [UnityTest,Category("Relay")]
        public IEnumerator RefusesWrongPasswordAndMismatchedMaps()
        {
            using(var host=New())using(var wrong=New())using(var mismatch=new Session(Map,c=>"wrong data")){
                var transport=UtpPacketTransport.Local(true);host.HostOver("Ironfold","Host","secret",transport);
                wrong.JoinOver("Wrong","incorrect",UtpPacketTransport.Local(false,transport.Port));mismatch.JoinOver("Mismatch","secret",UtpPacketTransport.Local(false,transport.Port));
                double until=Time.realtimeSinceStartupAsDouble+12;
                while(Before(until)&&(wrong.Failure.Length==0||mismatch.Failure.Length==0)){host.Update(.01);wrong.Update(.01);mismatch.Update(.01);yield return null;}
                Assert.That(wrong.Failure,Does.Contain("Join refused"));Assert.That(mismatch.Failure,Does.Contain("Different map"));
            }
        }
        sealed class DeferredGateway : IRelayGateway
        {
            public readonly TaskCompletionSource<RelayTicket> Completion=new TaskCompletionSource<RelayTicket>();
            public Task<RelayTicket> Connect(bool host,string code,CancellationToken token,Action<string> status){status("Connecting…");return Completion.Task;}
        }
        [UnityTest,Category("Relay")]
        public IEnumerator CancelledConnectionCannotAttachLateAndOfflineStaysAvailable()
        {
            yield return new EnterPlayMode();
            var online=OnlineGame.Create();var gateway=new DeferredGateway();var attempt=online.ConnectRelay(true,"Ironfold","Host","","",gateway);
            Assert.That(online.Pending,Is.True);online.Leave();
            var transport=UtpPacketTransport.Local(true);gateway.Completion.SetResult(new RelayTicket{Transport=transport,Code="TEST"});yield return null;
            Assert.That(attempt.IsCompleted,Is.True);Assert.That(transport.IsDisposed,Is.True);Assert.That(OnlineGame.Current,Is.Null);
            online=OnlineGame.Create();gateway=new DeferredGateway();attempt=online.ConnectRelay(true,"Ironfold","Host","","",gateway);
            typeof(OnlineGame).GetField("deadline",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).SetValue(online,Time.realtimeSinceStartupAsDouble-1);
            yield return null;Assert.That(online.Pending,Is.False);Assert.That(online.Error,Does.Contain("timed out"));
            transport=UtpPacketTransport.Local(true);gateway.Completion.SetResult(new RelayTicket{Transport=transport,Code="LATE"});yield return null;Assert.That(transport.IsDisposed,Is.True);Assert.That(online.Session,Is.Null);online.Leave();yield return null;
            online=OnlineGame.Create();online.Host("Rimewatch","Solo","",0,true);Assert.That(online.Session.Stage,Is.EqualTo(Stage.Factions));online.Leave();yield return null;
            yield return new ExitPlayMode();
        }
    }
}
#endif
