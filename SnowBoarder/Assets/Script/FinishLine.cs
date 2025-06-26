using UnityEngine;
using UnityEngine.SceneManagement;

public class FinishLine : MonoBehaviour
{
    [SerializeField] float delayTime = 1f;
    [SerializeField] ParticleSystem finishEffect;

    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!hasTriggered && other.CompareTag("Player"))
        {
            hasTriggered = true;
            finishEffect?.Play();
            GetComponent<AudioSource>()?.Play();

            // đợi 1 xíu r hiện panel win
            Invoke(nameof(ShowWinPanel), delayTime);
        }
    }

    void ShowWinPanel()
    {
        GameManager.Instance.ReachFinish(GameManager.Instance.currentScore, GameManager.Instance.currentDistance);

        WinPanelManager winPanel = FindAnyObjectByType<WinPanelManager>();
        if (winPanel != null)
        {
            winPanel.ShowWinPanel();
            Time.timeScale = 0f; // pause game.
        }
        else
        {
            Debug.LogError("WinPanelManager not found in scene!");
        }
    }
}
