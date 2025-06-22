using System.Collections;
using UnityEngine;

public class CoinSpawnerManager : MonoBehaviour
{
    [Header("Coin Spawner Prefab")]
    public CoinSpawner spawnerPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private float baseSpawnDistance = 2.0f; // Khoảng cách cơ bản trước nhân vật
    [SerializeField] private float spawnRange = 0.3f; // Phạm vi ngẫu nhiên
    [SerializeField] private float speedFactor = 3.0f; // Hệ số dự đoán
    [SerializeField] private float yOffset = 0.5f; // Độ cao bổ sung trên mặt đất
    [SerializeField] private float minSpawnInterval = 0.05f; // Giảm xuống 0.05 giây
    [SerializeField] private float maxSpawnInterval = 0.15f; // Giảm xuống 0.15 giây
    [SerializeField] private int maxSpawners = 5; // Giảm xuống 5 làm mặc định
    [SerializeField] private float maxPredictionDistance = 4.0f; // Giới hạn khoảng cách dự đoán
    [SerializeField] private float maxCoinsPerSecond = 5f; // Giới hạn số coin spawn mỗi giây

    private Transform playerTransform;
    private PlayerController playerController;
    private float lastSpawnX;
    private LayerMask groundLayer;
    private float lastSpawnTime;

    void Start()
    {
        playerTransform = FindFirstObjectByType<PlayerController>()?.transform;
        playerController = FindFirstObjectByType<PlayerController>();
        if (playerTransform == null || playerController == null)
        {
            Debug.LogError("PlayerController not found in scene! Coin spawning disabled.");
            return;
        }
        groundLayer = playerController.groundLayer;
        Debug.Log($"Found Player at position: {playerTransform.position}, groundCheck at: {playerController.groundCheck.position}, groundLayer: {LayerMask.LayerToName(groundLayer)}");

        lastSpawnX = playerTransform.position.x;
        lastSpawnTime = Time.time;
        StartCoroutine(SpawnCoinsRoutine());
    }

    IEnumerator SpawnCoinsRoutine()
    {
        while (true)
        {
            float spawnInterval = Random.Range(minSpawnInterval, maxSpawnInterval);
            yield return new WaitForSeconds(spawnInterval);

            if (playerTransform == null || playerController == null || GameManager.Instance == null)
            {
                Debug.LogError("PlayerTransform, PlayerController, or GameManager lost during runtime!");
                yield break;
            }

            // Kiểm tra giới hạn tần suất
            float timeSinceLastSpawn = Time.time - lastSpawnTime;
            if (timeSinceLastSpawn < 1f / maxCoinsPerSecond)
            {
                Debug.LogWarning($"Skipping spawn due to maxCoinsPerSecond limit: {maxCoinsPerSecond}, Time since last: {timeSinceLastSpawn}");
                yield return null;
                continue;
            }

            // Điều chỉnh maxSpawners dựa trên level
            int levelAdjustedMaxSpawners = maxSpawners + (GameManager.Instance.currentLevel - 1); // Tăng maxSpawners theo level
            if (transform.childCount >= levelAdjustedMaxSpawners)
            {
                Debug.LogWarning($"Max spawners reached: {levelAdjustedMaxSpawners}, skipping spawn");
                yield return null;
                continue;
            }

            // Dự đoán vị trí dựa trên tốc độ hiện tại
            float currentSpeed = GameManager.Instance.currentSpeed;
            float predictedDistance = Mathf.Min(currentSpeed * speedFactor * spawnInterval, maxPredictionDistance);
            float predictedX = playerTransform.position.x + baseSpawnDistance + Random.Range(-spawnRange, spawnRange) + predictedDistance;
            Debug.Log($"Predicted distance: {predictedDistance}, Current Speed: {currentSpeed}, Speed Factor: {speedFactor}, Max Prediction: {maxPredictionDistance}");

            // Sử dụng Raycast để lấy Y từ địa hình
            RaycastHit2D hit = Physics2D.Raycast(new Vector2(predictedX, playerTransform.position.y + 10f), Vector2.down, 20f, groundLayer);
            float spawnY = hit.collider != null ? hit.point.y + yOffset : playerTransform.position.y + yOffset;
            Vector2 spawnPos = new Vector2(predictedX, spawnY);
            Debug.Log($"Spawning coin at predicted position: {spawnPos} relative to Player at {playerTransform.position}, groundCheck at {playerController.groundCheck.position}, Raycast hit: {hit.collider != null}, Hit point: {hit.point}");

            if (spawnerPrefab != null)
            {
                CoinSpawner spawner = Instantiate(spawnerPrefab, transform);
                spawner.SpawnCoinsAt(spawnPos);
                Debug.Log($"Spawned CoinSpawner at {spawnPos}, Player position: {playerTransform.position}");
                lastSpawnTime = Time.time; // Cập nhật thời gian spawn
            }
            else
            {
                Debug.LogError("spawnerPrefab is not assigned in CoinSpawnerManager!");
            }

            lastSpawnX = predictedX;

            if (transform.childCount > levelAdjustedMaxSpawners)
            {
                Destroy(transform.GetChild(0).gameObject);
                Debug.Log("Removed oldest CoinSpawner to limit count.");
            }
        }
    }
}