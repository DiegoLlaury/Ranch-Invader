using System.Collections;
using UnityEngine;

/// <summary>
/// Plays a red flash on the impostor quad material when the enemy takes damage.
/// Call Flash() from EnemyBase.TakeDamage().
/// </summary>
public class EnemyHitFlash : MonoBehaviour
{
    private static readonly int HitIntensityId = Shader.PropertyToID("_HitIntensity");

    [Header("Flash Settings")]
    [Tooltip("Duration of the full flash in seconds.")]
    public float flashDuration = 0.15f;

    [Tooltip("Intensity of the hit color overlay (0 = none, 1 = full).")]
    [Range(0f, 1f)]
    public float flashPeakIntensity = 0.85f;

    private Material impostorMaterial;
    private Coroutine flashCoroutine;

    // Resolved lazily on first Flash() call because ImpostorEntityAI instantiates
    // the quad child in its own Start(), which may execute after ours.
    private bool isMaterialResolved;

    private void TryResolveMaterial()
    {
        if (isMaterialResolved) return;

        isMaterialResolved = true;

        MeshRenderer quadRenderer = GetComponentInChildren<MeshRenderer>();
        if (quadRenderer != null)
        {
            impostorMaterial = quadRenderer.material;
        }
        else
        {
            Debug.LogWarning($"[EnemyHitFlash] No MeshRenderer found in children of {gameObject.name}. " +
                             "Ensure ImpostorEntityAI has initialised the quad before the first hit.");
        }
    }

    /// <summary>
    /// Triggers the hit flash effect. Safe to call while a flash is already running.
    /// </summary>
    public void Flash()
    {
        TryResolveMaterial();

        if (impostorMaterial == null) return;

        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);

        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        float halfDuration = flashDuration * 0.5f;
        float elapsed = 0f;

        // Fade in
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            impostorMaterial.SetFloat(HitIntensityId, Mathf.Lerp(0f, flashPeakIntensity, t));
            yield return null;
        }

        elapsed = 0f;

        // Fade out
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            impostorMaterial.SetFloat(HitIntensityId, Mathf.Lerp(flashPeakIntensity, 0f, t));
            yield return null;
        }

        impostorMaterial.SetFloat(HitIntensityId, 0f);
        flashCoroutine = null;
    }
}
