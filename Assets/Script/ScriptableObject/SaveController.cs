// Legacy stub — board persistence is now handled by BoardSaveManager + BoardStateSerializer.
// Kept to avoid breaking existing asset references.
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;

[System.Serializable]
public struct PlayerDatasStruct
{
    // Checkpoint data
    public List<string> executedEventIds;
    public List<string> unclockWeaponSave;
    public int checkpointIndex;       // Index du dernier checkpoint activé (-1 = aucun)
    public bool isFirstPlay;          // true tant que le joueur n'a jamais sauvegardé de progression    
}

public class SaveController
{
    public void SaveGameData(PlayerDatasStruct playerDatas, string filename)
    {
        string data = JsonUtility.ToJson(playerDatas);
        File.WriteAllText(Path.Combine(Application.persistentDataPath, filename), data);
    }

    public PlayerDatasStruct LoadGameData(string filename)
    {
        string path = Path.Combine(Application.persistentDataPath, filename);
        if (File.Exists(path))
        {
            string data = File.ReadAllText(path);
            return JsonUtility.FromJson<PlayerDatasStruct>(data);
        }

        // Aucune sauvegarde existante : c'est la première partie
        var empty = new PlayerDatasStruct
        {
            checkpointIndex = -1,
            isFirstPlay = true
        };
        SaveGameData(empty, filename);
        return empty;
    }
}
