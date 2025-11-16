using UnityEngine;

/// <summary>
/// Visual debug helper for combat system - attach to Player or Enemy
/// Shows hitboxes, health, and layer info in the scene view
/// </summary>
public class CombatDebugger : MonoBehaviour
{
    [Header("Debug Visualization")]
    [SerializeField] private bool showHealthInfo = true;
    [SerializeField] private bool showLayerInfo = true;
    [SerializeField] private bool showHitboxes = true;
    [SerializeField] private Color healthBarColor = Color.green;
    [SerializeField] private Color hitboxColor = new Color(1f, 0f, 0f, 0.3f);

    private Health health;
    private Collider2D[] hitboxes;

    private void Awake()
    {
        health = GetComponent<Health>();
        hitboxes = GetComponentsInChildren<Collider2D>();
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        // Draw hitboxes
        if (showHitboxes)
        {
            Gizmos.color = hitboxColor;
            foreach (var hitbox in hitboxes)
            {
                if (hitbox.isTrigger && hitbox.enabled)
                {
                    if (hitbox is BoxCollider2D box)
                    {
                        Gizmos.matrix = Matrix4x4.TRS(
                            hitbox.transform.position,
                            hitbox.transform.rotation,
                            hitbox.transform.lossyScale
                        );
                        Gizmos.DrawCube(box.offset, box.size);
                    }
                    else if (hitbox is CapsuleCollider2D capsule)
                    {
                        // Simple sphere approximation for capsule
                        Gizmos.DrawWireSphere(hitbox.transform.position, capsule.size.x / 2f);
                    }
                }
            }
        }
    }

    private void OnGUI()
    {
        if (!showHealthInfo && !showLayerInfo) return;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 0.5f);
        if (screenPos.z < 0) return; // Behind camera

        float yOffset = 0;

        // Health bar
        if (showHealthInfo && health != null)
        {
            float barWidth = 100f;
            float barHeight = 20f;
            float healthPercent = (float)health.CurrentHP / Mathf.Max(1, 5); // Assuming max 5 for bar size

            Rect bgRect = new Rect(screenPos.x - barWidth / 2, Screen.height - screenPos.y + yOffset, barWidth, barHeight);
            Rect hpRect = new Rect(screenPos.x - barWidth / 2, Screen.height - screenPos.y + yOffset, barWidth * Mathf.Clamp01(healthPercent), barHeight);

            GUI.color = Color.black;
            GUI.DrawTexture(bgRect, Texture2D.whiteTexture);
            GUI.color = Color.Lerp(Color.red, Color.green, healthPercent);
            GUI.DrawTexture(hpRect, Texture2D.whiteTexture);
            GUI.color = Color.white;

            // HP text
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.alignment = TextAnchor.MiddleCenter;
            style.fontStyle = FontStyle.Bold;
            GUI.Label(bgRect, $"{health.CurrentHP} HP", style);

            yOffset += barHeight + 5;
        }

        // Layer info
        if (showLayerInfo)
        {
            GUIStyle labelStyle = new GUIStyle(GUI.skin.box);
            labelStyle.fontSize = 10;
            labelStyle.alignment = TextAnchor.MiddleCenter;
            
            string layerName = LayerMask.LayerToName(gameObject.layer);
            string info = $"{gameObject.name}\nLayer: {layerName} ({gameObject.layer})";
            
            Vector2 size = labelStyle.CalcSize(new GUIContent(info));
            Rect labelRect = new Rect(screenPos.x - size.x / 2, Screen.height - screenPos.y + yOffset, size.x, size.y);
            
            GUI.color = new Color(0, 0, 0, 0.7f);
            GUI.Box(labelRect, info, labelStyle);
            GUI.color = Color.white;
        }
    }
}
