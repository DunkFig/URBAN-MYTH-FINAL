// ResolutionGuard.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResolutionGuard : MonoBehaviour
{
    const int TargetWidth  = 1920;
    const int TargetHeight = 1080;
    const FullScreenMode Mode = FullScreenMode.Windowed;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);   // persist through all scenes
        SceneManager.activeSceneChanged += (_, __) => Apply(); // whenever a new scene loads
        Apply();                         // …and right now
    }

    void Apply()
    {
        if (Screen.width != TargetWidth || Screen.height != TargetHeight
            || Screen.fullScreenMode != Mode)
        {
            Screen.SetResolution(TargetWidth, TargetHeight, Mode);
        }
    }
}
