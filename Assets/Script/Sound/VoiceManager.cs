using UnityEngine;
using UnityEngine.Audio;

/// <summary>Priority levels for voice lines. Higher value = higher priority.</summary>
public enum VoicePriority
{
    Normal = 0,
    Objective = 10
}

/// <summary>
/// Singleton MonoBehaviour that manages exclusive voice playback with priority support.
/// Only one voice can play at a time. Objective-priority voices interrupt any current voice.
/// Uses a dedicated AudioSource separate from the SoundManager pool.
/// </summary>
public class VoiceManager : MonoBehaviour
{
    private static VoiceManager instance;
    public static VoiceManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<VoiceManager>();
                if (instance == null)
                    Debug.LogError("[VoiceManager] Not found in scene!");
            }
            return instance;
        }
    }

    [Header("Volume")]
    [Tooltip("Multiplicateur global appliqué sur le volume de chaque SoundData de voix.")]
    [Range(0f, 1f)] public float voiceVolume = 0.5f;

    [Header("Audio Mixer")]
    [Tooltip("Groupe du mixer auquel router les voix (optionnel).")]
    public AudioMixerGroup voiceMixerGroup;

    private AudioSource voiceAudioSource;
    private VoicePriority _currentPriority = VoicePriority.Normal;

    /// <summary>Returns true if a voice line is currently playing.</summary>
    public bool IsVoicePlaying => voiceAudioSource != null && voiceAudioSource.isPlaying;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        voiceAudioSource = gameObject.AddComponent<AudioSource>();
        voiceAudioSource.playOnAwake = false;
        voiceAudioSource.spatialBlend = 0f;
        voiceAudioSource.loop = false;
        voiceAudioSource.priority = 64;

        if (voiceMixerGroup != null)
            voiceAudioSource.outputAudioMixerGroup = voiceMixerGroup;
    }

    private void Update()
    {
        if (!voiceAudioSource.isPlaying)
            _currentPriority = VoicePriority.Normal;
    }

    /// <summary>
    /// Plays a voice if no higher-or-equal priority voice is currently playing.
    /// Returns false if blocked by a voice already in progress.
    /// </summary>
    public bool PlayVoice(string soundName, VoicePriority priority = VoicePriority.Normal)
    {
        if (IsVoicePlaying && _currentPriority >= priority)
            return false;

        return PlayVoiceInternal(soundName, priority);
    }

    /// <summary>Stops the current voice unconditionally, then plays the new one immediately.</summary>
    public void PlayVoiceForced(string soundName, VoicePriority priority = VoicePriority.Objective)
    {
        StopCurrentVoice(VoicePriority.Normal);
        PlayVoiceInternal(soundName, priority);
    }

    /// <summary>Stops the current voice if its priority is below or equal to minimumPriority.</summary>
    public void StopCurrentVoice(VoicePriority minimumPriority = VoicePriority.Normal)
    {
        if (_currentPriority <= minimumPriority)
        {
            voiceAudioSource.Stop();
            _currentPriority = VoicePriority.Normal;
        }
    }

    private bool PlayVoiceInternal(string soundName, VoicePriority priority)
    {
        if (SoundManager.Instance == null || SoundManager.Instance.soundDatabase == null)
        {
            Debug.LogWarning("[VoiceManager] SoundManager or SoundDatabase not available.");
            return false;
        }

        SoundData data = SoundManager.Instance.soundDatabase.GetSound(soundName);
        if (data == null)
        {
            Debug.LogWarning($"[VoiceManager] Sound '{soundName}' not found in SoundDatabase.");
            return false;
        }

        AudioClip clip = data.GetRandomClip();
        if (clip == null)
        {
            Debug.LogWarning($"[VoiceManager] Sound '{soundName}' has no clips assigned.");
            return false;
        }

        voiceAudioSource.Stop();
        voiceAudioSource.clip = clip;
        voiceAudioSource.volume = data.volume * voiceVolume;
        voiceAudioSource.pitch = data.GetRandomPitch();
        voiceAudioSource.outputAudioMixerGroup = voiceMixerGroup;
        voiceAudioSource.Play();

        _currentPriority = priority;
        return true;
    }
}
