using UnityEngine;
namespace FrostMaze
{
    public sealed class RtsCamera : MonoBehaviour
    {
        public float PanSpeed = 28, ZoomSpeed = 2, MinZoom = 5, MaxZoom = 25;
        public Vector2 BoundsMin, BoundsMax = new Vector2(30, 20);
        public Vector3 Focus;
        public float Yaw { get; private set; }
        public float Pitch { get; private set; }
        // Retain orthographicSize as the zoom scale for saved fixtures and inspector tooling.
        // It is the half-height at the focus plane; the actual rendering uses perspective.
        public float Zoom => view.orthographicSize;
        float targetZoom,lastZoom;
        public void ResetRotation(){Yaw=0;Apply();}
        public void CenterNorth()
        {
            // An off-axis perspective view still makes the map's spine lean after yaw resets.
            // Return to its symmetry plane without pulling the player out of the current area.
            Focus.x=(BoundsMin.x+BoundsMax.x)*.5f;
            ResetRotation();
        }
        Vector3 GroundRight => Quaternion.Euler(0,Yaw,0)*Vector3.right;
        Vector3 GroundUp => Quaternion.Euler(0,Yaw,0)*Vector3.forward;
        ICameraInput source = new DesktopInput();
        Camera view;
        Prototype game;
        bool dragAllowed,previewShown,titleShown;
        int previewWidth,previewHeight;
        bool SetupPreview => game!=null&&game.SetupOpen&&!game.LobbyOpen&&!game.MainMenuOpen;
        void LateUpdate()
        {
            if(view==null)return;
            if(game.SetupOpen&&game.MainMenuOpen&&!game.LobbyOpen){
                var focus=Focus;float yaw=Yaw,zoom=Zoom,last=lastZoom,pitch=Pitch;
                Focus=new Vector3(BoundsMax.x*.36f,0,-1.5f);
                Yaw=-16+Mathf.Sin(Time.unscaledTime*.035f)*2;view.orthographicSize=10;Apply();
                Focus=focus;Yaw=yaw;view.orthographicSize=zoom;lastZoom=last;Pitch=pitch;
                titleShown=true;previewShown=false;return;
            }
            if(titleShown){titleShown=false;Apply();}
            if(SetupPreview){
                if(!previewShown||previewWidth!=Screen.width||previewHeight!=Screen.height){
                    // Preview is a temporary rendered view; returning to play keeps the player's camera.
                    var focus=Focus;float yaw=Yaw,zoom=Zoom,target=targetZoom,last=lastZoom,pitch=Pitch,max=MaxZoom;
                    Yaw=0;Overview();
                    Focus=focus;Yaw=yaw;view.orthographicSize=zoom;targetZoom=target;lastZoom=last;Pitch=pitch;MaxZoom=max;
                    previewWidth=Screen.width;previewHeight=Screen.height;previewShown=true;
                }
            }else if(previewShown){previewShown=false;Apply();}
        }
        public void Initialize(float width, float height)
        {
            view = GetComponent<Camera>();
            game=FindFirstObjectByType<Prototype>();
            BoundsMax = new Vector2(width, height);
            Focus = new Vector3(width / 2, 0, height / 2);
            view.orthographic = false;
            Overview();
            MaxZoom=Mathf.Max(MaxZoom,Zoom*1.5f);
        }
        public void SetZoom(float scale,bool immediate=false)
        {
            targetZoom=Mathf.Clamp(scale,MinZoom,MaxZoom);
            if(immediate){view.orthographicSize=targetZoom;Apply();}
        }
        public void FocusPoint(FrostMaze.Simulation.V2 point)
        {
            Focus=new Vector3(point.X,0,point.Y);
            SetZoom(11,true);
        }
        public void Overview()
        {
            Focus=new Vector3(BoundsMax.x*.5f,0,BoundsMax.y*.5f);
            float top=game!=null&&!game.SetupOpen?game.TopHud.yMax+8:8;
            float bottom=game!=null&&!game.SetupOpen?(game.World.Grid.Find(game.SelectedTowerId)!=null?Screen.height-game.SelectionHud.yMin+8:24*game.UiScale):8;
            float low=bottom/Mathf.Max(1,Screen.height),high=1-top/Mathf.Max(1,Screen.height);
            float left=.035f,right=.965f;
            if(SetupPreview){
                float pad=24*game.UiScale;
                left=(game.Sidebar.xMax+pad)/Mathf.Max(1,Screen.width);
                right=1-pad/Mathf.Max(1,Screen.width);
                low=pad/Mathf.Max(1,Screen.height);high=1-low;
            }
            // Fit the actual projected corners, including perspective foreshortening and yaw.
            view.orthographicSize=Mathf.Max(18,Mathf.Max(BoundsMax.x/Mathf.Max(.1f,view.aspect),BoundsMax.y)*.45f);
            for(int i=0;i<60;i++) {
                Apply();
                if(SetupPreview){
                    // Center the projected silhouette, not merely the map's ground-space midpoint.
                    var min=new Vector2(float.PositiveInfinity,float.PositiveInfinity);var max=new Vector2(float.NegativeInfinity,float.NegativeInfinity);
                    for(int x=0;x<2;x++)for(int z=0;z<2;z++){
                        Vector2 p=view.WorldToViewportPoint(new Vector3(x*BoundsMax.x,0,z*BoundsMax.y));min=Vector2.Min(min,p);max=Vector2.Max(max,p);
                    }
                    var plane=new Plane(Vector3.up,Vector3.zero);
                    var from=view.ViewportPointToRay((min+max)*.5f);var to=view.ViewportPointToRay(new Vector3((left+right)*.5f,(low+high)*.5f,0));
                    if(plane.Raycast(from,out float a)&&plane.Raycast(to,out float b)){Focus+=from.GetPoint(a)-to.GetPoint(b);Apply();}
                }
                bool fits=true;
                for(int x=0;x<2;x++)for(int z=0;z<2;z++) {
                    var p=view.WorldToViewportPoint(new Vector3(x*BoundsMax.x,0,z*BoundsMax.y));
                    if(p.z<=0||p.x<left||p.x>right||p.y<low||p.y>high)fits=false;
                }
                if(fits)break;
                view.orthographicSize*=1.035f;
            }
            MaxZoom=Mathf.Max(MaxZoom,Zoom);targetZoom=Zoom;Apply();
        }
        public void SetInput(ICameraInput input){source=input;}
        public bool GroundPoint(Vector2 screen,out Vector3 point)
        {
            var ray=view.ScreenPointToRay(screen);
            if(new Plane(Vector3.up,Vector3.zero).Raycast(ray,out float distance)){point=ray.GetPoint(distance);return true;}
            point=default;return false;
        }
        void Update()
        {
            if(view==null)return;
            var intent=source.Read();var mouse=intent.Pointer;
            bool overUi=game!=null&&game.PointerOverHud(new Vector2(mouse.x,Screen.height-mouse.y));
            if(!intent.Dragging)dragAllowed=false;
            if(intent.DragStarted)dragAllowed=!overUi;
            if(overUi)intent.Zoom=0;
            if(!dragAllowed||overUi)intent.Drag=Vector2.zero;
            if(game!=null&&(game.SetupOpen||game.MenuOpen||game.ChatCapturesInput||game.ResultOpen)){dragAllowed=false;return;}
            if(!Mathf.Approximately(Zoom,lastZoom))targetZoom=Zoom;
            Yaw=Mathf.Repeat(Yaw+intent.Rotate*55*Time.unscaledDeltaTime,360);
            var pan=Vector2.ClampMagnitude(intent.Pan,1);
            Focus+=(GroundRight*pan.x+GroundUp*pan.y)*PanSpeed*Mathf.Clamp(Zoom/11,.5f,2.5f)*(intent.Fast?2:1)*Time.unscaledDeltaTime;
            Apply();
            if(intent.Drag!=Vector2.zero) {
                // Ground intersections keep the grabbed point under the cursor at any pitch/yaw.
                // Clamp synthetic/out-of-window samples so a ray can never cross the horizon.
                var previous=new Vector2(Mathf.Clamp(mouse.x+intent.Drag.x,0,Screen.width),Mathf.Clamp(mouse.y+intent.Drag.y,0,Screen.height));
                if(GroundPoint(previous,out var from)&&GroundPoint(mouse,out var to))Focus+=from-to;
            }
            Focus.x=Mathf.Clamp(Focus.x,BoundsMin.x,BoundsMax.x);
            Focus.z=Mathf.Clamp(Focus.z,BoundsMin.y,BoundsMax.y);
            if(intent.Zoom!=0)targetZoom=Mathf.Clamp(targetZoom-intent.Zoom*ZoomSpeed,MinZoom,MaxZoom);
            view.orthographicSize=Mathf.Lerp(Zoom,targetZoom,1-Mathf.Exp(-14*Time.unscaledDeltaTime));
            if(Mathf.Abs(Zoom-targetZoom)<.001f)view.orthographicSize=targetZoom;
            Apply();
        }
        void Apply()
        {
            float close=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(MinZoom,24,Zoom));
            Pitch=Mathf.Lerp(62,38,close);
            view.fieldOfView=Mathf.Lerp(16,46,close);
            float distance=Zoom/Mathf.Tan(view.fieldOfView*.5f*Mathf.Deg2Rad);
            // Keep the depth range proportional at overview distances for precise placement rays.
            view.nearClipPlane=Mathf.Max(.1f,distance*.02f);view.farClipPlane=distance+Mathf.Max(BoundsMax.x,BoundsMax.y)*2+20;
            transform.rotation=Quaternion.Euler(Pitch,Yaw,0);
            transform.position=Focus-transform.forward*distance;
            lastZoom=Zoom;
        }
    }
}
