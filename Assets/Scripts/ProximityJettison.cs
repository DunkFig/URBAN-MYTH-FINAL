using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ProximityJettison : MonoBehaviour
{
    [Tooltip("Drag your Player’s Transform here.")]
    public Transform playerTransform;

    [Tooltip("If your player uses a CharacterController, drag it here. Otherwise leave blank.")]
    public CharacterController playerController;

    [Tooltip("How close the player must be before jettison triggers (in world units).")]
    public float triggerDistance = 5f;

    [Tooltip("How far (units) to push the player when triggered.")]
    public float jettisonDistance = 5f;

    [Tooltip("Sound to play when the player is jettisoned.")]
    public AudioClip repelSound;

    private AudioSource _audioSource;
    private bool _hasTriggered;

    void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (playerTransform == null || repelSound == null)
            return;

        float sqrDist = (playerTransform.position - transform.position).sqrMagnitude;
        if (sqrDist <= triggerDistance * triggerDistance)
        {
            if (!_hasTriggered)
            {
                // Play the sound
                _audioSource.PlayOneShot(repelSound);

                // Calculate-away direction
                Vector3 awayDir = (playerTransform.position - transform.position).normalized;

                if (playerController != null)
                {
                    // If you have a CharacterController, move via Move()
                    // Multiply by jettisonDistance so Move() applies that offset instantly.
                    playerController.Move(awayDir * jettisonDistance);
                }
                else
                {
                    // Otherwise just shift the Transform
                    playerTransform.position += awayDir * jettisonDistance;
                }

                _hasTriggered = true;
            }
        }
        else
        {
            // reset so it can trigger again on re-enter
            _hasTriggered = false;
        }
    }

    // Visualize trigger radius
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, triggerDistance);
    }
}
