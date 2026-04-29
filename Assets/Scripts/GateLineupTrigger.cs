using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class GateLineupTrigger : MonoBehaviour
{
    [Header("Conditions")]
    [SerializeField] private int    requiredFollowers = 4;
    [SerializeField] private string playerTag         = "Player";

    [Header("Lineup Layout")]
    [SerializeField] private float lineupDistance = 2.0f;  // 排在玩家前方幾公尺
    [SerializeField] private float lineupSpacing  = 0.8f;  // 每隻之間的間距
    [SerializeField] private float lineupHeight   = 0f;    // 相對玩家高度偏移

    private bool _triggered;

    void Awake()
    {
        GetComponent<BoxCollider>().isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (_triggered) return;
        if (!other.CompareTag(playerTag)) return;

        List<CreatureFollower> followers = CreatureFollower.GetActiveFollowers();
        if (followers.Count < requiredFollowers) return;

        _triggered = true;
        TriggerLineup(followers);
    }

    private void TriggerLineup(List<CreatureFollower> followers)
    {
        Transform cam     = Camera.main != null ? Camera.main.transform : null;
        Vector3   origin  = cam != null ? cam.position : transform.position;

        // Flatten to horizontal plane
        Vector3 forward = cam != null
            ? new Vector3(cam.forward.x, 0f, cam.forward.z).normalized
            : Vector3.forward;
        Vector3 right   = new Vector3(forward.z, 0f, -forward.x); // perpendicular

        Vector3 rowCenter = origin + forward * lineupDistance + Vector3.up * lineupHeight;

        int   count = followers.Count;
        float totalWidth = (count - 1) * lineupSpacing;

        for (int i = 0; i < count; i++)
        {
            float   offset    = -totalWidth / 2f + i * lineupSpacing;
            Vector3 targetPos = rowCenter + right * offset;
            followers[i].LineUpAt(targetPos, forward);
        }
    }
}
