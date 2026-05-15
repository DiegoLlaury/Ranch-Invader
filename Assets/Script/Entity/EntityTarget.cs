using UnityEngine;

/// <summary>
/// The cow is invulnerable but reacts to all hits with a knockback + flee.
/// Implements IDamageable so weapons can hit it, and IKnockbackable for directional impulse.
/// </summary>
[RequireComponent(typeof(CowKnockback))]
public class EntityTarget : MonoBehaviour, IDamageable, IKnockbackable
{
    private CowKnockback cowKnockback;

    private void Awake()
    {
        cowKnockback = GetComponent<CowKnockback>();
    }

    /// <summary>
    /// Called by weapons via IDamageable. The cow takes no real damage.
    /// Knockback direction defaults to in front of the cow since no source position is available here.
    /// </summary>
    public void TakeDamage(float damage)
    {
        // Direction fallback : repousse la vache dans sa direction de face
        Vector3 fallbackSource = transform.position - transform.forward;
        cowKnockback.ApplyKnockback(fallbackSource);
        SoundManager.Instance.PlaySoundAtTransform("Cow_Hurt", transform);
    }

    /// <summary>
    /// Called by weapons via IKnockbackable immediately after TakeDamage,
    /// providing the real hit source position for accurate knockback direction.
    /// </summary>
    public void ReceiveKnockback(Vector3 hitSourcePosition)
    {
        cowKnockback.ApplyKnockback(hitSourcePosition);
    }
}
