using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public GameObject pausePanel;
    public Button resumeButton;
    public Button returnMenuButton;
    public Button pauseButton;

    private void Awake()
    {
    }

    private void Start()
    {
        pauseButton.onClick.AddListener(PauseGame);
        resumeButton.onClick.AddListener(ResumeGame);
        returnMenuButton.onClick.AddListener(ReturnToMenu);

        pausePanel.SetActive(false); 
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MenuScene")
        {
            pausePanel.SetActive(false); // Ẩn panel khi ở Menu
            pauseButton.gameObject.SetActive(false); // Ẩn nút pause luôn
        }
        else
        {
            pausePanel.SetActive(false); // Đảm bảo panel bị ẩn khi vào gameplay
            pauseButton.gameObject.SetActive(true); // Hiện nút pause
        }
    }


    public void PauseGame()
    {
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void ReturnToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MenuScene");
    }
}
