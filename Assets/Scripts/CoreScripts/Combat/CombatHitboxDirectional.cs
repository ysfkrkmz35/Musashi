using UnityEngine;
using Musashi.Core.Combat;

/// <summary>
/// Enhanced CombatHitbox with directional damage
/// FIXED: Artık controller'lar damage'i handle ediyor, hitbox sadece collision detect ediyor
/// </summary>
public class CombatHitboxDirectional : MonoBehaviour
{
    public Team team;
    public float baseDamage = 20f;
    public bool isHeavyAttack = false;
    public Collider hitCollider;

    private DirectionalCombatSystem _combatSystem;
    private AttackDirection _currentDirection = AttackDirection.Up;

    void Awake()
    {
        if (hitCollider) hitCollider.enabled = false;

        // Get combat system from parent
        _combatSystem = GetComponentInParent<DirectionalCombatSystem>();
    }

    // Animator Event
    public void AttackStart()
    {
        if (hitCollider) hitCollider.enabled = true;

        // Capture current attack direction when attack starts
        if (_combatSystem != null)
        {
            _currentDirection = _combatSystem.CurrentAttackDirection;
        }

        Debug.Log($"[Hitbox] Attack started - Direction: {_currentDirection}, Heavy: {isHeavyAttack}, Team: {team}");
    }

    public void AttackEnd()
    {
        if (hitCollider) hitCollider.enabled = false;
        Debug.Log($"[Hitbox] Attack ended");
    }

    void OnTriggerEnter(Collider other)
    {
        // SIMPLIFIED: Hitbox doesn't apply damage anymore
        // Controllers handle all damage logic now
        // This is just for visual/audio feedback

        var h = other.GetComponentInParent<HealthB>();
        if (!h) return;

        // Prevent friendly fire
        if ((team == Team.Player && h.team == Team.Player) ||
            (team == Team.Enemy && h.team == Team.Enemy))
        {
            return;
        }

        Debug.Log($"[Hitbox] Contact detected: {team} hitbox touched {h.team} target");

        // Optional: Play hit sound/effect here
        // But don't apply damage - controllers already did that
    }

    /// <summary>
    /// Set attack type (called from controller before attacking)
    /// </summary>
    public void SetAttackType(bool heavy)
    {
        isHeavyAttack = heavy;
    }
}
