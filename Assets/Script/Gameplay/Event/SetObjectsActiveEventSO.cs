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
        caller.StartCoroutine(ExecuteDelayed());
    }

    private IEnumerator ExecuteDelayed()
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        foreach (ObjectActivation target in targets)
        {
            GameObject obj = GameObject.Find(target.gameObjectName);

            if (obj != null)
                obj.SetActive(target.setActive);
            else
                Debug.LogWarning($"[SetObjectsActiveEventSO] '{target.gameObjectName}' introuvable.");
        }
    }
}
