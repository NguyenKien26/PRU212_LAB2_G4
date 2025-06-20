using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject optionPanel;
    public GameObject leaderboardPanel;
    public GameObject leaderboardButton;

    public LeaderboardManager leaderboardManager;

    public void StartGame()
    {
        SceneManager.LoadScene("LevelSelection");
    }

    public void Options()
    {
        optionPanel.SetActive(true);
        leaderboardButton.SetActive(false);
    }

    public void CloseOptions()
    {
        optionPanel.SetActive(false);
        leaderboardButton.SetActive(true);
    }

    public void ShowLeaderboard()
    {
        leaderboardPanel.SetActive(true);
        leaderboardManager.LoadLeaderboard(); 
    }

    public void CloseLeaderboard()
    {
        leaderboardPanel.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
