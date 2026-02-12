using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "SoundDatabase", menuName = "Audio/Sound Database")]
public class SoundDatabase : ScriptableObject
{
    [Header("All Game Sounds")]
    public List<SoundData> sounds = new List<SoundData>();

    private Dictionary<string, SoundData> soundDictionary;

    public void Initialize()
    {
        soundDictionary = new Dictionary<string, SoundData>();

        foreach (SoundData sound in sounds)
        {
            if (!string.IsNullOrEmpty(sound.soundName))
            {
                if (!soundDictionary.ContainsKey(sound.soundName))
                {
                    soundDictionary.Add(sound.soundName, sound);
                }
                else
                {
                    Debug.LogWarning($"Duplicate sound name detected: {sound.soundName}");
                }
            }
        }
    }

    public SoundData GetSound(string soundName)
    {
        if (soundDictionary == null)
        {
            Initialize();
        }

        if (soundDictionary.TryGetValue(soundName, out SoundData sound))
        {
            return sound;
        }

        Debug.LogWarning($"Sound '{soundName}' not found in database!");
        return null;
    }
}
