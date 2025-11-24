using UnityEngine;

public class SimpleHealthBar : MonoBehaviour
{
    [SerializeField] private Health health;
    [SerializeField] private Vector3 offset = new Vector3(0f, 0.6f, 0f);
    [SerializeField] private Vector2 barSize = new Vector2(0.5f, 0.08f);
    [SerializeField] private Color fullColor = Color.green;
    [SerializeField] private Color lowColor = Color.red;

    private GameObject bgBar;
    private GameObject fillBar;
    private Transform target;

    private void Start()
    {
        if (health == null)
            health = GetComponentInParent<Health>();

        if (health == null) return;

        target = health.transform;
        CreateBar();
        
        health.OnDamaged += UpdateHealth;
        health.OnDied += OnDied;
        
        UpdateHealth(health.CurrentHP, health.GetMaxHP());
    }

    private void CreateBar()
    {
        // Background
        bgBar = GameObject.CreatePrimitive(PrimitiveType.Quad);
        bgBar.name = "HealthBG";
        bgBar.transform.SetParent(transform);
        bgBar.transform.localPosition = Vector3.zero;
        bgBar.transform.localScale = new Vector3(barSize.x, barSize.y, 1f);
        Destroy(bgBar.GetComponent<Collider>());
        var bgRenderer = bgBar.GetComponent<MeshRenderer>();
        bgRenderer.material = new Material(Shader.Find("Sprites/Default"));
        bgRenderer.material.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        bgRenderer.sortingOrder = 100;

        // Fill
        fillBar = GameObject.CreatePrimitive(PrimitiveType.Quad);
        fillBar.name = "HealthFill";
        fillBar.transform.SetParent(transform);
        fillBar.transform.localPosition = Vector3.zero;
        fillBar.transform.localScale = new Vector3(barSize.x, barSize.y, 1f);
        Destroy(fillBar.GetComponent<Collider>());
        var fillRenderer = fillBar.GetComponent<MeshRenderer>();
        fillRenderer.material = new Material(Shader.Find("Sprites/Default"));
        fillRenderer.material.color = fullColor;
        fillRenderer.sortingOrder = 101;
    }

    private void LateUpdate()
    {
        if (target != null)
        {
            transform.position = target.position + offset;
            transform.rotation = Quaternion.identity;
        }
    }

    private void UpdateHealth(int current, int max)
    {
        if (fillBar == null) return;

        float percent = (float)current / Mathf.Max(1, max);
        
        // Scale the fill bar
        fillBar.transform.localScale = new Vector3(barSize.x * percent, barSize.y, 1f);
        
        // Move to align left
        float offsetX = -barSize.x * (1f - percent) * 0.5f;
        fillBar.transform.localPosition = new Vector3(offsetX, 0f, -0.01f);
        
        // Color lerp
        var renderer = fillBar.GetComponent<MeshRenderer>();
        renderer.material.color = Color.Lerp(lowColor, fullColor, percent / 0.3f);
    }

    private void OnDied()
    {
        if (fillBar != null)
            fillBar.SetActive(false);
    }

    private void OnDestroy()
    {
        if (health != null)
        {
            health.OnDamaged -= UpdateHealth;
            health.OnDied -= OnDied;
        }
    }
}
