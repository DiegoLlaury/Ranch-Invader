using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    [SerializeField] PlayerDatas gameDatas;

    public void ContinueGame()
    {
        //TODO : Charger la dernière sauvegarde
        GetComponent<SaveManager>().LoadGame();
        //TODO : Charge le dernier niveau joué
        if (gameDatas.Datas.IsPlayerInMiniGame)
        {
            SceneManager.LoadScene(gameDatas.Datas.MiniGameNumber);
        }
        else
        {
            SceneManager.LoadScene(1);
        }
    }
}
