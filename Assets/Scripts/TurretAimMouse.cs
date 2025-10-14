using UnityEngine;

public class TurretAimMouse : MonoBehaviour
{
    public Transform tankRoot;
    public LayerMask groundMask;
    public float rotateSpeed = 240f;
    public float maxPitchUp = 50f;
    public float maxPitchDown = -10f;
    public float aimDistance = 60f;
    public float minAimRadius = 4.0f;
    public float aimSmoothing = 0.06f;
    public float deadZoneDeg = 0.8f;

    public static Vector3 AimPointWS { get; private set; }
    Vector3 _vel;

    void Update()
    {
        if (!tankRoot) return;

        var cam = Camera.main;
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        Vector3 rawPoint;
        if (Physics.Raycast(ray, out var hit, 5000f, groundMask))
        {
            rawPoint = hit.point;
        }
        else
        {
            Vector3 horiz = Vector3.ProjectOnPlane(ray.direction, Vector3.up).normalized;
            if (horiz.sqrMagnitude < 1e-6f) horiz = tankRoot.forward;
            Vector3 right = Vector3.Cross(Vector3.up, horiz);
            float pitch = Vector3.SignedAngle(horiz, ray.direction, right);
            pitch = Mathf.Clamp(pitch, maxPitchDown, maxPitchUp);
            Vector3 clampedDir = Quaternion.AngleAxis(pitch, right) * horiz;
            rawPoint = tankRoot.position + clampedDir.normalized * aimDistance;
        }

        Vector3 v = rawPoint - tankRoot.position;
        Vector3 flat = new Vector3(v.x, 0, v.z);
        if (flat.sqrMagnitude < minAimRadius * minAimRadius && flat.sqrMagnitude > 1e-6f)
        {
            flat = flat.normalized * minAimRadius;
            rawPoint = tankRoot.position + flat;
        }


        AimPointWS = Vector3.SmoothDamp(AimPointWS == default ? rawPoint : AimPointWS,
                                        rawPoint, ref _vel, aimSmoothing);


        Vector3 local = tankRoot.InverseTransformPoint(AimPointWS); local.y = 0f;
        if (local.sqrMagnitude < 1e-6f) return;

        float targetYaw = Mathf.Atan2(local.x, local.z) * Mathf.Rad2Deg;
        float curYaw = transform.localEulerAngles.y; if (curYaw > 180) curYaw -= 360;
        float delta = Mathf.DeltaAngle(curYaw, targetYaw);
        if (Mathf.Abs(delta) < deadZoneDeg) return;

        Quaternion targetLocal = Quaternion.Euler(0f, targetYaw, 0f);
        transform.localRotation = Quaternion.RotateTowards(
            transform.localRotation, targetLocal, rotateSpeed * Time.deltaTime
        );
    }
}
