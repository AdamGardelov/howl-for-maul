using UnityEngine;
namespace FrostMaze
{
    public struct CameraIntent
    {
        public Vector2 Pan, Drag; public float Zoom;
    }
    public interface ICameraInput
    {
        CameraIntent Read();
    }
    // Input intent boundary is deliberately independent of the camera motor. Touch can supply the same intent later.
    public sealed class DesktopInput : ICameraInput
    {
        Vector3 previous;
        public CameraIntent Read()
        {
            var mouse = UnityEngine.Input.mousePosition;
            var intent = new CameraIntent
            {
                Pan = new Vector2((Held(KeyCode.D, KeyCode.RightArrow) ? 1 : 0) - (Held(KeyCode.A, KeyCode.LeftArrow) ? 1 : 0), (Held(KeyCode.W, KeyCode.UpArrow) ? 1 : 0) - (Held(KeyCode.S, KeyCode.DownArrow) ? 1 : 0)),
                Zoom = UnityEngine.Input.mouseScrollDelta.y,
                Drag = UnityEngine.Input.GetMouseButton(2) && !UnityEngine.Input.GetMouseButtonDown(2) ? new Vector2(previous.x - mouse.x, previous.y - mouse.y) : Vector2.zero
            };
            previous = mouse;
            return intent;
        }
        static bool Held(KeyCode a, KeyCode b) => UnityEngine.Input.GetKey(a) || UnityEngine.Input.GetKey(b);
    }
}
