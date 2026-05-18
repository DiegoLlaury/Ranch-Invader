using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gère la liste ordonnée des événements exécutés et la position de checkpoint courante.
/// Doit s'initialiser avant les GameplayTriggerZone — utilise DefaultExecutionOrder(-10).
/// </summary>
[DefaultExecutionOrder(-10)]
public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance { get; private set; }

    [SerializeField] private PlayerDatas playerDatas;
    [SerializeField] private SaveManager saveManager;

    private readonly List<string> sessionExecutedIds = new List<string>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        LoadFromSave();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    /// <summary>Enregistre un événement comme exécuté. Ne modifie pas la position de checkpoint.</summary>
    public void RegisterEvent(string eventId, Vector3 respawnPosition)
    {
        if (string.IsNullOrEmpty(eventId)) return;

        if (!sessionExecutedIds.Contains(eventId))
            sessionExecutedIds.Add(eventId);

        playerDatas.Datas.checkpointEventIndex = sessionExecutedIds.Count - 1;

        Save();
    }

    /// <summary>Définit explicitement la position de respawn (appelé par CheckpointZone).</summary>
    public void RegisterCheckpointPosition(Vector3 position)
    {
        if (playerDatas == null) return;

        playerDatas.Datas.checkpointPosition = position;
        Save();
    }

    /// <summary>Retourne true si l'événement a déjà été exécuté (session ou sauvegarde).</summary>
    public bool IsEventExecuted(string eventId)
    {
        return sessionExecutedIds.Contains(eventId);
    }

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

    /// <summary>Retourne la liste en lecture seule des IDs exécutés cette session.</summary>
    public IReadOnlyList<string> GetExecutedIds() => sessionExecutedIds;

    /// <summary>Retourne la position de respawn actuelle depuis PlayerDatas.</summary>
    public Vector3 GetCheckpointPosition()
    {
        return playerDatas != null ? playerDatas.Datas.checkpointPosition : Vector3.zero;
    }
}
