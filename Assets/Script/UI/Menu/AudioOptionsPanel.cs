using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

/// <summary>
/// Panneau audio — contrôle les quatre sliders de volume via l'AudioMixer exposé.
/// </summary>
public class AudioOptionsPanel : MonoBehaviour
{
    private const float MinDb = -80f;
    private const float MaxDb = 0f;
    private const string KeyMaster = "MasterVolume_Pref";
    private const string KeyMusic = "MusicVolume_Pref";
    private const string KeyVoice = "VoiceVolume_Pref";
    private const string KeySFX = "SFXVolume_Pref";

    [Header("Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("Sliders")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider voiceSlider;
    [SerializeField] private Slider sfxSlider;

    // Noms des paramètres exposés dans MixerAudio
    private const string ParamMaster = "MainVolume";   // déjà exposé
    private const string ParamMusic = "MusicVolume";  // à exposer
    private const string ParamVoice = "VoiceVolume";  // à exposer
    private const string ParamSFX = "SFXVolume";    // à exposer

    private void OnEnable()
    {
        LoadAndApply(masterSlider, KeyMaster, ParamMaster);
        LoadAndApply(musicSlider, KeyMusic, ParamMusic);
        LoadAndApply(voiceSlider, KeyVoice, ParamVoice);
        LoadAndApply(sfxSlider, KeySFX, ParamSFX);

        masterSlider.onValueChanged.AddListener(v => SetVolume(ParamMaster, KeyMaster, v));
        musicSlider.onValueChanged.AddListener(v => SetVolume(ParamMusic, KeyMusic, v));
        voiceSlider.onValueChanged.AddListener(v => SetVolume(ParamVoice, KeyVoice, v));
        sfxSlider.onValueChanged.AddListener(v => SetVolume(ParamSFX, KeySFX, v));
    }

    private void OnDisable()
    {
        masterSlider.onValueChanged.RemoveAllListeners();
        musicSlider.onValueChanged.RemoveAllListeners();
        voiceSlider.onValueChanged.RemoveAllListeners();
        sfxSlider.onValueChanged.RemoveAllListeners();
    }

    private void LoadAndApply(Slider slider, string prefKey, string mixerParam)
    {
        float normalized = PlayerPrefs.GetFloat(prefKey, 1f);
        slider.SetValueWithoutNotify(normalized);
        ApplyToMixer(mixerParam, normalized);
    }

    private void SetVolume(string mixerParam, string prefKey, float normalizedValue)
    {
        ApplyToMixer(mixerParam, normalizedValue);
        PlayerPrefs.SetFloat(prefKey, normalizedValue);
    }

    // Convertit [0,1] en décibels de manière logarithmique
    private void ApplyToMixer(string param, float normalized)
    {
        float db = normalized > 0.0001f
            ? Mathf.Lerp(MinDb, MaxDb, Mathf.Log10(1f + normalized * 9f) / Mathf.Log10(10f))
            : MinDb;
        audioMixer.SetFloat(param, db);
    }
}
