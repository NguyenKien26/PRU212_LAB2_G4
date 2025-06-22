using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class WinPanelManager : MonoBehaviour
{
    public GameObject winPanel;
    public Button toLevelSelectionButton;
    public Button backToMenuButton;
    public Button pauseButton;

    public TextMeshProUGUI highestScoreText;

    void Start()
    {
        winPanel.SetActive(false);
        toLevelSelectionButton.onClick.AddListener(GoToLevelSelector);
        backToMenuButton.onClick.AddListener(BackToMenu);
    }

    public void ShowWinPanel()
    {
        winPanel.SetActive(true);
        if (pauseButton != null)
            pauseButton.gameObject.SetActive(false);
        int level = GameManager.Instance.currentLevel;
        LevelScore data = GameManager.Instance.GetLevelData(level);

        if (data != null)
        {
            highestScoreText.text = $"Highest Score: {data.highestScore}";
        }
        else
        {
            highestScoreText.text = "Highest Score: N/A";
        }
    }

    private void GoToLevelSelector()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("LevelSelection");
    }

    private void BackToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MenuScene");
    }
}
