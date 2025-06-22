using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;
using TMPro;

[System.Serializable]
public class ScoreData
{
    public string playerName;
    public List<LevelScore> scores;
}

[System.Serializable]
public class LevelScore
{
    public int level;
    public int highestScore;
    public float highestDistance;
    public bool isFinished;
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // Singleton

    [SerializeField] private string scoreFileName = "scores.json";
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private TextMeshProUGUI distanceText;
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private TextMeshProUGUI congratText;
    [SerializeField] private GameObject floatingTextPrefab;

    private ScoreData scoreData;
    public int currentScore = 0;
    public float currentDistance = 0f;
    public float currentSpeed = 0f;
    public int currentLevel = 1; // Mặc định bắt đầu từ level 1
    private Canvas canvas;

    void Awake()
    {
        // Thiết lập Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Canvas not found in scene!");
        }

        LoadScoreData();
        if (congratText != null)
        {
            congratText.gameObject.SetActive(false); // Ẩn CongratText ban đầu
        }
        else
        {
            Debug.LogWarning("CongratText is not assigned in Inspector!");
        }

        // Kiểm tra các TextMeshProUGUI
        CheckTextReferences();
        UpdateUI();
    }

    void Start()
    {
        // Đặt tên PC nếu chưa có
        if (string.IsNullOrEmpty(scoreData.playerName))
        {
            scoreData.playerName = SystemInfo.deviceName;
            SaveScoreData();
        }
    }

    // Kiểm tra các tham chiếu TextMeshProUGUI
    private void CheckTextReferences()
    {
        if (scoreText == null) Debug.LogError("ScoreText is not assigned in Inspector!");
        if (highScoreText == null) Debug.LogError("HighScoreText is not assigned in Inspector!");
        if (distanceText == null) Debug.LogError("DistanceText is not assigned in Inspector!");
        if (speedText == null) Debug.LogError("SpeedText is not assigned in Inspector!");
        if (floatingTextPrefab == null) Debug.LogError("FloatingTextPrefab is not assigned in Inspector!");
    }

    // Thêm điểm (gọi khi lộn nhào hoặc ăn coin)
    public void AddScore(int points, Vector3 worldPosition, string message, Color color)
    {
        Debug.Log($"AddScore called with points: {points}, message: {message}, caller: {new System.Diagnostics.StackTrace().ToString()}");
        currentScore += points;
        UpdateLevelData(currentLevel, currentScore, currentDistance, false);
        UpdateUI();
        if (canvas != null && floatingTextPrefab != null)
        {
            ShowFloatingText(worldPosition, message, color);
        }
    }

    // Cập nhật khoảng cách
    public void UpdateDistance(float distance)
    {
        currentDistance = Mathf.Max(currentDistance, distance);
        UpdateUI();
    }

    // Cập nhật tốc độ
    public void UpdateSpeed(float speed)
    {
        currentSpeed = speed;
        UpdateUI();
    }

    // Cập nhật UI
    private void UpdateUI()
    {
        LevelScore levelScore = GetLevelData(currentLevel);
        int highScore = levelScore != null ? levelScore.highestScore : 0;

        if (scoreText != null)
            scoreText.text = $"Score: {currentScore}";
        else
            Debug.LogWarning("Cannot update ScoreText: reference is null");

        if (highScoreText != null)
            highScoreText.text = $"High Score: {highScore}";
        else
            Debug.LogWarning("Cannot update HighScoreText: reference is null");

        if (distanceText != null)
            distanceText.text = $"Distance: {currentDistance:F1} m";
        else
            Debug.LogWarning("Cannot update DistanceText: reference is null");

        if (speedText != null)
            speedText.text = $"Speed: {currentSpeed:F1} m/s";
        else
            Debug.LogWarning("Cannot update SpeedText: reference is null");
    }

    // Hiển thị floating text
    private void ShowFloatingText(Vector3 worldPosition, string message, Color color)
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPosition);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.GetComponent<RectTransform>(),
            screenPos,
            canvas.worldCamera,
            out Vector2 localPos
        );
        GameObject floatingText = Instantiate(floatingTextPrefab, canvas.transform);
        floatingText.GetComponent<RectTransform>().localPosition = localPos;
        TextMeshProUGUI textComponent = floatingText.GetComponent<TextMeshProUGUI>();

        if (textComponent != null)
        {
            textComponent.text = message;
            textComponent.color = color;
            floatingText.SetActive(true);
            StartCoroutine(AnimateFloatingText(floatingText));
        }
        else
        {
            Debug.LogError("FloatingTextPrefab is missing TextMeshProUGUI component!");
            Destroy(floatingText);
        }
    }

    private System.Collections.IEnumerator AnimateFloatingText(GameObject floatingText)
    {
        float duration = 1f;
        float elapsed = 0f;
        RectTransform rectTransform = floatingText.GetComponent<RectTransform>();
        Vector3 startPos = rectTransform.localPosition;
        Vector3 endPos = startPos + new Vector3(0, 50, 0);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            rectTransform.localPosition = Vector3.Lerp(startPos, endPos, t);
            floatingText.GetComponent<TextMeshProUGUI>().alpha = 1 - t;
            yield return null;
        }

        Destroy(floatingText);
    }

    // Xử lý khi cán đích
    public void ReachFinish(int score, float distance)
    {
        UpdateLevelData(currentLevel, score, distance, true);
        currentLevel++;
        if (currentLevel <= 3) // Giả sử có 3 level
        {
            SceneManager.LoadScene($"Level{currentLevel}");
            ResetLevel();
        }
        else
        {
            if (congratText != null)
            {
                congratText.text = "Chúc mừng! Bạn đã phá đảo!";
                congratText.gameObject.SetActive(true);
                StartCoroutine(HideCongratTextAfterDelay(3f));
            }
            SceneManager.LoadScene("LevelSelection");
        }
    }

    // Ẩn CongratText sau một khoảng thời gian
    private System.Collections.IEnumerator HideCongratTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (congratText != null)
        {
            congratText.gameObject.SetActive(false);
        }
    }

    // Xử lý khi game over
    public void GameOver(int score, float distance)
    {
        UpdateLevelData(currentLevel, score, distance, false);
        SceneManager.LoadScene($"Level{currentLevel}");
        ResetLevel();
    }

    // Reset dữ liệu level
    private void ResetLevel()
    {
        currentScore = 0;
        currentDistance = 0f;
        currentSpeed = 0f;
        if (congratText != null)
        {
            congratText.gameObject.SetActive(false);
        }
        UpdateUI();
    }

    // Cập nhật dữ liệu level
    private void UpdateLevelData(int level, int score, float distance, bool finished)
    {
        LevelScore levelScore = scoreData.scores.Find(s => s.level == level);
        if (levelScore == null)
        {
            levelScore = new LevelScore
            {
                level = level,
                highestScore = score,
                highestDistance = distance,
                isFinished = finished
            };
            scoreData.scores.Add(levelScore);
        }
        else
        {
            if (score > levelScore.highestScore)
                levelScore.highestScore = score;
            if (distance > levelScore.highestDistance)
                levelScore.highestDistance = distance;
            if (finished)
                levelScore.isFinished = true;
        }
        SaveScoreData();
    }

    // Kiểm tra xem level có được mở khóa không
    public bool IsLevelUnlocked(int level)
    {
        if (level == 1) return true;
        LevelScore previousLevelScore = scoreData.scores.Find(s => s.level == level - 1);
        return previousLevelScore != null && previousLevelScore.isFinished;
    }

    // Lấy dữ liệu level
    public LevelScore GetLevelData(int level)
    {
        return scoreData.scores.Find(s => s.level == level);
    }

    // Khởi tạo dữ liệu mặc định
    private ScoreData CreateDefaultScoreData()
    {
        return new ScoreData
        {
            playerName = "",
            scores = new List<LevelScore>
            {
                new LevelScore { level = 1, highestScore = 0, highestDistance = 0f, isFinished = false },
                new LevelScore { level = 2, highestScore = 0, highestDistance = 0f, isFinished = false },
                new LevelScore { level = 3, highestScore = 0, highestDistance = 0f, isFinished = false }
            }
        };
    }

    // Đọc dữ liệu JSON
    private void LoadScoreData()
    {
        string filePath = Path.Combine(Application.persistentDataPath, scoreFileName);
        try
        {
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                if (!string.IsNullOrEmpty(json))
                {
                    scoreData = JsonUtility.FromJson<ScoreData>(json);
                    if (scoreData == null || scoreData.scores == null)
                    {
                        Debug.LogWarning("File JSON bị hỏng, tạo dữ liệu mặc định.");
                        scoreData = CreateDefaultScoreData();
                        SaveScoreData();
                    }
                }
                else
                {
                    Debug.LogWarning("File JSON rỗng, tạo dữ liệu mặc định.");
                    scoreData = CreateDefaultScoreData();
                    SaveScoreData();
                }
            }
            else
            {
                Debug.Log("File JSON không tồn tại, tạo file mới.");
                scoreData = CreateDefaultScoreData();
                SaveScoreData();
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Lỗi khi đọc file JSON: {ex.Message}. Tạo dữ liệu mặc định.");
            scoreData = CreateDefaultScoreData();
            SaveScoreData();
        }
    }

    // Lưu dữ liệu JSON
    private void SaveScoreData()
    {
        string filePath = Path.Combine(Application.persistentDataPath, scoreFileName);
        try
        {
            if (scoreData == null)
            {
                Debug.LogWarning("ScoreData null, khởi tạo dữ liệu mặc định.");
                scoreData = CreateDefaultScoreData();
            }
            string json = JsonUtility.ToJson(scoreData, true);
            File.WriteAllText(filePath, json);
            Debug.Log($"Lưu dữ liệu thành công tại: {filePath}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Lỗi khi lưu file JSON: {ex.Message}");
        }
    }
}