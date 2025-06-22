using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;

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

    private ScoreData scoreData;
    public int currentScore = 0;
    public float currentDistance = 0f;
    public int currentLevel = 1;

    private float lastDistance = 0f; // <--- Thêm dòng này

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
        }

        LoadScoreData();
    }

    void Start()
    {
        if (string.IsNullOrEmpty(scoreData.playerName))
        {
            scoreData.playerName = SystemInfo.deviceName;
            SaveScoreData();
        }
    }

    public void AddScore(int points)
    {
        currentScore += points;
    }

    public void UpdateDistance(float distance)
    {
        currentDistance = Mathf.Max(currentDistance, distance);
    }

    // ✅ Hàm mới: Tính điểm theo khoảng cách
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

    public void ResetScoreAndDistance()
    {
        currentScore = 0;
        currentDistance = 0f;
        lastDistance = 0f;
    }

    public void ReachFinish(int score, float distance)
    {
        UpdateLevelData(currentLevel, score, distance, true);
    }

    public void GameOver(int score, float distance)
    {
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
