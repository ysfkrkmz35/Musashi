using UnityEngine;

/// <summary>
/// Temel Düşman AI - FSM tabanlı
/// Patrol, Chase, Attack, Stunned states
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour
{
    public enum EnemyState
    {
        Idle,
        Patrol,
        Chase,
        Attack,
        Stunned,
        Dead
    }

    [Header("State")]
    [SerializeField] private EnemyState currentState = EnemyState.Idle;
    public EnemyState CurrentState => currentState;

    [Header("Hareket")]
    public float moveSpeed = 3f;
    public float chaseSpeed = 5f;

    [Header("Algılama")]
    public float detectionRange = 5f;
    public float attackRange = 1.5f;
    public LayerMask playerLayer;

    [Header("Saldırı")]
    public float attackDamage = 15f;
    public float attackCooldown = 1.5f;
    public float attackDuration = 0.5f;
    public Transform attackPoint;

    [Header("Patrol")]
    public Transform[] patrolPoints;
    public float patrolWaitTime = 2f;

    [Header("Sağlık")]
    public float maxHealth = 100f;
    private float currentHealth;

    // === REFERANSLAR ===
    private Rigidbody2D rb;
    private Animator anim;
    private Transform player;

    // === PATROL DEĞİŞKENLERİ ===
    private int currentPatrolIndex = 0;
    private float patrolWaitTimer;
    private bool isWaiting = false;

    // === SALDIRI DEĞİŞKENLERİ ===
    private float lastAttackTime;

    // === STUN DEĞİŞKENLERİ ===
    private float stunTimer;

    // === FACING ===
    private bool facingRight = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Start()
    {
        currentHealth = maxHealth;
        
        // Player'ı bul
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        ChangeState(EnemyState.Patrol);
    }

    void Update()
    {
        if (player == null) return;

        switch (currentState)
        {
            case EnemyState.Idle:
                UpdateIdle();
                break;
            case EnemyState.Patrol:
                UpdatePatrol();
                break;
            case EnemyState.Chase:
                UpdateChase();
                break;
            case EnemyState.Attack:
                UpdateAttack();
                break;
            case EnemyState.Stunned:
                UpdateStunned();
                break;
            case EnemyState.Dead:
                // Hiçbir şey yapma
                break;
        }

        UpdateAnimator();
    }

    // === STATE UPDATES ===
    void UpdateIdle()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        // Player görürse chase'e geç
        if (IsPlayerInRange(detectionRange))
        {
            ChangeState(EnemyState.Chase);
        }
    }

    void UpdatePatrol()
    {
        // Patrol noktası yoksa idle kal
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            ChangeState(EnemyState.Idle);
            return;
        }

        // Player görürse chase'e geç
        if (IsPlayerInRange(detectionRange))
        {
            ChangeState(EnemyState.Chase);
            return;
        }

        // Bekleme modunda
        if (isWaiting)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            patrolWaitTimer -= Time.deltaTime;

            if (patrolWaitTimer <= 0)
            {
                isWaiting = false;
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            }
            return;
        }

        // Patrol noktasına git
        Transform targetPoint = patrolPoints[currentPatrolIndex];
        float direction = Mathf.Sign(targetPoint.position.x - transform.position.x);

        UpdateFacing(direction);
        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);

        // Noktaya ulaştı mı?
        if (Mathf.Abs(targetPoint.position.x - transform.position.x) < 0.2f)
        {
            isWaiting = true;
            patrolWaitTimer = patrolWaitTime;
        }
    }

    void UpdateChase()
    {
        // Player menzil dışına çıktıysa patrol'a dön
        if (!IsPlayerInRange(detectionRange * 1.5f))
        {
            ChangeState(EnemyState.Patrol);
            return;
        }

        // Saldırı menzilindeyse saldır
        if (IsPlayerInRange(attackRange))
        {
            ChangeState(EnemyState.Attack);
            return;
        }

        // Player'a doğru git
        float direction = Mathf.Sign(player.position.x - transform.position.x);
        UpdateFacing(direction);
        rb.linearVelocity = new Vector2(direction * chaseSpeed, rb.linearVelocity.y);
    }

    void UpdateAttack()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        // Player menzil dışına çıktıysa chase'e geç
        if (!IsPlayerInRange(attackRange * 1.5f))
        {
            ChangeState(EnemyState.Chase);
            return;
        }

        // Saldırı cooldown kontrolü
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            PerformAttack();
        }
    }

    void UpdateStunned()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        stunTimer -= Time.deltaTime;

        if (stunTimer <= 0)
        {
            ChangeState(EnemyState.Idle);
        }
    }

    // === SALDIRI ===
    void PerformAttack()
    {
        lastAttackTime = Time.time;
        anim.SetTrigger("Attack");

        // Hasar verme (biraz gecikmeyle, animasyonla senkron)
        Invoke(nameof(DealDamageToPlayer), 0.2f);
    }

    void DealDamageToPlayer()
    {
        if (currentState != EnemyState.Attack) return;
        if (!IsPlayerInRange(attackRange)) return;

        PlayerCombat playerCombat = player.GetComponent<PlayerCombat>();
        if (playerCombat != null)
        {
            playerCombat.TakeDamage(attackDamage, gameObject);
        }
    }

    // === HASAR ALMA ===
    public void TakeDamage(float damage, GameObject attacker)
    {
        if (currentState == EnemyState.Dead) return;

        currentHealth -= damage;
        anim.SetTrigger("Hit");

        Debug.Log($"{gameObject.name} hasar aldı: {damage}. Kalan can: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void GetStunned(float duration = 1.5f)
    {
        stunTimer = duration;
        ChangeState(EnemyState.Stunned);
        anim.SetTrigger("Stunned");
        Debug.Log($"{gameObject.name} stunlandı! Süre: {duration}");
    }

    void Die()
    {
        ChangeState(EnemyState.Dead);
        anim.SetTrigger("Die");
        rb.linearVelocity = Vector2.zero;

        // Collider'ı kapat
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        Debug.Log($"{gameObject.name} öldü!");

        // Opsiyonel: Belli süre sonra yok et
        // Destroy(gameObject, 3f);
    }

    // === STATE CHANGE ===
    void ChangeState(EnemyState newState)
    {
        if (currentState == newState) return;
        if (currentState == EnemyState.Dead) return; // Ölüyse state değişmez

        Debug.Log($"Enemy State: {currentState} -> {newState}");
        currentState = newState;
    }

    // === YARDIMCI METODLAR ===
    bool IsPlayerInRange(float range)
    {
        if (player == null) return false;
        return Vector2.Distance(transform.position, player.position) <= range;
    }

    void UpdateFacing(float direction)
    {
        if (direction > 0 && !facingRight)
        {
            Flip();
        }
        else if (direction < 0 && facingRight)
        {
            Flip();
        }
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    void UpdateAnimator()
    {
        anim.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
        anim.SetBool("IsGrounded", true); // Basit versiyon
    }

    // === GIZMOS ===
    void OnDrawGizmosSelected()
    {
        // Detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Attack point
        if (attackPoint != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(attackPoint.position, 0.5f);
        }

        // Patrol points
        if (patrolPoints != null)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                if (patrolPoints[i] != null)
                {
                    Gizmos.DrawWireSphere(patrolPoints[i].position, 0.3f);
                    if (i > 0 && patrolPoints[i - 1] != null)
                    {
                        Gizmos.DrawLine(patrolPoints[i - 1].position, patrolPoints[i].position);
                    }
                }
            }
        }
    }
}
