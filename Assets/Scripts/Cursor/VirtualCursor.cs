using UnityEngine;
using UnityEngine.UI;

public class VirtualCursor : MonoBehaviour
{
    public RectTransform reticle;
    public float sensitivity = 1.0f;
    public bool clampToScreen = true;
    public static Vector2 ScreenPos { get; private set; }

    void Start()
    {
        ScreenPos = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        if (reticle) reticle.position = ScreenPos;
    }

    void Update()
    {
        float dx = Input.GetAxis("Mouse X");
        float dy = Input.GetAxis("Mouse Y");

        var p = ScreenPos;
        p += new Vector2(dx, dy) * (sensitivity * 100f) * Time.deltaTime;

        if (clampToScreen)
        {
            p.x = Mathf.Clamp(p.x, 0f, Screen.width);
            p.y = Mathf.Clamp(p.y, 0f, Screen.height);
        }

        ScreenPos = p;
        if (reticle) reticle.position = ScreenPos;
    }

}