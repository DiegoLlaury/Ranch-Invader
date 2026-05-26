using UnityEngine;
using System;
using System.Collections;

public abstract class BaseWeapon : MonoBehaviour
{
    public WeaponData weaponData;

    public event Action OnAttackPerformed;
    public event Action<float> OnDurabilityChanged;
    public event Action<int> OnAmmoChanged;
    public event Action OnWeaponBroken;

    // Sound event name constants
    public const string SoundOnSwing = "OnSwing";   // Attack triggered
    public const string SoundOnHit = "OnHit";     // Projectile/melee connected
    public const string SoundOnEquip = "OnEquip";   // Weapon equipped
    public const string SoundOnEmpty = "OnEmpty";   // No ammo / broken
    public const string SoundOnReload = "OnReload"; // Weapon reloading

    protected float lastAttackTime;
    protected bool isAttacking;
    protected DrunkEffect drunkEffect;
    protected SoundEmitter soundEmitter;

    private Coroutine resetCoroutine;
    private int attackVersion;

    protected virtual void Awake()
    {
        drunkEffect = GetComponentInParent<DrunkEffect>();
        soundEmitter = GetComponent<SoundEmitter>();

        // M�morise les valeurs d'ammo d�finies dans l'asset avant toute modification runtime
        weaponData?.InitializeRuntimeAmmo();
    }

    public bool CanAttack()
    {
        return Time.time >= lastAttackTime + weaponData.attackCooldown && !isAttacking;
    }

    protected float GetFinalDamage()
    {
        float baseDamage = weaponData.damage;

        if (drunkEffect != null && drunkEffect.IsDrunk())
            return baseDamage + drunkEffect.GetDamageBoost();

        return baseDamage;
    }

    public virtual void Attack()
    {
        if (!CanAttack()) return;

        attackVersion++; // invalide toutes anciennes coroutines
        int localVersion = attackVersion;

        lastAttackTime = Time.time;
        isAttacking = true;

        soundEmitter?.Play(SoundOnSwing);

        PerformAttack();
        RaiseAttackPerformed();

        if (resetCoroutine != null)
            StopCoroutine(resetCoroutine);

        resetCoroutine = StartCoroutine(ResetAttackStateDelayed(localVersion));
    }

    private IEnumerator ResetAttackStateDelayed(int version)
    {
        yield return new WaitForSeconds(weaponData.animationDuration);

        // si une nouvelle attaque a commencé entre temps → on ignore
        if (version != attackVersion)
            yield break;

        ResetAttackState();
    }

    private IEnumerator ResetAttackStateDelayed()
    {
        yield return new WaitForSeconds(weaponData.animationDuration);

        ResetAttackState();
    }

    protected abstract void PerformAttack();

    protected virtual void ResetAttackState()
    {
        isAttacking = false;
    }

    public virtual void OnEquip()
    {
        // Feedback: equip sound
        soundEmitter?.Play(SoundOnEquip);
    }

    public virtual void OnAmmoRestored()
    {
        isAttacking = false;
    }

    public virtual void OnUnequip()
    {
        isAttacking = false;

        if (resetCoroutine != null)
        {
            StopCoroutine(resetCoroutine);
            resetCoroutine = null;
        }
    }

    public WeaponType GetWeaponType() => weaponData.weaponType;

    protected void RaiseAttackPerformed() => OnAttackPerformed?.Invoke();
    protected void RaiseDurabilityChanged(float v) => OnDurabilityChanged?.Invoke(v);
    protected void RaiseAmmoChanged(int v) => OnAmmoChanged?.Invoke(v);
    protected void RaiseWeaponBroken()
    {
        soundEmitter?.Play(SoundOnEmpty);
        OnWeaponBroken?.Invoke();
    }

    public void NotifyAmmoChanged()
    {
        RaiseAmmoChanged(weaponData.RuntimeCurrentAmmo);
    }
}
