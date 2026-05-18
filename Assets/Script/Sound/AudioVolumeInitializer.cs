using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Applique les volumes sauvegardés dans PlayerPrefs à l'AudioMixer au démarrage.
/// Doit être placé sur un GameObject persistant (ex. SoundManager) et s'exécuter avant
/// les autres composants audio (DefaultExecutionOrder négatif).
/// </summary>
[DefaultExecutionOrder(-20)]
public class AudioVolumeInitializer : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;

    private void Awake()
    {
        ApplyAll();
    }

    /// <summary>Relit les PlayerPrefs et réapplique tous les paramètres de volume au mixer.</summary>
    public void ApplyAll()
    {
        ApplyToMixer(AudioOptionsPanel.ParamMaster, PlayerPrefs.GetFloat(AudioOptionsPanel.KeyMaster, 1f));
        ApplyToMixer(AudioOptionsPanel.ParamMusic,  PlayerPrefs.GetFloat(AudioOptionsPanel.KeyMusic,  1f));
        ApplyToMixer(AudioOptionsPanel.ParamVoice,  PlayerPrefs.GetFloat(AudioOptionsPanel.KeyVoice,  1f));
        ApplyToMixer(AudioOptionsPanel.ParamSFX,    PlayerPrefs.GetFloat(AudioOptionsPanel.KeySFX,    1f));
    }

    private void ApplyToMixer(string param, float normalized)
    {
        const float minDb = -80f;
        const float maxDb = 0f;

        float db = normalized > 0.0001f
            ? Mathf.Lerp(minDb, maxDb, Mathf.Log10(1f + normalized * 9f) / Mathf.Log10(10f))
            : minDb;

        audioMixer.SetFloat(param, db);
    }
}
