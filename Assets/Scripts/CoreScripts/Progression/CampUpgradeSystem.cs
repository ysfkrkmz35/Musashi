using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

namespace Musashi.Core.Progression
{
    /// <summary>
    /// Camp upgrade system - handles post-duel upgrade selection
    /// Her düellodan sonra kamp ekranı ve yükseltme seçimi
    /// </summary>
    public class CampUpgradeSystem : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject campPanel;
        [SerializeField] private Button speedButton;
        [SerializeField] private Button powerButton;
        [SerializeField] private Button focusButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Text speedText;
        [SerializeField] private Text powerText;
        [SerializeField] private Text focusText;
        [SerializeField] private Text titleText;

        [Header("Animation")]
        [SerializeField] private float cardAnimDuration = 0.5f;
        [SerializeField] private AnimationCurve cardAnimCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Player Stats")]
        [SerializeField] private PlayerStats playerStats;

        // Events
        public event Action<UpgradeType> OnUpgradeSelected;
        public event Action OnCampClosed;

        private UpgradeType selectedUpgrade;
        private bool hasSelected = false;

        void Awake()
        {
            if (playerStats == null)
                playerStats = new PlayerStats();

            // Setup button listeners
            if (speedButton != null)
                speedButton.onClick.AddListener(() => SelectUpgrade(UpgradeType.Speed));
            if (powerButton != null)
                powerButton.onClick.AddListener(() => SelectUpgrade(UpgradeType.Power));
            if (focusButton != null)
                focusButton.onClick.AddListener(() => SelectUpgrade(UpgradeType.Focus));
            if (continueButton != null)
            {
                continueButton.onClick.AddListener(CloseCamp);
                continueButton.gameObject.SetActive(false);
            }

            // Hide panel initially
            if (campPanel != null)
                campPanel.SetActive(false);
        }

        /// <summary>
        /// Open camp after winning a duel
        /// </summary>
        public void OpenCamp()
        {
            if (campPanel != null)
                campPanel.SetActive(true);

            hasSelected = false;
            if (continueButton != null)
                continueButton.gameObject.SetActive(false);

            // Enable upgrade buttons
            if (speedButton != null) speedButton.interactable = true;
            if (powerButton != null) powerButton.interactable = true;
            if (focusButton != null) focusButton.interactable = true;

            // Update text
            UpdateUpgradeTexts();

            if (titleText != null)
                titleText.text = "Musashi Kamp Kuruyor...";

            // Pause game
            Time.timeScale = 0f;

            // Animate cards in
            StartCoroutine(AnimateCardsIn());
        }

        /// <summary>
        /// Update upgrade descriptions
        /// </summary>
        void UpdateUpgradeTexts()
        {
            if (speedText != null && playerStats != null)
                speedText.text = playerStats.GetUpgradeDescription(UpgradeType.Speed);
            if (powerText != null && playerStats != null)
                powerText.text = playerStats.GetUpgradeDescription(UpgradeType.Power);
            if (focusText != null && playerStats != null)
                focusText.text = playerStats.GetUpgradeDescription(UpgradeType.Focus);
        }

        /// <summary>
        /// Select an upgrade
        /// </summary>
        void SelectUpgrade(UpgradeType type)
        {
            if (hasSelected) return;

            selectedUpgrade = type;
            hasSelected = true;

            // Apply upgrade to stats
            if (playerStats != null)
                playerStats.ApplyUpgrade(type);

            // Disable buttons
            if (speedButton != null) speedButton.interactable = false;
            if (powerButton != null) powerButton.interactable = false;
            if (focusButton != null) focusButton.interactable = false;

            // Show continue button
            if (continueButton != null)
                continueButton.gameObject.SetActive(true);

            // Update UI
            UpdateUpgradeTexts();

            if (titleText != null)
            {
                string upgradeName = type == UpgradeType.Speed ? "Hız" :
                                     type == UpgradeType.Power ? "Güç" : "Odak";
                titleText.text = $"{upgradeName} Yükseltildi!";
            }

            // Notify listeners
            OnUpgradeSelected?.Invoke(type);

            Debug.Log($"[Camp] Upgrade selected: {type}");
        }

        /// <summary>
        /// Close camp and continue journey
        /// </summary>
        void CloseCamp()
        {
            if (campPanel != null)
                campPanel.SetActive(false);

            // Resume game
            Time.timeScale = 1f;

            // Notify listeners
            OnCampClosed?.Invoke();

            Debug.Log("[Camp] Closing camp, continuing journey");
        }

        /// <summary>
        /// Animate upgrade cards appearing
        /// </summary>
        IEnumerator AnimateCardsIn()
        {
            if (speedButton == null || powerButton == null || focusButton == null)
                yield break;

            // Start with cards scaled down
            speedButton.transform.localScale = Vector3.zero;
            powerButton.transform.localScale = Vector3.zero;
            focusButton.transform.localScale = Vector3.zero;

            float elapsed = 0f;
            while (elapsed < cardAnimDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = cardAnimCurve.Evaluate(elapsed / cardAnimDuration);

                speedButton.transform.localScale = Vector3.one * t;

                if (elapsed > cardAnimDuration * 0.2f)
                    powerButton.transform.localScale = Vector3.one * Mathf.Clamp01((elapsed - cardAnimDuration * 0.2f) / (cardAnimDuration * 0.8f));

                if (elapsed > cardAnimDuration * 0.4f)
                    focusButton.transform.localScale = Vector3.one * Mathf.Clamp01((elapsed - cardAnimDuration * 0.4f) / (cardAnimDuration * 0.6f));

                yield return null;
            }

            speedButton.transform.localScale = Vector3.one;
            powerButton.transform.localScale = Vector3.one;
            focusButton.transform.localScale = Vector3.one;
        }

        /// <summary>
        /// Get current player stats
        /// </summary>
        public PlayerStats GetPlayerStats()
        {
            return playerStats;
        }

        /// <summary>
        /// Reset stats for new run
        /// </summary>
        public void ResetStats()
        {
            if (playerStats != null)
                playerStats.Reset();
        }
    }
}
