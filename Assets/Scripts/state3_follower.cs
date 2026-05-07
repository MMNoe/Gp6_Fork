using UnityEngine;

public class SemicircleFormationFollower : MonoBehaviour
{
    [Header("目標設定")]
    [SerializeField] private Transform playerCamera; // 預設會抓 Camera.main

    [Header("追隨物件")]
    [SerializeField] private Transform[] instruments = new Transform[4]; 

    [Header("半圓陣列設定")]
    [SerializeField] private float radius = 2.0f;       // 半圓半徑
    [SerializeField] private float arcAngle = 120f;    // 半圓張開的角度 (例如 180 是正半圓)
    [SerializeField] private float heightOffset = -0.5f; // 垂直偏移（偏下）
    [SerializeField] private float forwardOffset = 0.5f; // 整體前移距離

    [Header("平滑移動")]
    [Range(0.01f, 1.0f)]
    [SerializeField] private float smoothSpeed = 0.1f;
    [SerializeField] private float rotationSpeed = 5.0f;

    private Vector3[] _velocities;

    void Start()
    {
        if (playerCamera == null && Camera.main != null)
        {
            playerCamera = Camera.main.transform;
        }

        _velocities = new Vector3[instruments.Length];
    }

    void LateUpdate()
    {
        if (playerCamera == null || instruments == null) return;

        // 取得玩家平面的前方與右方向量（忽略抬頭低頭的影響，維持水平半圓）
        Vector3 forward = new Vector3(playerCamera.forward.x, 0, playerCamera.forward.z).normalized;
        Vector3 right = new Vector3(playerCamera.right.x, 0, playerCamera.right.z).normalized;
        Vector3 up = Vector3.up;

        // 如果前方向量太小（例如垂直看上下），則不更新位置以防抖動
        if (forward.sqrMagnitude < 0.01f) return;

        for (int i = 0; i < instruments.Length; i++)
        {
            if (instruments[i] == null) continue;

            // 1. 計算每個物件在半圓中的角度偏移
            // 假設 4 個物件，i=0 到 3，分佈在 -arcAngle/2 到 arcAngle/2 之間
            float fraction = (instruments.Length > 1) ? (float)i / (instruments.Length - 1) : 0.5f;
            float angle = (fraction - 0.5f) * arcAngle;

            // 2. 計算目標座標
            // 以玩家位置為中心，先計算圓周上的點，再套用高度與前移
            Quaternion rotation = Quaternion.AngleAxis(angle, up);
            Vector3 direction = rotation * forward;
            
            Vector3 targetPosition = playerCamera.position 
                                     + (direction * radius) 
                                     + (up * heightOffset)
                                     + (forward * forwardOffset);

            // 3. 平滑移動位置
            instruments[i].position = Vector3.SmoothDamp(
                instruments[i].position, 
                targetPosition, 
                ref _velocities[i], 
                smoothSpeed
            );

            // 4. 旋轉：讓物件面朝玩家相機
            Vector3 lookAtPos = playerCamera.position;
            // 如果不想要物件上下傾斜，可以解開下一行的註釋：
            // lookAtPos.y = instruments[i].position.y; 

            Quaternion targetRotation = Quaternion.LookRotation(instruments[i].position - lookAtPos);
            instruments[i].rotation = Quaternion.Slerp(instruments[i].rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }
}