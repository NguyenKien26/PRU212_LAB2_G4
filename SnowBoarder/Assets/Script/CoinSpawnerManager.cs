using UnityEngine;

public class CoinSpawnerManager : MonoBehaviour
{
    [Header("Coin Spawner Prefab")]
    public CoinSpawner spawnerPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private float triggerZoneDistance = 30.0f; // Tăng để giảm mật độ
    [SerializeField] private float baseSpawnDistance = 10.0f; 
    [SerializeField] private float spawnRange = 5.0f; // Phạm vi ngẫu nhiên
    [SerializeField] private float yOffset = 0.2f; // Độ cao hợp lý
    [SerializeField] private int maxSpawners = 5;
    [SerializeField] private int maxCoinsPerZone = 1; // Giới hạn 1 coin mỗi vùng
    [SerializeField] private int maxCoinsOnScreen = 15;

    private Transform playerTransform;
    private PlayerController playerController;
    private LayerMask groundLayer;
    private float lastTriggerX;
    private int coinCount = 0;

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

        lastTriggerX = playerTransform.position.x; // Bắt đầu từ vị trí hiện tại
    }

    void Update()
    {
        if (playerTransform == null || playerController == null || GameManager.Instance == null)
        {
            Debug.LogError("Missing critical components!");
            return;
        }

        float currentX = playerTransform.position.x;
        float distanceTraveled = currentX - lastTriggerX;

        // Spawn coin khi người chơi vào vùng kích hoạt mới
        if (distanceTraveled >= triggerZoneDistance && coinCount < maxCoinsOnScreen)
        {
            // Thêm xác suất ngẫu nhiên để giảm số coin
            if (Random.value > 0.7f) // 30% cơ hội spawn
            {
                float spawnX = currentX + baseSpawnDistance + Random.Range(spawnRange, spawnRange + 3.0f); // Tăng khoảng cách
                Vector2 groundCheckPos = new Vector2(spawnX, playerController.groundCheck.position.y);
                RaycastHit2D hit = Physics2D.Raycast(groundCheckPos, Vector2.down, 10f, groundLayer);
                float spawnY = hit.collider != null ? hit.point.y + yOffset : playerController.groundCheck.position.y + yOffset;
                Vector2 spawnPos = new Vector2(spawnX, spawnY);
                Debug.Log($"Spawning coin at {spawnPos}, Raycast hit: {hit.collider != null}, Ground Check Y: {playerController.groundCheck.position.y}, Player X: {currentX}");

                if (spawnerPrefab != null && coinCount < maxCoinsPerZone * (maxSpawners + GameManager.Instance.currentLevel - 1))
                {
                    CoinSpawner spawner = Instantiate(spawnerPrefab, transform);
                    spawner.SpawnCoinsAt(spawnPos);
                    Debug.Log($"Spawned CoinSpawner at {spawnPos}, Spawner active: {spawner.gameObject.activeSelf}, Coin count: {coinCount + 1}");
                    coinCount++;
                    lastTriggerX = currentX; // Cập nhật vị trí vùng kích hoạt
                }
                else
                {
                    Debug.LogWarning($"Max coins or spawners reached, skipping spawn. Coin count: {coinCount}");
                }
            }
            else
            {
                Debug.Log($"Skipped spawn due to random chance at Player X: {currentX}");
            }
        }
    }

    // Cập nhật coinCount khi coin bị thu thập (gọi từ Coin)
    public void OnCoinCollected()
    {
        if (coinCount > 0)
        {
            coinCount--;
            Debug.Log($"Coin collected, updated coin count: {coinCount}");
        }
    }
}