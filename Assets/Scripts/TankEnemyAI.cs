using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(EnemySensor))]
public class TankEnemyAI : MonoBehaviour
{
    public Transform turret;
    public Transform muzzle;
    public GameObject bulletPrefab;

    [Header("Waypoints / Patrol")]
    public Transform[] waypoints;
    public Transform waypointGroup;
    public bool loop = true;
    public float waypointStopDist = 1.2f;
    public float pauseAtWaypoint = 0.8f;

    [Header("Movement")]
    public float patrolSpeed = 4f;
    public float chaseSpeed = 6f;
    public float rotateSpeed = 70f;

    [Header("Combat")]
    public float keepDistance = 18f;
    public float fireRange = 50f;
    public float fireInterval = 2.2f;
    public float turretYawSpeed = 160f;

    [Header("Search / Return")]
    public float lookAroundTime = 3.5f;
    public float returnSpeed = 5f;

    EnemySensor sensor;
    Rigidbody rb;

    enum State { Patrol, Chase, Combat, LookAround, Return }
    State state = State.Patrol;

    int wpIndex = 0;
    float pauseTimer = 0f;
    float nextFire = 0f;

    Vector3 lastSeenPos;
    float lookTimer = 0f;
    float lookYaw = 0f;

    public void SetWaypointGroup(Transform group)
    {
        waypointGroup = group;
        AutoBindWaypoints();
    }

    public void SetWaypoints(Transform[] list)
    {
        waypoints = list;
        wpIndex = FindNearestWP();
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        sensor = GetComponent<EnemySensor>();
    }

    void OnEnable()
    {
        if ((waypoints == null || waypoints.Length == 0) && waypointGroup)
            AutoBindWaypoints();
        if (waypoints != null && waypoints.Length > 0)
            wpIndex = FindNearestWP();
    }

    void OnValidate()
    {
        if (waypointGroup) AutoBindWaypoints();
    }

    void Update()
    {
        var tgt = sensor ? sensor.CurrentTarget : null;

        switch (state)
        {
            case State.Patrol:
                if (tgt) { state = State.Chase; lastSeenPos = tgt.position; }
                break;

            case State.Chase:
                if (!tgt) { state = State.LookAround; lookTimer = 0f; break; }
                lastSeenPos = tgt.position;
                if (DistXZ(transform.position, tgt.position) <= keepDistance + 1.5f) state = State.Combat;
                break;

            case State.Combat:
                if (!tgt) { state = State.LookAround; lookTimer = 0f; break; }
                lastSeenPos = tgt.position;
                float d = DistXZ(transform.position, tgt.position);
                if (d > keepDistance + 3f) state = State.Chase;
                break;

            case State.LookAround:
                lookTimer += Time.deltaTime;
                if (tgt) { state = State.Chase; break; }
                if (lookTimer >= lookAroundTime) { state = State.Return; }
                break;

            case State.Return:
                if (tgt) { state = State.Chase; break; }
                if (ArrivedXZ(transform.position, WaypointPos(wpIndex), waypointStopDist))
                {
                    state = State.Patrol; pauseTimer = 0f;
                }
                break;
        }
    }

    void FixedUpdate()
    {
        switch (state)
        {
            case State.Patrol: DoPatrol(); break;
            case State.Chase: DoChase(); break;
            case State.Combat: DoCombat(); break;
            case State.LookAround: DoLookAround(); break;
            case State.Return: DoReturn(); break;
        }
    }

    void AutoBindWaypoints()
    {
        if (!waypointGroup) return;
        int n = waypointGroup.childCount;
        if (n <= 0) return;
        waypoints = new Transform[n];
        for (int i = 0; i < n; i++) waypoints[i] = waypointGroup.GetChild(i);
        wpIndex = FindNearestWP();
    }

    void DoPatrol()
    {
        if (waypoints == null || waypoints.Length == 0) { IdleScan(); return; }

        Vector3 wp = WaypointPos(wpIndex);
        if (ArrivedXZ(transform.position, wp, waypointStopDist))
        {
            pauseTimer += Time.fixedDeltaTime;
            if (pauseTimer >= pauseAtWaypoint)
            {
                pauseTimer = 0f;
                wpIndex = NextWP(wpIndex);
            }
            FaceTowards(wp, rotateSpeed);
            return;
        }

        MoveTowards(wp, patrolSpeed, rotateSpeed);
        AimTurretForward();
    }

    void DoChase()
    {
        var tgt = sensor.CurrentTarget; if (!tgt) return;
        Vector3 tp = tgt.position;
        float d = DistXZ(transform.position, tp);

        if (d > keepDistance + 1.5f) MoveTowards(tp, chaseSpeed, rotateSpeed);
        else FaceTowards(tp, rotateSpeed);

        AimTurretAt(tp);
        TryFire(tp, d);
    }

    void DoCombat()
    {
        var tgt = sensor.CurrentTarget; if (!tgt) return;
        Vector3 tp = tgt.position;
        float d = DistXZ(transform.position, tp);

        if (d > keepDistance + 0.8f) MoveTowards(tp, chaseSpeed * 0.6f, rotateSpeed);
        else if (d < keepDistance - 0.8f) MoveTowardsBack(tp, chaseSpeed * 0.4f, rotateSpeed);
        else FaceTowards(tp, rotateSpeed);

        AimTurretAt(tp);
        TryFire(tp, d);
    }

    void DoLookAround()
    {
        if (!ArrivedXZ(transform.position, lastSeenPos, 0.8f))
            MoveTowards(lastSeenPos, returnSpeed, rotateSpeed);
        else
        {
            lookYaw = Mathf.Sin(Time.time * 1.8f) * 60f;
            rb.MoveRotation(Quaternion.RotateTowards(rb.rotation,
                Quaternion.Euler(0f, transform.eulerAngles.y + lookYaw, 0f),
                rotateSpeed * Time.fixedDeltaTime));
        }
        AimTurretForward();
    }

    void DoReturn()
    {
        Vector3 wp = WaypointPos(wpIndex);
        MoveTowards(wp, returnSpeed, rotateSpeed);
        AimTurretForward();
    }

    void MoveTowards(Vector3 world, float speed, float rotSpd)
    {
        Vector3 to = world - transform.position; to.y = 0f;
        if (to.sqrMagnitude < 1e-4f) return;
        Quaternion look = Quaternion.LookRotation(to.normalized, Vector3.up);
        rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, look, rotSpd * Time.fixedDeltaTime));
        rb.MovePosition(rb.position + transform.forward * (speed * Time.fixedDeltaTime));
    }

    void MoveTowardsBack(Vector3 world, float speed, float rotSpd)
    {
        Vector3 to = world - transform.position; to.y = 0f;
        if (to.sqrMagnitude < 1e-4f) return;
        Quaternion look = Quaternion.LookRotation(to.normalized, Vector3.up);
        rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, look, rotSpd * Time.fixedDeltaTime));
        rb.MovePosition(rb.position - transform.forward * (speed * Time.fixedDeltaTime));
    }

    void FaceTowards(Vector3 world, float rotSpd)
    {
        Vector3 to = world - transform.position; to.y = 0f;
        if (to.sqrMagnitude < 1e-4f) return;
        Quaternion look = Quaternion.LookRotation(to.normalized, Vector3.up);
        rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, look, rotSpd * Time.fixedDeltaTime));
    }

    void IdleScan()
    {
        float yaw = Mathf.Sin(Time.time * 0.8f) * 45f;
        Quaternion look = Quaternion.Euler(0f, transform.eulerAngles.y + yaw, 0f);
        rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, look, rotateSpeed * 0.5f * Time.fixedDeltaTime));
        AimTurretForward();
    }

    void AimTurretAt(Vector3 worldTarget)
    {
        if (!turret) return;
        Vector3 d = worldTarget - turret.position; d.y = 0f;
        if (d.sqrMagnitude < 1e-4f) return;
        Quaternion aim = Quaternion.LookRotation(d.normalized, Vector3.up);
        Quaternion local = Quaternion.Inverse(transform.rotation) * aim;
        turret.localRotation = Quaternion.RotateTowards(turret.localRotation, local, turretYawSpeed * Time.deltaTime);
    }

    void AimTurretForward()
    {
        if (!turret) return;
        Quaternion localForward = Quaternion.identity;
        turret.localRotation = Quaternion.RotateTowards(turret.localRotation, localForward, turretYawSpeed * Time.deltaTime);
    }

    void TryFire(Vector3 tp, float dist)
    {
        if (!muzzle || !bulletPrefab) return;
        if (dist > fireRange) return;
        if (Time.time < nextFire) return;

        nextFire = Time.time + fireInterval;
        var go = Instantiate(bulletPrefab, muzzle.position, muzzle.rotation);
        var rbB = go.GetComponent<Rigidbody>();
        if (rbB) rbB.linearVelocity = muzzle.forward * 60f;
    }

    int FindNearestWP()
    {
        if (waypoints == null || waypoints.Length == 0) return 0;
        int idx = 0; float best = float.MaxValue;
        for (int i = 0; i < waypoints.Length; i++)
        {
            float d = DistXZ(transform.position, WaypointPos(i));
            if (d < best) { best = d; idx = i; }
        }
        return idx;
    }

    int NextWP(int i)
    {
        if (waypoints == null || waypoints.Length == 0) return 0;
        int n = i + 1;
        if (n >= waypoints.Length) n = loop ? 0 : waypoints.Length - 1;
        return n;
    }

    Vector3 WaypointPos(int i) => waypoints[i] ? waypoints[i].position : transform.position;
    static float DistXZ(Vector3 a, Vector3 b) { a.y = 0; b.y = 0; return Vector3.Distance(a, b); }
    static bool ArrivedXZ(Vector3 a, Vector3 b, float eps) => DistXZ(a, b) <= eps;

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (waypoints == null) return;
        Gizmos.color = Color.cyan;
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (!waypoints[i]) continue;
            Gizmos.DrawWireSphere(waypoints[i].position, 0.5f);
            if (i + 1 < waypoints.Length && waypoints[i + 1])
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
            else if (loop && waypoints.Length > 1 && waypoints[0])
                Gizmos.DrawLine(waypoints[i].position, waypoints[0].position);
        }
    }
#endif
}