using UnityEngine;
using UnityEngine.UI;
using Musashi.Core.Combat;

/// <summary>
/// Visual stance indicators - shows attack/defense stances clearly
/// Görsel stance göstergeleri - saldırı/savunma durumunu net gösterir
/// </summary>
public class StanceIndicatorUI : MonoBehaviour
{
    [Header("Player Indicators")]
    public Image playerAttackUp;
    public Image playerAttackDown;
    public Image playerAttackLeft;
    public Image playerAttackRight;

    public Image playerDefenseUp;
    public Image playerDefenseDown;
    public Image playerDefenseLeft;
    public Image playerDefenseRight;

    [Header("Enemy Indicators")]
    public Image enemyAttackUp;
    public Image enemyAttackDown;
    public Image enemyAttackLeft;
    public Image enemyAttackRight;

    public Image enemyDefenseUp;
    public Image enemyDefenseDown;
    public Image enemyDefenseLeft;
    public Image enemyDefenseRight;

    [Header("Colors")]
    public Color attackNormalColor = new Color(1f, 0.5f, 0f, 0.3f); // Orange, transparent
    public Color attackSelectedColor = new Color(1f, 0.5f, 0f, 1f); // Orange, solid
    public Color attackCommittedColor = new Color(1f, 0f, 0f, 1f); // Red, solid

    public Color defenseNormalColor = new Color(0f, 0.5f, 1f, 0.3f); // Blue, transparent
    public Color defenseSelectedColor = new Color(0f, 0.5f, 1f, 1f); // Blue, solid

    public Color executableColor = new Color(0.5f, 0f, 0.5f, 1f); // Purple (death)

    [Header("Animation")]
    public float pulseSpeed = 3f;
    public float commitFlashSpeed = 10f;

    private PlayerStanceController _player;
    private EnemyStanceController _enemy;
    private StanceBasedCombatSystem _playerStance;
    private StanceBasedCombatSystem _enemyStance;

    private bool _playerCommitted = false;
    private bool _enemyCommitted = false;
    private float _flashTimer = 0f;

    void Start()
    {
        // --- FIXED LINES BELOW ---
        // Original was: FindFirstObjectByTypePlayerStanceController>(());
        _player = FindFirstObjectByType<PlayerStanceController>();
        _enemy = FindFirstObjectByType<EnemyStanceController>();
        // -------------------------

        if (_player != null)
            _playerStance = _player.GetComponent<StanceBasedCombatSystem>();
        if (_enemy != null)
            _enemyStance = _enemy.GetComponent<StanceBasedCombatSystem>();

        // Subscribe to events
        if (_playerStance != null)
        {
            _playerStance.OnAttackStanceChanged += OnPlayerAttackChanged;
            _playerStance.OnDefenseStanceChanged += OnPlayerDefenseChanged;
        }

        if (_enemyStance != null)
        {
            _enemyStance.OnAttackStanceChanged += OnEnemyAttackChanged;
            _enemyStance.OnDefenseStanceChanged += OnEnemyDefenseChanged;
        }

        InitializeIndicators();
    }

    void OnDestroy()
    {
        if (_playerStance != null)
        {
            _playerStance.OnAttackStanceChanged -= OnPlayerAttackChanged;
            _playerStance.OnDefenseStanceChanged -= OnPlayerDefenseChanged;
        }

        if (_enemyStance != null)
        {
            _enemyStance.OnAttackStanceChanged -= OnEnemyAttackChanged;
            _enemyStance.OnDefenseStanceChanged -= OnEnemyDefenseChanged;
        }
    }

    void Update()
    {
        _flashTimer += Time.deltaTime;

        UpdatePlayerStance();
        UpdateEnemyStance();
        UpdateCommitVisuals();
        UpdateExecutableVisuals();
    }

    void InitializeIndicators()
    {
        // Set all to normal initially
        SetAttackIndicators(playerAttackUp, playerAttackDown, playerAttackLeft, playerAttackRight, AttackDirection.None, false);
        SetDefenseIndicators(playerDefenseUp, playerDefenseDown, playerDefenseLeft, playerDefenseRight, AttackDirection.Up);

        SetAttackIndicators(enemyAttackUp, enemyAttackDown, enemyAttackLeft, enemyAttackRight, AttackDirection.None, false);
        SetDefenseIndicators(enemyDefenseUp, enemyDefenseDown, enemyDefenseLeft, enemyDefenseRight, AttackDirection.Up);
    }

    void OnPlayerAttackChanged(AttackDirection dir)
    {
        _playerCommitted = false;
    }

    void OnPlayerDefenseChanged(AttackDirection dir)
    {
        // Visual update happens in Update()
    }

    void OnEnemyAttackChanged(AttackDirection dir)
    {
        _enemyCommitted = false;
    }

    void OnEnemyDefenseChanged(AttackDirection dir)
    {
        // Visual update happens in Update()
    }

    void UpdatePlayerStance()
    {
        if (_playerStance == null) return;

        AttackDirection attackDir = _playerStance.CurrentAttackStance;
        AttackDirection defenseDir = _playerStance.CurrentDefenseStance;
        bool committed = _playerStance.isCommitted;

        _playerCommitted = committed;

        SetAttackIndicators(playerAttackUp, playerAttackDown, playerAttackLeft, playerAttackRight, attackDir, committed);
        SetDefenseIndicators(playerDefenseUp, playerDefenseDown, playerDefenseLeft, playerDefenseRight, defenseDir);
    }

    void UpdateEnemyStance()
    {
        if (_enemyStance == null) return;

        AttackDirection attackDir = _enemyStance.CurrentAttackStance;
        AttackDirection defenseDir = _enemyStance.CurrentDefenseStance;
        bool committed = _enemyStance.isCommitted;

        _enemyCommitted = committed;

        SetAttackIndicators(enemyAttackUp, enemyAttackDown, enemyAttackLeft, enemyAttackRight, attackDir, committed);
        SetDefenseIndicators(enemyDefenseUp, enemyDefenseDown, enemyDefenseLeft, enemyDefenseRight, defenseDir);
    }

    void SetAttackIndicators(Image up, Image down, Image left, Image right, AttackDirection selected, bool committed)
    {
        if (up != null) up.color = (selected == AttackDirection.Up) ? (committed ? attackCommittedColor : attackSelectedColor) : attackNormalColor;
        if (down != null) down.color = (selected == AttackDirection.Down) ? (committed ? attackCommittedColor : attackSelectedColor) : attackNormalColor;
        if (left != null) left.color = (selected == AttackDirection.Left) ? (committed ? attackCommittedColor : attackSelectedColor) : attackNormalColor;
        if (right != null) right.color = (selected == AttackDirection.Right) ? (committed ? attackCommittedColor : attackSelectedColor) : attackNormalColor;
    }

    void SetDefenseIndicators(Image up, Image down, Image left, Image right, AttackDirection selected)
    {
        if (up != null) up.color = (selected == AttackDirection.Up) ? defenseSelectedColor : defenseNormalColor;
        if (down != null) down.color = (selected == AttackDirection.Down) ? defenseSelectedColor : defenseNormalColor;
        if (left != null) left.color = (selected == AttackDirection.Left) ? defenseSelectedColor : defenseNormalColor;
        if (right != null) right.color = (selected == AttackDirection.Right) ? defenseSelectedColor : defenseNormalColor;
    }

    void UpdateCommitVisuals()
    {
        // Flash committed attacks
        if (_playerCommitted)
        {
            float flash = Mathf.PingPong(_flashTimer * commitFlashSpeed, 1f);
            Color flashColor = Color.Lerp(attackCommittedColor, Color.red, flash);

            if (_playerStance != null)
            {
                AttackDirection dir = _playerStance.CurrentAttackStance;
                if (dir == AttackDirection.Up && playerAttackUp != null) playerAttackUp.color = flashColor;
                if (dir == AttackDirection.Down && playerAttackDown != null) playerAttackDown.color = flashColor;
                if (dir == AttackDirection.Left && playerAttackLeft != null) playerAttackLeft.color = flashColor;
                if (dir == AttackDirection.Right && playerAttackRight != null) playerAttackRight.color = flashColor;
            }
        }

        if (_enemyCommitted)
        {
            float flash = Mathf.PingPong(_flashTimer * commitFlashSpeed, 1f);
            Color flashColor = Color.Lerp(attackCommittedColor, Color.red, flash);

            if (_enemyStance != null)
            {
                AttackDirection dir = _enemyStance.CurrentAttackStance;
                if (dir == AttackDirection.Up && enemyAttackUp != null) enemyAttackUp.color = flashColor;
                if (dir == AttackDirection.Down && enemyAttackDown != null) enemyAttackDown.color = flashColor;
                if (dir == AttackDirection.Left && enemyAttackLeft != null) enemyAttackLeft.color = flashColor;
                if (dir == AttackDirection.Right && enemyAttackRight != null) enemyAttackRight.color = flashColor;
            }
        }
    }

    void UpdateExecutableVisuals()
    {
        // Purple glow for executable characters
        if (_player != null && _player.IsExecutable)
        {
            float pulse = Mathf.PingPong(_flashTimer * pulseSpeed, 1f);
            Color pulseColor = Color.Lerp(defenseNormalColor, executableColor, pulse);

            if (playerDefenseUp != null) playerDefenseUp.color = pulseColor;
            if (playerDefenseDown != null) playerDefenseDown.color = pulseColor;
            if (playerDefenseLeft != null) playerDefenseLeft.color = pulseColor;
            if (playerDefenseRight != null) playerDefenseRight.color = pulseColor;
        }

        if (_enemy != null && _enemy.IsExecutable)
        {
            float pulse = Mathf.PingPong(_flashTimer * pulseSpeed, 1f);
            Color pulseColor = Color.Lerp(defenseNormalColor, executableColor, pulse);

            if (enemyDefenseUp != null) enemyDefenseUp.color = pulseColor;
            if (enemyDefenseDown != null) enemyDefenseDown.color = pulseColor;
            if (enemyDefenseLeft != null) enemyDefenseLeft.color = pulseColor;
            if (enemyDefenseRight != null) enemyDefenseRight.color = pulseColor;
        }
    }
}