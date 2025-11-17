using System.Collections;
using UnityEngine;
using Musashi.Core.Combat;

/// <summary>
/// Enemy Stance AI - ADVANCED LEARNING SYSTEM
///
/// YENİ ZEKİ SİSTEM:
/// - Oyuncunun saldırı paternlerini öğrenir
/// - Akıllıca tahmin eder ve karşı koyar
/// - Feint (sahte) hareketler yapar
/// - Focus yönetimi kritik - risk alır
/// - Executable durumunda son çare saldırı
/// - Oyuncunun zayıf anlarını bekler
/// </summary>
public class EnemyStanceController : MonoBehaviour
{
    public Transform player;

    [Header("Focus System")]
    public float focusMax = 100f;
    public float passiveRegenRate = 0.5f;

    [Header("Focus Costs")]
    public float attackCost = 20f;
    public float stanceChangeCost = 5f;
    public float failedDefenseCost = 35f;

    [Header("Focus Rewards")]
    public float successfulBlockReward = 15f;
    public float perfectBlockReward = 25f;

    [Header("AI Timing")]
    public Vector2 thinkInterval = new Vector2(1f, 3f);
    public float attackCooldown = 1f;

    [Header("AI Behavior - ADVANCED")]
    [Range(0f, 1f)] public float baseAggression = 0.5f;
    [Range(0f, 1f)] public float predictionSkill = 0.7f;
    [Range(0f, 1f)] public float feintChance = 0.4f;
    [Range(0f, 1f)] public float counterAttackChance = 0.6f;

    [Header("Position")]
    public float lockedX = 2.5f;
    public float lockedZ = 0f;

    // State
    private float _focus;
    private bool _canAct = true;
    private AttackDirection _currentStance = AttackDirection.Up;
    private bool _isFeinting = false;

    // Components
    private Animator _anim;
    private HealthB _hp;
    private StanceBasedCombatSystem _stanceSystem;
    private PlayerStanceController _playerController;

    // AI LEARNING - Pattern recognition
    private int[] _playerAttackHistory = new int[4]; // Track player's attack directions
    private int[] _playerDefenseHistory = new int[4]; // Track player's defense patterns
    private AttackDirection _lastPlayerStance = AttackDirection.None;
    private int _playerStanceChangeCount = 0;
    private float _playerAverageFocus = 100f;

    public float CurrentFocus => _focus;
    public bool IsExecutable => _focus < 20f;
    public AttackDirection CurrentStance => _currentStance;

    void Start()
    {
        _anim = GetComponent<Animator>();
        _hp = GetComponent<HealthB>();
        _stanceSystem = GetComponent<StanceBasedCombatSystem>();
        _focus = focusMax;

        _playerController = FindObjectOfType<PlayerStanceController>();

        if (_stanceSystem != null)
        {
            _stanceSystem.OnCombatResult += HandleCombatResult;
            _stanceSystem.OnExecutable += HandleExecutable;
        }

        // Set initial stance
        _stanceSystem?.SetAttackStance(_currentStance);
        _stanceSystem?.SetDefenseStance(_currentStance);

        UpdateFocusUI();
        LockToSpot();
        StartCoroutine(AIBrain());
        StartCoroutine(PatternLearning());
    }

    void OnDestroy()
    {
        if (_stanceSystem != null)
        {
            _stanceSystem.OnCombatResult -= HandleCombatResult;
            _stanceSystem.OnExecutable -= HandleExecutable;
        }
    }

    void Update()
    {
        LockToSpot();
        FacePlayer();
        PassiveRegen();

        if (_stanceSystem != null)
        {
            _stanceSystem.CheckExecutable(_focus);
        }
    }

    /// <summary>
    /// Pattern learning system - learns player behavior
    /// </summary>
    IEnumerator PatternLearning()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);

            if (_playerController != null)
            {
                // Track player's current stance
                AttackDirection currentPlayerStance = _playerController.CurrentStance;

                if (currentPlayerStance != _lastPlayerStance && _lastPlayerStance != AttackDirection.None)
                {
                    _playerStanceChangeCount++;
                }

                _lastPlayerStance = currentPlayerStance;

                // Track average focus
                _playerAverageFocus = Mathf.Lerp(_playerAverageFocus, _playerController.CurrentFocus, 0.1f);
            }
        }
    }

    IEnumerator AIBrain()
    {
        yield return new WaitForSeconds(2f);

        while (true)
        {
            if (!_canAct)
            {
                yield return null;
                continue;
            }

            float wait = Random.Range(thinkInterval.x, thinkInterval.y);
            yield return new WaitForSeconds(wait);

            // EXECUTABLE = TAMAMEN ÇARESİZ! Hiçbir şey yapamaz!
            if (IsExecutable)
            {
                Debug.Log("[Enemy AI] ☠️ EXECUTABLE! COMPLETELY HELPLESS! Waiting for death...");
                // NO ACTION POSSIBLE! Just wait for the final blow
                yield return new WaitForSeconds(1f);
                continue;
            }

            // Check if player is executable - FINISH HIM!
            bool playerExecutable = _playerController != null && _playerController.IsExecutable;

            if (playerExecutable && _focus >= attackCost)
            {
                Debug.Log("[Enemy AI] ☠️⚔️ PLAYER EXECUTABLE! EXECUTION!");
                yield return StartCoroutine(ExecutionAttack());
                continue;
            }

            // STRATEGIC DECISION MAKING
            float decision = Random.value;
            float aggression = CalculateSmartAggression();

            // Should we attack?
            if (_focus >= attackCost && decision < aggression)
            {
                // FEINT TACTIC - Change stance multiple times to confuse player
                if (Random.value < feintChance && _focus >= stanceChangeCost * 2)
                {
                    yield return StartCoroutine(FeintAttack());
                }
                else
                {
                    // Smart stance selection before attack
                    ChooseSmartStance(true);
                    yield return new WaitForSeconds(0.3f);
                    yield return StartCoroutine(CommitAttack());
                }
            }
            // Defensive stance switching
            else if (_focus >= stanceChangeCost)
            {
                // Predict player's next attack and defend
                ChooseSmartStance(false);
                yield return new WaitForSeconds(0.2f);
            }

            yield return new WaitForSeconds(0.2f);
        }
    }

    /// <summary>
    /// Calculate aggression based on game state
    /// </summary>
    float CalculateSmartAggression()
    {
        float aggro = baseAggression;

        if (_playerController == null) return aggro;

        // Very aggressive when player is low on focus
        float playerFocusPercent = _playerController.CurrentFocus / focusMax;
        if (playerFocusPercent < 0.4f)
        {
            aggro += 0.4f; // ATTACK when they're weak!
        }

        // Less aggressive when WE are low on focus (risky to attack)
        float ourFocusPercent = _focus / focusMax;
        if (ourFocusPercent < 0.4f)
        {
            aggro -= 0.3f; // Play defensively
        }

        // If player changes stance a lot, they're aggressive
        if (_playerStanceChangeCount > 5)
        {
            aggro += 0.2f; // Match their aggression
        }

        // If we just got hit, counter-attack!
        if (Random.value < counterAttackChance)
        {
            aggro += 0.15f;
        }

        return Mathf.Clamp01(aggro);
    }

    /// <summary>
    /// Smart stance selection based on learning
    /// </summary>
    void ChooseSmartStance(bool forAttack)
    {
        if (_playerController == null)
        {
            _currentStance = DirectionalHelper.GetRandomDirection();
            UpdateStance();
            return;
        }

        AttackDirection playerStance = _playerController.CurrentStance;
        float strategy = Random.value;

        if (forAttack)
        {
            // ATTACKING - Choose based on player's defensive pattern
            int mostDefendedDirection = GetMostDefendedDirection();
            int leastDefendedDirection = GetLeastDefendedDirection();

            if (strategy < predictionSkill)
            {
                // SMART: Attack where player defends least
                _currentStance = (AttackDirection)(leastDefendedDirection + 1);
                Debug.Log($"[Enemy AI] ⚔️ SMART ATTACK: Targeting weak defense ({_currentStance})");
            }
            else if (strategy < predictionSkill + 0.2f)
            {
                // OPPOSITE: Attack opposite of current player stance
                _currentStance = DirectionalHelper.GetOppositeDirection(playerStance);
                Debug.Log($"[Enemy AI] ⚔️ OPPOSITE ATTACK: {_currentStance}");
            }
            else
            {
                // RANDOM: Unpredictable
                _currentStance = DirectionalHelper.GetRandomDirection();
                Debug.Log($"[Enemy AI] ⚔️ RANDOM ATTACK: {_currentStance}");
            }
        }
        else
        {
            // DEFENDING - Predict player's next attack
            int mostAttackedDirection = GetMostAttackedDirection();

            if (strategy < predictionSkill)
            {
                // PREDICT: Defend where player attacks most
                _currentStance = (AttackDirection)(mostAttackedDirection + 1);
                Debug.Log($"[Enemy AI] 🛡️ PREDICTED DEFENSE: {_currentStance}");
            }
            else if (strategy < predictionSkill + 0.3f)
            {
                // MIRROR: Same stance as player
                _currentStance = playerStance;
                Debug.Log($"[Enemy AI] 🛡️ MIRROR DEFENSE: {_currentStance}");
            }
            else
            {
                // RANDOM: Mix it up
                _currentStance = DirectionalHelper.GetRandomDirection();
                Debug.Log($"[Enemy AI] 🛡️ RANDOM DEFENSE: {_currentStance}");
            }
        }

        UpdateStance();
    }

    void UpdateStance()
    {
        // Cost focus to change stance - EXPENSIVE!
        UseFocus(stanceChangeCost);
        _stanceSystem?.SetAttackStance(_currentStance);
        _stanceSystem?.SetDefenseStance(_currentStance);
        Debug.Log($"[Enemy] Stance → {_currentStance} (-{stanceChangeCost} focus)");
    }

    /// <summary>
    /// Feint attack - confuse player with rapid stance changes
    /// </summary>
    IEnumerator FeintAttack()
    {
        Debug.Log("[Enemy AI] 🎭 FEINT ATTACK! Confusing player...");
        _isFeinting = true;

        // Rapid stance changes
        for (int i = 0; i < 2; i++)
        {
            _currentStance = DirectionalHelper.GetRandomDirection();
            UpdateStance();
            yield return new WaitForSeconds(0.15f);
        }

        // Final attack from unexpected direction
        _currentStance = DirectionalHelper.GetRandomDirection();
        UpdateStance();
        yield return new WaitForSeconds(0.2f);

        _isFeinting = false;
        yield return StartCoroutine(CommitAttack());
    }

    IEnumerator CommitAttack()
    {
        if (!_canAct || _focus < attackCost) yield break;

        _canAct = false;

        _stanceSystem?.CommitAttack();
        UseFocus(attackCost);

        Debug.Log($"[Enemy] ⚔️ COMMITTED ATTACK from {_currentStance}! (-{attackCost} focus)");

        _anim?.SetTrigger("attackLight");
        _anim?.SetInteger("attackDirection", (int)_currentStance);

        float damage = 25f;
        var attackData = new DirectionalAttackData(
            _currentStance,
            false,
            damage,
            attackCost,
            gameObject
        );

        if (_playerController != null)
        {
            var result = _playerController.ProcessIncomingAttack(attackData);

            if (result.shouldTakeDamage)
            {
                var playerHealth = _playerController.GetComponent<HealthB>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damage);
                    Debug.Log($"[Enemy] ✅ HIT! {damage} damage!");

                    if (_playerController.IsExecutable)
                    {
                        Debug.Log("[Enemy] ☠️ PLAYER EXECUTABLE! Victory near!");
                    }
                }
            }
            else
            {
                Debug.Log($"[Enemy] ❌ Blocked! Player lost {result.focusCost} focus");
            }
        }

        yield return new WaitForSeconds(0.2f);
        _stanceSystem?.ResetCommit();

        yield return new WaitForSeconds(attackCooldown);
        _canAct = true;
    }

    IEnumerator DesperateAttack()
    {
        _canAct = false;

        Debug.Log("[Enemy] ☠️ DESPERATE ATTACK! ALL OR NOTHING!");

        _stanceSystem?.CommitAttack();
        UseFocus(attackCost);

        _anim?.SetTrigger("attackHeavy");
        _anim?.SetInteger("attackDirection", (int)_currentStance);

        float damage = 35f; // Higher damage when desperate
        var attackData = new DirectionalAttackData(
            _currentStance,
            false,
            damage,
            attackCost,
            gameObject
        );

        if (_playerController != null)
        {
            var result = _playerController.ProcessIncomingAttack(attackData);

            if (result.shouldTakeDamage)
            {
                var playerHealth = _playerController.GetComponent<HealthB>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damage);
                    Debug.Log($"[Enemy] ☠️💥 DESPERATE HIT! {damage} damage!");
                }
            }
        }

        yield return new WaitForSeconds(0.3f);
        _stanceSystem?.ResetCommit();

        yield return new WaitForSeconds(attackCooldown * 1.5f);
        _canAct = true;
    }

    IEnumerator ExecutionAttack()
    {
        _canAct = false;

        Debug.Log("[Enemy] ☠️⚔️☠️ EXECUTION ATTACK!");

        _stanceSystem?.CommitAttack();
        UseFocus(attackCost * 1.5f);

        _anim?.SetTrigger("executionAttack");
        _anim?.SetInteger("attackDirection", (int)_currentStance);

        // EXECUTION = MASSIVE DAMAGE
        float damage = 70f;
        var attackData = new DirectionalAttackData(
            _currentStance,
            true,
            damage,
            attackCost,
            gameObject
        );

        if (_playerController != null)
        {
            var result = _playerController.ProcessIncomingAttack(attackData);

            var playerHealth = _playerController.GetComponent<HealthB>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                Debug.Log($"[Enemy] ☠️☠️☠️ EXECUTION! {damage} DAMAGE!");
            }
        }

        yield return new WaitForSeconds(0.3f);
        _stanceSystem?.ResetCommit();

        yield return new WaitForSeconds(attackCooldown * 2f);
        _canAct = true;
    }

    public (bool shouldTakeDamage, float focusCost) ProcessIncomingAttack(DirectionalAttackData attackData)
    {
        // LEARN player's attack patterns
        int dirIndex = (int)attackData.Direction - 1;
        if (dirIndex >= 0 && dirIndex < 4)
        {
            _playerAttackHistory[dirIndex]++;
        }

        if (_stanceSystem == null)
            return (true, 0f);

        var result = _stanceSystem.ProcessIncomingAttack(attackData);

        // REWARD SYSTEM - Blocking gives focus!
        if (result.result == CombatResult.Blocked)
        {
            // Perfect block (blocked surprise attack) = BIG reward!
            if (attackData.IsHeavyAttack || attackData.Damage > 25f)
            {
                GainFocus(perfectBlockReward);
                Debug.Log($"[Enemy] ✨🛡️ PERFECT BLOCK! +{perfectBlockReward} focus!");
            }
            else
            {
                GainFocus(successfulBlockReward);
                Debug.Log($"[Enemy] 🛡️ Block successful! +{successfulBlockReward} focus!");
            }
            return (false, 0f);
        }
        else if (result.result == CombatResult.Hit)
        {
            // Failed defense = HEAVY penalty
            UseFocus(failedDefenseCost);
            Debug.Log($"[Enemy] 💥 Failed defense! -{failedDefenseCost} focus!");
            return (true, failedDefenseCost);
        }

        return (result.applyDamage, 0f);
    }

    void GainFocus(float amount)
    {
        _focus = Mathf.Clamp(_focus + amount, 0f, focusMax);
        UpdateFocusUI();
    }

    /// <summary>
    /// Get which direction player attacks most
    /// </summary>
    int GetMostAttackedDirection()
    {
        int maxIndex = 0;
        int maxCount = _playerAttackHistory[0];

        for (int i = 1; i < 4; i++)
        {
            if (_playerAttackHistory[i] > maxCount)
            {
                maxCount = _playerAttackHistory[i];
                maxIndex = i;
            }
        }

        return maxIndex;
    }

    /// <summary>
    /// Get which direction player defends least
    /// </summary>
    int GetLeastDefendedDirection()
    {
        int minIndex = 0;
        int minCount = _playerDefenseHistory[0];

        for (int i = 1; i < 4; i++)
        {
            if (_playerDefenseHistory[i] < minCount)
            {
                minCount = _playerDefenseHistory[i];
                minIndex = i;
            }
        }

        return minIndex;
    }

    /// <summary>
    /// Get which direction player defends most
    /// </summary>
    int GetMostDefendedDirection()
    {
        int maxIndex = 0;
        int maxCount = _playerDefenseHistory[0];

        for (int i = 1; i < 4; i++)
        {
            if (_playerDefenseHistory[i] > maxCount)
            {
                maxCount = _playerDefenseHistory[i];
                maxIndex = i;
            }
        }

        return maxIndex;
    }

    void PassiveRegen()
    {
        // EXECUTABLE = NO REGEN! Focus locked at 0!
        if (IsExecutable) return;

        if (_focus < focusMax)
        {
            _focus = Mathf.Clamp(_focus + passiveRegenRate * Time.deltaTime, 0f, focusMax);
            UpdateFocusUI();
        }
    }

    void UseFocus(float amount)
    {
        _focus = Mathf.Clamp(_focus - amount, 0f, focusMax);
        UpdateFocusUI();

        if (_focus <= 0f)
        {
            Debug.Log("[Enemy] ☠️ FOCUS DEPLETED! EXECUTABLE!");
        }
    }

    void UpdateFocusUI()
    {
        var ui = FindObjectOfType<FocusBar>();
        if (ui) ui.UpdateEnemyFocus(_focus, focusMax);
    }

    void FacePlayer()
    {
        if (!player) return;
        Vector3 dir = (player.position - transform.position);
        dir.y = 0f;
        dir.z = 0f;
        if (dir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir.normalized, Vector3.up), 0.2f);
    }

    void LockToSpot()
    {
        var p = transform.position;
        p.x = lockedX;
        p.z = lockedZ;
        transform.position = p;
    }

    void HandleCombatResult(CombatResult result)
    {
        // Combat results are now handled directly in ProcessIncomingAttack
        // This method is kept for event subscription compatibility
    }

    void HandleExecutable()
    {
        Debug.Log("[Enemy] ⚠️☠️ EXECUTABLE! NO DEFENSE!");
    }

    public bool CanReceiveDamage()
    {
        return !_stanceSystem.IsDodging;
    }
}
