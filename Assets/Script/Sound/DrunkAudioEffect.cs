using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Transitions the AudioMixer between sober and drunk states based on DrunkEffect.
/// Exposes Pitch, LowPass and Reverb parameters on the Main mixer group.
/// Attach on the SoundManager GameObject or any persistent object.
/// </summary>
public class DrunkAudioEffect : MonoBehaviour
{
    [Header("Mixer")]
    [Tooltip("Le MixerAudio principal.")]
    public AudioMixer audioMixer;

    [Header("Snapshots")]
    [Tooltip("Snapshot état normal — à créer dans le mixer.")]
    public AudioMixerSnapshot soberSnapshot;

    [Tooltip("Snapshot état ivre — à créer dans le mixer avec les effets actifs.")]
    public AudioMixerSnapshot drunkSnapshot;

    [Header("Transition")]
    [Tooltip("Durée du fade vers l'état ivre.")]
    public float fadeInDuration = 1.5f;

    [Tooltip("Durée du fade vers l'état normal.")]
    public float fadeOutDuration = 2.5f;

    [Header("Drunk Parameters (exposed from Mixer)")]
    [Tooltip("Nom du paramètre Pitch exposé sur le groupe Main.")]
    public string pitchParam = "MainPitch";

    [Tooltip("Nom du paramètre Low Pass exposé sur le groupe Main.")]
    public string lowPassParam = "MainLowPass";

    [Tooltip("Pitch cible quand ivre (légèrement abaissé).")]
    [Range(0.5f, 1f)]
    public float drunkPitch = 0.88f;

    [Tooltip("Fréquence Low Pass quand ivre (filtre les aigus).")]
    public float drunkLowPassFreq = 1200f;

    private DrunkEffect drunkEffect;
    private bool wasDrunk = false;

    // Valeurs normales mémorisées au Start
    private float soberPitch = 1f;
    private float soberLowPassFreq = 22000f;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            drunkEffect = player.GetComponent<DrunkEffect>();

        if (drunkEffect == null)
            Debug.LogWarning("[DrunkAudioEffect] DrunkEffect introuvable sur le joueur !");

        // Sauvegarde les valeurs sobres initiales
        audioMixer.GetFloat(pitchParam, out soberPitch);
        audioMixer.GetFloat(lowPassParam, out soberLowPassFreq);
    }

    private void Update()
    {
        if (drunkEffect == null) return;

        bool isDrunk = drunkEffect.IsDrunk();

        if (isDrunk && !wasDrunk)
        {
            EnterDrunkState();
            wasDrunk = true;
        }
        else if (!isDrunk && wasDrunk)
        {
            ExitDrunkState();
            wasDrunk = false;
        }

        // Modulation continue : intensité des effets suit le stack de bières
        if (isDrunk)
            UpdateDrunkIntensity();
    }

    private void EnterDrunkState()
    {
        // Transition par snapshot si configuré
        if (drunkSnapshot != null && soberSnapshot != null)
        {
            drunkSnapshot.TransitionTo(fadeInDuration);
            return;
        }

        // Fallback : paramètres exposés directs
        StopAllCoroutines();
        StartCoroutine(TransitionParam(pitchParam, soberPitch, drunkPitch, fadeInDuration));
        StartCoroutine(TransitionParam(lowPassParam, soberLowPassFreq, drunkLowPassFreq, fadeInDuration));
    }

    private void ExitDrunkState()
    {
        if (drunkSnapshot != null && soberSnapshot != null)
        {
            soberSnapshot.TransitionTo(fadeOutDuration);
            return;
        }

        StopAllCoroutines();
        StartCoroutine(TransitionParam(pitchParam, drunkPitch, soberPitch, fadeOutDuration));
        StartCoroutine(TransitionParam(lowPassParam, drunkLowPassFreq, soberLowPassFreq, fadeOutDuration));
    }

    /// <summary>
    /// Ajuste l'intensité du pitch en continu selon le stack de bières.
    /// </summary>
    private void UpdateDrunkIntensity()
    {
        if (drunkSnapshot != null) return; // géré par snapshot

        int stack = drunkEffect.GetBeerStack();
        int maxStack = 5;
        float t = maxStack > 1 ? Mathf.Clamp01((float)(stack - 1) / (maxStack - 1)) : 1f;

        float targetPitch = Mathf.Lerp(Mathf.Lerp(soberPitch, drunkPitch, 0.3f), drunkPitch, t);
        float targetLowPass = Mathf.Lerp(soberLowPassFreq, drunkLowPassFreq, t);

        audioMixer.SetFloat(pitchParam, targetPitch);
        audioMixer.SetFloat(lowPassParam, targetLowPass);
    }

    private System.Collections.IEnumerator TransitionParam(string param, float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float value = Mathf.Lerp(from, to, elapsed / duration);
            audioMixer.SetFloat(param, value);
            yield return null;
        }
        audioMixer.SetFloat(param, to);
    }
}
