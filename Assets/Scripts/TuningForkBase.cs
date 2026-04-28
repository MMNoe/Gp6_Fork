using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class TuningForkBase : OVRGrabbable
{
    [Header("Hold Pose")]
    [SerializeField] private Vector3 holdPositionOffset = Vector3.zero;
    [SerializeField] private Vector3 holdRotationOffset = Vector3.zero;

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

        base.GrabBegin(hand, grabPoint);

        _isAttached = true;
        _heldHand   = hand;

        HeldByController = hand.gameObject.name.ToLower().Contains("right")
            ? OVRInput.Controller.RTouch
            : OVRInput.Controller.LTouch;

        OnAttached(hand.transform);
    }

    // Don't call base: prevents OVRGrabbable from restoring isKinematic and applying throw velocity.
    // Only clears m_grabbedBy so isGrabbed = false, allowing LateUpdate to take over positioning.
    public override void GrabEnd(Vector3 linearVelocity, Vector3 angularVelocity)
    {
        m_grabbedBy       = null;
        m_grabbedCollider = null;
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
