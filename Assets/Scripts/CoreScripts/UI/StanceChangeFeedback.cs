using UnityEngine;
using UnityEngine.UI;
using Musashi.Core.Combat;
using System.Collections;

/// <summary>
/// Düşman stance değişimlerine ANINDA tepki veren feedback sistemi
/// - Görsel flash efekt
/// - Ses uyarısı
/// - Ekran kenarında büyük yön göstergesi
/// - Oyuncuya tepki vermesi için zaman kazandırır
/// </summary>
public class StanceChangeFeedback : MonoBehaviour
{
    [Header("References")]
    public Image enemyStanceFlash; // Full screen flash
    public Text enemyStanceText; // Large direction text
    public AudioSource feedbackAudio;

    [Header("Flash Settings")]
    public Color stanceChangeFlashColor = new Color(1f, 0.3f, 0f, 0.5f); // Orange flash
    public float flashDuration = 0.15f;
    public float textDisplayDuration = 0.6f;

    [Header("Audio")]
    public AudioClip stanceChangeSound;
    public AudioClip attackCommitSound;
    public AudioClip executionWarningSound;

    [Header("Text Settings")]
    public float textPulseSpeed = 8f;

    private EnemyStanceController _enemy;
    private StanceBasedCombatSystem _enemyStance;
    private AttackDirection _lastEnemyStance = AttackDirection.None;
    private bool _isShowingFeedback = false;

    void Start()
    {
        _enemy = FindObjectOfType<EnemyStanceController>();
        if (_enemy != null)
        {
            _enemyStance = _enemy.GetComponent<StanceBasedCombatSystem>();
            if (_enemyStance != null)
            {
                _enemyStance.OnDefenseStanceChanged += OnEnemyStanceChanged;
                _enemyStance.OnAttackStanceChanged += OnEnemyAttackChanged;
                _enemyStance.OnExecutable += OnEnemyExecutable;
            }
        }

        // Initialize UI
        if (enemyStanceFlash != null)
            enemyStanceFlash.color = Color.clear;
        if (enemyStanceText != null)
            enemyStanceText.text = "";
    }

    void OnDestroy()
    {
        if (_enemyStance != null)
        {
            _enemyStance.OnDefenseStanceChanged -= OnEnemyStanceChanged;
            _enemyStance.OnAttackStanceChanged -= OnEnemyAttackChanged;
            _enemyStance.OnExecutable -= OnEnemyExecutable;
        }
    }

    void OnEnemyStanceChanged(AttackDirection newStance)
    {
        // Düşman stance değiştirdi - UYARI!
        if (newStance != _lastEnemyStance && newStance != AttackDirection.None)
        {
            _lastEnemyStance = newStance;
            StartCoroutine(ShowStanceChangeFeedback(newStance));
        }
    }

    void OnEnemyAttackChanged(AttackDirection attackStance)
    {
        // Düşman saldırı hazırlıyor - dikkat!
        if (attackStance != AttackDirection.None)
        {
            PlaySound(attackCommitSound);
        }
    }

    void OnEnemyExecutable()
    {
        // Düşman executable - öldür!
        PlaySound(executionWarningSound);
        StartCoroutine(ShowExecutionFeedback());
    }

    IEnumerator ShowStanceChangeFeedback(AttackDirection direction)
    {
        if (_isShowingFeedback) yield break;
        _isShowingFeedback = true;

        // Sound
        PlaySound(stanceChangeSound);

        // Flash effect
        if (enemyStanceFlash != null)
        {
            enemyStanceFlash.color = stanceChangeFlashColor;
        }

        // Text
        string directionText = GetDirectionText(direction);
        if (enemyStanceText != null)
        {
            enemyStanceText.text = $"ENEMY STANCE: {directionText}";
            enemyStanceText.color = Color.yellow;
        }

        // Fade out flash
        float elapsed = 0f;
        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(stanceChangeFlashColor.a, 0f, elapsed / flashDuration);
            if (enemyStanceFlash != null)
            {
                Color c = stanceChangeFlashColor;
                c.a = alpha;
                enemyStanceFlash.color = c;
            }
            yield return null;
        }

        if (enemyStanceFlash != null)
            enemyStanceFlash.color = Color.clear;

        // Pulse text
        float textElapsed = 0f;
        while (textElapsed < textDisplayDuration)
        {
            textElapsed += Time.deltaTime;
            if (enemyStanceText != null)
            {
                float pulse = Mathf.PingPong(textElapsed * textPulseSpeed, 1f);
                enemyStanceText.color = Color.Lerp(Color.yellow, Color.red, pulse);
            }
            yield return null;
        }

        // Clear text
        if (enemyStanceText != null)
            enemyStanceText.text = "";

        _isShowingFeedback = false;
    }

    IEnumerator ShowExecutionFeedback()
    {
        if (enemyStanceText != null)
        {
            enemyStanceText.text = "☠️ EXECUTABLE! FINISH HIM! ☠️";
            enemyStanceText.color = Color.red;
        }

        yield return new WaitForSeconds(2f);

        if (enemyStanceText != null)
            enemyStanceText.text = "";
    }

    string GetDirectionText(AttackDirection dir)
    {
        switch (dir)
        {
            case AttackDirection.Up: return "↑ UP";
            case AttackDirection.Down: return "↓ DOWN";
            case AttackDirection.Left: return "← LEFT";
            case AttackDirection.Right: return "→ RIGHT";
            default: return "NONE";
        }
    }

    void PlaySound(AudioClip clip)
    {
        if (feedbackAudio != null && clip != null)
        {
            feedbackAudio.PlayOneShot(clip);
        }
    }
}
