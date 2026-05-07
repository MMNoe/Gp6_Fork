using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GatePanelManager : MonoBehaviour
{
    public static GatePanelManager Instance;

    [Header("Gate")]
    [SerializeField] private GameObject gate;

    [Header("Demo Music")]
    [SerializeField] private AudioClip   demoClip;
    [SerializeField] private AudioSource audioSource;

    [Header("Correct Sequence")]
    [Tooltip("BioInstrument ID 的順序，例如 [0,1,2,3]")]
    [SerializeField] private List<int> correctSequence;

    [Header("Gate Glow")]
    [SerializeField] private GameObject gateGlowing;
    [SerializeField] private GameObject gateGlowing1;
    [SerializeField] private float glowDuration = 1.5f;

    [Header("Gate Panels")]
    [SerializeField] private GameObject gatePanel;
    [SerializeField] private GameObject gatePanel1;
    [SerializeField] private Transform  gatePivot;
    [SerializeField] private Transform  gatePivot1;
    [SerializeField] private float      openDuration = 1.0f;
    [SerializeField] private float      openAngle    = -35f;
    [SerializeField] private float      openAngle1   = 330f;

    [Header("Lineup Layout")]
    [Tooltip("樂器排列的中心錨點。設定後忽略 Distance/Height，直接用此 Transform 的位置和朝向。")]
    [SerializeField] private Transform lineupAnchor;
    [SerializeField] private float lineupSpacing  = 0.8f;

    private readonly List<int> _playerSequence = new();
    private bool _lineupDone;
    private bool _solved;
    private Coroutine _glowCoroutine;
    private Coroutine _glowCoroutine1;

    void Awake()
    {
        Instance = this;
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (gateGlowing  != null) gateGlowing.SetActive(false);
        if (gateGlowing1 != null) gateGlowing1.SetActive(false);
    }

    void Start()
    {
        var followers = CreatureFollower.GetActiveFollowers();
        if (followers.Count > 0)
            TriggerLineup(followers);
        _lineupDone = true;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // Called by GatePanelInteractable when player hits a gate panel
    public void OnPanelHit()
    {
        Debug.Log("門被打了");
        if (_solved) return;

        if (audioSource != null && demoClip != null)
            audioSource.PlayOneShot(demoClip);

        TriggerGlow(gateGlowing,  ref _glowCoroutine);
        TriggerGlow(gateGlowing1, ref _glowCoroutine1);

        var followers = CreatureFollower.GetActiveFollowers();
        if (followers.Count > 0)
        {
            foreach (var f in followers) f.ShouldFollow = false;
            TriggerLineup(followers);
        }
    }

    // Called by BioInstrument when player hits an instrument
    public void RecordInstrumentHit(int id)
    {
        if (_solved || !_lineupDone) return;

        _playerSequence.Add(id);
        int idx = _playerSequence.Count - 1;

        if (idx >= correctSequence.Count || _playerSequence[idx] != correctSequence[idx])
        {
            _playerSequence.Clear();
            return;
        }

        if (_playerSequence.Count == correctSequence.Count)
        {
            _solved = true;
            OpenGate();
        }
    }

    private void TriggerLineup(List<CreatureFollower> followers)
    {
        Vector3 rowCenter, forward, right;

        if (lineupAnchor != null)
        {
            rowCenter = lineupAnchor.position;
            forward   = new Vector3(lineupAnchor.forward.x, 0f, lineupAnchor.forward.z).normalized;
        }
        else
        {
            Transform cam = Camera.main != null ? Camera.main.transform : null;
            Vector3 origin = cam != null ? cam.position : transform.position;
            forward = cam != null
                ? new Vector3(cam.forward.x, 0f, cam.forward.z).normalized
                : Vector3.forward;
            rowCenter = origin + forward * 1.0f;
        }

        right = new Vector3(forward.z, 0f, -forward.x);

        float totalWidth = (followers.Count - 1) * lineupSpacing;
        for (int i = 0; i < followers.Count; i++)
        {
            float   offset    = -totalWidth / 2f + i * lineupSpacing;
            Vector3 targetPos = rowCenter + right * offset;
            followers[i].LineUpAt(targetPos, forward);
        }
    }

    private void TriggerGlow(GameObject glowObj, ref Coroutine handle)
    {
        if (glowObj == null) return;
        if (handle != null) StopCoroutine(handle);
        handle = StartCoroutine(GlowOnce(glowObj));
    }

    private IEnumerator GlowOnce(GameObject glowObj)
    {
        glowObj.SetActive(true);
        if (glowObj.TryGetComponent<ParticleSystem>(out var ps)) ps.Play();
        yield return new WaitForSeconds(glowDuration);
        glowObj.SetActive(false);
    }

    private void OpenGate()
    {
        if (gatePivot  != null) StartCoroutine(RotateGate(gatePivot,  openAngle));
        if (gatePivot1 != null) StartCoroutine(RotateGate(gatePivot1, openAngle1));

        TriggerPanelOpen(gatePanel);
        TriggerPanelOpen(gatePanel1);
    }

    private void TriggerPanelOpen(GameObject panel)
    {
        if (panel == null) return;
        if (panel.TryGetComponent<Animator>(out var anim))
            anim.SetTrigger("Open");
        else
            panel.SetActive(false);
    }

    private IEnumerator RotateGate(Transform pivot, float targetLocalY)
    {
        Quaternion startRot = pivot.localRotation;
        Vector3    euler    = startRot.eulerAngles;
        Quaternion endRot   = Quaternion.Euler(euler.x, targetLocalY, euler.z);
        float elapsed = 0f;
        while (elapsed < openDuration)
        {
            elapsed += Time.deltaTime;
            pivot.localRotation = Quaternion.Slerp(startRot, endRot, elapsed / openDuration);
            yield return null;
        }
        pivot.localRotation = endRot;
    }
}
