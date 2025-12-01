using UnityEngine;

public class MobAI : MonoBehaviour
{
    [Header("Ayarlar")]
    public float moveSpeed = 3f;
    public float chaseRange = 8f;  // Oyuncuyu fark etme mesafesi
    public float attackRange = 1.5f; // Vurma mesafesi
    public float attackCooldown = 2f; // Kaç saniyede bir vursun?

    [Header("Referanslar")]
    public GameObject attackHitbox; // Mobun silahındaki/elindeki hitbox
    private Transform player;
    private Rigidbody2D rb;
    private Animator anim;
    private CharacterStats stats;

    private float lastAttackTime;
    private bool isAttacking = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        stats = GetComponent<CharacterStats>();

        // Oyuncuyu Tag ile bul (Player objesinin Tag'i "Player" olmalı!)
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        if (attackHitbox != null) attackHitbox.SetActive(false);
    }

    void Update()
    {
        // 1. Eğer ölüyseniz hiçbir şey yapma
        if (stats.isDead)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // 2. Oyuncu yoksa bekle
        if (player == null) return;

        // 3. Mesafeyi ölç
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (isAttacking)
        {
            // Saldırıyorsa hareket etmesin
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // --- YAPAY ZEKA KARAR MEKANİZMASI ---

        if (distanceToPlayer > chaseRange)
        {
            // DURUM: IDLE (Oyuncu çok uzak)
            StopMoving();
        }
        else if (distanceToPlayer <= chaseRange && distanceToPlayer > attackRange)
        {
            // DURUM: CHASE (Kovala)
            MoveTowardsPlayer();
        }
        else if (distanceToPlayer <= attackRange)
        {
            // DURUM: ATTACK (Vur)
            StopMoving();
            if (Time.time > lastAttackTime + attackCooldown)
            {
                Attack();
            }
        }
    }

    void MoveTowardsPlayer()
    {
        // Yönü bul (Sağ mı sol mu?)
        Vector2 direction = (player.position - transform.position).normalized;
        float dirX = direction.x;

        // Hareket et
        rb.linearVelocity = new Vector2(dirX * moveSpeed, rb.linearVelocity.y);

        // Animasyon
        anim.SetBool("IsRunning", true);

        // Yüzünü dön
        if (dirX > 0) transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);
        else if (dirX < 0) transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);
    }

    void StopMoving()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        anim.SetBool("IsRunning", false);
    }

    void Attack()
    {
        lastAttackTime = Time.time;
        isAttacking = true;
        anim.SetTrigger("Attack");
    }

    // --- ANIMATION EVENTS (Mob Animasyonlarına Eklenecek) ---

    // Animasyonun başı/ortası (Vuruş başlıyor)
    public void EnableHitbox()
    {
        if (attackHitbox != null) attackHitbox.SetActive(true);
    }

    // Animasyonun vuruş anı bitişi (Vuruş bitti)
    public void DisableHitbox()
    {
        if (attackHitbox != null) attackHitbox.SetActive(false);
    }

    // Animasyonun EN SON karesi (Reset - Çok Önemli!)
    public void EndAttack()
    {
        isAttacking = false;
        if (attackHitbox != null) attackHitbox.SetActive(false);
    }

    // Editörde mesafeleri görmek için
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}