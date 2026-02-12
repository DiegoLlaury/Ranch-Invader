using UnityEngine;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    private static SoundManager instance;
    public static SoundManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<SoundManager>();

                if (instance == null)
                {
                    Debug.LogError("SoundManager not found in scene! Please add a SoundManager GameObject to your scene.");
                }
            }
            return instance;
        }
    }

    [Header("Configuration")]
    public SoundDatabase soundDatabase;

    [Header("Audio Source Pool")]
    [SerializeField] private int poolSize = 20;
    private Queue<AudioSource> audioSourcePool;
    private List<AudioSource> activeAudioSources;

    [Header("Volume Controls")]
    [Range(0f, 1f)]
    public float masterVolume = 1f;

    [Range(0f, 1f)]
    public float sfxVolume = 1f;

    [Range(0f, 1f)]
    public float musicVolume = 1f;

    private bool isInitialized = false;

    private Dictionary<string, float> soundCooldowns = new Dictionary<string, float>();

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        Initialize();
    }

    private void Initialize()
    {
        if (isInitialized)
            return;

        if (soundDatabase == null)
        {
            Debug.LogError("[SoundManager] SoundDatabase not assigned! Please assign it in the Inspector.");
            return;
        }

        soundDatabase.Initialize();

        audioSourcePool = new Queue<AudioSource>();
        activeAudioSources = new List<AudioSource>();

        for (int i = 0; i < poolSize; i++)
        {
            CreateNewAudioSource();
        }

        soundCooldowns.Clear();

        isInitialized = true;
        Debug.Log($"[SoundManager] Initialized with {poolSize} audio sources");
    }

    private AudioSource CreateNewAudioSource()
    {
        GameObject audioObject = new GameObject($"AudioSource_{audioSourcePool.Count}");
        audioObject.transform.SetParent(transform);
        AudioSource audioSource = audioObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;
        audioSourcePool.Enqueue(audioSource);
        return audioSource;
    }

    private AudioSource GetAudioSource()
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[SoundManager] Not initialized yet!");
            Initialize();
        }

        if (audioSourcePool.Count == 0)
        {
            Debug.LogWarning("[SoundManager] Pool exhausted, creating new AudioSource");
            return CreateNewAudioSource();
        }

        AudioSource source = audioSourcePool.Dequeue();
        activeAudioSources.Add(source);
        return source;
    }

    private void ReturnAudioSource(AudioSource source)
    {
        if (source == null) return;

        source.Stop();
        source.clip = null;
        source.loop = false;

        activeAudioSources.Remove(source);
        audioSourcePool.Enqueue(source);
    }

    private void Update()
    {
        for (int i = activeAudioSources.Count - 1; i >= 0; i--)
        {
            AudioSource source = activeAudioSources[i];

            if (source != null && !source.isPlaying && !source.loop)
            {
                ReturnAudioSource(source);
            }
        }
    }

    private bool CanPlaySound(string soundName, float cooldown)
    {
        if (!soundCooldowns.ContainsKey(soundName))
        {
            return true;
        }

        float lastPlayedTime = soundCooldowns[soundName];
        return Time.time >= lastPlayedTime + cooldown;
    }

    private void MarkSoundAsPlayed(string soundName)
    {
        soundCooldowns[soundName] = Time.time;
    }

    public AudioSource PlaySound(string soundName, Vector3 position)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[SoundManager] Attempting to play sound before initialization!");
            Initialize();
        }

        if (soundDatabase == null)
        {
            Debug.LogError("[SoundManager] SoundDatabase is null!");
            return null;
        }

        SoundData sound = soundDatabase.GetSound(soundName);
        if (sound == null)
        {
            Debug.LogWarning($"[SoundManager] Sound '{soundName}' not found in database!");
            return null;
        }

        if (!CanPlaySound(soundName, sound.cooldown))
        {
            return null;
        }

        AudioClip clip = sound.GetRandomClip();
        if (clip == null)
        {
            Debug.LogWarning($"[SoundManager] Sound '{soundName}' has no clip!");
            return null;
        }

        AudioSource audioSource = GetAudioSource();

        audioSource.transform.position = position;
        audioSource.clip = clip;
        audioSource.volume = sound.volume * sfxVolume * masterVolume;
        audioSource.pitch = sound.GetRandomPitch();
        audioSource.loop = sound.loop;

        if (sound.is3D)
        {
            audioSource.spatialBlend = 1f;
            audioSource.minDistance = sound.minDistance;
            audioSource.maxDistance = sound.maxDistance;
            audioSource.rolloffMode = AudioRolloffMode.Linear;
        }
        else
        {
            audioSource.spatialBlend = 0f;
        }

        audioSource.Play();
        MarkSoundAsPlayed(soundName);

        return audioSource;
    }

    public AudioSource PlaySound2D(string soundName)
    {
        return PlaySound(soundName, Vector3.zero);
    }

    public AudioSource PlaySoundAtPosition(string soundName, Vector3 position)
    {
        return PlaySound(soundName, position);
    }

    public AudioSource PlaySoundAtTransform(string soundName, Transform target)
    {
        AudioSource source = PlaySound(soundName, target.position);

        if (source != null && !source.loop)
        {
            StartCoroutine(FollowTransform(source, target));
        }

        return source;
    }

    private System.Collections.IEnumerator FollowTransform(AudioSource source, Transform target)
    {
        while (source != null && source.isPlaying && target != null)
        {
            source.transform.position = target.position;
            yield return null;
        }
    }

    public void StopSound(AudioSource source)
    {
        if (source != null)
        {
            ReturnAudioSource(source);
        }
    }

    public void StopAllSounds()
    {
        foreach (AudioSource source in activeAudioSources.ToArray())
        {
            ReturnAudioSource(source);
        }
    }

    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
    }
}
