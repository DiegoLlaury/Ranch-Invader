using UnityEngine;

public class AudioDebugger : MonoBehaviour
{
    public KeyCode debugKey = KeyCode.F1;

    private void Update()
    {
        if (Input.GetKeyDown(debugKey))
        {
            DebugAllAudioSources();
        }
    }

    private void DebugAllAudioSources()
    {
        AudioSource[] allAudioSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);

        Debug.Log($"=== AUDIO DEBUG ({allAudioSources.Length} AudioSources found) ===");

        foreach (AudioSource source in allAudioSources)
        {
            if (source.isPlaying)
            {
                Debug.Log($"[PLAYING] {source.gameObject.name}");
                Debug.Log($"  - Clip: {source.clip?.name ?? "NULL"}");
                Debug.Log($"  - Position: {source.transform.position}");
                Debug.Log($"  - Spatial Blend: {source.spatialBlend} (0=2D, 1=3D)");
                Debug.Log($"  - Volume: {source.volume}");
                Debug.Log($"  - Min Distance: {source.minDistance}");
                Debug.Log($"  - Max Distance: {source.maxDistance}");
                Debug.Log($"  - Rolloff Mode: {source.rolloffMode}");
            }
        }

        AudioListener listener = FindFirstObjectByType<AudioListener>();
        if (listener != null)
        {
            Debug.Log($"AudioListener position: {listener.transform.position}");
        }
        else
        {
            Debug.LogError("NO AUDIO LISTENER FOUND!");
        }
    }
}
