using UnityEngine;

public class CursorManager : MonoBehaviour
{
    [SerializeField] KeyCode toggleKey = KeyCode.Escape;

    void Start()
    {
        LockAndHide();
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            if (Cursor.lockState == CursorLockMode.Locked) UnlockAndShow();
            else LockAndHide();
        }
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && Cursor.lockState != CursorLockMode.Locked) LockAndHide();
    }

    void OnDisable() { UnlockAndShow(); }

    static void LockAndHide()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    static void UnlockAndShow()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}