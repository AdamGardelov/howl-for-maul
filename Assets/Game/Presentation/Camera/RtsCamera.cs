using UnityEngine;
namespace FrostMaze
{
    public sealed class RtsCamera : MonoBehaviour
    {
        public float PanSpeed = 15, ZoomSpeed = 2, MinZoom = 5, MaxZoom = 25;
        public Vector2 BoundsMin, BoundsMax = new Vector2(30, 20);
        public Vector3 Focus;
        ICameraInput source = new DesktopInput();
        Camera view;
        public void Initialize(float width, float height)
        {
            view = GetComponent<Camera>();
            BoundsMax = new Vector2(width, height);
            Focus = new Vector3(width / 2, 0, height / 2);
            view.orthographic = true;
            view.orthographicSize = Mathf.Max(height * 0.65f, width * 0.56f / view.aspect);
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
            var pan = Vector2.ClampMagnitude(intent.Pan, 1);
            Focus += new Vector3(pan.x, 0, pan.y) * PanSpeed * Time.unscaledDeltaTime;
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
