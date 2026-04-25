using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class HealthUIController : MonoBehaviour
{
    [Header("UI 貝殼物件")]
    // 在 Inspector 中手動拖入，或讓 Start 自動按名字排序尋找
    public List<GameObject> shellIcons = new List<GameObject>();

    // 由 GameManager 呼叫
    public void UpdateHealthDisplay(int currentHP)
    {
        for (int i = 0; i < shellIcons.Count; i++)
        {
            if (shellIcons[i] != null)
            {
                // 如果索引小於血量則顯示，否則隱藏
                shellIcons[i].SetActive(i < currentHP);
            }
        }
    }
}