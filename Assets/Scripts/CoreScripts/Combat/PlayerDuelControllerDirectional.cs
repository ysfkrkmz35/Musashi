using System.Collections;
using UnityEngine;
using Musashi.Core.Combat;

/// <summary>
/// Enhanced PlayerDuelController with 4-directional attack/defense system
/// FIXED: Saldırılar artık enemy'ye ulaşıyor, defense doğru çalışıyor
/// </summary>
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(DirectionalCombatSystem))]
public class PlayerDuelControllerDirectional : MonoBehaviour
{
    [Header("Focus")]
    public float focusMax = 100f;
    public float focusRegenRate = 10f;
    public float meditateBonus = 15f;

    [Header("Costs")]
    public float lightCost = 6f;
    public float heavyCost = 12f;
    public float parryCost = 5f;
    public float dodgeCost = 10f;

    [Header("Timings")]
    public float attackCooldown = 0.4f;
    public float actionLock = 0.1f;

    [Header("Attack Keys")]
    public KeyCode lightKey = KeyCode.Mouse0;
    public KeyCode heavyKey = KeyCode.Mouse1;
    public KeyCode parryKey = KeyCode.LeftShift;
    public KeyCode dodgeKey = KeyCode.Space;
    public KeyCode meditateKey = KeyCode.R;

    [Header("Directional Keys - Attack")]
    [Tooltip("Arrow keys ile saldırı yönü seçimi")]
    public KeyCode upAttackKey = KeyCode.UpArrow;
    public KeyCode downAttackKey = KeyCode.DownArrow;
    public KeyCode leftAttackKey = KeyCode.LeftArrow;
    public KeyCode rightAttackKey = KeyCode.RightArrow;

    [Header("Directional Keys - Defense")]
    [Tooltip("WASD ile savunma yönü değiştirme")]
    public KeyCode upDefenseKey = KeyCode.W;
    public KeyCode downDefenseKey = KeyCode.S;
    public KeyCode leftDefenseKey = KeyCode.A;
    public KeyCode rightDefenseKey = KeyCode.D;

    [Header("Duel Lock")]
    public float lockedX = 0f;
    public float lockedZ = 0f;

    private float _focus;
    private bool _canAct = true;
    private bool _isMeditating = false;
    private Animator _anim;
    private HealthB _hp;
    private DirectionalCombatSystem _combatSystem;
    private EnemyDuelControllerDirectional_V2 _enemy;
    private CombatHitboxDirectional _hitbox;

    // Directional state
    private AttackDirection _currentAttackDir = AttackDirection.Up;
    private AttackDirection _currentDefenseDir = AttackDirection.Up;

    public float CurrentFocus => _focus;
    public bool CanAct => _canAct;

    void Start()
    {
        _anim = GetComponent<Animator>();
        _hp = GetComponent<HealthB>();
        _combatSystem = GetComponent<DirectionalCombatSystem>();
        _hitbox = GetComponentInChildren<CombatHitboxDirectional>();
        _focus = focusMax;

        // Find enemy
        _enemy = FindAnyObjectByType<EnemyDuelControllerDirectional_V2>();
        if (_enemy == null)
        {
            var oldEnemy = FindAnyObjectByType<EnemyDuelControllerDirectional>();
            if (oldEnemy) Debug.LogWarning("[Player] Old enemy controller found! Please update to V2");
        }

        // Subscribe to combat events
        if (_combatSystem != null)
        {
            _combatSystem.OnCombatResult += HandleCombatResult;
            _combatSystem.OnCounterWindowOpened += HandleCounterWindowOpened;
        }

        UpdateFocusUI();
        LockToSpot();
    }

    void OnDestroy()
    {
        if (_combatSystem != null)
        {
            _combatSystem.OnCombatResult -= HandleCombatResult;
            _combatSystem.OnCounterWindowOpened -= HandleCounterWindowOpened;
        }
    }

    void Update()
    {
        LockToSpot();
        HandleDirectionalInput();
        HandleActionInput();
        PassiveRegen();
    }

    /// <summary>
    /// Yönlü input handling - arrow keys/WASD
    /// FIXED: Defense direction artık toggle style çalışıyor
    /// </summary>
    void HandleDirectionalInput()
    {
        // Attack direction (Arrow keys) - Select and hold
        if (Input.GetKeyDown(upAttackKey))
            SetAttackDirection(AttackDirection.Up);
        else if (Input.GetKeyDown(downAttackKey))
            SetAttackDirection(AttackDirection.Down);
        else if (Input.GetKeyDown(leftAttackKey))
            SetAttackDirection(AttackDirection.Left);
        else if (Input.GetKeyDown(rightAttackKey))
            SetAttackDirection(AttackDirection.Right);

        // Defense direction (WASD) - Toggle on press
        if (Input.GetKeyDown(upDefenseKey))
            SetDefenseDirection(AttackDirection.Up);
        else if (Input.GetKeyDown(downDefenseKey))
            SetDefenseDirection(AttackDirection.Down);
        else if (Input.GetKeyDown(leftDefenseKey))
            SetDefenseDirection(AttackDirection.Left);
        else if (Input.GetKeyDown(rightDefenseKey))
            SetDefenseDirection(AttackDirection.Right);
    }

    void HandleActionInput()
    {
        if (!_canAct) return;

        // Meditasyon: odak doldurur, tamamen savunmasız
        if (Input.GetKey(meditateKey))
        {
            if (!_isMeditating)
            {
                _isMeditating = true;
                _anim.SetBool("isMeditating", true);
            }
            GainFocus((focusRegenRate + meditateBonus) * Time.deltaTime);
            return;
        }
        else
        {
            if (_isMeditating)
            {
                _isMeditating = false;
                _anim.SetBool("isMeditating", false);
            }
        }

        // Light attack
        if (Input.GetKeyDown(lightKey) && _focus >= lightCost)
        {
            StartCoroutine(AttackRoutine(false));
            return;
        }

        // Heavy attack
        if (Input.GetKeyDown(heavyKey) && _focus >= heavyCost)
        {
            StartCoroutine(AttackRoutine(true));
            return;
        }

        // Parry
        if (Input.GetKeyDown(parryKey) && _focus >= parryCost)
        {
            StartCoroutine(ParryRoutine());
            return;
        }

        // Dodge
        if (Input.GetKeyDown(dodgeKey) && _focus >= dodgeCost)
        {
            StartCoroutine(DodgeRoutine());
            return;
        }
    }

    void SetAttackDirection(AttackDirection dir)
    {
        _currentAttackDir = dir;
        _combatSystem?.SetAttackDirection(dir);
        Debug.Log($"[Player] Attack direction: {dir}");
    }

    void SetDefenseDirection(AttackDirection dir)
    {
        _currentDefenseDir = dir;
        _combatSystem?.SetDefenseDirection(dir);
        Debug.Log($"[Player] Defense stance: {dir}");
    }

    IEnumerator AttackRoutine(bool isHeavy)
    {
        _canAct = false;

        // Animation trigger based on attack direction + type
        string animTrigger = isHeavy ? "attackHeavy" : "attackLight";
        _anim.SetTrigger(animTrigger);

        // Set animator parameters for direction
        _anim.SetInteger("attackDirection", (int)_currentAttackDir);

        // Update hitbox
        if (_hitbox != null)
        {
            _hitbox.SetAttackType(isHeavy);
        }

        // Cost
        float cost = isHeavy ? heavyCost : lightCost;
        UseFocus(cost);

        // Create attack data for enemy to process
        float damage = isHeavy ? 25f : 12f;
        var attackData = new DirectionalAttackData(
            _currentAttackDir,
            isHeavy,
            damage,
            cost,
            gameObject
        );

        // FIXED: Send attack to enemy directly
        if (_enemy != null)
        {
            var result = _enemy.ProcessIncomingAttack(attackData);

            if (result.shouldTakeDamage)
            {
                // Apply damage through Health system
                var enemyHealth = _enemy.GetComponent<HealthB>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(damage);
                    Debug.Log($"[Player] Hit enemy for {damage} damage! Direction: {_currentAttackDir}");
                }
            }
            else
            {
                Debug.Log($"[Player] Enemy defended! Focus cost: {result.focusCost}");
            }
        }

        yield return new WaitForSeconds(actionLock);
        yield return new WaitForSeconds(attackCooldown);
        _canAct = true;
    }

    IEnumerator ParryRoutine()
    {
        _canAct = false;
        UseFocus(parryCost);

        // Activate parry in combat system
        _combatSystem?.ActivateParry();
        _anim.SetTrigger("parry");

        // Set defense direction indicator animation
        _anim.SetInteger("defenseDirection", (int)_currentDefenseDir);

        Debug.Log($"[Player] Parry activated! Defending: {_currentDefenseDir}");

        yield return new WaitForSeconds(actionLock);
        _canAct = true;
    }

    IEnumerator DodgeRoutine()
    {
        _canAct = false;
        UseFocus(dodgeCost);

        // Activate dodge i-frames
        _combatSystem?.ActivateDodge();
        _anim.SetTrigger("dodge");

        Debug.Log("[Player] Dodge activated!");

        yield return new WaitForSeconds(actionLock);
        _canAct = true;
    }

    void PassiveRegen()
    {
        if (_focus < focusMax && !_isMeditating)
            GainFocus(focusRegenRate * Time.deltaTime);
    }

    void UseFocus(float amt)
    {
        _focus = Mathf.Clamp(_focus - amt, 0f, focusMax);
        UpdateFocusUI();
    }

    void GainFocus(float amt)
    {
        _focus = Mathf.Clamp(_focus + amt, 0f, focusMax);
        UpdateFocusUI();
    }

    void UpdateFocusUI()
    {
        var ui = FindFirstObjectByType<FocusBar>();
        if (ui) ui.UpdatePlayerFocus(_focus, focusMax);
    }

    void LockToSpot()
    {
        var p = transform.position;
        p.x = lockedX; p.z = lockedZ;
        transform.position = p;
    }

    /// <summary>
    /// Called by enemy when attacking player
    /// Processes attack through directional combat system
    /// </summary>
    public (bool shouldTakeDamage, float focusCost) ProcessIncomingAttack(DirectionalAttackData attackData)
    {
        if (_combatSystem == null)
            return (true, 0f); // No combat system = take full damage

        var result = _combatSystem.ProcessIncomingAttack(attackData);

        // Apply focus cost
        if (result.focusCost > 0f)
            UseFocus(result.focusCost);

        Debug.Log($"[Player] Incoming attack from {attackData.Direction} - Result: {result.result}, Damage: {result.applyDamage}");

        return (result.applyDamage, result.focusCost);
    }

    /// <summary>
    /// Combat result event handler
    /// </summary>
    void HandleCombatResult(CombatResult result)
    {
        switch (result)
        {
            case CombatResult.Blocked:
                Debug.Log("[Player] ✋ Blocked attack!");
                break;
            case CombatResult.ParrySuccess:
                Debug.Log("[Player] ⚡ PARRY SUCCESS! Counter window opened!");
                // Refund some focus on successful parry
                GainFocus(parryCost * 0.5f);
                break;
            case CombatResult.ParryFailed:
                Debug.Log("[Player] ❌ Parry FAILED!");
                break;
            case CombatResult.Dodged:
                Debug.Log("[Player] 🌀 Dodged!");
                break;
            case CombatResult.Hit:
                Debug.Log("[Player] 💥 Got hit!");
                break;
        }
    }

    /// <summary>
    /// Counter window opened - player can attack for bonus
    /// </summary>
    void HandleCounterWindowOpened()
    {
        Debug.Log("[Player] 🎯 COUNTER WINDOW! Attack now for bonus damage!");
        // Temporary attack speed boost
        StartCoroutine(CounterBoostRoutine());
    }

    IEnumerator CounterBoostRoutine()
    {
        float originalCooldown = attackCooldown;
        attackCooldown = 0.2f; // Super fast attack
        yield return new WaitForSeconds(0.8f); // Counter window duration
        attackCooldown = originalCooldown;
    }

    // Health system integration
    public bool CanReceiveDamage()
    {
        return !_combatSystem.IsDodging;
    }
}
