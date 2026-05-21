using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Pickup de munitions. Recharge toutes les armes de l'inventaire du joueur
/// d'un pourcentage de leur capacité maximale (arrondi au supérieur).
/// Pour le fusil : augmente uniquement la réserve (maxAmmo) sans toucher au chargeur.
/// Pour les autres armes : augmente directement currentAmmo.
/// </summary>
public class AmmoPickup : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Pourcentage de la capacité maximale restaurée (0.5 = 50%)")]
    [Range(0.01f, 1f)]
    [SerializeField] private float refillPercent = 0.5f;

    [Tooltip("Jouer un son au ramassage (nom dans SoundManager)")]
    [SerializeField] private string pickupSoundName = "AmmoPickup";

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        WeaponController controller = other.GetComponent<WeaponController>();
        WeaponInventory inventory = other.GetComponent<WeaponInventory>();

        if (controller == null || inventory == null) return;

        bool anyRefilled = RefillAllWeapons(controller, inventory);

        if (!anyRefilled) return;

        SoundManager.Instance?.PlaySound2D(pickupSoundName);
        Destroy(gameObject);
    }

    private bool RefillAllWeapons(WeaponController controller, WeaponInventory inventory)
    {
        bool anyRefilled = false;

        foreach (WeaponType weaponType in inventory.GetUnlockedWeapons())
        {
            WeaponData data = controller.GetWeaponData(weaponType);
            if (data == null) continue;

            // Le fusil gère une réserve séparée de son chargeur
            bool isReserveOnly = weaponType == WeaponType.Shotgun;

            int added = data.AddAmmoByPercent(refillPercent, isReserveOnly);

            if (added > 0)
            {
                anyRefilled = true;

                // Notifie l'UI si c'est l'arme actuellement équipée
                if (controller.GetCurrentWeaponType() == weaponType)
                {
                    BaseWeapon weapon = controller.GetCurrentWeapon();
                    weapon?.OnAmmoRestored();
                    weapon?.NotifyAmmoChanged();
                }
            }
        }

        return anyRefilled;
    }
}
