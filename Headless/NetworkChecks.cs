using System;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.Threading;
using FrostMaze.Simulation;
using FrostMaze.Simulation.Online;
static class NetworkChecks
{
    static Scenario Map(string name){string file=Path.Combine(AppContext.BaseDirectory,"Layouts",name+".txt");return name=="Ironfold"?ReferenceMaps.Ironfold(File.ReadAllText(file)):ReferenceMaps.Rimewatch(File.ReadAllText(file));}
    static Session New()=>new Session(Map,StateDigest.Scenario);
    static void Check(bool value,string message){if(!value)throw new Exception(message);}
    static readonly System.Collections.Generic.Dictionary<Session,Stage> sent=new System.Collections.Generic.Dictionary<Session,Stage>();
    static void Setup(Session s,int faction,int lane){var me=s.Members.FirstOrDefault(m=>m.Id==s.LocalId);if(me==null)return;
        if(s.IsHost&&s.Stage==Stage.Lobby&&s.Members.Count==2&&s.Members.All(m=>m.Ready))s.Send(new Packet{Kind=Kind.Begin});
        if(sent.TryGetValue(s,out var previous)&&previous==s.Stage)return;sent[s]=s.Stage;
        if(s.Stage==Stage.Lobby){if(!me.Ready)s.Send(new Packet{Kind=Kind.Ready});if(s.IsHost&&s.Members.Count==2&&s.Members.All(m=>m.Ready))s.Send(new Packet{Kind=Kind.Begin});}
        if(s.Stage==Stage.Factions){if(me.Faction<0)s.Send(new Packet{Kind=Kind.Faction,A=faction});if(!me.Ready)s.Send(new Packet{Kind=Kind.Ready});}
        if(s.Stage==Stage.Lanes){if(me.Lane<0)s.Send(new Packet{Kind=Kind.Lane,A=lane});if(!me.Ready)s.Send(new Packet{Kind=Kind.Ready});}
        if(s.Stage==Stage.Difficulty&&me.Vote<0)s.Send(new Packet{Kind=Kind.Difficulty,A=1});
    }
    static void Build(Session session){var w=session.World;int design=w.Config.Factions[w.Players[w.ActivePlayer].Faction].Designs[0];w.SelectedDesign=design;
        var start=w.SnapBuildOrigin(w.BuilderPosition);for(float y=Math.Max(0,start.Y-3);y<Math.Min(w.Config.Height,start.Y+3);y+=w.PlacementStep)for(float x=Math.Max(0,start.X-3);x<Math.Min(w.Config.Width,start.X+3);x+=w.PlacementStep)if(w.CanBuild(x,y,out _)){session.Submit(new Order{Kind=ActionKind.Build,Player=0,Design=design,X=x,Y=y});return;}throw new Exception("No legal paid test footprint");}
    public static int Peer(int port){using(var client=New()){client.Join("127.0.0.1",port,"Remote tester","test-password");var timeout=Stopwatch.StartNew();bool built=false,spoof=false,pauseVoted=false,resumeVoted=false;int held=0;long frozen=-1;
        while(timeout.Elapsed.TotalSeconds<35){client.Update(.01);Check(client.Failure.Length==0,client.Failure);Setup(client,1,1);
            if(client.World!=null){var w=client.World;if(!built){Build(client);built=true;}
                if(w.Tick>180&&!spoof){var foreign=w.Grid.Towers.FirstOrDefault(t=>w.TowerOwner(t.Id)==0);if(foreign!=null){client.Submit(new Order{Kind=ActionKind.Upgrade,Player=0,Target=foreign.Id});spoof=true;}}
                if(!pauseVoted&&client.Votes>0&&!client.Paused){client.Send(new Packet{Kind=Kind.PauseVote});pauseVoted=true;}
                if(client.Paused){if(frozen<0)frozen=w.Tick;Check(w.Tick==frozen,"Paused client tick advanced");if(++held>30&&!resumeVoted){client.Send(new Packet{Kind=Kind.PauseVote});resumeVoted=true;}}
                if(w.Tick>=620){Check(spoof&&pauseVoted&&resumeVoted,"Missing peer phases");Console.WriteLine("PASS remote paid build, owner isolation, pause/resume and 620 ordered ticks; "+StateDigest.Of(w));return 0;}
            }Thread.Sleep(2);
        }throw new Exception("Remote process timed out");}}
    static void Pair(string map){using(var host=New()){host.Host(map,"Host tester","test-password",0);
        var start=new ProcessStartInfo("dotnet"){UseShellExecute=false,RedirectStandardOutput=true,RedirectStandardError=true};start.ArgumentList.Add(typeof(NetworkChecks).Assembly.Location);start.ArgumentList.Add("--network-peer");start.ArgumentList.Add(host.Port.ToString());using(var peer=Process.Start(start)){
            var timeout=Stopwatch.StartNew();bool built=false,launched=false,voted=false,resumed=false,walletChecked=false,sawPause=false;long pauseTick=-1;
            try{while(timeout.Elapsed.TotalSeconds<35){host.Update(.01);Setup(host,0,0);Check(host.Failure.Length==0,host.Failure);
                if(host.World!=null){var w=host.World;if(!built){Build(host);built=true;}
                    if(w.Tick>230&&!walletChecked){Check(w.Grid.Towers.Count==2,"Both paid towers missing");Check(w.Grid.Towers.All(t=>t.Level==1),"Foreign upgrade accepted");foreach(int owner in new[]{0,1}){var tower=w.Grid.Towers.Single(t=>w.TowerOwner(t.Id)==owner);Check(w.Players[owner].Gold==600-w.Config.Catalog[tower.Design].Cost,"Separate wallet mismatch");}walletChecked=true;}
                    if(w.Tick>240&&!launched){host.Submit(new Order{Kind=ActionKind.Launch});launched=true;}
                    if(w.Tick>300&&!voted){host.Send(new Packet{Kind=Kind.PauseVote});Check(!host.Paused,"One of two votes paused the match");voted=true;}
                    if(host.Paused){if(!sawPause){pauseTick=w.Tick;sawPause=true;}if(!resumed){Check(w.Tick==pauseTick,"Paused host tick advanced");if(host.Votes>0){host.Send(new Packet{Kind=Kind.PauseVote});Check(!host.Paused,"Majority failed to resume");resumed=true;}}}
                    if(peer.HasExited){var output=peer.StandardOutput.ReadToEnd();var errors=peer.StandardError.ReadToEnd();Check(peer.ExitCode==0,errors+output);for(int i=0;i<20;i++){host.Update(.01);Thread.Sleep(2);}Check(host.Members.Count(m=>m.Connected)==1&&host.Paused,"Disconnect failed to pause");host.Send(new Packet{Kind=Kind.PauseVote});Check(!host.Paused,"Remaining player cannot resume");Check(walletChecked&&sawPause&&resumed,"Missing host checks");Console.WriteLine("PASS "+map+" two-process session, all "+w.LaneCount+" lanes, paid wallets, authority and disconnect recovery");Console.WriteLine(output);return;}
                }Thread.Sleep(2);
            }throw new Exception("Host process timed out");}finally{if(!peer.HasExited)peer.Kill();}
        }}}
    static void Refusals(){using(var host=New())using(var wrong=New())using(var mismatch=new Session(Map,c=>"different-data")){
        host.Host("Rimewatch","Host","secret",0);wrong.Join("127.0.0.1",host.Port,"Wrong","incorrect");mismatch.Join("127.0.0.1",host.Port,"Mismatch","secret");
        for(int i=0;i<1500&&(wrong.Failure.Length==0||mismatch.Failure.Length==0);i++){host.Update(.01);wrong.Update(.01);mismatch.Update(.01);Thread.Sleep(2);}
        Check(wrong.Failure.Length>0&&!wrong.IsConnected,"Wrong password accepted");Check(mismatch.Failure.Contains("Different map"),"Content mismatch accepted");Console.WriteLine("PASS wrong password and mismatched game data refused");
    }}
    static void VotesAndLanes(){using(var host=New())using(var a=New())using(var b=New()){
        host.Host("Rimewatch","Host","",0);a.Join("127.0.0.1",host.Port,"A","");b.Join("127.0.0.1",host.Port,"B","");
        void Pump(int count){for(int i=0;i<count;i++){host.Update(.01);a.Update(.01);b.Update(.01);Thread.Sleep(2);}}
        for(int i=0;i<500&&host.Members.Count<3;i++)Pump(1);Pump(10);Check(host.Members.Count==3,"Three-player join failed");
        foreach(var s in new[]{host,a,b})s.Send(new Packet{Kind=Kind.Ready});Pump(20);host.Send(new Packet{Kind=Kind.Begin});Pump(20);
        foreach(var s in new[]{host,a,b}){s.Send(new Packet{Kind=Kind.Faction,A=0});s.Send(new Packet{Kind=Kind.Ready});}Pump(20);
        host.Send(new Packet{Kind=Kind.Lane,A=0});a.Send(new Packet{Kind=Kind.Lane,A=0});Pump(20);Check(host.Members.Count(m=>m.Lane==0)==1,"Duplicate lane accepted");
        a.Send(new Packet{Kind=Kind.Lane,A=1});b.Send(new Packet{Kind=Kind.Lane,A=2});Pump(20);foreach(var s in new[]{host,a,b})s.Send(new Packet{Kind=Kind.Ready});Pump(20);
        host.Send(new Packet{Kind=Kind.Difficulty,A=0});a.Send(new Packet{Kind=Kind.Difficulty,A=1});b.Send(new Packet{Kind=Kind.Difficulty,A=2});Pump(20);Check(host.World.Difficulty==Difficulty.Normal,"Tied votes should choose Normal");Check(host.World.Players.All(p=>p.Gold==400),"Three-player budget mismatch");
        host.Send(new Packet{Kind=Kind.PauseVote});Pump(10);Check(!host.Paused&&host.RequiredVotes==2,"Bad three-player majority");Pump(2100);Check(host.Votes==0&&!host.Paused,"Expired vote persisted");
        a.Send(new Packet{Kind=Kind.PauseVote});b.Send(new Packet{Kind=Kind.PauseVote});Pump(20);Check(host.Paused&&a.Paused&&b.Paused,"Two of three failed to pause");Console.WriteLine("PASS unique lane reservation, tied difficulty, 400-gold wallets, majority and expiring pause votes");
    }}
    static void CapacityAndValidation(){using(var host=New()){
        host.Host("Rimewatch","Host","",0);var clients=new[]{New(),New(),New(),New()};
        void Pump(int n){for(int i=0;i<n;i++){host.Update(.01);foreach(var c in clients)c.Update(.01);Thread.Sleep(2);}}
        try{foreach(var c in clients)c.Join("127.0.0.1",host.Port,"Guest","");Pump(250);
            Check(host.Members.Count==4,"Lobby exceeds four or lost valid peers");var admitted=clients.Where(c=>c.IsConnected).ToArray();Check(admitted.Length==3&&clients.Count(c=>c.Failure.Length>0)==1,"Capacity rejection missing");
            host.Send(new Packet{Kind=Kind.Ready});foreach(var c in admitted)c.Send(new Packet{Kind=Kind.Ready});Pump(30);host.Send(new Packet{Kind=Kind.Begin});Pump(30);
            using(var late=New()){late.Join("127.0.0.1",host.Port,"Late","");for(int i=0;i<250&&late.Failure.Length==0;i++){Pump(1);late.Update(.01);}Check(late.Failure.Length>0,"Late join accepted");}
            var team=new[]{host}.Concat(admitted).ToArray();for(int i=0;i<4;i++){team[i].Send(new Packet{Kind=Kind.Faction,A=i});team[i].Send(new Packet{Kind=Kind.Ready});}Pump(30);
            for(int i=0;i<4;i++)team[i].Send(new Packet{Kind=Kind.Lane,A=i});Pump(30);foreach(var c in team)c.Send(new Packet{Kind=Kind.Ready});Pump(30);foreach(var c in team)c.Send(new Packet{Kind=Kind.Difficulty,A=1});Pump(30);
            Check(host.World.Players.All(p=>p.Gold==300),"Four-player wallet split incorrect");
            admitted[0].Submit(new Order{Kind=ActionKind.Build,X=float.NaN,Y=10});admitted[0].Submit(new Order{Kind=ActionKind.Build,X=10000,Y=10});Pump(30);Check(host.World.Grid.Towers.Count==0,"Malformed coordinates mutated world");
            host.Send(new Packet{Kind=Kind.PauseVote});admitted[0].Send(new Packet{Kind=Kind.PauseVote});Pump(20);Check(!host.Paused&&host.RequiredVotes==3,"Two of four is not a majority");admitted[1].Send(new Packet{Kind=Kind.PauseVote});Pump(20);Check(host.Paused,"Three of four failed to pause");
            bool rejected=false;try{Protocol.Decode(new byte[Protocol.MaxBytes+1]);}catch(InvalidDataException){rejected=true;}Check(rejected,"Oversized packet accepted");
            Console.WriteLine("PASS capacity four, late join rejection, 300-gold wallets, malformed coordinates and three-of-four vote");
        }finally{foreach(var c in clients)c.Dispose();}
    }}
    public static int Run(){try{Refusals();VotesAndLanes();CapacityAndValidation();Pair("Rimewatch");Pair("Ironfold");return 0;}catch(Exception e){Console.Error.WriteLine(e);return 1;}}
}
