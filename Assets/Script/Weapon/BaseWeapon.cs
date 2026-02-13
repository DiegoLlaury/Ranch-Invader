using UnityEngine;
using System;

public abstract class BaseWeapon : MonoBehaviour
{
    public WeaponData weaponData;

    public event Action OnAttackPerformed;
    public event Action<float> OnDurabilityChanged;
    public event Action<int> OnAmmoChanged;
    public event Action OnWeaponBroken;

    protected float lastAttackTime;
    protected bool isAttacking;
    protected DrunkEffect drunkEffect;

    protected virtual void Awake()
    {
        drunkEffect = GetComponentInParent<DrunkEffect>();
    }

    public bool CanAttack()
    {
        return Time.time >= lastAttackTime + weaponData.attackCooldown && !isAttacking;
    }

    protected float GetFinalDamage()
    {
        float baseDamage = weaponData.damage;

        if (drunkEffect != null && drunkEffect.IsDrunk())
        {
            float bonus = drunkEffect.GetDamageBoost();
            return baseDamage + bonus;
        }

        return baseDamage;
    }

    public virtual void Attack()
    {
        if (!CanAttack())
            return;

        lastAttackTime = Time.time;
        isAttacking = true;

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
    }

    public virtual void OnUnequip()
    {
    }

    public WeaponType GetWeaponType()
    {
        return weaponData.weaponType;
    }

    protected void RaiseAttackPerformed()
    {
        OnAttackPerformed?.Invoke();
    }

    protected void RaiseDurabilityChanged(float normalizedDurability)
    {
        OnDurabilityChanged?.Invoke(normalizedDurability);
    }

    protected void RaiseAmmoChanged(int currentAmmo)
    {
        OnAmmoChanged?.Invoke(currentAmmo);
    }

    protected void RaiseWeaponBroken()
    {
        OnWeaponBroken?.Invoke();
    }
}
