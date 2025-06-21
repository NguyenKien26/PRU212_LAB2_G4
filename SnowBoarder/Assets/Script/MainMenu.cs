using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject optionPanel;
    public GameObject howToPlayPanel;
    public GameObject howToPlayButton;

    public void StartGame()
    {
        SceneManager.LoadScene("Level1");
    }

    public void Options()
    {
        optionPanel.SetActive(true);
        howToPlayButton.SetActive(false);
    }

    public void CloseOptions()
    {
        optionPanel.SetActive(false);
        howToPlayButton.SetActive(true);
    }

    public void ShowHowToPlay()
    {
        howToPlayPanel.SetActive(true);
    }

    public void CloseHowToPlay()
    {
        howToPlayPanel.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
