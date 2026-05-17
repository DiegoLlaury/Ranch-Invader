using UnityEngine;

/// <summary>
/// Destructible generator object that sustains a force field.
/// Implements IDamageable with HitFlash feedback (no knockback).
/// Notifies a GeneratorGroupSO on death (shared event) and/or fires a solo GameplayEventSO.
/// </summary>
public class GeneratorObject : MonoBehaviour, IDamageable
{
    [Header("Health")]
    [Tooltip("Maximum hit points of the generator.")]
    [SerializeField] private float maxHealth = 100f;

    [Header("Group")]
    [Tooltip("Shared group SO. The group event fires only when all generators in the group are destroyed.")]
    [SerializeField] private GeneratorGroupSO generatorGroup;

    [Header("Solo Event (optional)")]
    [Tooltip("GameplayEventSO fired immediately when THIS generator is destroyed, regardless of the group.")]
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

        // Reset avant toute inscription — Awake est garanti avant Start sur tous les objets
        generatorGroup?.ResetCount();
    }

    private void Start()
    {
        currentHealth = maxHealth;
        generatorGroup?.Register();
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

        // Solo event — fires immediately on this generator's death
        onDeathEvent?.Execute(this);

        // Group event — fires only when all generators in the group are down
        generatorGroup?.NotifyDestroyed(this);

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
