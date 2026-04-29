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

    private CreatureFollower _follower;
    private AudioSource      _audio;
    private bool             _hit;

    void Start()
    {
        _follower = GetComponent<CreatureFollower>();
        _audio    = GetComponent<AudioSource>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (_hit) return;

        ItemTuningFork fork = other.GetComponentInParent<ItemTuningFork>();
        if (fork == null) return;
        if (fork.HeldController != OVRInput.Controller.LTouch) return;

        _hit = true;
        StartCoroutine(HandleHit());
    }

    private IEnumerator HandleHit()
    {
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
            r.material.color = hitColor;

        if (_audio != null && hitSound != null)
            _audio.PlayOneShot(hitSound);

        if (octopusType == OctopusType.Follower)
        {
            _follower.ShouldFollow = true;
        }
        else
        {
            yield return new WaitForSeconds(disappearDelay);

            float    elapsed    = 0f;
            Vector3  startScale = transform.localScale;

            while (elapsed < shrinkDuration)
            {
                elapsed += Time.deltaTime;
                transform.localScale = Vector3.Lerp(startScale, Vector3.zero, elapsed / shrinkDuration);
                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
