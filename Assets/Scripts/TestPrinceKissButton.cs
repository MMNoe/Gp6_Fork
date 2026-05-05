using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Test button that replicates the prince section of UrsulaBossManager.HandleVictorySequence:
///   1. Swap princeFish → princeHuman
///   2. Rotate prince to face the camera (proxy for the player in the test scene)
///   3. Play the kiss animation via the trigger name
///
/// In the real scene this is triggered automatically when bossHP reaches 0.
/// </summary>
[RequireComponent(typeof(Button))]
public class TestPrinceKissButton : MonoBehaviour
{
    [SerializeField] private GameObject princeFish;
    [SerializeField] private GameObject princeHuman;
    [SerializeField] private string kissTrigger = "doKiss";

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        StartCoroutine(PlayKissSequence());
    }

    private IEnumerator PlayKissSequence()
    {
        if (princeFish != null) princeFish.SetActive(false);
        if (princeHuman != null) princeHuman.SetActive(true);

        yield return new WaitForSeconds(1.0f);

        // Rotate toward the camera (stand-in for the player in the test scene)
        if (princeHuman != null && Camera.main != null)
            yield return StartCoroutine(RotateToward(princeHuman, Camera.main.transform));

        // --- DEBUG ---
        if (princeHuman == null) {
            Debug.LogError("[PrinceKiss] princeHuman is NULL — drag it into the Inspector slot.");
            yield break;
        }
        Debug.Log($"[PrinceKiss] princeHuman found: {princeHuman.name}, active={princeHuman.activeInHierarchy}");

        Animator anim = princeHuman.GetComponent<Animator>();
        if (anim == null) {
            Debug.LogError("[PrinceKiss] princeHuman has NO Animator component.");
            yield break;
        }
        Debug.Log($"[PrinceKiss] Animator found. Controller={(anim.runtimeAnimatorController == null ? "NULL" : anim.runtimeAnimatorController.name)}, enabled={anim.enabled}");

        var triggers = System.Array.FindAll(anim.parameters, p => p.type == AnimatorControllerParameterType.Trigger);
        if (triggers.Length == 0)
            Debug.LogWarning("[PrinceKiss] Animator has NO trigger parameters at all.");
        else
            Debug.Log("[PrinceKiss] Triggers found: " + string.Join(", ", System.Array.ConvertAll(triggers, t => t.name)));

        bool hasTrigger = System.Array.Exists(triggers, t => t.name == kissTrigger);
        if (!hasTrigger)
            Debug.LogError($"[PrinceKiss] Trigger '{kissTrigger}' NOT found in Animator. Check spelling or the controller.");
        else
            Debug.Log($"[PrinceKiss] Calling SetTrigger(\"{kissTrigger}\")");
        // --- END DEBUG ---

        anim.SetTrigger(kissTrigger);
    }

    private IEnumerator RotateToward(GameObject actor, Transform target)
    {
        float duration = 1.0f;
        float elapsed = 0f;
        Quaternion startRot = actor.transform.rotation;

        Vector3 dir = target.position - actor.transform.position;
        dir.y = 0;
        Quaternion endRot = Quaternion.LookRotation(dir);

        while (elapsed < duration)
        {
            actor.transform.rotation = Quaternion.Slerp(startRot, endRot, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        actor.transform.rotation = endRot;
    }
}
