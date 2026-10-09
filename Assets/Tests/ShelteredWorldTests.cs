#if UNITY_EDITOR
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEditor.SceneManagement;
using FrostMaze.Simulation;
namespace FrostMaze.Tests
{
    public sealed class ShelteredWorldTests
    {
        [UnityTest]
        public IEnumerator ExteriorRespectsEveryBuildingAndBakedMapsRemainValid() {
            EditorSceneManager.OpenScene("Assets/Game/Maps/MazeLab.unity");yield return new EnterPlayMode();yield return null;
            foreach(var name in new[]{"Ironfold","Rimewatch","Ironfold"}) {
                var game=Object.FindFirstObjectByType<Prototype>();game.OpenSetup();game.ChooseMap(Resources.Load<MapDefinition>(name));yield return null;yield return null;game=Object.FindFirstObjectByType<Prototype>();
                var scenery=game.GetComponentInChildren<MapScenery>();Assert.That(scenery.UsesBakedSurfaces,Is.True,"Shipped maps must not repaint on switching");
                var set=WorldSurfaceSet.Find(game.World.Config);Assert.That(set,Is.Not.Null);Assert.That(set.Exterior.width,Is.EqualTo(1024));
                var backdrop=game.GetComponentInChildren<WorldBackdrop>();Assert.That(backdrop.GetComponentsInChildren<Collider>(),Is.Empty);
                foreach(var filter in backdrop.GetComponentsInChildren<MeshFilter>()) {
                    if(filter.name!="Distant ridges"&&!filter.name.StartsWith("Sheltered"))continue;
                    var vertices=filter.sharedMesh.vertices;var triangles=filter.sharedMesh.triangles;
                    for(int i=0;i<triangles.Length;i+=3) {
                        var a=vertices[triangles[i]];var b=vertices[triangles[i+1]];var c=vertices[triangles[i+2]];
                        float loX=Mathf.Min(a.x,b.x,c.x),hiX=Mathf.Max(a.x,b.x,c.x),loZ=Mathf.Min(a.z,b.z,c.z),hiZ=Mathf.Max(a.z,b.z,c.z);
                        foreach(var rect in backdrop.SettlementBounds)for(int side=0;side<2;side++) {
                            float left=side==0?rect.xMin:game.World.Config.Width-rect.xMax,right=side==0?rect.xMax:game.World.Config.Width-rect.xMin;
                            Assert.That(hiX<left||loX>right||hiZ<rect.yMin||loZ>rect.yMax,Is.True,"Scattered geometry penetrates a building envelope: "+filter.name);
                        }
                    }
                }
                var modified=JsonUtility.FromJson<Scenario>(JsonUtility.ToJson(game.World.Config));modified.LayoutRows[0]="?"+modified.LayoutRows[0].Substring(1);
                Assert.That(WorldSurfaceSet.Find(modified),Is.Null,"Changed maps must reject stale paint");
            }
            yield return new ExitPlayMode();
        }
        sealed class Pan : ICameraInput {public CameraIntent Read()=>new CameraIntent{Pan=Vector2.one,Rotate=1,Zoom=1};}
        [UnityTest]
        public IEnumerator ChatCapturesCameraAndTerminalResultsCanBeReviewed() {
            EditorSceneManager.OpenScene("Assets/Game/Maps/MazeLab.unity");yield return new EnterPlayMode();yield return null;
            var game=Object.FindFirstObjectByType<Prototype>();var online=OnlineGame.Create();online.Host(game.Map.name,"Host","",0);yield return null;
            var net=online.Session;net.SendChat("Protect the hearth.");game.OpenChat();Assert.That(game.ChatCapturesInput,Is.True);
            var camera=game.View.GetComponent<RtsCamera>();camera.SetInput(new Pan());var focus=camera.Focus;float yaw=camera.Yaw,zoom=camera.Zoom;yield return null;yield return null;
            Assert.That(camera.Focus,Is.EqualTo(focus));Assert.That(camera.Yaw,Is.EqualTo(yaw));Assert.That(camera.Zoom,Is.EqualTo(zoom));
            Assert.That(game.PointerOverHud(game.ChatTriggerRect.center),Is.True,"Chat button must not place a tower through the HUD");
            game.CloseChat();game.LeaveOnline();yield return null;game.StartMatch();game.Paused=true;
            Assert.That(game.ResultOpen,Is.False);var w=game.World;var route=w.LaneRoute(0,true);
            for(int i=0;i<w.Config.StartingLives;i++){var enemy=w.Spawn(new WaveSpec{Flying=true},route[route.Length-1]);enemy.Checkpoint=route.Length-1;w.Step();}
            w.Step();yield return null;Assert.That(w.Lives,Is.Zero);Assert.That(w.Finished,Is.True);Assert.That(game.ResultOpen,Is.True);
            game.InspectResult();Assert.That(game.ResultOpen,Is.False);game.ShowResult();Assert.That(game.ResultOpen,Is.True);
            game.StartMatch();Assert.That(game.ResultOpen,Is.False,"New match retained the result modal");
            yield return new ExitPlayMode();
        }
    }
}
#endif
