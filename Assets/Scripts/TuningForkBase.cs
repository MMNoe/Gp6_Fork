using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class TuningForkBase : OVRGrabbable
{
    [Header("Hold Pose")]
    [SerializeField] private Vector3 holdPositionOffset = Vector3.zero;
    [SerializeField] private Vector3 holdRotationOffset = Vector3.zero;

    [Header("Hand Restriction")]
    [Tooltip("None = either hand. Set LTouch or RTouch to restrict to one hand.")]
    [SerializeField] private OVRInput.Controller allowedHand = OVRInput.Controller.None;

    private Rigidbody _rb;
    private OVRGrabber _heldHand;
    private bool _isAttached;

    public bool IsAttached => _isAttached;
    protected OVRInput.Controller HeldByController { get; private set; }
    public OVRInput.Controller HeldController => HeldByController;

    protected override void Start()
    {
        base.Start();
        _rb = GetComponent<Rigidbody>();
    }

    public override void GrabBegin(OVRGrabber hand, Collider grabPoint)
    {
        if (_isAttached) return;

        OVRInput.Controller handController = hand.gameObject.name.ToLower().Contains("right")
            ? OVRInput.Controller.RTouch
            : OVRInput.Controller.LTouch;

        if (allowedHand != OVRInput.Controller.None && handController != allowedHand) return;

        base.GrabBegin(hand, grabPoint);

        _isAttached      = true;
        _heldHand        = hand;
        HeldByController = handController;

        OnAttached(hand.transform);
    }

    // Don't call base: prevents OVRGrabbable from restoring isKinematic and applying throw velocity.
    // Manually clear OVRGrabbable grab state; keep _isAttached/_heldHand so LateUpdate keeps positioning the fork.
    public override void GrabEnd(Vector3 linearVelocity, Vector3 angularVelocity)
    {
        m_grabbedBy       = null;
        m_grabbedCollider = null;
        // _isAttached intentionally kept true — fork stays attached via LateUpdate after OVRGrabber releases it
    }

    // LateUpdate runs after OVRGrabber.Update, taking over position control in the same frame.
    void LateUpdate()
    {
        if (!_isAttached || _heldHand == null || isGrabbed) return;

        _rb.isKinematic = true;
        _rb.useGravity  = false;

        if (transform.parent != _heldHand.transform)
            transform.SetParent(_heldHand.transform, false);

        transform.localPosition    = holdPositionOffset;
        transform.localEulerAngles = holdRotationOffset;
    }

    protected virtual void OnAttached(Transform anchor) { }
}
