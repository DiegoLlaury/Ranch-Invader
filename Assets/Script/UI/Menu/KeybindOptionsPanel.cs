using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Panneau de touches — bascule entre WASD et ZQSD via overrides sur StarterAssets.inputactions.
/// </summary>
public class KeybindOptionsPanel : MonoBehaviour
{
    private const string PrefKeyLayout = "KeyLayout_Pref"; // 0 = WASD, 1 = ZQSD

    // Binding IDs du composite Move dans StarterAssets.inputactions
    private const string IdUp = "2063a8b5-6a45-43de-851b-65f3d46e7b58";
    private const string IdDown = "64e4d037-32e1-4fb9-80e4-fc7330404dfe";
    private const string IdLeft = "0fce8b11-5eab-4e4e-a741-b732e7b20873";
    private const string IdRight = "7bdda0d6-57a8-47c8-8238-8aecf3110e47";

    [Header("Input Asset")]
    [Tooltip("Glisse ici StarterAssets.inputactions")]
    [SerializeField] private InputActionAsset inputActions;

    [Header("UI")]
    [SerializeField] private Button wasdButton;
    [SerializeField] private Button zqsdButton;
    [SerializeField] private TMP_Text currentLayoutLabel;

    private void OnEnable()
    {
        wasdButton.onClick.AddListener(() => ApplyLayout(false));
        zqsdButton.onClick.AddListener(() => ApplyLayout(true));

        bool isZQSD = PlayerPrefs.GetInt(PrefKeyLayout, 0) == 1;
        ApplyLayout(isZQSD);
    }

    private void OnDisable()
    {
        wasdButton.onClick.RemoveAllListeners();
        zqsdButton.onClick.RemoveAllListeners();
    }

    private void ApplyLayout(bool useZQSD)
    {
        PlayerPrefs.SetInt(PrefKeyLayout, useZQSD ? 1 : 0);
        currentLayoutLabel.text = useZQSD ? "ZQSD" : "WASD";

        var action = inputActions.FindAction("Move");
        if (action == null)
        {
            Debug.LogError("[KeybindOptionsPanel] Action 'Move' introuvable dans StarterAssets.inputactions.");
            return;
        }

        action.ApplyBindingOverride(new InputBinding { id = System.Guid.Parse(IdUp), overridePath = useZQSD ? "<Keyboard>/z" : "<Keyboard>/w" });
        action.ApplyBindingOverride(new InputBinding { id = System.Guid.Parse(IdDown), overridePath = "<Keyboard>/s" });
        action.ApplyBindingOverride(new InputBinding { id = System.Guid.Parse(IdLeft), overridePath = useZQSD ? "<Keyboard>/q" : "<Keyboard>/a" });
        action.ApplyBindingOverride(new InputBinding { id = System.Guid.Parse(IdRight), overridePath = "<Keyboard>/d" });
    }
}
