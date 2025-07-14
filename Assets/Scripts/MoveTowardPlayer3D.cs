using UnityEngine;

[RequireComponent(typeof(Transform))]
public class MoveTowardPlayer3D : MonoBehaviour
{
    [Tooltip("Assign the player's Transform here (e.g. the Camera or Player object)")]
    public Transform playerTransform;

    [Tooltip("Speed at which the character moves toward the player (units per second)")]
    public float moveSpeed = 2f;

    [Tooltip("Lowest world-space Y position this mesh is allowed to reach")]
    public float lowestY = 0f;

    void Update()
    {
        if (playerTransform == null) 
            return;

        // Compute the next position moving toward the player
        Vector3 nextPos = Vector3.MoveTowards(
            transform.position,
            playerTransform.position,
            moveSpeed * Time.deltaTime
        );

        // Clamp Y so we don't go below the floor threshold
        nextPos.y = Mathf.Max(nextPos.y, lowestY);

        transform.position = nextPos;
    }

    // Optional: visualize the floor threshold plane in the editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        // draw a simple horizontal line at lowestY
        Gizmos.DrawLine(
            new Vector3(transform.position.x - 5f, lowestY, transform.position.z - 5f),
            new Vector3(transform.position.x + 5f, lowestY, transform.position.z + 5f)
        );
    }
}
