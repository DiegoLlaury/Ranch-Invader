using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections.Generic;

public class WeaponController : MonoBehaviour
{
    [Header("Configuration des armes")]
    public List<WeaponData> availableWeapons = new List<WeaponData>();

    [Header("GameObjects des armes")]
    public GameObject fistWeaponObject;
    public GameObject shovelWeaponObject;
    public GameObject shotgunWeaponObject;
    public GameObject pitchforkWeaponObject;

    private Dictionary<WeaponType, BaseWeapon> weaponInstances = new Dictionary<WeaponType, BaseWeapon>();
    private WeaponType currentWeaponType = WeaponType.Fist;
    private BaseWeapon currentWeapon;

    public event Action<WeaponType> OnWeaponChanged;
    public event Action OnAttackTriggered;

    private PlayerInput playerInput;
    private InputAction weapon1Action;
    private InputAction weapon2Action;
    private InputAction weapon3Action;
    private InputAction weapon4Action;
    private InputAction attackAction;
    private InputAction nextWeaponAction;
    private InputAction previousWeaponAction;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

        if (playerInput != null)
        {
            weapon1Action = playerInput.actions["Weapon1"];
            weapon2Action = playerInput.actions["Weapon2"];
            weapon3Action = playerInput.actions["Weapon3"];
            weapon4Action = playerInput.actions["Weapon4"];
            attackAction = playerInput.actions["Attack"];
            nextWeaponAction = playerInput.actions["NextWeapon"];
            previousWeaponAction = playerInput.actions["PreviousWeapon"];
        }
    }

    private void OnEnable()
    {
        if (weapon1Action != null) weapon1Action.performed += OnWeapon1Performed;
        if (weapon2Action != null) weapon2Action.performed += OnWeapon2Performed;
        if (weapon3Action != null) weapon3Action.performed += OnWeapon3Performed;
        if (weapon4Action != null) weapon4Action.performed += OnWeapon4Performed;
        if (attackAction != null) attackAction.performed += OnAttackPerformed;
        if (nextWeaponAction != null) nextWeaponAction.performed += OnNextWeaponPerformed;
        if (previousWeaponAction != null) previousWeaponAction.performed += OnPreviousWeaponPerformed;
    }

    private void OnDisable()
    {
        if (weapon1Action != null) weapon1Action.performed -= OnWeapon1Performed;
        if (weapon2Action != null) weapon2Action.performed -= OnWeapon2Performed;
        if (weapon3Action != null) weapon3Action.performed -= OnWeapon3Performed;
        if (weapon4Action != null) weapon4Action.performed -= OnWeapon4Performed;
        if (attackAction != null) attackAction.performed -= OnAttackPerformed;
        if (nextWeaponAction != null) nextWeaponAction.performed -= OnNextWeaponPerformed;
        if (previousWeaponAction != null) previousWeaponAction.performed -= OnPreviousWeaponPerformed;
    }

    private void Start()
    {
        InitializeWeapons();
        SwitchWeapon(WeaponType.Fist);
    }

    private void InitializeWeapons()
    {
        if (fistWeaponObject != null)
        {
            BaseWeapon weapon = fistWeaponObject.GetComponent<BaseWeapon>();
            if (weapon != null)
            {
                weaponInstances[WeaponType.Fist] = weapon;
                RegisterWeaponEvents(weapon);
            }
        }

        if (shovelWeaponObject != null)
        {
            BaseWeapon weapon = shovelWeaponObject.GetComponent<BaseWeapon>();
            if (weapon != null)
            {
                weaponInstances[WeaponType.Shovel] = weapon;
                RegisterWeaponEvents(weapon);
            }
        }

        if (shotgunWeaponObject != null)
        {
            BaseWeapon weapon = shotgunWeaponObject.GetComponent<BaseWeapon>();
            if (weapon != null)
            {
                weaponInstances[WeaponType.Shotgun] = weapon;
                RegisterWeaponEvents(weapon);
            }
        }

        if (pitchforkWeaponObject != null)
        {
            BaseWeapon weapon = pitchforkWeaponObject.GetComponent<BaseWeapon>();
            if (weapon != null)
            {
                weaponInstances[WeaponType.Pitchfork] = weapon;
                RegisterWeaponEvents(weapon);
            }
        }
    }

    private void RegisterWeaponEvents(BaseWeapon weapon)
    {
        weapon.OnAttackPerformed += HandleWeaponAttackPerformed;
    }

    private void HandleWeaponAttackPerformed()
    {
        OnAttackTriggered?.Invoke();
    }

    public void SwitchWeapon(WeaponType newWeaponType)
    {
        if (!weaponInstances.ContainsKey(newWeaponType))
        {
            Debug.LogWarning($"Arme {newWeaponType} non trouvée !");
            return;
        }

        if (currentWeapon != null)
        {
            currentWeapon.OnUnequip();
            currentWeapon.gameObject.SetActive(false);
        }

        currentWeaponType = newWeaponType;
        currentWeapon = weaponInstances[newWeaponType];

        if (currentWeapon != null)
        {
            currentWeapon.gameObject.SetActive(true);
            currentWeapon.OnEquip();
        }

        OnWeaponChanged?.Invoke(newWeaponType);
    }

    public void Attack()
    {
        if (currentWeapon != null && currentWeapon.CanAttack())
        {
            currentWeapon.Attack();
        }
    }

    public void NextWeapon()
    {
        int currentIndex = (int)currentWeaponType;
        int nextIndex = (currentIndex + 1) % 4;
        SwitchWeapon((WeaponType)nextIndex);
    }

    public void PreviousWeapon()
    {
        int currentIndex = (int)currentWeaponType;
        int previousIndex = currentIndex - 1;
        if (previousIndex < 0) previousIndex = 3;
        SwitchWeapon((WeaponType)previousIndex);
    }

    private void OnWeapon1Performed(InputAction.CallbackContext context)
    {
        SwitchWeapon(WeaponType.Fist);
    }

    private void OnWeapon2Performed(InputAction.CallbackContext context)
    {
        SwitchWeapon(WeaponType.Shovel);
    }

    private void OnWeapon3Performed(InputAction.CallbackContext context)
    {
        SwitchWeapon(WeaponType.Shotgun);
    }

    private void OnWeapon4Performed(InputAction.CallbackContext context)
    {
        SwitchWeapon(WeaponType.Pitchfork);
    }

    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        Attack();
    }

    private void OnNextWeaponPerformed(InputAction.CallbackContext context)
    {
        NextWeapon();
    }

    private void OnPreviousWeaponPerformed(InputAction.CallbackContext context)
    {
        PreviousWeapon();
    }

    public WeaponType GetCurrentWeaponType()
    {
        return currentWeaponType;
    }

    public BaseWeapon GetCurrentWeapon()
    {
        return currentWeapon;
    }

    public WeaponData GetWeaponData(WeaponType weaponType)
    {
        if (weaponInstances.ContainsKey(weaponType))
        {
            return weaponInstances[weaponType].weaponData;
        }
        return null;
    }
}
