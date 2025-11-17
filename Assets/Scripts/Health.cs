using UnityEngine;

public class HealthB : MonoBehaviour
{
    public Team team;
    public float maxHP = 100f;
    public float currentHP;

    public Animator anim;

    void Start()
    {
        currentHP = maxHP;
        if (!anim) anim = GetComponent<Animator>();
    }

    public void TakeDamage(float dmg)
    {
        if (currentHP <= 0f) return;

        // Parry / i-frame kontrolleri:
        bool canReceive = true;

        // Check old controllers
        var pc = GetComponent<PlayerDuelController>();
        if (pc) canReceive = pc.CanReceiveDamage();

        var ec = GetComponent<EnemyDuelController>();
        if (ec) canReceive = ec.CanReceiveDamage();

        // Check new directional controllers
        var pcDir = GetComponent<PlayerDuelControllerDirectional>();
        if (pcDir) canReceive = pcDir.CanReceiveDamage();

        var ecDir = GetComponent<EnemyDuelControllerDirectional>();
        if (ecDir) canReceive = ecDir.CanReceiveDamage();

        // Check V2 controllers
        var ecDirV2 = GetComponent<EnemyDuelControllerDirectional_V2>();
        if (ecDirV2) canReceive = ecDirV2.CanReceiveDamage();

        if (!canReceive) return;

        currentHP -= dmg;
        if (anim) anim.SetTrigger("hit");

        if (currentHP <= 0f)
        {
            currentHP = 0f;
            Die();
        }
    }

    void Die()
    {
        if (anim) anim.SetTrigger("die");

        // Input/AI kapat - old controllers
        var pc = GetComponent<PlayerDuelController>();
        if (pc) pc.enabled = false;
        var ec = GetComponent<EnemyDuelController>();
        if (ec) ec.enabled = false;

        // Input/AI kapat - new directional controllers
        var pcDir = GetComponent<PlayerDuelControllerDirectional>();
        if (pcDir) pcDir.enabled = false;
        var ecDir = GetComponent<EnemyDuelControllerDirectional>();
        if (ecDir) ecDir.enabled = false;

        // Input/AI kapat - V2 controllers
        var ecDirV2 = GetComponent<EnemyDuelControllerDirectional_V2>();
        if (ecDirV2) ecDirV2.enabled = false;

        // Notify adapter for game state management
        var adapter = GetComponent<Musashi.Core.Combat.HealthDirectionalAdapter>();
        if (adapter) adapter.OnDeath();
    }
}
