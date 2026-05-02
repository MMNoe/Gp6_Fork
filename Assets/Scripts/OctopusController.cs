using System.Collections;
using UnityEngine;

public enum OctopusType { Follower, Disappear }

[RequireComponent(typeof(CreatureFollower))]
public class OctopusController : MonoBehaviour
{
    [Header("Behaviour")]
    [SerializeField] private OctopusType octopusType    = OctopusType.Disappear;
    [SerializeField] private Color       hitColor       = new Color(1f, 0.85f, 0.2f);
    [SerializeField] private float       disappearDelay = 1.2f;
    [SerializeField] private float       shrinkDuration = 0.4f;

    [Header("Audio")]
    [SerializeField] private AudioClip hitSound;

    [Header("VFX")]
    [SerializeField] private ParticleSystem hitParticle;

    private CreatureFollower _follower;
    private AudioSource      _audio;
    private int              _hitCount;
    private bool             _disappearing;

    void Start()
    {
        _follower = GetComponent<CreatureFollower>();
        _audio    = GetComponent<AudioSource>();
    }

    public void ForceActivate()
    {
        int maxHits = octopusType == OctopusType.Follower ? 2 : 3;
        if (_hitCount >= maxHits) return;
        _hitCount = maxHits;
        ApplyHitTwo();
        if (octopusType == OctopusType.Disappear)
            StartCoroutine(DisappearRoutine());
    }

    void OnTriggerEnter(Collider other)
    {
        if (_disappearing) return;

        ItemTuningFork fork = other.GetComponentInParent<ItemTuningFork>();
        if (fork == null) return;
        if (fork.HeldController != OVRInput.Controller.LTouch) return;

        // Follower type is done after hit 2; Disappear type needs up to hit 3
        int maxHits = octopusType == OctopusType.Follower ? 2 : 3;
        if (_hitCount >= maxHits) return;

        _hitCount++;

        if (hitParticle != null) hitParticle.Play();
        if (_audio != null && hitSound != null)
            _audio.PlayOneShot(hitSound);

        if (_hitCount == 2)
            ApplyHitTwo();
        else if (_hitCount == 3)
            StartCoroutine(DisappearRoutine());
    }

    private void ApplyHitTwo()
    {
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
            r.material.color = hitColor;

        if (octopusType == OctopusType.Follower)
            _follower.ShouldFollow = true;
    }

    private IEnumerator DisappearRoutine()
    {
        _disappearing = true;
        yield return new WaitForSeconds(disappearDelay);

        float   elapsed    = 0f;
        Vector3 startScale = transform.localScale;

        while (elapsed < shrinkDuration)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, elapsed / shrinkDuration);
            yield return null;
        }

        Destroy(gameObject);
    }
}
