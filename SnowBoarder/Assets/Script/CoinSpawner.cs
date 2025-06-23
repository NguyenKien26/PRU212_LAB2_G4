using UnityEngine;

public class CoinSpawner : MonoBehaviour
{
    [Header("Coin Prefab")]
    [Header("Matrix Settings")]
    public GameObject coinPrefab;
    public int minRows = 1;
    public int maxRows = 4;
    public int minCols = 1;
    public int maxCols = 4;
    public float spacing = 1f;

    public void SpawnCoinsAt(Vector2 positions)
    {
        int rows = Random.Range(minRows, maxRows + 1);
        int cols = Random.Range(minCols, maxCols + 1);

        for (int row = 0; row < rows; row++)
        {
            for (int coll = 0; coll < cols; coll++)
            {
                Vector2 spawnPos = positions + new Vector2(coll * spacing, row * spacing);
                Instantiate(coinPrefab, spawnPos, Quaternion.identity);
            }
        }
    }
}
