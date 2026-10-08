using UnityEngine;
using UnityEngine.Rendering;
namespace FrostMaze
{
    // Cached north-up scenery render. Static scenery uses layer 30; units and HUD do not.
    public sealed class RenderedMinimap : System.IDisposable
    {
        public Texture2D Texture {get;private set;}
        public void Prepare(Prototype game)
        {
            if(Texture!=null)return;
            var root=new GameObject("Minimap snapshot camera");var camera=root.AddComponent<Camera>();camera.enabled=false;
            var target=RenderTexture.GetTemporary(512,512,24,RenderTextureFormat.ARGB32);var previous=RenderTexture.active;
            try {
                camera.orthographic=true;camera.orthographicSize=game.World.Config.Height*.5f;
                camera.aspect=game.World.Config.Width/(float)game.World.Config.Height;
                camera.transform.SetPositionAndRotation(new Vector3(game.World.Config.Width*.5f,80,game.World.Config.Height*.5f),Quaternion.Euler(90,0,0));
                camera.nearClipPlane=.1f;camera.farClipPlane=120;camera.cullingMask=1<<30;
                camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.025f,.04f,.047f);
                camera.allowHDR=false;camera.allowMSAA=false;camera.targetTexture=target;
                if(GraphicsSettings.currentRenderPipeline==null)camera.Render();
                else {var request=new RenderPipeline.StandardRequest{destination=target};if(!RenderPipeline.SupportsRenderRequest(camera,request))return;RenderPipeline.SubmitRenderRequest(camera,request);}
                RenderTexture.active=target;
                Texture=new Texture2D(512,512,TextureFormat.RGB24,false){name="North-up map scenery",filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};
                Texture.ReadPixels(new Rect(0,0,512,512),0,0);Texture.Apply(false,false);
            } finally {
                RenderTexture.active=previous;camera.targetTexture=null;root.SetActive(false);Object.Destroy(root);RenderTexture.ReleaseTemporary(target);
            }
        }
        public void Dispose(){if(Texture!=null)Object.Destroy(Texture);Texture=null;}
    }
}
