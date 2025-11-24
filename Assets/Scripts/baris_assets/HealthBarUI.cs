using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    private Health health;
    private Slider slider;
    private Text label;
    private CanvasGroup canvasGroup;
    private Transform followTarget;
    private Vector3 worldOffset;

    private Color fullColor = Color.green;
    private Color lowColor = Color.red;
    private float lowThreshold = 0.3f;
    private bool hideWhenFull = false;
    private bool hideWhenDead = true;

    private Camera mainCam;

    public void Initialize(Health targetHealth, Slider targetSlider, Text targetLabel, CanvasGroup group, Transform follow, Vector3 offset)
    {
        health = targetHealth;
        slider = targetSlider;
        label = targetLabel;
        canvasGroup = group;
        followTarget = follow;
        worldOffset = offset;
        mainCam = Camera.main;

        if (health != null)
        {
            health.OnDamaged += OnHealthChanged;
            health.OnDied += OnDied;
        }

        UpdateDisplay(health.CurrentHP, health.GetMaxHP());
    }

    public void SetColors(Color full, Color low, float threshold)
    {
        fullColor = full;
        lowColor = low;
        lowThreshold = threshold;
    }

    public void SetVisibilityOptions(bool hideFull, bool hideDead)
    {
        hideWhenFull = hideFull;
        hideWhenDead = hideDead;
    }

    public void SetFollowTarget(Transform target, Vector3 offset)
    {
        followTarget = target;
        worldOffset = offset;
    }

    private void OnDestroy()
    {
        if (health != null)
        {
            health.OnDamaged -= OnHealthChanged;
            health.OnDied -= OnDied;
        }
    }

    private void LateUpdate()
    {
        if (followTarget != null && mainCam != null)
        {
            Vector3 worldPos = followTarget.position + worldOffset;
            Vector3 screenPos = mainCam.WorldToScreenPoint(worldPos);
            transform.position = screenPos;
        }
    }

    private void OnHealthChanged(int current, int max)
    {
        UpdateDisplay(current, max);
    }

    private void UpdateDisplay(int current, int max)
    {
        if (slider != null)
        {
            slider.maxValue = max;
            slider.value = current;

            float healthPercent = (float)current / Mathf.Max(1, max);
            
            if (slider.fillRect != null)
            {
                var fillImage = slider.fillRect.GetComponent<Image>();
                if (fillImage != null)
                {
                    fillImage.color = Color.Lerp(lowColor, fullColor, healthPercent / lowThreshold);
                }
            }
        }

        if (label != null)
        {
            label.text = $"{current}/{max}";
        }

        if (hideWhenFull && canvasGroup != null)
        {
            float healthPercent = (float)current / Mathf.Max(1, max);
            canvasGroup.alpha = healthPercent >= 1f ? 0f : 1f;
        }
    }

    private void OnDied()
    {
        if (hideWhenDead && canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
    }
}
