using UnityEngine;

// Attach to each gate panel GameObject (with a trigger Collider).
// Detects right-hand fork hits and notifies GatePanelManager.
public class GatePanelInteractable : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        ItemTuningFork fork = other.GetComponentInParent<ItemTuningFork>();
        if (fork == null) return;
        if (fork.HeldController != OVRInput.Controller.RTouch) return;

        if (GatePanelManager.Instance != null)
            GatePanelManager.Instance.OnPanelHit();
    }
}
