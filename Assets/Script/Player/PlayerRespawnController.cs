using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerRespawnController : MonoBehaviour
{
    private const string MainMenuSceneName = "Menu";
    private const string PlayerActionMap = "Player";

    [Header("Player")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private StarterAssetsInputs playerInputs;
    [SerializeField] private PlayerInput playerInput;

    [Header("Systems")]
    [SerializeField] private CheckpointManager checkpointManager;
    [SerializeField] private SceneStateRestorer sceneStateRestorer;
    [SerializeField] private DeathScreenUI deathScreenUI;

    private void Start()
    {
        if (playerHealth != null)
            playerHealth.OnDeath.AddListener(OnPlayerDied);

        ApplyCheckpointOnLoad();
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.OnDeath.RemoveListener(OnPlayerDied);
    }

    private void OnPlayerDied()
    {
        deathScreenUI?.Show(this);
    }

    /// <summary>Déclenché par le bouton "Continuer" de l'écran de mort.</summary>
    public void OnContinueClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>Déclenché par le bouton "Quitter" de l'écran de mort.</summary>
    public void OnQuitClicked()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene(MainMenuSceneName);
    }

    private void ApplyCheckpointOnLoad()
    {
        if (checkpointManager == null || playerTransform == null) return;

        // Utilise l'index sauvegardé pour retrouver le point de spawn exact
        Vector3 spawnPosition = checkpointManager.GetSpawnPosition();

        if (spawnPosition != Vector3.zero)
            playerTransform.position = spawnPosition;

        sceneStateRestorer?.RestoreStateAtCheckpoint();
    }
}
