using UnityEngine;

/// <summary>
/// Fast melee enemy. Aggressively chases the player and attacks quickly with reduced damage.
/// </summary>
public class EnemyMeleeFast : EnemyBase
{
    [Header("Fast Melee Settings")]
    [Tooltip("Distance at which the enemy starts chasing the player.")]
    public float chaseRange = 12f;

    protected override void Start()
    {
        base.Start();

        // Fast melee defaults
        attackRange = 1.2f;
        attackDamage = 6f;
        attackCooldown = 0.6f;
        navAgent.speed = 5f;
        navAgent.acceleration = 12f;
    }

    protected override void UpdateBehavior()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= detectionRange)
        {
            if (distanceToPlayer > attackRange)
            {
                MoveToward(playerTransform.position);
            }
            else
            {
                StopMovement();

                if (CanAttack())
                {
                    DealDamageToPlayer();
                }
            }
        }
        else
        {
            StopMovement();
        }
    }
}
