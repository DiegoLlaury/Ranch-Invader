using UnityEngine;

public class MeleeWeapon : BaseWeapon
{
    [Header("Détection de collision")]
    public LayerMask enemyLayer;
    public Transform attackPoint;

    protected override void PerformAttack()
    {
        Collider[] hitEnemies = Physics.OverlapSphere(
            attackPoint != null ? attackPoint.position : transform.position,
            weaponData.range,
            enemyLayer
        );

        foreach (Collider enemy in hitEnemies)
        {
            DealDamage(enemy.gameObject);
        }

        if (weaponData.weaponType == WeaponType.Shovel)
        {
            ReduceDurability();
        }
    }

    protected virtual void DealDamage(GameObject target)
    {
        IDamageable damageable = target.GetComponent<IDamageable>();
        if (damageable != null)
        {
            float finalDamage = GetFinalDamage();
            damageable.TakeDamage(finalDamage);

            if (drunkEffect != null && drunkEffect.IsDrunk())
            {
                Debug.Log($"Dégâts avec bonus bourré ! Dégâts de base: {weaponData.damage}, Dégâts finaux: {finalDamage}");
            }
        }

        if (weaponData.hitEffectPrefab != null)
        {
            Instantiate(weaponData.hitEffectPrefab, target.transform.position, Quaternion.identity);
        }
    }

    protected void ReduceDurability()
    {
        weaponData.currentDurability -= weaponData.durabilityLossPerHit;
        weaponData.currentDurability = Mathf.Max(0, weaponData.currentDurability);

        RaiseDurabilityChanged(weaponData.currentDurability / weaponData.maxDurability);

        if (weaponData.currentDurability <= 0)
        {
            RaiseWeaponBroken();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, weaponData.range);
        }
    }
}
