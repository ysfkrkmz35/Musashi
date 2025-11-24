using UnityEngine;
using TMPro;

public class DamageNumber : MonoBehaviour
{
    [SerializeField] private float lifetime = 1f;
    [SerializeField] private float floatSpeed = 2f;
    
    private TextMeshProUGUI textMesh;
    private Color originalColor;
    private float timer;

    public void Initialize(int damage, Vector3 position)
    {
        transform.position = position;
        textMesh = GetComponentInChildren<TextMeshProUGUI>();
        if (textMesh != null)
        {
            textMesh.text = damage.ToString();
            originalColor = textMesh.color;
        }
        timer = lifetime;
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;
        
        if (textMesh != null)
        {
            float alpha = Mathf.Clamp01(timer / lifetime);
            textMesh.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
        }

        if (timer <= 0f)
        {
            Destroy(gameObject);
        }
    }
}
