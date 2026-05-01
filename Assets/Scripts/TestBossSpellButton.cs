using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach to a UI Button to simulate Ursula's spell effect without VR.
/// In the real scene, StartSpellEffect() is called automatically by BossLoop
/// when the boss enters the attack phase.
/// </summary>
[RequireComponent(typeof(Button))]
public class TestBossSpellButton : MonoBehaviour
{
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        if (UrsulaBossManager.Instance != null)
            UrsulaBossManager.Instance.StartSpellEffect();
    }
}
