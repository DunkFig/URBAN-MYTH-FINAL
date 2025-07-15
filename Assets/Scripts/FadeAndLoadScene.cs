using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CanvasGroup))]
public class FadeAndLoadScene : MonoBehaviour
{
    [Header("Fade Settings")]
    public float fadeDuration = 2f;
    public string sceneToLoad;

    [Header("Audio Fade")]
    public bool fadeAudio = true;

    private CanvasGroup canvasGroup;
    private PlayerControls controls;

    void Awake()
    {
        // Setup canvas group
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;

        // Setup input
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
        // Use your mapped Interact action (Square button!)
        if (controls.Player.Interact.triggered)
        {
            StartCoroutine(FadeOutAndLoad());
        }
    }

    public IEnumerator FadeOutAndLoad()
    {
        canvasGroup.blocksRaycasts = true;

        float timeElapsed = 0f;
        float initialVolume = AudioListener.volume;

        while (timeElapsed < fadeDuration)
        {
            float t = timeElapsed / fadeDuration;

            // Fade to black
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);

            // Fade audio if enabled
            // if (fadeAudio)
            //     AudioListener.volume = Mathf.Lerp(initialVolume, 0f, t);

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure fully faded
        canvasGroup.alpha = 1f;
        // if (fadeAudio)
        //     AudioListener.volume = 0f;

        // Load new scene
        SceneManager.LoadScene(sceneToLoad);
    }
}
