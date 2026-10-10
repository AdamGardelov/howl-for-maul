using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using FrostMaze.Simulation;
using UnityEngine;
namespace FrostMaze
{
    // Explicit diagnostic flag only. Uses paid towers and real campaign enemies at Last Stand.
    public sealed class StandaloneEnemySmoke : MonoBehaviour
    {
        string output;
        sealed class Still : ICameraInput { public CameraIntent Read()=>default; }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Run()
        {
            var args=Environment.GetCommandLineArgs();int at=Array.IndexOf(args,"--howl-enemy-check");if(at<0||at+1>=args.Length)return;
            var go=new GameObject("Howl enemy package verification");DontDestroyOnLoad(go);go.AddComponent<StandaloneEnemySmoke>().output=args[at+1];
        }
        IEnumerator Start()
        {
            Directory.CreateDirectory(output);yield return new WaitForSecondsRealtime(3);
            foreach(string map in new[]{"Ironfold","Rimewatch"}) {
                FindFirstObjectByType<Prototype>().ChooseMap(Resources.Load<MapDefinition>(map));yield return new WaitForSecondsRealtime(3);
                var game=FindFirstObjectByType<Prototype>();game.StartMatch();game.Paused=true;game.SoundEnabled=false;game.MusicVolume=0;game.MoveMode=true;
                var w=game.World;var start=w.BuilderPosition;int built=0,spent=0;
                for(int y=0;y<4&&built<4;y++)for(int x=-3;x<=3&&built<4;x+=2) {
                    int cx=Mathf.FloorToInt(start.X)+x,cy=Mathf.FloorToInt(start.Y)+y*2;
                    if(!w.CanBuild(cx,cy,out _))continue;
                    int count=w.Grid.Towers.Count,gold=w.Gold,cost=w.BuildCost;if(!w.OrderBuild(cx,cy,out _))continue;
                    for(int t=0;t<700&&w.Grid.Towers.Count==count;t++)w.Step();
                    if(w.Grid.Towers.Count!=count+1||w.Gold!=gold-cost){Fail("Paid defense changed");yield break;}spent+=cost;built++;
                }
                if(built<4){Fail("Incomplete paid fixture");yield break;}
                int[] waves={0,11,12,13,10,4,18,19};var sites=new List<V2>();
                for(int row=0;row<6;row++)for(int col=0;col<7;col++){var p=start+new V2(-5+col*1.6f,-3+row*1.4f);if(w.Grid.Clear(p,p,.24f))sites.Add(p);}
                if(sites.Count<16){Fail("Insufficient clear review locations");yield break;}
                for(int n=0;n<16;n++) {
                    var e=w.Spawn(w.Config.Waves[waves[n%8]],sites[n]);if(e==null){Fail("Spawn rejected");yield break;}e.Checkpoint=w.LaneRoute(0,e.Spec.Flying).Length-1;e.Velocity=new V2(0,-1);
                }
                yield return null;yield return null;
                var forms=new HashSet<HowlForm>();foreach(var view in FindObjectsByType<EnemyView>(FindObjectsSortMode.None)){forms.Add(view.Form);if(view.GetComponentsInChildren<Collider>().Length!=0){Fail("Cosmetic blocker");yield break;}}
                if(forms.Count!=8){Fail("Missing creature type");yield break;}
                var camera=game.View.GetComponent<RtsCamera>();camera.SetInput(new Still());camera.ResetRotation();camera.FocusPoint(start+new V2(0,1));camera.SetZoom(8,true);
                yield return Capture(map+"-Enemy-Host");camera.SetZoom(5,true);yield return Capture(map+"-Enemy-Close");
                long before=w.Tick;for(int t=0;t<90;t++){w.Step();if(t%6==0)yield return null;}yield return null;
                if(w.Tick!=before+90){Fail("Combat did not advance");yield break;}yield return Capture(map+"-Paid-Combat");
                Screen.SetResolution(960,600,false);yield return new WaitForSecondsRealtime(.5f);yield return Capture(map+"-Small-HUD");Screen.SetResolution(1440,900,false);yield return new WaitForSecondsRealtime(.5f);
                Debug.Log("HOWL_ENEMY_CHECK_PASS "+map+" forms="+forms.Count+" paidTowers="+built+" spent="+spent+" killed="+w.Killed+" remaining="+w.Enemies.Count+" nativeWindows=false");
            }
            Debug.Log("HOWL_ENEMY_CHECK_COMPLETE");Application.Quit(0);
        }
        IEnumerator Capture(string name){yield return new WaitForSecondsRealtime(.3f);yield return new WaitForEndOfFrame();var texture=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(Path.Combine(output,name+".png"),texture.EncodeToPNG());Destroy(texture);}
        void Fail(string why){Debug.LogError("HOWL_ENEMY_CHECK_FAIL "+why);Application.Quit(1);}
    }
}
