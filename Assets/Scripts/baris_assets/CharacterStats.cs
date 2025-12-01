using UnityEngine;
using UnityEngine.UI; // <-- BU SATIRI MUTLAKA EKLE!

public class CharacterStats : MonoBehaviour
{
    [Header("Can Ayarları")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("UI")]
    public Slider healthSlider; // <-- YENİ: Unity'den buraya Slider sürükleyeceğiz

    [Header("Görsel Ayarlar")]
    public float deathYOffset = 0f;

    [Header("Durum")]
    public bool isDead = false;

    private Animator anim;
    private PlayerCombatB playerCombat;

    void Start()
    {
        currentHealth = maxHealth;
        anim = GetComponent<Animator>();
        playerCombat = GetComponent<PlayerCombatB>();

        // --- YENİ: Slider Başlangıç Ayarı ---
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
        // ------------------------------------
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        if (playerCombat != null)
        {
            if (playerCombat.CheckParry()) return;
            if (playerCombat.CheckBlock())
            {
                damage /= 2;
            }
        }

        currentHealth -= damage;
        anim.SetTrigger("GetHit");

        // --- YENİ: Slider Güncelleme ---
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }
        // -------------------------------

        // Player Reset (Stuck Fix)
        if (playerCombat != null) playerCombat.OnHitReset();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        anim.SetBool("IsDead", true);

        transform.position = new Vector3(transform.position.x, transform.position.y - deathYOffset, transform.position.z);

        if (healthSlider != null) healthSlider.gameObject.SetActive(false); // Ölünce can barını gizle

        GetComponent<Collider2D>().enabled = false;
        GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
        GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;

        if (GetComponent<PlayerController2D>()) GetComponent<PlayerController2D>().enabled = false;
        if (GetComponent<PlayerCombatB>()) GetComponent<PlayerCombatB>().enabled = false;
        if (GetComponent<MobAI>()) GetComponent<MobAI>().enabled = false;
    }
}