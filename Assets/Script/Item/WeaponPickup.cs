using UnityEngine;

/// <summary>
/// Place this component on a weapon pickup object in the world.
/// When the player interacts with it, the weapon is added to their WeaponInventory,
/// its ammo is reset to initial values, and it is equipped immediately.
/// Fires a GameplayEventSO on pickup and optionally plays a voice line via SoundManager.
/// </summary>
public class WeaponPickup : MonoBehaviour, IInteractable
{
    [Header("Configuration")]
    [SerializeField] private WeaponType weaponType;
    [SerializeField] private string interactionLabel = "Ramasser";

    [Header("Son de voix au ramassage")]
    [Tooltip("Nom du son enregistré dans la SoundDatabase à jouer au ramassage (voix)")]
    [SerializeField] private string pickupVoiceSoundName = "";

    [Header("Événement au ramassage")]
    [Tooltip("GameplayEventSO déclenché après que le joueur ait ramassé l'arme")]
    [SerializeField] private GameplayEventSO onPickedUpEvent;

    public string InteractionLabel => interactionLabel;

    /// <summary>Returns true only if the player does not already own this weapon.</summary>
    public bool CanInteract(GameObject interactor)
    {
        WeaponInventory inventory = interactor.GetComponent<WeaponInventory>();
        return inventory != null && !inventory.HasWeapon(weaponType);
    }

    /// <summary>Adds the weapon to inventory, equips it, plays the voice line, and fires the pickup event.</summary>
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

        PlayPickupVoice();

        // On passe un MonoBehaviour persistant (depuis le joueur) comme caller
        // pour que l'event puisse s'exécuter même après la destruction du pickup.
        MonoBehaviour eventCaller = interactor.GetComponent<WeaponController>() as MonoBehaviour
            ?? interactor.GetComponent<MonoBehaviour>();
        onPickedUpEvent?.Execute(eventCaller);

        Destroy(gameObject);
    }

    /// <summary>Plays the configured voice sound through VoiceManager with Objective priority.</summary>
    private void PlayPickupVoice()
    {
        if (string.IsNullOrEmpty(pickupVoiceSoundName)) return;

        if (VoiceManager.Instance != null)
            VoiceManager.Instance.PlayVoiceForced(pickupVoiceSoundName, VoicePriority.Objective);
        else
            Debug.LogWarning("[WeaponPickup] VoiceManager introuvable — impossible de jouer la voix.");
    }
}
