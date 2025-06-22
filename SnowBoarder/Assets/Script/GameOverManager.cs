using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class GameOverManager : MonoBehaviour
{
    public GameObject gameOverPanel;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI distanceText;
    public Button retryButton;
    public Button returnButton;
    public Button pauseButton;

    private int currentLevel;

    void Start()
    {
        gameOverPanel.SetActive(false);

        if (retryButton != null)
            retryButton.onClick.AddListener(RetryLevel);
        if (returnButton != null)
            returnButton.onClick.AddListener(ReturnToMenu);
    }

    public void ShowGameOver(int score, float distance)
    {
        Time.timeScale = 0f;
        currentLevel = GameManager.Instance.currentLevel;

        gameOverPanel.SetActive(true);
        scoreText.text = "Score: " + score;
        distanceText.text = "Distance: " + distance.ToString("F1") + "m";

        if (pauseButton != null)
            pauseButton.gameObject.SetActive(false);
    }

    public void RetryLevel()
    {
        Debug.Log("RetryLevel called");
        Time.timeScale = 1f;

        if (GameManager.Instance != null)
        {
            //GameManager.Instance.StopAllFloatingTextCoroutines(); // Dừng các FloatingText
            GameManager.Instance.ResetScoreAndDistance();
            GameManager.Instance.ResetPlayer();
            SceneManager.LoadScene("Level" + GameManager.Instance.currentLevel);
        }
        else
        {
            Debug.LogError("GameManager.Instance is null!");
        }
    }




    public void ReturnToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MenuScene");
    }
}
