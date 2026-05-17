using UnityEngine;

/// <summary>
/// Shared ScriptableObject linking multiple GeneratorObjects to a single GameplayEventSO.
/// The event fires only when all registered generators have been destroyed.
/// </summary>
[CreateAssetMenu(menuName = "Gameplay/Events/Generator Group", fileName = "Event_GeneratorGroup")]
public class GeneratorGroupSO : ScriptableObject
{
    [Tooltip("Event fired when all generators in this group are destroyed.")]
    [SerializeField] private GameplayEventSO onAllDestroyedEvent;

    private int remainingCount;

    /// <summary>
    /// Resets the group counter to zero. Must be called once per Play Mode session
    /// before any Register() call. GeneratorObject calls this in Awake().
    /// </summary>
    public void ResetCount()
    {
        remainingCount = 0;
    }

    /// <summary>
    /// Registers a generator into the group. Called in Start() after all ResetCount() calls.
    /// </summary>
    public void Register()
    {
        remainingCount++;
        Debug.Log($"[GeneratorGroupSO] {name} — Register. Compteur : {remainingCount}");
    }

    /// <summary>
    /// Called by a GeneratorObject when it is destroyed.
    /// Fires onAllDestroyedEvent when the last generator in the group goes down.
    /// </summary>
    public void NotifyDestroyed(MonoBehaviour caller)
    {
        remainingCount--;
        Debug.Log($"[GeneratorGroupSO] {name} — NotifyDestroyed. Restant : {remainingCount}");

        if (remainingCount <= 0)
            onAllDestroyedEvent?.Execute(caller);
    }
}
