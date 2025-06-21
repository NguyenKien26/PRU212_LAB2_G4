using UnityEngine;

public class PauseButtonHandler : MonoBehaviour
{
    private PauseManager pauseManager;

    void Start()
    {
        pauseManager = Object.FindFirstObjectByType<PauseManager>();
        if (pauseManager == null)
        {
            Debug.LogError("PauseManager không t?m th?y. Ð?m b?o nó ðang t?n t?i và có DontDestroyOnLoad.");
        }
    }

    public void OnPauseButtonClicked()
    {
        pauseManager?.PauseGame();
    }
}
