using UnityEngine;

public class EntityTarget : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth;
    [SerializeField] private float health;

    private void Start()
    {
        health = maxHealth;
    }

    void IDamageable.TakeDamage(float damage)
    {
        health -= damage;

        if (health <= 0)
        {
            SoundManager.Instance.PlaySoundAtTransform("Cow_Death", transform);
            
            Destroy(gameObject);
            return;
        }

        SoundManager.Instance.PlaySoundAtTransform("Cow_Hurt", transform);
    }
}
