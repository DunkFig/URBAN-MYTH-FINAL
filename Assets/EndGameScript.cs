using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class EndGameScript : MonoBehaviour
{
    [Header("Timers")]
    public float countdownTime = 10f;   // Time before Game Over panel fades in
    public float finalDelayTime = 5f;   // Time before final fade & scene change

    [Header("UI")]
    public CanvasGroup gameOverPanel;   // Panel that says "Game Over"
    public CanvasGroup finalBlackoutPanel; // Final black fade-out
    public Image[] gameOverImages;      // Any images/texts on the panel you want to fade with it

    [Header("Post Processing")]
    public Volume postProcessVolume;
    private ColorAdjustments colorAdjustments;
    private LensDistortion lensDistortion;
    private ChromaticAberration chromaticAberration;

    [Header("Post FX Targets")]
    public float targetContrast = -100f;
    public float targetHueShift = 180f;
    public float targetLensDistortion = -1f;
    public float targetChromaticAberration = 1f;

    [Header("Audio")]
    public AudioSource endTrackAudio;  // The designated end track
    private AudioSource[] allAudioSources;

    [Header("Scene")]
    public string nextSceneName;

    void Awake()
    {
        // Find Post-Processing effects
        postProcessVolume.profile.TryGet(out colorAdjustments);
        postProcessVolume.profile.TryGet(out lensDistortion);
        postProcessVolume.profile.TryGet(out chromaticAberration);

        // Ensure panels are transparent at start
        if (gameOverPanel != null) gameOverPanel.alpha = 0f;
        if (finalBlackoutPanel != null) finalBlackoutPanel.alpha = 0f;

        // Also zero any child images if you want them to fade with the panel
        if (gameOverImages != null)
        {
            foreach (var img in gameOverImages)
                img.color = new Color(img.color.r, img.color.g, img.color.b, 0f);
        }
    }

    public void TriggerEndSequence()
    {
        Debug.Log("🔴 EndGameScript triggered!");

        // ✅ Mute all other audio except end track
        allAudioSources = FindObjectsOfType<AudioSource>();
        foreach (AudioSource src in allAudioSources)
        {
            if (src != endTrackAudio)
                src.mute = true;
        }

        // ✅ Play the end track
        if (!endTrackAudio.isPlaying)
            endTrackAudio.Play();

        // ✅ Start the sequence
        StartCoroutine(EndSequence());
    }

    IEnumerator EndSequence()
    {
        float t = 0f;

        // Store initial values
        float initialContrast = colorAdjustments.contrast.value;
        float initialHueShift = colorAdjustments.hueShift.value;
        float initialLensDistortion = lensDistortion.intensity.value;
        float initialChromaticAberration = chromaticAberration.intensity.value;

        // Lerp post-process values over countdown
        while (t < countdownTime)
        {
            t += Time.deltaTime;
            float progress = t / countdownTime;

            colorAdjustments.contrast.value = Mathf.Lerp(initialContrast, targetContrast, progress);
            colorAdjustments.hueShift.value = Mathf.Lerp(initialHueShift, targetHueShift, progress);
            lensDistortion.intensity.value = Mathf.Lerp(initialLensDistortion, targetLensDistortion, progress);
            chromaticAberration.intensity.value = Mathf.Lerp(initialChromaticAberration, targetChromaticAberration, progress);

            yield return null;
        }

        // ✅ Fade in Game Over panel + images
        yield return StartCoroutine(FadeCanvasGroup(gameOverPanel, 0f, 1f, 1f));

        if (gameOverImages != null)
        {
            foreach (var img in gameOverImages)
            {
                StartCoroutine(FadeImage(img, 0f, 1f, 1f));
            }
        }

        // ✅ Wait for final delay
        yield return new WaitForSeconds(finalDelayTime);

        // ✅ Fade in final black panel
        yield return StartCoroutine(FadeCanvasGroup(finalBlackoutPanel, 0f, 1f, 1.5f));

        // ✅ Load next scene
        SceneManager.LoadScene(nextSceneName);
    }

    IEnumerator FadeCanvasGroup(CanvasGroup group, float startAlpha, float endAlpha, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            group.alpha = Mathf.Lerp(startAlpha, endAlpha, t / duration);
            yield return null;
        }
        group.alpha = endAlpha;
    }

    IEnumerator FadeImage(Image img, float startAlpha, float endAlpha, float duration)
    {
        float t = 0f;
        Color original = img.color;
        while (t < duration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(startAlpha, endAlpha, t / duration);
            img.color = new Color(original.r, original.g, original.b, a);
            yield return null;
        }
        img.color = new Color(original.r, original.g, original.b, endAlpha);
    }
}
