using UnityEngine;
using UnityEngine.UI;
using Musashi.Core.Combat;

namespace Musashi.Core.UI
{
    /// <summary>
    /// Enhanced DirectionalIndicatorUI V2 with telegraph visual feedback
    /// Telegraph sırasında enemy okları parlar (uyarı)
    /// </summary>
    public class DirectionalIndicatorUI_V2 : MonoBehaviour
    {
        [Header("Player Indicators")]
        [SerializeField] private Image playerUpArrow;
        [SerializeField] private Image playerDownArrow;
        [SerializeField] private Image playerLeftArrow;
        [SerializeField] private Image playerRightArrow;

        [Header("Enemy Indicators")]
        [SerializeField] private Image enemyUpArrow;
        [SerializeField] private Image enemyDownArrow;
        [SerializeField] private Image enemyLeftArrow;
        [SerializeField] private Image enemyRightArrow;

        [Header("Colors")]
        [SerializeField] private Color attackColor = new Color(1f, 0.2f, 0.2f, 0.8f);
        [SerializeField] private Color defenseColor = new Color(0.2f, 0.5f, 1f, 0.8f);
        [SerializeField] private Color inactiveColor = new Color(0.3f, 0.3f, 0.3f, 0.3f);
        [SerializeField] private Color parryColor = new Color(1f, 0.8f, 0.2f, 1f);
        [SerializeField] private Color counterColor = new Color(0f, 1f, 0.5f, 1f);
        [SerializeField] private Color telegraphColor = new Color(1f, 0.5f, 0f, 1f); // Turuncu uyarı

        [Header("Telegraph Animation")]
        [SerializeField] private float telegraphPulseSpeed = 8f;

        private DirectionalCombatSystem playerCombat;
        private DirectionalCombatSystem enemyCombat;
        private AttackTelegraphSystem enemyTelegraph;

        private bool isPlayerInCounter = false;
        private float pulseTimer = 0f;

        void Start()
        {
            var player = FindObjectOfType<PlayerDuelControllerDirectional>();
            var enemy = FindObjectOfType<EnemyDuelControllerDirectional_V2>();

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
                enemyTelegraph = enemy.GetComponent<AttackTelegraphSystem>();

                if (enemyCombat != null)
                {
                    enemyCombat.OnAttackDirectionChanged += UpdateEnemyAttackIndicator;
                    enemyCombat.OnDefenseDirectionChanged += UpdateEnemyDefenseIndicator;
                }
            }

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
            if (isPlayerInCounter)
            {
                pulseTimer += Time.deltaTime * telegraphPulseSpeed;
                float pulse = 1f + Mathf.Sin(pulseTimer) * 0.3f;
                if (playerUpArrow != null)
                    playerUpArrow.transform.parent.localScale = Vector3.one * pulse;
            }

            UpdateParryVisuals();
            UpdateTelegraphVisuals();
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

            if (playerUpArrow != null && dir != AttackDirection.Up)
                playerUpArrow.color = (dir == AttackDirection.Up) ? defColor : inactiveColor;
            if (playerDownArrow != null && dir != AttackDirection.Down)
                playerDownArrow.color = (dir == AttackDirection.Down) ? defColor : inactiveColor;
            if (playerLeftArrow != null && dir != AttackDirection.Left)
                playerLeftArrow.color = (dir == AttackDirection.Left) ? defColor : inactiveColor;
            if (playerRightArrow != null && dir != AttackDirection.Right)
                playerRightArrow.color = (dir == AttackDirection.Right) ? defColor : inactiveColor;
        }

        void UpdateEnemyAttackIndicator(AttackDirection dir)
        {
            // Check if telegraphing
            Color attackCol = attackColor;
            if (enemyTelegraph != null && enemyTelegraph.IsTelegraphing)
            {
                // Pulse between telegraph color and white
                float pulse = Mathf.PingPong(Time.time * telegraphPulseSpeed, 1f);
                attackCol = Color.Lerp(telegraphColor, Color.white, pulse);
            }

            if (enemyUpArrow != null) enemyUpArrow.color = (dir == AttackDirection.Up) ? attackCol : inactiveColor;
            if (enemyDownArrow != null) enemyDownArrow.color = (dir == AttackDirection.Down) ? attackCol : inactiveColor;
            if (enemyLeftArrow != null) enemyLeftArrow.color = (dir == AttackDirection.Left) ? attackCol : inactiveColor;
            if (enemyRightArrow != null) enemyRightArrow.color = (dir == AttackDirection.Right) ? attackCol : inactiveColor;
        }

        void UpdateEnemyDefenseIndicator(AttackDirection dir)
        {
            Color defColor = (enemyCombat != null && enemyCombat.IsParrying) ? parryColor : defenseColor;

            if (enemyUpArrow != null) enemyUpArrow.color = (dir == AttackDirection.Up) ? defColor : inactiveColor;
            if (enemyDownArrow != null) enemyDownArrow.color = (dir == AttackDirection.Down) ? defColor : inactiveColor;
            if (enemyLeftArrow != null) enemyLeftArrow.color = (dir == AttackDirection.Left) ? defColor : inactiveColor;
            if (enemyRightArrow != null) enemyRightArrow.color = (dir == AttackDirection.Right) ? defColor : inactiveColor;
        }

        void UpdateParryVisuals()
        {
            if (playerCombat != null && playerCombat.IsParrying)
            {
                UpdatePlayerDefenseIndicator(playerCombat.CurrentDefenseDirection);
            }

            if (enemyCombat != null && enemyCombat.IsParrying)
            {
                UpdateEnemyDefenseIndicator(enemyCombat.CurrentDefenseDirection);
            }
        }

        void UpdateTelegraphVisuals()
        {
            // Continuously update enemy attack indicator if telegraphing
            if (enemyTelegraph != null && enemyTelegraph.IsTelegraphing && enemyCombat != null)
            {
                UpdateEnemyAttackIndicator(enemyCombat.CurrentAttackDirection);
            }
        }

        void OnPlayerCounterOpened()
        {
            isPlayerInCounter = true;
            pulseTimer = 0f;

            if (playerUpArrow != null) playerUpArrow.color = counterColor;
            if (playerDownArrow != null) playerDownArrow.color = counterColor;
            if (playerLeftArrow != null) playerLeftArrow.color = counterColor;
            if (playerRightArrow != null) playerRightArrow.color = counterColor;
        }

        void OnPlayerCounterClosed()
        {
            isPlayerInCounter = false;
            if (playerUpArrow != null && playerUpArrow.transform.parent != null)
                playerUpArrow.transform.parent.localScale = Vector3.one;

            UpdatePlayerAttackIndicator(playerCombat.CurrentAttackDirection);
        }
    }
}
