using System.Collections;
using UnityEngine;

public class DebugCollectAll : MonoBehaviour
{
    [SerializeField] private float spawnWait = 1.5f; // seconds to wait for all chests to spawn creatures

    private bool _running;
    private string _status = "";

    void OnGUI()
    {
        GUIStyle btnStyle = new GUIStyle(GUI.skin.button) { fontSize = 18 };
        GUIStyle lblStyle = new GUIStyle(GUI.skin.label)  { fontSize = 14 };

        if (!_running && GUI.Button(new Rect(10, 10, 240, 50), "DEBUG: 全部收集", btnStyle))
            StartCoroutine(CollectAll());

        if (_status != "")
            GUI.Label(new Rect(10, 65, 300, 30), _status, lblStyle);
    }

    private IEnumerator CollectAll()
    {
        _running = true;
        _status  = "開箱中...";

        foreach (var chest in FindObjectsOfType<ChestInteractable>())
            chest.ForceOpen();

        yield return new WaitForSeconds(spawnWait);

        _status = "設定跟隨...";

        // Mark all OctopusControllers (prevents re-trigger; activates follower type)
        foreach (var oct in FindObjectsOfType<OctopusController>())
            oct.ForceActivate();

        // Set all remaining CreatureFollowers (生物1/2/3) to follow
        foreach (var cf in FindObjectsOfType<CreatureFollower>())
            cf.ShouldFollow = true;

        _status  = "完成！所有 follower 已收集";
        _running = false;
    }
}
