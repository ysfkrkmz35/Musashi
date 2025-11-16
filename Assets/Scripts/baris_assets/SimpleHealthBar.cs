using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple health bar that follows a character and updates automatically
/// </summary>
public class SimpleHealthBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Health health;
    [SerializeField] private Image fillImage;
    [SerializeField] private CanvasGroup canvasGroup;
    
    [Header("Settings")]
    [SerializeField] private Vector3 offset = new Vector3(0, 0.6f, 0);
    [SerializeField] private Color fullHealthColor = Color.green;
    [SerializeField] private Color lowHealthColor = Color.red;
    [SerializeField] private float lowHealthThreshold = 0.3f;
    [SerializeField] private bool hideWhenFull = false;
    [SerializeField] private bool hideWhenDead = true;

    private Transform target;
    private Camera mainCam;

    private void Awake()
    {
        mainCam = Camera.main;
        
        if (health == null)
            health = GetComponentInParent<Health>();
            
        if (health != null)
        {
            target = health.transform;
            health.OnDamaged += OnHealthChanged;
            health.OnDied += OnDied;
        }

        UpdateHealthBar();
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
        if (target != null && mainCam != null)
        {
            // Follow target with offset
            Vector3 worldPos = target.position + offset;
            transform.position = worldPos;
            
            // Face camera
            transform.rotation = mainCam.transform.rotation;
        }
    }

    private void OnHealthChanged(int current, int max)
    {
        UpdateHealthBar();
    }

    private void OnDied()
    {
        if (hideWhenDead && canvasGroup != null)
            canvasGroup.alpha = 0f;
    }

    private void UpdateHealthBar()
    {
        if (health == null || fillImage == null) return;

        int current = health.CurrentHP;
        int max = Mathf.Max(1, 5); // Assuming max for visual purposes
        float fillAmount = Mathf.Clamp01((float)current / max);

        fillImage.fillAmount = fillAmount;

        // Color gradient
        fillImage.color = Color.Lerp(lowHealthColor, fullHealthColor, 
            fillAmount / lowHealthThreshold);

        // Hide when full
        if (hideWhenFull && canvasGroup != null)
        {
            canvasGroup.alpha = fillAmount >= 1f ? 0f : 1f;
        }
    }
}
