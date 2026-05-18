using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Affiche l'image de tête du personnage avec le sprite correspondant au seuil de vie courant.
/// Suit le même pattern d'abonnement que HealthBarUI.
/// </summary>
public class HealthPortraitUI : MonoBehaviour
{
    [SerializeField] private Image portraitImage;
    [SerializeField] private PlayerHealth playerHealth;

    [Tooltip("Sprites dans l'ordre décroissant de santé : [0]=pleine vie, [N-1]=mort imminente")]
    [SerializeField] private Sprite[] damageLevelSprites;

    private void Start()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged.AddListener(UpdatePortrait);
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged.RemoveListener(UpdatePortrait);
    }

    private void UpdatePortrait(float healthPercentage)
    {
        if (portraitImage == null || damageLevelSprites == null || damageLevelSprites.Length == 0)
            return;

        int index = Mathf.FloorToInt((1f - healthPercentage) * damageLevelSprites.Length);
        index = Mathf.Clamp(index, 0, damageLevelSprites.Length - 1);

        portraitImage.sprite = damageLevelSprites[index];
    }
}
