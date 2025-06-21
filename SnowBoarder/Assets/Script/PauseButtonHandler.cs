using UnityEngine;

public class PauseButtonHandler : MonoBehaviour
{
    private PauseManager pauseManager;

    void Start()
    {
        pauseManager = Object.FindFirstObjectByType<PauseManager>();
        if (pauseManager == null)
        {
            Debug.LogError("PauseManager kh�ng t?m th?y. �?m b?o n� �ang t?n t?i v� c� DontDestroyOnLoad.");
        }
    }

    public void OnPauseButtonClicked()
    {
        pauseManager?.PauseGame();
    }
}
