using UnityEngine;
using TMPro;

public class LeaderboardManager : MonoBehaviour
{
    public TextMeshProUGUI scoreText;

    public void LoadLeaderboard()
    {
        // Lấy dữ liệu từ PlayerPrefs
        int highestScore = PlayerPrefs.GetInt("HighestScore", 0);
        int lastScore = PlayerPrefs.GetInt("LastScore", 0);

        // Hiển thị điểm số
        scoreText.text = $" Highest Score: {highestScore}\n" +
                         $" Last Score: {lastScore}";
    }

    // Gọi hàm này ở cuối ván chơi để cập nhật điểm
    public void SaveScore(int newScore)
    {
        PlayerPrefs.SetInt("LastScore", newScore);

        int highestScore = PlayerPrefs.GetInt("HighestScore", 0);
        if (newScore > highestScore)
        {
            PlayerPrefs.SetInt("HighestScore", newScore);
        }

        PlayerPrefs.Save();
    }
}
