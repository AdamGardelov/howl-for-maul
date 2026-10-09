using System;
using System.Collections;
using System.IO;
using UnityEngine;
namespace FrostMaze
{
    // Explicit release QA fixture. Ordinary players never enter this path.
    public sealed class StandaloneMenuSmoke : MonoBehaviour
    {
        string output;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Run(){var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"--howl-menu-check");if(i<0||i+1>=args.Length)return;
            var go=new GameObject("Standalone menu verification");DontDestroyOnLoad(go);go.AddComponent<StandaloneMenuSmoke>().output=args[i+1];}
        IEnumerator Start()
        {
            Directory.CreateDirectory(output);
            yield return new WaitForSecondsRealtime(3);
            var game=FindFirstObjectByType<Prototype>();
            if(game==null||!game.MainMenuOpen){Fail("Missing title screen");yield break;}
            long tick=game.World.Tick;
            yield return Capture("Title-Rimewatch");
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
            game.OpenSetup();game.BeginSolo();yield return new WaitForSecondsRealtime(1);yield return Capture("Solo-Factions");
            if(!game.LobbyOpen){Fail("Play Solo failed to enter faction selection");yield break;}
            game.LeaveOnline();game.OpenMainMenu();yield return null;
            Debug.Log("HOWL_MENU_CHECK_PASS title/settings/credits/resize/map-change/solo-entry; world frozen");game.QuitGame();
        }
        IEnumerator Capture(string name){yield return new WaitForSecondsRealtime(.3f);yield return new WaitForEndOfFrame();
            var image=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(Path.Combine(output,"Howl-"+name+".png"),image.EncodeToPNG());Destroy(image);}
        void Fail(string message){Debug.LogError(message);Application.Quit(1);}
    }
}
