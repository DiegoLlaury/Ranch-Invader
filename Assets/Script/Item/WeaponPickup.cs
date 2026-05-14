using UnityEngine;

/// <summary>
/// Place this component on a weapon pickup object in the world.
/// When the player interacts with it, the weapon is added to their WeaponInventory
/// and equipped immediately via WeaponController.
/// </summary>
public class WeaponPickup : MonoBehaviour, IInteractable
{
    [Header("Configuration")]
    [SerializeField] private WeaponType weaponType;
    [SerializeField] private string interactionLabel = "Pick up";

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
            Debug.LogWarning("[WeaponPickup] No WeaponInventory found on interactor.");
            return;
        }

        inventory.AddWeapon(weaponType);

        if (controller != null)
            controller.UnlockWeapon(weaponType);

        Destroy(gameObject);
    }
}
