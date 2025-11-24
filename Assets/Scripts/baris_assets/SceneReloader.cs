using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneReloader : MonoBehaviour
{
    [SerializeField] private float delayBeforeReload = 2f;
    private Health health;
    private bool isReloading = false;

    private void Awake()
    {
        health = GetComponent<Health>();
        if (health != null)
        {
            health.OnDied += OnPlayerDied;
        }
    }

    private void OnDestroy()
    {
        if (health != null)
        {
            health.OnDied -= OnPlayerDied;
        }
    }

    private void OnPlayerDied()
    {
        if (!isReloading)
        {
            isReloading = true;
            Invoke(nameof(ReloadScene), delayBeforeReload);
        }
    }

    private void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
