using UnityEngine;

/// <summary>
/// Ana Player FSM Controller - Tüm state'leri yönetir
/// Movement ve Combat state'lerini koordine eder
/// </summary>
public class PlayerStateMachine : MonoBehaviour
{
    // === STATES ===
    public enum PlayerState
    {
        Idle,
        Running,
        Jumping,
        Falling,
        Attacking,
        Blocking,
        Dashing,
        Stunned,
        Parrying
    }

    [Header("State Bilgisi")]
    [SerializeField] private PlayerState currentState = PlayerState.Idle;
    public PlayerState CurrentState => currentState;

    // === REFERANSLAR ===
    private PlayerMovement movement;
    private PlayerCombat combat;
    private Rigidbody2D rb;
    private Animator anim;

    // === STATE DEĞİŞİM EVENT'LERİ ===
    public System.Action<PlayerState, PlayerState> OnStateChanged;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        movement = GetComponent<PlayerMovement>();
        combat = GetComponent<PlayerCombat>();
    }

    void Start()
    {
        ChangeState(PlayerState.Idle);
    }

    /// <summary>
    /// State değişikliği - tüm scriptler bu fonksiyonu kullanır
    /// </summary>
    public void ChangeState(PlayerState newState)
    {
        if (currentState == newState) return;

        // Bazı state'lerden çıkış engellenebilir
        if (!CanExitState(currentState, newState)) return;

        PlayerState previousState = currentState;
        
        // Eski state'den çık
        ExitState(currentState);
        
        // Yeni state'e gir
        currentState = newState;
        EnterState(newState);

        // Event tetikle
        OnStateChanged?.Invoke(previousState, newState);

        Debug.Log($"State: {previousState} -> {newState}");
    }

    /// <summary>
    /// State'den çıkılabilir mi kontrolü
    /// </summary>
    private bool CanExitState(PlayerState from, PlayerState to)
    {
        // Stunned iken sadece Idle'a geçebilir (stun süresi dolunca)
        if (from == PlayerState.Stunned && to != PlayerState.Idle)
            return false;

        // Attacking iken sadece belirli state'lere geçilebilir
        if (from == PlayerState.Attacking)
        {
            // Dash ile cancel edilebilir veya animasyon bitince Idle'a
            if (to != PlayerState.Dashing && to != PlayerState.Idle && to != PlayerState.Stunned)
                return false;
        }

        return true;
    }

    /// <summary>
    /// State'e girerken yapılacaklar
    /// </summary>
    private void EnterState(PlayerState state)
    {
        switch (state)
        {
            case PlayerState.Idle:
                anim.SetBool("IsRunning", false);
                anim.SetBool("IsGrounded", true);
                break;

            case PlayerState.Running:
                anim.SetBool("IsRunning", true);
                break;

            case PlayerState.Jumping:
                anim.SetTrigger("Jump");
                anim.SetBool("IsGrounded", false);
                break;

            case PlayerState.Falling:
                anim.SetBool("IsGrounded", false);
                break;

            case PlayerState.Attacking:
                // Combat script handles this
                break;

            case PlayerState.Blocking:
                anim.SetBool("IsBlocking", true);
                break;

            case PlayerState.Dashing:
                anim.SetTrigger("Dash");
                break;

            case PlayerState.Stunned:
                anim.SetTrigger("Stunned");
                break;

            case PlayerState.Parrying:
                anim.SetTrigger("Parry");
                break;
        }
    }

    /// <summary>
    /// State'den çıkarken yapılacaklar
    /// </summary>
    private void ExitState(PlayerState state)
    {
        switch (state)
        {
            case PlayerState.Running:
                anim.SetBool("IsRunning", false);
                break;

            case PlayerState.Blocking:
                anim.SetBool("IsBlocking", false);
                break;
        }
    }

    // === YARDIMCI FONKSİYONLAR ===

    /// <summary>
    /// Hareket edebilir mi?
    /// </summary>
    public bool CanMove()
    {
        return currentState == PlayerState.Idle ||
               currentState == PlayerState.Running ||
               currentState == PlayerState.Jumping ||
               currentState == PlayerState.Falling;
    }

    /// <summary>
    /// Saldırabilir mi?
    /// </summary>
    public bool CanAttack()
    {
        return currentState == PlayerState.Idle ||
               currentState == PlayerState.Running;
    }

    /// <summary>
    /// Zıplayabilir mi?
    /// </summary>
    public bool CanJump()
    {
        return currentState == PlayerState.Idle ||
               currentState == PlayerState.Running;
    }

    /// <summary>
    /// Dash yapabilir mi?
    /// </summary>
    public bool CanDash()
    {
        return currentState != PlayerState.Stunned &&
               currentState != PlayerState.Dashing;
    }

    /// <summary>
    /// Blok yapabilir mi?
    /// </summary>
    public bool CanBlock()
    {
        return currentState == PlayerState.Idle ||
               currentState == PlayerState.Running ||
               currentState == PlayerState.Jumping ||
               currentState == PlayerState.Falling;
    }

    /// <summary>
    /// Havada mı?
    /// </summary>
    public bool IsAirborne()
    {
        return currentState == PlayerState.Jumping ||
               currentState == PlayerState.Falling;
    }

    /// <summary>
    /// Combat state'inde mi?
    /// </summary>
    public bool IsInCombatState()
    {
        return currentState == PlayerState.Attacking ||
               currentState == PlayerState.Blocking ||
               currentState == PlayerState.Parrying ||
               currentState == PlayerState.Dashing;
    }
}
