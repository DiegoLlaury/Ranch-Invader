using UnityEngine;

/// <summary>
/// Interface to implement on any object the player can interact with via the InteractionSystem.
/// </summary>
public interface IInteractable
{
    /// <summary>Label displayed in the interaction prompt UI.</summary>
    string InteractionLabel { get; }

    /// <summary>Called when the player successfully interacts with this object.</summary>
    void OnInteract(GameObject interactor);

    /// <summary>Returns whether the interaction is currently available.</summary>
    bool CanInteract(GameObject interactor);
}
