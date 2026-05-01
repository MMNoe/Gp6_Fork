
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UrsulaBossManager : MonoBehaviour
{
    public static UrsulaBossManager Instance;

    [Header("基礎設定")]
    public int bossHP = 3;
    public Transform[] spawnPoints; 
    public GameObject realUrsula;
    public GameObject fakeUrsula;

    [Header("動畫設定")]
    private Animator realAnim;
    private Animator fakeAnim;

    [Header("音效設定")]
    public AudioSource bossAudioSource;
    public AudioClip singingSound;   
    public AudioClip realUrsulaSound; 
    public AudioClip fakeUrsulaSound; 

    [Header("防禦機制")]
    public List<int> correctSequence = new List<int> { 0, 1, 2 }; 
    private List<int> playerInputSequence = new List<int>();
    
    [Header("狀態監控")]
    public bool isDefenseWindow = false; 
    public bool isCooldown = false;  

    [Header("移動設定")]
    public float moveSpeed = 2.0f; 

    [Header("勝利後設定")]
    public GameObject cage; 
    public GameObject prince; 
    public string princeKissTrigger = "doKiss"; 
    public GameObject princeFish;
    public GameObject princeHuman;
     
    void Awake() { Instance = this; }

    void Start() {
        realAnim = realUrsula.GetComponent<Animator>();
        fakeAnim = fakeUrsula.GetComponent<Animator>();
        
        SetupHitDetection(realUrsula, true);
        SetupHitDetection(fakeUrsula, false);

        StartCoroutine(BossLoop());
    }

    void SetupHitDetection(GameObject obj, bool isReal) {
        var detector = obj.GetComponent<UrsulaHitDetector>();
        if (detector == null) detector = obj.AddComponent<UrsulaHitDetector>();
        detector.isReal = isReal;
        detector.manager = this;
    }

    IEnumerator BossLoop() {
        while (bossHP > 0) {
            // State 0: 初始
            Debug.Log("烏蘇拉開始唱歌...");
            SetAllAnimations("doIdle");
            yield return new WaitForSeconds(2f);

            // State 1: 烏蘇拉攻擊
            Debug.Log("烏蘇拉開始唱歌...");
            
            if (bossAudioSource != null && singingSound != null) {
                bossAudioSource.clip = singingSound;
                bossAudioSource.Play();
            }
            //SetAllAnimations("doIdle");
            
            isDefenseWindow = true;
            playerInputSequence.Clear();

            // 秒數要改成她唱多久！
            float timer = 0;
            SetAllAnimations("doSpell");
            while (isDefenseWindow && timer < 3f) {
                timer += Time.deltaTime;
                yield return null;
                Debug.Log("正在唱");
            }
            

            if (isDefenseWindow) {
                DefenseFailed(); // 時間內沒打
            }
            bossAudioSource.Stop();

            // State 2: 烏蘇拉冷卻 
            isCooldown = true;
            Debug.Log("烏蘇拉冷卻中，玩家可以辨認或攻擊");
            

            float cooldownTimer = 0;
            while (isCooldown && cooldownTimer < 10f) {
                SetAllAnimations("doIdle"); 
                cooldownTimer += Time.deltaTime;
                yield return null;
            }
            isCooldown = false;

            // State 3: 位移
            if (bossHP > 0) {
                yield return StartCoroutine(MoveAndSwapRoutine());
            }
            
            // State 4: 若 HP>0 回 State 0
        }
        Debug.Log("Boss 戰結束！");
        yield return StartCoroutine(HandleVictorySequence());
    }


    IEnumerator MoveAndSwapRoutine() {
        Debug.Log("烏蘇拉變換位置中...");
        SetAllAnimations("doWalking"); 

        int rand = Random.Range(0, 2);
        Vector3 targetReal = spawnPoints[rand].position;
        Vector3 targetFake = spawnPoints[1 - rand].position;

        realUrsula.transform.LookAt(targetReal);
        fakeUrsula.transform.LookAt(targetFake);


        while (Vector3.Distance(realUrsula.transform.position, targetReal) > 0.05f) {
            SetAllAnimations("doWalking"); 
            realUrsula.transform.position = Vector3.MoveTowards(
                realUrsula.transform.position, 
                targetReal, 
                moveSpeed * Time.deltaTime
            );

            fakeUrsula.transform.position = Vector3.MoveTowards(
                fakeUrsula.transform.position, 
                targetFake, 
                moveSpeed * Time.deltaTime
            );

            yield return null;
        }

        realUrsula.transform.position = targetReal;
        fakeUrsula.transform.position = targetFake;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) {
            realUrsula.transform.LookAt(player.transform);
            fakeUrsula.transform.LookAt(player.transform);
        }

        Debug.Log("到達位置！");
        SetAllAnimations("doIdle");
    }

    void SetAllAnimations(string triggerName) {
        if (realAnim != null) realAnim.SetTrigger(triggerName);
        if (fakeAnim != null) fakeAnim.SetTrigger(triggerName);
    }


    public void RecordInstrumentHit(int id) {
        if (!isDefenseWindow) return;
        playerInputSequence.Add(id);
        
        for (int i = 0; i < playerInputSequence.Count; i++) {
            if (playerInputSequence[i] != correctSequence[i]) {
                DefenseFailed(); 
                playerInputSequence.Clear();
                return;
            }
        }

        if (playerInputSequence.Count == correctSequence.Count) {
            DefenseSuccess();
        }
    }

    public void OnIdentifyHit(bool isReal) {
        if (!isCooldown) return;
        // 播放辨識聲音
        AudioClip clipToPlay = isReal ? realUrsulaSound : fakeUrsulaSound;
        if (bossAudioSource != null && clipToPlay != null) {
            bossAudioSource.PlayOneShot(clipToPlay);
        }
    }

    public void OnBossAttacked(bool isRealHit) {
        if (!isCooldown) return;

        if (isRealHit) {
            bossHP--;
            Debug.Log("打中真身！剩餘血量：" + bossHP);
        } else {
            GameManager.Instance.PlayerTakeDamage(1);
            Debug.Log("打中分身！玩家扣血");
        }
        isCooldown = false; 
    }

    // --- 基礎邏輯 ---

    void DefenseSuccess() {
        isDefenseWindow = false;
        Debug.Log("成功防禦！");
    }

    void DefenseFailed() {
        isDefenseWindow = false;
        GameManager.Instance.PlayerTakeDamage(1);
        Debug.Log("防禦失敗或超時，玩家扣血");
    }

    void SwapPositions() {
        int rand = Random.Range(0, 2);
        realUrsula.transform.position = spawnPoints[rand].position;
        fakeUrsula.transform.position = spawnPoints[1 - rand].position;
        // 轉向對準玩家 (選用)
        // realUrsula.transform.LookAt(GameObject.FindGameObjectWithTag("Player").transform);
        // fakeUrsula.transform.LookAt(GameObject.FindGameObjectWithTag("Player").transform);
    }

    IEnumerator HandleVictorySequence() {
        Debug.Log("戰鬥結束，進入勝利序列！");

        realUrsula.SetActive(false);
        fakeUrsula.SetActive(false);

        if (cage != null) {
            cage.SetActive(false); 
            Debug.Log("籠子已打開！");
        }

        if (princeFish != null && princeHuman != null) {
        
            yield return new WaitForSeconds(0.5f); 
            
            princeFish.SetActive(false);
            princeHuman.SetActive(true); 
            
            Debug.Log("王子變回人類了！");
        }

        yield return new WaitForSeconds(1.0f);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (princeHuman != null && player != null) {
            // 這裡要對 princeHuman 做 LookAt 邏輯
            yield return StartCoroutine(RotateTowardsPlayer(princeHuman, player.transform));
        }

   
        Animator humanAnim = princeHuman.GetComponent<Animator>();
        if (humanAnim != null) {
            humanAnim.SetTrigger("doKiss");
        }
    }   

    IEnumerator RotateTowardsPlayer(GameObject actor, Transform target) {
        float duration = 1.0f;
        float elapsed = 0;
        Quaternion startRot = actor.transform.rotation;
        
        Vector3 dir = target.position - actor.transform.position;
        dir.y = 0;
        Quaternion endRot = Quaternion.LookRotation(dir);

        while (elapsed < duration) {
            actor.transform.rotation = Quaternion.Slerp(startRot, endRot, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        actor.transform.rotation = endRot;
    }
 }


public class UrsulaHitDetector : MonoBehaviour {
    public bool isReal;
    public UrsulaBossManager manager;

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("fork")) {
            manager.OnIdentifyHit(isReal);
        }
        else if (other.CompareTag("angelfork")) {
            manager.OnBossAttacked(isReal);
        }
    }
}