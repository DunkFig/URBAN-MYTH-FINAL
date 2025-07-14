using UnityEngine;

[RequireComponent(typeof(Transform))]
public class FacePlayer3D : MonoBehaviour
{
    [Tooltip("Assign the player's Transform here (e.g. the Camera or Player object)")]
    public Transform playerTransform;

    [Tooltip("How fast the character rotates to face the player (degrees per second)")]
    public float rotationSpeed = 180f;

    [Tooltip("Euler angle offset (in degrees) to apply after LookRotation so your model's 'front' can be corrected")]
    public Vector3 frontOffsetEuler = Vector3.zero;

    void Update()
    {
        if (playerTransform == null)
            return;

        // Direction vector from this object to the player
        Vector3 direction = playerTransform.position - transform.position;

        // If too close / zero length, bail out
        if (direction.sqrMagnitude < 0.0001f)
            return;

        // Base look rotation toward player
        Quaternion lookRot = Quaternion.LookRotation(direction.normalized);

        // Apply your custom front-facing offset
        Quaternion targetRot = lookRot * Quaternion.Euler(frontOffsetEuler);

        // Smoothly rotate toward that adjusted orientation
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRot,
            rotationSpeed * Time.deltaTime
        );
    }

    // Optional: visualize your trigger/front offset in the editor
    void OnDrawGizmosSelected()
    {
        // Show the 'forward' direction your mesh will end up facing
        Quaternion lookRot = Quaternion.identity;
        if (playerTransform != null)
        {
            Vector3 dir = (playerTransform.position - transform.position).normalized;
            lookRot = Quaternion.LookRotation(dir) * Quaternion.Euler(frontOffsetEuler);
        }
        else
        {
            // if no player assigned, just show local forward + offset
            lookRot = transform.rotation * Quaternion.Euler(frontOffsetEuler);
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + lookRot * Vector3.forward);
    }
}
