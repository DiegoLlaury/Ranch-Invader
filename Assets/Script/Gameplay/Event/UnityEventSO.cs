using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Gameplay/Events/Unity Event", fileName = "Event_UnityEvent")]
public class UnityEventSO : GameplayEventSO
{
    // Les UnityEvent ne peuvent pas être sérialisés dans un SO directement,
    // on passe par un proxy MonoBehaviour dans la scène.
    [Tooltip("Nom du GameObject portant un GameplayEventProxy pour cet événement")]
    public string proxyName;

    public override void Execute(MonoBehaviour caller)
    {
        CoroutineRunner.Instance.Run(ExecuteDelayed());
    }


    private IEnumerator ExecuteDelayed()
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        GameObject obj = GameObject.Find(proxyName);
        if (obj == null)
        {
            Debug.LogWarning($"[UnityEventSO] Proxy '{proxyName}' introuvable.");
            yield break;
        }

        GameplayEventProxy proxy = obj.GetComponent<GameplayEventProxy>();
        proxy?.Trigger();
    }
}
