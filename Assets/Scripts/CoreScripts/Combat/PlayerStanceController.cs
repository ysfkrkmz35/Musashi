using System.Collections;
using UnityEngine;
using Musashi.Core.Combat;

/// <summary>
/// Player Stance Controller - DOUBLE TAP COMMIT SYSTEM
///
/// MEKAN İK:
/// - WASD = Stance seç (attack + defense aynı tuş!)
/// - İkinci kez aynı tuşa bas = COMMIT (o yönden saldır!)
/// - Farklı yöne bas = Stance değiştir (-2 focus)
/// - Block = -5 focus
/// - Failed defense = -15 focus
/// - Executable = Defense yok, bir sonraki hit = ölüm
/// </summary>
public class PlayerStanceController : MonoBehaviour
{
    [Header("Focus System")]
    public float focusMax = 100f;
    public float passiveRegenRate = 0.5f; // VERY SLOW passive regen

    [Header("Focus Costs")]
    public float attackCost = 20f;
    public float stanceChangeCost = 5f;
    public float failedDefenseCost = 35f;
    public float surpriseAttackCost = 30f;

    [Header("Focus Rewards")]
    public float successfulBlockReward = 15f; // Gain focus on block!
    public float perfectBlockReward = 25f; // Extra reward for blocking surprise attacks

    [Header("Surprise Attack")]
    public float surpriseAttackWindow = 0.4f;
    public float surpriseAttackDamageMultiplier = 2.0f; // DOUBLE damage!
    public float surpriseAttackPenalty = 20f;

    [Header("Timing")]
    public float doubleTapWindow = 0.5f;
    public float attackCooldown = 0.8f;

    [Header("Input Keys")]
    public KeyCode upKey = KeyCode.W;
    public KeyCode downKey = KeyCode.S;
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;

    [Header("Position")]
    public float lockedX = 0f;
    public float lockedZ = 0f;

    // State
    private float _focus;
    private bool _canAct = true;

    // Double-tap tracking
    private AttackDirection _currentStance = AttackDirection.Up;
    private AttackDirection _lastPressedDirection = AttackDirection.None;
    private float _lastPressTime = 0f;

    // Surprise attack tracking
    private float _lastStanceChangeTime = 0f;
    private bool _canSurpriseAttack = false;

    // INPUT BUFFERING - daha responsive combat!
    private AttackDirection _bufferedInput = AttackDirection.None;
    private float _bufferTime = 0f;
    private const float INPUT_BUFFER_WINDOW = 0.2f; // 200ms buffer window

    // COMBO SYSTEM
    private int _stanceChangeCombo = 0;
    private float _lastStanceComboTime = 0f;
    private const float COMBO_WINDOW = 1.5f;
    private const int MAX_COMBO = 5;

    // Components
    private Animator _anim;
    private HealthB _hp;
    private StanceBasedCombatSystem _stanceSystem;
    private EnemyStanceController _enemy;

    public float CurrentFocus => _focus;
    public bool CanAct => _canAct;
    public bool IsExecutable => _focus < 20f;
    public AttackDirection CurrentStance => _currentStance;

    void Start()
    {
        _anim = GetComponent<Animator>();
        _hp = GetComponent<HealthB>();
        _stanceSystem = GetComponent<StanceBasedCombatSystem>();
        _focus = focusMax;

        _enemy = FindFirstObjectByType<EnemyStanceController>();

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
        HandleStanceInput();
        HandleInputBuffer();
        HandleComboTimer();
        HandleSpecialActions();
        PassiveRegen();

        // Update executable status
        if (_stanceSystem != null)
        {
            _stanceSystem.CheckExecutable(_focus);
        }
    }

    void HandleStanceInput()
    {
        if (!_canAct) return;

        // EXECUTABLE = TAMAMEN ÇARESİZ! Hiçbir hamle yapamaz!
        if (IsExecutable)
        {
            // NO INPUT ALLOWED! Just wait for death...
            return;
        }

        // Normal stance input
        CheckStanceInput();
    }

    void CheckStanceInput()
    {
        AttackDirection pressedDirection = AttackDirection.None;

        if (Input.GetKeyDown(upKey)) pressedDirection = AttackDirection.Up;
        else if (Input.GetKeyDown(downKey)) pressedDirection = AttackDirection.Down;
        else if (Input.GetKeyDown(leftKey)) pressedDirection = AttackDirection.Left;
        else if (Input.GetKeyDown(rightKey)) pressedDirection = AttackDirection.Right;

        if (pressedDirection == AttackDirection.None) return;

        float timeSinceLastPress = Time.time - _lastPressTime;

        // DOUBLE TAP = COMMIT ATTACK!
        if (pressedDirection == _lastPressedDirection && timeSinceLastPress < doubleTapWindow)
        {
            // COMMIT TO ATTACK!
            StartCoroutine(CommitAttack());
            _lastPressedDirection = AttackDirection.None; // Reset
            _bufferedInput = AttackDirection.None; // Clear buffer
        }
        else
        {
            // STANCE CHANGE (or buffer if busy)
            if (!_canAct)
            {
                // Buffer the input!
                _bufferedInput = pressedDirection;
                _bufferTime = Time.time;
                Debug.Log($"[Player] Input buffered: {pressedDirection}");
            }
            else if (pressedDirection != _currentStance)
            {
                ChangeStance(pressedDirection);
            }

            _lastPressedDirection = pressedDirection;
            _lastPressTime = Time.time;
        }
    }

    void HandleInputBuffer()
    {
        // Process buffered input when player becomes available
        if (_bufferedInput != AttackDirection.None && _canAct)
        {
            float bufferAge = Time.time - _bufferTime;
            if (bufferAge < INPUT_BUFFER_WINDOW)
            {
                Debug.Log($"[Player] Processing buffered input: {_bufferedInput}");
                if (_bufferedInput != _currentStance)
                {
                    ChangeStance(_bufferedInput);
                }
                _lastPressedDirection = _bufferedInput;
                _lastPressTime = Time.time;
            }
            _bufferedInput = AttackDirection.None;
        }
    }

    void HandleComboTimer()
    {
        // Reset combo if too much time passed
        if (Time.time - _lastStanceComboTime > COMBO_WINDOW && _stanceChangeCombo > 0)
        {
            Debug.Log($"[Player] Combo broken! ({_stanceChangeCombo} hits)");
            _stanceChangeCombo = 0;
        }
    }

    void ChangeStance(AttackDirection newStance)
    {
        // COMBO SYSTEM - Fast stance changes = reduced cost!
        float timeSinceLastCombo = Time.time - _lastStanceComboTime;
        if (timeSinceLastCombo < COMBO_WINDOW)
        {
            _stanceChangeCombo = Mathf.Min(_stanceChangeCombo + 1, MAX_COMBO);
        }
        else
        {
            _stanceChangeCombo = 1;
        }
        _lastStanceComboTime = Time.time;

        // Combo bonus: Reduced cost for consecutive stance changes!
        float costMultiplier = 1f - (_stanceChangeCombo * 0.1f); // 10% reduction per combo
        costMultiplier = Mathf.Max(costMultiplier, 0.5f); // Min 50% cost
        float actualCost = stanceChangeCost * costMultiplier;

        UseFocus(actualCost);

        _currentStance = newStance;

        // Update both attack and defense stance
        _stanceSystem?.SetAttackStance(_currentStance);
        _stanceSystem?.SetDefenseStance(_currentStance);

        // Enable surprise attack window
        _lastStanceChangeTime = Time.time;
        _canSurpriseAttack = true;

        if (_stanceChangeCombo > 1)
        {
            Debug.Log($"[Player] ⚡ COMBO x{_stanceChangeCombo}! Stance → {_currentStance} (-{actualCost:F1} focus) [SURPRISE READY!]");
        }
        else
        {
            Debug.Log($"[Player] Stance changed to {_currentStance} (-{actualCost:F1} focus) [SURPRISE ATTACK READY!]");
        }
    }

    IEnumerator CommitAttack()
    {
        if (!_canAct) yield break;

        // Check for SURPRISE ATTACK!
        bool isSurpriseAttack = false;
        float timeSinceStanceChange = Time.time - _lastStanceChangeTime;

        if (_canSurpriseAttack && timeSinceStanceChange <= surpriseAttackWindow)
        {
            isSurpriseAttack = true;
            _canSurpriseAttack = false;
        }

        // Check focus cost
        float totalCost = isSurpriseAttack ? surpriseAttackCost : attackCost;

        if (_focus < totalCost)
        {
            Debug.Log("[Player] ❌ Not enough focus to attack!");
            yield break;
        }

        _canAct = false;

        // Commit!
        _stanceSystem?.CommitAttack();
        UseFocus(totalCost);

        if (isSurpriseAttack)
        {
            Debug.Log($"[Player] ⚡⚔️ SURPRISE ATTACK from {_currentStance}! (-{totalCost} focus) [+80% DAMAGE!]");
        }
        else
        {
            Debug.Log($"[Player] ⚔️ ATTACK COMMITTED from {_currentStance}! (-{totalCost} focus)");
        }

        // Animation
        _anim?.SetTrigger(isSurpriseAttack ? "attackHeavy" : "attackLight");
        _anim?.SetInteger("attackDirection", (int)_currentStance);

        // Create attack data
        float baseDamage = 20f;
        float damage = isSurpriseAttack ? baseDamage * surpriseAttackDamageMultiplier : baseDamage;
        var attackData = new DirectionalAttackData(
            _currentStance,
            isSurpriseAttack,
            damage,
            totalCost,
            gameObject
        );

        // Send to enemy
        if (_enemy != null)
        {
            var result = _enemy.ProcessIncomingAttack(attackData);

            if (result.shouldTakeDamage)
            {
                var enemyHealth = _enemy.GetComponent<HealthB>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(damage);

                    if (isSurpriseAttack)
                    {
                        Debug.Log($"[Player] ⚡✅ SURPRISE HIT! {damage} damage (+80% bonus!)");
                    }
                    else
                    {
                        Debug.Log($"[Player] ✅ HIT! {damage} damage");
                    }

                    // Check if enemy is now executable
                    if (_enemy.IsExecutable)
                    {
                        Debug.Log("[Player] ☠️ ENEMY EXECUTABLE! One more hit wins!");
                    }
                }
            }
            else
            {
                // Surprise attack FAILED = Extra penalty!
                if (isSurpriseAttack)
                {
                    UseFocus(surpriseAttackPenalty);
                    Debug.Log($"[Player] ❌💥 SURPRISE ATTACK BLOCKED! Extra penalty: -{surpriseAttackPenalty} focus!");
                }
                else
                {
                    Debug.Log($"[Player] ❌ Enemy defended! They lost {result.focusCost} focus");
                }
            }
        }

        // Reset
        yield return new WaitForSeconds(0.2f);
        _stanceSystem?.ResetCommit();

        yield return new WaitForSeconds(attackCooldown);
        _canAct = true;
    }

    void HandleSpecialActions()
    {
        // NO SPECIAL ACTIONS! Only stance combat!
    }

    public (bool shouldTakeDamage, float focusCost) ProcessIncomingAttack(DirectionalAttackData attackData)
    {
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
                Debug.Log($"[Player] ✨🛡️ PERFECT BLOCK! +{perfectBlockReward} focus!");
            }
            else
            {
                GainFocus(successfulBlockReward);
                Debug.Log($"[Player] 🛡️ Block successful! +{successfulBlockReward} focus!");
            }
            return (false, 0f);
        }
        else if (result.result == CombatResult.Hit)
        {
            // Failed defense = HEAVY penalty
            UseFocus(failedDefenseCost);
            Debug.Log($"[Player] 💥 Failed defense! -{failedDefenseCost} focus!");
            return (true, failedDefenseCost);
        }

        return (result.applyDamage, 0f);
    }

    void PassiveRegen()
    {
        // EXECUTABLE = NO REGEN! Focus locked at 0!
        if (IsExecutable) return;

        if (_focus < focusMax)
        {
            GainFocus(passiveRegenRate * Time.deltaTime);
        }
    }

    void UseFocus(float amount)
    {
        _focus = Mathf.Clamp(_focus - amount, 0f, focusMax);
        UpdateFocusUI();

        if (_focus <= 0f)
        {
            Debug.Log("[Player] ☠️ FOCUS DEPLETED! EXECUTABLE!");
        }
    }

    void GainFocus(float amount)
    {
        _focus = Mathf.Clamp(_focus + amount, 0f, focusMax);
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
        Debug.Log("[Player] ⚠️☠️ EXECUTABLE! No defense! BLOCK to gain focus!");
    }

    public bool CanReceiveDamage()
    {
        return !_stanceSystem.IsDodging;
    }
}
