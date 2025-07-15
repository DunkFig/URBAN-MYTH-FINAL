using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(AudioSource))]
public class NPCDialogue : MonoBehaviour
{
    [Header("Dialogue Data")]
    [TextArea] public string[] dialogueLines;
    public float wordDelay = 0.2f;
    public float audioSnippetDuration = 0.2f;
    public AudioClip speechAudioClip;
    public AudioClip skipAudioClip;
    public float interactionDistance = 3f;

    [Header("References")]
    public Transform playerTransform;      // Your Player or Camera
    public PlayerMovement playerMovement;  // Drag your PlayerMovement script here
    public Camera mainCamera;              // Player camera
    public Camera dialogueCamera;          // NPC face camera
    public GameObject dialoguePanel;       // World-space Canvas root
    public TMP_Text dialogueText;          // TMPUGUI component

    [Header("Scene Transition")]
    public bool enableSceneTransition = false;

#if UNITY_EDITOR
    [Tooltip("Drag the Scene asset you want to load (must be added to Build Settings)")]
    public SceneAsset nextSceneAsset;
#endif

    private string nextSceneName;

    // Audio
    private AudioSource speechSource, sfxSource;

    // State
    private int currentLineIndex = 0;
    private bool isDialogActive = false;
    private bool isTyping = false;
    private Coroutine typingCoroutine;

    // Input System
    private PlayerControls controls;

    void Awake()
    {
        // Setup audio sources
        speechSource = gameObject.AddComponent<AudioSource>();
        sfxSource = gameObject.AddComponent<AudioSource>();
        speechSource.clip = speechAudioClip;

#if UNITY_EDITOR
        if (nextSceneAsset != null)
            nextSceneName = nextSceneAsset.name;
#endif

        // Hide dialogue UI and camera
        dialoguePanel.SetActive(false);
        dialogueCamera.enabled = false;

        // Setup Input System
        controls = new PlayerControls();
    }

    void OnEnable()
    {
        controls.Enable();
    }

    void OnDisable()
    {
        controls.Disable();
    }

    void Update()
    {
        // If player is too far, do nothing
        if (Vector3.Distance(playerTransform.position, transform.position) > interactionDistance)
            return;

        // Check the Interact action (Square button) in the Player map
        if (!isDialogActive && controls.Player.Interact.triggered)
        {
            StartDialogue();
        }
        else if (isDialogActive && controls.Player.Interact.triggered)
        {
            sfxSource.PlayOneShot(skipAudioClip);

            if (isTyping)
            {
                StopCoroutine(typingCoroutine);
                dialogueText.text = dialogueLines[currentLineIndex];
                isTyping = false;
            }
            else
            {
                currentLineIndex++;
                if (currentLineIndex < dialogueLines.Length)
                {
                    typingCoroutine = StartCoroutine(TypeLine(dialogueLines[currentLineIndex]));
                }
                else
                {
                    EndDialogue();
                }
            }
        }
    }

    void StartDialogue()
    {
        isDialogActive = true;
        currentLineIndex = 0;

        // Freeze player movement
        if (playerMovement != null)
            playerMovement.enabled = false;

        // Show dialogue UI
        dialoguePanel.SetActive(true);

        // Swap cameras
        mainCamera.enabled = false;
        dialogueCamera.enabled = true;

        typingCoroutine = StartCoroutine(TypeLine(dialogueLines[0]));
    }

    IEnumerator TypeLine(string line)
    {
        isTyping = true;
        dialogueText.text = "";

        string[] words = line.Split(' ');
        foreach (string word in words)
        {
            // Play random snippet of speech clip
            if (speechAudioClip != null)
            {
                float maxStart = Mathf.Max(0f, speechAudioClip.length - audioSnippetDuration);
                speechSource.time = Random.Range(0f, maxStart);
                speechSource.Play();
            }

            dialogueText.text += (dialogueText.text.Length > 0 ? " " : "") + word;

            yield return new WaitForSeconds(wordDelay);

            speechSource.Stop();
        }

        isTyping = false;
    }

    void EndDialogue()
    {
        isDialogActive = false;

        // Transition to another scene if enabled
        if (enableSceneTransition && !string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
            return;
        }

        // Unfreeze player
        if (playerMovement != null)
            playerMovement.enabled = true;

        // Hide UI
        dialoguePanel.SetActive(false);

        // Restore cameras
        dialogueCamera.enabled = false;
        mainCamera.enabled = true;
    }
}
