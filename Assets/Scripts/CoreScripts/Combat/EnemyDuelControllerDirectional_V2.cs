using System.Collections;
using UnityEngine;
using Musashi.Core.Combat;

/// <summary>
/// Enhanced EnemyDuelController V2 with Attack Telegraph system
/// FIXED: Stratejik saldırı sistemi, daha akıllı savunma, dengeli tempo
/// </summary>
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(DirectionalCombatSystem))]
[RequireComponent(typeof(AttackTelegraphSystem))]
public class EnemyDuelControllerDirectional_V2 : MonoBehaviour
{
    public Transform player;

    [Header("Focus")]
    public float focusMax = 100f;
    public float focusRegenRate = 5f;

    [Header("Costs")]
    public float lightCost = 12f;
    public float heavyCost = 20f;
    public float parryCost = 8f;

    [Header("AI Timings")]
    public Vector2 thinkEvery = new Vector2(2.5f, 4.5f);
    public float attackCooldown = 1.2f;

    [Header("AI Behavior")]
    [Range(0f, 1f)] public float aggressionLevel = 0.4f;
    [Range(0f, 1f)] public float predictionSkill = 0.35f;
    [Range(0f, 1f)] public float adaptationRate = 0.25f;

    [Header("Telegraph Settings")]
    [Tooltip("Oyuncuya saldırı öncesi ne kadar süre verilsin")]
    public bool useTelegraph = true;

    [Header("Duel Lock")]
    public float lockedX = 2.5f;
    public float lockedZ = 0f;

    private float _focus;
    private bool _canAct = true;
    private Animator _anim;
    private DirectionalCombatSystem _combatSystem;
    private AttackTelegraphSystem _telegraphSystem;
    private PlayerDuelControllerDirectional _playerController;

    // AI Learning - Track player patterns
    private int[] _playerDirectionHistory = new int[4];
    private AttackDirection _lastPlayerAttackDir = AttackDirection.Up;
    private AttackDirection _currentDefenseDir = AttackDirection.Up;
    private AttackDirection _currentAttackDir = AttackDirection.Up;

    // Pattern tracking
    private int _consecutiveHits = 0;
    private int _consecutiveMisses = 0;

    public float CurrentFocus => _focus;

    void Start()
    {
        _anim = GetComponent<Animator>();
        _combatSystem = GetComponent<DirectionalCombatSystem>();
        _telegraphSystem = GetComponent<AttackTelegraphSystem>();
        _focus = focusMax;

        // --- FIXED LINE BELOW ---
        // Removed the garbage text "()()ional>();"
        _playerController = FindFirstObjectByType<PlayerDuelControllerDirectional>();
        // ------------------------

        if (_combatSystem != null)
        {
            _combatSystem.OnCombatResult += HandleCombatResult;
        }

        UpdateFocusUI();
        LockToSpot();
        StartCoroutine(Brain());
    }

    void OnDestroy()
    {
        if (_combatSystem != null)
        {
            _combatSystem.OnCombatResult -= HandleCombatResult;
        }
    }

    void Update()
    {
        LockToSpot();
        FacePlayer();
        PassiveRegen();
        UpdateDefenseDirection();
    }

    IEnumerator Brain()
    {
        // Initial delay
        yield return new WaitForSeconds(2f);

        while (true)
        {
            if (!_canAct)
            {
                yield return null;
                continue;
            }

            float wait = Random.Range(thinkEvery.x, thinkEvery.y);
            yield return new WaitForSeconds(wait);

            // Decision making based on current state
            float decision = Random.value;
            float aggro = aggressionLevel;

            // Adapt aggression based on performance
            if (_consecutiveHits >= 2)
            {
                aggro += 0.2f; // More aggressive if landing hits
            }
            else if (_consecutiveMisses >= 2)
            {
                aggro -= 0.2f; // More defensive if missing
            }

            aggro = Mathf.Clamp01(aggro);

            // Choose action based on focus and aggression
            if (_focus >= heavyCost && decision < (aggro * 0.25f))
            {
                // Heavy attack - rare, powerful
                ChooseAttackDirection(true);
                yield return StartCoroutine(AttackRoutine(true));
            }
            else if (_focus >= lightCost && decision < (aggro * 0.6f))
            {
                // Light attack - main offense
                ChooseAttackDirection(false);
                yield return StartCoroutine(AttackRoutine(false));
            }
            else if (_focus >= parryCost && decision < 0.25f)
            {
                // Parry - defensive option
                yield return StartCoroutine(ParryRoutine());
            }
            else
            {
                // Wait and observe
                yield return new WaitForSeconds(0.8f);
            }
        }
    }

    void ChooseAttackDirection(bool isHeavy)
    {
        if (_playerController == null)
        {
            _currentAttackDir = DirectionalCombatSystem.GetRandomDirection();
            return;
        }

        // Get player's current defense
        AttackDirection playerDefense = _combatSystem.CurrentDefenseDirection;

        // Strategy selection
        float strategyRoll = Random.value;

        if (strategyRoll < predictionSkill)
        {
            // SMART: Attack where player is NOT defending
            _currentAttackDir = DirectionalCombatSystem.GetOppositeDirection(playerDefense);
            Debug.Log($"[Enemy AI] Smart attack - targeting opposite of player defense ({playerDefense})");
        }
        else if (strategyRoll < predictionSkill + adaptationRate)
        {
            // ADAPTIVE: Attack player's least defended direction
            _currentAttackDir = GetLeastDefendedDirection();
            Debug.Log($"[Enemy AI] Adaptive attack - targeting weak spot");
        }
        else if (isHeavy && strategyRoll < predictionSkill + adaptationRate + 0.2f)
        {
            // FEINT: For heavy attacks, sometimes attack same direction to catch greedy players
            _currentAttackDir = playerDefense;
            Debug.Log($"[Enemy AI] Feint attack - same direction as defense!");
        }
        else
        {
            // RANDOM: Mix it up to stay unpredictable
            _currentAttackDir = DirectionalCombatSystem.GetRandomDirection();
            Debug.Log($"[Enemy AI] Random attack - keeping player guessing");
        }

        Debug.Log($"[Enemy] 🎯 Chosen attack direction: {_currentAttackDir}");
    }

    void UpdateDefenseDirection()
    {
        if (_playerController == null) return;

        // Don't change defense while attacking
        if (!_canAct) return;

        // Predict player's next attack based on history
        if (Random.value < predictionSkill * 0.7f)
        {
            AttackDirection predictedAttack = PredictPlayerAttack();
            _currentDefenseDir = predictedAttack;
            _combatSystem?.SetDefenseDirection(_currentDefenseDir);
        }
        else if (Random.value < 0.15f)
        {
            // Occasionally change defense randomly
            _currentDefenseDir = DirectionalCombatSystem.GetRandomDirection();
            _combatSystem?.SetDefenseDirection(_currentDefenseDir);
        }
    }

    AttackDirection PredictPlayerAttack()
    {
        // Find player's most used direction
        int maxIndex = 0;
        for (int i = 1; i < 4; i++)
        {
            if (_playerDirectionHistory[i] > _playerDirectionHistory[maxIndex])
                maxIndex = i;
        }

        // If player is predictable, defend against their favorite
        if (_playerDirectionHistory[maxIndex] > 3)
        {
            return (AttackDirection)(maxIndex + 1);
        }

        // Otherwise use last attack direction
        return _lastPlayerAttackDir;
    }

    AttackDirection GetLeastDefendedDirection()
    {
        // Find direction player uses least
        int minIndex = 0;
        for (int i = 1; i < 4; i++)
        {
            if (_playerDirectionHistory[i] < _playerDirectionHistory[minIndex])
                minIndex = i;
        }
        return (AttackDirection)(minIndex + 1);
    }

    IEnumerator AttackRoutine(bool isHeavy)
    {
        _canAct = false;

        // TELEGRAPH PHASE - Oyuncuya uyarı ver
        if (useTelegraph && _telegraphSystem != null)
        {
            Debug.Log($"[Enemy] ⚠️ TELEGRAPH! Attack incoming from {_currentAttackDir}");

            // Set direction BEFORE telegraph
            _combatSystem?.SetAttackDirection(_currentAttackDir);

            yield return StartCoroutine(_telegraphSystem.TelegraphAttack(_currentAttackDir));
        }

        // ATTACK PHASE - Gerçek saldırı
        string animTrigger = isHeavy ? "attackHeavy" : "attackLight";
        _anim.SetTrigger(animTrigger);
        _anim.SetInteger("attackDirection", (int)_currentAttackDir);

        float cost = isHeavy ? heavyCost : lightCost;
        UseFocus(cost);

        float damage = isHeavy ? 30f : 18f;
        var attackData = new DirectionalAttackData(
            _currentAttackDir,
            isHeavy,
            damage,
            cost,
            gameObject
        );

        // Send attack to player
        if (_playerController != null)
        {
            var result = _playerController.ProcessIncomingAttack(attackData);

            if (result.shouldTakeDamage)
            {
                // Apply damage through Health system
                // Note: Ensure HealthB exists in your project or change this to your health script
                var playerHealth = _playerController.GetComponent<HealthB>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damage);
                    Debug.Log($"[Enemy] ✅ HIT! Dealt {damage} damage to player!");
                    _consecutiveHits++;
                    _consecutiveMisses = 0;
                }
            }
            else
            {
                Debug.Log($"[Enemy] ❌ DEFENDED! Player blocked/parried/dodged. Focus cost: {result.focusCost}");
                _consecutiveMisses++;
                _consecutiveHits = 0;
            }
        }

        yield return new WaitForSeconds(attackCooldown);
        _canAct = true;
    }

    IEnumerator ParryRoutine()
    {
        _canAct = false;
        UseFocus(parryCost);

        // Choose smart defense direction
        if (Random.value < predictionSkill)
        {
            _currentDefenseDir = PredictPlayerAttack();
        }
        else
        {
            _currentDefenseDir = DirectionalCombatSystem.GetRandomDirection();
        }

        _combatSystem?.SetDefenseDirection(_currentDefenseDir);
        _combatSystem?.ActivateParry();

        _anim.SetTrigger("parry");
        _anim.SetInteger("defenseDirection", (int)_currentDefenseDir);

        Debug.Log($"[Enemy] Parry ready - defending {_currentDefenseDir}");

        yield return new WaitForSeconds(0.5f);
        _canAct = true;
    }

    void PassiveRegen()
    {
        if (_focus < focusMax)
        {
            _focus = Mathf.Clamp(_focus + focusRegenRate * Time.deltaTime, 0f, focusMax);
            UpdateFocusUI();
        }
    }

    void UseFocus(float amt)
    {
        _focus = Mathf.Clamp(_focus - amt, 0f, focusMax);
        UpdateFocusUI();
    }

    void UpdateFocusUI()
    {
        var ui = FindFirstObjectByType<FocusBar>();
        if (ui) ui.UpdateEnemyFocus(_focus, focusMax);
    }

    void FacePlayer()
    {
        if (!player) return;
        Vector3 dir = (player.position - transform.position);
        dir.y = 0f; dir.z = 0f;
        if (dir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir.normalized, Vector3.up), 0.2f);
    }

    void LockToSpot()
    {
        var p = transform.position;
        p.x = lockedX; p.z = lockedZ;
        transform.position = p;
    }

    public (bool shouldTakeDamage, float focusCost) ProcessIncomingAttack(DirectionalAttackData attackData)
    {
        // Track player attack patterns
        int dirIndex = (int)attackData.Direction - 1;
        if (dirIndex >= 0 && dirIndex < 4)
            _playerDirectionHistory[dirIndex]++;

        _lastPlayerAttackDir = attackData.Direction;

        if (_combatSystem == null)
            return (true, 0f);

        var result = _combatSystem.ProcessIncomingAttack(attackData);

        if (result.focusCost > 0f)
            UseFocus(result.focusCost);

        Debug.Log($"[Enemy] Incoming attack from {attackData.Direction} - Result: {result.result}, Taking damage: {result.applyDamage}");

        return (result.applyDamage, result.focusCost);
    }

    void HandleCombatResult(CombatResult result)
    {
        switch (result)
        {
            case CombatResult.Blocked:
                Debug.Log("[Enemy] Blocked player's attack!");
                break;
            case CombatResult.ParrySuccess:
                Debug.Log("[Enemy] ⚡ PARRY SUCCESS! Counter opportunity!");
                // Increase aggression after successful parry
                aggressionLevel = Mathf.Clamp01(aggressionLevel + 0.15f);
                // Quick counter attack
                if (_focus >= lightCost)
                {
                    StartCoroutine(QuickCounterAttack());
                }
                break;
            case CombatResult.ParryFailed:
                Debug.Log("[Enemy] Parry failed!");
                aggressionLevel = Mathf.Clamp01(aggressionLevel - 0.1f);
                break;
            case CombatResult.Dodged:
                Debug.Log("[Enemy] Player dodged my attack!");
                break;
            case CombatResult.Hit:
                Debug.Log("[Enemy] Hit the player!");
                break;
        }
    }

    IEnumerator QuickCounterAttack()
    {
        yield return new WaitForSeconds(0.2f);

        if (!_canAct) yield break;

        Debug.Log("[Enemy] 🔥 COUNTER ATTACK!");
        ChooseAttackDirection(false);
        yield return StartCoroutine(AttackRoutine(false));
    }

    public bool CanReceiveDamage()
    {
        return !_combatSystem.IsDodging;
    }
}