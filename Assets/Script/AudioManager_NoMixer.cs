using UnityEngine;
using UnityEngine.UI;

public class AudioManager_NoMixer : MonoBehaviour
{
    [Header("UI Sliders")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("AudioSources")]
    [SerializeField] private AudioSource[] musicSources;
    [SerializeField] private AudioSource[] sfxSources;

    private const string MUSIC_KEY = "musicVolume";
    private const string SFX_KEY = "sfxVolume";

    private void Start()
    {
        LoadVolumes();
    }

    public void OnMusicSliderChanged()
    {
        float volume = musicSlider.value;

        foreach (var source in musicSources)
            source.volume = volume;

        PlayerPrefs.SetFloat(MUSIC_KEY, volume);
    }

    public void OnSFXSliderChanged()
    {
        float volume = sfxSlider.value;

        foreach (var source in sfxSources)
            source.volume = volume;

        PlayerPrefs.SetFloat(SFX_KEY, volume);
    }

    private void LoadVolumes()
    {
        float musicVol = PlayerPrefs.GetFloat(MUSIC_KEY, 1f);
        float sfxVol = PlayerPrefs.GetFloat(SFX_KEY, 1f);

        musicSlider.value = musicVol;
        sfxSlider.value = sfxVol;

        foreach (var source in musicSources)
            source.volume = musicVol;

        foreach (var source in sfxSources)
            source.volume = sfxVol;
    }
}
