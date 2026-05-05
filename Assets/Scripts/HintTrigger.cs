using UnityEngine;


public class HintTrigger : MonoBehaviour
{
    [Header("傳送點設定")]
    public Transform fishSpawnPos;
    public Transform fishTargetPos;
    public FishHint thefish;

    [Header("語音設定")]
    public AudioClip hintClip;
   
    private bool activated = false;


    private void OnTriggerEnter(Collider other)
    {
        
        if (!activated && other.CompareTag("Player"))
        {
            Debug.Log("觸發提示0");
            if (FishHint.Instance != null)
            {
                
                FishHint.Instance.StartHint(fishSpawnPos, fishTargetPos, hintClip);
                activated = true;
                Debug.Log("觸發提示1");
            }
        }
    }
}