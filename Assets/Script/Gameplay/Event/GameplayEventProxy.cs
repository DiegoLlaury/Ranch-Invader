using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Proxy placé dans la scène pour exposer un UnityEvent configurable
/// depuis l'Inspector, déclenché par un UnityEventSO.
/// </summary>
public class GameplayEventProxy : MonoBehaviour
{
    [Tooltip("Action(s) à exécuter (ouvrir porte, lancer dialogue, etc.)")]
    public UnityEvent onTriggered;

    /// <summary>Déclenche le UnityEvent.</summary>
    public void Trigger()
    {
        onTriggered?.Invoke();
    }
}
