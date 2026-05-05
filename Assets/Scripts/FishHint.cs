using UnityEngine;
using System.Collections;


public class FishHint : MonoBehaviour
{
    public static FishHint Instance;


    [Header("移動參數")]
    public float moveSpeed = 3f;
    public float rotateSpeed = 5f;


    [Header("組件")]
    public AudioSource audioSource;
    private Animator anim;
    private bool isWorking = false;


    void Awake()
    {
        Instance = this;
    }


    void Start()
    {
        //anim = GetComponent<Animator>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        gameObject.SetActive(false);
    }


    public void StartHint(Transform spawn, Transform target, AudioClip clip)
    {
        gameObject.SetActive(true);
        if (isWorking) return;
        Debug.Log("in fish");
        
        StartCoroutine(HintSequence(spawn, target, clip));
    }


    IEnumerator HintSequence(Transform spawn, Transform target, AudioClip clip)
    {
        isWorking = true;
       
        transform.position = spawn.position;
        transform.rotation = spawn.rotation;
        //gameObject.SetActive(true);


        //if (anim != null) anim.SetBool("isSwimming", true);


        while (Vector3.Distance(transform.position, target.position) > 0.3f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);
           
            Vector3 dir = target.position - transform.position;
            if (dir != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotateSpeed * Time.deltaTime);
            }
            yield return null;
        }


        //if (anim != null) anim.SetBool("isSwimming", false);


        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Vector3 toPlayer = player.transform.position - transform.position;
            toPlayer.y = 0;
            transform.rotation = Quaternion.LookRotation(toPlayer);
        }


        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
            yield return new WaitForSeconds(clip.length + 2f);
        }


        gameObject.SetActive(false);
        isWorking = false;
    }
}