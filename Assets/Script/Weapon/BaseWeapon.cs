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

    public bool CanAttack()
    {
        return Time.time >= lastAttackTime + weaponData.attackCooldown && !isAttacking;
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
