using UnityEngine;

/// <summary>
/// Zone de collision (trigger) qui déclenche des GameplayEventSO
/// quand le joueur entre, sort ou reste dans la zone.
/// Nécessite un BoxCollider en mode trigger sur le même GameObject.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class GameplayTriggerZone : MonoBehaviour
{
    [Header("Detection")]
    [Tooltip("Tag du GameObject déclencheur (généralement 'Player')")]
    public string triggerTag = "Player";

    [Tooltip("Si activé, la zone ne se déclenche qu'une seule fois")]
    public bool triggerOnce = true;

    [Header("Events")]
    [Tooltip("Événements déclenchés à l'entrée dans la zone")]
    public GameplayEventSO[] onEnterEvents;

    [Tooltip("Événements déclenchés à la sortie de la zone")]
    public GameplayEventSO[] onExitEvents;

    private bool hasTriggered = false;

    private void Awake()
    {
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        boxCollider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(triggerTag)) return;
        if (triggerOnce && hasTriggered) return;

        hasTriggered = true;
        FireEvents(onEnterEvents);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(triggerTag)) return;

        FireEvents(onExitEvents);
    }

    private void FireEvents(GameplayEventSO[] events)
    {
        foreach (GameplayEventSO gameplayEvent in events)
            gameplayEvent?.Execute(this);
    }

    /// <summary>Réinitialise la zone pour qu'elle puisse se déclencher à nouveau.</summary>
    public void Reset()
    {
        hasTriggered = false;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        BoxCollider box = GetComponent<BoxCollider>();
        if (box == null) return;

        Gizmos.color = new Color(0f, 1f, 0.5f, 0.3f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(box.center, box.size);

        Gizmos.color = new Color(0f, 1f, 0.5f, 0.9f);
        Gizmos.DrawWireCube(box.center, box.size);
    }
#endif
}
