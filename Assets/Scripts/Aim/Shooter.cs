using UnityEngine;

public class Shooter : MonoBehaviour
{
    public Transform muzzle;
    public GameObject shellPrefab;
    public float fireCD = 0.25f;
    public float muzzleSpeed = 60f;

    float nextFire;

    void Update()
    {
        if (!muzzle || !shellPrefab) return;
        if (Input.GetMouseButton(0) && Time.time >= nextFire)
        {
            nextFire = Time.time + fireCD;
            GameObject shell = Instantiate(shellPrefab, muzzle.position, muzzle.rotation);
            Rigidbody rb = shell.GetComponent<Rigidbody>();
            if (rb) rb.linearVelocity = muzzle.forward * muzzleSpeed;
        }
    }
}