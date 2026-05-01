using UnityEngine;

public class LevelPortal : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("玩家到達終點，前往下一關");

            if (GameManager.Instance != null)
            {
                GameManager.Instance.LoadNextLevel();
            }
        }
    }
}