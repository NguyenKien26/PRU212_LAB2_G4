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
    public static GameManager Instance;

    [SerializeField] private string scoreFileName = "scores.json";
    [SerializeField] private float scorePerMeter = 10f;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI distanceText;
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private GameObject floatingTextPrefab;
    // Thêm các ngưỡng tốc độ để so sánh
    [SerializeField] private float slowSpeedThreshold = 5f;
    [SerializeField] private float defaultSpeedThreshold = 10f;
    [SerializeField] private float boostSpeedThreshold = 15f;

    private ScoreData scoreData;
    public int currentScore = 0;
    public float currentDistance = 0f;
    public float currentSpeed = 0f;
    public int currentLevel = 1;
    private float lastDistance = 0f;
    private Canvas canvas;
    private List<Coroutine> activeFloatingTextCoroutines = new List<Coroutine>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("GameManager created.");
        }
        else
        {
            Debug.LogWarning("Duplicate GameManager destroyed.");
            Destroy(gameObject);
            return;
        }

        canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("Canvas not found in scene during Awake! Will try to find in Start.");
        }
        //else
        //{
        //    DontDestroyOnLoad(canvas.gameObject);
        //}

        LoadScoreData();
        CheckTextReferences();
    }

    void Start()
    {
        if (string.IsNullOrEmpty(scoreData.playerName))
        {
            scoreData.playerName = SystemInfo.deviceName;
            SaveScoreData();
        }

        if (canvas == null)
        {
            canvas = FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                //DontDestroyOnLoad(canvas.gameObject);
                Debug.Log("Canvas found in Start.");
            }
            else
            {
                Debug.LogError("No Canvas found in scene!");
            }
        }

        CheckTextReferences();
        UpdateUI();
    }

    private void CheckTextReferences()
    {
        if (scoreText == null) Debug.LogError("ScoreText is not assigned in Inspector!");
        if (distanceText == null) Debug.LogError("DistanceText is not assigned in Inspector!");
        if (speedText == null) Debug.LogError("SpeedText is not assigned in Inspector!");
        if (floatingTextPrefab == null) Debug.LogError("FloatingTextPrefab is not assigned in Inspector!");
    }

    public void AddScore(int points, Vector3 worldPosition = default, string message = "", Color color = default)
    {
        currentScore += points;
        UpdateUI();

        if (!string.IsNullOrEmpty(message) && canvas != null && floatingTextPrefab != null)
        {
            PlayerController player = FindFirstObjectByType<PlayerController>();
            if (player != null)
            {
                Transform head = player.transform.Find("Head");
                worldPosition = head != null ? head.position : player.transform.position + Vector3.up * 1f;
            }
            ShowFloatingText(worldPosition, message, color == default ? Color.white : color);
        }
    }

    public void UpdateDistance(float distance)
    {
        currentDistance = Mathf.Max(currentDistance, distance);
        UpdateUI();
    }

    public void UpdateSpeed(float speed)
    {
        currentSpeed = speed;
        Debug.Log($"Speed updated: {speed}");
        UpdateUI();
    }

    public void UpdateDistanceAndScore(float currentX)
    {
        float delta = currentX - lastDistance;
        if (delta >= 1f)
        {
            int addScore = Mathf.FloorToInt(delta * scorePerMeter);
            AddScore(addScore);
            lastDistance = currentX;
        }

        UpdateDistance(currentX);

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateScore(currentScore);
            UIManager.Instance.UpdateDistance(currentDistance);
        }
    }

    private void UpdateUI()
    {
        if (scoreText != null)
            speedText.text = $"Score: {currentScore}";
        if (distanceText != null)
            distanceText.text = $"Distance: {currentDistance:F1} m";
        if (speedText != null)
        {
            // Định dạng tốc độ với màu sắc dựa trên nấc
            string speedTextString = $"Speed: {currentSpeed:F1} m/s";
            if (Mathf.Approximately(currentSpeed, slowSpeedThreshold))
            {
                speedText.text = $"<color=green>{speedTextString}</color>";
            }
            else if (Mathf.Approximately(currentSpeed, boostSpeedThreshold))
            {
                speedText.text = $"<color=red>{speedTextString}</color>";
            }
            else
            {
                speedText.text = $"<color=yellow>{speedTextString}</color>"; // Mặc định
            }
        }
    }

    private void ShowFloatingText(Vector3 worldPosition, string message, Color color)
    {
        if (Camera.main == null)
        {
            Debug.LogError("Main Camera not found, cannot show FloatingText!");
            return;
        }

        if (canvas == null)
        {
            Debug.LogError("Canvas is null, cannot show FloatingText!");
            return;
        }

        Debug.Log($"Showing FloatingText at world position: {worldPosition}, message: {message}, color: {color}");

        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPosition);
        Debug.Log($"Screen position: {screenPos}");

        bool isPointOnScreen = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.GetComponent<RectTransform>(),
            screenPos,
            canvas.worldCamera,
            out Vector2 localPos
        );

        Debug.Log($"Local position in Canvas: {localPos}, IsPointOnScreen: {isPointOnScreen}");

        if (!isPointOnScreen)
        {
            Debug.LogWarning("FloatingText position is off-screen!");
        }

        GameObject floatingText = Instantiate(floatingTextPrefab, canvas.transform);
        RectTransform rectTransform = floatingText.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.localPosition = localPos;
            Debug.Log($"FloatingText instantiated at local position: {rectTransform.localPosition}");
        }
        else
        {
            Debug.LogError("FloatingTextPrefab is missing RectTransform!");
        }

        TextMeshProUGUI textComponent = floatingText.GetComponent<TextMeshProUGUI>();
        if (textComponent != null)
        {
            textComponent.text = message;
            textComponent.color = color;
            floatingText.SetActive(true);
            Debug.Log($"FloatingText set: text={message}, color={color}, active={floatingText.activeSelf}");
            Coroutine coroutine = StartCoroutine(AnimateFloatingText(floatingText));
            activeFloatingTextCoroutines.Add(coroutine);
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
        Vector3 startPos = rectTransform != null ? rectTransform.localPosition : Vector3.zero;
        Vector3 endPos = startPos + new Vector3(0, 50, 0);
        TextMeshProUGUI textComponent = floatingText.GetComponent<TextMeshProUGUI>();

        while (elapsed < duration)
        {
            if (floatingText == null || rectTransform == null || textComponent == null)
            {
                Debug.LogWarning("FloatingText destroyed or missing components during animation!");
                yield break;
            }

            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            rectTransform.localPosition = Vector3.Lerp(startPos, endPos, t);
            textComponent.alpha = 1 - t;
            yield return null;
        }

        if (floatingText != null)
        {
            Destroy(floatingText);
            Debug.Log("FloatingText destroyed after animation.");
        }
    }

    public void StopAllFloatingTextCoroutines()
    {
        foreach (Coroutine coroutine in activeFloatingTextCoroutines)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
        }
        activeFloatingTextCoroutines.Clear();
        Debug.Log("All floating text coroutines stopped.");
    }

    public void ResetScoreAndDistance()
    {
        currentScore = 0;
        currentDistance = 0f;
        currentSpeed = 0f;
        lastDistance = 0f;
        StopAllFloatingTextCoroutines();
        UpdateUI();
    }

    public void ReachFinish(int score, float distance)
    {
        StopAllFloatingTextCoroutines();
        UpdateLevelData(currentLevel, score, distance, true);
    }

    public void GameOver(int score, float distance)
    {
        StopAllFloatingTextCoroutines();
        UpdateLevelData(currentLevel, score, distance, false);

        GameOverManager gameOverManager = FindAnyObjectByType<GameOverManager>();
        if (gameOverManager != null)
        {
            gameOverManager.ShowGameOver(currentScore, currentDistance);
        }
        else
        {
            Debug.LogError("GameOverManager not found!");
        }
    }

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

    public bool IsLevelUnlocked(int level)
    {
        if (level == 1) return true;
        LevelScore previousLevelScore = scoreData.scores.Find(s => s.level == level - 1);
        return previousLevelScore != null && previousLevelScore.isFinished;
    }

    public LevelScore GetLevelData(int level)
    {
        return scoreData.scores.Find(s => s.level == level);
    }

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
                Debug.Log("File JSON không tồn tại, tạo file mới với dữ liệu mặc định.");
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

    private void SaveScoreData()
    {
        string filePath = Path.Combine(Application.persistentDataPath, scoreFileName);

        try
        {
            if (scoreData == null)
            {
                Debug.LogWarning("ScoreData null, khởi tạo dữ liệu mặc định trước khi lưu.");
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