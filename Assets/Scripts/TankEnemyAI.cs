using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class TankEnemyAI : MonoBehaviour
{
    public EnemySensor sensor;
    public Transform turret;
    public Transform muzzle;
    public GameObject bulletPrefab;

    public float moveSpeed = 6f;
    public float rotateSpeed = 70f;
    public float keepDistance = 18f;
    public float fireInterval = 2.2f;
    public float fireRange = 50f;

    Rigidbody rb;
    float nextFire;

    void Awake() { rb = GetComponent<Rigidbody>(); if (!sensor) sensor = GetComponent<EnemySensor>(); }

    void FixedUpdate()
    {
        var tgt = sensor ? sensor.CurrentTarget : null;
        if (!tgt) { IdlePatrol(); return; }

        Vector3 to = tgt.position - transform.position; to.y = 0;
        float dist = to.magnitude;
        Vector3 dir = to.sqrMagnitude > 0.001f ? to.normalized : transform.forward;

        var desiredRot = Quaternion.LookRotation(dir, Vector3.up);
        rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, desiredRot, rotateSpeed * Time.fixedDeltaTime));

        if (dist > keepDistance + 2f) rb.MovePosition(rb.position + dir * (moveSpeed * Time.fixedDeltaTime));
        else if (dist < keepDistance - 2f) rb.MovePosition(rb.position - dir * (moveSpeed * 0.6f * Time.fixedDeltaTime));

        if (turret)
        {
            Vector3 tdir = tgt.position - turret.position; tdir.y = 0;
            if (tdir.sqrMagnitude > 0.001f)
            {
                var aimWorld = Quaternion.LookRotation(tdir, Vector3.up);
                var local = Quaternion.Inverse(transform.rotation) * aimWorld;
                turret.localRotation = Quaternion.RotateTowards(turret.localRotation, local, 160f * Time.deltaTime);
            }
        }

        if (dist <= fireRange && Time.time >= nextFire)
        {
            nextFire = Time.time + fireInterval + Random.Range(-0.4f, 0.4f);
            Shoot();
        }
    }

    void IdlePatrol()
    {
        rb.MoveRotation(rb.rotation * Quaternion.Euler(0, 20f * Time.fixedDeltaTime, 0));
    }

    void Shoot()
    {
        if (!bulletPrefab || !muzzle) return;
        var go = Instantiate(bulletPrefab, muzzle.position, muzzle.rotation);
        var rbB = go.GetComponent<Rigidbody>();
        if (rbB) rbB.linearVelocity = muzzle.forward * 60f;
    }
}