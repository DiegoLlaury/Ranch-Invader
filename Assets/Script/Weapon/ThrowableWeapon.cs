using UnityEngine;
using System.Collections;

public class ThrowableWeapon : BaseWeapon
{
    [Header("Projectile")]
    public GameObject projectilePrefab;
    public Transform throwPoint;
    public float throwForce = 20f;
    public Camera playerCamera;

    [Header("Visuel en main")]
    [Tooltip("Le mesh de la fourche tenu en main — masqué quand les munitions sont épuisées")]
    public GameObject handMeshObject;

    private WeaponUIController cachedUIController;

    protected override void Awake()
    {
        base.Awake();
        cachedUIController = Object.FindAnyObjectByType<WeaponUIController>();
    }

    public override void OnEquip()
    {
        base.OnEquip();
        RefreshHandMesh();
    }

    public override void Attack()
    {
        if (weaponData.RuntimeCurrentAmmo <= 0)
        {
            soundEmitter?.Play(SoundOnEmpty);
            return;
        }
        if (!CanAttack()) return;
        base.Attack();
    }

    protected override void PerformAttack()
    {
        weaponData.RuntimeCurrentAmmo--;
        RaiseAmmoChanged(weaponData.RuntimeCurrentAmmo);
        ThrowProjectile();
        RefreshHandMesh();
    }

    /// <summary>
    /// Override : bloque isAttacking jusqu'à la fin de l'animation UI,
    /// empêchant tout lancer pendant qu'elle joue.
    /// </summary>
    protected override void ResetAttackState()
    {
        // Ne remet pas isAttacking à false immédiatement — attend la fin de l'anim UI
        StartCoroutine(WaitForAnimationThenReset());
    }

    private IEnumerator WaitForAnimationThenReset()
    {
        if (cachedUIController == null)
            cachedUIController = Object.FindAnyObjectByType<WeaponUIController>();

        // Attend une frame pour laisser l'animation démarrer
        yield return null;

        // Attend que l'animation UI soit terminée
        while (cachedUIController != null && cachedUIController.IsAnimating)
            yield return null;

        isAttacking = false;
    }

    private void ThrowProjectile()
    {
        if (projectilePrefab == null || throwPoint == null) return;

        if (playerCamera == null)
            playerCamera = Camera.main;

        Vector3 throwDirection = playerCamera.transform.forward;
        GameObject projectile = Instantiate(
            projectilePrefab,
            throwPoint.position,
            Quaternion.LookRotation(throwDirection)
        );

        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb != null)
            rb.linearVelocity = throwDirection * throwForce;

        Projectile projectileScript = projectile.GetComponent<Projectile>();
        if (projectileScript != null)
            projectileScript.damage = weaponData.damage;
    }

    private void RefreshHandMesh()
    {
        if (handMeshObject != null)
            handMeshObject.SetActive(weaponData.RuntimeCurrentAmmo > 0);
    }
}
