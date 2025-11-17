using UnityEngine;
using System;

namespace Musashi.Core.Combat
{
    /// <summary>
    /// Stance-Based Combat System
    /// Her iki taraf da stance seçer, sonra commit eder
    /// Başarısız savunma = ekstra focus kaybı
    /// Focus = 0 → Execution (infaz)
    /// </summary>
    public class StanceBasedCombatSystem : MonoBehaviour
    {
        [Header("Current Stance Selection")]
        public AttackDirection selectedAttackStance = AttackDirection.None;
        public AttackDirection selectedDefenseStance = AttackDirection.Up;
        public bool isCommitted = false;

        [Header("Focus System")]
        [Tooltip("Failed defense (hit) - heavy penalty")]
        public float failedDefensePenalty = 35f;

        [Header("Execution")]
        [Tooltip("Below this focus, can be executed")]
        public float executionThreshold = 20f;

        [Header("State")]
        public bool isDodging = false;
        public bool isInCounterWindow = false;
        public bool isExecutable = false;

        private float dodgeTimer = 0f;
        private float counterTimer = 0f;
        private const float DODGE_DURATION = 0.4f;
        private const float COUNTER_DURATION = 1.2f;

        // Events
        public event Action<AttackDirection> OnAttackStanceChanged;
        public event Action<AttackDirection> OnDefenseStanceChanged;
        public event Action<CombatResult> OnCombatResult;
        public event Action OnCounterWindowOpened;
        public event Action OnExecutable;

        // Properties
        public AttackDirection CurrentAttackStance => selectedAttackStance;
        public AttackDirection CurrentDefenseStance => selectedDefenseStance;
        public bool IsDodging => isDodging;
        public bool IsInCounterWindow => isInCounterWindow;
        public bool IsExecutable => isExecutable;

        void Update()
        {
            UpdateTimers();
        }

        void UpdateTimers()
        {
            // Dodge i-frames
            if (isDodging)
            {
                dodgeTimer -= Time.deltaTime;
                if (dodgeTimer <= 0f)
                {
                    isDodging = false;
                }
            }

            // Counter window
            if (isInCounterWindow)
            {
                counterTimer -= Time.deltaTime;
                if (counterTimer <= 0f)
                {
                    isInCounterWindow = false;
                }
            }
        }

        /// <summary>
        /// Set attack stance (can change before commit)
        /// </summary>
        public void SetAttackStance(AttackDirection direction)
        {
            if (isCommitted) return; // Can't change after commit

            selectedAttackStance = direction;
            OnAttackStanceChanged?.Invoke(direction);
            Debug.Log($"[Stance] Attack stance set: {direction}");
        }

        /// <summary>
        /// Set defense stance (can change anytime)
        /// </summary>
        public void SetDefenseStance(AttackDirection direction)
        {
            selectedDefenseStance = direction;
            OnDefenseStanceChanged?.Invoke(direction);
            Debug.Log($"[Stance] Defense stance set: {direction}");
        }

        /// <summary>
        /// Commit to attack - locks in the stance
        /// </summary>
        public void CommitAttack()
        {
            if (selectedAttackStance == AttackDirection.None)
            {
                Debug.LogWarning("[Stance] Can't commit - no attack stance selected!");
                return;
            }

            isCommitted = true;
            Debug.Log($"[Stance] ⚔️ COMMITTED to attack from {selectedAttackStance}");
        }

        /// <summary>
        /// Reset after attack completes
        /// </summary>
        public void ResetCommit()
        {
            isCommitted = false;
            selectedAttackStance = AttackDirection.None;
        }

        /// <summary>
        /// Activate dodge
        /// </summary>
        public void ActivateDodge()
        {
            isDodging = true;
            dodgeTimer = DODGE_DURATION;
            Debug.Log("[Stance] 🌀 Dodge activated!");
        }

        /// <summary>
        /// Process incoming attack and determine result
        /// </summary>
        public (CombatResult result, bool applyDamage, float focusCost) ProcessIncomingAttack(DirectionalAttackData attackData)
        {
            // EXECUTABLE = CAN'T DEFEND! Always takes hit!
            if (isExecutable)
            {
                OnCombatResult?.Invoke(CombatResult.Hit);
                Debug.Log("[Combat] ☠️ EXECUTABLE! No defense possible!");
                return (CombatResult.Hit, true, 0f); // No focus penalty, they're already dead
            }

            // Check dodge first
            if (isDodging)
            {
                OnCombatResult?.Invoke(CombatResult.Dodged);
                return (CombatResult.Dodged, false, 0f);
            }

            // Check if defense matches attack direction
            if (selectedDefenseStance == attackData.Direction)
            {
                // SAME DIRECTION = BLOCK (no cost, controller will reward!)
                OnCombatResult?.Invoke(CombatResult.Blocked);
                return (CombatResult.Blocked, false, 0f);
            }
            else
            {
                // DIFFERENT DIRECTION = HIT (failed defense)
                OnCombatResult?.Invoke(CombatResult.Hit);

                // HEAVY FOCUS PENALTY for failed defense!
                return (CombatResult.Hit, true, failedDefensePenalty);
            }
        }

        /// <summary>
        /// Special parry - requires active input at right time
        /// Different direction + active parry = counter window
        /// </summary>
        public (CombatResult result, bool applyDamage, float focusCost) ProcessIncomingAttackWithParry(DirectionalAttackData attackData)
        {
            // Check dodge first
            if (isDodging)
            {
                OnCombatResult?.Invoke(CombatResult.Dodged);
                return (CombatResult.Dodged, false, 0f);
            }

            // Different direction = PARRY SUCCESS
            if (selectedDefenseStance != attackData.Direction)
            {
                OpenCounterWindow();
                OnCombatResult?.Invoke(CombatResult.ParrySuccess);
                return (CombatResult.ParrySuccess, false, 0f);
            }
            else
            {
                // Same direction = PARRY FAILED (you guessed wrong)
                OnCombatResult?.Invoke(CombatResult.ParryFailed);
                return (CombatResult.ParryFailed, true, failedDefensePenalty);
            }
        }

        /// <summary>
        /// Open counter window after successful parry
        /// </summary>
        void OpenCounterWindow()
        {
            isInCounterWindow = true;
            counterTimer = COUNTER_DURATION;
            OnCounterWindowOpened?.Invoke();
            Debug.Log("[Stance] 🎯 COUNTER WINDOW OPENED! 1.2s to strike!");
        }

        /// <summary>
        /// Check if target can be executed
        /// </summary>
        public void CheckExecutable(float currentFocus)
        {
            bool wasExecutable = isExecutable;
            isExecutable = currentFocus <= executionThreshold;

            if (isExecutable && !wasExecutable)
            {
                OnExecutable?.Invoke();
                Debug.Log("[Stance] ☠️ EXECUTABLE! Focus too low!");
            }
        }
    }
}
