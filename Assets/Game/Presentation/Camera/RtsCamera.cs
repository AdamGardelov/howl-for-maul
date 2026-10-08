using UnityEngine;
namespace FrostMaze
{
    public sealed class RtsCamera : MonoBehaviour
    {
        public float PanSpeed = 28, ZoomSpeed = 2, MinZoom = 5, MaxZoom = 25;
        public Vector2 BoundsMin, BoundsMax = new Vector2(30, 20);
        public Vector3 Focus;
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
            view.orthographicSize=Mathf.Max(BoundsMax.y*Mathf.Sin(55*Mathf.Deg2Rad)*.55f,BoundsMax.x*.55f/Mathf.Max(.1f,view.aspect));
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
            bool overUi=game!=null&&(game.Sidebar.Contains(uiPoint)||game.MinimapRect.Contains(uiPoint));
            if(!intent.Dragging)dragAllowed=false;
            if(intent.DragStarted)dragAllowed=!overUi;
            if(overUi)intent.Zoom=0;
            if(!dragAllowed||overUi)intent.Drag=Vector2.zero;
            if(game!=null&&game.SetupOpen){dragAllowed=false;return;}
            var pan = Vector2.ClampMagnitude(intent.Pan, 1);
            Focus += new Vector3(pan.x, 0, pan.y) * PanSpeed * Mathf.Clamp(view.orthographicSize/11,.5f,2.5f) * (intent.Fast?2:1) * Time.unscaledDeltaTime;
            float pixelScale = view.orthographicSize * 2 / Mathf.Max(1, Screen.height);
            Focus += new Vector3(intent.Drag.x, 0, intent.Drag.y / Mathf.Sin(55 * Mathf.Deg2Rad)) * pixelScale;
            Focus.x = Mathf.Clamp(Focus.x, BoundsMin.x, BoundsMax.x);
            Focus.z = Mathf.Clamp(Focus.z, BoundsMin.y, BoundsMax.y);
            view.orthographicSize = Mathf.Clamp(view.orthographicSize - intent.Zoom * ZoomSpeed, MinZoom, MaxZoom);
            Apply();
        }
        void Apply()
        {
            transform.rotation = Quaternion.Euler(55, 0, 0);
            transform.position = Focus - transform.forward * 40;
        }
    }
}
