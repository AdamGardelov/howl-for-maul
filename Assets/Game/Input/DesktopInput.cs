using UnityEngine;
namespace FrostMaze
{
    public struct CameraIntent
    {
        public Vector2 Pan, Drag, Pointer; public float Zoom;
        public bool Dragging, DragStarted, Fast;
    }
    public interface ICameraInput { CameraIntent Read(); }
    // Input intent boundary is independent of the camera motor; sampled gestures are testable.
    public sealed class DesktopInput : ICameraInput
    {
        Vector2 previous; bool wasDragging;
        public CameraIntent Read()
        {
            return ReadSample(UnityEngine.Input.mousePosition,new Vector2(Screen.width,Screen.height),
                new Vector2((Held(KeyCode.D,KeyCode.RightArrow)?1:0)-(Held(KeyCode.A,KeyCode.LeftArrow)?1:0),(Held(KeyCode.W,KeyCode.UpArrow)?1:0)-(Held(KeyCode.S,KeyCode.DownArrow)?1:0)),
                UnityEngine.Input.mouseScrollDelta.y,Application.isFocused,UnityEngine.Input.GetMouseButton(2),UnityEngine.Input.GetMouseButton(0),UnityEngine.Input.GetKey(KeyCode.Space),Held(KeyCode.LeftShift,KeyCode.RightShift));
        }
        public CameraIntent ReadSample(Vector2 mouse,Vector2 screen,Vector2 keyboard,float zoom,bool focused,bool middle,bool left,bool space,bool fast)
        {
            bool inside=mouse.x>=0&&mouse.y>=0&&mouse.x<screen.x&&mouse.y<screen.y;
            bool dragging=focused&&inside&&(middle||(space&&left));
            var intent=new CameraIntent{Pointer=mouse};
            if(focused) {
                intent.Pan=keyboard;
                if(inside&&!dragging&&!left)intent.Pan+=EdgePan(mouse,screen);
                intent.Zoom=inside?zoom:0;intent.Fast=fast;
                intent.Dragging=dragging;intent.DragStarted=dragging&&!wasDragging;
                intent.Drag=dragging&&wasDragging?previous-mouse:Vector2.zero;
            }
            previous=mouse;wasDragging=dragging;return intent;
        }
        public static Vector2 EdgePan(Vector2 mouse,Vector2 screen)
        {
            const float margin=16;
            if(mouse.x<0||mouse.y<0||mouse.x>=screen.x||mouse.y>=screen.y)return Vector2.zero;
            return new Vector2(mouse.x<margin?-1:mouse.x>=screen.x-margin?1:0,mouse.y<margin?-1:mouse.y>=screen.y-margin?1:0);
        }
        static bool Held(KeyCode a,KeyCode b)=>UnityEngine.Input.GetKey(a)||UnityEngine.Input.GetKey(b);
    }
}
