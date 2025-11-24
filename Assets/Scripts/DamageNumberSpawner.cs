using UnityEngine;
using UnityEngine.UI;

public class DamageNumberSpawner : MonoBehaviour
{
    [SerializeField] private GameObject damageNumberPrefab;
    [SerializeField] private Color damageColor = Color.red;
    [SerializeField] private Vector3 spawnOffset = new Vector3(0, 0.5f, 0);

    private Health health;
    private Canvas worldCanvas;

    // Logic Fix: Variable to track health before the hit
    private int _previousHealth;

    private void Awake()
    {
        health = GetComponent<Health>();

        FindOrCreateWorldCanvas();
    }

    private void Start()
    {
        if (health != null)
        {
            // Initialize previous health to prevent huge numbers on start
            // Assuming your Health script has a public currentHealth, otherwise use max
            // _previousHealth = health.currentHealth; 

            // For now, we subscribe to the event
            health.OnDamaged += OnDamaged;
        }
    }

    private void OnDestroy()
    {
        if (health != null)
        {
            health.OnDamaged -= OnDamaged;
        }
    }

    private void OnDamaged(int currentHP, int maxHP)
    {
        // LOGIC FIX: 
        // If this is the first hit, initialize _previousHealth to maxHP
        if (_previousHealth == 0) _previousHealth = maxHP;

        // Calculate damage based on the difference from the LAST frame
        int damage = _previousHealth - currentHP;

        // Update previous health for the next hit
        _previousHealth = currentHP;

        if (damage <= 0) return;

        SpawnDamageNumber(damage);
    }

    private void SpawnDamageNumber(int damage)
    {
        if (damageNumberPrefab == null || worldCanvas == null) return;

        Vector3 spawnPos = transform.position + spawnOffset;
        GameObject numberObj = Instantiate(damageNumberPrefab, spawnPos, Quaternion.identity, worldCanvas.transform);

        DamageNumbers damageNum = numberObj.GetComponent<DamageNumbers>();
        if (damageNum != null)
        {
            damageNum.Setup(damage, damageColor);
        }
    }

    private void FindOrCreateWorldCanvas()
    {
        // --- FIXED LINE BELOW ---
        // Original was: FindFirstObjectByTypeCanvas>(());
        worldCanvas = FindFirstObjectByType<Canvas>();
        // ------------------------

        // If we found a canvas, but it's a UI (Overlay) canvas, we shouldn't use it for world numbers.
        // We create a new one specifically for damage numbers.
        if (worldCanvas == null || worldCanvas.renderMode != RenderMode.WorldSpace)
        {
            GameObject canvasObj = new GameObject("WorldCanvas_DamageNumbers");
            worldCanvas = canvasObj.AddComponent<Canvas>();
            worldCanvas.renderMode = RenderMode.WorldSpace;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();

            worldCanvas.transform.position = Vector3.zero;
            // A size of 100x100 is very small for a canvas, 
            // usually you want it to match the scale of your world units.
            worldCanvas.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);
        }
    }
}