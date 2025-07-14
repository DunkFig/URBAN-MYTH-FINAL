using UnityEngine;

public class FacePlayer : MonoBehaviour
{
    [Tooltip("Assign the player's Transform here (e.g. the Camera or Player object)")]
    public Transform playerTransform;

    [Tooltip("Speed at which the character rotates to face the player")]
    public float rotationSpeed = 5f;

    void Update()
    {
        if (playerTransform == null) return;

        // Determine direction from NPC to player
        Vector3 direction = playerTransform.position - transform.position;

        // Optional: keep the NPC upright by zeroing out any vertical difference
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            // Calculate the rotation needed to look at the player
            Quaternion targetRot = Quaternion.LookRotation(direction);

            // Smoothly rotate towards the player
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                Time.deltaTime * rotationSpeed
            );
        }
    }
}
