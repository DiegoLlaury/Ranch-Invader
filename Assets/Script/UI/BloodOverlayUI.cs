using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Affiche des taches de sang semi-transparentes quand la vie est basse.
/// Le fondu est animé via Mathf.Lerp dans Update() pour un rendu fluide.
/// </summary>
public class BloodOverlayUI : MonoBehaviour
{
    [SerializeField] private Image[] bloodSplatImages;
    [SerializeField] private PlayerHealth playerHealth;

    [Tooltip("En dessous de ce pourcentage de vie (0–1), l'overlay apparaît")]
    [SerializeField] private float visibilityThreshold = 0.4f;

    [Tooltip("Alpha maximum des images de sang à vie très basse")]
    [SerializeField] private float maxAlpha = 0.7f;

    [Tooltip("Vitesse du fondu de l'effet de sang")]
    [SerializeField] private float fadeSpeed = 3f;

    private float targetAlpha;
    private float currentAlpha;

    private void Start()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged.AddListener(OnHealthChanged);

        SetImagesAlpha(0f);
        currentAlpha = 0f;
        targetAlpha = 0f;
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged.RemoveListener(OnHealthChanged);
    }

    private void Update()
    {
        if (Mathf.Approximately(currentAlpha, targetAlpha)) return;

        currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, Time.deltaTime * fadeSpeed);
        SetImagesAlpha(currentAlpha);
    }

    private void OnHealthChanged(float healthPercentage)
    {
        if (healthPercentage > visibilityThreshold)
        {
            targetAlpha = 0f;
        }
        else
        {
            float t = (visibilityThreshold - healthPercentage) / visibilityThreshold;
            targetAlpha = t * maxAlpha;
        }
    }

    private void SetImagesAlpha(float alpha)
    {
        if (bloodSplatImages == null) return;

        foreach (Image img in bloodSplatImages)
        {
            if (img == null) continue;
            Color c = img.color;
            c.a = alpha;
            img.color = c;
        }
    }
}
