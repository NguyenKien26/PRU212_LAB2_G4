using UnityEngine;

public class CoinSpawner : MonoBehaviour
{
    [Header("Coin Prefab")]
    public GameObject coinPrefab;

    [Header("Matrix Settings")]
    public int minRows = 1;
    public int maxRows = 1; // Giới hạn 1 hàng
    public int minCols = 1;
    public int maxCols = 1; // Giới hạn 1 cột
    public float spacing = 1f;

    public void SpawnCoinsAt(Vector2 position)
    {
        if (coinPrefab == null)
        {
            Debug.LogError("coinPrefab is not assigned in CoinSpawner!");
            return;
        }

        int rows = Random.Range(minRows, maxRows + 1);
        int cols = Random.Range(minCols, maxCols + 1);
        Debug.Log($"Spawning {rows}x{cols} coin matrix at {position}");

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                Vector2 spawnPos = position + new Vector2(col * spacing, row * spacing);
                GameObject coin = Instantiate(coinPrefab, spawnPos, Quaternion.identity);
                if (coin != null)
                {
                    coin.SetActive(true); // Đảm bảo coin được enable
                    SpriteRenderer sr = coin.GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        sr.enabled = true; // Đảm bảo SpriteRenderer được bật
                        Debug.Log($"Coin spawned at {spawnPos}, active state: {coin.activeSelf}, sprite enabled: {sr.enabled}");
                    }
                    else
                    {
                        Debug.LogError($"Coin at {spawnPos} missing SpriteRenderer!");
                    }
                }
                else
                {
                    Debug.LogError("Failed to instantiate coin at {spawnPos}!");
                }
            }
        }
    }
}