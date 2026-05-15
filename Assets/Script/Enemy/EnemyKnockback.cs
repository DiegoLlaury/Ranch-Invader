using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Applies a physical knockback to the enemy when it takes damage.
/// Temporarily disables the NavMeshAgent, translates the enemy along the hit direction,
/// then re-enables the agent once the impulse decays.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyKnockback : MonoBehaviour
{
    [Header("Knockback Settings")]
    [Tooltip("Maximum distance the enemy can be pushed back in world units.")]
    public float knockbackDistance = 1.2f;

    [Tooltip("Duration of the knockback movement in seconds.")]
    public float knockbackDuration = 0.18f;

    [Tooltip("Animation curve controlling knockback speed over time (X = normalized time, Y = speed multiplier).")]
    public AnimationCurve knockbackCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

    private NavMeshAgent navAgent;
    private Coroutine knockbackCoroutine;

    private void Awake()
    {
        navAgent = GetComponent<NavMeshAgent>();
    }

    /// <summary>
    /// Triggers a knockback impulse away from the given hit source position.
    /// Safe to call while a knockback is already running — it will be restarted.
    /// </summary>
    /// <param name="hitSourcePosition">World-space position of whoever dealt the hit.</param>
    public void ApplyKnockback(Vector3 hitSourcePosition)
    {
        Vector3 direction = transform.position - hitSourcePosition;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            direction = -transform.forward;

        direction.Normalize();

        if (knockbackCoroutine != null)
            StopCoroutine(knockbackCoroutine);

        knockbackCoroutine = StartCoroutine(KnockbackRoutine(direction));
    }

    private IEnumerator KnockbackRoutine(Vector3 direction)
    {
        navAgent.isStopped = true;
        navAgent.updatePosition = false;

        float elapsed = 0f;

        while (elapsed < knockbackDuration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsed / knockbackDuration);
            float speedMultiplier = knockbackCurve.Evaluate(normalizedTime);
            float stepDistance = (knockbackDistance / knockbackDuration) * speedMultiplier * Time.deltaTime;

            // Déplace directement le transform — contourne les problèmes de baseOffset du NavMeshAgent.
            transform.position += direction * stepDistance;

            // Resynchronise l'agent à chaque frame pour qu'il reste ancré sur le NavMesh.
            if (navAgent.isOnNavMesh)
                navAgent.Warp(transform.position);

            yield return null;
        }

        navAgent.updatePosition = true;
        navAgent.isStopped = false;

        knockbackCoroutine = null;
    }

}
