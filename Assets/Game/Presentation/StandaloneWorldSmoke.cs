using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using FrostMaze.Simulation;
using UnityEngine;
namespace FrostMaze
{
    // Opt-in packaged visual/placement check; never active for normal players.
    public sealed class StandaloneWorldSmoke : MonoBehaviour
    {
        string output;
        sealed class Still : ICameraInput { public CameraIntent Read()=>default; }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Run() {
            var args=Environment.GetCommandLineArgs();int at=Array.IndexOf(args,"--howl-world-check");if(at<0||at+1>=args.Length)return;
            var go=new GameObject("World art verification");DontDestroyOnLoad(go);go.AddComponent<StandaloneWorldSmoke>().output=args[at+1];
        }
        IEnumerator Start() {
            Directory.CreateDirectory(output);yield return new WaitForSecondsRealtime(3);
            foreach(string map in new[]{"Ironfold","Rimewatch"}) {
                var game=FindFirstObjectByType<Prototype>();game.ChooseMap(Resources.Load<MapDefinition>(map));yield return new WaitForSecondsRealtime(3);
                game=FindFirstObjectByType<Prototype>();game.StartMatch();game.Paused=true;game.MusicVolume=0;
                var world=game.World;var camera=game.View.GetComponent<RtsCamera>();camera.SetInput(new Still());camera.ResetRotation();
                string mask=string.Join("\n",world.Config.LayoutRows);
                var backdrop=game.GetComponentInChildren<WorldBackdrop>();
                if(backdrop==null||backdrop.RefugeVertices<1000){Fail("Missing authored refuge");yield break;}
                var vertices=new HashSet<Vector3>();int total=0;
                foreach(var mesh in backdrop.GetComponentsInChildren<MeshFilter>())if(mesh.name.Contains(" refuge ")) {
                    if(mesh.GetComponent<Collider>()!=null){Fail("Refuge has collider");yield break;}
                    foreach(var v in mesh.sharedMesh.vertices) {
                        if(v.z>=0){Fail("Refuge intrudes into playable rectangle");yield break;}
                        vertices.Add(new Vector3(Mathf.Round(v.x*1000),Mathf.Round(v.y*1000),Mathf.Round(v.z*1000)));total++;
                    }
                }
                foreach(var v in vertices) {
                    float mirror=Mathf.Round(world.Config.Width*1000)-v.x;
                    // Quantization at a half-millimeter can land on opposite sides after subtraction.
                    if(!vertices.Contains(new Vector3(mirror,v.y,v.z))&&!vertices.Contains(new Vector3(mirror-1,v.y,v.z))&&!vertices.Contains(new Vector3(mirror+1,v.y,v.z))){Fail("Unpaired refuge vertex "+v);yield break;}
                }
                int spent=0,built=0;
                var start=world.BuilderPosition;
                for(int row=0;row<5&&built<8;row++)for(int col=-3;col<=3&&built<8;col+=2) {
                    int x=Mathf.FloorToInt(start.X)+col,z=Mathf.FloorToInt(start.Y)+row*2;
                    if(!world.CanBuild(x,z,out _))continue;
                    int before=world.Gold,cost=world.BuildCost,count=world.Grid.Towers.Count;
                    if(!world.OrderBuild(x,z,out _))continue;
                    for(int i=0;i<500&&world.Grid.Towers.Count==count;i++)world.Step();
                    if(world.Grid.Towers.Count!=count+1||world.Gold!=before-cost){Fail("Paid defense did not complete with exact cost");yield break;}
                    spent+=cost;built++;
                }
                if(built<3||mask!=string.Join("\n",world.Config.LayoutRows)){Fail("Paid defense / mask invariant failed");yield break;}
                game.MoveMode=true;yield return null;
                camera.FocusPoint(new V2(32,map=="Ironfold"?6:11));camera.SetZoom(12,true);yield return Capture(map+"-Last-Stand");
                camera.FocusPoint(new V2(32,0));camera.SetZoom(17,true);yield return Capture(map+"-Settlement");
                camera.FocusPoint(new V2(20,-3));camera.SetZoom(9,true);yield return Capture(map+"-Refuge-Detail");
                var landmarks=game.GetComponentInChildren<MapScenery>().LandmarkPositions;
                if(landmarks.Count>0){
                    var anchor=landmarks[0];camera.FocusPoint(new V2(anchor.x,anchor.z+1));camera.SetZoom(6,true);yield return Capture(map+"-Landmark-Detail");
                }
                camera.FocusPoint(start);camera.SetZoom(7,true);
                var tower=world.Grid.Towers[0];
                world.Spawn(new WaveSpec{Health=5000,Speed=1.5f},tower.Center+new V2(1.5f,1.5f));
                world.Spawn(new WaveSpec{Health=5000,Speed=3},tower.Center+new V2(2.5f,1.5f));
                world.Spawn(new WaveSpec{Health=5000,Speed=2,Flying=true},tower.Center+new V2(-1.5f,2));
                game.Paused=false;yield return new WaitForSecondsRealtime(1.5f);game.Paused=true;yield return Capture(map+"-Paid-Defense");
                camera.Overview();camera.Focus.x+=4;game.ResetView();
                foreach(float z in new[]{0f,(float)world.Config.Height})
                    if(Mathf.Abs(game.View.WorldToViewportPoint(new Vector3(world.Config.Width*.5f,0,z)).x-.5f)>.00001f){Fail("Camera reset is off center");yield break;}
                yield return Capture(map+"-Overview");
                Debug.Log("HOWL_CAMERA_CENTER_PASS "+map+" north=true centerline=true");
                Debug.Log("HOWL_WORLD_CHECK_PASS "+map+" refugeVertices="+total+" paidTowers="+built+" spent="+spent+" maskUnchanged=true mirrored=true");
            }
            Debug.Log("HOWL_WORLD_CHECK_COMPLETE");Application.Quit(0);
        }
        IEnumerator Capture(string name) {yield return new WaitForSecondsRealtime(.3f);yield return new WaitForEndOfFrame();var texture=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(Path.Combine(output,name+".png"),texture.EncodeToPNG());Destroy(texture);}
        void Fail(string why){Debug.LogError("HOWL_WORLD_CHECK_FAIL "+why);Application.Quit(1);}
    }
}
