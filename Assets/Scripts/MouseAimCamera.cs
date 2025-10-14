using UnityEngine;

public class MouseAimCamera : MonoBehaviour
{
    public Transform target;
    public float distance = 11f;
    public float height = 2.2f;
    public float sideOffset = 0f;
    public float posLerp = 10f, rotLerp = 14f;

    void LateUpdate()
    {
        if (!target) return;
        Vector3 aim = TurretAimMouse.AimPointWS;
        if (aim == default) aim = target.position + target.forward * 8f;

        Vector3 dir = aim - target.position; dir.y = 0f;
        if (dir.sqrMagnitude < 1e-4f) dir = target.forward;
        dir.Normalize();

        Vector3 right = Vector3.Cross(Vector3.up, dir);
        Vector3 desiredPos = target.position + Vector3.up * height - dir * distance + right * sideOffset;

        transform.position = Vector3.Lerp(transform.position, desiredPos, posLerp * Time.deltaTime);
        Quaternion lookRot = Quaternion.LookRotation((aim - transform.position).normalized, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, rotLerp * Time.deltaTime);
    }
}
