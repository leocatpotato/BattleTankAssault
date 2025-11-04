using UnityEngine;

public class AimingController : MonoBehaviour
{
    [Header("References")]
    public Transform turret;
    public Transform barrel;
    public Transform muzzle;
    public LayerMask aimMask;

    [Header("Angles & Dynamics")]
    public float maxPitchUp = 50f;
    public float maxPitchDown = -10f;
    public float yawSpeed = 120f;
    public float pitchSpeed = 80f;
    public float smoothing = 0.06f;

    [Header("Virtual Cursor")]
    public float sensitivity = 1.0f;
    public bool clampToScreen = true;
    public static Vector2 ScreenPos { get; private set; }

    Vector3 targetPos, aimVel;
    Vector2 screenPos;

    void Start()
    {
        targetPos = transform.position + transform.forward * 50f;
        screenPos = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
    }

    void Update()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            float dx = Input.GetAxisRaw("Mouse X");
            float dy = Input.GetAxisRaw("Mouse Y");
            screenPos += new Vector2(dx, dy) * (sensitivity * 100f) * Time.deltaTime;
        }
        else
        {
            screenPos = Input.mousePosition;
        }

        if (clampToScreen)
        {
            screenPos.x = Mathf.Clamp(screenPos.x, 0f, Screen.width);
            screenPos.y = Mathf.Clamp(screenPos.y, 0f, Screen.height);
        }

        Camera cam = Camera.main; if (!cam || !turret || !barrel) return;
        Ray ray = cam.ScreenPointToRay(screenPos);

        if (Physics.Raycast(ray, out RaycastHit hit, 2000f, aimMask))
            targetPos = Vector3.SmoothDamp(targetPos, hit.point, ref aimVel, smoothing);
        else
            targetPos = Vector3.SmoothDamp(targetPos, ray.GetPoint(100f), ref aimVel, smoothing);

        Vector3 dirOnTank = transform.InverseTransformPoint(targetPos);
        dirOnTank.y = 0f;
        if (dirOnTank.sqrMagnitude > 0.01f)
        {
            float yaw = Mathf.Atan2(dirOnTank.x, dirOnTank.z) * Mathf.Rad2Deg;
            Quaternion q = Quaternion.Euler(0f, yaw, 0f);
            turret.localRotation = Quaternion.RotateTowards(turret.localRotation, q, yawSpeed * Time.deltaTime);
        }

        Vector3 localDir = turret.InverseTransformPoint(targetPos);
        float pitch = Mathf.Atan2(localDir.y, localDir.z) * Mathf.Rad2Deg;
        pitch = Mathf.Clamp(pitch, maxPitchDown, maxPitchUp);
        Quaternion qp = Quaternion.Euler(-pitch, 0f, 0f);
        barrel.localRotation = Quaternion.RotateTowards(barrel.localRotation, qp, pitchSpeed * Time.deltaTime);

        ScreenPos = screenPos;
    }
}