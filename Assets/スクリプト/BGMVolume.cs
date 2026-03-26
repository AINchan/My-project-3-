using UnityEngine;
using UnityEngine.UI;

public class BGMVolume : MonoBehaviour
{
    private const string BGM_VOLUME_KEY = "BGM_VOLUME";

    [SerializeField] private Slider bgmSlider;
    [SerializeField] private AudioSource TitleAudioSource;
    [SerializeField] private AudioSource GameAudioSource;

    private void Start()
    {
        float savedVolume = PlayerPrefs.GetFloat(BGM_VOLUME_KEY, 0.5f);

        if (bgmSlider != null)
        {
            bgmSlider.value = savedVolume;
            bgmSlider.onValueChanged.AddListener(OnChangedVolume);
        }

        ApplyVolumeToAll(savedVolume);
    }

    private void OnChangedVolume(float value)
    {
        ApplyVolumeToAll(value);
        PlayerPrefs.SetFloat(BGM_VOLUME_KEY, value);
        PlayerPrefs.Save();
    }

    private void ApplyVolumeToAll(float value)
    {
        if (TitleAudioSource != null)
            TitleAudioSource.volume = value;

        if (GameAudioSource != null)
            GameAudioSource.volume = value;

        AudioSource[] allSources = FindObjectsOfType<AudioSource>();

        foreach (var src in allSources)
        {
            if (src != null)
                src.volume = value;
        }
    }
}