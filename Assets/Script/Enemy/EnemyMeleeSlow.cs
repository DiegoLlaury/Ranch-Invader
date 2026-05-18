using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Slow melee enemy. Periodically moves toward the player and attacks with a large melee range.
/// </summary>
public class EnemyMeleeSlow : EnemyBase
{
    [Header("Slow Melee Settings")]
    [Tooltip("Delay between each move-toward-player decision.")]
    public float moveDecisionInterval = 3f;

    [Tooltip("How long the enemy pursues before pausing again.")]
    public float chaseDuration = 2f;

    private float lastMoveDecisionTime;
    private bool isChasing;
    private float chaseEndTime;

    protected override void Start()
    {
        base.Start();

        lastMoveDecisionTime = -moveDecisionInterval;
    }

    protected override void UpdateBehavior()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // Decide to chase periodically
        if (!isChasing && Time.time - lastMoveDecisionTime >= moveDecisionInterval)
        {
            if (distanceToPlayer <= detectionRange)
            {
                isChasing = true;
                chaseEndTime = Time.time + chaseDuration;
                lastMoveDecisionTime = Time.time;
            }
        }

        // Chase window
        if (isChasing)
        {
            if (Time.time >= chaseEndTime || distanceToPlayer <= attackRange)
            {
                isChasing = false;
                StopMovement();
            }
            else
            {
                MoveToward(playerTransform.position);
            }
        }

        // Attack when in range
        if (distanceToPlayer <= attackRange && CanAttack())
        {
            DealDamageToPlayer();
        }
    }
}
