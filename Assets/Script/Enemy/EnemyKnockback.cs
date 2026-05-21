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
        {
            StopCoroutine(knockbackCoroutine);

            if (navAgent != null &&
                navAgent.isActiveAndEnabled &&
                navAgent.isOnNavMesh)
            {
                navAgent.updatePosition = true;
                navAgent.isStopped = false;
            }
        }

        knockbackCoroutine = StartCoroutine(KnockbackRoutine(direction));
    }

    private IEnumerator KnockbackRoutine(Vector3 direction)
    {
        if (navAgent == null || !navAgent.isActiveAndEnabled || !navAgent.isOnNavMesh)
        {
            yield break;
        }

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
            Vector3 nextPosition = transform.position + direction * stepDistance;

            if (TryGetValidNavMeshPosition(nextPosition, out Vector3 validPosition))
            {
                transform.position = validPosition;

                if (navAgent.isOnNavMesh)
                    navAgent.Warp(validPosition);
            }
            else
            {
                break;
            }

            yield return null;
        }

        if (navAgent != null &&
        navAgent.isActiveAndEnabled &&
        navAgent.isOnNavMesh)
        {
            navAgent.updatePosition = true;
            navAgent.isStopped = false;
        }

        knockbackCoroutine = null;
    }

    private bool TryGetValidNavMeshPosition(Vector3 targetPosition, out Vector3 validPosition)
    {
        if (NavMesh.SamplePosition(targetPosition,
                                   out NavMeshHit hit,
                                   1f,
                                   NavMesh.AllAreas))
        {
            validPosition = hit.position;
            return true;
        }

        validPosition = transform.position;
        return false;
    }

}
