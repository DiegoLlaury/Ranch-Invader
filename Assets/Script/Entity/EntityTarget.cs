using UnityEngine;

public class EntityTarget : MonoBehaviour, IDamageable
{
    [SerializeField] private AudioSource audioHurt;
    [SerializeField] private AudioSource audioDeath;
    // need animation of hurt on entity 

    [SerializeField] private float maxHealth;
    [SerializeField] private float health;

    private void Start()
    {
        health = maxHealth;
    }

    void IDamageable.TakeDamage(float damage)
    {
        Debug.Log("Hit!");
        health -= damage;

        if (health <= 0)
        {
            if (audioDeath != null)
                audioDeath.Play();

            Destroy(gameObject);  
            return;
        }

        if (audioHurt != null)
        audioHurt.Play();
    }
}
