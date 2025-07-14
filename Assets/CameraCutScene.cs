using UnityEngine;
using System.Collections;

public class CameraCutscene : MonoBehaviour
{
    [Header("Camera Setup")]
    public Camera mainCamera;
    public Camera cutsceneCamera;

    [Header("Player Control")]
    public MonoBehaviour playerController;

    [Header("Cutscene Settings")]
    public float cutsceneDuration = 20f;

    [Header("Optional Mesh Mover")]
    public MeshMover meshMover;

    [Header("Audio")]
    public AudioClip cutsceneAudioClip;   // The new audio to play during the cutscene
    private AudioSource audioSource;

    private AudioListener mainListener;
    private AudioListener cutsceneListener;

    void Awake()
    {
        // Cache AudioListeners
        if (mainCamera != null) mainListener = mainCamera.GetComponent<AudioListener>();
        if (cutsceneCamera != null) cutsceneListener = cutsceneCamera.GetComponent<AudioListener>();

        // Make sure only main camera listener is enabled initially
        if (mainListener != null) mainListener.enabled = true;
        if (cutsceneListener != null) cutsceneListener.enabled = false;

        // Add an AudioSource for the cutscene clip
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void PlayCutscene()
    {
        StartCoroutine(CutsceneCoroutine());

        if (meshMover != null)
            meshMover.StartMoving();
    }

    public void TriggerReward()
    {
        PlayCutscene();
    }

    IEnumerator CutsceneCoroutine()
    {
        Debug.Log("Cutscene started!");

        // Switch cameras
        mainCamera.enabled = false;
        cutsceneCamera.enabled = true;

        // Switch AudioListeners
        if (mainListener != null) mainListener.enabled = false;
        if (cutsceneListener != null) cutsceneListener.enabled = true;

        // Play cutscene audio
        if (cutsceneAudioClip != null)
        {
            audioSource.clip = cutsceneAudioClip;
            audioSource.Play();
        }

        // Disable player control
        if (playerController != null)
            playerController.enabled = false;

        yield return new WaitForSeconds(cutsceneDuration);

        // Switch back
        cutsceneCamera.enabled = false;
        mainCamera.enabled = true;

        if (mainListener != null) mainListener.enabled = true;
        if (cutsceneListener != null) cutsceneListener.enabled = false;

        if (playerController != null)
            playerController.enabled = true;

        Debug.Log("Cutscene complete!");
    }
}
