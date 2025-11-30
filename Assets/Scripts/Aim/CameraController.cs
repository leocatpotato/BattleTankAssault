using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    public float distance = 12f;
    public float height = 3f;
    public float sideOffset = 0f;
    public float zoomSpeed = 5f;
    public float minDistance = 6f;
    public float maxDistance = 20f;
    public float lerpSpeed = 10f;

    void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        distance = Mathf.Clamp(distance - scroll * zoomSpeed, minDistance, maxDistance);
    }

    void LateUpdate()
    {
        if (!target) return;

        Vector3 aimPos = AimingController_FindAim();
        Vector3 dir = (aimPos - target.position);
        dir.y = 0;
        if (dir.sqrMagnitude < 0.1f) dir = target.forward;
        dir.Normalize();

        Vector3 right = Vector3.Cross(Vector3.up, dir);
        Vector3 desired = target.position + Vector3.up * height - dir * distance + right * sideOffset;

        transform.position = Vector3.Lerp(transform.position, desired, lerpSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation,
            Quaternion.LookRotation(aimPos - transform.position, Vector3.up),
            lerpSpeed * Time.deltaTime);
    }

    Vector3 AimingController_FindAim()
    {
        var aim = AimingController_Find();
        if (aim != null) return aim.transform.position + aim.transform.forward * 40f;
        return target.position + target.forward * 40f;
    }

    AimingController AimingController_Find()
    {
        return target ? target.GetComponent<AimingController>() : null;
    }
}