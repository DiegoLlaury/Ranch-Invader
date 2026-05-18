using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Represents a single weapon slot in the HUD.
/// Displays the weapon icon, the key binding hint, and adapts its
/// visual state based on whether it is selected, unlocked, or locked.
/// </summary>
public class WeaponSlotUI : MonoBehaviour
{
    [Header("Références UI")]
    [Tooltip("Image affichant l'icône de l'arme (weaponIconSprite du WeaponData)")]
    public Image weaponIcon;

    [Tooltip("Texte affichant la touche de sélection (ex. '1', '2', 'Q', 'E')")]
    public TextMeshProUGUI keyLabel;

    [Tooltip("Image de fond ou de bordure du slot (optionnel)")]
    public Image slotBackground;

    [Header("Configuration")]
    [Tooltip("Texte de la touche affiché dans le label (ex. '1', '2')")]
    public string keyHint = "1";

    private void Awake()
    {
        if (keyLabel != null)
            keyLabel.text = keyHint;
    }

    /// <summary>Shows or hides the entire slot.</summary>
    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }

    /// <summary>Updates the weapon icon sprite.</summary>
    public void SetIcon(Sprite icon)
    {
        if (weaponIcon == null) return;
        weaponIcon.sprite = icon;
        weaponIcon.enabled = icon != null;
    }

    /// <summary>Applies selected or unselected tint to the icon and background. The key label always stays fully opaque.</summary>
    public void SetSelected(bool selected, Color selectedColor, Color unselectedColor)
    {
        Color targetColor = selected ? selectedColor : unselectedColor;

        if (weaponIcon != null)
            weaponIcon.color = targetColor;

        if (slotBackground != null)
            slotBackground.color = targetColor;

        // La touche reste toujours visible à pleine opacité, quelle que soit la sélection.
        if (keyLabel != null)
        {
            Color keyColor = targetColor;
            keyColor.a = 1f;
            keyLabel.color = keyColor;
        }
    }
}
