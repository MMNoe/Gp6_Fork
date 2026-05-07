using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Swim_Stage2 : MonoBehaviour
{
    [SerializeField] private OVRCameraRig cameraRig;
    [SerializeField] private float swimForce = 3f;
    [SerializeField] private float drag = 2f;
    [SerializeField] private float pushThreshold = 0.5f;

    [Header("Swimming SFX")]
    [SerializeField] private AudioClip[] swimSounds;
    [SerializeField] private float stopVelocityThreshold = 0.05f;
    
    [Header("高度限制設定")]
    public float minHeight = -5.0f;

    private AudioSource _audioSource;
    private int _lastClipIndex = -1;

    private Vector3 _velocity;
    private Vector3 _rightPrevLocal;
    private Vector3 _leftPrevLocal;
    private bool _initialized;
    private bool _swimmingEnabled = true;

    void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

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
            _velocity += headForward * (totalPush * swimForce * Time.deltaTime);

        //Debug.Log($"[Swim] rLocal={rightLocal:F2} rightVel={rightHandVel.magnitude:F2} rightPush={rightPush:F2}");

        _velocity = Vector3.Lerp(_velocity, Vector3.zero, drag * Time.deltaTime);

        transform.position += _velocity * Time.deltaTime;

        if (transform.position.y < minHeight)
        {
            // 強制將 Y 座標拉回最低限制
            Vector3 clampedPosition = transform.position;
            clampedPosition.y = minHeight;
            transform.position = clampedPosition;

            // 3. 【進階優化】消除向下墜落的垂直慣性
            // 這樣玩家撞到地版時不會因為 Velocity.y 還是負值而產生抖動
            if (_velocity.y < 0)
            {
                _velocity.y = 0;
            }
        }

        UpdateSwimmingAudio();
    }

    private void UpdateSwimmingAudio()
    {
        if (swimSounds == null || swimSounds.Length == 0) return;

        bool isMoving = _velocity.magnitude > stopVelocityThreshold;

        if (isMoving)
        {
            if (!_audioSource.isPlaying)
                PlayNextClip();
        }
        else
        {
            if (_audioSource.isPlaying)
                _audioSource.Stop();
        }
    }

    private void PlayNextClip()
    {
        int index;
        if (swimSounds.Length == 1)
        {
            index = 0;
        }
        else
        {
            do { index = Random.Range(0, swimSounds.Length); }
            while (index == _lastClipIndex);
        }

        _lastClipIndex = index;
        _audioSource.clip = swimSounds[index];
        _audioSource.Play();
    }
}
