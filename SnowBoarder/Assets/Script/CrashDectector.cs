using UnityEngine;
using UnityEngine.SceneManagement;

public class CrashDectector : MonoBehaviour
{
    [SerializeField] float loadTime = 0.1f;
    [SerializeField] ParticleSystem crashEffect;
    [SerializeField] AudioClip crashSFX;
    bool hasPlayedSound = false;
    bool hasCrashed = false;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!hasCrashed && (other.CompareTag("Ground") || other.CompareTag("Rock")))
        {
            hasCrashed = true;
            FindFirstObjectByType<PlayerController>().DisablePlayer();

            if (crashEffect != null)
            {
                crashEffect.Play();
            }

            if (!hasPlayedSound && crashSFX != null)
            {
                hasPlayedSound = true;
                AudioSource.PlayClipAtPoint(crashSFX, Camera.main.transform.position);
            }

            // Dừng game ngay lập tức
            Time.timeScale = 0f;
        }
    }

    void ReloadScene()
    {
        Time.timeScale = 1f; // Khôi phục tốc độ thời gian
        SceneManager.LoadScene(0);
    }

}
