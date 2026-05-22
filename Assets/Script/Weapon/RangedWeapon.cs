using UnityEngine;
using System.Collections;

public class RangedWeapon : BaseWeapon
{
    [Header("Raycast")]
    public Transform shootPoint;
    public LayerMask hitLayers;
    public Camera playerCamera;

    private bool isReloading;
    private WeaponUIController cachedUIController;

    protected override void Awake()
    {
        base.Awake();
        cachedUIController = Object.FindAnyObjectByType<WeaponUIController>();
    }

    public override void Attack()
    {
        if (isReloading) return;

        // Bloque aussi pendant que l'animation UI joue (couvre la fenêtre fin de reload)
        if (cachedUIController != null && cachedUIController.IsAnimating) return;

        if (weaponData.RuntimeCurrentAmmo <= 0)
        {
            int reserve = weaponData.RuntimeMaxAmmo - weaponData.RuntimeCurrentAmmo;
            if (reserve > 0)
                StartCoroutine(ReloadCoroutine());
            else
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
        Shoot();
    }

    /// <summary>
    /// Bloque isAttacking jusqu'à la fin de l'animation UI, comme ThrowableWeapon.
    /// Empêche un second tir de partir pendant que l'animation du premier joue.
    /// </summary>
    protected override void ResetAttackState()
    {
        StartCoroutine(WaitForAnimationThenReset());
    }

    private IEnumerator WaitForAnimationThenReset()
    {
        if (cachedUIController == null)
            cachedUIController = Object.FindAnyObjectByType<WeaponUIController>();

        yield return null;

        float timeout = Mathf.Max(weaponData.animationDuration + 0.5f, 1f);
        float elapsed = 0f;

        while (cachedUIController != null &&
               cachedUIController.IsAnimating &&
               elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        isAttacking = false;
    }

    private void Shoot()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit, weaponData.range, hitLayers, QueryTriggerInteraction.Collide))
        {
            IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>();
            if (damageable != null)
                damageable.TakeDamage(GetFinalDamage());

            // Transmit shooter position for directional knockback
            IKnockbackable knockbackable = hit.collider.GetComponentInParent<IKnockbackable>();
            knockbackable?.ReceiveKnockback(shootPoint != null ? shootPoint.position : transform.position);

            if (weaponData.hitEffectPrefab != null)
                Instantiate(weaponData.hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
        }


        if (weaponData.muzzleFlashPrefab != null && shootPoint != null)
            Instantiate(weaponData.muzzleFlashPrefab, shootPoint.position, shootPoint.rotation);
    }

    private IEnumerator ReloadCoroutine()
    {
        isReloading = true;
        cachedUIController?.PlayReloadAnimation();
        soundEmitter?.Play(SoundOnReload);
        yield return new WaitForSeconds(weaponData.reloadTime);

        int toReload = Mathf.Min(weaponData.ammoPerReload,
                                 weaponData.RuntimeMaxAmmo - weaponData.RuntimeCurrentAmmo);
        weaponData.RuntimeCurrentAmmo += toReload;
        weaponData.RuntimeMaxAmmo -= toReload;

        RaiseAmmoChanged(weaponData.RuntimeCurrentAmmo);
        isReloading = false;
    }
}
