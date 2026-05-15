using UnityEngine;

/// <summary>
/// Detects player contact via the cow's trigger collider and triggers the knockback + flee.
/// </summary>
[RequireComponent(typeof(CowKnockback))]
public class CowPlayerContact : MonoBehaviour
{
    private const string PlayerTag = "Player";

    private CowKnockback cowKnockback;

    private void Awake()
    {
        cowKnockback = GetComponent<CowKnockback>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(PlayerTag)) return;

        cowKnockback.ApplyKnockback(other.transform.position);
        SoundManager.Instance.PlaySoundAtTransform("Cow_Hurt", transform);
    }
}
