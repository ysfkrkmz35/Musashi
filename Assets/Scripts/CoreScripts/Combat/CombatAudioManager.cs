using UnityEngine;
using Musashi.Core.Combat;

namespace Musashi.Core.Audio
{
    /// <summary>
    /// Handles all combat audio (sword clashes, parries, hits)
    /// Dövüş ses efektleri yöneticisi
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class CombatAudioManager : MonoBehaviour
    {
        [Header("Audio Clips")]
        [SerializeField] private AudioClip[] lightAttackSounds;
        [SerializeField] private AudioClip[] heavyAttackSounds;
        [SerializeField] private AudioClip[] swordClashSounds;
        [SerializeField] private AudioClip[] parrySounds;
        [SerializeField] private AudioClip[] dodgeSounds;
        [SerializeField] private AudioClip[] hitSounds;
        [SerializeField] private AudioClip[] deathSounds;
        [SerializeField] private AudioClip meditationSound;

        [Header("Volume Settings")]
        [SerializeField] private float attackVolume = 0.5f;
        [SerializeField] private float clashVolume = 0.7f;
        [SerializeField] private float parryVolume = 0.8f;
        [SerializeField] private float dodgeVolume = 0.4f;
        [SerializeField] private float hitVolume = 0.6f;
        [SerializeField] private float deathVolume = 0.9f;

        [Header("Pitch Variation")]
        [SerializeField] private float pitchVariation = 0.1f;

        private AudioSource audioSource;

        void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }

        public void PlayLightAttack()
        {
            PlayRandomClip(lightAttackSounds, attackVolume);
        }

        public void PlayHeavyAttack()
        {
            PlayRandomClip(heavyAttackSounds, attackVolume);
        }

        public void PlaySwordClash()
        {
            PlayRandomClip(swordClashSounds, clashVolume);
        }

        public void PlayParry(bool success)
        {
            if (success)
            {
                // Successful parry - higher pitch
                PlayRandomClip(parrySounds, parryVolume, 1f + pitchVariation);
            }
            else
            {
                // Failed parry - lower pitch
                PlayRandomClip(parrySounds, parryVolume * 0.7f, 1f - pitchVariation);
            }
        }

        public void PlayDodge()
        {
            PlayRandomClip(dodgeSounds, dodgeVolume);
        }

        public void PlayHit()
        {
            PlayRandomClip(hitSounds, hitVolume);
        }

        public void PlayDeath()
        {
            PlayRandomClip(deathSounds, deathVolume);
        }

        public void PlayMeditation(bool loop = false)
        {
            if (meditationSound == null) return;

            audioSource.clip = meditationSound;
            audioSource.loop = loop;
            audioSource.volume = 0.3f;
            audioSource.Play();
        }

        public void StopMeditation()
        {
            if (audioSource.clip == meditationSound)
                audioSource.Stop();
        }

        private void PlayRandomClip(AudioClip[] clips, float volume, float pitch = 1f)
        {
            if (clips == null || clips.Length == 0) return;

            AudioClip clip = clips[Random.Range(0, clips.Length)];
            if (clip == null) return;

            audioSource.pitch = pitch + Random.Range(-pitchVariation, pitchVariation);
            audioSource.PlayOneShot(clip, volume);
        }

        /// <summary>
        /// Subscribe to combat events
        /// </summary>
        public void SubscribeToCombatEvents(DirectionalCombatSystem combat)
        {
            if (combat == null) return;

            combat.OnCombatResult += HandleCombatResult;
        }

        private void HandleCombatResult(CombatResult result)
        {
            switch (result)
            {
                case CombatResult.Blocked:
                    PlaySwordClash();
                    break;
                case CombatResult.ParrySuccess:
                    PlayParry(true);
                    break;
                case CombatResult.ParryFailed:
                    PlayParry(false);
                    PlayHit();
                    break;
                case CombatResult.Dodged:
                    PlayDodge();
                    break;
                case CombatResult.Hit:
                    PlayHit();
                    break;
            }
        }
    }
}
