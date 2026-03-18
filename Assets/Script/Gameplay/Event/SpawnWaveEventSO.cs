using UnityEngine;

[CreateAssetMenu(menuName = "Gameplay/Events/Spawn Wave", fileName = "Event_SpawnWave")]
public class SpawnWaveEventSO : GameplayEventSO
{
    [Tooltip("Spawner cible. Si null, cherche un EnemySpawner dans la scène par son nom.")]
    public string targetSpawnerName;

    [Tooltip("Index de la vague à déclencher (laisser -1 = prochaine vague automatique)")]
    public int waveIndex = -1;

    public override void Execute(MonoBehaviour caller)
    {
        caller.StartCoroutine(ExecuteDelayed(caller));
    }

    private System.Collections.IEnumerator ExecuteDelayed(MonoBehaviour caller)
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        EnemySpawner spawner = FindSpawner();

        if (spawner == null)
        {
            Debug.LogWarning($"[SpawnWaveEventSO] Spawner '{targetSpawnerName}' introuvable.");
            yield break;
        }

        if (waveIndex >= 0)
            spawner.TriggerWave(waveIndex);
        else
            spawner.TriggerNextWave();
    }

    private EnemySpawner FindSpawner()
    {
        if (string.IsNullOrEmpty(targetSpawnerName))
            return null;

        GameObject obj = GameObject.Find(targetSpawnerName);
        return obj != null ? obj.GetComponent<EnemySpawner>() : null;
    }
}
