using UnityEngine;

namespace Musashi.Core.Combat
{
    /// <summary>
    /// Adapter to connect old Health (HealthB) system with new Directional controllers
    /// Eski Health sistemini yeni directional controller'a bağlar
    /// </summary>
    [RequireComponent(typeof(HealthB))]
    public class HealthDirectionalAdapter : MonoBehaviour
    {
        private HealthB health;
        private PlayerDuelControllerDirectional playerController;
        private EnemyDuelControllerDirectional enemyController;

        void Awake()
        {
            health = GetComponent<HealthB>();
            playerController = GetComponent<PlayerDuelControllerDirectional>();
            enemyController = GetComponent<EnemyDuelControllerDirectional>();

            // Subscribe to death event
            if (health != null)
            {
                // Hook into TakeDamage to check CanReceiveDamage first
                // This is already handled by HealthB checking the controller's CanReceiveDamage method
            }
        }

        void OnDestroy()
        {
            // Cleanup if needed
        }

        /// <summary>
        /// Called when this character dies
        /// </summary>
        public void OnDeath()
        {
            // Notify game manager
            var gameManager = FindObjectOfType<Musashi.Core.Managers.MusashiGameManager>();
            if (gameManager == null) return;

            if (playerController != null)
            {
                // Player died
                gameManager.OnDuelDefeat();
                Debug.Log("[HealthAdapter] Player died - notifying GameManager");
            }
            else if (enemyController != null)
            {
                // Enemy died
                gameManager.OnDuelVictory();
                Debug.Log("[HealthAdapter] Enemy died - notifying GameManager");
            }
        }
    }
}
