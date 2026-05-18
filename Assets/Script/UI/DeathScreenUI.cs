using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Contrôle l'affichage et le masquage de l'écran de mort.
/// Suit le même pattern que PauseMenuController pour la gestion du temps et des inputs.
/// </summary>
public class DeathScreenUI : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button quitButton;

    [Header("Player Input")]
    [SerializeField] private StarterAssetsInputs playerInputs;
    [SerializeField] private PlayerInput playerInput;

    [Header("Player Controller")]
    [SerializeField] private FirstPersonController firstPersonController;

    /// <summary>Affiche l'écran de mort et câble les boutons.</summary>
    public void Show(PlayerRespawnController controller)
    {
        root.SetActive(true);

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Bloquer tout contrôle du joueur (mouvement + caméra)
        if (firstPersonController != null)
            firstPersonController.IsControlEnabled = false;

        if (playerInputs != null)
        {
            playerInputs.move = Vector2.zero;
            playerInputs.look = Vector2.zero;
            playerInputs.jump = false;
            playerInputs.sprint = false;
            playerInputs.cursorLocked = false;
            playerInputs.cursorInputForLook = false;
            playerInputs.SetCursorState(false);
        }

        if (playerInput != null)
            playerInput.SwitchCurrentActionMap("UI");

        continueButton.onClick.RemoveAllListeners();
        continueButton.onClick.AddListener(controller.OnContinueClicked);

        quitButton.onClick.RemoveAllListeners();
        quitButton.onClick.AddListener(controller.OnQuitClicked);
    }

    /// <summary>Cache l'écran de mort.</summary>
    public void Hide()
    {
        root.SetActive(false);
    }

    /// <summary>Réactive les contrôles du joueur après le respawn.</summary>
    public void RestorePlayerControl()
    {
        if (firstPersonController != null)
            firstPersonController.IsControlEnabled = true;
    }
}
