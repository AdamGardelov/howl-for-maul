using System;
using System.Collections;
using System.IO;
using UnityEngine;
using FrostMaze.Simulation;
using FrostMaze.Simulation.Online;
namespace FrostMaze
{
    // Opt-in rendered acceptance fixture. Never creates a lobby in ordinary play.
    public sealed class StandaloneSocialSmoke : MonoBehaviour
    {
        string output;Session guest;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Run(){var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"--howl-social-check");if(i<0||i+1>=args.Length)return;var root=new GameObject("Social UI verification");DontDestroyOnLoad(root);root.AddComponent<StandaloneSocialSmoke>().output=args[i+1];}
        static Scenario Resolve(string map)=>JsonUtility.FromJson<Scenario>(JsonUtility.ToJson(Resources.Load<MapDefinition>(map).Settings));
        IEnumerator Start(){
            Directory.CreateDirectory(output);yield return new WaitForSecondsRealtime(2);
            foreach(var map in new[]{"Rimewatch","Ironfold","Rimewatch","Ironfold"}) {
                var previous=FindFirstObjectByType<Prototype>();previous.OpenSetup();double at=Time.realtimeSinceStartupAsDouble;
                previous.ChooseMap(Resources.Load<MapDefinition>(map));yield return null;yield return null;
                var game=FindFirstObjectByType<Prototype>();
                if(game.Map.name!=map||!game.GetComponentInChildren<MapScenery>().UsesBakedSurfaces){Fail("Map switching did not load baked "+map);yield break;}
                Debug.Log("HOWL_SWITCH_READY "+map+" ms="+((Time.realtimeSinceStartupAsDouble-at)*1000).ToString("F1",System.Globalization.CultureInfo.InvariantCulture));
            }
            var current=FindFirstObjectByType<Prototype>();var online=OnlineGame.Create();online.Host(current.Map.name,"Anvilwarden","",0);var host=online.Session;
            guest=new Session(Resolve,StateDigest.Scenario);guest.Join("127.0.0.1",host.Port,"Rimekeeper","");
            for(int i=0;i<400&&!guest.IsConnected;i++)yield return Pump(1);
            if(!guest.IsConnected){Fail("Guest could not join");yield break;}
            host.SendChat("I'll guard Last Stand. Build a long maze up top.");guest.SendChat("On it! Saving frost control for the next air wave.");yield return Pump(20);
            current.OpenChat();yield return Capture("Lobby-Chat");current.CloseChat();
            host.Send(new Packet{Kind=Kind.Ready});guest.Send(new Packet{Kind=Kind.Ready});yield return Pump(10);host.Send(new Packet{Kind=Kind.Begin});yield return Pump(10);
            host.Send(new Packet{Kind=Kind.Faction,A=0});guest.Send(new Packet{Kind=Kind.Faction,A=1});host.Send(new Packet{Kind=Kind.Ready});guest.Send(new Packet{Kind=Kind.Ready});yield return Pump(10);
            host.Send(new Packet{Kind=Kind.Lane,A=7});guest.Send(new Packet{Kind=Kind.Lane,A=0});host.Send(new Packet{Kind=Kind.Ready});guest.Send(new Packet{Kind=Kind.Ready});yield return Pump(10);
            host.Send(new Packet{Kind=Kind.Difficulty,A=1});guest.Send(new Packet{Kind=Kind.Difficulty,A=1});yield return Pump(20);
            if(host.World==null||guest.World==null||host.Chat.Count!=2||guest.Chat.Count!=2){Fail("Staged match/chat failed");yield break;}
            current.OpenChat();yield return Capture("Match-Chat");current.CloseChat();
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"--howl-chat-input-check")>=0) {
                Debug.Log("HOWL_CHAT_KEYBOARD_READY");float deadline=Time.realtimeSinceStartup+25;
                while(!current.ChatOpen&&Time.realtimeSinceStartup<deadline)yield return Pump(1);
                if(!current.ChatOpen){Fail("Enter did not open chat");yield break;}
                var camera=current.View.GetComponent<RtsCamera>();var focus=camera.Focus;float yaw=camera.Yaw,zoom=camera.Zoom;int design=host.World.SelectedDesign,gold=host.World.Gold;long tick=host.World.Tick;
                while(host.Chat.Count==2&&Time.realtimeSinceStartup<deadline)yield return Pump(1);
                yield return Pump(3);
                if(host.Chat.Count!=3||host.Chat[2].Text!="qwer123 pgu x"||current.ChatOpen||host.World.SelectedDesign!=design||host.World.Gold!=gold||host.Votes!=0||camera.Focus!=focus||camera.Yaw!=yaw||camera.Zoom!=zoom||host.World.Tick<=tick){Fail("Chat keystrokes escaped into gameplay or message did not send");yield break;}
                Debug.Log("HOWL_CHAT_KEYBOARD_PASS");
            }
            guest.Dispose();guest=null;current.LeaveOnline();yield return null;
            current.StartMatch();current.Paused=true;var world=current.World;var route=world.LaneRoute(0,true);
            for(int i=0;i<world.Config.StartingLives;i++){var enemy=world.Spawn(new WaveSpec{Flying=true},route[route.Length-1]);enemy.Checkpoint=route.Length-1;world.Step();}
            world.Step();yield return null;
            if(!world.Finished||world.Lives!=0||!current.ResultOpen){Fail("Last life did not open defeat");yield break;}
            yield return Capture("Defeat");current.InspectResult();if(current.ResultOpen){Fail("Inspect did not dismiss result");yield break;}
            current.StartMatch();if(current.ResultOpen){Fail("Result leaked into new match");yield break;}
            // Controlled final-wave fixture tests the victory branch, not campaign balance.
            var config=Resolve(current.Map.name);config.Waves=new[]{config.Waves[0]};config.Waves[0].Count=1;var victory=new World(config);current.AdoptOnlineWorld(victory);current.Paused=true;victory.StartWave();for(int i=0;i<500&&!victory.Finished;i++){victory.Step();foreach(var enemy in victory.Enemies)enemy.Health=0;}yield return null;
            if(!victory.Won||!current.ResultOpen){Fail("Victory did not open result");yield break;}
            yield return Capture("Victory");Debug.Log("HOWL_SOCIAL_CHECK_PASS switches=4 lobbyChat=true matchChat=true lastLifeDefeat=true review=true restart=true victory=true");Application.Quit(0);
        }
        IEnumerator Pump(int count){for(int i=0;i<count;i++){guest?.Update(Time.unscaledDeltaTime);yield return null;}}
        IEnumerator Capture(string name){yield return new WaitForSecondsRealtime(.3f);yield return new WaitForEndOfFrame();var texture=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(Path.Combine(output,name+".png"),texture.EncodeToPNG());Destroy(texture);}
        void Fail(string message){Debug.LogError("HOWL_SOCIAL_CHECK_FAIL "+message);guest?.Dispose();Application.Quit(1);}
        void OnDestroy(){guest?.Dispose();}
    }
}
