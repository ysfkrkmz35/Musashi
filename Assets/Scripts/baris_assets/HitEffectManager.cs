using System.Collections;
using UnityEngine;

public class HitEffectManager : MonoBehaviour
{
    public static HitEffectManager Instance { get; private set; }

    [Header("Flash Effect")]
    [SerializeField] private Color hitFlashColor = Color.red;
    [SerializeField] private float hitFlashDuration = 0.1f;

    [Header("Damage Numbers")]
    [SerializeField] private bool enableDamageNumbers = true;
    [SerializeField] private GameObject damageNumberPrefab;
    [SerializeField] private Vector3 damageNumberOffset = new Vector3(0f, 0.5f, 0f);

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void TriggerHitEffect(GameObject target, int damage)
    {
        if (target == null) return;

        var spriteRenderer = target.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            StartCoroutine(FlashRoutine(spriteRenderer));
        }

        if (enableDamageNumbers && damageNumberPrefab != null)
        {
            SpawnDamageNumber(target.transform.position + damageNumberOffset, damage);
        }
    }

    private IEnumerator FlashRoutine(SpriteRenderer sr)
    {
        Color originalColor = sr.color;
        sr.color = hitFlashColor;
        yield return new WaitForSeconds(hitFlashDuration);
        sr.color = originalColor;
    }

    private void SpawnDamageNumber(Vector3 position, int damage)
    {
        GameObject numberObj = Instantiate(damageNumberPrefab, position, Quaternion.identity);
        var damageNum = numberObj.GetComponent<DamageNumber>();
        if (damageNum != null)
        {
            damageNum.Initialize(damage, position);
        }
    }
}
