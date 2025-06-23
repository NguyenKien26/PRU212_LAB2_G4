using UnityEngine;

public class Coin : MonoBehaviour
{
    [SerializeField] private int coinScore = 50; // Điểm số khi ăn coin

    void Start()
    {
        Debug.Log($"Coin spawned at position: {transform.position}, Active: {gameObject.activeSelf}, Layer: {LayerMask.LayerToName(gameObject.layer)}, Tag: {gameObject.tag}");
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null)
        {
            Debug.LogError("Coin has no SpriteRenderer or sprite assigned!");
        }
        else
        {
            Debug.Log($"Coin sprite active: {sr.enabled}, color: {sr.color}");
        }

        // Tìm tất cả các coin khác và bỏ qua va chạm
        Coin[] allCoins = FindObjectsByType<Coin>(FindObjectsSortMode.None);
        foreach (Coin otherCoin in allCoins)
        {
            if (otherCoin != this)
            {
                Collider2D thisCollider = GetComponent<Collider2D>();
                Collider2D otherCollider = otherCoin.GetComponent<Collider2D>();
                if (thisCollider != null && otherCollider != null && !Physics2D.GetIgnoreCollision(thisCollider, otherCollider))
                {
                    Physics2D.IgnoreCollision(thisCollider, otherCollider, true);
                    Debug.Log($"Ignored collision between this Coin at {transform.position} and other Coin at {otherCoin.transform.position}");
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"Coin triggered by: {other.gameObject.name}, Tag: {other.tag}, Layer: {LayerMask.LayerToName(other.gameObject.layer)}, Coin Position: {transform.position}, Active: {gameObject.activeSelf}");
        if (other.CompareTag("Player"))
        {
            if (GameManager.Instance != null)
            {
                Transform head = other.transform.Find("Head");
                Vector3 position = head != null ? head.position : other.transform.position + Vector3.up * 0.7f;
                GameManager.Instance.AddScore(coinScore, position, $"+{coinScore}", Color.yellow);
                Debug.Log($"Coin collected by Player! Added {coinScore} points at position {position}");

                // Ẩn coin thay vì destroy, và thông báo cho CoinSpawnerManager
                gameObject.SetActive(false);
                CoinSpawnerManager manager = FindObjectOfType<CoinSpawnerManager>();
                if (manager != null)
                {
                    manager.OnCoinCollected();
                }
                else
                {
                    Debug.LogError("CoinSpawnerManager not found!");
                }
            }
            else
            {
                Debug.LogError("GameManager.Instance is null in Coin!");
            }
        }
        else
        {
            Debug.LogWarning($"Coin triggered by non-Player object: {other.gameObject.name}, ignoring. Coin Position: {transform.position}");
        }
    }
}