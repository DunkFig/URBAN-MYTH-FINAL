using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(AudioSource))]
public class WaterRespawn : MonoBehaviour
{
    [Header("Player to check")]
    [SerializeField] Transform playerTransform;

    [Header("Walkable Land Parent")]
    [SerializeField] Transform walkableMeshesParent;

    [Header("Water Level")]
    [SerializeField] float waterHeight = 0f;

    [Header("Safe Respawn Offset")]
    [SerializeField] float heightAboveGround = 2f;

    [Header("Respawn Sound")]
    [SerializeField] AudioClip respawnClip;

    AudioSource      audioSource;
    PlayerControls   controls;

    /*──────────────────────────────────────────*/
    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        controls    = new PlayerControls();         // generated input‑actions asset
    }

    void OnEnable()  => controls.Enable();          // <<< start pumping input
    void OnDisable() => controls.Disable();

    void Update()
    {
        if (HasPlayerFallenIntoWater() || PlayerPressedRespawn())
            Respawn();
    }

    /*──────── helpers ────────*/
    bool HasPlayerFallenIntoWater() =>
        playerTransform.position.y < waterHeight;

    bool PlayerPressedRespawn()
    {
        // Triangle / Y / X (Switch) – make sure “Interact3” is a **Button**
        if (controls.Player.Interact3.WasPerformedThisFrame())
            return true;

        // Keyboard fallback (R)
        return Keyboard.current?.rKey.wasPressedThisFrame ?? false;
    }

    void Respawn()
    {
        var nearest = FindNearestWalkableMesh();
        if (nearest == null) return;

        Vector3 safePos = nearest.position + Vector3.up * heightAboveGround;
        playerTransform.position = safePos;

        if (respawnClip) audioSource.PlayOneShot(respawnClip);
    }

    Transform FindNearestWalkableMesh()
    {
        Transform closest = null;
        float minDistSq   = float.MaxValue;

        foreach (Transform child in walkableMeshesParent)
        {
            float d = (child.position - playerTransform.position).sqrMagnitude;
            if (d < minDistSq) { closest = child; minDistSq = d; }
        }
        return closest;
    }
}
