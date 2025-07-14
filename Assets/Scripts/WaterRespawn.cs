using UnityEngine;

public class WaterRespawn : MonoBehaviour
{
    [Header("Player to check")]
    public Transform playerTransform;

    [Header("Walkable Land Parent")]
    public Transform walkableMeshesParent;

    [Header("Water Level")]
    public float waterHeight = 0f;

    [Header("Safe Respawn Offset")]
    public float heightAboveGround = 2f;

    [Header("Respawn Sound")]
    public AudioClip respawnClip;

    private AudioSource audioSource;

    void Start()
    {
        // Create or get the AudioSource
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        if (HasPlayerFallenIntoWater())
        {
            Transform nearestLand = FindNearestWalkableMesh();
            if (nearestLand != null)
            {
                Vector3 safePosition = nearestLand.position + Vector3.up * heightAboveGround;
                playerTransform.position = safePosition;

                Debug.Log("Player respawned to: " + safePosition);

                PlayRespawnSound();
            }
        }
    }

    bool HasPlayerFallenIntoWater()
    {
        return playerTransform.position.y < waterHeight;
    }

    Transform FindNearestWalkableMesh()
    {
        Transform closest = null;
        float closestDistanceSqr = Mathf.Infinity;

        foreach (Transform child in walkableMeshesParent)
        {
            float distanceSqr = (child.position - playerTransform.position).sqrMagnitude;
            if (distanceSqr < closestDistanceSqr)
            {
                closest = child;
                closestDistanceSqr = distanceSqr;
            }
        }

        return closest;
    }

    void PlayRespawnSound()
    {
        if (respawnClip != null && audioSource != null)
        {
            audioSource.PlayOneShot(respawnClip);
        }
    }
}
