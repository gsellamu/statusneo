// OrbitCamera.cs — gentle auto-orbit + drag-to-look, so the lens feels alive in the
// demo. Pure view aid; no twin coupling.

using UnityEngine;

namespace Modutecture.Lens
{
    public class OrbitCamera : MonoBehaviour
    {
        public Vector3 target = new Vector3(2f, 0.4f, 1.5f);   // room centre
        public float radius = 5.5f;
        public float autoSpeed = 6f;       // deg/sec
        public float dragSpeed = 0.3f;

        float _theta = 40f, _phi = 48f;
        bool _dragging; Vector3 _last;

        void Update()
        {
            if (Input.GetMouseButtonDown(1)) { _dragging = true; _last = Input.mousePosition; }
            if (Input.GetMouseButtonUp(1))   { _dragging = false; }

            if (_dragging)
            {
                var d = Input.mousePosition - _last; _last = Input.mousePosition;
                _theta -= d.x * dragSpeed;
                _phi = Mathf.Clamp(_phi - d.y * dragSpeed, 12f, 80f);
            }
            else _theta += autoSpeed * Time.deltaTime;

            radius = Mathf.Clamp(radius - Input.mouseScrollDelta.y * 0.5f, 2.5f, 12f);

            float t = _theta * Mathf.Deg2Rad, p = _phi * Mathf.Deg2Rad;
            var pos = new Vector3(
                target.x + radius * Mathf.Sin(p) * Mathf.Cos(t),
                target.y + radius * Mathf.Cos(p),
                target.z + radius * Mathf.Sin(p) * Mathf.Sin(t));
            transform.position = pos;
            transform.LookAt(target);
        }
    }
}
