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
    public static GameManager Instance; // Singleton

    [SerializeField] private string scoreFileName = "scores.json";
    private ScoreData scoreData;
    public int currentScore = 0;
    public float currentDistance = 0f;
    public int currentLevel = 1; // Mặc định bắt đầu từ level 1

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
        }

        // Tải dữ liệu JSON
        LoadScoreData();
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

    // Thêm điểm (gọi từ PlayerController khi lộn nhào)
    public void AddScore(int points)
    {
        currentScore += points;
        //Debug.Log($"Current Score: {currentScore}");
    }

    // Cập nhật khoảng cách hiện tại
    public void UpdateDistance(float distance)
    {
        currentDistance = Mathf.Max(currentDistance, distance);
        //Debug.Log($"Current Distance: {currentDistance}");
    }

    // Xử lý khi cán đích
    public void ReachFinish(int score, float distance)
    {
        // Cập nhật dữ liệu level với isFinished = true
        UpdateLevelData(currentLevel, score, distance, true);

        // Chuyển sang level tiếp theo
        currentLevel++;
        if (currentLevel <= 3) // Giả sử có 3 level
        {
            SceneManager.LoadScene($"Level{currentLevel}");
            currentScore = 0; // Reset điểm
            currentDistance = 0f; // Reset khoảng cách
        }
        else
        {
            Debug.Log("Game Completed! Back to LevelSelection.");
            SceneManager.LoadScene("LevelSelection");
        }
    }

    // Xử lý khi game over
    public void GameOver(int score, float distance)
    {
        // Cập nhật dữ liệu level với isFinished = false
        UpdateLevelData(currentLevel, score, distance, false);

        // Reload level hiện tại
        SceneManager.LoadScene($"Level{currentLevel}");
        currentScore = 0; // Reset điểm
        currentDistance = 0f; // Reset khoảng cách
    }

    // Cập nhật điểm cao nhất, khoảng cách max, và trạng thái hoàn thành
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
                levelScore.isFinished = true; // Chỉ đặt true nếu cán đích
        }
        SaveScoreData();
    }

    // Kiểm tra xem level có được mở khóa không
    public bool IsLevelUnlocked(int level)
    {
        if (level == 1) return true; // Level 1 luôn mở
        LevelScore previousLevelScore = scoreData.scores.Find(s => s.level == level - 1);
        return previousLevelScore != null && previousLevelScore.isFinished;
    }

    // Lấy dữ liệu level
    public LevelScore GetLevelData(int level)
    {
        return scoreData.scores.Find(s => s.level == level);
    }

    // Khởi tạo dữ liệu mặc định nếu file không tồn tại hoặc bị lỗi
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

    // Đọc dữ liệu JSON từ file
    private void LoadScoreData()
    {
        string filePath = Path.Combine(Application.persistentDataPath, scoreFileName);

        try
        {
            // Kiểm tra xem file có tồn tại không
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                if (!string.IsNullOrEmpty(json))
                {
                    scoreData = JsonUtility.FromJson<ScoreData>(json);
                    // Kiểm tra xem dữ liệu có hợp lệ không
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

    // Lưu dữ liệu JSON vào file
    private void SaveScoreData()
    {
        string filePath = Path.Combine(Application.persistentDataPath, scoreFileName);

        try
        {
            // Đảm bảo scoreData không null
            if (scoreData == null)
            {
                Debug.LogWarning("ScoreData null, khởi tạo dữ liệu mặc định trước khi lưu.");
                scoreData = CreateDefaultScoreData();
            }

            // Chuyển dữ liệu thành JSON
            string json = JsonUtility.ToJson(scoreData, true);

            // Ghi file
            File.WriteAllText(filePath, json);
            Debug.Log($"Lưu dữ liệu thành công tại: {filePath}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Lỗi khi lưu file JSON: {ex.Message}");
        }
    }
}