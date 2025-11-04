using UnityEngine;

public class ReticleFollower : MonoBehaviour
{
    public Canvas canvas;
    RectTransform rt;

    void Awake()
    {
        rt = (RectTransform)transform;
        if (!canvas) canvas = GetComponentInParent<Canvas>();
    }

    void LateUpdate()
    {
        Vector2 sp = AimingController.ScreenPos;

        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            rt.position = sp;
        }
        else
        {
            RectTransform canvasRect = (RectTransform)canvas.transform;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, sp, canvas.worldCamera, out var local);
            rt.localPosition = local;
        }
    }
}
