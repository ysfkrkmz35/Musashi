using UnityEngine;

/// <summary>
/// Automatically aligns character to ground level based on collider and ground check
/// Attach this to Player and Enemy for consistent Y positioning
/// </summary>
[ExecuteInEditMode]
public class GroundAligner : MonoBehaviour
{
    [Header("Ground Detection")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float checkRadius = 0.2f;
    
    [Header("Auto-Align Settings")]
    [SerializeField] private bool autoAlignInEditor = true;
    [SerializeField] private bool showDebug = true;
    [SerializeField] private float desiredHeightAboveGround = 0.35f; // Half of 0.7 collider height

    [Header("Manual Alignment")]
    [SerializeField] private bool alignNow = false;

    private void Update()
    {
        if (!Application.isPlaying && autoAlignInEditor)
        {
            if (alignNow)
            {
                AlignToGround();
                alignNow = false;
            }
        }
    }

    [ContextMenu("Align To Ground")]
    public void AlignToGround()
    {
        if (groundCheck == null)
        {
            Debug.LogWarning($"{name}: No GroundCheck assigned!", this);
            return;
        }

        // Raycast downward from ground check
        RaycastHit2D hit = Physics2D.Raycast(
            groundCheck.position, 
            Vector2.down, 
            10f, 
            groundLayer
        );

        if (hit.collider != null)
        {
            // Calculate desired Y position
            // Ground hit point + desired height above ground
            float groundY = hit.point.y;
            float newY = groundY + desiredHeightAboveGround;
            
            Vector3 newPos = transform.position;
            newPos.y = newY;
            transform.position = newPos;

            if (showDebug)
            {
                Debug.Log($"{name} aligned to ground at Y = {newY:F3} (Ground = {groundY:F3})", this);
            }
        }
        else
        {
            Debug.LogWarning($"{name}: No ground detected below GroundCheck!", this);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;

        // Draw ground check
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(groundCheck.position, checkRadius);

        // Draw raycast line
        Gizmos.color = Color.red;
        Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * 2f);
    }

    private void OnValidate()
    {
        // Clamp height to reasonable values
        desiredHeightAboveGround = Mathf.Max(0.1f, desiredHeightAboveGround);
    }
}
