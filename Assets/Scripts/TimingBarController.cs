using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimingBarController : MonoBehaviour
{
    [Header("References")]
    public SMSInteractionManager smsManager; // Drag your SMSInteractionManager in here
    public PromptSynthesizer promptSynthesizer;  // Drag your PromptSynthesizer here!
    public TMP_Text timerText;               // Drag your TMP_Text UI here
    public RectTransform timingBar;          // Drag your bar’s RectTransform here

    [Header("Timing Bar Size")]
    public float fullWidth = 400f;           // The width of the bar when full

    [Header("Audio")]
    public AudioSource anticipationSource;   // Drag your anticipation AudioSource here
    public AudioSource finalBeepSource;      // Drag your final beep AudioSource here

    [Header("Volume Settings")]
    public float minVolumeDb = -144f;        // Volume at start
    public float maxVolumeDb = -12f;         // Volume when bar is empty

    private bool playedEndSound = true;
    private bool hasTriggeredSynthesis = false;


    void Update()
    {
        if (smsManager == null)
            return;

        float timeLeft = smsManager.timeLeft;
        float timeLimit = smsManager.submissionTimeLimit;

        // Clamp to avoid negative
        timeLeft = Mathf.Max(0f, timeLeft);

        // --------------------------
        // 1️⃣ Update the timer text
        // --------------------------
        timerText.text = timeLeft.ToString("0.0") + "s";

        // --------------------------
        // 2️⃣ Update timing bar width
        // --------------------------
        float t = timeLeft / timeLimit; // 1.0 → 0.0
        Vector2 size = timingBar.sizeDelta;
        size.x = fullWidth * t;
        timingBar.sizeDelta = size;

        // --------------------------
        // 3️⃣ Update anticipation volume
        // --------------------------
        float newVolumeDb = Mathf.Lerp(maxVolumeDb, minVolumeDb, t); // Invert because minDb = full bar
        anticipationSource.volume = DbToLinear(newVolumeDb);

        if (!anticipationSource.isPlaying && timeLeft > 0)
            anticipationSource.Play();

        // --------------------------
        // 4️⃣ Play final beep at zero
        // --------------------------
        if (timeLeft <= 0f && !playedEndSound)
        {
            hasTriggeredSynthesis = true;
            promptSynthesizer.StartSynthesis();
            playedEndSound = true;
            anticipationSource.Stop();
            finalBeepSource.Play();
        }

        if (timeLeft > 0f)
        {
            hasTriggeredSynthesis = false;
            playedEndSound = false; // Reset so it can play again
        }
    }

    float DbToLinear(float db)
    {
        return Mathf.Pow(10f, db / 20f);
    }
}
