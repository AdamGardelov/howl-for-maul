using UnityEngine;
namespace FrostMaze
{
    public sealed class RtsCamera : MonoBehaviour
    {
        public float PanSpeed = 28, ZoomSpeed = 2, MinZoom = 5, MaxZoom = 25;
        public Vector2 BoundsMin, BoundsMax = new Vector2(30, 20);
        public Vector3 Focus;
        public float Yaw { get; private set; }
        public void ResetRotation(){Yaw=0;Apply();}
        Vector3 GroundRight => Quaternion.Euler(0,Yaw,0)*Vector3.right;
        Vector3 GroundUp => Quaternion.Euler(0,Yaw,0)*Vector3.forward;
        ICameraInput source = new DesktopInput();
        Camera view;
        Prototype game;
        bool dragAllowed;
        public void Initialize(float width, float height)
        {
            view = GetComponent<Camera>();
            game=FindFirstObjectByType<Prototype>();
            BoundsMax = new Vector2(width, height);
            Focus = new Vector3(width / 2, 0, height / 2);
            view.orthographic = true;
            Overview();
            MaxZoom=Mathf.Max(MaxZoom,view.orthographicSize*1.5f);
            Apply();
        }
        public void FocusPoint(FrostMaze.Simulation.V2 point)
        {
            Focus=new Vector3(point.X,0,point.Y);
            view.orthographicSize=11;
            Apply();
        }
        public void Overview()
        {
            Focus=new Vector3(BoundsMax.x*.5f,0,BoundsMax.y*.5f);
            float top=game!=null&&!game.SetupOpen?game.TopHud.yMax+8:0;
            float bottom=game!=null&&!game.SetupOpen?(game.World.Grid.Find(game.SelectedTowerId)!=null?Screen.height-game.SelectionHud.yMin+8:24*game.UiScale):0;
            float fraction=Mathf.Max(.2f,(Screen.height-top-bottom)/Mathf.Max(1,Screen.height));
            float radians=Yaw*Mathf.Deg2Rad,c=Mathf.Abs(Mathf.Cos(radians)),s=Mathf.Abs(Mathf.Sin(radians));
            float across=BoundsMax.x*c+BoundsMax.y*s,up=BoundsMax.x*s+BoundsMax.y*c;
            view.orthographicSize=Mathf.Max(up*Mathf.Sin(55*Mathf.Deg2Rad)*.55f/fraction,across*.55f/Mathf.Max(.1f,view.aspect));
            Focus-=GroundUp*((bottom-top)*view.orthographicSize/Mathf.Max(1,Screen.height)/Mathf.Sin(55*Mathf.Deg2Rad));
            MaxZoom=Mathf.Max(MaxZoom,view.orthographicSize);
            Apply();
        }
        public void SetInput(ICameraInput input)
        {
            source = input;
        }
        void Update()
        {
            if (view == null)
                return;
            var intent = source.Read();
            var mouse=intent.Pointer;
            var uiPoint=new Vector2(mouse.x,Screen.height-mouse.y);
            bool overUi=game!=null&&game.PointerOverHud(uiPoint);
            if(!intent.Dragging)dragAllowed=false;
            if(intent.DragStarted)dragAllowed=!overUi;
            if(overUi)intent.Zoom=0;
            if(!dragAllowed||overUi)intent.Drag=Vector2.zero;
            if(game!=null&&(game.SetupOpen||game.MenuOpen)){dragAllowed=false;return;}
            Yaw=Mathf.Repeat(Yaw+intent.Rotate*55*Time.unscaledDeltaTime,360);
            var pan = Vector2.ClampMagnitude(intent.Pan, 1);
            Focus += (GroundRight*pan.x+GroundUp*pan.y) * PanSpeed * Mathf.Clamp(view.orthographicSize/11,.5f,2.5f) * (intent.Fast?2:1) * Time.unscaledDeltaTime;
            float pixelScale = view.orthographicSize * 2 / Mathf.Max(1, Screen.height);
            Focus += (GroundRight*intent.Drag.x+GroundUp*(intent.Drag.y / Mathf.Sin(55 * Mathf.Deg2Rad))) * pixelScale;
            Focus.x = Mathf.Clamp(Focus.x, BoundsMin.x, BoundsMax.x);
            Focus.z = Mathf.Clamp(Focus.z, BoundsMin.y, BoundsMax.y);
            view.orthographicSize = Mathf.Clamp(view.orthographicSize - intent.Zoom * ZoomSpeed, MinZoom, MaxZoom);
            Apply();
        }
        void Apply()
        {
            transform.rotation = Quaternion.Euler(55, Yaw, 0);
            transform.position = Focus - transform.forward * 40;
        }
    }
}
