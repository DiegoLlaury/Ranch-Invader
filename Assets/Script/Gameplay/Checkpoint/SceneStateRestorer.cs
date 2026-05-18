using UnityEngine;

/// <summary>
/// Remet les GameplayTriggerZone et les EnemySpawner dans l'état qu'ils devaient avoir
/// juste avant le dernier checkpoint non-déclenché. À appeler lors du respawn du joueur.
/// </summary>
public class SceneStateRestorer : MonoBehaviour
{
    [SerializeField] private CheckpointManager checkpointManager;
    [SerializeField] private GameplayTriggerZone[] allTriggerZones;
    [SerializeField] private EnemySpawner[] allSpawners;

    /// <summary>Remet à zéro les triggers et spawners selon les événements déjà exécutés.</summary>
    public void RestoreStateAtCheckpoint()
    {
        if (checkpointManager == null)
        {
            Debug.LogWarning("[SceneStateRestorer] CheckpointManager non assigné.");
            return;
        }

        RestoreTriggerZones();
        RestoreSpawners();
    }

    private void RestoreTriggerZones()
    {
        if (allTriggerZones == null) return;

        foreach (GameplayTriggerZone zone in allTriggerZones)
        {
            if (zone == null) continue;

            bool allEventsExecuted = AreAllZoneEventsExecuted(zone);

            if (allEventsExecuted)
                zone.ResetToCompleted();
            else
                zone.Reset();
        }
    }

    private bool AreAllZoneEventsExecuted(GameplayTriggerZone zone)
    {
        GameplayEventSO[] events = zone.onEnterEvents;
        if (events == null || events.Length == 0) return false;

        bool foundAtLeastOneTrackedEvent = false;

        foreach (GameplayEventSO ev in events)
        {
            if (ev == null) continue;
            if (string.IsNullOrEmpty(ev.eventId)) continue;

            foundAtLeastOneTrackedEvent = true;

            if (!checkpointManager.IsEventExecuted(ev.eventId))
                return false;
        }

        return foundAtLeastOneTrackedEvent;
    }


    private void RestoreSpawners()
    {
        if (allSpawners == null) return;

        foreach (EnemySpawner spawner in allSpawners)
        {
            if (spawner == null) continue;

            bool allWavesExecuted = AreAllSpawnerWavesExecuted(spawner);

            if (allWavesExecuted)
                spawner.ResetToCompleted();
            else
                spawner.ResetToInitial();
        }
    }

    private bool AreAllSpawnerWavesExecuted(EnemySpawner spawner)
    {
        if (spawner.waves == null || spawner.waves.Length == 0) return false;

        bool foundAtLeastOneTrackedEvent = false;

        foreach (EnemySpawner.EnemyWave wave in spawner.waves)
        {
            if (wave == null || wave.onWaveCleared == null) continue;

            foreach (GameplayEventSO ev in wave.onWaveCleared)
            {
                if (ev == null) continue;
                if (string.IsNullOrEmpty(ev.eventId)) continue;

                foundAtLeastOneTrackedEvent = true;

                if (!checkpointManager.IsEventExecuted(ev.eventId))
                    return false;
            }
        }

        // Si aucun event trackable n'a été trouvé, on ne peut pas
        // déterminer l'état → on considère le spawner comme non terminé.
        return foundAtLeastOneTrackedEvent;
    }

}
