using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class EnemyDuelController : MonoBehaviour
{
    public Transform player;

    [Header("Focus")]
    public float focusMax = 100f;
    public float focusRegenRate = 5f;

    [Header("Costs")]
    public float lightCost = 10f;
    public float heavyCost = 18f;
    public float parryCost = 8f;

    [Header("Timings")]
    public Vector2 thinkEvery = new Vector2(1.2f, 2.0f); // karar aralığı
    public float attackCooldown = 0.7f;
    public float parryWindow = 0.22f;

    [Header("Duel Lock")]
    public float lockedX = 2.5f;
    public float lockedZ = 0f;

    private float _focus;
    private bool _canAct = true;
    private bool _parryActive = false;
    private Animator _anim;

    void Start()
    {
        _anim = GetComponent<Animator>();
        _focus = focusMax;
        UpdateFocusUI();
        LockToSpot();
        StartCoroutine(Brain());
    }

    void Update()
    {
        LockToSpot();
        FacePlayer();
        PassiveRegen();
    }

    IEnumerator Brain()
    {
        while (true)
        {
            if (!_canAct) { yield return null; continue; }

            float wait = Random.Range(thinkEvery.x, thinkEvery.y);
            yield return new WaitForSeconds(wait);

            // Basit karar: odak yeterliyse saldır; bazen parry dene; yoksa bekle
            if (_focus >= heavyCost && Random.value > 0.6f)
            {
                yield return StartCoroutine(AttackRoutine("attackHeavy", heavyCost));
            }
            else if (_focus >= lightCost && Random.value > 0.3f)
            {
                yield return StartCoroutine(AttackRoutine("attackLight", lightCost));
            }
            else if (_focus >= parryCost && Random.value > 0.7f)
            {
                yield return StartCoroutine(ParryRoutine());
            }
            else
            {
                // Bekle ve odak topla
                yield return new WaitForSeconds(0.4f);
            }
        }
    }

    IEnumerator AttackRoutine(string trigger, float cost)
    {
        _canAct = false;
        _anim.SetTrigger(trigger);
        UseFocus(cost);
        yield return new WaitForSeconds(attackCooldown);
        _canAct = true;
    }

    IEnumerator ParryRoutine()
    {
        _canAct = false;
        UseFocus(parryCost);
        _parryActive = true;
        _anim.SetTrigger("parry");
        yield return new WaitForSeconds(parryWindow);
        _parryActive = false;
        yield return new WaitForSeconds(0.15f);
        _canAct = true;
    }

    void PassiveRegen()
    {
        if (_focus < focusMax)
        {
            _focus = Mathf.Clamp(_focus + focusRegenRate * Time.deltaTime, 0f, focusMax);
            UpdateFocusUI();
        }
    }

    void UseFocus(float amt)
    {
        _focus = Mathf.Clamp(_focus - amt, 0f, focusMax);
        UpdateFocusUI();
    }

    void UpdateFocusUI()
    {
        // --- FIX IS HERE ---
        // Original was: FindFirstObjectByTypeFocusBar>(());
        // Correct syntax:
        var ui = FindFirstObjectByType<FocusBar>();

        if (ui != null)
        {
            ui.UpdateEnemyFocus(_focus, focusMax);
        }
    }

    void FacePlayer()
    {
        if (!player) return;
        Vector3 dir = (player.position - transform.position);
        dir.y = 0f; dir.z = 0f; // sadece X yönü
        if (dir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir.normalized, Vector3.up), 0.2f);
    }

    void LockToSpot()
    {
        var p = transform.position;
        p.x = lockedX; p.z = lockedZ;
        transform.position = p;
    }

    // Health çağırır
    public bool CanReceiveDamage()
    {
        return !_parryActive; // parry aktifken hasar yeme
    }
}