using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using FrostMaze.Simulation;
namespace FrostMaze
{
    // One cached image per design, rendered from the same geometry as placed towers.
    // Temporary preview objects never enter the simulation or the playable map.
    public sealed class TowerPortraits : System.IDisposable
    {
        readonly Dictionary<int,Texture2D> cache=new Dictionary<int,Texture2D>();
        public Texture2D Get(int design)=>cache.TryGetValue(design,out var image)?image:null;
        public void Prepare(Prototype game,int design)
        {
            if(cache.ContainsKey(design))return;
            var definition=game.World.Config.Catalog[design];int faction=0;
            for(int i=0;i<game.World.Config.Factions.Length;i++)
                if(System.Array.IndexOf(game.World.Config.Factions[i].Designs,design)>=0)faction=i;
            var root=new GameObject("Tower portrait preview");
            var cameraObject=new GameObject("Tower portrait camera");
            var target=RenderTexture.GetTemporary(160,160,24,RenderTextureFormat.ARGB32);
            var previous=RenderTexture.active;
            Texture2D image=null;
            try {
                var tower=new Tower{Design=design,Spec=definition.Spec,Health=definition.Spec.Health,Name=definition.Name};
                root.AddComponent<TowerView>().Initialize(game,tower,definition,faction);
                root.transform.position=new Vector3(10000,10000,10000);
                foreach(var t in root.GetComponentsInChildren<Transform>(true))t.gameObject.layer=31;
                var bounds=new Bounds(root.transform.position,Vector3.zero);
                foreach(var renderer in root.GetComponentsInChildren<Renderer>())if(renderer.enabled)bounds.Encapsulate(renderer.bounds);
                var camera=cameraObject.AddComponent<Camera>();camera.enabled=false;
                camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.065f,.105f,.12f,1);
                camera.cullingMask=1<<31;camera.orthographic=true;camera.aspect=1;
                camera.nearClipPlane=.01f;camera.farClipPlane=30;camera.allowHDR=false;camera.allowMSAA=false;
                camera.transform.rotation=Quaternion.Euler(24,155,0);
                camera.transform.position=bounds.center-camera.transform.forward*8;
                float extent=0;
                for(int x=-1;x<=1;x+=2)for(int y=-1;y<=1;y+=2)for(int z=-1;z<=1;z+=2) {
                    var corner=Vector3.Scale(bounds.extents,new Vector3(x,y,z));
                    extent=Mathf.Max(extent,Mathf.Abs(Vector3.Dot(corner,camera.transform.right)),Mathf.Abs(Vector3.Dot(corner,camera.transform.up)));
                }
                camera.orthographicSize=Mathf.Max(.5f,extent*1.08f);
                camera.targetTexture=target;
                if(GraphicsSettings.currentRenderPipeline==null)camera.Render();
                else {
                    var request=new RenderPipeline.StandardRequest{destination=target};
                    if(!RenderPipeline.SupportsRenderRequest(camera,request))return;
                    RenderPipeline.SubmitRenderRequest(camera,request);
                }
                RenderTexture.active=target;
                image=new Texture2D(160,160,TextureFormat.RGB24,false){name="Tower portrait "+design,filterMode=FilterMode.Bilinear};
                image.ReadPixels(new Rect(0,0,160,160),0,0);image.Apply(false,true);
                cache.Add(design,image);image=null;
            } finally {
                RenderTexture.active=previous;
                var previewCamera=cameraObject.GetComponent<Camera>();if(previewCamera!=null)previewCamera.targetTexture=null;
                root.SetActive(false);cameraObject.SetActive(false);
                Object.Destroy(root);Object.Destroy(cameraObject);RenderTexture.ReleaseTemporary(target);
                if(image!=null)Object.Destroy(image);
            }
        }
        public void Dispose(){foreach(var image in cache.Values)if(image!=null)Object.Destroy(image);cache.Clear();}
    }
}
