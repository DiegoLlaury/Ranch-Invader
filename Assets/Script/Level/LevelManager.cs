using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    [SerializeField] PlayerDatas gameDatas;

    public void ContinueGame()
    {
        //TODO : Charger la dernière sauvegarde
        GetComponent<SaveManager>().LoadGame();

            SceneManager.LoadScene(1);
    }
}
