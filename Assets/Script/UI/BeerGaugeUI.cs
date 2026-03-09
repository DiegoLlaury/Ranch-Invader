using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BeerGaugeUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DrunkEffect drunkEffect;

    [Header("Beer Texture Fill")]
    [SerializeField] private Image durationBarFill;
    [SerializeField] private float slideDownDistance = 80f;
    [SerializeField] private float fillLerpSpeed = 8f;

    [Header("Wave Effect")]
    [Tooltip("Amplitude de l'ondulation au stack 1.")]
    [SerializeField] private float waveAmplitudeMin = 0.008f;
    [Tooltip("Amplitude de l'ondulation au stack max.")]
    [SerializeField] private float waveAmplitudeMax = 0.04f;
    [Tooltip("Vitesse de l'ondulation au stack 1.")]
    [SerializeField] private float waveSpeedMin = 1.5f;
    [Tooltip("Vitesse de l'ondulation au stack max.")]
    [SerializeField] private float waveSpeedMax = 5f;

    [Header("Gauge Visibility")]
    [SerializeField] private GameObject gaugeRoot;

    [Header("Combo Counter")]
    [SerializeField] private TextMeshProUGUI comboText;
    [SerializeField] private ComboTextEffect comboTextEffect;

    [Header("Combo Animation")]
    [SerializeField] private float comboPunchScale = 1.4f;
    [SerializeField] private float comboScaleReturnSpeed = 10f;

    private static readonly int ShaderWaveAmplitude = Shader.PropertyToID("_WaveAmplitude");
    private static readonly int ShaderWaveSpeed = Shader.PropertyToID("_WaveSpeed");

    private Vector2 fillOriginPosition;
    private RectTransform fillRectTransform;
    private Material waveMaterialInstance;
    private float displayedFill = 1f;
    private int lastBeerStack = 0;
    private Vector3 comboBaseScale;

    private void Start()
    {
        if (drunkEffect == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                drunkEffect = player.GetComponent<DrunkEffect>();

            if (drunkEffect == null)
                Debug.LogWarning("[BeerGaugeUI] DrunkEffect introuvable sur le joueur !");
        }

        if (durationBarFill != null)
        {
            fillRectTransform = durationBarFill.rectTransform;
            fillOriginPosition = fillRectTransform.anchoredPosition;

            // Instance propre du material pour ne pas affecter les autres objets partageant le même
            if (durationBarFill.material != null)
            {
                waveMaterialInstance = new Material(durationBarFill.material);
                durationBarFill.material = waveMaterialInstance;
            }
        }

        if (comboText != null)
            comboBaseScale = comboText.transform.localScale;

        if (comboTextEffect == null && comboText != null)
            comboTextEffect = comboText.GetComponent<ComboTextEffect>();

        if (gaugeRoot == gameObject)
        {
            Debug.LogError("[BeerGaugeUI] gaugeRoot ne peut pas être ce GameObject lui-même.");
            gaugeRoot = null;
        }

        SetGaugeVisible(false);
    }

    private void OnDestroy()
    {
        if (waveMaterialInstance != null)
            Destroy(waveMaterialInstance);
    }

    private void Update()
    {
        if (drunkEffect == null) return;

        bool isDrunk = drunkEffect.IsDrunk();
        SetGaugeVisible(isDrunk);

        if (!isDrunk)
        {
            displayedFill = 1f;
            lastBeerStack = 0;
            ResetFillTransform();
            return;
        }

        UpdateDurationBar();
        UpdateComboCounter();
        AnimateComboScale();
    }

    // ── Duration Bar ──────────────────────────────────────────────────────────

    private void UpdateDurationBar()
    {
        float total = drunkEffect.GetTotalDuration();
        float remaining = drunkEffect.GetRemainingDuration();
        float targetFill = total > 0f ? Mathf.Clamp01(remaining / total) : 0f;

        displayedFill = Mathf.Lerp(displayedFill, targetFill, fillLerpSpeed * Time.deltaTime);

        ApplySlideAndFade(displayedFill);
        UpdateWave();
    }

    private void ApplySlideAndFade(float fillRatio)
    {
        if (durationBarFill == null || fillRectTransform == null) return;

        float offsetY = (1f - fillRatio) * -slideDownDistance;
        fillRectTransform.anchoredPosition = new Vector2(fillOriginPosition.x, fillOriginPosition.y + offsetY);

        Color c = durationBarFill.color;
        c.a = fillRatio;
        durationBarFill.color = c;
    }

    private void UpdateWave()
    {
        if (waveMaterialInstance == null) return;

        int maxStack = drunkEffect.GetBeerStack();
        int maxBeer = 5; // Doit correspondre à maxBeerStack dans DrunkEffect
        float t = maxBeer > 1 ? Mathf.Clamp01((float)(maxStack - 1) / (maxBeer - 1)) : 1f;

        waveMaterialInstance.SetFloat(ShaderWaveAmplitude, Mathf.Lerp(waveAmplitudeMin, waveAmplitudeMax, t));
        waveMaterialInstance.SetFloat(ShaderWaveSpeed, Mathf.Lerp(waveSpeedMin, waveSpeedMax, t));
    }

    private void ResetFillTransform()
    {
        if (fillRectTransform == null) return;
        fillRectTransform.anchoredPosition = fillOriginPosition;

        if (durationBarFill != null)
        {
            Color c = durationBarFill.color;
            c.a = 1f;
            durationBarFill.color = c;
        }
    }

    // ── Combo Counter ─────────────────────────────────────────────────────────

    private void UpdateComboCounter()
    {
        int stack = drunkEffect.GetBeerStack();

        if (stack != lastBeerStack)
        {
            if (comboText != null)
                comboText.transform.localScale = comboBaseScale * comboPunchScale;

            lastBeerStack = stack;
        }

        if (comboText != null)
            comboText.text = $"x{stack}";

        comboTextEffect?.UpdateEffect(stack);
    }

    private void AnimateComboScale()
    {
        if (comboText == null) return;

        comboText.transform.localScale = Vector3.Lerp(
            comboText.transform.localScale,
            comboBaseScale,
            comboScaleReturnSpeed * Time.deltaTime
        );
    }

    // ── Visibility ────────────────────────────────────────────────────────────

    private void SetGaugeVisible(bool visible)
    {
        if (gaugeRoot != null && gaugeRoot.activeSelf != visible)
            gaugeRoot.SetActive(visible);
    }
}
