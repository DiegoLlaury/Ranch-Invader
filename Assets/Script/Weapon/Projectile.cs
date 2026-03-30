using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float damage = 15f;
    public float lifetime = 5f;
    public GameObject impactEffect;
    public LayerMask hitLayers;

    public bool stickToTarget = false;
    public float stickDuration = 3f;
    private bool isStuck = false;


    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isStuck) return;

        if (((1 << collision.gameObject.layer) & hitLayers) != 0)
        {
            // GetComponentInParent couvre le cas où la collision touche un enfant
            // (ex. ImpostorQuad) dont l'IDamageable est sur le root parent (EnemyBase).
            IDamageable damageable = collision.gameObject.GetComponentInParent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
            }

            if (impactEffect != null)
            {
                Instantiate(impactEffect, collision.contacts[0].point, Quaternion.LookRotation(collision.contacts[0].normal));
            }

            if (stickToTarget)
            {
                StickToTarget(collision);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    private void StickToTarget(Collision collision)
    {
        isStuck = true;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        transform.SetParent(collision.transform);

        Destroy(gameObject, stickDuration);
    }


}
