using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gère la liste ordonnée des événements exécutés et l'index de checkpoint courant.
/// Les CheckpointZone s'enregistrent ici via RegisterCheckpoint() au Awake,
/// puis notifient l'activation via ActivateCheckpoint(index).
/// Doit s'initialiser avant les autres composants — utilise DefaultExecutionOrder(-10).
/// </summary>
[DefaultExecutionOrder(-10)]
public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance { get; private set; }

    [SerializeField] private PlayerDatas playerDatas;
    [SerializeField] private SaveManager saveManager;

    private readonly List<string> sessionExecutedIds = new List<string>();

    // Tableau trié par index croissant, rempli par les CheckpointZone au Awake
    private readonly SortedDictionary<int, CheckpointZone> registeredCheckpoints
        = new SortedDictionary<int, CheckpointZone>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        LoadFromSave();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    // ── Enregistrement des zones ──────────────────────────────────────────────

    /// <summary>Appelé par chaque CheckpointZone au Awake pour s'inscrire dans le manager.</summary>
    public void RegisterCheckpoint(int index, CheckpointZone zone)
    {
        if (registeredCheckpoints.ContainsKey(index))
        {
            Debug.LogWarning($"[CheckpointManager] Index {index} déjà utilisé par '{registeredCheckpoints[index].name}'. " +
                             $"'{zone.name}' sera ignoré.");
            return;
        }
        registeredCheckpoints[index] = zone;
    }

    // ── Activation ────────────────────────────────────────────────────────────

    /// <summary>
    /// Enregistre l'index comme checkpoint actif si supérieur à l'index courant.
    /// Appelé par CheckpointZone quand le joueur entre dans la zone.
    /// </summary>
    public void ActivateCheckpoint(int index)
    {
        if (playerDatas == null) return;
        if (index <= playerDatas.Datas.checkpointIndex) return;

        playerDatas.Datas.checkpointIndex = index;
        playerDatas.Datas.isFirstPlay = false;
        Save();

        Debug.Log($"[CheckpointManager] Checkpoint activé → index {index}");
    }

    // ── Position de spawn ─────────────────────────────────────────────────────

    /// <summary>
    /// Retourne la position de spawn correspondant à l'index sauvegardé.
    /// Retourne Vector3.zero si aucun checkpoint n'a été activé.
    /// </summary>
    public Vector3 GetSpawnPosition()
    {
        if (playerDatas == null) return Vector3.zero;

        int savedIndex = playerDatas.Datas.checkpointIndex;
        if (savedIndex < 0) return Vector3.zero;

        if (registeredCheckpoints.TryGetValue(savedIndex, out CheckpointZone zone))
            return zone.GetSpawnPosition();

        Debug.LogWarning($"[CheckpointManager] Aucun CheckpointZone trouvé pour l'index {savedIndex}.");
        return Vector3.zero;
    }

    // ── Événements ────────────────────────────────────────────────────────────

    /// <summary>Enregistre un événement comme exécuté pour cette session.</summary>
    public void RegisterEvent(string eventId)
    {
        if (string.IsNullOrEmpty(eventId)) return;
        if (!sessionExecutedIds.Contains(eventId))
            sessionExecutedIds.Add(eventId);
        Save();
    }

    /// <summary>Retourne true si l'événement a déjà été exécuté (session ou sauvegarde).</summary>
    public bool IsEventExecuted(string eventId) => sessionExecutedIds.Contains(eventId);

    /// <summary>Retourne la liste en lecture seule des IDs exécutés cette session.</summary>
    public IReadOnlyList<string> GetExecutedIds() => sessionExecutedIds;

    // ── Persistance ───────────────────────────────────────────────────────────

    /// <summary>Applique l'état sauvegardé : restaure sessionExecutedIds depuis PlayerDatas.</summary>
    public void LoadFromSave()
    {
        sessionExecutedIds.Clear();
        if (playerDatas == null) return;

        List<string> saved = playerDatas.Datas.executedEventIds;
        if (saved != null)
            sessionExecutedIds.AddRange(saved);
    }

    /// <summary>Persiste l'état courant dans PlayerDatas et déclenche SaveManager.SaveGame().</summary>
    public void Save()
    {
        if (playerDatas == null) return;
        playerDatas.Datas.executedEventIds = new List<string>(sessionExecutedIds);
        saveManager?.SaveGame();
    }

    /// <summary>Retourne l'index de checkpoint actuellement sauvegardé (-1 si aucun).</summary>
    public int GetCheckpointIndex() => playerDatas != null ? playerDatas.Datas.checkpointIndex : -1;
}
