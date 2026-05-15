using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles knockback and flee behavior for the cow when touched by the player.
/// After the impulse, the cow flees to a random position within its zone,
/// then resumes normal RandomMovementAI behavior.
/// Also detects enemies hit during the knockback and damages them.
/// </summary>
[RequireComponent(typeof(RandomMovementAI))]
public class CowKnockback : MonoBehaviour
{
    [Header("Knockback Settings")]
    [Tooltip("Distance the cow is pushed back on contact.")]
    public float knockbackDistance = 3f;

    [Tooltip("Duration of the knockback translation in seconds.")]
    public float knockbackDuration = 0.25f;

    [Tooltip("Speed curve over normalized knockback time.")]
    public AnimationCurve knockbackCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

    [Header("Flee Settings")]
    [Tooltip("Multiplier applied to the normal move speed during flee.")]
    public float fleeSpeedMultiplier = 2.5f;

    [Tooltip("Distance the cow tries to reach while fleeing (within the zone).")]
    public float fleeDistance = 8f;

    [Tooltip("Max attempts to find a valid flee target inside the zone.")]
    public int fleePositionAttempts = 15;

    [Header("Enemy Hit Settings")]
    [Tooltip("Damage dealt to enemies the cow collides with during knockback.")]
    public float enemyDamage = 20f;

    [Tooltip("Radius of the overlap sphere used to detect enemies during knockback.")]
    public float enemyDetectionRadius = 1f;

    [Tooltip("Layer(s) that contain enemies.")]
    public LayerMask enemyLayer;

    [Header("Obstacle Settings")]
    [Tooltip("Layer(s) considered as solid obstacles (fences, walls…).")]
    public LayerMask obstacleLayer;

    [Tooltip("Radius used to validate the flee destination is not inside an obstacle.")]
    public float positionValidationRadius = 0.5f;

    // ── State ─────────────────────────────────────────────────────────────────

    private bool isInKnockbackState = false;
    private RandomMovementAI movementAI;
    private Coroutine activeRoutine;

    private readonly HashSet<GameObject> hitEnemiesThisKnockback = new HashSet<GameObject>();

    // ── Unity ─────────────────────────────────────────────────────────────────

    private void Awake()
    {
        movementAI = GetComponent<RandomMovementAI>();
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Triggers the knockback + flee sequence. Call this when the player touches the cow.
    /// </summary>
    /// <param name="hitSourcePosition">World-space position of the player.</param>
    public void ApplyKnockback(Vector3 hitSourcePosition)
    {
        if (isInKnockbackState) return;

        Vector3 direction = transform.position - hitSourcePosition;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            direction = -transform.forward;

        direction.Normalize();

        if (activeRoutine != null)
            StopCoroutine(activeRoutine);

        activeRoutine = StartCoroutine(KnockbackAndFleeRoutine(direction));
    }

    // ── Routines ──────────────────────────────────────────────────────────────

    private IEnumerator KnockbackAndFleeRoutine(Vector3 knockbackDirection)
    {
        isInKnockbackState = true;
        hitEnemiesThisKnockback.Clear();

        // Stoppe proprement les coroutines de l'IA — enabled = false ne suffit pas
        movementAI.Pause();

        yield return StartCoroutine(KnockbackImpulse(knockbackDirection));
        yield return StartCoroutine(FleeToSafePosition(knockbackDirection));

        // Relance la boucle de déplacement depuis le début
        movementAI.Resume();
        isInKnockbackState = false;
        activeRoutine = null;
    }

    private IEnumerator KnockbackImpulse(Vector3 direction)
    {
        float elapsed = 0f;

        while (elapsed < knockbackDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / knockbackDuration);
            float speedMultiplier = knockbackCurve.Evaluate(t);
            float step = (knockbackDistance / knockbackDuration) * speedMultiplier * Time.deltaTime;

            if (!WouldHitObstacle(direction, step))
                transform.position += direction * step;

            HitEnemiesAlong(direction);

            yield return null;
        }
    }

    private IEnumerator FleeToSafePosition(Vector3 knockbackDirection)
    {
        Vector3 fleeTarget = FindFleeTarget(knockbackDirection);

        if (fleeTarget == Vector3.zero)
            yield break;

        float speed = movementAI.moveSpeed * fleeSpeedMultiplier;

        Vector3 initialDir = (fleeTarget - transform.position);
        initialDir.y = 0f;
        if (initialDir.sqrMagnitude > 0.001f)
        {
            Vector3 normalizedDir = initialDir.normalized;
            transform.rotation = Quaternion.LookRotation(normalizedDir);
            // Synchronise FacingDirection pour que Entity_Behavior affiche le bon sprite
            movementAI.FacingDirection = normalizedDir;
        }

        while (Vector3.Distance(transform.position, fleeTarget) > 0.3f)
        {
            Vector3 dir = (fleeTarget - transform.position);
            dir.y = 0f;
            dir.Normalize();

            // Met à jour la direction à chaque frame pour le sprite
            movementAI.FacingDirection = dir;

            float step = speed * Time.deltaTime;

            if (!WouldHitObstacle(dir, step))
            {
                transform.position += dir * step;
            }
            else
            {
                Vector3 perp = Vector3.Cross(dir, Vector3.up);
                if (!WouldHitObstacle(perp, step))
                    transform.position += perp * step;
                else if (!WouldHitObstacle(-perp, step))
                    transform.position += -perp * step;
                else
                    break;
            }

            yield return null;
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private Vector3 FindFleeTarget(Vector3 knockbackDirection)
    {
        Vector3 center = movementAI.zoneCenter;
        Vector2 size = movementAI.zoneSize;

        for (int i = 0; i < fleePositionAttempts; i++)
        {
            // First half of attempts: bias toward knockback direction (flee away from player)
            // Second half: fully random within zone
            float bias = (i < fleePositionAttempts / 2) ? 1f : 0f;
            Vector3 randomDir = (knockbackDirection * bias + Random.insideUnitSphere).normalized;
            randomDir.y = 0f;

            if (randomDir.sqrMagnitude < 0.001f)
                randomDir = knockbackDirection;

            randomDir.Normalize();

            float distance = Random.Range(fleeDistance * 0.5f, fleeDistance);
            Vector3 candidate = transform.position + randomDir * distance;

            // Clamp inside zone bounds
            candidate.x = Mathf.Clamp(candidate.x, center.x - size.x / 2f, center.x + size.x / 2f);
            candidate.z = Mathf.Clamp(candidate.z, center.y - size.y / 2f, center.y + size.y / 2f);
            candidate.y = transform.position.y;

            if (IsPositionValid(candidate))
                return candidate;
        }

        return Vector3.zero;
    }

    private bool IsPositionValid(Vector3 position)
    {
        if (obstacleLayer == 0) return true;

        Collider[] colliders = Physics.OverlapSphere(position, positionValidationRadius, obstacleLayer);
        foreach (Collider col in colliders)
        {
            if (col.transform == transform || col.transform.IsChildOf(transform))
                continue;
            return false;
        }
        return true;
    }

    private bool WouldHitObstacle(Vector3 direction, float distance)
    {
        if (obstacleLayer == 0) return false;

        Vector3 origin = transform.position + Vector3.up * 0.5f;
        return Physics.Raycast(origin, direction, distance + 0.1f, obstacleLayer);
    }

    private void HitEnemiesAlong(Vector3 direction)
    {
        if (enemyLayer == 0) return;

        Collider[] hits = Physics.OverlapSphere(transform.position, enemyDetectionRadius, enemyLayer);

        foreach (Collider col in hits)
        {
            if (hitEnemiesThisKnockback.Contains(col.gameObject)) continue;
            hitEnemiesThisKnockback.Add(col.gameObject);

            IDamageable damageable = col.GetComponent<IDamageable>();
            damageable?.TakeDamage(enemyDamage);

            EnemyKnockback enemyKnockback = col.GetComponent<EnemyKnockback>();
            if (enemyKnockback != null)
            {
                // Fake hit source behind the cow so the enemy flies in the cow's travel direction
                Vector3 fakeHitSource = transform.position - direction;
                enemyKnockback.ApplyKnockback(fakeHitSource);
            }
        }
    }

    // ── Gizmos ────────────────────────────────────────────────────────────────

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.6f);
        Gizmos.DrawWireSphere(transform.position, enemyDetectionRadius);
    }
#endif
}
