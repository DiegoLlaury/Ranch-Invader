using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerDatas", menuName = "Scriptable Objects/PlayerDatas")]
public class PlayerDatas : ScriptableObject
{
    public PlayerDatasStruct Datas;

    /// <summary>
    /// Remet toutes les données joueur à leur valeur initiale.
    /// checkpointIndex est mis à -1 (aucun checkpoint), isFirstPlay à true.
    /// À appeler depuis le menu principal ou les outils de debug.
    /// </summary>
    public void ResetToDefaults()
    {
        Datas = new PlayerDatasStruct
        {
            executedEventIds  = new List<string>(),
            unclockWeaponSave = new List<string>(),
            checkpointIndex   = -1,
            isFirstPlay       = true
        };
    }

    /// <summary>
    /// Retourne true si les données sont vierges : aucun checkpoint activé,
    /// liste d'événements vide ou nulle, et drapeau isFirstPlay actif.
    /// </summary>
    public bool HasNoProgress()
    {
        bool noCheckpoint  = Datas.checkpointIndex < 0;
        bool noEvents      = Datas.executedEventIds == null || Datas.executedEventIds.Count == 0;
        return noCheckpoint && noEvents && Datas.isFirstPlay;
    }
}
