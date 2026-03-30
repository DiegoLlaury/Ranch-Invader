using UnityEngine;

/// <summary>
/// Ranged enemy. Rotates to face the player before firing predicted projectiles.
/// </summary>
public class EnemyRanged : EnemyBase
{
    [Header("Ranged Settings")]
    public GameObject enemyProjectilePrefab;

    [Tooltip("Speed of the fired projectile, used for leading the target.")]
    public float projectileSpeed = 12f;

    [Tooltip("Preferred distance from the player.")]
    public float preferredDistance = 8f;

    [Tooltip("Minimum distance before backing away from the player.")]
    public float retreatDistance = 4f;

    [Tooltip("Forward offset of the projectile spawn point from the enemy center.")]
    public float projectileSpawnOffset = 0.8f;

    [Tooltip("Height of the projectile spawn point relative to the enemy base.")]
    public float projectileSpawnHeight = 1f;

    [Header("Aim Settings")]
    [Tooltip("Maximum angle (degrees) between forward and player direction to allow firing.")]
    public float readyToFireAngle = 10f;

    private CharacterController playerCharacterController;

    // True when the enemy is in the aiming phase (stopped moving, rotating toward player)
    private bool isAiming;

    private Vector3 PlayerBodyCenter
    {
        get
        {
            if (playerCharacterController != null)
                return playerTransform.position + playerCharacterController.center;

            return playerTransform.position + Vector3.up;
        }
    }

    protected override void Start()
    {
        base.Start();

        attackRange = 14f;
        attackDamage = 15f;
        attackCooldown = 2.5f;
        navAgent.speed = 2.5f;

        if (playerTransform != null)
            playerCharacterController = playerTransform.GetComponent<CharacterController>();
    }

    // While aiming, override the base rotation to face the player instead of movement direction
    protected override Vector3 GetDesiredForward()
    {
        if (isAiming)
        {
            Vector3 toPlayer = playerTransform.position - transform.position;
            toPlayer.y = 0f;
            return toPlayer;
        }

        return base.GetDesiredForward();
    }

    protected override void UpdateBehavior()
    {
        if (enemyProjectilePrefab == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer > detectionRange)
        {
            isAiming = false;
            StopMovement();
            return;
        }

        // Positioning logic
        if (distanceToPlayer < retreatDistance)
        {
            isAiming = false;
            Vector3 retreatDirection = (transform.position - playerTransform.position).normalized;
            MoveToward(transform.position + retreatDirection * preferredDistance);
        }
        else if (distanceToPlayer > preferredDistance + 1f)
        {
            isAiming = false;
            MoveToward(playerTransform.position);
        }
        else
        {
            StopMovement();
        }

        // Aim and fire when in range and cooldown is ready
        if (distanceToPlayer <= attackRange && CanAttack())
        {
            isAiming = true;

            // Measure current angle between forward and the player
            Vector3 toPlayer = playerTransform.position - transform.position;
            toPlayer.y = 0f;
            float angleToPlayer = Vector3.Angle(transform.forward, toPlayer);

            // Only fire once properly facing the player
            if (angleToPlayer <= readyToFireAngle)
            {
                isAiming = false;
                FirePredictedProjectile();
            }
        }
        else
        {
            isAiming = false;
        }
    }

    private void FirePredictedProjectile()
    {
        lastAttackTime = Time.time;

        // Notify animator subscribers that an attack has been executed
        RaiseOnAttack();

        // Feedback: fire sound
        soundEmitter?.Play(SoundOnAttack);

        Vector3 targetPos = PredictPlayerPosition();
        Vector3 enemyCenter = transform.position + Vector3.up * projectileSpawnHeight;

        // Spawn offset along the actual aim direction, not transform.forward,
        // to stay accurate with predicted positions
        Vector3 toTarget = (targetPos - enemyCenter).normalized;
        Vector3 spawnPos = enemyCenter + toTarget * projectileSpawnOffset;

        GameObject projectileObj = Instantiate(enemyProjectilePrefab, spawnPos, Quaternion.identity);

        EnemyProjectile enemyProjectile = projectileObj.GetComponent<EnemyProjectile>();
        if (enemyProjectile != null)
        {
            enemyProjectile.damage = attackDamage;
            enemyProjectile.Launch(toTarget, projectileSpeed);
        }
    }

    private Vector3 PredictPlayerPosition()
    {
        Vector3 bodyCenter = PlayerBodyCenter;

        Vector3 playerVelocity = playerCharacterController != null
            ? playerCharacterController.velocity
            : Vector3.zero;

        float distanceToTarget = Vector3.Distance(transform.position, bodyCenter);
        float timeToReach = distanceToTarget / projectileSpeed;

        return bodyCenter + playerVelocity * timeToReach;
    }

#if UNITY_EDITOR
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, preferredDistance);

        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireSphere(transform.position, retreatDistance);

        if (Application.isPlaying && playerTransform != null)
        {
            // Point de vis�e r�el
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(PlayerBodyCenter, 0.15f);

            // Arc repr�sentant le readyToFireAngle
            Gizmos.color = isAiming ? Color.green : Color.grey;
            Vector3 leftBound = Quaternion.Euler(0, -readyToFireAngle, 0) * transform.forward;
            Vector3 rightBound = Quaternion.Euler(0, readyToFireAngle, 0) * transform.forward;
            Gizmos.DrawRay(transform.position + Vector3.up, leftBound * 3f);
            Gizmos.DrawRay(transform.position + Vector3.up, rightBound * 3f);
        }
    }
#endif
}
