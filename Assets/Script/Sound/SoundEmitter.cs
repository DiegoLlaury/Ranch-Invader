using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Reusable component that maps named sound events to SoundDatabase keys.
/// Attach to any GameObject (enemy, player, projectile) and configure
/// the sound bindings in the Inspector without touching code.
/// </summary>
public class SoundEmitter : MonoBehaviour
{
    [Serializable]
    public struct SoundBinding
    {
        [Tooltip("Event name used in code, e.g. 'OnAttack', 'OnDeath', 'OnHit'.")]
        public string eventName;

        [Tooltip("Matching key in the SoundDatabase.")]
        public string soundKey;
    }

    [Header("Sound Bindings")]
    [Tooltip("Map each gameplay event name to a key in the SoundDatabase.")]
    public List<SoundBinding> bindings = new List<SoundBinding>();

    private Dictionary<string, string> bindingMap;

    private void Awake()
    {
        BuildMap();
    }

    private void BuildMap()
    {
        bindingMap = new Dictionary<string, string>(bindings.Count);

        foreach (SoundBinding binding in bindings)
        {
            if (!string.IsNullOrEmpty(binding.eventName) && !string.IsNullOrEmpty(binding.soundKey))
            {
                bindingMap[binding.eventName] = binding.soundKey;
            }
        }
    }

    /// <summary>
    /// Plays the sound bound to the given event name at this transform's position.
    /// </summary>
    public void Play(string eventName)
    {
        if (!TryGetKey(eventName, out string key)) return;
        SoundManager.Instance?.PlaySoundAtPosition(key, transform.position);
    }

    /// <summary>
    /// Plays the sound bound to the given event name at a custom world position.
    /// </summary>
    public void PlayAt(string eventName, Vector3 position)
    {
        if (!TryGetKey(eventName, out string key)) return;
        SoundManager.Instance?.PlaySoundAtPosition(key, position);
    }

    /// <summary>
    /// Plays the sound bound to the given event name as a 2D sound (UI, global feedback).
    /// </summary>
    public void Play2D(string eventName)
    {
        if (!TryGetKey(eventName, out string key)) return;
        SoundManager.Instance?.PlaySound2D(key);
    }

    /// <summary>
    /// Returns true if a binding exists for this event name.
    /// </summary>
    public bool HasBinding(string eventName)
    {
        if (bindingMap == null) BuildMap();
        return bindingMap.ContainsKey(eventName);
    }

    private bool TryGetKey(string eventName, out string key)
    {
        if (bindingMap == null) BuildMap();

        if (bindingMap.TryGetValue(eventName, out key)) return true;

        Debug.LogWarning($"[SoundEmitter] No binding found for event '{eventName}' on {gameObject.name}.");
        return false;
    }
}
