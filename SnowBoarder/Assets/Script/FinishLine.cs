using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FinishLine : MonoBehaviour
{
    [SerializeField] float loadTime = 1f;
    [SerializeField]
    ParticleSystem finishEffect;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.tag == "Player")
        {
            finishEffect.Play();
            GetComponent<AudioSource>().Play();
            Invoke("ReloadScene",loadTime);
        }
    }
    void ReloadScene()
    {
        SceneManager.LoadScene(0);
    }
}
