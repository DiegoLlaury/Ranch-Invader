using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Panneau graphismes — qualité, plein écran, résolution.
/// </summary>
public class GraphicsOptionsPanel : MonoBehaviour
{
    private const string PrefQuality = "GraphicsQuality_Pref";
    private const string PrefFullscreen = "Fullscreen_Pref";
    private const string PrefResIndex = "ResolutionIndex_Pref";

    [Header("UI")]
    [SerializeField] private TMP_Dropdown qualityDropdown;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private TMP_Dropdown resolutionDropdown;

    private Resolution[] availableResolutions;

    private void OnEnable()
    {
        SetupQuality();
        SetupFullscreen();
        SetupResolutions();
    }

    private void SetupQuality()
    {
        qualityDropdown.ClearOptions();
        var names = new List<string>(QualitySettings.names);
        qualityDropdown.AddOptions(names);

        int saved = PlayerPrefs.GetInt(PrefQuality, QualitySettings.GetQualityLevel());
        qualityDropdown.SetValueWithoutNotify(saved);

        qualityDropdown.onValueChanged.AddListener(index =>
        {
            QualitySettings.SetQualityLevel(index, true);
            PlayerPrefs.SetInt(PrefQuality, index);
        });
    }

    private void SetupFullscreen()
    {
        bool saved = PlayerPrefs.GetInt(PrefFullscreen, Screen.fullScreen ? 1 : 0) == 1;
        fullscreenToggle.SetIsOnWithoutNotify(saved);
        Screen.fullScreen = saved;

        fullscreenToggle.onValueChanged.AddListener(value =>
        {
            Screen.fullScreen = value;
            PlayerPrefs.SetInt(PrefFullscreen, value ? 1 : 0);
        });
    }

    private void SetupResolutions()
    {
        availableResolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        var options = new List<string>();
        int currentIndex = 0;

        for (int i = 0; i < availableResolutions.Length; i++)
        {
            var r = availableResolutions[i];
            options.Add($"{r.width} x {r.height} @ {Mathf.RoundToInt((float)r.refreshRateRatio.value)}Hz");

            if (r.width == Screen.currentResolution.width &&
                r.height == Screen.currentResolution.height)
                currentIndex = i;
        }

        resolutionDropdown.AddOptions(options);
        int saved = PlayerPrefs.GetInt(PrefResIndex, currentIndex);
        resolutionDropdown.SetValueWithoutNotify(saved);

        resolutionDropdown.onValueChanged.AddListener(index =>
        {
            Screen.SetResolution(
                availableResolutions[index].width,
                availableResolutions[index].height,
                Screen.fullScreen);
            PlayerPrefs.SetInt(PrefResIndex, index);
        });
    }

    private void OnDisable()
    {
        qualityDropdown.onValueChanged.RemoveAllListeners();
        fullscreenToggle.onValueChanged.RemoveAllListeners();
        resolutionDropdown.onValueChanged.RemoveAllListeners();
    }
}
