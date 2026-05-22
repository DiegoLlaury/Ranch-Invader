using UnityEngine;

/// <summary>
/// Place this component on a weapon pickup object in the world.
/// When the player interacts with it, the weapon is added to their WeaponInventory,
/// its ammo is reset to initial values, and it is equipped immediately.
/// Fires one or more GameplayEventSO on pickup and optionally plays a voice line.
/// </summary>
public class WeaponPickup : MonoBehaviour, IInteractable
{
    [Header("Configuration")]
    [SerializeField] private WeaponType weaponType;
    [SerializeField] private string interactionLabel = "Ramasser";

    [Header("Son de voix au ramassage")]
    [Tooltip("Nom du son enregistré dans la SoundDatabase à jouer au ramassage (voix)")]
    [SerializeField] private string pickupVoiceSoundName = "";

    [Header("Événements au ramassage")]
    [Tooltip("GameplayEventSO déclenchés après que le joueur ait ramassé l'arme")]
    [SerializeField] private GameplayEventSO[] onPickedUpEvents;

    public string InteractionLabel => interactionLabel;

    /// <summary>
    /// Returns true only if the player does not already own this weapon.
    /// </summary>
    public bool CanInteract(GameObject interactor)
    {
        WeaponInventory inventory = interactor.GetComponent<WeaponInventory>();
        return inventory != null && !inventory.HasWeapon(weaponType);
    }

    /// <summary>
    /// Adds the weapon to inventory, equips it,
    /// plays the voice line, and fires pickup events.
    /// </summary>
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

        // Caller persistant pour les events async/coroutines
        MonoBehaviour eventCaller =
            interactor.GetComponent<WeaponController>() as MonoBehaviour
            ?? interactor.GetComponent<MonoBehaviour>();

        ExecutePickupEvents(eventCaller);

        Destroy(gameObject);
    }

    /// <summary>
    /// Exécute tous les GameplayEventSO configurés.
    /// </summary>
    private void ExecutePickupEvents(MonoBehaviour caller)
    {
        if (onPickedUpEvents == null || onPickedUpEvents.Length == 0)
            return;

        foreach (GameplayEventSO gameplayEvent in onPickedUpEvents)
        {
            if (gameplayEvent == null)
                continue;

            gameplayEvent.Execute(caller);
        }
    }

    /// <summary>
    /// Plays the configured voice sound through VoiceManager.
    /// </summary>
    private void PlayPickupVoice()
    {
        if (string.IsNullOrEmpty(pickupVoiceSoundName))
            return;

        if (VoiceManager.Instance != null)
        {
            VoiceManager.Instance.PlayVoiceForced(
                pickupVoiceSoundName,
                VoicePriority.Objective
            );
        }
        else
        {
            Debug.LogWarning(
                "[WeaponPickup] VoiceManager introuvable — impossible de jouer la voix."
            );
        }
    }
}