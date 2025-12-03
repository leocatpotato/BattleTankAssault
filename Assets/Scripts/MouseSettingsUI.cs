using UnityEngine;
using UnityEngine.UI;

public class MouseSettingsUI : MonoBehaviour
{
    public Slider sensitivitySlider;

    private AimingController aiming;

    private void Start()
    {
        aiming = FindFirstObjectByType<AimingController>();

        float saved = PlayerPrefs.GetFloat("MouseSensitivity", 1.0f);
        if (sensitivitySlider != null)
        {
            sensitivitySlider.minValue = 0.5f;
            sensitivitySlider.maxValue = 5.0f;
            sensitivitySlider.value = saved;
        }

        if (aiming != null)
        {
            aiming.sensitivity = saved;
        }
    }

    public void OnSensitivityChanged(float value)
    {
        if (aiming == null)
            aiming = FindFirstObjectByType<AimingController>();

        if (aiming != null)
        {
            aiming.sensitivity = value;
        }

        PlayerPrefs.SetFloat("MouseSensitivity", value);
    }
}