using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Listens to InteractionSystem events and displays an interaction prompt
/// (key image + optional label) when the player looks at an interactable object.
/// </summary>
public class InteractionPromptUI : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private InteractionSystem interactionSystem;

    [Header("UI Elements")]
    [SerializeField] private GameObject promptRoot;
    [SerializeField] private Image keyImage;
    [SerializeField] private TextMeshProUGUI interactionLabel;

    [Header("Touche à afficher")]
    [SerializeField] private Sprite keySprite;

    private void Awake()
    {
        // Masque le prompt au démarrage
        SetPromptVisible(false);
    }

    private void OnEnable()
    {
        if (interactionSystem == null) return;

        interactionSystem.OnInteractableFound += HandleInteractableFound;
        interactionSystem.OnInteractableLost += HandleInteractableLost;
    }

    private void OnDisable()
    {
        if (interactionSystem == null) return;

        interactionSystem.OnInteractableFound -= HandleInteractableFound;
        interactionSystem.OnInteractableLost -= HandleInteractableLost;
    }

    private void HandleInteractableFound(IInteractable interactable)
    {
        if (keyImage != null && keySprite != null)
            keyImage.sprite = keySprite;

        if (interactionLabel != null)
            interactionLabel.text = interactable.InteractionLabel;

        SetPromptVisible(true);
    }

    private void HandleInteractableLost()
    {
        SetPromptVisible(false);
    }

    private void SetPromptVisible(bool visible)
    {
        if (promptRoot != null)
            promptRoot.SetActive(visible);
    }
}
