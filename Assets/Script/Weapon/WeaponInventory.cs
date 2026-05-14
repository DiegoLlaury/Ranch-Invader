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

    /// <summary>Adds a weapon type to the inventory if not already present.</summary>
    public void AddWeapon(WeaponType weaponType)
    {
        if (unlockedWeapons.Contains(weaponType))
        {
            Debug.Log($"[WeaponInventory] {weaponType} already in inventory.");
            return;
        }

        unlockedWeapons.Add(weaponType);
        Debug.Log($"[WeaponInventory] {weaponType} added to inventory.");
        OnWeaponAdded?.Invoke(weaponType);
    }

    /// <summary>Returns true if the player owns the given weapon type.</summary>
    public bool HasWeapon(WeaponType weaponType) => unlockedWeapons.Contains(weaponType);

    /// <summary>Returns a copy of the current unlocked weapon set.</summary>
    public IReadOnlyCollection<WeaponType> GetUnlockedWeapons() => unlockedWeapons;
}
