using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Player Combat Sistemi - FSM ile entegre
/// Saldırı Komboları, Blok, Parry, Dash
/// </summary>
[RequireComponent(typeof(PlayerStateMachine))]
public class PlayerCombat : MonoBehaviour
{
    [Header("Dash Ayarları")]
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    [Header("Blok & Posture Sistemi")]
    public float maxBlockPosture = 100f;
    public float postureRegenRate = 15f;
    public float stunDuration = 2f;
    public Slider postureSlider;

    [Header("Parry (Kusursuz Blok)")]
    public float perfectBlockWindow = 0.2f;

    [Header("Saldırı Ayarları")]
    public float attackRange = 1.5f;
    public Transform attackPoint;
    public LayerMask enemyLayers;
    public float[] attackDamage = { 10f, 15f, 25f }; // Her kombo vuruşu için farklı hasar
    public float comboResetTime = 1f;

    // === REFERANSLAR ===
    private Rigidbody2D rb;
    private Animator anim;
    private PlayerStateMachine stateMachine;
    private PlayerMovement movement;

    // === KOMBO DEĞİŞKENLERİ ===
    private int comboStep = 0;
    private float lastAttackTime = 0;

    // === BLOK DEĞİŞKENLERİ ===
    private float currentPosture;
    private float blockStartTime;
    private float stunTimer;

    // === DASH DEĞİŞKENLERİ ===
    private float dashEndTime;
    private float lastDashTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        stateMachine = GetComponent<PlayerStateMachine>();
        movement = GetComponent<PlayerMovement>();
    }

    void Start()
    {
        currentPosture = maxBlockPosture;

        if (postureSlider != null)
        {
            postureSlider.maxValue = maxBlockPosture;
            postureSlider.value = currentPosture;
        }
    }

    void Update()
    {
        // Stun iyileşme
        if (stateMachine.CurrentState == PlayerStateMachine.PlayerState.Stunned)
        {
            RecoverStun();
            UpdateUI();
            return;
        }

        // Dash bitişi kontrolü
        if (stateMachine.CurrentState == PlayerStateMachine.PlayerState.Dashing)
        {
            if (Time.time >= dashEndTime)
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                stateMachine.ChangeState(PlayerStateMachine.PlayerState.Idle);
            }
            return;
        }

        HandlePostureRegen();
        HandleInput();
        UpdateUI();
    }

    void HandleInput()
    {
        // === DASH ===
        if (Input.GetKeyDown(KeyCode.Space) && CanDash())
        {
            PerformDash();
        }

        // === BLOK ===
        if (Input.GetKeyDown(KeyCode.F) && stateMachine.CanBlock())
        {
            StartBlocking();
        }
        else if (Input.GetKey(KeyCode.F) && stateMachine.CurrentState == PlayerStateMachine.PlayerState.Blocking)
        {
            // Blok tutmaya devam et
        }
        else if (Input.GetKeyUp(KeyCode.F) && stateMachine.CurrentState == PlayerStateMachine.PlayerState.Blocking)
        {
            StopBlocking();
        }

        // === SALDIRI (KOMBO) ===
        if (Input.GetMouseButtonDown(0) && stateMachine.CanAttack())
        {
            PerformAttack();
        }
    }

    // === SALDIRI ===
    void PerformAttack()
    {
        // Kombo zaman aşımı kontrolü
        if (Time.time - lastAttackTime > comboResetTime)
        {
            comboStep = 0;
        }

        lastAttackTime = Time.time;
        comboStep++;

        // 3 vuruşluk döngü
        if (comboStep > 3) comboStep = 1;

        stateMachine.ChangeState(PlayerStateMachine.PlayerState.Attacking);
        anim.SetTrigger("Attack" + comboStep);

        // Hasar ver (attack point üzerinden)
        DealDamage();

        Debug.Log("Saldırı: " + comboStep);

        // Animasyon bitiminde state sıfırla
        float attackDuration = GetAttackDuration(comboStep);
        Invoke(nameof(ResetFromAttack), attackDuration);
    }

    void DealDamage()
    {
        if (attackPoint == null) return;

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        float damage = attackDamage[Mathf.Clamp(comboStep - 1, 0, attackDamage.Length - 1)];

        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
            if (enemyAI != null)
            {
                enemyAI.TakeDamage(damage, gameObject);
            }
            Debug.Log("Düşmana vuruldu: " + enemy.name + " - Hasar: " + damage);
        }
    }

    float GetAttackDuration(int step)
    {
        // Her saldırı animasyonunun süresi (ayarlanabilir)
        switch (step)
        {
            case 1: return 0.35f;
            case 2: return 0.3f;
            case 3: return 0.5f;
            default: return 0.4f;
        }
    }

    void ResetFromAttack()
    {
        if (stateMachine.CurrentState == PlayerStateMachine.PlayerState.Attacking)
        {
            stateMachine.ChangeState(PlayerStateMachine.PlayerState.Idle);
        }
    }

    // === BLOK ===
    void StartBlocking()
    {
        stateMachine.ChangeState(PlayerStateMachine.PlayerState.Blocking);
        blockStartTime = Time.time;
        anim.SetBool("IsBlocking", true);
    }

    void StopBlocking()
    {
        anim.SetBool("IsBlocking", false);
        stateMachine.ChangeState(PlayerStateMachine.PlayerState.Idle);
    }

    // === DASH ===
    bool CanDash()
    {
        return Time.time > lastDashTime + dashCooldown && stateMachine.CanDash();
    }

    void PerformDash()
    {
        stateMachine.ChangeState(PlayerStateMachine.PlayerState.Dashing);
        lastDashTime = Time.time;
        dashEndTime = Time.time + dashDuration;

        // Karakterin baktığı yöne dash
        float dashDirection = transform.localScale.x > 0 ? 1 : -1;
        rb.linearVelocity = new Vector2(dashDirection * dashSpeed, 0);

        anim.SetTrigger("Dash");
    }

    // === HASAR ALMA VE PARRY ===
    public void TakeDamage(float damage, GameObject attacker)
    {
        // Dash sırasında i-frame
        if (stateMachine.CurrentState == PlayerStateMachine.PlayerState.Dashing)
            return;

        if (stateMachine.CurrentState == PlayerStateMachine.PlayerState.Blocking)
        {
            // PARRY kontrolü
            if (Time.time - blockStartTime <= perfectBlockWindow)
            {
                Debug.Log("PARRY! Düşman Stunlandı!");
                stateMachine.ChangeState(PlayerStateMachine.PlayerState.Parrying);

                // Düşmanı stunla
                EnemyAI enemy = attacker.GetComponent<EnemyAI>();
                if (enemy != null)
                {
                    enemy.GetStunned(1.5f);
                }

                // Kısa parry animasyonu sonrası idle'a dön
                Invoke(nameof(ResetFromParry), 0.3f);
                return;
            }

            // Normal blok
            currentPosture -= damage * 2f;
            Debug.Log("Bloklandı. Posture: " + currentPosture);

            if (currentPosture <= 0)
            {
                GetStunned();
            }
        }
        else
        {
            // Direkt hasar
            Debug.Log("Hasar Alındı: " + damage);
            // TODO: Health sistemi eklenecek
            // health -= damage;
        }
    }

    void ResetFromParry()
    {
        if (stateMachine.CurrentState == PlayerStateMachine.PlayerState.Parrying)
        {
            stateMachine.ChangeState(PlayerStateMachine.PlayerState.Idle);
        }
    }

    // === STUN ===
    public void GetStunned()
    {
        stateMachine.ChangeState(PlayerStateMachine.PlayerState.Stunned);
        currentPosture = 0;
        stunTimer = stunDuration;
        anim.SetBool("IsBlocking", false);
        Debug.Log("Karakter STUNLANDI!");
    }

    void RecoverStun()
    {
        stunTimer -= Time.deltaTime;

        if (stunTimer <= 0)
        {
            currentPosture = maxBlockPosture;
            stateMachine.ChangeState(PlayerStateMachine.PlayerState.Idle);
        }
    }

    // === POSTURE REGEN ===
    void HandlePostureRegen()
    {
        if (stateMachine.CurrentState != PlayerStateMachine.PlayerState.Blocking &&
            stateMachine.CurrentState != PlayerStateMachine.PlayerState.Stunned)
        {
            currentPosture += postureRegenRate * Time.deltaTime;
            currentPosture = Mathf.Clamp(currentPosture, 0, maxBlockPosture);
        }
    }

    // === UI ===
    void UpdateUI()
    {
        if (postureSlider != null)
        {
            postureSlider.value = currentPosture;
        }
    }

    // === GIZMOS ===
    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
