using UnityEngine;

public class CoinSpawner : MonoBehaviour
{
    [Header("Coin Prefab")]
    public GameObject coinPrefab;
    [Header("Matrix Settings")]
    public int minRows = 1;
    public int maxRows = 4;
    public int minCols = 1;
    public int maxCols = 4;
    public float spacing = 1f;

    public void SpawnCoinsAt(Vector2 position)
    {
        // Random số hàng và cột cho matrix coin
        int rows = Random.Range(minRows, maxRows + 1);
        int cols = Random.Range(minCols, maxCols + 1);

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                Vector2 spawnPos = position + new Vector2(col * spacing, row * spacing);
                Instantiate(coinPrefab, spawnPos, Quaternion.identity);
            }
        }
    }
}
