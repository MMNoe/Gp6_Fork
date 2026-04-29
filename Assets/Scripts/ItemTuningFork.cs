using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ItemTuningFork : TuningForkBase
{
    [Header("Hit Settings")]
    [SerializeField] private float hitVelocityThreshold = 0.5f;
    [SerializeField] private float hitCooldown = 0.5f;

    private float _lastHitTime = -Mathf.Infinity;

    [Header("Haptic Settings")]
    [SerializeField] private float hapticDuration   = 0.2f;
    [SerializeField] private float hapticFrequency  = 1f;
    [SerializeField] private float hapticAmplitude  = 0.8f;

    private AudioSource _audioSource;

    protected override void Start()
    {
        base.Start();
        _audioSource = GetComponent<AudioSource>();

        if (_audioSource.clip == null)
            _audioSource.clip = GenerateBeep(440f, 0.3f);
    }

    private AudioClip GenerateBeep(float frequency, float duration)
    {
        int sampleRate = AudioSettings.outputSampleRate;
        int samples    = Mathf.RoundToInt(sampleRate * duration);
        float[] data   = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t      = (float)i / sampleRate;
            float fade   = 1f - (t / duration);           // linear fade out
            data[i]      = Mathf.Sin(2f * Mathf.PI * frequency * t) * fade * 0.5f;
        }

        AudioClip clip = AudioClip.Create("TuningForkBeep", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!IsAttached) return;
        if (other.transform.IsChildOf(transform.root)) return;
        if (Time.time - _lastHitTime < hitCooldown) return;
        if (OVRInput.GetLocalControllerVelocity(HeldByController).magnitude < hitVelocityThreshold) return;

        // Left hand (angel fork) cannot open chests
        if (HeldByController == OVRInput.Controller.LTouch &&
            other.GetComponentInParent<ChestInteractable>() != null) return;

        _lastHitTime = Time.time;
        _audioSource.PlayOneShot(_audioSource.clip);
        StartCoroutine(HapticRoutine());

        ChestInteractable chest = other.GetComponentInParent<ChestInteractable>();
        if (chest != null) chest.RegisterHit();
    }

    private IEnumerator HapticRoutine()
    {
        OVRInput.SetControllerVibration(hapticFrequency, hapticAmplitude, HeldByController);
        yield return new WaitForSeconds(hapticDuration);
        OVRInput.SetControllerVibration(0f, 0f, HeldByController);
    }
}
