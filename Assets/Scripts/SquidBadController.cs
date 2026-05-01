using System.Collections;
using UnityEngine;

/// <summary>
/// Attached to a Squid_Bad prefab instance.
/// When hit by the magical (left-hand) fork, plays a particle burst
/// and then swaps the material to M_Squid_Delighten.
///
/// HOW TO WIRE IN THE REAL SCENE (VR):
///   Option A – Automatic via trigger (no code change needed):
///     Make sure Squid_Bad has a Collider set to "Is Trigger",
///     and the magical fork prefab also has a trigger Collider.
///     OnTriggerEnter below will fire automatically.
///
///   Option B – Call from ItemTuningFork (if you need extra control):
///     In ItemTuningFork.OnTriggerEnter, after the cooldown checks, add:
///       SquidBadController squid = other.GetComponentInParent<SquidBadController>();
///       if (squid != null && HeldByController == OVRInput.Controller.LTouch)
///           squid.TriggerTransformation();
/// </summary>
public class SquidBadController : MonoBehaviour
{
    [Header("Material")]
    [SerializeField] private Material transformedMaterial;   // drag M_Squid_Delighten here

    [Header("Particle")]
    [SerializeField] private ParticleSystem transformParticle; // drag child ParticleSystem here

    [Header("Timing")]
    [SerializeField] private float materialSwapDelay = 0.15f;  // seconds after burst before material changes

    private Renderer[] _renderers;
    private bool _triggered;

    void Awake()
    {
        // Exclude ParticleSystemRenderer so the nested particle system's
        // material is not overwritten when the squid material swaps.
        _renderers = System.Array.FindAll(
            GetComponentsInChildren<Renderer>(),
            r => r is not ParticleSystemRenderer
        );
    }

    // --- VR trigger detection ---
    // This fires when the fork collider overlaps the Squid_Bad collider.
    // It checks that the hitting object is the magical fork held in the left hand.
    void OnTriggerEnter(Collider other)
    {
        if (_triggered) return;

        ItemTuningFork fork = other.GetComponentInParent<ItemTuningFork>();
        if (fork == null) return;

        // Left hand = magical/angel fork
        if (fork.HeldController != OVRInput.Controller.LTouch) return;

        TriggerTransformation();
    }

    // --- Public entry point ---
    // Call this from a test button, or from any other script.
    public void TriggerTransformation()
    {
        if (_triggered) return;
        _triggered = true;

        if (transformParticle != null)
            transformParticle.Play();

        StartCoroutine(SwapMaterialAfterDelay());
    }

    private IEnumerator SwapMaterialAfterDelay()
    {
        yield return new WaitForSeconds(materialSwapDelay);

        if (transformedMaterial != null)
        {
            foreach (Renderer r in _renderers)
                r.material = transformedMaterial;
        }
    }
}
