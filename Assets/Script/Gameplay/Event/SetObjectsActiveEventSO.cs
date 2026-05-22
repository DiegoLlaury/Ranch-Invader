using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Gameplay/Events/Set Objects Active", fileName = "Event_SetObjectsActive")]
public class SetObjectsActiveEventSO : GameplayEventSO
{
    [System.Serializable]
    public struct ObjectActivation
    {
        public string gameObjectName;
        public bool setActive;
    }

    [Tooltip("Liste des GameObjects à activer ou désactiver")]
    public ObjectActivation[] targets;

    public override void Execute(MonoBehaviour caller)
    {
        NotifyCheckpoint(caller);

        // Gameplay live
        CoroutineRunner.Instance.Run(ExecuteDelayed());
    }

    public override void RestoreState(Object instigator)
    {
        // Reconstruction instantanée silencieuse
        ApplyState();
    }

    private IEnumerator ExecuteDelayed()
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        ApplyState();
    }

    /// <summary>
    /// Applique réellement l'état des objets.
    /// Utilisé par Execute() ET RestoreState().
    /// </summary>
    private void ApplyState()
    {
        foreach (ObjectActivation target in targets)
        {
            GameObject obj = GameObject.Find(target.gameObjectName);

            if (obj != null)
            {
                obj.SetActive(target.setActive);
            }
            else
            {
                Debug.LogWarning(
                    $"[SetObjectsActiveEventSO] '{target.gameObjectName}' introuvable."
                );
            }
        }
    }
}