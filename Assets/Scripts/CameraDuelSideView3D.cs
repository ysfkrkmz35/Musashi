using UnityEngine;

public class CameraDuelSideView3D : MonoBehaviour
{
    public Transform player;
    public Transform enemy;

    [Header("Framing")]
    public float height = 2.0f;
    public float distance = 10f;     // sabit uzaklık (duello sabit olduğu için)
    public float smooth = 0.12f;

    [Tooltip("Kamera bakış yönü (genelde +Z). Kamerayı -Z tarafına yerleştirin).")]
    public Vector3 viewDir = Vector3.forward;
    public Vector3 lateralOffset = Vector3.zero;

    [Header("Cinematic")]
    public bool subtleDrift = true;
    public float driftAmplitude = 0.15f;
    public float driftSpeed = 0.3f;

    void LateUpdate()
    {
        if (!player || !enemy) return;

        Vector3 mid = (player.position + enemy.position) * 0.5f;
        Vector3 desired = mid + Vector3.up * height - viewDir.normalized * distance + lateralOffset;

        if (subtleDrift)
        {
            desired += new Vector3(
                Mathf.Sin(Time.time * driftSpeed) * driftAmplitude,
                Mathf.Cos(Time.time * driftSpeed * 0.9f) * driftAmplitude,
                0f
            );
        }

        transform.position = Vector3.Lerp(transform.position, desired, smooth);
        Vector3 lookTarget = new Vector3(mid.x, mid.y + 0.5f, mid.z);
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookTarget - transform.position, Vector3.up), smooth);
    }
}
