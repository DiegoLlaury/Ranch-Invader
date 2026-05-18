using TMPro;
using UnityEngine;

/// <summary>
/// Affiche la valeur entière des points de vie du joueur sous la forme "HP / HPMax".
/// S'abonne à PlayerHealth.OnHealthChanged exactement comme HealthBarUI.
/// </summary>
public class HealthValueUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI healthLabel;
    [SerializeField] private PlayerHealth playerHealth;

    [Tooltip("Format d'affichage. Utilisez {0} pour les HP courants.")]
    [SerializeField] private string displayFormat = "{0}";

    private void Start()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged.AddListener(OnHealthChanged);
            Refresh();
        }
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged.RemoveListener(OnHealthChanged);
    }

    private void OnHealthChanged(float percentage)
    {
        Refresh();
    }

    /// <summary>Met à jour le label avec la valeur entière courante des HP.</summary>
    private void Refresh()
    {
        if (healthLabel == null || playerHealth == null) return;

        healthLabel.text = string.Format(displayFormat,
            playerHealth.GetCurrentHealth());
    }
}
