using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections.Generic;

public class WeaponController : MonoBehaviour
{
    [Header("Armes disponibles")]
    public List<WeaponData> availableWeapons = new List<WeaponData>();

    [Header("Armes GameObjects")]
    public GameObject fistWeaponObject;
    public GameObject shovelWeaponObject;
    public GameObject shotgunWeaponObject;
    public GameObject pitchforkWeaponObject;

    private Dictionary<WeaponType, BaseWeapon> weaponInstances = new Dictionary<WeaponType, BaseWeapon>();
    private WeaponType currentWeaponType = WeaponType.Fist;
    private BaseWeapon currentWeapon;

    public event Action<WeaponType> OnWeaponChanged;
    public event Action OnAttackTriggered;

    private void Start()
    {
        InitializeWeapons();
        SwitchWeapon(WeaponType.Fist);
    }

    private void InitializeWeapons()
    {
        if (fistWeaponObject != null)
        {
            BaseWeapon fist = fistWeaponObject.GetComponent<BaseWeapon>();
            if (fist != null) weaponInstances[WeaponType.Fist] = fist;
        }

        if (shovelWeaponObject != null)
        {
            BaseWeapon shovel = shovelWeaponObject.GetComponent<BaseWeapon>();
            if (shovel != null) weaponInstances[WeaponType.Shovel] = shovel;
        }

        if (shotgunWeaponObject != null)
        {
            BaseWeapon shotgun = shotgunWeaponObject.GetComponent<BaseWeapon>();
            if (shotgun != null) weaponInstances[WeaponType.Shotgun] = shotgun;
        }

        if (pitchforkWeaponObject != null)
        {
            BaseWeapon pitchfork = pitchforkWeaponObject.GetComponent<BaseWeapon>();
            if (pitchfork != null) weaponInstances[WeaponType.Pitchfork] = pitchfork;
        }

        foreach (var weapon in weaponInstances.Values)
        {
            if (weapon != null)
            {
                weapon.gameObject.SetActive(false);
                weapon.OnAttackPerformed += HandleAttackPerformed;
            }
        }
    }

    public void SwitchWeapon(WeaponType newWeaponType)
    {
        if (newWeaponType == currentWeaponType)
            return;

        if (currentWeapon != null)
        {
            currentWeapon.OnUnequip();
            currentWeapon.gameObject.SetActive(false);
        }

        currentWeaponType = newWeaponType;

        if (weaponInstances.ContainsKey(newWeaponType))
        {
            currentWeapon = weaponInstances[newWeaponType];
            currentWeapon.gameObject.SetActive(true);
            currentWeapon.OnEquip();

            OnWeaponChanged?.Invoke(newWeaponType);
        }
    }

    public void NextWeapon()
    {
        int nextIndex = ((int)currentWeaponType + 1) % System.Enum.GetValues(typeof(WeaponType)).Length;
        SwitchWeapon((WeaponType)nextIndex);
    }

    public void PreviousWeapon()
    {
        int prevIndex = ((int)currentWeaponType - 1);
        if (prevIndex < 0)
            prevIndex = System.Enum.GetValues(typeof(WeaponType)).Length - 1;
        SwitchWeapon((WeaponType)prevIndex);
    }

    public void Attack()
    {
        if (currentWeapon != null && currentWeapon.CanAttack())
        {
            currentWeapon.Attack();
        }
    }

    private void HandleAttackPerformed()
    {
        OnAttackTriggered?.Invoke();
    }

    public BaseWeapon GetCurrentWeapon()
    {
        return currentWeapon;
    }

    public WeaponType GetCurrentWeaponType()
    {
        return currentWeaponType;
    }

    public WeaponData GetWeaponData(WeaponType type)
    {
        foreach (var data in availableWeapons)
        {
            if (data.weaponType == type)
                return data;
        }
        return null;
    }

    public void OnWeapon1(InputAction.CallbackContext context)
    {
        if (context.performed) SwitchWeapon(WeaponType.Fist);
    }

    public void OnWeapon2(InputAction.CallbackContext context)
    {
        if (context.performed) SwitchWeapon(WeaponType.Shovel);
    }

    public void OnWeapon3(InputAction.CallbackContext context)
    {
        if (context.performed) SwitchWeapon(WeaponType.Shotgun);
    }

    public void OnWeapon4(InputAction.CallbackContext context)
    {
        if (context.performed) SwitchWeapon(WeaponType.Pitchfork);
    }

    public void OnAttackInput(InputAction.CallbackContext context)
    {
        if (context.performed) Attack();
    }

    public void OnNextWeaponInput(InputAction.CallbackContext context)
    {
        if (context.performed) NextWeapon();
    }

    public void OnPreviousWeaponInput(InputAction.CallbackContext context)
    {
        if (context.performed) PreviousWeapon();
    }

    private void OnDestroy()
    {
        foreach (var weapon in weaponInstances.Values)
        {
            if (weapon != null)
            {
                weapon.OnAttackPerformed -= HandleAttackPerformed;
            }
        }
    }
}
