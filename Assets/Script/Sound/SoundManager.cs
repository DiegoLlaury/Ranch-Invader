using UnityEngine;
using UnityEngine.Audio;
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
                    Debug.LogError("[SoundManager] Not found in scene!");
            }
            return instance;
        }
    }

    [Header("Configuration")]
    public SoundDatabase soundDatabase;

    [Header("Audio Mixer")]
    [Tooltip("Groupe 'SFX' du MixerAudio — tous les sons 3D y seront routés.")]
    public AudioMixerGroup sfxMixerGroup;

    [Tooltip("Groupe 'Music' du MixerAudio — pour la musique de fond.")]
    public AudioMixerGroup musicMixerGroup;

    [Header("Audio Source Pool")]
    [SerializeField] private int poolSize = 20;
    private Queue<AudioSource> audioSourcePool;
    private List<AudioSource> activeAudioSources;

    [Header("Volume Controls")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 1f;

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
        if (isInitialized) return;

        if (soundDatabase == null)
        {
            Debug.LogError("[SoundManager] SoundDatabase not assigned!");
            return;
        }

        soundDatabase.Initialize();

        audioSourcePool = new Queue<AudioSource>();
        activeAudioSources = new List<AudioSource>();

        for (int i = 0; i < poolSize; i++)
            CreateNewAudioSource();

        soundCooldowns.Clear();
        isInitialized = true;
    }

    private AudioSource CreateNewAudioSource()
    {
        GameObject audioObject = new GameObject($"AudioSource_{audioSourcePool.Count}");
        audioObject.transform.SetParent(transform);

        AudioSource audioSource = audioObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.minDistance = 1f;
        audioSource.maxDistance = 50f;
        audioSource.dopplerLevel = 0f;
        audioSource.spread = 0f;
        audioSource.priority = 128;

        // Routed vers le groupe SFX par défaut
        audioSource.outputAudioMixerGroup = sfxMixerGroup;

        audioSourcePool.Enqueue(audioSource);
        return audioSource;
    }

    private AudioSource GetAudioSource()
    {
        if (!isInitialized) Initialize();

        if (audioSourcePool.Count == 0)
        {
            Debug.LogWarning("[SoundManager] Pool exhausted, creating new AudioSource.");
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
        source.outputAudioMixerGroup = sfxMixerGroup; // reset au groupe par défaut

        activeAudioSources.Remove(source);
        audioSourcePool.Enqueue(source);
    }

    private void Update()
    {
        for (int i = activeAudioSources.Count - 1; i >= 0; i--)
        {
            AudioSource source = activeAudioSources[i];
            if (source != null && !source.isPlaying && !source.loop)
                ReturnAudioSource(source);
        }
    }

    // ── Play API ──────────────────────────────────────────────────────────────

    public AudioSource PlaySound(string soundName, Vector3 position)
    {
        if (!isInitialized) Initialize();
        if (soundDatabase == null) return null;

        SoundData sound = soundDatabase.GetSound(soundName);
        if (sound == null) return null;

        if (!CanPlaySound(soundName, sound.cooldown)) return null;

        AudioClip clip = sound.GetRandomClip();
        if (clip == null) return null;

        AudioSource audioSource = GetAudioSource();

        audioSource.transform.position = position;
        audioSource.clip = clip;
        audioSource.volume = sound.volume * sfxVolume * masterVolume;
        audioSource.pitch = sound.GetRandomPitch();
        audioSource.loop = sound.loop;

        // Permet à SoundData de forcer un groupe spécifique (Music, etc.)
        audioSource.outputAudioMixerGroup = sfxMixerGroup;

        if (sound.is3D)
        {
            audioSource.spatialBlend = 1f;
            audioSource.minDistance = Mathf.Max(sound.minDistance, 0.1f);
            audioSource.maxDistance = Mathf.Max(sound.maxDistance, audioSource.minDistance + 1f);
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            audioSource.dopplerLevel = 0f;
            audioSource.spread = 0f;
        }
        else
        {
            audioSource.spatialBlend = 0f;
        }

        audioSource.Play();
        MarkSoundAsPlayed(soundName);
        return audioSource;
    }

    public AudioSource PlaySound2D(string soundName) => PlaySound(soundName, Vector3.zero);
    public AudioSource PlaySoundAtPosition(string soundName, Vector3 position) => PlaySound(soundName, position);

    public AudioSource PlaySoundAtTransform(string soundName, Transform target)
    {
        AudioSource source = PlaySound(soundName, target.position);
        if (source != null && !source.loop)
            StartCoroutine(FollowTransform(source, target));
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

    // ── Cooldown ──────────────────────────────────────────────────────────────

    private bool CanPlaySound(string soundName, float cooldown)
    {
        return !soundCooldowns.TryGetValue(soundName, out float lastTime)
               || Time.time >= lastTime + cooldown;
    }

    private void MarkSoundAsPlayed(string soundName)
    {
        soundCooldowns[soundName] = Time.time;
    }
}
