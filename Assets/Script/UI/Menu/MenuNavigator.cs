using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Contrôleur racine du menu principal.
/// </summary>
public class MenuNavigator : MonoBehaviour, IOptionsHost
{
    [Header("Main UI")]
    [SerializeField] private GameObject buttonsGroup;

    [Header("Panels")]
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject creditsPanel;

    [Header("Scene")]
    [SerializeField] private string gameSceneName = "Level 01";

    private void Start()
    {
        ReturnToMainMenu();
    }

    public void OnPlayClicked()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void OnOptionsClicked()
    {
        SetMainButtonsVisible(false);
        CloseAllPanels();
        optionsPanel.SetActive(true);
    }

    public void OnCreditsClicked()
    {
        SetMainButtonsVisible(false);
        CloseAllPanels();
        creditsPanel.SetActive(true);
        creditsPanel.GetComponent<CreditsController>()?.PlayCredits(this);
    }

    public void OnQuitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    /// <summary>Retourne aux boutons principaux depuis un sous-panel.</summary>
    public void ReturnToMainMenu()
    {
        CloseAllPanels();
        SetMainButtonsVisible(true);
    }

    // IOptionsHost
    public void ReturnToPreviousScreen() => ReturnToMainMenu();

    private void CloseAllPanels()
    {
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);
    }

    private void SetMainButtonsVisible(bool visible)
    {
        if (buttonsGroup != null) buttonsGroup.SetActive(visible);
    }
}
