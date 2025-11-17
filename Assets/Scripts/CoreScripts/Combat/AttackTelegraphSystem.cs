using UnityEngine;
using System.Collections;

namespace Musashi.Core.Combat
{
    /// <summary>
    /// Attack telegraph system - shows enemy attack direction before they strike
    /// Düşman saldırısını önceden gösterir, oyuncuya hazırlanma zamanı verir
    /// </summary>
    public class AttackTelegraphSystem : MonoBehaviour
    {
        [Header("Telegraph Settings")]
        [SerializeField] private float telegraphDuration = 1.0f;      // Saldırı öncesi bekleme süresi
        [SerializeField] private float slowMotionScale = 0.5f;        // Zaman yavaşlama %50
        [SerializeField] private bool enableSlowMotion = true;         // Slow-mo açık/kapalı

        [Header("Visual Feedback")]
        [SerializeField] private Color telegraphColor = new Color(1f, 0.3f, 0.3f, 1f); // Kırmızı uyarı
        [SerializeField] private float pulseSpeed = 8f;                // Yanıp sönme hızı

        [Header("Audio")]
        [SerializeField] private AudioClip telegraphSound;             // "Woosh" uyarı sesi
        [SerializeField] private float telegraphVolume = 0.6f;

        private AudioSource audioSource;
        private DirectionalCombatSystem combatSystem;
        private bool isTelegraphing = false;

        public bool IsTelegraphing => isTelegraphing;
        public float TelegraphDuration => telegraphDuration;

        void Awake()
        {
            combatSystem = GetComponent<DirectionalCombatSystem>();
            audioSource = GetComponent<AudioSource>();

            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
        }

        /// <summary>
        /// Start telegraphing an attack (called by enemy before attacking)
        /// </summary>
        public IEnumerator TelegraphAttack(AttackDirection direction)
        {
            if (isTelegraphing) yield break;

            isTelegraphing = true;

            // Set attack direction early so UI can show it
            if (combatSystem != null)
            {
                combatSystem.SetAttackDirection(direction);
            }

            // Play sound
            PlayTelegraphSound();

            // Start slow motion
            if (enableSlowMotion)
            {
                StartSlowMotion();
            }

            // Wait telegraph duration
            float elapsed = 0f;
            while (elapsed < telegraphDuration)
            {
                elapsed += Time.unscaledDeltaTime; // Use unscaled time for accurate timing during slow-mo
                yield return null;
            }

            // End slow motion
            if (enableSlowMotion)
            {
                EndSlowMotion();
            }

            isTelegraphing = false;
        }

        /// <summary>
        /// Start slow motion effect
        /// </summary>
        void StartSlowMotion()
        {
            Time.timeScale = slowMotionScale;
            Time.fixedDeltaTime = 0.02f * slowMotionScale;
        }

        /// <summary>
        /// End slow motion effect
        /// </summary>
        void EndSlowMotion()
        {
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;
        }

        /// <summary>
        /// Play telegraph warning sound
        /// </summary>
        void PlayTelegraphSound()
        {
            if (telegraphSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(telegraphSound, telegraphVolume);
            }
        }

        /// <summary>
        /// Get telegraph color for UI pulsing
        /// </summary>
        public Color GetTelegraphColor(float time)
        {
            if (!isTelegraphing) return Color.clear;

            // Pulse effect
            float pulse = Mathf.PingPong(time * pulseSpeed, 1f);
            return Color.Lerp(telegraphColor, Color.white, pulse);
        }

        void OnDestroy()
        {
            // Ensure time scale is reset
            if (Time.timeScale != 1f)
            {
                Time.timeScale = 1f;
                Time.fixedDeltaTime = 0.02f;
            }
        }
    }
}
