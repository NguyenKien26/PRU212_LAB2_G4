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
        // Tìm và gán lại nút pause mới (nếu có trong scene)
        pauseButton = GameObject.Find("PauseButton")?.GetComponent<Button>();
        if (pauseButton != null)
        {
            pauseButton.onClick.RemoveAllListeners(); // Xóa sự kiện cũ nếu có
            pauseButton.onClick.AddListener(PauseGame);
            pauseButton.gameObject.SetActive(true);
        }

        if (resumeButton == null)
            resumeButton = GameObject.Find("ResumeButton")?.GetComponent<Button>();
        if (returnMenuButton == null)
            returnMenuButton = GameObject.Find("ReturnButton")?.GetComponent<Button>();

        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);
        if (returnMenuButton != null)
            returnMenuButton.onClick.AddListener(ReturnToMenu);

        if (scene.name == "MenuScene")
        {
            pausePanel.SetActive(false); // Ẩn panel khi ở Menu
            pauseButton.gameObject.SetActive(false); // Ẩn nút pause luôn
            pausePanel?.SetActive(false);
            pauseButton?.gameObject.SetActive(false);
        }
        else
        {
            pausePanel.SetActive(false); // Đảm bảo panel bị ẩn khi vào gameplay
            pauseButton.gameObject.SetActive(true); // Hiện nút pause
            pausePanel?.SetActive(false);
            pauseButton?.gameObject.SetActive(true);
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
