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


    // Sound event name constants — use these as keys in the SoundEmitter Inspector
    public const string SoundOnDetect = "OnDetect";   // Player spotted for the first time
    public const string SoundOnAttack = "OnAttack";   // Attack swing / shot fired
    public const string SoundOnHit = "OnHit";      // Received damage
    public const string SoundOnDeath = "OnDeath";    // Killed
    public const string SoundOnMove = "OnMove";     // Footstep / movement loop

    protected NavMeshAgent navAgent;
    protected SoundEmitter soundEmitter;
    protected float lastAttackTime;
    protected float lastNavUpdateTime;
    protected bool isDead;

    private bool hasDetectedPlayer;

    public Vector3 FacingDirection { get; private set; }
    public bool IsMoving => navAgent != null && navAgent.velocity.sqrMagnitude > 0.01f;

    protected virtual void Awake()
    {
        navAgent = GetComponent<NavMeshAgent>();
        navAgent.updateRotation = false;

        soundEmitter = GetComponent<SoundEmitter>();
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
    }

    protected virtual void Update()
    {
        if (isDead || playerTransform == null) return;

        UpdateRotation();
        UpdateFacingDirection();
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
        return velocity;
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
        return Vector3.Distance(transform.position, playerTransform.position) <= range;
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

        // Feedback: attack swing sound
        soundEmitter?.Play(SoundOnAttack);

        if (attackVfxPrefab != null)
            Instantiate(attackVfxPrefab, transform.position, transform.rotation);


        PlayerHealth playerHealth = playerTransform.GetComponent<PlayerHealth>();
        if (playerHealth != null)
            playerHealth.TakeDamage(attackDamage);
    }

    // ── Damage & Death ────────────────────────────────────────────────────────

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        // Feedback: hit reaction sound
        soundEmitter?.Play(SoundOnHit);

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


        Destroy(gameObject);
    }

    // ── Facing ────────────────────────────────────────────────────────────────

    private void UpdateFacingDirection()
    {
        Vector3 velocity = navAgent.velocity;
        velocity.y = 0f;

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
