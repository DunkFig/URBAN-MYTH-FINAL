using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

[RequireComponent(typeof(AudioSource))]
public class PromptSynthesizer : MonoBehaviour
{
    [Header("References")]
    public SMSInteractionManager smsManager;
    public RectTransform panel;
    public TMP_Text promptText;     // The final prompt sentence
    public TMP_Text explainerText;  // The animated CAVEMAN text
    public TMP_Text loadingText;    // The "loading..." label

    [Header("Server")]
    public string synthesizeUrl = "https://sms.abstract.computer/synthesize";

    [Header("Audio")]
    public AudioSource resultSound;     // Plays when final prompt appears
    public AudioClip speechAudioClip;   // Snippet for typing words
    public float wordDelay = 0.2f;
    public float audioSnippetDuration = 0.2f;

    private AudioSource speechSource;

    private bool isPanelVisible = false;
    private bool isTyping = false;

    private Coroutine typingCoroutine;

    private PlayerControls controls;

    void Start()
    {
        panel.gameObject.SetActive(false);  // Start hidden
    }

    void Awake()
    {
        controls = new PlayerControls();

        // Extra AudioSource for word snippets
        speechSource = gameObject.AddComponent<AudioSource>();
        speechSource.clip = speechAudioClip;
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
        if (controls.Player.Interact.triggered && isPanelVisible && !isTyping)
        {
            Debug.Log("Square pressed again: closing panel and resetting.");

            // ✅ Hide panel
            panel.gameObject.SetActive(false);
            isPanelVisible = false;

            // ✅ Clear the ScrollView content
            smsManager.ClearScrollView();

            // ✅ Clear local arrays & server submissions
            smsManager.StartCoroutine(smsManager.ResetSubmissionsAtStart());
            smsManager.localSeenSubmissions.Clear();

            // ✅ Restart collection window
            smsManager.StartCoroutine(smsManager.StartCollectionWindow());
        }
    }

    public void StartSynthesis()
    {
        Debug.Log("Starting synthesis process...");

        isPanelVisible = true;
        panel.gameObject.SetActive(true);

        loadingText.gameObject.SetActive(true);
        promptText.text = "";
        explainerText.text = "";

        StartCoroutine(SendForSynthesis());
    }

    IEnumerator SendForSynthesis()
    {
        loadingText.text = "Synthesizing... Please wait.";

        // Build the entries array
        List<string> entries = new List<string>();
        foreach (var sub in smsManager.localSeenSubmissions)
        {
            string[] parts = sub.Split(':');
            if (parts.Length > 1)
            {
                entries.Add(parts[1].Trim());
            }
        }

        string jsonBody = JsonUtility.ToJson(new BodyEntries() { entries = entries });

        Debug.Log("🚀 Sending synthesis request with: " + jsonBody);

        UnityWebRequest req = new UnityWebRequest(synthesizeUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
        req.uploadHandler = new UploadHandlerRaw(bodyRaw);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("✅ Synthesis received: " + req.downloadHandler.text);

            string raw = req.downloadHandler.text;
            Debug.Log("RAW JSON: " + raw);

            OpenAIResponse parsed = JsonUtility.FromJson<OpenAIResponse>(raw);

            // Split: expects [explanation]\n[prompt]
            string[] parts = parsed.result.Split(new[] { '\n' }, 2);

            string explanationPart = parts.Length > 0 ? parts[0].Trim() : "";
            string promptPart = parts.Length > 1 ? parts[1].Trim() : "";

            loadingText.gameObject.SetActive(false);

            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            typingCoroutine = StartCoroutine(TypeExplainerThenPrompt(explanationPart, promptPart));
        }
        else
        {
            Debug.LogError("❌ Synthesis failed: " + req.error);
            loadingText.text = "Error synthesizing.";
        }
    }

IEnumerator TypeExplainerThenPrompt(string explanation, string prompt)
{
    isTyping = true;

    explainerText.text = "";  // Top box
    promptText.text = "";     // Bottom box

    Debug.Log("🧩 Typing caveman explanation first: " + explanation);

    string[] words = prompt.Split(' ');
    foreach (string word in words)
    {
        if (speechAudioClip != null)
        {
            float maxStart = Mathf.Max(0f, speechAudioClip.length - audioSnippetDuration);
            speechSource.time = Random.Range(0f, maxStart);
            speechSource.Play();
        }

        explainerText.text += (explainerText.text.Length > 0 ? " " : "") + word;

        yield return new WaitForSeconds(wordDelay);

        speechSource.Stop();
    }

    yield return new WaitForSeconds(0.3f);

    promptText.text = explanation.Trim();
    resultSound?.Play();

    Debug.Log("✅ Final succinct prompt shown: " + prompt);

    isTyping = false;
}

    [System.Serializable]
    public class BodyEntries
    {
        public List<string> entries;
    }

    [System.Serializable]
    public class OpenAIResponse
    {
        public string result;
    }
}
