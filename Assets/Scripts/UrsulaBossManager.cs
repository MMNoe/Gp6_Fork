
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

    [Header("粒子效果 — 真烏蘇拉")]
    public ParticleSystem realHitParticle;    // 真烏蘇扣血
    public ParticleSystem realWindParticle;   // 真烏蘇旋風（朝向玩家，自動旋轉）
    public ParticleSystem realSpellParticle;  // 真烏蘇發威（位置旋轉綁定烏蘇拉）

    [Header("粒子效果 — 假烏蘇拉")]
    public ParticleSystem fakeHitParticle;    // 假蘇拉扣血
    public ParticleSystem fakeWindParticle;   // 假蘇拉旋風（朝向玩家，自動旋轉）
    public ParticleSystem fakeSpellParticle;  // 假蘇拉發威（位置旋轉綁定烏蘇拉）

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
        if (obj == null) { Debug.LogError($"SetupHitDetection: obj is null (isReal={isReal})"); return; }
        Debug.Log($"SetupHitDetection on: {obj.name} (isReal={isReal})");

        var detector = obj.GetComponent<UrsulaHitDetector>();
        if (detector == null) detector = obj.AddComponent<UrsulaHitDetector>();
        Debug.Log($"UrsulaHitDetector added to: {detector.gameObject.name}");

        detector.isReal = isReal;
        detector.manager = this;
        detector.hitParticle = isReal ? realHitParticle : fakeHitParticle;
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
            StartSpellEffect();
            while (isDefenseWindow && timer < 3f) {
                timer += Time.deltaTime;
                yield return null;
                Debug.Log("正在唱");
            }

            StopSpellEffect();
            if (isDefenseWindow) {
                DefenseFailed(); // 時間內沒打
            }
            bossAudioSource.Stop();

            if (bossHP <= 0) break; // 防禦成功扣完血 → 直接進勝利序列

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

    // --- 施法特效 ---

    // Public so TestBossSpellButton can call it directly.
    // In the real scene this is called automatically by BossLoop.
    public void StartSpellEffect() {
        // Falls back to the main camera when no Player-tagged object exists (e.g. test scene).
        Transform target = null;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            target = player.transform;
        else if (Camera.main != null)
            target = Camera.main.transform;

        PlayWindParticle(realWindParticle, target);
        PlayWindParticle(fakeWindParticle, target);

        if (realSpellParticle != null) realSpellParticle.Play();
        if (fakeSpellParticle != null) fakeSpellParticle.Play();
    }

    void PlayWindParticle(ParticleSystem ps, Transform target) {
        if (ps == null) return;
        if (target != null) {
            Vector3 dir = (target.position - ps.transform.position).normalized;
            if (dir != Vector3.zero)
                ps.transform.rotation = Quaternion.LookRotation(dir);
        }
        ps.Play();
    }

    void StopSpellEffect() {
        if (realWindParticle != null) realWindParticle.Stop();
        if (fakeWindParticle != null) fakeWindParticle.Stop();
        if (realSpellParticle != null) realSpellParticle.Stop();
        if (fakeSpellParticle != null) fakeSpellParticle.Stop();
    }

    // --- 基礎邏輯 ---

    void DefenseSuccess() {
        isDefenseWindow = false;
        bossHP--;
        Debug.Log("成功防禦！Boss HP 剩：" + bossHP);
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

        Debug.Log($"[Victory] princeFish={(princeFish == null ? "NULL" : princeFish.name)}, princeHuman={(princeHuman == null ? "NULL" : princeHuman.name)}");
        if (princeFish != null ) {
            Debug.Log("有魚！");
            if ( princeHuman != null){
        
            yield return new WaitForSeconds(0.5f); 
            
            princeFish.SetActive(false);
            princeHuman.SetActive(true); 
            
            Debug.Log("王子變回人類了！");
            }
        }

        yield return new WaitForSeconds(1.0f);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (princeHuman != null && player != null) {
            // 這裡要對 princeHuman 做 LookAt 邏輯
            yield return StartCoroutine(RotateTowardsPlayer(princeHuman, player.transform));
        }

   
        princeHuman?.GetComponent<Animator>()?.SetTrigger("doKiss");
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
    public ParticleSystem hitParticle;  // set by UrsulaBossManager.SetupHitDetection

    private void OnTriggerEnter(Collider other) {
        Debug.Log("有了 ");
        if (other.CompareTag("fork")) {
            manager.OnIdentifyHit(isReal);
            Debug.Log("被打了");
        }
        else if (other.CompareTag("angelfork")) {
            if (hitParticle != null) hitParticle.Play();
            manager.OnBossAttacked(isReal);
            Debug.Log("被天使打了");
        }
    }
}