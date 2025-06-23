using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    public AudioSource sfxAudioSource;
    public AudioClip clickClip;


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
            return;
        }
    }

    void Start()
    {
        LoadVolumeSettings();
    }

    public void SetVolume(string parameter, float volume)
    {
        float dB = Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20f;
        audioMixer.SetFloat(parameter, dB);

        PlayerPrefs.SetFloat(parameter, volume);
        PlayerPrefs.Save();
    }

    public void LoadVolumeSettings()
    {
        SetVolume("MasterVolume", PlayerPrefs.GetFloat("MasterVolume", 1f));
        SetVolume("MusicVolume", PlayerPrefs.GetFloat("MusicVolume", 1f));
        SetVolume("SFXVolume", PlayerPrefs.GetFloat("SFXVolume", 1f));
    }
    public void PlayClickSound()
    {
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(Mathf.Clamp(PlayerPrefs.GetFloat("SFXVolume", 1f), 0.0001f, 1f)) * 20f);
        sfxAudioSource.PlayOneShot(clickClip);
    }
}
