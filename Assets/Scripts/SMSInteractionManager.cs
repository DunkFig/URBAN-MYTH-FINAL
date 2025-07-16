using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class SMSInteractionManager : MonoBehaviour
{
    [Header("Server")]
    public string serverBaseUrl = "https://sms.abstract.computer";

    [Header("UI")]
    public Transform contentRoot; 
    public GameObject textEntryPrefab; 
    public ScrollRect scrollRect;
    public PromptSynthesizer promptSynthesizer;

    [Header("Timing")]
    public float submissionTimeLimit = 30f; 
    [HideInInspector] public float timeLeft;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip randomClip;

    private bool isCollecting = false;
    public List<string> localSeenSubmissions = new List<string>();

    private PlayerControls controls;

    void Start()
    {
        StartCoroutine(ResetSubmissionsAtStart());
    }

    void Awake()
    {
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
        if (controls != null && controls.Player.Interact.triggered && !isCollecting)
        {
            StartCoroutine(StartCollectionWindow());
        }
    }

    public IEnumerator StartCollectionWindow()
    {
        Debug.Log("Starting SMS collection window!");
        isCollecting = true;
        timeLeft = submissionTimeLimit;

        UnityWebRequest startReq = UnityWebRequest.PostWwwForm(serverBaseUrl + "/start-submissions", "");
        yield return startReq.SendWebRequest();

        StartCoroutine(PollSubmissionsRoutine());

        while (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            yield return null;
        }

        UnityWebRequest stopReq = UnityWebRequest.PostWwwForm(serverBaseUrl + "/stop-submissions", "");
        yield return stopReq.SendWebRequest();

        Debug.Log("Collection window closed.");

        isCollecting = false;

        HandleCollectedData();
    }

    IEnumerator PollSubmissionsRoutine()
    {
        while (isCollecting)
        {
            UnityWebRequest req = UnityWebRequest.Get(serverBaseUrl + "/submissions");
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                SubmissionList submissionList = JsonUtility.FromJson<SubmissionList>(req.downloadHandler.text);
                foreach (var sub in submissionList.submissions)
                {
                    string uniqueKey = sub.from + ":" + sub.text;
                    if (!localSeenSubmissions.Contains(uniqueKey))
                    {
                        localSeenSubmissions.Add(uniqueKey);
                        AddSubmissionToUI(sub.from, sub.text);
                        PlayRandomAudioSnippet();
                    }
                }
            }
            else
            {
                Debug.LogError("Failed to fetch submissions: " + req.error);
            }

            yield return new WaitForSeconds(1f);
        }
    }

    void AddSubmissionToUI(string from, string text)
    {
        GameObject entry = Instantiate(textEntryPrefab, contentRoot);
        entry.SetActive(true);

        TMP_Text entryText = entry.GetComponentInChildren<TMP_Text>();
        if (entryText != null)
        {
            entryText.text = from + " : " + text;
        }
        else
        {
            Debug.LogError("No TMP_Text found in spawned prefab!");
        }

        StartCoroutine(ScrollToBottomNextFrame());
    }

    IEnumerator ScrollToBottomNextFrame()
    {
        yield return null; // Wait one frame
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }

    void PlayRandomAudioSnippet()
    {
        if (randomClip == null || audioSource == null)
            return;

        float maxStart = Mathf.Max(0f, randomClip.length - 0.5f);
        audioSource.time = Random.Range(0f, maxStart);
        audioSource.clip = randomClip;
        audioSource.Play();
        StartCoroutine(StopAfterHalfSec());
    }

    IEnumerator StopAfterHalfSec()
    {
        yield return new WaitForSeconds(0.5f);
        audioSource.Stop();
    }

    void HandleCollectedData()
    {
        Debug.Log("🔗 HandleCollectedData: triggering synthesis.");

        if (promptSynthesizer != null)
        {
            promptSynthesizer.StartSynthesis();
        }
        else
        {
            Debug.LogWarning("⚠️ No PromptSynthesizer assigned in SMSInteractionManager!");
        }    
    }

    [System.Serializable]
    public class SubmissionList
    {
        public List<Submission> submissions;
    }

    [System.Serializable]
    public class Submission
    {
        public string from;
        public string text;
    }


public IEnumerator ResetSubmissionsAtStart()
{
    UnityWebRequest resetReq = UnityWebRequest.PostWwwForm(serverBaseUrl + "/reset-submissions", "");
    yield return resetReq.SendWebRequest();

    if (resetReq.result == UnityWebRequest.Result.Success)
    {
        Debug.Log("✅ Submissions reset on server start");
    }
    else
    {
        Debug.LogError("❌ Failed to reset submissions: " + resetReq.error);
    }
}

public void ClearScrollView()
{
    foreach (Transform child in contentRoot)
    {
        Destroy(child.gameObject);
    }
    Debug.Log("✅ ScrollView cleared!");
}



}
