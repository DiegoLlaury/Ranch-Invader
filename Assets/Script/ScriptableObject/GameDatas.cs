using UnityEngine;

[CreateAssetMenu(fileName = "GameDatas", menuName = "Scriptable Objects/GameDatas")]
public class GameDatas : ScriptableObject
{
    public int PlayerCellNumber;
    public int cellNumber;
    public int fleshNumber;
    public bool IsPlayerInMiniGame;
    public int MiniGameNumber;
}
