using UnityEngine;
using UnityEngine.UI;
using Musashi.Core.Combat;

namespace Musashi.Core.UI
{
    /// <summary>
    /// Displays directional attack/defense indicators for both player and enemy
    /// For Honor tarzı yön göstergeleri
    /// </summary>
    public class DirectionalIndicatorUI : MonoBehaviour
    {
        [Header("Player Indicators")]
        [SerializeField] private GameObject playerAttackIndicator;
        [SerializeField] private GameObject playerDefenseIndicator;
        [SerializeField] private Image playerUpArrow;
        [SerializeField] private Image playerDownArrow;
        [SerializeField] private Image playerLeftArrow;
        [SerializeField] private Image playerRightArrow;

        [Header("Enemy Indicators")]
        [SerializeField] private GameObject enemyAttackIndicator;
        [SerializeField] private GameObject enemyDefenseIndicator;
        [SerializeField] private Image enemyUpArrow;
        [SerializeField] private Image enemyDownArrow;
        [SerializeField] private Image enemyLeftArrow;
        [SerializeField] private Image enemyRightArrow;

        [Header("Colors")]
        [SerializeField] private Color attackColor = new Color(1f, 0.2f, 0.2f, 0.8f); // Red
        [SerializeField] private Color defenseColor = new Color(0.2f, 0.5f, 1f, 0.8f); // Blue
        [SerializeField] private Color inactiveColor = new Color(0.3f, 0.3f, 0.3f, 0.3f); // Gray
        [SerializeField] private Color parryColor = new Color(1f, 0.8f, 0.2f, 1f); // Gold
        [SerializeField] private Color counterColor = new Color(0f, 1f, 0.5f, 1f); // Green

        [Header("References")]
        private DirectionalCombatSystem playerCombat;
        private DirectionalCombatSystem enemyCombat;

        [Header("Animation")]
        [SerializeField] private float pulseSpeed = 2f;
        [SerializeField] private float pulseAmount = 0.3f;

        private bool isPlayerInCounter = false;
        private float pulseTimer = 0f;

        void Start()
        {
            // --- FIX START ---
            // Removed garbage text "()()nal>();" and "()()al>();"
            var player = FindFirstObjectByType<PlayerDuelControllerDirectional>();
            var enemy = FindFirstObjectByType<EnemyDuelControllerDirectional>();
            // --- FIX END ---

            if (player != null)
            {
                playerCombat = player.GetComponent<DirectionalCombatSystem>();
                if (playerCombat != null)
                {
                    playerCombat.OnAttackDirectionChanged += UpdatePlayerAttackIndicator;
                    playerCombat.OnDefenseDirectionChanged += UpdatePlayerDefenseIndicator;
                    playerCombat.OnCounterWindowOpened += OnPlayerCounterOpened;
                    playerCombat.OnCounterWindowClosed += OnPlayerCounterClosed;
                }
            }

            if (enemy != null)
            {
                enemyCombat = enemy.GetComponent<DirectionalCombatSystem>();
                if (enemyCombat != null)
                {
                    enemyCombat.OnAttackDirectionChanged += UpdateEnemyAttackIndicator;
                    enemyCombat.OnDefenseDirectionChanged += UpdateEnemyDefenseIndicator;
                }
            }

            // Initialize
            UpdatePlayerAttackIndicator(AttackDirection.Up);
            UpdatePlayerDefenseIndicator(AttackDirection.Up);
            UpdateEnemyAttackIndicator(AttackDirection.Up);
            UpdateEnemyDefenseIndicator(AttackDirection.Up);
        }

        void OnDestroy()
        {
            if (playerCombat != null)
            {
                playerCombat.OnAttackDirectionChanged -= UpdatePlayerAttackIndicator;
                playerCombat.OnDefenseDirectionChanged -= UpdatePlayerDefenseIndicator;
                playerCombat.OnCounterWindowOpened -= OnPlayerCounterOpened;
                playerCombat.OnCounterWindowClosed -= OnPlayerCounterClosed;
            }

            if (enemyCombat != null)
            {
                enemyCombat.OnAttackDirectionChanged -= UpdateEnemyAttackIndicator;
                enemyCombat.OnDefenseDirectionChanged -= UpdateEnemyDefenseIndicator;
            }
        }

        void Update()
        {
            // Pulse effect for counter window
            if (isPlayerInCounter)
            {
                pulseTimer += Time.deltaTime * pulseSpeed;
                float pulse = 1f + Mathf.Sin(pulseTimer) * pulseAmount;
                if (playerAttackIndicator != null)
                    playerAttackIndicator.transform.localScale = Vector3.one * pulse;
            }

            // Check parry state for color change
            UpdateParryVisuals();
        }

        void UpdatePlayerAttackIndicator(AttackDirection dir)
        {
            if (playerUpArrow != null) playerUpArrow.color = (dir == AttackDirection.Up) ? attackColor : inactiveColor;
            if (playerDownArrow != null) playerDownArrow.color = (dir == AttackDirection.Down) ? attackColor : inactiveColor;
            if (playerLeftArrow != null) playerLeftArrow.color = (dir == AttackDirection.Left) ? attackColor : inactiveColor;
            if (playerRightArrow != null) playerRightArrow.color = (dir == AttackDirection.Right) ? attackColor : inactiveColor;
        }

        void UpdatePlayerDefenseIndicator(AttackDirection dir)
        {
            Color defColor = (playerCombat != null && playerCombat.IsParrying) ? parryColor : defenseColor;

            // LOGIC FIX: Removed "&& dir != AttackDirection..." checks.
            // If you keep that check, the arrow never turns to the active color because the if-statement becomes false when the direction matches.
            if (playerUpArrow != null)
                playerUpArrow.color = (dir == AttackDirection.Up) ? defColor : inactiveColor;

            if (playerDownArrow != null)
                playerDownArrow.color = (dir == AttackDirection.Down) ? defColor : inactiveColor;

            if (playerLeftArrow != null)
                playerLeftArrow.color = (dir == AttackDirection.Left) ? defColor : inactiveColor;

            if (playerRightArrow != null)
                playerRightArrow.color = (dir == AttackDirection.Right) ? defColor : inactiveColor;
        }

        void UpdateEnemyAttackIndicator(AttackDirection dir)
        {
            if (enemyUpArrow != null) enemyUpArrow.color = (dir == AttackDirection.Up) ? attackColor : inactiveColor;
            if (enemyDownArrow != null) enemyDownArrow.color = (dir == AttackDirection.Down) ? attackColor : inactiveColor;
            if (enemyLeftArrow != null) enemyLeftArrow.color = (dir == AttackDirection.Left) ? attackColor : inactiveColor;
            if (enemyRightArrow != null) enemyRightArrow.color = (dir == AttackDirection.Right) ? attackColor : inactiveColor;
        }

        void UpdateEnemyDefenseIndicator(AttackDirection dir)
        {
            Color defColor = (enemyCombat != null && enemyCombat.IsParrying) ? parryColor : defenseColor;

            // LOGIC FIX: Removed "&& dir != AttackDirection..." checks here as well.
            if (enemyUpArrow != null)
                enemyUpArrow.color = (dir == AttackDirection.Up) ? defColor : inactiveColor;

            if (enemyDownArrow != null)
                enemyDownArrow.color = (dir == AttackDirection.Down) ? defColor : inactiveColor;

            if (enemyLeftArrow != null)
                enemyLeftArrow.color = (dir == AttackDirection.Left) ? defColor : inactiveColor;

            if (enemyRightArrow != null)
                enemyRightArrow.color = (dir == AttackDirection.Right) ? defColor : inactiveColor;
        }

        void UpdateParryVisuals()
        {
            // Update player defense color when parrying
            if (playerCombat != null && playerCombat.IsParrying)
            {
                UpdatePlayerDefenseIndicator(playerCombat.CurrentDefenseDirection);
            }

            // Update enemy defense color when parrying
            if (enemyCombat != null && enemyCombat.IsParrying)
            {
                UpdateEnemyDefenseIndicator(enemyCombat.CurrentDefenseDirection);
            }
        }

        void OnPlayerCounterOpened()
        {
            isPlayerInCounter = true;
            pulseTimer = 0f;

            // Change attack indicator to counter color
            if (playerUpArrow != null) playerUpArrow.color = counterColor;
            if (playerDownArrow != null) playerDownArrow.color = counterColor;
            if (playerLeftArrow != null) playerLeftArrow.color = counterColor;
            if (playerRightArrow != null) playerRightArrow.color = counterColor;
        }

        void OnPlayerCounterClosed()
        {
            isPlayerInCounter = false;
            if (playerAttackIndicator != null)
                playerAttackIndicator.transform.localScale = Vector3.one;

            // Reset colors
            UpdatePlayerAttackIndicator(playerCombat.CurrentAttackDirection);
        }
    }
}