using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    [Header("Paramètres de Vie")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    [Header("Événements")]
    public UnityEvent<float> OnHealthChanged;
    public UnityEvent OnDeath;
    public UnityEvent OnRevive;

    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth / maxHealth);
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        OnHealthChanged?.Invoke(currentHealth / maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (isDead) return;

        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);

        OnHealthChanged?.Invoke(currentHealth / maxHealth);
    }

    private void Die()
    {
        isDead = true;
        OnDeath?.Invoke();
    }

    public void Revive()
    {
        isDead = false;
        currentHealth = maxHealth;
        OnRevive?.Invoke();
        OnHealthChanged?.Invoke(currentHealth / maxHealth);
    }

    public float GetHealthPercentage() => currentHealth / maxHealth;
    public bool IsDead() => isDead;
}
