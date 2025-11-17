using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Manages game state and scene restarting
/// Attach to Player to restart scene on death
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Player Reference")]
    [SerializeField] private Health playerHealth;
    
    [Header("Restart Settings")]
    [SerializeField] private float restartDelay = 2.5f;
    [SerializeField] private bool fadeToBlack = true;
    [SerializeField] private CanvasGroup fadePanel;

    [Header("UI (Optional)")]
    [SerializeField] private GameObject gameOverText;

    private bool isRestarting = false;

    private void Awake()
    {
        if (playerHealth == null)
            playerHealth = GetComponent<Health>();

        if (playerHealth != null)
            playerHealth.OnDied += OnPlayerDied;

        if (gameOverText != null)
            gameOverText.SetActive(false);
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.OnDied -= OnPlayerDied;
    }

    private void OnPlayerDied()
    {
        if (isRestarting) return;
        isRestarting = true;

        Debug.Log("Player died! Restarting scene...");

        if (gameOverText != null)
            gameOverText.SetActive(true);

        StartCoroutine(RestartScene());
    }

    private IEnumerator RestartScene()
    {
        // Optional fade
        if (fadeToBlack && fadePanel != null)
        {
            float elapsed = 0f;
            float fadeDuration = restartDelay * 0.5f;
            
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                fadePanel.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
                yield return null;
            }
        }

        // Wait
        yield return new WaitForSeconds(fadeToBlack ? restartDelay * 0.5f : restartDelay);

        // Reload current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Public method to restart scene manually (for UI button)
    public void RestartGame()
    {
        if (!isRestarting)
            StartCoroutine(RestartScene());
    }
}
