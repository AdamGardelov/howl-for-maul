using System;
using System.Collections;
using System.IO;
using UnityEngine;
namespace FrostMaze
{
    // Explicit release QA fixture. Ordinary players never enter this path.
    public sealed class StandaloneMenuSmoke : MonoBehaviour
    {
        string output;bool mapCheck,onlineCheck;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Run(){var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"--howl-menu-check");bool maps=false;if(i<0){i=Array.IndexOf(args,"--howl-map-check");maps=true;}bool online=false;if(i<0){i=Array.IndexOf(args,"--howl-online-menu-check");maps=false;online=true;}if(i<0||i+1>=args.Length)return;
            var go=new GameObject("Standalone menu verification");DontDestroyOnLoad(go);var check=go.AddComponent<StandaloneMenuSmoke>();check.output=args[i+1];check.mapCheck=maps;check.onlineCheck=online;}
        IEnumerator Start()
        {
            Directory.CreateDirectory(output);
            if(onlineCheck){yield return OnlineCheck();yield break;}
            if(mapCheck){yield return MapCheck();yield break;}
            yield return new WaitForSecondsRealtime(3);
            var game=FindFirstObjectByType<Prototype>();
            if(game==null||!game.MainMenuOpen){Fail("Missing title screen");yield break;}
            long tick=game.World.Tick;
            yield return Capture("Title-Default");
            var page=typeof(PrototypeHud).GetField("titlePage",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
            page.SetValue(game.GetComponent<PrototypeHud>(),1);yield return Capture("Title-Settings");
            page.SetValue(game.GetComponent<PrototypeHud>(),2);yield return Capture("Title-Credits");
            if(game.World.Tick!=tick){Fail("Title advanced the match");yield break;}
            page.SetValue(game.GetComponent<PrototypeHud>(),0);
            Screen.SetResolution(960,600,false);yield return new WaitForSecondsRealtime(1);yield return Capture("Title-Small");
            game.OpenSetup();yield return Capture("Map-Selection");
            game.ChooseMap(Resources.Load<MapDefinition>("Ironfold"));yield return new WaitForSecondsRealtime(3);
            game=FindFirstObjectByType<Prototype>();if(game.MainMenuOpen){Fail("Map choice returned to title");yield break;}
            game.OpenMainMenu();yield return Capture("Title-Ironfold");
            game.OpenSetup();game.BeginSolo();for(int frame=0;frame<24;frame++)yield return null;yield return Capture("Solo-Factions");
            if(!game.LobbyOpen){Fail("Play Solo failed to enter faction selection");yield break;}
            game.Net.Send(new Simulation.Online.Packet{Kind=Simulation.Online.Kind.Faction,A=0});
            game.Net.Send(new Simulation.Online.Packet{Kind=Simulation.Online.Kind.Ready});
            for(int frame=0;frame<4;frame++)yield return null;
            if(game.Net.Stage!=Simulation.Online.Stage.Difficulty){Fail("Missing solo difficulty stage");yield break;}
            yield return Capture("Solo-Difficulty-Small");
            Screen.SetResolution(1440,900,false);yield return new WaitForSecondsRealtime(1);yield return Capture("Solo-Difficulty");
            Screen.SetResolution(960,600,false);yield return new WaitForSecondsRealtime(1);
            game.LeaveOnline();game.StartMatch();game.Paused=true;yield return Capture("Classic-HUD-Small");
            Screen.SetResolution(1440,900,false);yield return new WaitForSecondsRealtime(1);yield return Capture("Classic-HUD");
            game.ToggleMenu();yield return Capture("Pause-Settings");game.ToggleMenu();
            game.OpenMainMenu();yield return null;
            Debug.Log("HOWL_MENU_CHECK_PASS title/settings/credits/resize/map-change/solo-entry; world frozen");game.QuitGame();
        }
        sealed class PendingGateway : IRelayGateway {
            public readonly System.Threading.Tasks.TaskCompletionSource<RelayTicket> Result=new System.Threading.Tasks.TaskCompletionSource<RelayTicket>();
            public System.Threading.Tasks.Task<RelayTicket> Connect(bool host,string code,System.Threading.CancellationToken cancel,Action<string> status){status("Creating a private online lobby…");return Result.Task;}
        }
        IEnumerator OnlineCheck(){
            yield return new WaitForSecondsRealtime(3);var game=FindFirstObjectByType<Prototype>();game.OpenSetup();
            typeof(PrototypeHud).GetField("onlineForm",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).SetValue(game.GetComponent<PrototypeHud>(),true);
            yield return Capture("Online-Entry");
            var online=OnlineGame.Create();var gateway=new PendingGateway();var task=online.ConnectRelay(true,game.Map.name,"Host","","",gateway);
            yield return Capture("Online-Connecting");
            gateway.Result.SetException(new InvalidOperationException("Connection timed out. Check your internet connection and retry."));
            while(!task.IsCompleted)yield return null;
            if(online.Pending||online.Error.Length==0){Fail("Missing online failure state");yield break;}
            yield return Capture("Online-Retry");
            Screen.SetResolution(960,600,false);yield return new WaitForSecondsRealtime(1);yield return Capture("Online-Retry-Small");
            game.LeaveOnline();yield return null;yield return Capture("Online-Entry-Small");
            game.BeginSolo();yield return Capture("Online-Offline-Solo");
            if(game.Net==null||game.Net.Stage!=Simulation.Online.Stage.Factions){Fail("Offline solo broken");yield break;}
            game.LeaveOnline();Debug.Log("HOWL_ONLINE_MENU_PASS entry/pending/retry/resize/offline; gateway fixture, no cloud allocation");Application.Quit(0);
        }
        IEnumerator MapCheck()
        {
            yield return new WaitForSecondsRealtime(3);
            foreach(string map in new[]{"Ironfold","Rimewatch"}){
                var game=FindFirstObjectByType<Prototype>();game.ChooseMap(Resources.Load<MapDefinition>(map));yield return new WaitForSecondsRealtime(3);
                game=FindFirstObjectByType<Prototype>();game.OpenSetup();yield return Capture(map+"-Mirrored-Overview");
                foreach(var row in game.World.Config.LayoutRows)for(int x=0;x<row.Length/2;x++)if(row[x]!=row[row.Length-1-x]){Fail("Packaged map is asymmetric");yield break;}
                game.StartMatch();game.Paused=true;
                var camera=game.View.GetComponent<RtsCamera>();camera.ResetRotation();camera.FocusPoint(new Simulation.V2(32,map=="Ironfold"?6:13));camera.SetZoom(7,true);
                yield return Capture(map+"-Mirrored-Exit");
            }
            Debug.Log("HOWL_MAP_CHECK_PASS both packaged masks symmetric; overview and exit captures");Application.Quit(0);
        }
        IEnumerator Capture(string name){yield return new WaitForSecondsRealtime(.3f);yield return new WaitForEndOfFrame();
            var image=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(Path.Combine(output,"Howl-"+name+".png"),image.EncodeToPNG());Destroy(image);}
        void Fail(string message){Debug.LogError(message);Application.Quit(1);}
    }
}
