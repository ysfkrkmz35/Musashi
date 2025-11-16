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
    }

    public void TakeDamage(int amount)
    {
        if (CurrentHP <= 0) return;

        CurrentHP = Mathf.Max(0, CurrentHP - Mathf.Max(1, amount));
        OnDamaged?.Invoke(CurrentHP, maxHP);

        if (CurrentHP <= 0)
            OnDied?.Invoke();
    }

    public void HealFull()
    {
        CurrentHP = Mathf.Max(1, maxHP);
    }
}
