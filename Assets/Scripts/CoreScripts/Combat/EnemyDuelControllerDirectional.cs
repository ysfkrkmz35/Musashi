using System.Collections;
using UnityEngine;
using Musashi.Core.Combat;

/// <summary>
/// Enhanced EnemyDuelController with 4-directional AI
/// Yönlü saldırı/savunma yapabilen düşman yapay zekası
/// </summary>
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(DirectionalCombatSystem))]
public class EnemyDuelControllerDirectional : MonoBehaviour
{
    public Transform player;

    [Header("Focus")]
    public float focusMax = 100f;
    public float focusRegenRate = 5f;

    [Header("Costs")]
    public float lightCost = 10f;
    public float heavyCost = 18f;
    public float parryCost = 8f;

    [Header("AI Timings")]
    public Vector2 thinkEvery = new Vector2(1.2f, 2.0f);
    public float attackCooldown = 0.7f;

    [Header("AI Behavior")]
    [Range(0f, 1f)] public float aggressionLevel = 0.5f; // Yüksek = daha saldırgan
    [Range(0f, 1f)] public float predictionSkill = 0.3f; // Oyuncunun yönünü tahmin etme
    [Range(0f, 1f)] public float adaptationRate = 0.2f;  // Oyuncunun tercihlerini öğrenme

    [Header("Duel Lock")]
    public float lockedX = 2.5f;
    public float lockedZ = 0f;

    private float _focus;
    private bool _canAct = true;
    private Animator _anim;
    private DirectionalCombatSystem _combatSystem;
    private PlayerDuelControllerDirectional _playerController;

    // AI Learning
    private int[] _playerDirectionHistory = new int[4]; // Up, Down, Left, Right counters
    private AttackDirection _lastPlayerAttackDir = AttackDirection.Up;
    private AttackDirection _currentDefenseDir = AttackDirection.Up;
    private AttackDirection _currentAttackDir = AttackDirection.Up;

    public float CurrentFocus => _focus;

    void Start()
    {
        _anim = GetComponent<Animator>();
        _combatSystem = GetComponent<DirectionalCombatSystem>();
        _focus = focusMax;

        // --- FIXED LINE BELOW ---
        // Removed garbage text "()ional>();"
        _playerController = FindFirstObjectByType<PlayerDuelControllerDirectional>();
        // ------------------------

        // Subscribe to combat events
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

    /// <summary>
    /// AI Brain - makes decisions based on focus, aggression, and player behavior
    /// </summary>
    IEnumerator Brain()
    {
        while (true)
        {
            if (!_canAct) { yield return null; continue; }

            float wait = Random.Range(thinkEvery.x, thinkEvery.y);
            yield return new WaitForSeconds(wait);

            // Decision making based on aggression and focus
            float decision = Random.value;

            if (_focus >= heavyCost && decision < (aggressionLevel * 0.4f))
            {
                // Heavy attack
                ChooseAttackDirection();
                yield return StartCoroutine(AttackRoutine(true));
            }
            else if (_focus >= lightCost && decision < (aggressionLevel * 0.7f))
            {
                // Light attack
                ChooseAttackDirection();
                yield return StartCoroutine(AttackRoutine(false));
            }
            else if (_focus >= parryCost && decision < 0.3f)
            {
                // Parry attempt
                yield return StartCoroutine(ParryRoutine());
            }
            else
            {
                // Wait and regenerate focus
                yield return new WaitForSeconds(0.4f);
            }
        }
    }

    /// <summary>
    /// Choose attack direction based on AI skill and player patterns
    /// </summary>
    void ChooseAttackDirection()
    {
        if (Random.value < predictionSkill && _playerController != null)
        {
            // Try to predict player's defense
            // Attack where player is NOT defending (opposite direction)
            AttackDirection playerDefense = _combatSystem.CurrentDefenseDirection;

            if (Random.value < adaptationRate)
            {
                // Learn from history - attack player's least defended direction
                _currentAttackDir = GetLeastDefendedDirection();
            }
            else
            {
                // Simple prediction - attack opposite of current defense
                _currentAttackDir = DirectionalCombatSystem.GetOppositeDirection(playerDefense);
            }
        }
        else
        {
            // Random attack direction
            _currentAttackDir = DirectionalCombatSystem.GetRandomDirection();
        }

        _combatSystem?.SetAttackDirection(_currentAttackDir);
        Debug.Log($"[Enemy] Attacking from {_currentAttackDir}");
    }

    /// <summary>
    /// Update defense direction reactively
    /// </summary>
    void UpdateDefenseDirection()
    {
        if (_playerController == null) return;

        // Reactive defense - try to match player's attack direction
        if (Random.value < predictionSkill)
        {
            // Predict and defend
            AttackDirection predictedAttack = PredictPlayerAttack();
            _currentDefenseDir = predictedAttack;
        }
        else
        {
            // Random defense switching
            if (Random.value < 0.1f) // 10% chance each frame to switch
            {
                _currentDefenseDir = DirectionalCombatSystem.GetRandomDirection();
            }
        }

        _combatSystem?.SetDefenseDirection(_currentDefenseDir);
    }

    /// <summary>
    /// Predict player's next attack based on history
    /// </summary>
    AttackDirection PredictPlayerAttack()
    {
        // Find most used direction from history
        int maxIndex = 0;
        for (int i = 1; i < 4; i++)
        {
            if (_playerDirectionHistory[i] > _playerDirectionHistory[maxIndex])
                maxIndex = i;
        }

        return (AttackDirection)(maxIndex + 1);
    }

    /// <summary>
    /// Get direction player defends least
    /// </summary>
    AttackDirection GetLeastDefendedDirection()
    {
        // Inverse of prediction - attack where player doesn't expect
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

        string animTrigger = isHeavy ? "attackHeavy" : "attackLight";
        _anim.SetTrigger(animTrigger);
        _anim.SetInteger("attackDirection", (int)_currentAttackDir);

        float cost = isHeavy ? heavyCost : lightCost;
        UseFocus(cost);

        // Create attack data to send to player
        var attackData = new DirectionalAttackData(
            _currentAttackDir,
            isHeavy,
            isHeavy ? 30f : 15f,
            cost,
            gameObject
        );

        // Send attack to player
        if (_playerController != null)
        {
            var result = _playerController.ProcessIncomingAttack(attackData);
            Debug.Log($"[Enemy] Attack result: damage={result.shouldTakeDamage}, focusCost={result.focusCost}");
        }

        yield return new WaitForSeconds(attackCooldown);
        _canAct = true;
    }

    IEnumerator ParryRoutine()
    {
        _canAct = false;
        UseFocus(parryCost);

        // Choose defensive direction
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

        yield return new WaitForSeconds(0.3f);
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

    /// <summary>
    /// Called by player when attacking enemy
    /// </summary>
    public (bool shouldTakeDamage, float focusCost) ProcessIncomingAttack(DirectionalAttackData attackData)
    {
        // Update learning history
        int dirIndex = (int)attackData.Direction - 1;
        if (dirIndex >= 0 && dirIndex < 4)
            _playerDirectionHistory[dirIndex]++;

        _lastPlayerAttackDir = attackData.Direction;

        if (_combatSystem == null)
            return (true, 0f);

        var result = _combatSystem.ProcessIncomingAttack(attackData);

        if (result.focusCost > 0f)
            UseFocus(result.focusCost);

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
                Debug.Log("[Enemy] Blocked attack!");
                break;
            case CombatResult.ParrySuccess:
                Debug.Log("[Enemy] PARRY SUCCESS!");
                // Increase aggression after successful parry
                aggressionLevel = Mathf.Clamp01(aggressionLevel + 0.1f);
                break;
            case CombatResult.ParryFailed:
                Debug.Log("[Enemy] Parry FAILED!");
                // Decrease aggression after failed parry
                aggressionLevel = Mathf.Clamp01(aggressionLevel - 0.15f);
                break;
            case CombatResult.Dodged:
                Debug.Log("[Enemy] Player dodged!");
                break;
            case CombatResult.Hit:
                Debug.Log("[Enemy] Hit player!");
                break;
        }
    }

    public bool CanReceiveDamage()
    {
        return !_combatSystem.IsDodging;
    }
}