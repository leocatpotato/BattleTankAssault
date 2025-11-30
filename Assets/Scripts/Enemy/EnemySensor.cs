using UnityEngine;

public class EnemySensor : MonoBehaviour
{
    public float detectRadius = 35f;
    public float loseRadius = 45f;
    public float fov = 140f;
    public LayerMask losMask;
    public Transform eye;

    public Transform CurrentTarget { get; private set; }

    void Reset() { eye = transform; }

    void Update()
    {
        if (CurrentTarget)
        {
            if (!HasLOS(CurrentTarget) && DistanceXZ(CurrentTarget.position) > loseRadius)
                CurrentTarget = null;
            return;
        }
        var player = FindClosestPlayer();
        if (player && CanSee(player)) CurrentTarget = player;
    }

    Transform FindClosestPlayer()
    {
        var players = GameObject.FindGameObjectsWithTag("Player");
        Transform best = null; float bestD = 1e9f;
        foreach (var p in players)
        {
            float d = DistanceXZ(p.transform.position);
            if (d < detectRadius && d < bestD) { bestD = d; best = p.transform; }
        }
        return best;
    }

    bool CanSee(Transform t)
    {
        Vector3 dir = (t.position - eye.position); dir.y = 0;
        if (dir.sqrMagnitude < 0.01f) return true;
        float ang = Vector3.Angle(eye.forward, dir);
        if (ang > fov * 0.5f) return false;
        return HasLOS(t);
    }

    bool HasLOS(Transform t)
    {
        Vector3 from = eye.position + Vector3.up * 0.2f;
        Vector3 to = t.position + Vector3.up * 0.6f;
        Vector3 dir = (to - from).normalized;
        float dist = Vector3.Distance(from, to);
        return !Physics.Raycast(from, dir, dist, losMask);
    }

    float DistanceXZ(Vector3 wpos)
    {
        var a = transform.position; a.y = 0; wpos.y = 0;
        return Vector3.Distance(a, wpos);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, detectRadius);
        Gizmos.color = Color.gray; Gizmos.DrawWireSphere(transform.position, loseRadius);
    }
}