

using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class HitVocal : MonoBehaviour
{
    [Header("音效設定")]
    public AudioClip hitSound; 
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("fork"))
        {
            Debug.Log("有敲到了~");

            if (hitSound != null)
            {
                audioSource.PlayOneShot(hitSound);
            }
        }
    }
}

