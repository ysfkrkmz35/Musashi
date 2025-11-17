using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HitboxDamage : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private int damage = 1;

    [Header("Filtering")]
    [Tooltip("Only colliders on these layers can be hit (e.g. Characters).")]
    [SerializeField] private LayerMask targetLayers = ~0; // default: everything
    [Tooltip("Optional: also require a tag on the target's ROOT. Leave empty to ignore tags.")]
    [SerializeField] private string targetRootTag = "";

    [Header("Owner")]
    [Tooltip("Used to avoid hitting yourself; if left null uses transform.root.")]
    [SerializeField] private Transform ownerRoot;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    private Collider2D col;
    private readonly HashSet<Health> hitThisSwing = new();

    private void Awake()
    {
        col = GetComponent<Collider2D>();
        col.isTrigger = true; // ensure trigger
        if (ownerRoot == null) ownerRoot = transform.root;
    }

    

    // Track previous enabled state to detect when we get re-enabled
    private bool wasEnabled = false;

    private void Update()
    {
        // Clear hit list whenever the collider transitions from disabled to enabled
        bool nowEnabled = col != null && col.enabled;
        if (nowEnabled && !wasEnabled)
        {
            hitThisSwing.Clear();
            if (debugLogs) Debug.Log($"{name} re-enabled, cleared hit list");
        }
        wasEnabled = nowEnabled;
    }
private void OnEnable() { hitThisSwing.Clear(); }
    private void OnDisable() { hitThisSwing.Clear(); }

private void OnTriggerEnter2D(Collider2D other)
    {
        if (!enabled)
        {
            if (debugLogs) Debug.Log($"{name} disabled, ignoring trigger");
            return;
        }

        if (debugLogs) Debug.Log($"{name} triggered by {other.name} (Layer: {other.gameObject.layer})");

        // Ignore self/root
        if (ownerRoot && other.transform.root == ownerRoot)
        {
            if (debugLogs) Debug.Log($"{name} ignoring self: {other.transform.root.name}");
            return;
        }

        // Layer filter
        int layerBit = 1 << other.gameObject.layer;
        if ((layerBit & targetLayers) == 0)
        {
            if (debugLogs) Debug.Log($"{name} layer mismatch: {other.gameObject.layer} not in targetLayers {targetLayers.value}");
            return;
        }

        // Find Health on the other side (anywhere up the hierarchy)
        var health = other.GetComponentInParent<Health>();
        if (health == null)
        {
            if (debugLogs) Debug.Log($"{name} no Health component found on {other.name}");
            return;
        }

        // Optional root tag filter
        if (!string.IsNullOrEmpty(targetRootTag) && !other.transform.root.CompareTag(targetRootTag))
        {
            if (debugLogs) Debug.Log($"{name} tag mismatch: {other.transform.root.tag} != {targetRootTag}");
            return;
        }

        // Avoid multi-hit on the same target for this swing
        if (!hitThisSwing.Add(health))
        {
            if (debugLogs) Debug.Log($"{name} already hit {other.transform.root.name} this swing");
            return;
        }

        health.TakeDamage(damage);
        if (debugLogs) Debug.Log($"✓ {name} hit {other.transform.root.name} for {damage} damage!", this);
    }
}
