using UnityEngine;

public class Shooter : MonoBehaviour
{
    public Transform muzzle;
    public GameObject shellPrefab;
    public float muzzleSpeed = 60f;
    public float fireCD = 0.2f;
    public bool preferLowArc = true;
    public float minAimRadius = 4f;

    float nextFire;

    void Update()
    {
        if (!muzzle || !shellPrefab) return;

        if (Input.GetMouseButton(0) && Time.time >= nextFire)
        {
            nextFire = Time.time + fireCD;

            Vector3 aim = TurretAimMouse.AimPointWS;
            if (aim == default) aim = muzzle.position + muzzle.forward * 20f;

            Vector3 flat = aim - transform.position; flat.y = 0;
            if (flat.sqrMagnitude < minAimRadius * minAimRadius && flat.sqrMagnitude > 1e-6f)
                aim = transform.position + flat.normalized * minAimRadius;

            Vector3 vel;
            bool ok = SolveBallistic(muzzle.position, aim, muzzleSpeed, out vel, preferLowArc);
            if (!ok) vel = (aim - muzzle.position).normalized * muzzleSpeed;

            var go = Instantiate(shellPrefab, muzzle.position, Quaternion.LookRotation(vel));
            var rb = go.GetComponent<Rigidbody>();
            rb.linearVelocity = vel;
            rb.useGravity = true;
        }
    }


    bool SolveBallistic(Vector3 start, Vector3 target, float speed, out Vector3 vel, bool lowArc)
    {
        vel = Vector3.zero;
        Vector3 diff = target - start;
        Vector3 diffXZ = new Vector3(diff.x, 0f, diff.z);
        float x = diffXZ.magnitude;
        float y = diff.y;
        float g = Mathf.Abs(Physics.gravity.y);
        float s2 = speed * speed;

        float disc = s2 * s2 - g * (g * x * x + 2f * y * s2);
        if (disc < 0f || x < 1e-4f) return false;

        float sqrt = Mathf.Sqrt(disc);

        float tanTheta = lowArc ? (s2 - sqrt) / (g * x) : (s2 + sqrt) / (g * x);
        float cos = 1f / Mathf.Sqrt(1f + tanTheta * tanTheta);
        float sin = tanTheta * cos;

        Vector3 dirXZ = diffXZ.normalized;
        Vector3 vxz = dirXZ * (speed * cos);
        float vy = speed * sin;

        vel = vxz + Vector3.up * vy;
        return true;
    }
}
