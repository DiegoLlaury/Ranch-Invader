using UnityEngine;

public class InteractableCat : MonoBehaviour, IInteractable
{
    // 
    [Header("Interaction")]
    [SerializeField] private string interactionLabel = "¨Pet";

    [Header("Sound")]
    public const string SoundOnInteract = "OnPet";

    protected SoundEmitter soundEmitter;
    public string InteractionLabel => interactionLabel;

    void Start()
    {
        soundEmitter = GetComponent<SoundEmitter>();
    }

    public bool CanInteract(GameObject interactor)
    {
        return true;
    }

    public void OnInteract(GameObject interactor)
    {
        soundEmitter?.Play(SoundOnInteract);
    }
}
