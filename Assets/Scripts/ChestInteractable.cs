using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody))]
public class ChestInteractable : MonoBehaviour
{
    [Header("Hit Settings")]
    [SerializeField] private int hitsToOpen = 3;

    [Header("Creature Spawn")]
    [SerializeField] private GameObject[] creaturePrefabs;
    [SerializeField] private int   followCount  = 2;    // first N creatures will follow player
    [SerializeField] private float spawnDelay   = 0.8f; // seconds after open trigger before spawning

    private Animator _animator;
    private int  _hitCount;
    private bool _opened;

    void Awake()
    {
        _animator = GetComponent<Animator>();

        // Kinematic triggers (the fork) cannot detect static colliders.
        // A non-kinematic, fully-frozen Rigidbody makes this a "Rigidbody Collider"
        // so OnTriggerEnter fires correctly while the chest stays immovable.
        var rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.useGravity  = false;
        rb.constraints = RigidbodyConstraints.FreezeAll;
    }

    public void RegisterHit()
    {
        if (_opened) return;
        _hitCount++;
        if (_hitCount >= hitsToOpen)
        {
            _opened = true;
            _animator.Play("Open", 0, 0f);
            StartCoroutine(SpawnCreatures());
        }
    }

    private IEnumerator SpawnCreatures()
    {
        yield return new WaitForSeconds(spawnDelay);

        for (int i = 0; i < creaturePrefabs.Length; i++)
        {
            if (creaturePrefabs[i] == null) continue;

            Vector3 spawnPos = transform.position
                + Random.insideUnitSphere * 0.4f
                + Vector3.up * 0.5f;

            GameObject creature = Instantiate(creaturePrefabs[i], spawnPos, Random.rotation);

            CreatureFollower follower = creature.GetComponent<CreatureFollower>();
            if (follower != null)
                follower.ShouldFollow = i < followCount;
        }
    }
}
