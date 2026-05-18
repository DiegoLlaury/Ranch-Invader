using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

/// <summary>
/// Panneau audio — contrôle les quatre sliders de volume via l'AudioMixer exposé.
/// Les valeurs du mixer sont appliquées une seule fois au démarrage via AudioVolumeInitializer,
/// puis mises à jour uniquement quand un slider change. Cela évite de baisser la musique
/// à chaque ouverture du panneau.
/// </summary>
public class AudioOptionsPanel : MonoBehaviour
{
    private const float MinDb = -80f;
    private const float MaxDb = 0f;

    public const string KeyMaster = "MasterVolume_Pref";
    public const string KeyMusic  = "MusicVolume_Pref";
    public const string KeyVoice  = "VoiceVolume_Pref";
    public const string KeySFX    = "SFXVolume_Pref";

    [Header("Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("Sliders")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider voiceSlider;
    [SerializeField] private Slider sfxSlider;

    // Noms des paramètres exposés dans MixerAudio
    public const string ParamMaster = "MainVolume";
    public const string ParamMusic  = "MusicVolume";
    public const string ParamVoice  = "VoiceVolume";
    public const string ParamSFX    = "SFXVolume";

    private void OnEnable()
    {
        // Synchronise uniquement les sliders avec les valeurs sauvegardées.
        // On N'applique PAS au mixer ici : le mixer est déjà initialisé au démarrage
        // par AudioVolumeInitializer, et le modifier à chaque OnEnable causait un
        // abaissement permanent du volume de la musique.
        masterSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat(KeyMaster, 1f));
        musicSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat(KeyMusic, 1f));
        voiceSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat(KeyVoice, 1f));
        sfxSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat(KeySFX, 1f));

        masterSlider.onValueChanged.AddListener(v => SetVolume(ParamMaster, KeyMaster, v));
        musicSlider.onValueChanged.AddListener(v => SetVolume(ParamMusic,  KeyMusic,  v));
        voiceSlider.onValueChanged.AddListener(v => SetVolume(ParamVoice,  KeyVoice,  v));
        sfxSlider.onValueChanged.AddListener(v => SetVolume(ParamSFX,    KeySFX,    v));
    }

    private void OnDisable()
    {
        masterSlider.onValueChanged.RemoveAllListeners();
        musicSlider.onValueChanged.RemoveAllListeners();
        voiceSlider.onValueChanged.RemoveAllListeners();
        sfxSlider.onValueChanged.RemoveAllListeners();
    }

    private void SetVolume(string mixerParam, string prefKey, float normalizedValue)
    {
        ApplyToMixer(mixerParam, normalizedValue);
        PlayerPrefs.SetFloat(prefKey, normalizedValue);
    }

    /// <summary>Convertit [0,1] en décibels de manière logarithmique et applique au mixer.</summary>
    public void ApplyToMixer(string param, float normalized)
    {
        float db = normalized > 0.0001f
            ? Mathf.Lerp(MinDb, MaxDb, Mathf.Log10(1f + normalized * 9f) / Mathf.Log10(10f))
            : MinDb;
        audioMixer.SetFloat(param, db);
    }
}
