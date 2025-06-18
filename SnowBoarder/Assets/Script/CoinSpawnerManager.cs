using UnityEngine;

public class CoinSpawnerManager : MonoBehaviour
{
    [Header("Manage Coin")]
    public CoinSpawner spawnerPrefab;
    public Vector2[] spawnPositions;

    void Start()
    {
        foreach (Vector2 pos in spawnPositions)
        {
            CoinSpawner spawner = Instantiate(spawnerPrefab, transform);
            spawner.SpawnCoinsAt(pos);
        }
    }
}
