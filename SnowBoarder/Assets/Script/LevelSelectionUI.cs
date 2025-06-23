using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelSelectionUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI highScoreText_Level1;
    [SerializeField] private TextMeshProUGUI highScoreText_Level2;
    public Button backToMenu;

    void Start()
    {
        backToMenu.onClick.AddListener(BackToMenu);
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager.Instance is null. Cannot load scores.");
            return;
        }

        var level1 = GameManager.Instance.GetLevelData(1);
        var level2 = GameManager.Instance.GetLevelData(2);

        if (level1 != null && highScoreText_Level1 != null)
        {
            highScoreText_Level1.text = $"High Score: {level1.highestScore}";
        }

        if (level2 != null && highScoreText_Level2 != null)
        {
            highScoreText_Level2.text = $"High Score: {level2.highestScore}";
        }
    }

    public void BackToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MenuScene");
    }
}