using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [Header("Settings")]
    public float damage = 15f;
    public float lifetime = 6f;

    [Header("Detection")]
    public float detectionRadius = 0.2f;

    [Header("Layers")]
    public LayerMask hitLayers;

    [Header("Sprite Billboard")]
    public Transform spriteTransform;

    // Sound event name constants
    public const string SoundOnImpact = "OnImpact"; // Hit player or obstacle

    private Vector3 moveDirection;
    private float moveSpeed;
    private Transform playerTransform;
    private SoundEmitter soundEmitter;
    private bool hasHit;

    private void Awake()
    {
        soundEmitter = GetComponent<SoundEmitter>();
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            playerTransform = playerObj.transform;

        if (spriteTransform == null && transform.childCount > 0)
            spriteTransform = transform.GetChild(0);
    }

    /// <summary>
    /// Launches the projectile in the given direction at the given speed.
    /// </summary>
    public void Launch(Vector3 direction, float speed)
    {
        moveDirection = direction.normalized;
        moveSpeed = speed;
    }

    private void Update()
    {
        if (hasHit) return;

        Move();
        FaceSpriteTowardPlayer();
    }

    private void Move()
    {
        float stepDistance = moveSpeed * Time.deltaTime;

        if (Physics.SphereCast(transform.position, detectionRadius, moveDirection, out RaycastHit hit, stepDistance, hitLayers, QueryTriggerInteraction.Ignore))
        {
            hasHit = true;

            // Feedback: impact sound before destroy
            soundEmitter?.PlayAt(SoundOnImpact, hit.point);

            PlayerHealth playerHealth = hit.collider.GetComponent<PlayerHealth>();
            if (playerHealth != null)
                playerHealth.TakeDamage(damage);

            Destroy(gameObject);
            return;
        }

        transform.position += moveDirection * stepDistance;
    }

    private void FaceSpriteTowardPlayer()
    {
        if (spriteTransform == null || playerTransform == null) return;

        Vector3 toPlayer = playerTransform.position - spriteTransform.position;
        toPlayer.y = 0f;

        if (toPlayer.sqrMagnitude < 0.001f) return;

        spriteTransform.rotation = Quaternion.LookRotation(toPlayer.normalized);
    }
}
