using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerDuelController : MonoBehaviour
{
    [Header("Focus")]
    public float focusMax = 100f;
    public float focusRegenRate = 6f;
    public float meditateBonus = 12f;

    [Header("Costs")]
    public float lightCost = 10f;
    public float heavyCost = 20f;
    public float parryCost = 8f;
    public float dodgeCost = 12f;

    [Header("Timings")]
    public float attackCooldown = 0.65f;
    public float parryWindow = 0.25f;      // parry aktifken gelen darbe karşılanır
    public float dodgeIFrame = 0.30f;      // dodge sırasında hasar almaz
    public float actionLock = 0.15f;       // anim başı kısa kilit

    [Header("Keys")]
    public KeyCode lightKey = KeyCode.Mouse0;
    public KeyCode heavyKey = KeyCode.Mouse1;
    public KeyCode parryKey = KeyCode.LeftShift;
    public KeyCode dodgeKey = KeyCode.Space;
    public KeyCode meditateKey = KeyCode.R;

    [Header("Duel Lock")]
    public float lockedX = 0f;
    public float lockedZ = 0f;

    private float _focus;
    private bool _canAct = true;
    private bool _isParryActive = false;
    private bool _isInvulnerable = false;
    private Animator _anim;
    private Health _hp;

    void Start()
    {
        _anim = GetComponent<Animator>();
        _hp = GetComponent<Health>();
        _focus = focusMax;
        UpdateFocusUI();
        LockToSpot();
    }

    void Update()
    {
        LockToSpot();
        HandleInput();
        PassiveRegen();
    }

    void HandleInput()
    {
        if (!_canAct) return;

        // Meditasyon: odak doldurur, tamamen savunmasız
        if (Input.GetKey(meditateKey))
        {
            _anim.SetBool("isMeditating", true);
            GainFocus((focusRegenRate + meditateBonus) * Time.deltaTime);
            return;
        }
        else
        {
            _anim.SetBool("isMeditating", false);
        }

        if (Input.GetKeyDown(lightKey) && _focus >= lightCost)
        {
            StartCoroutine(AttackRoutine("lightAttack", lightCost));
            return;
        }

        if (Input.GetKeyDown(heavyKey) && _focus >= heavyCost)
        {
            StartCoroutine(AttackRoutine("heavyAttack", heavyCost));
            return;
        }

        if (Input.GetKeyDown(parryKey) && _focus >= parryCost)
        {
            StartCoroutine(ParryRoutine());
            return;
        }

        if (Input.GetKeyDown(dodgeKey) && _focus >= dodgeCost)
        {
            StartCoroutine(DodgeRoutine());
            return;
        }
    }

    IEnumerator AttackRoutine(string trigger, float cost)
    {
        _canAct = false;
        _anim.SetTrigger(trigger);
        UseFocus(cost);
        yield return new WaitForSeconds(actionLock);
        yield return new WaitForSeconds(attackCooldown);
        _canAct = true;
    }

    IEnumerator ParryRoutine()
    {
        _canAct = false;
        UseFocus(parryCost);
        _isParryActive = true;
        _anim.SetTrigger("parry");
        yield return new WaitForSeconds(parryWindow);
        _isParryActive = false;
        yield return new WaitForSeconds(actionLock);
        _canAct = true;
    }

    IEnumerator DodgeRoutine()
    {
        _canAct = false;
        UseFocus(dodgeCost);
        _isInvulnerable = true;
        _anim.SetTrigger("dodge");
        yield return new WaitForSeconds(dodgeIFrame);
        _isInvulnerable = false;
        yield return new WaitForSeconds(actionLock);
        _canAct = true;
    }

    void PassiveRegen()
    {
        if (_focus < focusMax && !_anim.GetBool("isMeditating"))
            GainFocus(focusRegenRate * Time.deltaTime);
    }

    void UseFocus(float amt)
    {
        _focus = Mathf.Clamp(_focus - amt, 0f, focusMax);
        UpdateFocusUI();
    }

    void GainFocus(float amt)
    {
        _focus = Mathf.Clamp(_focus + amt, 0f, focusMax);
        UpdateFocusUI();
    }

    void UpdateFocusUI()
    {
        // --- FIX IS HERE ---
        // Original: var ui = FindFirstObjectByTypeFocusBar>(());
        // Correct Syntax:
        var ui = FindFirstObjectByType<FocusBar>();

        if (ui != null)
        {
            ui.UpdatePlayerFocus(_focus, focusMax);
        }
    }

    void LockToSpot()
    {
        var p = transform.position;
        p.x = lockedX; p.z = lockedZ;
        transform.position = p;
    }

    // Health tarafından çağrılacak hasar kuralı
    public bool CanReceiveDamage()
    {
        if (_isInvulnerable) return false;
        // Parry penceresinde gelen saldırıyı geri çevirebilirsin (örnek):
        // burada özel geri tepmeyi tetikleyen bir event yayınlayabilirsiniz.
        return !_isParryActive; // parry açıkken direkt hasar yeme
    }
}