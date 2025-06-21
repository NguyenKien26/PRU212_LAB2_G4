using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject optionPanel;
    public GameObject optionButton;
    public GameObject howToPlayPanel;
    public GameObject howToPlayButton;

    public void StartGame()
    {
        SceneManager.LoadScene("LevelSelection");
    }

    public void ShowOptions()
    {
        optionPanel.SetActive(true);
        howToPlayButton.SetActive(false);
    }

    public void CloseOptions()
    {
        howToPlayButton.SetActive(true);
        optionPanel.SetActive(false);
    }

    public void ShowHowToPlay()
    {
        howToPlayPanel.SetActive(true);
        optionButton.SetActive(false);
    }

    public void CloseHowToPlay()
    {
        howToPlayPanel.SetActive(false);
        optionButton.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
