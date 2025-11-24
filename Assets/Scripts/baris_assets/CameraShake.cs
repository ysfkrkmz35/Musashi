using UnityEngine;

public class CameraShake : MonoBehaviour
{
    private Vector3 originalPosition;
    private float shakeTime = 0f;
    private float shakeDuration = 0f;
    private float shakeMagnitude = 0f;

    private void Awake()
    {
        originalPosition = transform.localPosition;
    }

    private void Update()
    {
        if (shakeTime > 0f)
        {
            transform.localPosition = originalPosition + Random.insideUnitSphere * shakeMagnitude;
            shakeTime -= Time.deltaTime;
        }
        else if (shakeDuration > 0f)
        {
            shakeDuration = 0f;
            transform.localPosition = originalPosition;
        }
    }

    public void Shake(float duration = 0.15f, float magnitude = 0.1f)
    {
        shakeTime = duration;
        shakeDuration = duration;
        shakeMagnitude = magnitude;
        originalPosition = transform.localPosition;
    }
}
