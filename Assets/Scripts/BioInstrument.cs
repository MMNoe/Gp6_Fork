using UnityEngine;

public class BioInstrument : MonoBehaviour
{
    [Header("基礎設定")]
    public int instrumentID;
    public AudioSource audioSource;
    public AudioClip hitSound;

    private void Start()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("fork"))
        {
            if (audioSource != null && hitSound != null)
            {
                audioSource.PlayOneShot(hitSound);
            }

            if (UrsulaBossManager.Instance != null)
                UrsulaBossManager.Instance.RecordInstrumentHit(instrumentID);

            if (GatePanelManager.Instance != null)
                GatePanelManager.Instance.RecordInstrumentHit(instrumentID);
        }
    }
}