using UnityEngine;
using System.Collections;
using Musashi.Core.Combat;

namespace Musashi.Core.Effects
{
    /// <summary>
    /// Handles combat visual effects (sparks, trails, camera shake, slow-mo)
    /// Dövüş görsel efektleri yöneticisi
    /// </summary>
    public class CombatEffectsManager : MonoBehaviour
    {
        [Header("Particle Effects")]
        [SerializeField] private GameObject swordTrailPrefab;
        [SerializeField] private GameObject parrySparksPrefab;
        [SerializeField] private GameObject hitSparksPrefab;
        [SerializeField] private GameObject blockSparksPrefab;

        [Header("Camera Effects")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private float normalShakeIntensity = 0.1f;
        [SerializeField] private float parryShakeIntensity = 0.3f;
        [SerializeField] private float shakeDuration = 0.2f;

        [Header("Slow Motion")]
        [SerializeField] private bool enableSlowMotion = true;
        [SerializeField] private float parrySlowMotionScale = 0.3f;
        [SerializeField] private float parrySlowMotionDuration = 0.3f;

        [Header("Screen Flash")]
        [SerializeField] private UnityEngine.UI.Image flashImage;
        [SerializeField] private Color parryFlashColor = new Color(1f, 0.8f, 0.2f, 0.3f);
        [SerializeField] private Color hitFlashColor = new Color(1f, 0.2f, 0.2f, 0.3f);
        [SerializeField] private float flashDuration = 0.15f;

        private Vector3 originalCameraPosition;
        private Coroutine shakeCoroutine;
        private Coroutine slowMotionCoroutine;
        private Coroutine flashCoroutine;

        void Start()
        {
            if (mainCamera == null)
                mainCamera = Camera.main;

            if (mainCamera != null)
                originalCameraPosition = mainCamera.transform.localPosition;
        }

        /// <summary>
        /// Spawn sword trail effect
        /// </summary>
        public void SpawnSwordTrail(Transform weaponTransform)
        {
            if (swordTrailPrefab == null || weaponTransform == null) return;

            GameObject trail = Instantiate(swordTrailPrefab, weaponTransform.position, weaponTransform.rotation);
            trail.transform.SetParent(weaponTransform);
            Destroy(trail, 1f);
        }

        /// <summary>
        /// Spawn parry sparks at contact point
        /// </summary>
        public void SpawnParrySparks(Vector3 position)
        {
            if (parrySparksPrefab == null) return;

            GameObject sparks = Instantiate(parrySparksPrefab, position, Quaternion.identity);
            Destroy(sparks, 2f);
        }

        /// <summary>
        /// Spawn hit sparks
        /// </summary>
        public void SpawnHitSparks(Vector3 position)
        {
            if (hitSparksPrefab == null) return;

            GameObject sparks = Instantiate(hitSparksPrefab, position, Quaternion.identity);
            Destroy(sparks, 1f);
        }

        /// <summary>
        /// Spawn block sparks
        /// </summary>
        public void SpawnBlockSparks(Vector3 position)
        {
            if (blockSparksPrefab == null) return;

            GameObject sparks = Instantiate(blockSparksPrefab, position, Quaternion.identity);
            Destroy(sparks, 1f);
        }

        /// <summary>
        /// Trigger camera shake
        /// </summary>
        public void TriggerCameraShake(float intensity = -1f)
        {
            if (mainCamera == null) return;

            if (intensity < 0f)
                intensity = normalShakeIntensity;

            if (shakeCoroutine != null)
                StopCoroutine(shakeCoroutine);

            shakeCoroutine = StartCoroutine(CameraShake(intensity, shakeDuration));
        }

        /// <summary>
        /// Trigger parry camera shake (stronger)
        /// </summary>
        public void TriggerParryShake()
        {
            TriggerCameraShake(parryShakeIntensity);
        }

        /// <summary>
        /// Camera shake coroutine
        /// </summary>
        IEnumerator CameraShake(float intensity, float duration)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float x = Random.Range(-1f, 1f) * intensity;
                float y = Random.Range(-1f, 1f) * intensity;

                mainCamera.transform.localPosition = originalCameraPosition + new Vector3(x, y, 0);

                elapsed += Time.deltaTime;
                yield return null;
            }

            mainCamera.transform.localPosition = originalCameraPosition;
        }

        /// <summary>
        /// Trigger slow motion on parry
        /// </summary>
        public void TriggerParrySlowMotion()
        {
            if (!enableSlowMotion) return;

            if (slowMotionCoroutine != null)
                StopCoroutine(slowMotionCoroutine);

            slowMotionCoroutine = StartCoroutine(SlowMotionEffect(parrySlowMotionScale, parrySlowMotionDuration));
        }

        /// <summary>
        /// Slow motion effect coroutine
        /// </summary>
        IEnumerator SlowMotionEffect(float timeScale, float duration)
        {
            Time.timeScale = timeScale;
            Time.fixedDeltaTime = 0.02f * timeScale;

            yield return new WaitForSecondsRealtime(duration);

            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;
        }

        /// <summary>
        /// Trigger screen flash
        /// </summary>
        public void TriggerFlash(Color color)
        {
            if (flashImage == null) return;

            if (flashCoroutine != null)
                StopCoroutine(flashCoroutine);

            flashCoroutine = StartCoroutine(ScreenFlash(color));
        }

        /// <summary>
        /// Screen flash coroutine
        /// </summary>
        IEnumerator ScreenFlash(Color color)
        {
            flashImage.color = color;
            flashImage.enabled = true;

            float elapsed = 0f;
            while (elapsed < flashDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(color.a, 0f, elapsed / flashDuration);
                flashImage.color = new Color(color.r, color.g, color.b, alpha);
                yield return null;
            }

            flashImage.enabled = false;
        }

        /// <summary>
        /// Subscribe to combat events
        /// </summary>
        public void SubscribeToCombatEvents(DirectionalCombatSystem combat, Transform contactPoint)
        {
            if (combat == null) return;

            combat.OnCombatResult += (result) => HandleCombatResult(result, contactPoint);
        }

        private void HandleCombatResult(CombatResult result, Transform contactPoint)
        {
            Vector3 position = contactPoint != null ? contactPoint.position : Vector3.zero;

            switch (result)
            {
                case CombatResult.Blocked:
                    SpawnBlockSparks(position);
                    TriggerCameraShake();
                    break;

                case CombatResult.ParrySuccess:
                    SpawnParrySparks(position);
                    TriggerParryShake();
                    TriggerParrySlowMotion();
                    TriggerFlash(parryFlashColor);
                    break;

                case CombatResult.ParryFailed:
                    SpawnHitSparks(position);
                    TriggerCameraShake(parryShakeIntensity * 0.5f);
                    TriggerFlash(hitFlashColor);
                    break;

                case CombatResult.Hit:
                    SpawnHitSparks(position);
                    TriggerCameraShake();
                    TriggerFlash(hitFlashColor);
                    break;

                case CombatResult.Dodged:
                    // No effects for dodge
                    break;
            }
        }
    }
}
