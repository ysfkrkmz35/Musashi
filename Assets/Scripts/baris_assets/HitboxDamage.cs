using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HitboxDamage : MonoBehaviour
{
    [SerializeField] private int damage = 1;
    [SerializeField] private string targetTag = "Enemy"; // set "Player" on enemy weapon
    [Tooltip("Owner root used to avoid self-hits; left null -> uses transform.root at runtime.")]
    [SerializeField] private Transform ownerRoot;

    private Collider2D col;
    private readonly HashSet<Collider2D> hitThisSwing = new();

    private void Awake()
    {
        col = GetComponent<Collider2D>();
        if (ownerRoot == null) ownerRoot = transform.root;
        col.isTrigger = true;
    }

    private void OnEnable() { hitThisSwing.Clear(); }
    private void OnDisable() { hitThisSwing.Clear(); }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!enabled) return;
        if (hitThisSwing.Contains(other)) return;

        // ignore self / same root
        if (ownerRoot && other.transform.root == ownerRoot) return;

        // must match tag + have Health
        if (!other.CompareTag(targetTag)) return;

        var health = other.GetComponentInParent<Health>();
        if (health == null) return;

        health.TakeDamage(damage);
        hitThisSwing.Add(other);
    }
}
