using UnityEngine;

public class TurretAimMouse : MonoBehaviour
{
    public Transform tankRoot;
    public LayerMask groundMask;
    public float rotateSpeed = 240f;
    public float maxPitchUp = 50f, maxPitchDown = -10f;
    public float aimDistance = 60f;
    public float minAimRadius = 4f;

    public float holdAngle = 1.2f;
    public float releaseAngle = 3.0f;

    public static Vector3 AimPointWS { get; private set; }
    Vector3 _aimVel;
    bool _locked;

    void Update()
    {
        if (!tankRoot) return;

        Ray ray = Camera.main.ScreenPointToRay(VirtualCursor.ScreenPos);
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

        Vector3 flat = rawPoint - tankRoot.position; flat.y = 0f;
        if (flat.sqrMagnitude > 1e-6f && flat.magnitude < minAimRadius)
            rawPoint = tankRoot.position + flat.normalized * minAimRadius;

        AimPointWS = Vector3.SmoothDamp(AimPointWS == default ? rawPoint : AimPointWS,
                                        rawPoint, ref _aimVel, 0.06f);

        Vector3 local = tankRoot.InverseTransformPoint(AimPointWS); local.y = 0f;
        if (local.sqrMagnitude < 1e-6f) return;

        float targetYaw = Mathf.Atan2(local.x, local.z) * Mathf.Rad2Deg;
        float curYaw = transform.localEulerAngles.y; if (curYaw > 180) curYaw -= 360;
        float err = Mathf.DeltaAngle(curYaw, targetYaw);

        if (_locked)
        {
            if (Mathf.Abs(err) >= releaseAngle) _locked = false;
            else { transform.localRotation = Quaternion.Euler(0f, targetYaw, 0f); return; }
        }
        else if (Mathf.Abs(err) <= holdAngle)
        {
            _locked = true;
            transform.localRotation = Quaternion.Euler(0f, targetYaw, 0f);
            return;
        }

        transform.localRotation = Quaternion.RotateTowards(
            transform.localRotation,
            Quaternion.Euler(0f, targetYaw, 0f),
            rotateSpeed * Time.deltaTime
        );
    }
}
