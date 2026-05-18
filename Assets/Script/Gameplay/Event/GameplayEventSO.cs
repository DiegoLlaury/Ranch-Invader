using UnityEngine;

/// <summary>
/// Classe de base pour tous les �v�nements gameplay configurables.
/// H�riter de cette classe pour cr�er un nouveau type d'�v�nement.
/// </summary>
public abstract class GameplayEventSO : ScriptableObject
{
    [Tooltip("Délai en secondes avant l'exécution de cet événement")]
    public float delay = 0f;

    [Tooltip("Identifiant unique de cet événement. Doit être renseigné manuellement dans l'Inspector.")]
    public string eventId;

    /// <summary>Déclenche l'événement depuis le MonoBehaviour appelant.</summary>
    public abstract void Execute(MonoBehaviour caller);

    /// <summary>Notifie le CheckpointManager que cet événement a été exécuté.</summary>
    protected void NotifyCheckpoint(MonoBehaviour caller)
    {
        if (!string.IsNullOrEmpty(eventId))
            CheckpointManager.Instance?.RegisterEvent(eventId);
    }
}
