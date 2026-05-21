using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tracks which weapons the player has unlocked at runtime.
/// Starts empty — weapons are added via WeaponPickup.
/// </summary>
public class WeaponInventory : MonoBehaviour
{
    private readonly HashSet<WeaponType> unlockedWeapons = new HashSet<WeaponType>();

    public event Action<WeaponType> OnWeaponAdded;
    [SerializeField] private PlayerDatas playerDatas;

    private void Start()
    {
        LoadWeaponsFromSave();
    }

    /// <summary>Adds a weapon type to the inventory if not already present.</summary>
    public void AddWeapon(WeaponType weaponType)
    {
        if (unlockedWeapons.Contains(weaponType))
        {
            Debug.Log($"[WeaponInventory] {weaponType} already in inventory.");
            return;
        }

        unlockedWeapons.Add(weaponType);

        // Sauvegarde dans les datas
        if (!playerDatas.Datas.unclockWeaponSave.Contains(weaponType.ToString()))
        {
            playerDatas.Datas.unclockWeaponSave.Add(weaponType.ToString());
        }
        Debug.Log($"[WeaponInventory] {weaponType} added to inventory.");

        OnWeaponAdded?.Invoke(weaponType);
    }

    private void LoadWeaponsFromSave()
    {
        if (playerDatas.Datas.unclockWeaponSave == null)
            return;

        foreach (string weaponName in playerDatas.Datas.unclockWeaponSave)
        {
            if (Enum.TryParse(weaponName, out WeaponType weaponType))
            {
                unlockedWeapons.Add(weaponType);

                // Important :
                // notifie les systèmes d'armes/UI
                OnWeaponAdded?.Invoke(weaponType);

                Debug.Log($"[WeaponInventory] Restored weapon : {weaponType}");
            }
        }
    }

    /// <summary>Returns true if the player owns the given weapon type.</summary>
    public bool HasWeapon(WeaponType weaponType) => unlockedWeapons.Contains(weaponType);

    /// <summary>Returns a copy of the current unlocked weapon set.</summary>
    public IReadOnlyCollection<WeaponType> GetUnlockedWeapons() => unlockedWeapons;
}
