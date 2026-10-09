#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using FrostMaze.Simulation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEditor.SceneManagement;
using Object=UnityEngine.Object;
namespace FrostMaze.Tests
{
    public sealed class PreviewPresentationTests
    {
        [UnityTest]
        public IEnumerator ConstructionCopiesStayCosmeticAndReuseTheirResources()
        {
            EditorSceneManager.OpenScene("Assets/Game/Maps/MazeLab.unity");yield return new EnterPlayMode();yield return null;
            foreach(string map in new[]{"Ironfold","Rimewatch"}) {
                var game=Object.FindFirstObjectByType<Prototype>();
                if(game.Map.name!=map){game.ChooseMap(Resources.Load<MapDefinition>(map));yield return null;yield return null;game=Object.FindFirstObjectByType<Prototype>();}
                game.StartMatch();game.Paused=true;game.enabled=false;
                var world=game.World;var preview=game.GetComponentInChildren<BuildPlacementPreview>();preview.enabled=false;
                int gold=world.Gold;long tick=world.Tick;int shots=world.Shots.Count;
                var origin=world.SnapBuildOrigin(world.BuilderPosition);bool found=false;
                for(int y=-3;y<4&&!found;y++)for(int x=-3;x<4&&!found;x++){
                    var at=world.SnapBuildOrigin(world.BuilderPosition+new V2(x,y));
                    if(world.CanBuild(at.X,at.Y,out _)){origin=at;found=true;}
                }
                Assert.That(found,Is.True);
                Material ownedPreview=null;
                for(int design=0;design<world.Config.Catalog.Length;design++) {
                    preview.Show(design,origin,true);var root=preview.ActiveModel;
                    Assert.That(root.GetComponent<TowerView>().Subject.Id,Is.Zero);
                    Assert.That(root.transform.position.x,Is.EqualTo(origin.X+world.Config.Catalog[design].Spec.Width*.5f));
                    foreach(var collider in root.GetComponentsInChildren<Collider>())Assert.That(collider.enabled,Is.False);
                    foreach(var r in root.GetComponentsInChildren<MeshRenderer>())if(r.enabled){
                        Assert.That(r.sharedMaterial.shader.name,Is.EqualTo("Howl/Model preview"));ownedPreview=r.sharedMaterial;
                        Assert.That(r.shadowCastingMode,Is.EqualTo(UnityEngine.Rendering.ShadowCastingMode.Off));
                    }
                    var current=root;preview.Show(design,origin,false);Assert.That(preview.ActiveModel,Is.SameAs(current));
                    Assert.That(ownedPreview.GetColor("_Tint").r,Is.GreaterThan(.9f));
                }
                preview.Show(0,origin,true);var cached=preview.ActiveModel;
                for(int i=0;i<16;i++)preview.Show(0,origin,true);
                long before=GC.GetAllocatedBytesForCurrentThread();
                for(int i=0;i<300;i++)preview.Show(0,origin+new V2(i%2*.5f,0),true);
                Assert.That(GC.GetAllocatedBytesForCurrentThread()-before,Is.Zero,"Steady placement allocates");
                Assert.That(preview.ActiveModel,Is.SameAs(cached));
                Assert.That(world.CanBuild(origin.X,origin.Y,out _),Is.True,"Preview blocks real construction");
                Assert.That(world.Gold,Is.EqualTo(gold));Assert.That(world.Grid.Towers.Count,Is.Zero);
                Assert.That(world.Tick,Is.EqualTo(tick));Assert.That(world.Shots.Count,Is.EqualTo(shots));Assert.That(world.QueuedBuilds,Is.Zero);
                game.HasHover=true;game.Hover=origin;game.HoverBuildValid=true;world.SelectedDesign=0;preview.Refresh();Assert.That(preview.ActiveModel,Is.Not.Null);
                game.MenuOpen=true;preview.Refresh();Assert.That(preview.ActiveModel,Is.Null);game.MenuOpen=false;
                game.SetupOpen=true;preview.Refresh();Assert.That(preview.ActiveModel,Is.Null);game.SetupOpen=false;
                game.MoveMode=true;preview.Refresh();Assert.That(preview.ActiveModel,Is.Null);game.MoveMode=false;
                game.SellMode=true;preview.Refresh();Assert.That(preview.ActiveModel,Is.Null);game.SellMode=false;
                game.HasHover=false;preview.Refresh();Assert.That(preview.ActiveModel,Is.Null);game.HasHover=true;
                Assert.That(world.OrderBuild(origin.X,origin.Y,out _),Is.True);
                for(int i=0;i<500&&world.QueuedBuilds>0;i++)world.Step();
                Assert.That(world.Grid.Towers.Count,Is.EqualTo(1));Assert.That(world.Gold,Is.EqualTo(gold-world.BuildCost));
                preview.Refresh();Assert.That(preview.ActiveModel,Is.Null,"Inspection must hide the construction copy");
                Object.Destroy(preview.gameObject);yield return null;
                Assert.That(ownedPreview==null,Is.True,"Preview-owned material leaked");
                foreach(var palette in game.TowerPalette(0))Assert.That(palette.shader.name,Is.Not.EqualTo("Howl/Model preview"));
            }
            yield return new ExitPlayMode();
        }
        [UnityTest]
        public IEnumerator PortraitsStayReadableWithoutChangingWorldLighting()
        {
            EditorSceneManager.OpenScene("Assets/Game/Maps/MazeLab.unity");yield return new EnterPlayMode();yield return null;
            string output=Path.Combine(Application.dataPath,"../Logs/PreviewPresentation");Directory.CreateDirectory(output);
            foreach(string map in new[]{"Ironfold","Rimewatch"}) {
                var game=Object.FindFirstObjectByType<Prototype>();
                if(game.Map.name!=map){game.ChooseMap(Resources.Load<MapDefinition>(map));yield return null;yield return null;game=Object.FindFirstObjectByType<Prototype>();}
                game.StartMatch();game.Paused=true;
                var ambient=RenderSettings.ambientSkyColor;var palette=game.TowerPalette(0);var bodyColor=palette[0].color;var bodyShader=palette[0].shader;
                int count=game.World.Config.Catalog.Length,columns=map=="Ironfold"?7:5,rows=(count+columns-1)/columns;
                var sheet=new Texture2D(columns*160,rows*160,TextureFormat.RGB24,false);
                var portraits=new TowerPortraits();var target=RenderTexture.GetTemporary(160,160,0);var previous=RenderTexture.active;
                var readback=new Texture2D(160,160,TextureFormat.RGB24,false);
                try {
                    for(int i=0;i<count;i++) {
                        portraits.Prepare(game,i);var image=portraits.Get(i);Assert.That(image,Is.Not.Null);
                        Graphics.Blit(image,target);RenderTexture.active=target;readback.ReadPixels(new Rect(0,0,160,160),0,0);readback.Apply();
                        var pixels=readback.GetPixels();int readable=0,magenta=0;
                        foreach(var pixel in pixels){if(pixel.grayscale>.22f)readable++;if(pixel.r>.9f&&pixel.b>.9f&&pixel.g<.05f)magenta++;}
                        Assert.That(readable,Is.GreaterThan(450),map+" portrait is too dark: "+i);Assert.That(magenta,Is.Zero,"Missing shader");
                        sheet.SetPixels(i%columns*160,(rows-1-i/columns)*160,160,160,pixels);
                        portraits.Prepare(game,i);Assert.That(portraits.Get(i),Is.SameAs(image));yield return null;
                    }
                    sheet.Apply();File.WriteAllBytes(Path.Combine(output,map+"-Portraits.png"),sheet.EncodeToPNG());
                    Assert.That(RenderSettings.ambientSkyColor,Is.EqualTo(ambient));Assert.That(palette[0].color,Is.EqualTo(bodyColor));Assert.That(palette[0].shader,Is.SameAs(bodyShader));
                    Assert.That(game.World.Grid.Towers.Count,Is.Zero);Assert.That(game.World.QueuedBuilds,Is.Zero);
                } finally {RenderTexture.active=previous;RenderTexture.ReleaseTemporary(target);portraits.Dispose();Object.Destroy(sheet);Object.Destroy(readback);}
                yield return null;
                Assert.That(GameObject.Find("Tower portrait preview"),Is.Null);Assert.That(GameObject.Find("Tower portrait camera"),Is.Null);
            }
            yield return new ExitPlayMode();
        }
    }
}
#endif
