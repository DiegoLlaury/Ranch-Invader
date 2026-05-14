using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gère la navigation à deux niveaux dans le panel Options.
/// Niveau 1 — TabBar seul. Niveau 2 — Contenu d'un tab.
/// Compatible avec le menu principal et le menu pause via IOptionsHost.
/// </summary>
public class OptionsController : MonoBehaviour
{
    [Header("TabBar")]
    [SerializeField] private GameObject tabBar;

    [Header("Tabs")]
    [SerializeField] private GameObject audioTab;
    [SerializeField] private GameObject keybindTab;
    [SerializeField] private GameObject graphicsTab;

    [Header("Navigation")]
    [SerializeField] private Button backButton;

    private IOptionsHost host;
    private GameObject activeTab;

    private void Awake()
    {
        // Cherche MenuNavigator ou PauseMenuController dans les parents
        host = GetComponentInParent<IOptionsHost>();

        audioTab.SetActive(false);
        keybindTab.SetActive(false);
        graphicsTab.SetActive(false);
    }

    private void OnEnable()
    {
        EnterTabBar();
        backButton.onClick.AddListener(OnBackClicked);
    }

    private void OnDisable()
    {
        backButton.onClick.RemoveAllListeners();
    }

    /// <summary>Ouvre l'onglet Son.</summary>
    public void ShowAudio() => OpenTab(audioTab);

    /// <summary>Ouvre l'onglet Touches.</summary>
    public void ShowKeybind() => OpenTab(keybindTab);

    /// <summary>Ouvre l'onglet Graphismes.</summary>
    public void ShowGraphics() => OpenTab(graphicsTab);

    private void OpenTab(GameObject tab)
    {
        HideAllTabs();
        tab.SetActive(true);
        activeTab = tab;
    }

    private void EnterTabBar()
    {
        HideAllTabs();
        activeTab = null;
        tabBar.SetActive(true);
    }

    private void HideAllTabs()
    {
        audioTab.SetActive(false);
        keybindTab.SetActive(false);
        graphicsTab.SetActive(false);
    }

    private void OnBackClicked()
    {
        if (activeTab != null)
            EnterTabBar();
        else
            host?.ReturnToPreviousScreen();
    }
}
