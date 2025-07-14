using UnityEngine;

public enum CollectibleType { Coin, Gem, Key }

[RequireComponent(typeof(Collider))]
public class Collectible : MonoBehaviour
{
    [Tooltip("Select what kind of collectible this is.")]
    public CollectibleType type = CollectibleType.Coin;

    [Tooltip("How many points this item is worth.")]
    public int value = 1;

    [Tooltip("Sound to play on collection.")]
    public AudioClip collectSound;

    void Reset()
    {
        // Ensure the collider is a trigger
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Play pickup SFX at the main camera
        if (collectSound != null)
            AudioSource.PlayClipAtPoint(collectSound, Camera.main.transform.position);

        // Notify manager
        CollectibleManager.Instance.Add(type, value);

        // Remove from scene
        Destroy(gameObject);
    }
}
