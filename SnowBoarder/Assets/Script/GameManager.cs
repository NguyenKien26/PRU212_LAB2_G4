using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

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
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private TextMeshProUGUI distanceText;
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private TextMeshProUGUI congratText;
    [SerializeField] private GameObject floatingTextPrefab;
    [SerializeField] private float scorePerMeter = 10f;

    private ScoreData scoreData;
    public int currentScore = 0;
    public float currentDistance = 0f;
    public float currentSpeed = 0f;
    public int currentLevel = 1;
    private Canvas canvas;
    private List<Coroutine> activeFloatingTextCoroutines = new List<Coroutine>();
    private float lastDistance = 0f;

    void Awake()
    {
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
            Debug.LogWarning("Canvas not found in scene during Awake! Will try to find in Start.");
        }
        else
        {
            DontDestroyOnLoad(canvas.gameObject);
        }

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
            canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                DontDestroyOnLoad(canvas.gameObject);
                Debug.Log("Canvas found in Start.");
            }
            else
            {
                Debug.LogError("No Canvas found in scene! Please add a Canvas with TextMeshProUGUI components.");
            }
        }

        CheckTextReferences();
        if (congratText != null)
        {
            congratText.gameObject.SetActive(false);
        }

        UpdateUI();
    }

    private void CheckTextReferences()
    {
        if (scoreText == null) Debug.LogError("ScoreText is not assigned!");
        if (highScoreText == null) Debug.LogError("HighScoreText is not assigned!");
        if (distanceText == null) Debug.LogError("DistanceText is not assigned!");
        if (speedText == null) Debug.LogError("SpeedText is not assigned!");
        if (congratText == null) Debug.LogWarning("CongratText is not assigned!");
        if (floatingTextPrefab == null) Debug.LogError("FloatingTextPrefab is not assigned!");
    }

    public void AddScore(int points, Vector3 worldPosition, string message, Color color)
    {
        currentScore += points;
        UpdateLevelData(currentLevel, currentScore, currentDistance, false);
        UpdateUI();
        if (canvas != null && floatingTextPrefab != null)
        {
            ShowFloatingText(worldPosition, message, color);
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
        UpdateUI();
    }

    private void UpdateUI()
    {
        LevelScore levelScore = GetLevelData(currentLevel);
        int highScore = levelScore != null ? levelScore.highestScore : 0;

        if (scoreText != null)
            scoreText.text = $"Score: {currentScore}";
        if (highScoreText != null)
            highScoreText.text = $"High Score: {highScore}";
        if (distanceText != null)
            distanceText.text = $"Distance: {currentDistance:F1} m";
        if (speedText != null)
            speedText.text = $"Speed: {currentSpeed:F1} m/s";
    }

    private void ShowFloatingText(Vector3 worldPosition, string message, Color color)
    {
        if (Camera.main == null || canvas == null) return;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPosition);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.GetComponent<RectTransform>(),
            screenPos,
            canvas.worldCamera,
            out Vector2 localPos
        );

        GameObject floatingText = Instantiate(floatingTextPrefab, canvas.transform);
        RectTransform rectTransform = floatingText.GetComponent<RectTransform>();
        rectTransform.localPosition = localPos;

        TextMeshProUGUI textComponent = floatingText.GetComponent<TextMeshProUGUI>();
        if (textComponent != null)
        {
            textComponent.text = message;
            textComponent.color = color;
            Coroutine coroutine = StartCoroutine(AnimateFloatingText(floatingText));
            activeFloatingTextCoroutines.Add(coroutine);
        }
        else
        {
            Destroy(floatingText);
        }
    }

    private IEnumerator AnimateFloatingText(GameObject floatingText)
    {
        float duration = 1f;
        float elapsed = 0f;
        RectTransform rectTransform = floatingText.GetComponent<RectTransform>();
        Vector3 startPos = rectTransform.localPosition;
        Vector3 endPos = startPos + new Vector3(0, 50, 0);
        TextMeshProUGUI textComponent = floatingText.GetComponent<TextMeshProUGUI>();

        while (elapsed < duration)
        {
            if (floatingText == null || rectTransform == null || textComponent == null) yield break;
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            rectTransform.localPosition = Vector3.Lerp(startPos, endPos, t);
            textComponent.alpha = 1 - t;
            yield return null;
        }

        Destroy(floatingText);
    }

    private void StopAllFloatingTextCoroutines()
    {
        foreach (Coroutine coroutine in activeFloatingTextCoroutines)
        {
            if (coroutine != null) StopCoroutine(coroutine);
        }
        activeFloatingTextCoroutines.Clear();
    }

    public void UpdateDistanceAndScore(float currentX)
    {
        float delta = currentX - lastDistance;
        if (delta >= 1f)
        {
            int addScore = Mathf.FloorToInt(delta * scorePerMeter);
            AddScore(addScore, Vector3.zero, "+" + addScore, Color.yellow);
            lastDistance = currentX;
        }

        UpdateDistance(currentX);
    }

    public void ResetScoreAndDistance()
    {
        currentScore = 0;
        currentDistance = 0f;
        currentSpeed = 0f;
        lastDistance = 0f;
        if (congratText != null) congratText.gameObject.SetActive(false);
        UpdateUI();
    }

    public void ReachFinish(int score, float distance)
    {
        StopAllFloatingTextCoroutines();
        UpdateLevelData(currentLevel, score, distance, true);
        currentLevel++;
        if (currentLevel <= 3)
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

    private IEnumerator HideCongratTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (congratText != null)
        {
            congratText.gameObject.SetActive(false);
        }
    }

    public void GameOver(int score, float distance)
    {
        StopAllFloatingTextCoroutines();
        UpdateLevelData(currentLevel, score, distance, false);
        SceneManager.LoadScene($"Level{currentLevel}");
        ResetLevel();
    }

    public void ResetPlayer()
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            player.ResetPlayerState();
        }
    }

    private void ResetLevel()
    {
        StopAllFloatingTextCoroutines();
        ResetScoreAndDistance();
        ResetPlayer();
    }

    private void UpdateLevelData(int level, int score, float distance, bool finished)
    {
        LevelScore levelScore = scoreData.scores.Find(s => s.level == level);
        if (levelScore == null)
        {
            levelScore = new LevelScore { level = level, highestScore = score, highestDistance = distance, isFinished = finished };
            scoreData.scores.Add(levelScore);
        }
        else
        {
            if (score > levelScore.highestScore) levelScore.highestScore = score;
            if (distance > levelScore.highestDistance) levelScore.highestDistance = distance;
            if (finished) levelScore.isFinished = true;
        }
        SaveScoreData();
    }

    public bool IsLevelUnlocked(int level)
    {
        if (level == 1) return true;
        LevelScore prev = scoreData.scores.Find(s => s.level == level - 1);
        return prev != null && prev.isFinished;
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
                scoreData = JsonUtility.FromJson<ScoreData>(json);
                if (scoreData == null || scoreData.scores == null)
                {
                    scoreData = CreateDefaultScoreData();
                    SaveScoreData();
                }
            }
            else
            {
                scoreData = CreateDefaultScoreData();
                SaveScoreData();
            }
        }
        catch
        {
            scoreData = CreateDefaultScoreData();
            SaveScoreData();
        }
    }

    private void SaveScoreData()
    {
        string filePath = Path.Combine(Application.persistentDataPath, scoreFileName);
        try
        {
            if (scoreData == null) scoreData = CreateDefaultScoreData();
            string json = JsonUtility.ToJson(scoreData, true);
            File.WriteAllText(filePath, json);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Save error: {ex.Message}");
        }
    }
}
