using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioMixer audioMixer;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {

        float masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);

        // Gán vào AudioMixer
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(Mathf.Clamp(masterVolume, 0.0001f, 1f)) * 20);
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(Mathf.Clamp(musicVolume, 0.0001f, 1f)) * 20);
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(Mathf.Clamp(sfxVolume, 0.0001f, 1f)) * 20);
    }
}
