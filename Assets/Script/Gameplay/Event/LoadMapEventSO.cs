using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(menuName = "Gameplay/Events/Load Map", fileName = "Event_LoadMap")]
public class LoadMapEventSO : GameplayEventSO
{
    [Tooltip("Nom exact de la sc�ne � charger (doit �tre dans les Build Settings)")]
    public string sceneName;

    public override void Execute(MonoBehaviour caller)
    {
        NotifyCheckpoint(caller);
        caller.StartCoroutine(ExecuteDelayed());
    }

    private IEnumerator ExecuteDelayed()
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("[LoadMapEventSO] Aucun nom de sc�ne assign�.");
            yield break;
        }

        SceneManager.LoadScene(sceneName);
    }
}
