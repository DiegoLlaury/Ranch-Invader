using UnityEngine;

[System.Serializable]
public class SoundData
{
    [Header("Sound Identity")]
    public string soundName;

    [Header("Audio Clips")]
    [Tooltip("Ajoutez plusieurs clips pour des variants aléatoires")]
    public AudioClip[] clips;

    [Header("Settings")]
    [Range(0f, 1f)]
    public float volume = 1f;

    [Range(0.1f, 3f)]
    public float pitch = 1f;

    [Range(0f, 0.5f)]
    [Tooltip("Variation aléatoire du pitch")]
    public float pitchVariation = 0.1f;

    [Header("3D Sound Settings")]
    public bool is3D = true;

    [Range(0f, 100f)]
    public float minDistance = 1f;

    [Range(0f, 500f)]
    public float maxDistance = 50f;

    [Header("Behavior")]
    public bool loop = false;

    [Range(0f, 10f)]
    [Tooltip("Temps minimum entre deux lectures du même son")]
    public float cooldown = 0.1f;

    public AudioClip GetRandomClip()
    {
        if (clips == null || clips.Length == 0)
        {
            Debug.LogWarning($"Sound '{soundName}' has no audio clips assigned!");
            return null;
        }

        return clips[Random.Range(0, clips.Length)];
    }

    public float GetRandomPitch()
    {
        return pitch + Random.Range(-pitchVariation, pitchVariation);
    }
}
