using UnityEngine;

/// <summary>
/// Classe de base pour tous les événements gameplay configurables.
/// Hériter de cette classe pour créer un nouveau type d'événement.
/// </summary>
public abstract class GameplayEventSO : ScriptableObject
{
    [Tooltip("Délai en secondes avant l'exécution de cet événement")]
    public float delay = 0f;

    /// <summary>Déclenche l'événement depuis le MonoBehaviour appelant.</summary>
    public abstract void Execute(MonoBehaviour caller);
}
