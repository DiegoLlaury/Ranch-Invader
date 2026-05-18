using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public abstract class EnemyBase : MonoBehaviour, IDamageable
{
    [Header("References")]
    public Transform playerTransform;

    [Header("Health")]
    [SerializeField] protected float maxHealth = 100f;
    protected float currentHealth;

    [Header("Detection")]
    public float detectionRange = 15f;

    [Header("Combat")]
    public float attackRange = 1.5f;
    public float attackDamage = 10f;
    public float attackCooldown = 1.5f;

    [Header("NavMesh")]
    public float navMeshUpdateRate = 0.2f;

    [Header("Rotation")]
    public float rotationSpeed = 8f;

    [Header("VFX")]
    [Tooltip("Particle prefab played at the enemy position when it attacks.")]
    public GameObject attackVfxPrefab;

    [Tooltip("Particle prefab played at the enemy position when it dies.")]
    public GameObject deathVfxPrefab;

    [Tooltip("Particle prefab played at the enemy position when it spawns (teleport effect).")]
    public GameObject spawnVfxPrefab;

    [Tooltip("Rayon de recherche du NavMesh au spawn si l'ennemi apparaît hors du NavMesh")]
    public float navMeshSnapRadius = 5f;

    [Header("Slope")]
    [Tooltip("Incline l'ennemi visuellement sur les pentes pour plus de réalisme")]
    public bool alignToSlope = false;

    [Tooltip("Vitesse d'interpolation de l'alignement sur la pente")]
    public float slopeAlignSpeed = 8f;

    [Tooltip("Masque de layer utilisé pour détecter le sol")]
    public LayerMask groundLayer = ~0;


    // Sound event name constants — use these as keys in the SoundEmitter Inspector
    public const string SoundOnDetect = "OnDetect";   // Player spotted for the first time
    public const string SoundOnAttack = "OnAttack";   // Attack swing / shot fired
    public const string SoundOnHit = "OnHit";      // Received damage
    public const string SoundOnDeath = "OnDeath";    // Killed
    public const string SoundOnMove = "OnMove";     // Footstep / movement loop

    /// <summary>
    /// Fired whenever this enemy executes an attack. The parameter is the attacking EnemyBase instance.
    /// Subscribe from EnemyAnimatorController to drive the Attack trigger on the Animator.
    /// </summary>
    public static event System.Action<EnemyBase> OnEnemyAttack;

    public static event System.Action<Vector3> OnEnemyDied;

    /// <summary>
    /// Raises the OnEnemyAttack event for this instance. Call from subclasses when an attack is executed.
    /// </summary>
    protected void RaiseOnAttack() => OnEnemyAttack?.Invoke(this);

    protected NavMeshAgent navAgent;
    protected SoundEmitter soundEmitter;
    protected float lastAttackTime;
    protected float lastNavUpdateTime;
    protected bool isDead;
    private EnemyHitFlash hitFlash;
    private EnemyKnockback knockback;
    private PlayerHealth cachedPlayerHealth;

    private const float SlopeRaycastInterval = 0.1f;
    private float lastSlopeRaycastTime;


    private bool hasDetectedPlayer;
    private Quaternion targetSlopeRotation = Quaternion.identity;
    private const float MinVelocityToRotate = 0.05f;


    public Vector3 FacingDirection { get; private set; }
    public bool IsMoving => navAgent != null && navAgent.hasPath && !navAgent.isStopped;

    protected virtual void Awake()
    {
        navAgent = GetComponent<NavMeshAgent>();
        navAgent.updateRotation = false;

        soundEmitter = GetComponent<SoundEmitter>();

        hitFlash = GetComponent<EnemyHitFlash>();
        knockback = GetComponent<EnemyKnockback>();

        // The NavMeshAgent owns movement — Rigidbody must be kinematic
        // to prevent physics forces from conflicting with agent steering.
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    protected virtual void Start()
    {
        currentHealth = maxHealth;

        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                playerTransform = playerObj.transform;
            else
                Debug.LogWarning($"[{gameObject.name}] No GameObject with tag 'Player' found.");
        }

        if (playerTransform != null)
            cachedPlayerHealth = playerTransform.GetComponent<PlayerHealth>();

        SnapToNavMesh(); 
        if (spawnVfxPrefab != null)
            Instantiate(spawnVfxPrefab, transform.position, Quaternion.identity);
    }

    private void SnapToNavMesh()
    {
        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, navMeshSnapRadius, NavMesh.AllAreas))
        {
            navAgent.Warp(hit.position);

            // Raycast from well above to find the true ground surface,
            // preventing the enemy from sinking into geometry baked below the terrain mesh.
            Vector3 rayOrigin = hit.position + Vector3.up * 5f;
            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit groundHit, 10f, groundLayer))
                navAgent.Warp(new Vector3(hit.position.x, groundHit.point.y, hit.position.z));
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] Aucun NavMesh trouvé dans un rayon de {navMeshSnapRadius}m. Vérifiez le bake.");
        }
    }


    protected virtual void Update()
    {
        if (isDead || playerTransform == null) return;

        UpdateRotation();
        UpdateFacingDirection();
        UpdateSlopeAlignment();
        CheckDetection();
        UpdateBehavior();
    }

    protected abstract void UpdateBehavior();

    // ── Detection ─────────────────────────────────────────────────────────────

    private void CheckDetection()
    {
        bool inRange = IsPlayerInRange(detectionRange);

        if (inRange && !hasDetectedPlayer)
        {
            hasDetectedPlayer = true;
            OnPlayerDetected();
        }
        else if (!inRange && hasDetectedPlayer)
        {
            hasDetectedPlayer = false;
        }
    }

    /// <summary>
    /// Called once when the player enters detection range. Override to add custom behavior.
    /// </summary>
    protected virtual void OnPlayerDetected()
    {
        soundEmitter?.Play(SoundOnDetect);
    }

    // ── Rotation ──────────────────────────────────────────────────────────────

    private void UpdateRotation()
    {
        Vector3 desiredForward = GetDesiredForward();
        if (desiredForward.sqrMagnitude < 0.001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(desiredForward);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    protected virtual Vector3 GetDesiredForward()
    {
        Vector3 velocity = navAgent.velocity;
        velocity.y = 0f;

        // If the agent is nearly stopped (stuck against NavMesh boundary), 
        // rotate toward the player directly to avoid spinning in place.
        if (velocity.sqrMagnitude < MinVelocityToRotate * MinVelocityToRotate && playerTransform != null)
        {
            Vector3 toPlayer = playerTransform.position - transform.position;
            toPlayer.y = 0f;
            return toPlayer;
        }

        return velocity;
    }

    /// <summary>
    /// Aligne visuellement l'ennemi sur la normale du sol sous lui si alignToSlope est activé.
    /// Throttlé à 10 fois/seconde pour éviter un raycast par frame.
    /// </summary>
    private void UpdateSlopeAlignment()
    {
        if (!alignToSlope) return;

        // Lisse la rotation en permanence, mais ne recalcule le raycast qu'à intervalle fixe
        transform.rotation = Quaternion.Slerp(transform.rotation, targetSlopeRotation, slopeAlignSpeed * Time.deltaTime);

        if (Time.time - lastSlopeRaycastTime < SlopeRaycastInterval) return;
        lastSlopeRaycastTime = Time.time;

        if (Physics.Raycast(transform.position + Vector3.up * 0.2f, Vector3.down, out RaycastHit hit, 2f, groundLayer))
        {
            // Preserve the current yaw and apply only the slope tilt on top of it,
            // so the slope alignment never overwrites the AI's forward direction.
            Quaternion yawRotation  = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);
            Quaternion slopeTilt    = Quaternion.FromToRotation(Vector3.up, hit.normal);
            targetSlopeRotation     = slopeTilt * yawRotation;
        }
    }

    /// <summary>
    /// Rotates smoothly toward a world-space target position on the Y axis.
    /// Returns the remaining angle in degrees.
    /// </summary>
    protected float RotateToward(Vector3 targetPosition)
    {
        Vector3 toTarget = targetPosition - transform.position;
        toTarget.y = 0f;

        if (toTarget.sqrMagnitude < 0.001f) return 0f;

        Quaternion targetRotation = Quaternion.LookRotation(toTarget.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        return Vector3.Angle(transform.forward, toTarget.normalized);
    }

    // ── Movement ──────────────────────────────────────────────────────────────

    protected void MoveToward(Vector3 target)
    {
        if (Time.time - lastNavUpdateTime < navMeshUpdateRate) return;

        lastNavUpdateTime = Time.time;
        navAgent.SetDestination(target);
    }

    protected void StopMovement()
    {
        if (navAgent.isOnNavMesh)
            navAgent.ResetPath();
    }

    // ── Combat ────────────────────────────────────────────────────────────────

    protected bool IsPlayerInRange(float range)
    {
        return (transform.position - playerTransform.position).sqrMagnitude <= range * range;
    }

    protected bool CanAttack()
    {
        return Time.time - lastAttackTime >= attackCooldown;
    }

    /// <summary>
    /// Deals melee damage to the player and plays the attack sound.
    /// </summary>
    protected void DealDamageToPlayer()
    {
        lastAttackTime = Time.time;

        // Notify animator subscribers that an attack has been executed
        RaiseOnAttack();

        // Feedback: attack swing sound
        soundEmitter?.Play(SoundOnAttack);

        if (attackVfxPrefab != null)
            Instantiate(attackVfxPrefab, transform.position, transform.rotation);

        if (cachedPlayerHealth != null)
            cachedPlayerHealth.TakeDamage(attackDamage);
    }

    // ── Damage & Death ────────────────────────────────────────────────────────

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        soundEmitter?.Play(SoundOnHit);
        hitFlash?.Flash();
        knockback?.ApplyKnockback(
            playerTransform != null ? playerTransform.position : transform.position - transform.forward
        );

        if (currentHealth <= 0f)
            Die();
    }

    protected virtual void Die()
    {
        isDead = true;
        StopMovement();

        // Feedback: death sound
        soundEmitter?.Play(SoundOnDeath);

        if (deathVfxPrefab != null)
            Instantiate(deathVfxPrefab, transform.position, Quaternion.identity);

        OnEnemyDied?.Invoke(transform.position);

        Destroy(gameObject);
    }

    // ── Facing ────────────────────────────────────────────────────────────────

    private void UpdateFacingDirection()
    {
        Vector3 velocity = navAgent.velocity;
        velocity.y = 0f;

        // Only update the facing direction when there is meaningful velocity.
        // If the agent is stopping or stopped, preserve the last valid direction
        // to prevent it from snapping to zero and causing a backward-facing frame.
        if (velocity.sqrMagnitude > 0.01f)
            FacingDirection = velocity.normalized;
    }

#if UNITY_EDITOR
    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
#endif
}
