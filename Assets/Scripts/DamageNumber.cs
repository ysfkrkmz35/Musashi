using UnityEngine;
using UnityEngine.UI;

public class DamageNumbers : MonoBehaviour
{
    [SerializeField] private Text damageText;
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float lifetime = 1f;
    [SerializeField] private float fadeSpeed = 2f;

    private CanvasGroup canvasGroup;
    private float timer;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void Setup(int damage, Color color)
    {
        if (damageText != null)
        {
            damageText.text = damage.ToString();
            damageText.color = color;
        }
    }

    private void Update()
    {
        timer += Time.deltaTime;

        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f - (timer / lifetime) * fadeSpeed;
        }

        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}
