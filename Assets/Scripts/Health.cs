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
        var pc = GetComponent<PlayerDuelController>();
        if (pc) canReceive = pc.CanReceiveDamage();

        var ec = GetComponent<EnemyDuelController>();
        if (ec) canReceive = ec.CanReceiveDamage();

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

        // Input/AI kapat
        var pc = GetComponent<PlayerDuelController>();
        if (pc) pc.enabled = false;
        var ec = GetComponent<EnemyDuelController>();
        if (ec) ec.enabled = false;
    }
}
