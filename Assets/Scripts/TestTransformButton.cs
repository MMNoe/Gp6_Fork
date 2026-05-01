using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach this to the same GameObject as a Unity UI Button.
/// Drag the Squid_Bad instance into the "target" field in the Inspector.
/// Clicking the button calls TriggerTransformation() on it.
/// </summary>
[RequireComponent(typeof(Button))]
public class TestTransformButton : MonoBehaviour
{
    [SerializeField] private SquidBadController target;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        if (target != null)
            target.TriggerTransformation();
    }
}
