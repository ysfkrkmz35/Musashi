using UnityEngine;

public enum Team { Player, Enemy }

public class CombatHitbox : MonoBehaviour
{
    public Team team;
    public float damage = 20f;
    public Collider hitCollider; // isTrigger = true

    void Awake()
    {
        if (hitCollider) hitCollider.enabled = false;
    }

    // Animator Event
    public void AttackStart() { if (hitCollider) hitCollider.enabled = true; }
    public void AttackEnd()   { if (hitCollider) hitCollider.enabled = false; }

    void OnTriggerEnter(Collider other)
    {
        var h = other.GetComponentInParent<Health>();
        if (!h) return;

        // Dost ateşini engelle
        if ((team == Team.Player && h.team == Team.Player) ||
            (team == Team.Enemy  && h.team == Team.Enemy)) return;

        h.TakeDamage(damage);
    }
}
