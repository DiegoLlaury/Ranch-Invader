using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Gameplay/Events/Chained Event", fileName = "Event_Chained")]
public class ChainedEventSO : GameplayEventSO
{
    [Tooltip("Événements à exécuter en séquence, dans l'ordre du tableau")]
    public GameplayEventSO[] chain;

    public override void Execute(MonoBehaviour caller)
    {
        caller.StartCoroutine(ExecuteChain(caller));
    }

    private IEnumerator ExecuteChain(MonoBehaviour caller)
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        foreach (GameplayEventSO gameplayEvent in chain)
        {
            if (gameplayEvent != null)
                gameplayEvent.Execute(caller);

            // Attendre le délai de l'event suivant avant de continuer
            float waitTime = gameplayEvent != null ? gameplayEvent.delay : 0f;
            if (waitTime > 0f)
                yield return new WaitForSeconds(waitTime);
        }
    }
}
