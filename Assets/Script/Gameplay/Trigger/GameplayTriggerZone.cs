using UnityEngine;

/// <summary>
/// Zone de collision (trigger) qui d�clenche des GameplayEventSO
/// quand le joueur entre, sort ou reste dans la zone.
/// N�cessite un BoxCollider en mode trigger sur le m�me GameObject.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class GameplayTriggerZone : MonoBehaviour
{
    [Header("Detection")]
    [Tooltip("Tag du GameObject d�clencheur (g�n�ralement 'Player')")]
    public string triggerTag = "Player";

    [Tooltip("Si activ�, la zone ne se d�clenche qu'une seule fois")]
    public bool triggerOnce = true;

    [Header("Events")]
    [Tooltip("�v�nements d�clench�s � l'entr�e dans la zone")]
    public GameplayEventSO[] onEnterEvents;

    [Tooltip("�v�nements d�clench�s � la sortie de la zone")]
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

    /// <summary>Marque la zone comme déjà déclenchée sans exécuter les events (post-checkpoint).</summary>
    public void ResetToCompleted()
    {
        hasTriggered = true;
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
