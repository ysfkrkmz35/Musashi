using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using Musashi.Core.Progression;

namespace Musashi.Core.Managers
{
    /// <summary>
    /// Main game manager for Musashi duel journey
    /// Oyun akışını yöneten ana manager
    /// </summary>
    public class MusashiGameManager : MonoBehaviour
    {
        public static MusashiGameManager Instance { get; private set; }

        [Header("Journey Configuration")]
        [SerializeField] private int totalDuels = 7;           // Dağa tırmanış - 7 düello
        [SerializeField] private int currentDuelIndex = 0;

        [Header("Scene Names")]
        [SerializeField] private string duelSceneName = "baris";
        [SerializeField] private string campSceneName = "Camp";
        [SerializeField] private string finalBossSceneName = "FinalDuel";
        [SerializeField] private string victorySceneName = "Victory";

        [Header("Player Stats")]
        [SerializeField] private PlayerStats playerStats;

        [Header("References")]
        [SerializeField] private CampUpgradeSystem campSystem;

        // State
        private GameState currentState = GameState.Duel;
        private bool isPlayerAlive = true;

        public PlayerStats PlayerStats => playerStats;
        public int CurrentDuelIndex => currentDuelIndex;
        public int TotalDuels => totalDuels;
        public bool IsFinalDuel => currentDuelIndex >= totalDuels - 1;

        void Awake()
        {
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            if (playerStats == null)
                playerStats = new PlayerStats();
        }

        void Start()
        {
            // Subscribe to camp events
            if (campSystem != null)
            {
                campSystem.OnUpgradeSelected += OnUpgradeSelected;
                campSystem.OnCampClosed += OnCampClosed;
            }
        }

        /// <summary>
        /// Called when player wins a duel
        /// </summary>
        public void OnDuelVictory()
        {
            Debug.Log($"[GameManager] Duel {currentDuelIndex + 1} victory!");

            currentDuelIndex++;

            // Check if final duel
            if (currentDuelIndex >= totalDuels)
            {
                StartCoroutine(LoadFinalDuel());
            }
            else
            {
                // Open camp for upgrade
                StartCoroutine(OpenCampAfterDelay(2f));
            }
        }

        /// <summary>
        /// Called when player loses a duel
        /// </summary>
        public void OnDuelDefeat()
        {
            Debug.Log("[GameManager] Duel defeated!");
            isPlayerAlive = false;

            // Game over - restart journey
            StartCoroutine(RestartJourney(3f));
        }

        /// <summary>
        /// Open camp after victory delay
        /// </summary>
        IEnumerator OpenCampAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);

            currentState = GameState.Camp;

            // Find or create camp system
            if (campSystem == null)
                campSystem = FindObjectOfType<CampUpgradeSystem>();

            if (campSystem != null)
            {
                campSystem.OpenCamp();
            }
            else
            {
                Debug.LogWarning("[GameManager] No CampUpgradeSystem found! Continuing to next duel...");
                OnCampClosed();
            }
        }

        /// <summary>
        /// Upgrade selected event
        /// </summary>
        void OnUpgradeSelected(UpgradeType upgrade)
        {
            Debug.Log($"[GameManager] Upgrade selected: {upgrade}");
            // Stats already applied by CampUpgradeSystem
        }

        /// <summary>
        /// Camp closed - continue to next duel
        /// </summary>
        void OnCampClosed()
        {
            Debug.Log("[GameManager] Camp closed, loading next duel...");
            currentState = GameState.Duel;
            StartCoroutine(LoadNextDuel());
        }

        /// <summary>
        /// Load next duel scene
        /// </summary>
        IEnumerator LoadNextDuel()
        {
            yield return new WaitForSeconds(0.5f);

            // Reload duel scene (in real game, you'd have different enemy types)
            SceneManager.LoadScene(duelSceneName);
        }

        /// <summary>
        /// Load final boss duel
        /// </summary>
        IEnumerator LoadFinalDuel()
        {
            Debug.Log("[GameManager] Loading FINAL DUEL vs Shadow Musashi!");
            yield return new WaitForSeconds(2f);

            // Load final boss scene (or use same scene with different enemy)
            SceneManager.LoadScene(duelSceneName);
        }

        /// <summary>
        /// Restart journey from beginning
        /// </summary>
        IEnumerator RestartJourney(float delay)
        {
            yield return new WaitForSeconds(delay);

            // Reset stats
            if (playerStats != null)
                playerStats.Reset();

            currentDuelIndex = 0;
            isPlayerAlive = true;
            currentState = GameState.Duel;

            // Reload first duel
            SceneManager.LoadScene(duelSceneName);
        }

        /// <summary>
        /// Victory - journey complete
        /// </summary>
        public void OnJourneyComplete()
        {
            Debug.Log("[GameManager] JOURNEY COMPLETE! Musashi defeated Shadow Self!");
            // Load victory scene
            // SceneManager.LoadScene(victorySceneName);
        }

        /// <summary>
        /// Apply current stats to player controller
        /// Call this from PlayerDuelController.Start()
        /// </summary>
        public void ApplyStatsToPlayer(PlayerDuelControllerDirectional player)
        {
            if (playerStats == null || player == null) return;

            // Apply stats
            player.focusMax = playerStats.CurrentMaxFocus;
            player.focusRegenRate = playerStats.CurrentFocusRegen;
            player.attackCooldown = 0.65f / playerStats.CurrentAttackSpeed;

            Debug.Log($"[GameManager] Applied stats to player - Focus: {playerStats.CurrentMaxFocus}, Regen: {playerStats.CurrentFocusRegen}, Speed: {playerStats.CurrentAttackSpeed}x");
        }

        /// <summary>
        /// Apply stats to enemy (scale difficulty)
        /// </summary>
        public void ApplyDifficultyToEnemy(EnemyDuelControllerDirectional enemy)
        {
            if (enemy == null) return;

            // Scale enemy difficulty based on duel index
            float difficultyMultiplier = 1f + (currentDuelIndex * 0.15f);

            enemy.focusRegenRate = 5f * difficultyMultiplier;
            enemy.aggressionLevel = Mathf.Clamp01(0.3f + currentDuelIndex * 0.1f);
            enemy.predictionSkill = Mathf.Clamp01(0.2f + currentDuelIndex * 0.08f);

            Debug.Log($"[GameManager] Enemy difficulty scaled - Duel {currentDuelIndex + 1}/{totalDuels}, Aggression: {enemy.aggressionLevel:F2}, Prediction: {enemy.predictionSkill:F2}");
        }

        // Debug GUI
        void OnGUI()
        {
            if (!Application.isPlaying) return;

            GUILayout.BeginArea(new Rect(10, 10, 300, 150));
            GUILayout.Label($"=== MUSASHI JOURNEY ===");
            GUILayout.Label($"Duel: {currentDuelIndex + 1} / {totalDuels}");
            GUILayout.Label($"State: {currentState}");
            if (playerStats != null)
            {
                GUILayout.Label($"Upgrades: Speed {playerStats.speedUpgrades} | Power {playerStats.powerUpgrades} | Focus {playerStats.focusUpgrades}");
            }
            GUILayout.EndArea();
        }
    }

    public enum GameState
    {
        Duel,
        Camp,
        FinalDuel,
        Victory,
        Defeat
    }
}
