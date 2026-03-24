using UnityEngine;

/// <summary>
/// Fast melee enemy. Aggressively chases the player and attacks quickly with reduced damage.
/// </summary>
public class EnemyMeleeFast : EnemyBase
{
    [Header("Fast Melee Settings")]
    [Tooltip("Distance at which the enemy starts chasing the player.")]
    public float chaseRange = 12f;

    [Header("Stuck Detection")]
    [Tooltip("Vitesse en-dessous de laquelle l'ennemi est consid�r� bloqu�")]
    public float stuckSpeedThreshold = 0.3f;
    [Tooltip("Dur�e avant de tenter un contournement")]
    public float stuckDuration = 1.0f;
    [Tooltip("Rayon du point de contournement al�atoire")]
    public float steerRadius = 2.5f;

    private float stuckTimer;
    private Vector3 lastPosition;

    protected override void Start()
    {
        base.Start();

        attackRange = 1.2f;
        attackDamage = 6f;
        attackCooldown = 0.6f;
        navAgent.speed = 5f;
        navAgent.acceleration = 12f;

        lastPosition = transform.position;
    }

    protected override void UpdateBehavior()
    {
        float sqrDistanceToPlayer = (transform.position - playerTransform.position).sqrMagnitude;

        if (sqrDistanceToPlayer <= detectionRange * detectionRange)
        {
            if (sqrDistanceToPlayer > attackRange * attackRange)
            {
                HandleStuckDetection();
                MoveToward(playerTransform.position);
            }
            else
            {
                stuckTimer = 0f;
                StopMovement();

                if (CanAttack())
                    DealDamageToPlayer();
            }
        }
        else
        {
            stuckTimer = 0f;
            StopMovement();
        }
    }

    private void HandleStuckDetection()
    {
        bool isStuck = navAgent.velocity.magnitude < stuckSpeedThreshold;

        if (isStuck)
        {
            stuckTimer += Time.deltaTime;

            if (stuckTimer >= stuckDuration)
            {
                stuckTimer = 0f;
                TrySteerAroundObstacle();
            }
        }
        else
        {
            stuckTimer = 0f;
        }
    }

    /// <summary>
    /// Sets a temporary steering destination offset from the current position to unblock the agent.
    /// </summary>
    private void TrySteerAroundObstacle()
    {
        Vector3 randomOffset = Random.insideUnitSphere * steerRadius;
        randomOffset.y = 0f;
        Vector3 steerTarget = transform.position + randomOffset;

        if (UnityEngine.AI.NavMesh.SamplePosition(steerTarget, out UnityEngine.AI.NavMeshHit hit, steerRadius, UnityEngine.AI.NavMesh.AllAreas))
        {
            navAgent.SetDestination(hit.position);
            lastNavUpdateTime = Time.time; // Reset le rate limiter de MoveToward
        }
    }
}
