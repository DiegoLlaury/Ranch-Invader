using UnityEngine;

/// <summary>
/// Interactable that fires a GameplayEventSO when the player interacts with it.
/// Assign any GameplayEventSO (LoadMapEventSO, UnityEventSO, ChainedEventSO…) in the Inspector.
/// </summary>
public class InteractableDoor : MonoBehaviour, IInteractable
{
    [Header("Interaction")]
    [SerializeField] private string interactionLabel = "Entrer";
    [SerializeField] private bool singleUse = true;

    [Header("Événement Gameplay")]
    [Tooltip("L'event déclenché lors de l'interaction. Supporte tous les GameplayEventSO.")]
    [SerializeField] private GameplayEventSO gameplayEvent;

    private bool hasBeenUsed = false;

    public string InteractionLabel => interactionLabel;

    public bool CanInteract(GameObject interactor)
    {
        return !singleUse || !hasBeenUsed;
    }

    public void OnInteract(GameObject interactor)
    {
        if (singleUse && hasBeenUsed) return;

        if (singleUse)
            hasBeenUsed = true;

        if (gameplayEvent != null)
            gameplayEvent.Execute(this);
        else
            Debug.LogWarning($"[InteractableDoor] Aucun GameplayEventSO assigné sur {gameObject.name}.");
    }
}
