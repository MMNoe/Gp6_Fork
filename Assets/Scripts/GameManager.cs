using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("音效設定")]
    public AudioSource bgmSource;
    public List<AudioClip> sceneBGMs;

    [Header("玩家數值")]
    public int maxHealth = 30;
    public int currentHealth;

    [Header("音叉自動裝備")]
    public bool hasTuningFork = false;
    public TuningForkBase tuningForkPrefab;
    public TuningForkBase angelForkPrefab;

    [Header("場景設定")]
    public string playerTag = "Player";
    public string spawnPointTag = "SceneSpawnPoint";

    // 內部引用，每一關載入時會自動重新尋找
    private HealthUIController healthUI;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            bgmSource = GetComponent<AudioSource>();
            if (bgmSource == null) bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        currentHealth = maxHealth;
    }

    // 每一關載入後自動執行
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PositionPlayerAtStart();

        healthUI = FindObjectOfType<HealthUIController>();
        if (healthUI != null)
        {
            healthUI.UpdateHealthDisplay(currentHealth);
        }
        PlaySceneBGM(scene.buildIndex);

        int idx = scene.buildIndex;
        if (idx == 1 || idx == 2)
            StartCoroutine(AutoEquipForksRoutine());
    }

    public void NotifyForkPickedUp() => hasTuningFork = true;

    private IEnumerator AutoEquipForksRoutine()
    {
        yield return null; // wait one frame so all Start() methods have run

        OVRGrabber leftGrabber = null, rightGrabber = null;
        foreach (var g in FindObjectsOfType<OVRGrabber>())
        {
            if (g.gameObject.name.ToLower().Contains("right")) rightGrabber = g;
            else leftGrabber = g;
        }

        if (leftGrabber == null || rightGrabber == null)
        {
            Debug.LogWarning("[GameManager] AutoEquip: OVRGrabber(s) not found.");
            yield break;
        }

        if (hasTuningFork && tuningForkPrefab != null)
        {
            var fork = Instantiate(tuningForkPrefab);
            fork.AutoAttach(rightGrabber);
        }

        if (angelForkPrefab != null)
        {
            var fork = Instantiate(angelForkPrefab);
            fork.AutoAttach(leftGrabber);
        }
    }

    void PlaySceneBGM(int index)
    {
        if (index < sceneBGMs.Count && sceneBGMs[index] != null)
        {
            if (bgmSource.clip == sceneBGMs[index]) return;

            bgmSource.clip = sceneBGMs[index];
            bgmSource.Play();
            Debug.Log($"正在播放場景 {index} 的 BGM: {sceneBGMs[index].name}");
        }
    }

    // 供其他程式呼叫：玩家扣血
    public void PlayerTakeDamage(int amount)
    {
        currentHealth -= amount;
        
        if (healthUI != null)
            healthUI.UpdateHealthDisplay(currentHealth);

        if (currentHealth <= 0)
            Respawn();
    }

    // 供其他程式呼叫：前往下一關
    public void LoadNextLevel()
    {
        int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextIndex);
        }
    }

    private void PositionPlayerAtStart()
    {
        GameObject spawnPoint = GameObject.FindGameObjectWithTag(spawnPointTag);
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);

        if (spawnPoint != null && player != null)
        {
            player.transform.position = spawnPoint.transform.position;
            player.transform.rotation = spawnPoint.transform.rotation;
        }
    }

    void Respawn()
    {
        currentHealth = maxHealth;
        // 重新載入當前場景
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}