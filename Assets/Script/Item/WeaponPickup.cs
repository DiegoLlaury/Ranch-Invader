using UnityEngine;

/// <summary>
/// Place this component on a weapon pickup object in the world.
/// When the player interacts with it, the weapon is added to their WeaponInventory,
/// its ammo is reset to initial values, and it is equipped immediately.
/// </summary>
public class WeaponPickup : MonoBehaviour, IInteractable
{
    [Header("Configuration")]
    [SerializeField] private WeaponType weaponType;
    [SerializeField] private string interactionLabel = "Ramasser";

    public string InteractionLabel => interactionLabel;

    public bool CanInteract(GameObject interactor)
    {
        WeaponInventory inventory = interactor.GetComponent<WeaponInventory>();
        return inventory != null && !inventory.HasWeapon(weaponType);
    }

    public void OnInteract(GameObject interactor)
    {
        WeaponInventory inventory = interactor.GetComponent<WeaponInventory>();
        WeaponController controller = interactor.GetComponent<WeaponController>();

        if (inventory == null)
        {
            Debug.LogWarning("[WeaponPickup] Aucun WeaponInventory trouvé sur l'interacteur.");
            return;
        }

        inventory.AddWeapon(weaponType);

        // Remet les munitions à leur valeur initiale (asset) avant équipement
        if (controller != null)
        {
            WeaponData data = controller.GetWeaponData(weaponType);
            data?.ResetRuntimeAmmo();

            controller.UnlockWeapon(weaponType);
        }

        Destroy(gameObject);
    }
}
