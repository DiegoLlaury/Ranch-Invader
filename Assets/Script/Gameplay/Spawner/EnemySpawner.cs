using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Spawner configurable de vagues d'ennemis.
/// D�clenche des GameplayEventSO quand tous les ennemis d'une vague sont �limin�s.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class EnemyWave
    {
        [Tooltip("Prefabs d'ennemis � spawner pour cette vague")]
        public GameObject[] enemyPrefabs;

        [Tooltip("D�lai entre chaque spawn d'ennemi dans la vague")]
        public float spawnInterval = 0.5f;

        [Tooltip("�v�nements d�clench�s quand tous les ennemis de cette vague sont �limin�s")]
        public GameplayEventSO[] onWaveCleared;
    }

    [Header("Waves")]
    [Tooltip("Liste des vagues configurables")]
    public EnemyWave[] waves;

    [Tooltip("D�clenche automatiquement la vague 0 au d�marrage")]
    public bool autoStartOnAwake = false;

    [Header("Spawn Points")]
    [Tooltip("Points de spawn utilis�s � la rotation. Si vide, utilise la position du spawner.")]
    public Transform[] spawnPoints;

    [Header("Events")]
    [Tooltip("�v�nements d�clench�s quand TOUTES les vagues sont termin�es")]
    public GameplayEventSO[] onAllWavesCleared;

    private int currentWaveIndex = -1;
    private int spawnPointIndex = 0;
    private readonly List<GameObject> aliveEnemies = new List<GameObject>();
    private bool allWavesCompleted = false;

    private const float SpawnOccupancyRadius = 1.2f;
    private const int SpawnMaxAttempts = 8;
    private const float SpawnOffsetRadius = 2.0f;

    private void Start()
    {
        if (autoStartOnAwake)
            TriggerNextWave();
    }

    /// <summary>D�clenche la vague suivante dans l'ordre.</summary>
    public void TriggerNextWave()
    {
        TriggerWave(currentWaveIndex + 1);
    }

    /// <summary>D�clenche une vague par son index.</summary>
    public void TriggerWave(int index)
    {
        if (allWavesCompleted)
        {
            Debug.LogWarning($"[EnemySpawner:{name}] Toutes les vagues sont d�j� termin�es.");
            return;
        }

        if (index < 0 || index >= waves.Length)
        {
            Debug.LogWarning($"[EnemySpawner:{name}] Index de vague {index} invalide.");
            return;
        }

        currentWaveIndex = index;
        StartCoroutine(SpawnWaveCoroutine(waves[index]));
    }

    private IEnumerator SpawnWaveCoroutine(EnemyWave wave)
    {
        aliveEnemies.Clear();

        foreach (GameObject prefab in wave.enemyPrefabs)
        {
            if (prefab == null) continue;

            Vector3 spawnPosition = GetNextSpawnPosition();
            GameObject enemy = Instantiate(prefab, spawnPosition, Quaternion.identity);

            // S'abonne � la destruction de l'ennemi pour suivre son �tat
            EnemyDeathNotifier notifier = enemy.AddComponent<EnemyDeathNotifier>();
            notifier.Initialize(this);

            aliveEnemies.Add(enemy);

            if (wave.spawnInterval > 0f)
                yield return new WaitForSeconds(wave.spawnInterval);
        }
    }

    /// <summary>Appelé par EnemyDeathNotifier quand un ennemi est détruit.</summary>
    public void OnEnemyDied(GameObject enemy)
    {
        aliveEnemies.Remove(enemy);

        // Nettoie les références nulles restantes (destructions inattendues)
        aliveEnemies.RemoveAll(e => e == null);

        if (aliveEnemies.Count == 0)
            OnWaveCleared();
    }

    private void OnWaveCleared()
    {
        EnemyWave clearedWave = waves[currentWaveIndex];

        foreach (GameplayEventSO gameplayEvent in clearedWave.onWaveCleared)
            gameplayEvent?.Execute(this);

        bool isLastWave = currentWaveIndex >= waves.Length - 1;

        if (isLastWave)
        {
            allWavesCompleted = true;

            foreach (GameplayEventSO gameplayEvent in onAllWavesCleared)
                gameplayEvent?.Execute(this);
        }
    }

    private Vector3 GetNextSpawnPosition()
    {
        Vector3 basePosition = spawnPoints != null && spawnPoints.Length > 0
            ? (spawnPoints[spawnPointIndex % spawnPoints.Length] != null
                ? spawnPoints[spawnPointIndex % spawnPoints.Length].position
                : transform.position)
            : transform.position;

        spawnPointIndex++;

        // Try to find a free spot around the base position
        for (int attempt = 0; attempt < SpawnMaxAttempts; attempt++)
        {
            Vector3 candidate;

            if (attempt == 0)
            {
                candidate = basePosition;
            }
            else
            {
                float angle = (360f / (SpawnMaxAttempts - 1)) * (attempt - 1) * Mathf.Deg2Rad;
                candidate = basePosition + new Vector3(
                    Mathf.Cos(angle) * SpawnOffsetRadius,
                    0f,
                    Mathf.Sin(angle) * SpawnOffsetRadius
                );
            }

            // Snap candidate to NavMesh
            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, SpawnOffsetRadius, NavMesh.AllAreas))
            {
                candidate = hit.position;
            }

            // Check if any alive enemy is too close
            bool occupied = false;
            foreach (GameObject enemy in aliveEnemies)
            {
                if (enemy == null) continue;
                if (Vector3.Distance(enemy.transform.position, candidate) < SpawnOccupancyRadius)
                {
                    occupied = true;
                    break;
                }
            }

            if (!occupied)
                return candidate;
        }

        // Fallback: return base position even if occupied
        return basePosition;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.5f);

        if (spawnPoints == null) return;

        Gizmos.color = new Color(1f, 0.4f, 0f);
        foreach (Transform point in spawnPoints)
        {
            if (point != null)
                Gizmos.DrawWireSphere(point.position, 0.3f);
        }
    }
#endif
}
