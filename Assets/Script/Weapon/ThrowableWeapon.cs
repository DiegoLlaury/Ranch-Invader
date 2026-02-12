using UnityEngine;
using System.Collections;

public class ThrowableWeapon : BaseWeapon
{
    [Header("Projectile")]
    public GameObject projectilePrefab;
    public Transform throwPoint;
    public float throwForce = 20f;

    public Camera playerCamera;

    private bool isReloading;
    private WeaponUIController cachedUIController;

    protected override void Awake()
    {
        base.Awake();
        cachedUIController = Object.FindFirstObjectByType<WeaponUIController>();
    }

    public override void Attack()
    {
        if (weaponData.currentAmmo <= 0)
        {
            Reload();
            return;
        }

        if (!CanAttack() || isReloading)
            return;

        base.Attack();
    }

    protected override void PerformAttack()
    {
        weaponData.currentAmmo--;
        RaiseAmmoChanged(weaponData.currentAmmo);

        if (projectilePrefab != null && throwPoint != null)
        {
            if (playerCamera == null)
                playerCamera = Camera.main;

            Vector3 throwDirection = playerCamera.transform.forward;

            GameObject projectile = Instantiate(projectilePrefab, throwPoint.position, Quaternion.LookRotation(throwDirection));

            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = throwDirection * throwForce;
            }

            Projectile projectileScript = projectile.GetComponent<Projectile>();
            if (projectileScript != null)
            {
                projectileScript.damage = weaponData.damage;
            }
        }

        if (weaponData.currentAmmo <= 0)
        {
            Invoke(nameof(Reload), weaponData.animationDuration);
        }
    }

    public void Reload()
    {
        if (isReloading || weaponData.currentAmmo == weaponData.maxAmmo)
            return;

        CancelInvoke(nameof(Reload));
        StartCoroutine(ReloadCoroutine());
    }

    private IEnumerator ReloadCoroutine()
    {
        isReloading = true;

        if (cachedUIController == null)
        {
            cachedUIController = Object.FindFirstObjectByType<WeaponUIController>();
        }

        if (cachedUIController != null)
        {
            cachedUIController.PlayReloadAnimation();
        }

        yield return new WaitForSeconds(weaponData.reloadTime);

        weaponData.currentAmmo = weaponData.maxAmmo;
        RaiseAmmoChanged(weaponData.currentAmmo);
        isReloading = false;
    }
}
