using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float damage = 15f;
    public float lifetime = 5f;
    public GameObject impactEffect;
    public LayerMask hitLayers;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & hitLayers) != 0)
        {
            IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
            }

            if (impactEffect != null)
            {
                Instantiate(impactEffect, collision.contacts[0].point, Quaternion.LookRotation(collision.contacts[0].normal));
            }

            Destroy(gameObject);
        }
    }
}
