using UnityEngine;

public class Shooter : MonoBehaviour
{
    [Header("Shooting")]
    public Transform muzzle;
    public GameObject shellPrefab;
    public float fireCD = 0.25f;
    public float muzzleSpeed = 60f;

    [Header("Audio")]
    public AudioSource fireAudio;

    float nextFire;

    void Awake()
    {
        if (!fireAudio && muzzle != null)
        {
            fireAudio = muzzle.GetComponent<AudioSource>();
        }
    }

    void Update()
    {
        if (!muzzle || !shellPrefab) return;

        if (Input.GetMouseButton(0) && Time.time >= nextFire)
        {
            nextFire = Time.time + fireCD;

            GameObject shellObj = Instantiate(shellPrefab, muzzle.position, muzzle.rotation);

            Rigidbody rb = shellObj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = muzzle.forward * muzzleSpeed;
            }

            Shell shell = shellObj.GetComponent<Shell>();
            if (shell != null)
            {
                shell.Init(false, transform.root);
            }

            if (fireAudio && fireAudio.clip != null)
            {
                fireAudio.PlayOneShot(fireAudio.clip);
            }
        }
    }
}