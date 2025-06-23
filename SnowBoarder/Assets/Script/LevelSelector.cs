using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LevelSelector : MonoBehaviour
{
    public int level;
    public TextMeshProUGUI levelText;
    private Button button;

    void Start()
    {
        button = GetComponent<Button>();
        levelText.text = level.ToString();

        bool isUnlocked = GameManager.Instance.IsLevelUnlocked(level);
        button.interactable = isUnlocked;
        levelText.alpha = isUnlocked ? 1f : 0.5f;
    }

    public void OpenScene()
    {
        if (GameManager.Instance.IsLevelUnlocked(level))
        {
            GameManager.Instance.currentLevel = level;
            SceneManager.LoadScene("Level" + level.ToString());
        }
    }
}