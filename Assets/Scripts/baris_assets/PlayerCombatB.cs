using UnityEngine;
using System.Collections;

public class PlayerCombatB : MonoBehaviour
{
    [Header("Referanslar")]
    public GameObject attackHitbox;

    [Header("Saldırı Hissi")]
    public float inputBufferTime = 0.4f;  // Tıklamayı ne kadar süre hafızada tutsun?

    private int comboIndex = 0;
    private float lastInputTime;          // Son tıklama zamanı
    private bool isAttacking = false;     // Şu an animasyon oynuyor mu?

    [Header("Blok & Parry")]
    public bool isBlocking = false;
    private float blockStartTime;
    public float parryWindow = 0.3f;

    [Header("Dash Ayarları")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    private float lastDashTime;

    private Animator anim;
    private PlayerController2D controller;
    private Rigidbody2D rb;

    void Start()
    {
        anim = GetComponent<Animator>();
        controller = GetComponent<PlayerController2D>();
        rb = GetComponent<Rigidbody2D>();
        if (attackHitbox != null) attackHitbox.SetActive(false);
    }

    void Update()
    {
        // 1. Tıklamayı Her Zaman Dinle ve Hafızaya Al
        if (Input.GetMouseButtonDown(0))
        {
            lastInputTime = Time.time;
        }

        // 2. Saldırı Başlatıcı (Sadece DURURKEN çalışır)
        // Eğer saldırmıyorsam VE buffer süresi içinde tıklama yapıldıysa -> BAŞLAT
        if (!isAttacking && !isBlocking && Time.time - lastInputTime < inputBufferTime)
        {
            StartComboAt(1); // Komboyu 1'den başlat
        }

        HandleBlock();
        HandleDash();
    }

    // Komboyu başlatan veya devam ettiren fonksiyon
    void StartComboAt(int index)
    {
        isAttacking = true;
        comboIndex = index;

        // Animasyon triggerlarını temizle ve yenisini ver
        anim.ResetTrigger("Attack1");
        anim.ResetTrigger("Attack2");
        anim.ResetTrigger("Attack3");
        anim.SetTrigger("Attack" + comboIndex);

        controller.canMove = false; // Hareketi kitle
    }

    // --- ANIMATION EVENTS (Bu sistemin kalbi burası) ---

    // Bu fonksiyonu animasyonun EN SON KARESİNE koymalısın.
    public void EndAttack()
    {
        // Animasyon bittiğinde kontrol et:
        // Oyuncu animasyon sırasında tekrar tuşa bastı mı?
        if (Time.time - lastInputTime < inputBufferTime)
        {
            // EVET BASTI -> Bir sonraki saldırıya geç
            comboIndex++;
            if (comboIndex > 3) comboIndex = 1; // 3'ten sonra 1'e dön
            StartComboAt(comboIndex);
        }
        else
        {
            // HAYIR BASMADI -> Komboyu bitir ve normale dön
            isAttacking = false;
            comboIndex = 0; // Sayacı sıfırla
            controller.canMove = true;
            if (attackHitbox != null) attackHitbox.SetActive(false);
        }
    }

    public void EnableHitbox()
    {
        if (attackHitbox != null) attackHitbox.SetActive(true);
    }

    public void DisableHitbox()
    {
        if (attackHitbox != null) attackHitbox.SetActive(false);
    }

    // --- DİĞER FONKSİYONLAR ---
    void HandleBlock()
    {
        if (isAttacking) return; // Saldırırken blok yapama

        if (Input.GetKeyDown(KeyCode.F))
        {
            isBlocking = true;
            blockStartTime = Time.time;
            anim.SetBool("IsBlocking", true);
            controller.canMove = false;
        }
        else if (Input.GetKeyUp(KeyCode.F))
        {
            isBlocking = false;
            anim.SetBool("IsBlocking", false);
            controller.canMove = true;
        }
    }

    void HandleDash()
    {
        if (Input.GetKeyDown(KeyCode.V) && Time.time > lastDashTime + dashCooldown && !isBlocking && !isAttacking)
        {
            StartCoroutine(PerformDash());
        }
    }

    IEnumerator PerformDash()
    {
        controller.canMove = false;
        anim.SetTrigger("Roll");
        lastDashTime = Time.time;
        float direction = transform.localScale.x;
        rb.linearVelocity = new Vector2(direction * dashSpeed, 0);
        yield return new WaitForSeconds(dashDuration);
        rb.linearVelocity = Vector2.zero;
        controller.canMove = true;
    }

    public bool CheckParry()
    {
        if (isBlocking && (Time.time - blockStartTime <= parryWindow)) return true;
        return false;
    }

    public bool CheckBlock() { return isBlocking; }

    // ... Diğer fonksiyonların altına ekle ...

    // HASAR ALINCA ÇAĞRILACAK ACİL DURUM FONKSİYONU
    public void OnHitReset()
    {
        // Saldırıyı iptal et
        isAttacking = false;
        comboIndex = 0; // Komboyu sıfırla

        // Kılıç hitbox'ı açık kaldıysa kapat (Kendini kesme)
        if (attackHitbox != null) attackHitbox.SetActive(false);

        // Hareketi tekrar aç (BU ÇOK ÖNEMLİ)
        if (controller != null) controller.canMove = true;

        // Bloklamayı durdur
        isBlocking = false;
        if (anim != null) anim.SetBool("IsBlocking", false);
    }
}
