using UnityEngine;
using System.Collections;

public class RangedWeapon : BaseWeapon
{
    [Header("Raycast")]
    public Transform shootPoint;
    public LayerMask hitLayers;
    public Camera playerCamera;

    private bool isReloading;

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

        if (playerCamera == null)
            playerCamera = Camera.main;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        if (Physics.Raycast(ray, out RaycastHit hit, weaponData.range, hitLayers))
        {
            IDamageable damageable = hit.collider.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(weaponData.damage);
            }

            if (weaponData.hitEffectPrefab != null)
            {
                Instantiate(weaponData.hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
            }
        }

        if (weaponData.muzzleFlashPrefab != null && shootPoint != null)
        {
            Instantiate(weaponData.muzzleFlashPrefab, shootPoint.position, shootPoint.rotation);
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

        StartCoroutine(ReloadCoroutine());
    }

    private IEnumerator ReloadCoroutine()
    {
        isReloading = true;
        yield return new WaitForSeconds(weaponData.reloadTime);

        weaponData.currentAmmo = weaponData.maxAmmo;
        RaiseAmmoChanged(weaponData.currentAmmo);
        isReloading = false;
    }
}