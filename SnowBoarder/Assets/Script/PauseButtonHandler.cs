using UnityEngine;

public class PauseButtonHandler : MonoBehaviour
{
    private PauseManager pauseManager;

    void Start()
    {
        pauseManager = Object.FindFirstObjectByType<PauseManager>();
        if (pauseManager == null)
        {
            Debug.LogError("PauseManager not found.  make sure  exist and have DontDestroyOnLoad.");
        }
    }

    public void OnPauseButtonClicked()
    {
        pauseManager?.PauseGame();
    }
}
