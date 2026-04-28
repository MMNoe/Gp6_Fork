using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class HealthUIController : MonoBehaviour
{
    [Header("UI 貝殼物件")]
    public List<GameObject> shellIcons = new List<GameObject>();

    public void UpdateHealthDisplay(int currentHP)
    {
        for (int i = 0; i < shellIcons.Count; i++)
        {
            if (shellIcons[i] != null)
            {
                shellIcons[i].SetActive(i < currentHP);
            }
        }
    }
}