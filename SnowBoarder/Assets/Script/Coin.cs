using UnityEngine;

public class Coin : MonoBehaviour
{
    [SerializeField] AudioClip collectSFX;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Phát âm thanh nếu có
            if (collectSFX != null)
            {
                AudioSource.PlayClipAtPoint(collectSFX, Camera.main.transform.position);
            }

            // Xoá coin
            Destroy(gameObject);
        }
    }
}
