using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class NPCDialogue : MonoBehaviour
{
    [Header("Dialogue Data")]
    [TextArea] public string[] dialogueLines;
    public float  wordDelay            = 0.2f;
    public float  audioSnippetDuration = 0.2f;
    public AudioClip speechAudioClip;
    public AudioClip skipAudioClip;
    public float  interactionDistance  = 3f;

    [Header("References")]
    public Transform      playerTransform;   // Your Player or Camera
    public PlayerMovement playerMovement;    // Drag in your PlayerMovement script here
    public Camera         mainCamera;        // Your main FPV camera
    public Camera         dialogueCamera;    // Zoom-in camera on NPC’s face
    public GameObject     dialoguePanel;     // World-space Canvas root
    public TMP_Text       dialogueText;      // TextMeshPro UGUI component

    [Header("Scene Transition")]
    [Tooltip("Enable to transition to another scene after dialogue ends")]
    public bool enableSceneTransition = false;

    #if UNITY_EDITOR
    [Tooltip("Drag the Scene asset you want to load (must be added to Build Settings)")]
    public SceneAsset nextSceneAsset;
    #endif

    // internal state
    private AudioSource speechSource, sfxSource;
    private int        currentLineIndex = 0;
    private bool       isDialogActive   = false;
    private bool       isTyping         = false;
    private Coroutine  typingCoroutine;
    private string     nextSceneName;

    void Awake()
    {
        // add & configure AudioSources
        speechSource = gameObject.AddComponent<AudioSource>();
        speechSource.clip = speechAudioClip;
        sfxSource    = gameObject.AddComponent<AudioSource>();

        // determine scene name at runtime
        #if UNITY_EDITOR
        if (nextSceneAsset != null)
            nextSceneName = nextSceneAsset.name;
        #endif

        // hide dialogue UI + dialogue camera initially
        dialoguePanel.SetActive(false);
        dialogueCamera.enabled = false;
    }

    void Update()
    {
        // only allow pressing Space to start/advance when in range
        if (Vector3.Distance(playerTransform.position, transform.position) > interactionDistance)
            return;

        if (!isDialogActive && Input.GetKeyDown(KeyCode.Space))
        {
            StartDialogue();
        }
        else if (isDialogActive && Input.GetKeyDown(KeyCode.Space))
        {
            sfxSource.PlayOneShot(skipAudioClip);

            if (isTyping)
            {
                // finish current panel immediately
                StopCoroutine(typingCoroutine);
                dialogueText.text = dialogueLines[currentLineIndex];
                isTyping = false;
            }
            else
            {
                // move to next panel or end
                currentLineIndex++;
                if (currentLineIndex < dialogueLines.Length)
                    typingCoroutine = StartCoroutine(TypeLine(dialogueLines[currentLineIndex]));
                else
                    EndDialogue();
            }
        }
    }

    void StartDialogue()
    {
        isDialogActive   = true;
        currentLineIndex = 0;

        // freeze player controls
        if (playerMovement != null)
            playerMovement.enabled = false;

        dialoguePanel.SetActive(true);

        // swap cameras
        mainCamera.enabled     = false;
        dialogueCamera.enabled = true;

        typingCoroutine = StartCoroutine(TypeLine(dialogueLines[0]));
    }

    IEnumerator TypeLine(string line)
    {
        isTyping        = true;
        dialogueText.text = string.Empty;

        string[] words = line.Split(' ');
        foreach (var word in words)
        {
            // random snippet from speech clip
            float maxStart = Mathf.Max(0f, speechSource.clip.length - audioSnippetDuration);
            speechSource.time = Random.Range(0f, maxStart);
            speechSource.Play();

            // append next word
            dialogueText.text += (dialogueText.text.Length > 0 ? " " : string.Empty) + word;

            yield return new WaitForSeconds(wordDelay);
            speechSource.Stop();
        }

        isTyping = false;
    }

    void EndDialogue()
    {
        isDialogActive = false;

        // if transition is enabled and a scene name is set, load next scene
        if (enableSceneTransition && !string.IsNullOrEmpty(nextSceneName))
        {
            // ensure scene is added to Build Settings
            SceneManager.LoadScene(nextSceneName);
            return;
        }

        // unfreeze player controls
        if (playerMovement != null)
            playerMovement.enabled = true;

        dialoguePanel.SetActive(false);

        // swap cameras back
        dialogueCamera.enabled = false;
        mainCamera.enabled     = true;
    }
}
