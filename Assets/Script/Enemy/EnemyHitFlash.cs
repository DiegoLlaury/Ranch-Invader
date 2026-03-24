using System.Collections;
using UnityEngine;

/// <summary>
/// Plays a red flash on the impostor quad material when the enemy takes damage.
/// Driven by EnemyBase.TakeDamage().
/// </summary>
public class EnemyHitFlash : MonoBehaviour
{
    private static readonly int HitIntensityId = Shader.PropertyToID("_HitIntensity");

    [Header("Flash Settings")]
    [Tooltip("Duration of the full flash in seconds.")]
    public float flashDuration = 0.15f;

    [Tooltip("Peak intensity of the hit color overlay (0 = none, 1 = full).")]
    [Range(0f, 1f)]
    public float flashPeakIntensity = 0.85f;

    private Material impostorMaterial;
    private Coroutine flashCoroutine;

    // Retried every Flash() call until resolved — ImpostorEntity.Initialize() runs
    // one frame after ImpostorEntityAI.SetupImpostor() (AddComponent defers Start).
    private bool TryResolveMaterial()
    {
        if (impostorMaterial != null) return true;

        // ImpostorEntity est sur un enfant instancié dynamiquement au runtime.
        // GetComponentInChildren couvre les enfants existants au moment de l'appel.
        ImpostorEntity impostorEntity = GetComponentInChildren<ImpostorEntity>(true);

        if (impostorEntity == null)
        {
            Debug.LogWarning($"[EnemyHitFlash] ImpostorEntity not found in children of {gameObject.name}.");
            return false;
        }

        impostorMaterial = impostorEntity.ImpostorMaterialInstance;

        if (impostorMaterial == null)
        {
            // ImpostorEntity existe mais Initialize() n'a pas encore tourné — retry au prochain hit.
            return false;
        }

        Debug.Log($"[EnemyHitFlash] Material resolved on {gameObject.name}: {impostorMaterial.name}");
        return true;
    }


    /// <summary>
    /// Triggers the hit flash. Safe to call while a flash is already running.
    /// </summary>
    public void Flash()
    {
        if (!TryResolveMaterial()) return;

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
            impostorMaterial.SetFloat(HitIntensityId, Mathf.Lerp(0f, flashPeakIntensity, elapsed / halfDuration));
            yield return null;
        }

        elapsed = 0f;

        // Fade out
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            impostorMaterial.SetFloat(HitIntensityId, Mathf.Lerp(flashPeakIntensity, 0f, elapsed / halfDuration));
            yield return null;
        }

        impostorMaterial.SetFloat(HitIntensityId, 0f);
        flashCoroutine = null;
    }
}
