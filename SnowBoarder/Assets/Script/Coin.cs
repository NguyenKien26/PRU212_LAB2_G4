using UnityEngine;

public class Coin : MonoBehaviour
{
    [SerializeField] AudioClip collectSFX;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (collectSFX != null)
            {
                AudioSource.PlayClipAtPoint(collectSFX, Camera.main.transform.position);
            }

            Destroy(gameObject);
        }
    }
}
