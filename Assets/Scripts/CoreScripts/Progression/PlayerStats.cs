using UnityEngine;
using System;

namespace Musashi.Core.Progression
{
    /// <summary>
    /// Player stat tracking and upgrade management
    /// Oyuncu istatistiklerini ve yükseltmeleri yöneten sistem
    /// </summary>
    [System.Serializable]
    public class PlayerStats
    {
        [Header("Base Stats")]
        public float baseAttackSpeed = 1f;
        public float baseAttackDamage = 15f;
        public float baseFocusRegen = 6f;
        public float baseMaxFocus = 100f;

        [Header("Upgrades - Hız (Speed)")]
        public int speedUpgrades = 0;
        public float speedPerUpgrade = 0.15f;        // +15% attack speed per upgrade
        public float dodgeWindowPerUpgrade = 0.05f;  // +0.05s dodge window

        [Header("Upgrades - Güç (Power)")]
        public int powerUpgrades = 0;
        public float damagePerUpgrade = 5f;          // +5 damage per upgrade
        public float stanceBreakPerUpgrade = 0.1f;   // +10% stance break power

        [Header("Upgrades - Odak (Focus)")]
        public int focusUpgrades = 0;
        public float focusRegenPerUpgrade = 2f;      // +2 focus/sec per upgrade
        public float maxFocusPerUpgrade = 10f;       // +10 max focus per upgrade

        // Events
        public event Action<UpgradeType> OnUpgradeApplied;

        // Computed stats
        public float CurrentAttackSpeed => baseAttackSpeed * (1f + speedUpgrades * speedPerUpgrade);
        public float CurrentAttackDamage => baseAttackDamage + (powerUpgrades * damagePerUpgrade);
        public float CurrentFocusRegen => baseFocusRegen + (focusUpgrades * focusRegenPerUpgrade);
        public float CurrentMaxFocus => baseMaxFocus + (focusUpgrades * maxFocusPerUpgrade);
        public float CurrentDodgeWindow => 0.3f + (speedUpgrades * dodgeWindowPerUpgrade);
        public float CurrentStanceBreak => 1f + (powerUpgrades * stanceBreakPerUpgrade);

        public int TotalUpgrades => speedUpgrades + powerUpgrades + focusUpgrades;

        /// <summary>
        /// Apply upgrade to stats
        /// </summary>
        public void ApplyUpgrade(UpgradeType type)
        {
            switch (type)
            {
                case UpgradeType.Speed:
                    speedUpgrades++;
                    Debug.Log($"[Stats] Speed upgraded! Level: {speedUpgrades}");
                    break;
                case UpgradeType.Power:
                    powerUpgrades++;
                    Debug.Log($"[Stats] Power upgraded! Level: {powerUpgrades}");
                    break;
                case UpgradeType.Focus:
                    focusUpgrades++;
                    Debug.Log($"[Stats] Focus upgraded! Level: {focusUpgrades}");
                    break;
            }

            OnUpgradeApplied?.Invoke(type);
            LogStats();
        }

        /// <summary>
        /// Reset all upgrades (for new run)
        /// </summary>
        public void Reset()
        {
            speedUpgrades = 0;
            powerUpgrades = 0;
            focusUpgrades = 0;
            Debug.Log("[Stats] Stats reset for new run");
        }

        /// <summary>
        /// Debug log current stats
        /// </summary>
        public void LogStats()
        {
            Debug.Log($"[Stats] Total Upgrades: {TotalUpgrades}\n" +
                      $"Speed: {speedUpgrades} | Attack Speed: {CurrentAttackSpeed:F2}x | Dodge: {CurrentDodgeWindow:F2}s\n" +
                      $"Power: {powerUpgrades} | Damage: {CurrentAttackDamage:F1} | Stance: {CurrentStanceBreak:F2}x\n" +
                      $"Focus: {focusUpgrades} | Regen: {CurrentFocusRegen:F1}/s | Max: {CurrentMaxFocus:F0}");
        }

        /// <summary>
        /// Get upgrade description for UI
        /// </summary>
        public string GetUpgradeDescription(UpgradeType type)
        {
            switch (type)
            {
                case UpgradeType.Speed:
                    return $"Hız\n+{(speedPerUpgrade * 100):F0}% Saldırı Hızı\n+{dodgeWindowPerUpgrade:F2}s Dodge Penceresi\nSeviye: {speedUpgrades}";
                case UpgradeType.Power:
                    return $"Güç\n+{damagePerUpgrade:F0} Hasar\n+{(stanceBreakPerUpgrade * 100):F0}% Duruş Kırma\nSeviye: {powerUpgrades}";
                case UpgradeType.Focus:
                    return $"Odak\n+{focusRegenPerUpgrade:F0} Odak/sn\n+{maxFocusPerUpgrade:F0} Max Odak\nSeviye: {focusUpgrades}";
                default:
                    return "";
            }
        }
    }

    /// <summary>
    /// Upgrade type enum
    /// </summary>
    public enum UpgradeType
    {
        Speed,  // Hız
        Power,  // Güç
        Focus   // Odak
    }
}
