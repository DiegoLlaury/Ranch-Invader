using UnityEngine;
using System;

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

    protected float lastAttackTime;
    protected bool isAttacking;
    protected DrunkEffect drunkEffect;
    protected SoundEmitter soundEmitter;

    protected virtual void Awake()
    {
        drunkEffect = GetComponentInParent<DrunkEffect>();
        soundEmitter = GetComponent<SoundEmitter>();

        // Mémorise les valeurs d'ammo définies dans l'asset avant toute modification runtime
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

        lastAttackTime = Time.time;
        isAttacking = true;

        // Feedback: swing sound
        soundEmitter?.Play(SoundOnSwing);

        PerformAttack();
        RaiseAttackPerformed();

        Invoke(nameof(ResetAttackState), weaponData.animationDuration);
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

    public virtual void OnUnequip() { }

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
        RaiseAmmoChanged(weaponData.currentAmmo);
    }
}
