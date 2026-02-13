using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Image healthBarFill;
    [SerializeField] private PlayerHealth playerHealth;

    void Start()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged.AddListener(UpdateHealthBar);
        }
    }

    private void UpdateHealthBar(float healthPercentage)
    {
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = healthPercentage;
        }
    }
}
