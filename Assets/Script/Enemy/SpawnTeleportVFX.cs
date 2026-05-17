using System.Collections;
using UnityEngine;

/// <summary>
/// Animates a teleportation VFX by scaling it up then shrinking it back to zero,
/// then destroys the GameObject. Attach this to the spawn VFX prefab.
/// </summary>
[RequireComponent(typeof(ParticleSystem))]
public class SpawnTeleportVFX : MonoBehaviour
{
    [Tooltip("Duration of the scale-up phase.")]
    public float growDuration = 0.4f;

    [Tooltip("Duration the VFX stays at full scale before shrinking.")]
    public float holdDuration = 0.3f;

    [Tooltip("Duration of the scale-down phase.")]
    public float shrinkDuration = 0.5f;

    [Tooltip("Maximum local scale reached at peak.")]
    public float peakScale = 1.5f;

    [Tooltip("Animation curve applied to the grow phase.")]
    public AnimationCurve growCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Tooltip("Animation curve applied to the shrink phase.")]
    public AnimationCurve shrinkCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

    private ParticleSystem cachedParticleSystem;

    private void Awake()
    {
        cachedParticleSystem = GetComponent<ParticleSystem>();
        transform.localScale = Vector3.zero;
    }

    private void Start()
    {
        StartCoroutine(PlayScaleAnimation());
    }

    private IEnumerator PlayScaleAnimation()
    {
        // ── Grow ──────────────────────────────────────────────────────────
        float elapsed = 0f;
        while (elapsed < growDuration)
        {
            float t = elapsed / growDuration;
            float scale = growCurve.Evaluate(t) * peakScale;
            transform.localScale = Vector3.one * scale;
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = Vector3.one * peakScale;

        // ── Hold ──────────────────────────────────────────────────────────
        yield return new WaitForSeconds(holdDuration);

        // ── Shrink ────────────────────────────────────────────────────────
        elapsed = 0f;
        while (elapsed < shrinkDuration)
        {
            float t = elapsed / shrinkDuration;
            float scale = shrinkCurve.Evaluate(t) * peakScale;
            transform.localScale = Vector3.one * scale;
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = Vector3.zero;

        // Stop emission so existing particles finish naturally, then destroy
        if (cachedParticleSystem != null)
        {
            cachedParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            yield return new WaitForSeconds(cachedParticleSystem.main.startLifetime.constantMax);
        }

        Destroy(gameObject);
    }
}
