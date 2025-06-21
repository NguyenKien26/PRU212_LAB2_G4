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
        if (!hasPlayedSound && !hasCrashed && other.CompareTag("Ground"))
        {
            hasPlayedSound = true;
            hasCrashed = true;
            FindFirstObjectByType<PlayerController>().DisablePlayer();
            crashEffect.Play();

            if (crashSFX != null)
            {
                AudioSource.PlayClipAtPoint(crashSFX, Camera.main.transform.position);
            }

            Invoke(nameof(ReloadScene), loadTime);
        }

    }
    void ReloadScene()
    {
        SceneManager.LoadScene(0);
    }
}
