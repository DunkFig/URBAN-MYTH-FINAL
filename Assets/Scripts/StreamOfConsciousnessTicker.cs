// StreamOfConsciousnessTicker.cs ─ v2
using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class StreamOfConsciousnessTicker : MonoBehaviour
{
    [Header("References")]
    [SerializeField] TextMeshProUGUI tickerText;   // assign in Inspector
    [SerializeField] TextAsset       sourceFile;   // drag your .txt here

    [Header("Scrolling")]
    [SerializeField] float baseSpeed        = 100f;   // starting pixels‑per‑second
    [SerializeField] float accelPerSecond   = 10f;    // how much speed grows / sec
    [SerializeField] string separator       = "     ";

    // ─────────────────────────────────────────────────────────────────────────
    // ░ STATIC  →  persists across scene loads within the same play session ░
    // ─────────────────────────────────────────────────────────────────────────
    static bool   s_Initialized;          // have we visited *any* ticker yet?
    static float  s_ElapsedInTicker;      // seconds spent inside the ticker scene(s)
    static float  s_SavedOffset;          // x‑offset stored when scene last exited

    // ─────────────────────────────────────────────────────────────────────────
    RectTransform _rect;
    float         _loopWidth;             // half‑width of the doubled string
    float         _scrollPos;             // cached pos to avoid GetComponent calls

    void Awake()
    {
        if (tickerText == null || sourceFile == null)
        {
            Debug.LogError("Ticker not configured.");
            enabled = false;
            return;
        }

        _rect = tickerText.rectTransform;

        // Build doubled string once
        string raw = sourceFile.text.Replace("\r", "");
        string loop = raw + separator;

        // Duplicate so wrapping is seamless (loop+loop)
        tickerText.text = loop + loop;

        // Starting position
        _scrollPos = s_Initialized ? -s_SavedOffset : 0f;

        // Move the rect immediately (before first frame)
        _rect.anchoredPosition = new Vector2(_scrollPos, 0f);

        // Make sure we keep speed running only while this scene is active
        SceneManager.sceneUnloaded += OnSceneUnloaded;

        if (PlayerPrefs.HasKey("TickerOffset"))
    {
        s_SavedOffset = PlayerPrefs.GetFloat("TickerOffset");
        s_Initialized = true;
    }
    //  ────────────────────────────────────────────────────────────────────
    
    _scrollPos = s_Initialized ? -s_SavedOffset : 0f;
    _rect.anchoredPosition = new Vector2(_scrollPos, -29f);   // keep Y fixed
    SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    IEnumerator Start()
    {
        // Wait one frame for TextMeshPro geometry, then cache loop width
        yield return null;
        _loopWidth = tickerText.preferredWidth * 0.5f; // half because doubled
    }

    void Update()
    {
        // 1) Advance cumulative time spent *inside* ticker scenes
        s_ElapsedInTicker += Time.deltaTime;

        // 2) Compute current speed
        float speed = baseSpeed + accelPerSecond * s_ElapsedInTicker;

        // 3) Scroll left
        _scrollPos -= speed * Time.deltaTime;
        if (_scrollPos <= -_loopWidth)
            _scrollPos += _loopWidth;          // wrap seamlessly

        _rect.anchoredPosition = new Vector2(_scrollPos, -29f);
    }

    void OnSceneUnloaded(Scene scene)
    {
        s_Initialized = true;

        // Correct, always‑positive distance walked so far
        s_SavedOffset = Mathf.Repeat(-_scrollPos, _loopWidth);

        // Optional persistence between launches
        PlayerPrefs.SetFloat("TickerOffset", s_SavedOffset);
        PlayerPrefs.Save();

        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    void OnDestroy()  // safety net for Domain Reload disabled
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }
}
