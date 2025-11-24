using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private int maxHP = 5;
    public int CurrentHP { get; private set; }

    public event Action<int, int> OnDamaged; // (current, max)
    public event Action OnDied;

private void Awake()
    {
        CurrentHP = Mathf.Max(1, maxHP);
        Debug.Log($"Health initialized on {gameObject.name}: {CurrentHP}/{maxHP}");
    }

public void TakeDamage(int amount)
    {
        if (CurrentHP <= 0)
        {
            Debug.Log($"{gameObject.name} is already dead, ignoring damage");
            return;
        }

        int oldHP = CurrentHP;
        CurrentHP = Mathf.Max(0, CurrentHP - Mathf.Max(1, amount));
        Debug.Log($"{gameObject.name} took {amount} damage: {oldHP} -> {CurrentHP}");
        
        OnDamaged?.Invoke(CurrentHP, maxHP);

        // Trigger hit effects
        if (HitEffectManager.Instance != null)
        {
            HitEffectManager.Instance.TriggerHitEffect(gameObject, amount);
        }

        if (CurrentHP <= 0)
        {
            Debug.Log($"{gameObject.name} has died!");
            OnDied?.Invoke();
        }
    }

    public void HealFull()
    {
        CurrentHP = Mathf.Max(1, maxHP);
    }


public int GetMaxHP() { return maxHP; }
}
