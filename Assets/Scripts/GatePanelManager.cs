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

    [Header("Lineup Layout")]
    [SerializeField] private float lineupDistance = 2.0f;
    [SerializeField] private float lineupSpacing  = 0.8f;
    [SerializeField] private float lineupHeight   = 0f;

    private readonly List<int> _playerSequence = new();
    private bool _lineupDone;
    private bool _solved;

    void Awake()
    {
        Instance = this;
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // Called by GatePanelInteractable when player hits a gate panel
    public void OnPanelHit()
    {
        if (_solved) return;

        if (audioSource != null && demoClip != null)
            audioSource.PlayOneShot(demoClip);

        if (!_lineupDone)
        {
            var followers = CreatureFollower.GetActiveFollowers();
            if (followers.Count > 0)
            {
                _lineupDone = true;
                TriggerLineup(followers);
            }
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
        Transform cam    = Camera.main != null ? Camera.main.transform : null;
        Vector3   origin = cam != null ? cam.position : transform.position;

        Vector3 forward = cam != null
            ? new Vector3(cam.forward.x, 0f, cam.forward.z).normalized
            : Vector3.forward;
        Vector3 right = new Vector3(forward.z, 0f, -forward.x);

        Vector3 rowCenter  = origin + forward * lineupDistance + Vector3.up * lineupHeight;
        float   totalWidth = (followers.Count - 1) * lineupSpacing;

        for (int i = 0; i < followers.Count; i++)
        {
            float   offset    = -totalWidth / 2f + i * lineupSpacing;
            Vector3 targetPos = rowCenter + right * offset;
            followers[i].LineUpAt(targetPos, forward);
        }
    }

    private void OpenGate()
    {
        if (gate == null) return;
        Animator anim = gate.GetComponent<Animator>();
        if (anim != null)
            anim.SetTrigger("Open");
        else
            gate.SetActive(false);
    }
}
