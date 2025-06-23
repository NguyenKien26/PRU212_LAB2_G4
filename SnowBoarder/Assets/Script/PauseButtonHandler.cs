using UnityEngine;

public class PauseButtonHandler : MonoBehaviour
{
    private PauseManager pauseManager;

    void Start()
    {
        pauseManager = Object.FindFirstObjectByType<PauseManager>();
        if (pauseManager == null)
        {
            Debug.LogError("PauseManager không tim thay. Ðam bao nó dang tin t?i và có DontDestroyOnLoad.");
        }
    }

    public void OnPauseButtonClicked()
    {
        pauseManager?.PauseGame();
    }
}
