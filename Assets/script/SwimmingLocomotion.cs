using UnityEngine;

public class SwimmingLocomotion : MonoBehaviour
{
    [SerializeField] private OVRCameraRig cameraRig;
    [SerializeField] private float swimForce = 3f;
    [SerializeField] private float drag = 2f;
    [SerializeField] private float pushThreshold = 0.5f;

    private Vector3 _velocity;
    private Vector3 _rightPrevLocal;
    private Vector3 _leftPrevLocal;
    private bool _initialized;
    private bool _swimmingEnabled = true;

    void Update()
    {
        if (cameraRig == null) return;

        if (OVRInput.GetDown(OVRInput.Button.One))
            _swimmingEnabled = !_swimmingEnabled;

        Transform ts = cameraRig.trackingSpace;

        Vector3 rightLocal = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
        Vector3 leftLocal  = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch);

        if (!_initialized)
        {
            _rightPrevLocal = rightLocal;
            _leftPrevLocal  = leftLocal;
            _initialized = true;
            return;
        }

        // Position delta in local tracking space → convert to world space velocity
        Vector3 rightHandVel = ts.TransformDirection((rightLocal - _rightPrevLocal) / Time.deltaTime);
        Vector3 leftHandVel  = ts.TransformDirection((leftLocal  - _leftPrevLocal)  / Time.deltaTime);

        _rightPrevLocal = rightLocal;
        _leftPrevLocal  = leftLocal;

        Vector3 headForward = cameraRig.centerEyeAnchor.forward;

        float rightPush = _swimmingEnabled ? Mathf.Max(0f, Vector3.Dot(-rightHandVel, headForward)) : 0f;
        float leftPush  = _swimmingEnabled ? Mathf.Max(0f, Vector3.Dot(-leftHandVel,  headForward)) : 0f;

        float totalPush = 0f;
        if (rightPush > pushThreshold) totalPush += rightPush;
        if (leftPush  > pushThreshold) totalPush += leftPush;

        if (totalPush > 0f)
            _velocity += headForward * totalPush * swimForce * Time.deltaTime;

        //Debug.Log($"[Swim] rLocal={rightLocal:F2} rightVel={rightHandVel.magnitude:F2} rightPush={rightPush:F2}");

        _velocity = Vector3.Lerp(_velocity, Vector3.zero, drag * Time.deltaTime);

        transform.position += _velocity * Time.deltaTime;
    }
}
