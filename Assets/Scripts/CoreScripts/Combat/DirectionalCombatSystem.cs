using UnityEngine;
using System;

namespace Musashi.Core.Combat
{
    /// <summary>
    /// Core directional combat system - handles attack/defense direction logic
    /// Yönlü saldırı ve savunma mantığını yöneten merkezi sistem
    /// </summary>
    public class DirectionalCombatSystem : MonoBehaviour
    {
        [Header("Current Stance")]
        [SerializeField] private AttackDirection currentDefenseDirection = AttackDirection.Up;
        [SerializeField] private AttackDirection currentAttackDirection = AttackDirection.Up;

        [Header("State")]
        [SerializeField] private bool isParrying = false;
        [SerializeField] private bool isDodging = false;
        [SerializeField] private bool isInCounterWindow = false;

        [Header("Timing Windows")]
        [SerializeField] private float parryWindow = 0.25f;
        [SerializeField] private float counterWindow = 0.8f;
        [SerializeField] private float dodgeWindow = 0.3f;

        [Header("Focus Costs")]
        [SerializeField] private float blockFocusCost = 5f;
        [SerializeField] private float parryFocusCost = 8f;
        [SerializeField] private float parryFailPenalty = 15f;

        // Events
        public event Action<AttackDirection> OnAttackDirectionChanged;
        public event Action<AttackDirection> OnDefenseDirectionChanged;
        public event Action<CombatResult> OnCombatResult;
        public event Action OnCounterWindowOpened;
        public event Action OnCounterWindowClosed;

        private float parryTimer = 0f;
        private float counterTimer = 0f;
        private float dodgeTimer = 0f;

        // References
        private PlayerDuelController playerController;
        private EnemyDuelController enemyController;

        public AttackDirection CurrentAttackDirection => currentAttackDirection;
        public AttackDirection CurrentDefenseDirection => currentDefenseDirection;
        public bool IsParrying => isParrying;
        public bool IsDodging => isDodging;
        public bool IsInCounterWindow => isInCounterWindow;

        private void Update()
        {
            UpdateTimers();
        }

        private void UpdateTimers()
        {
            // Parry window
            if (isParrying)
            {
                parryTimer -= Time.deltaTime;
                if (parryTimer <= 0f)
                {
                    isParrying = false;
                }
            }

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
                    CloseCounterWindow();
                }
            }
        }

        /// <summary>
        /// Set current attack direction (called from input)
        /// </summary>
        public void SetAttackDirection(AttackDirection direction)
        {
            if (currentAttackDirection != direction)
            {
                currentAttackDirection = direction;
                OnAttackDirectionChanged?.Invoke(direction);
            }
        }

        /// <summary>
        /// Set current defense direction (called from input)
        /// </summary>
        public void SetDefenseDirection(AttackDirection direction)
        {
            if (currentDefenseDirection != direction)
            {
                currentDefenseDirection = direction;
                OnDefenseDirectionChanged?.Invoke(direction);
            }
        }

        /// <summary>
        /// Activate parry state
        /// </summary>
        public void ActivateParry()
        {
            isParrying = true;
            parryTimer = parryWindow;
        }

        /// <summary>
        /// Activate dodge state
        /// </summary>
        public void ActivateDodge()
        {
            isDodging = true;
            dodgeTimer = dodgeWindow;
        }

        /// <summary>
        /// Process incoming attack with directional logic
        /// Returns combat result and whether damage should be applied
        /// </summary>
        public (CombatResult result, bool applyDamage, float focusCost) ProcessIncomingAttack(DirectionalAttackData attackData)
        {
            // Dodge check - i-frames bypass all
            if (isDodging)
            {
                OnCombatResult?.Invoke(CombatResult.Dodged);
                return (CombatResult.Dodged, false, 0f);
            }

            // Parry check - must be active AND on different direction
            if (isParrying)
            {
                // Parry başarılı: Farklı yönde olmalı (For Honor mantığı)
                if (currentDefenseDirection != attackData.Direction)
                {
                    OpenCounterWindow();
                    OnCombatResult?.Invoke(CombatResult.ParrySuccess);
                    isParrying = false; // Parry consumed
                    return (CombatResult.ParrySuccess, false, parryFocusCost);
                }
                else
                {
                    // Parry başarısız: Aynı yönde parry denendi
                    OnCombatResult?.Invoke(CombatResult.ParryFailed);
                    isParrying = false;
                    return (CombatResult.ParryFailed, true, parryFocusCost + parryFailPenalty);
                }
            }

            // Block check - must be on same direction
            if (currentDefenseDirection == attackData.Direction)
            {
                OnCombatResult?.Invoke(CombatResult.Blocked);
                return (CombatResult.Blocked, false, blockFocusCost);
            }

            // No defense - hit confirmed
            OnCombatResult?.Invoke(CombatResult.Hit);
            return (CombatResult.Hit, true, 0f);
        }

        /// <summary>
        /// Open counter-attack window after successful parry
        /// </summary>
        private void OpenCounterWindow()
        {
            isInCounterWindow = true;
            counterTimer = counterWindow;
            OnCounterWindowOpened?.Invoke();
        }

        /// <summary>
        /// Close counter-attack window
        /// </summary>
        private void CloseCounterWindow()
        {
            isInCounterWindow = false;
            OnCounterWindowClosed?.Invoke();
        }

        /// <summary>
        /// Get random direction (for AI)
        /// </summary>
        public static AttackDirection GetRandomDirection()
        {
            int random = UnityEngine.Random.Range(1, 5); // 1-4
            return (AttackDirection)random;
        }

        /// <summary>
        /// Get opposite direction (for AI prediction)
        /// </summary>
        public static AttackDirection GetOppositeDirection(AttackDirection dir)
        {
            switch (dir)
            {
                case AttackDirection.Up: return AttackDirection.Down;
                case AttackDirection.Down: return AttackDirection.Up;
                case AttackDirection.Left: return AttackDirection.Right;
                case AttackDirection.Right: return AttackDirection.Left;
                default: return AttackDirection.Up;
            }
        }

        // Debug
        private void OnGUI()
        {
            if (!Application.isPlaying) return;

            GUILayout.BeginArea(new Rect(10, 150, 300, 200));
            GUILayout.Label($"Attack Dir: {currentAttackDirection}");
            GUILayout.Label($"Defense Dir: {currentDefenseDirection}");
            GUILayout.Label($"Parrying: {isParrying} ({parryTimer:F2}s)");
            GUILayout.Label($"Dodging: {isDodging} ({dodgeTimer:F2}s)");
            GUILayout.Label($"Counter Window: {isInCounterWindow} ({counterTimer:F2}s)");
            GUILayout.EndArea();
        }
    }
}
