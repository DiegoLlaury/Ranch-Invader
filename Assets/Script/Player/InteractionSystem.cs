using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Casts a ray from the camera each frame to detect nearby IInteractable objects.
/// Fires events when a target is found, lost, or interacted with.
/// </summary>
public class InteractionSystem : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private float interactionRange = 2.5f;
    [SerializeField] private LayerMask interactionLayerMask = ~0;
    [SerializeField] private Transform cameraTransform;

    public event Action<IInteractable> OnInteractableFound;
    public event Action OnInteractableLost;
    public event Action<IInteractable> OnInteracted;

    private IInteractable currentTarget;
    private PlayerInput playerInput;
    private InputAction interactAction;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

        if (playerInput != null)
        {
            // Cherche dans la map "Player" explicitement pour éviter toute ambiguïté
            InputActionMap playerMap = playerInput.actions.FindActionMap("Player", throwIfNotFound: true);
            interactAction = playerMap.FindAction("Interact", throwIfNotFound: true);
        }

        if (cameraTransform == null)
            cameraTransform = Camera.main != null ? Camera.main.transform : transform;
    }

    private void OnEnable()
    {
        if (interactAction != null)
            interactAction.performed += OnInteractPerformed;
    }

    private void OnDisable()
    {
        if (interactAction != null)
            interactAction.performed -= OnInteractPerformed;
    }

    private void Update()
    {
        DetectInteractable();
    }

    private void DetectInteractable()
    {
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        bool hitSomething = Physics.Raycast(ray, out RaycastHit hit, interactionRange, interactionLayerMask);

        IInteractable detected = null;

        if (hitSomething)
            detected = hit.collider.GetComponent<IInteractable>();

        if (detected != null && detected.CanInteract(gameObject))
        {
            if (!ReferenceEquals(detected, currentTarget))
            {
                currentTarget = detected;
                OnInteractableFound?.Invoke(currentTarget);
            }
        }
        else
        {
            if (currentTarget != null)
            {
                currentTarget = null;
                OnInteractableLost?.Invoke();
            }
        }
    }

    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        if (currentTarget == null || !currentTarget.CanInteract(gameObject))
            return;

        currentTarget.OnInteract(gameObject);
        OnInteracted?.Invoke(currentTarget);
    }

    private void OnDrawGizmosSelected()
    {
        if (cameraTransform == null) return;
        Gizmos.color = currentTarget != null ? Color.green : Color.yellow;
        Gizmos.DrawRay(cameraTransform.position, cameraTransform.forward * interactionRange);
    }
}
