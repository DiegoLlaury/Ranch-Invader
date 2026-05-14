using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Gère le menu pause en jeu. Écoute l'action "Pause" de StarterAssets.inputactions
/// via InputActionReference pour rester cohérent avec le reste du projet.
/// </summary>
public class PauseMenuController : MonoBehaviour, IOptionsHost
{
    private const string MainMenuSceneName = "Menu";

    [Header("Input")]
    [Tooltip("Glisse ici l'action Player/Pause de StarterAssets.inputactions")]
    [SerializeField] private InputActionReference pauseAction;

    [Header("Player")]
    [Tooltip("StarterAssetsInputs du joueur")]
    [SerializeField] private StarterAssetsInputs playerInputs;

    [Tooltip("PlayerInput du joueur — pour switcher entre action maps Player/UI")]
    [SerializeField] private PlayerInput playerInput;

    [Header("Panels")]
    [SerializeField] private GameObject pauseButtonsGroup;
    [SerializeField] private GameObject optionsPanel;

    private bool isPaused;

    private void OnEnable()
    {
        pauseAction.action.Enable();
        pauseAction.action.performed += OnPausePerformed;
    }

    private void OnDisable()
    {
        pauseAction.action.performed -= OnPausePerformed;
        pauseAction.action.Disable();
    }

    private void Start()
    {
        SetPauseUIVisible(false);
    }

    private void OnPausePerformed(InputAction.CallbackContext ctx)
    {
        TogglePause();
    }

    // ── API publique (boutons UI) ─────────────────────────────────────────────

    /// <summary>Reprend le jeu et ferme le menu pause.</summary>
    public void OnResumeClicked()
    {
        Resume();
    }

    /// <summary>Ouvre le panel Options depuis le menu pause.</summary>
    public void OnOptionsClicked()
    {
        pauseButtonsGroup.SetActive(false);
        optionsPanel.SetActive(true);
    }

    /// <summary>Retourne au menu principal et remet le temps à la normale.</summary>
    public void OnQuitClicked()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene(MainMenuSceneName);
    }

    // IOptionsHost
    public void ReturnToPreviousScreen()
    {
        optionsPanel.SetActive(false);
        pauseButtonsGroup.SetActive(true);
    }

    // ── Logique pause ─────────────────────────────────────────────────────────

    private void TogglePause()
    {
        if (isPaused) Resume();
        else Pause();
    }

    private void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;
        SetPlayerInputEnabled(false);
        SetPauseUIVisible(true);
    }

    private void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;
        SetPlayerInputEnabled(true);
        optionsPanel.SetActive(false);
        SetPauseUIVisible(false);
    }

    private void SetPauseUIVisible(bool visible)
    {
        pauseButtonsGroup.SetActive(visible);
        if (!visible) optionsPanel.SetActive(false);
    }

    private void SetCursorFree(bool free)
    {
        if (playerInputs == null) return;

        playerInputs.cursorLocked = !free;
        playerInputs.cursorInputForLook = !free;
        playerInputs.SetCursorState(!free);
    }

    private void SetPlayerInputEnabled(bool enabled)
    {
        // Libère/reverrouille le curseur
        if (playerInputs != null)
        {
            playerInputs.cursorLocked = enabled;
            playerInputs.cursorInputForLook = enabled;
            playerInputs.SetCursorState(enabled);
        }

        // Bascule l'action map : "UI" en pause, "Player" en jeu
        if (playerInput != null)
        {
            playerInput.SwitchCurrentActionMap(enabled ? "Player" : "UI");
        }
    }


    private void OnDestroy()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
