using UnityEngine;

/// <summary>
/// Destructible generator object that sustains a force field.
/// Implements IDamageable with HitFlash feedback (no knockback).
/// Fires a GameplayEventSO on death and notifies its linked ElectricArcRenderer.
/// </summary>
public class GeneratorObject : MonoBehaviour, IDamageable
{
    [Header("Health")]
    [Tooltip("Maximum hit points of the generator.")]
    [SerializeField] private float maxHealth = 100f;

    [Header("On Death Event")]
    [Tooltip("GameplayEventSO to fire when the generator is destroyed.")]
    [SerializeField] private GameplayEventSO onDeathEvent;

    [Header("VFX")]
    [Tooltip("Particle prefab spawned at the generator's position on death.")]
    [SerializeField] private GameObject deathVfxPrefab;

    [Header("Arc")]
    [Tooltip("The ElectricArcRenderer linking this generator to the force field. Disabled on death.")]
    [SerializeField] private ElectricArcRenderer electricArc;

    private const string SoundOnHit = "OnHit";
    private const string SoundOnDeath = "OnDeath";

    private float currentHealth;
    private bool isDead;
    private EnemyHitFlash hitFlash;
    private SoundEmitter soundEmitter;

    private void Awake()
    {
        hitFlash = GetComponent<EnemyHitFlash>();
        soundEmitter = GetComponent<SoundEmitter>();
    }

    private void Start()
    {
        currentHealth = maxHealth;
    }

    /// <summary>
    /// Applies damage to the generator. Triggers HitFlash on each hit, no knockback.
    /// </summary>
    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        soundEmitter?.Play(SoundOnHit);
        hitFlash?.Flash();

        if (currentHealth <= 0f)
            Die();
    }

    private void Die()
    {
        isDead = true;

        soundEmitter?.Play(SoundOnDeath);

        if (deathVfxPrefab != null)
            Instantiate(deathVfxPrefab, transform.position, Quaternion.identity);

        if (electricArc != null)
            electricArc.Deactivate();

        onDeathEvent?.Execute(this);

        Destroy(gameObject);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
#endif
}
