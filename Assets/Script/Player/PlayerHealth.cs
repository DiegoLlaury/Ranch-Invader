using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    [Header("Paramètres de Vie")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    [Header("Invincibilité")]
    [Tooltip("Durée d'invincibilité en secondes après avoir reçu un coup.")]
    [SerializeField] private float damageCooldown = 0.5f;
    private float lastDamageTime = -999f;

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
        if (Time.time - lastDamageTime < damageCooldown) return;

        lastDamageTime = Time.time;
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        SoundManager.Instance?.PlaySound2D("Player_Hurt");

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
        VoiceManager.Instance?.PlayVoiceForced("Voice_Death", VoicePriority.Objective);
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

    /// <summary>Returns the current health as an integer value.</summary>
    public int GetCurrentHealth() => Mathf.CeilToInt(currentHealth);

    /// <summary>Returns the maximum health as an integer value.</summary>
    public int GetMaxHealth() => Mathf.RoundToInt(maxHealth);
}
