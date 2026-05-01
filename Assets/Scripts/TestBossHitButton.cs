using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach to a UI Button to simulate a fork hit on Ursula without VR.
/// Set isRealHit = true on one button (hits real Ursula) and false on another (hits fake).
/// In the real scene, UrsulaBossManager.OnBossAttacked is called automatically
/// via UrsulaHitDetector.OnTriggerEnter when the angelfork tag collides with an Ursula.
/// </summary>
[RequireComponent(typeof(Button))]
public class TestBossHitButton : MonoBehaviour
{
    [SerializeField] private bool isRealHit = true;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        if (UrsulaBossManager.Instance == null) return;

        // Play the particle manually for testing since OnTriggerEnter won't fire
        ParticleSystem particle = isRealHit
            ? UrsulaBossManager.Instance.realHitParticle
            : UrsulaBossManager.Instance.fakeHitParticle;

        if (particle != null) particle.Play();

        UrsulaBossManager.Instance.OnBossAttacked(isRealHit);
    }
}
