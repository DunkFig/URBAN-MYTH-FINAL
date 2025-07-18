using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CanvasGroup))]
public class FadeAndLoadScene : MonoBehaviour
{
    [Header("Fade Settings")]
    [SerializeField] float  fadeDuration = 2f;

    // Name of the scene you want to load (must be in Build Settings)
    [SerializeField] string sceneToLoad = "NextScene";

    // [Header("Audio Fade")]
    // [SerializeField] bool   fadeAudio = true;

    CanvasGroup     canvasGroup;
    PlayerControls  controls;
    bool            isFading;
    int             targetBuildIndex = -1;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;

        controls = new PlayerControls();

        // Resolve once so we catch typos in the Editor
        targetBuildIndex = SceneUtility.GetBuildIndexByScenePath(sceneToLoad);
        if (targetBuildIndex < 0)
        {
            // Try by name (common in 2022)
            targetBuildIndex = SceneManager.GetSceneByName(sceneToLoad).buildIndex;
        }

#if UNITY_EDITOR
        if (targetBuildIndex < 0)
            Debug.LogError($"[FadeAndLoadScene] Scene \"{sceneToLoad}\" is not listed in Build Settings!");
#endif
    }

    void OnEnable()  => controls.Enable();
    void OnDisable() => controls.Disable();

    void Update()
    {
        if (isFading) return;

        // Fire only on *this* frame’s press – avoids held‑button retriggers
        if (controls.Player.Interact.WasPerformedThisFrame())
        {
            // Skip if our target scene is invalid or same as current
            if (targetBuildIndex < 0 ||
                targetBuildIndex == SceneManager.GetActiveScene().buildIndex)
            {
#if UNITY_EDITOR
                Debug.LogWarning("[FadeAndLoadScene] Ignoring scene load; target invalid or same as current.");
#endif
                return;
            }

            StartCoroutine(FadeOutAndLoad());
        }
    }

    IEnumerator FadeOutAndLoad()
    {
        isFading = true;
        canvasGroup.blocksRaycasts = true;

        float t = 0f;
        float startVol = AudioListener.volume;

        while (t < fadeDuration)
        {
            float k = t / fadeDuration;
            canvasGroup.alpha = k;
            // if (fadeAudio) AudioListener.volume = Mathf.Lerp(startVol, 0f, k);

            t += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = 1f;
        // if (fadeAudio) AudioListener.volume = 0f;

        // Load by build index if we resolved one; otherwise fall back to name
        if (targetBuildIndex >= 0)
            SceneManager.LoadScene(targetBuildIndex);
        else
            SceneManager.LoadScene(sceneToLoad);
    }
}
