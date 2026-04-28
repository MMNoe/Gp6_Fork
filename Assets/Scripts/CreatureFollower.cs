using System.Collections.Generic;
using UnityEngine;

public class CreatureFollower : MonoBehaviour
{
    [Header("Follow Settings")]
    [SerializeField] private bool  shouldFollow  = false;
    [SerializeField] private float moveSpeed     = 1.5f;
    [SerializeField] private float rotateSpeed   = 3f;
    [SerializeField] private float followDistance = 1.2f;

    [Header("Animation State Names")]
    [SerializeField] private string swimStateName = "Armature|swim";
    [SerializeField] private string idleStateName = "Armature|idle";

    [Header("Idle Drift (non-following creatures)")]
    [SerializeField] private float driftSpeed  = 0.3f;
    [SerializeField] private float driftRadius = 1.0f;

    [Header("Separation")]
    [SerializeField] private float separationRadius = 0.8f;
    [SerializeField] private float separationForce  = 3f;

    // Shared registry — no FindObjectsOfType needed every frame
    private static readonly List<CreatureFollower> _all = new();

    private Transform _player;
    private Vector3   _driftCenter;
    private float     _driftAngle;
    private Animator  _animator;
    private bool      _animPlayed;

    public bool ShouldFollow
    {
        get => shouldFollow;
        set => shouldFollow = value;
    }

    void OnEnable()  => _all.Add(this);
    void OnDisable() => _all.Remove(this);

    void Start()
    {
        _player      = Camera.main != null ? Camera.main.transform : null;
        _driftCenter = transform.position;
        _driftAngle  = Random.Range(0f, 360f);
        _animator    = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        // Animator.Play() in Start() is silently ignored because the Animator
        // hasn't finished initialising yet — defer to the first Update frame.
        if (!_animPlayed && _animator != null)
        {
            _animPlayed = true;
            _animator.Play(swimStateName);
        }

        if (shouldFollow)
            Follow();
        else
            IdleDrift();

        Separate();
    }

    private void Follow()
    {
        if (_player == null) return;

        Vector3 toPlayer = _player.position - transform.position;
        float dist = toPlayer.magnitude;

        if (dist <= followDistance) return;

        Quaternion targetRot = Quaternion.LookRotation(toPlayer.normalized);
        transform.SetPositionAndRotation(
            Vector3.MoveTowards(transform.position, _player.position, moveSpeed * Time.deltaTime),
            Quaternion.Slerp(transform.rotation, targetRot, rotateSpeed * Time.deltaTime));
    }

    private void IdleDrift()
    {
        _driftAngle += driftSpeed * Time.deltaTime * 30f;
        float rad = _driftAngle * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad * 0.4f) * 0.3f, Mathf.Sin(rad)) * driftRadius;
        Vector3 target = _driftCenter + offset;

        transform.position = Vector3.MoveTowards(transform.position, target, driftSpeed * Time.deltaTime);

        Vector3 dir = target - transform.position;
        if (dir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 2f * Time.deltaTime);
    }

    // Push away from any other CreatureFollower that is too close
    private void Separate()
    {
        foreach (CreatureFollower other in _all)
        {
            if (other == this) continue;

            Vector3 away = transform.position - other.transform.position;
            float dist = away.magnitude;

            if (dist > 0.001f && dist < separationRadius)
            {
                float overlap = (separationRadius - dist) / separationRadius;
                transform.position += away.normalized * overlap * separationForce * Time.deltaTime;
            }
        }
    }
}
