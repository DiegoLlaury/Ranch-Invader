using UnityEngine;
using TMPro;

/// <summary>
/// Drives the combo text visual effects based on the current beer stack :
/// - Color interpolates from dark red to bright red as stack increases.
/// - Text shakes with increasing intensity per stack level.
/// - At max stack, each character cycles through a rainbow palette via vertex colors.
/// Attach on the same GameObject as the TextMeshProUGUI combo label.
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class ComboTextEffect : MonoBehaviour
{
    [Header("Stack Settings")]
    [Tooltip("Must match DrunkEffect.maxBeerStack.")]
    [SerializeField] private int maxStack = 5;

    [Header("Color Ramp")]
    [Tooltip("Color at stack 1 — dark red.")]
    [SerializeField] private Color colorMin = new Color(0.4f, 0f, 0f, 1f);

    [Tooltip("Color at stack max-1 — bright red.")]
    [SerializeField] private Color colorMax = new Color(1f, 0.05f, 0.05f, 1f);

    [Header("Shake")]
    [Tooltip("Shake amplitude in pixels at stack 1.")]
    [SerializeField] private float shakeAmplitudeMin = 0.5f;

    [Tooltip("Shake amplitude in pixels at max stack.")]
    [SerializeField] private float shakeAmplitudeMax = 6f;

    [Tooltip("Shake frequency — higher = faster jitter.")]
    [SerializeField] private float shakeFrequency = 28f;

    [Header("Rainbow (max stack only)")]
    [Tooltip("How fast the rainbow hue cycles, in hue units per second (0-1 range).")]
    [SerializeField] private float rainbowSpeed = 0.6f;

    [Tooltip("Hue offset between each character at max stack.")]
    [SerializeField] private float rainbowCharOffset = 0.15f;

    private TextMeshProUGUI label;
    private int currentStack = 0;

    private void Awake()
    {
        label = GetComponent<TextMeshProUGUI>();
    }

    /// <summary>
    /// Call this from BeerGaugeUI whenever the stack value changes or each frame while active.
    /// </summary>
    public void UpdateEffect(int stack)
    {
        currentStack = Mathf.Clamp(stack, 0, maxStack);

        if (currentStack == 0) return;

        // Update geometry so vertex arrays are fresh
        label.ForceMeshUpdate();

        if (currentStack >= maxStack)
        {
            ApplyRainbow();
        }
        else
        {
            ApplyStackColor();
        }

        ApplyShake();

        // Push modified vertices to the mesh
        label.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32 | TMP_VertexDataUpdateFlags.Vertices);
    }

    // ── Color ─────────────────────────────────────────────────────────────────

    private void ApplyStackColor()
    {
        float t = maxStack > 1 ? (float)(currentStack - 1) / (maxStack - 2) : 1f;
        Color stackColor = Color.Lerp(colorMin, colorMax, t);

        TMP_TextInfo textInfo = label.textInfo;

        for (int c = 0; c < textInfo.characterCount; c++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[c];
            if (!charInfo.isVisible) continue;

            int materialIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;

            Color32 col32 = stackColor;
            textInfo.meshInfo[materialIndex].colors32[vertexIndex + 0] = col32;
            textInfo.meshInfo[materialIndex].colors32[vertexIndex + 1] = col32;
            textInfo.meshInfo[materialIndex].colors32[vertexIndex + 2] = col32;
            textInfo.meshInfo[materialIndex].colors32[vertexIndex + 3] = col32;
        }
    }

    private void ApplyRainbow()
    {
        TMP_TextInfo textInfo = label.textInfo;
        float baseHue = (Time.time * rainbowSpeed) % 1f;

        for (int c = 0; c < textInfo.characterCount; c++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[c];
            if (!charInfo.isVisible) continue;

            float hue = (baseHue + c * rainbowCharOffset) % 1f;
            Color rainbow = Color.HSVToRGB(hue, 1f, 1f);

            int materialIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;

            // Slight gradient per character: top verts slightly brighter
            Color32 topColor = rainbow;
            Color32 bottomColor = Color.Lerp(rainbow, Color.black, 0.3f);

            textInfo.meshInfo[materialIndex].colors32[vertexIndex + 0] = bottomColor;
            textInfo.meshInfo[materialIndex].colors32[vertexIndex + 1] = bottomColor;
            textInfo.meshInfo[materialIndex].colors32[vertexIndex + 2] = topColor;
            textInfo.meshInfo[materialIndex].colors32[vertexIndex + 3] = topColor;
        }
    }

    // ── Shake ─────────────────────────────────────────────────────────────────

    private void ApplyShake()
    {
        float t = maxStack > 1 ? (float)(currentStack - 1) / (maxStack - 1) : 1f;
        float amplitude = Mathf.Lerp(shakeAmplitudeMin, shakeAmplitudeMax, t);

        TMP_TextInfo textInfo = label.textInfo;

        for (int c = 0; c < textInfo.characterCount; c++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[c];
            if (!charInfo.isVisible) continue;

            // Each character gets an independent noise offset so they jitter separately
            float offsetX = Mathf.Sin(Time.time * shakeFrequency + c * 1.73f) * amplitude;
            float offsetY = Mathf.Cos(Time.time * shakeFrequency + c * 2.51f) * amplitude;
            Vector3 shake = new Vector3(offsetX, offsetY, 0f);

            int materialIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;

            textInfo.meshInfo[materialIndex].vertices[vertexIndex + 0] += shake;
            textInfo.meshInfo[materialIndex].vertices[vertexIndex + 1] += shake;
            textInfo.meshInfo[materialIndex].vertices[vertexIndex + 2] += shake;
            textInfo.meshInfo[materialIndex].vertices[vertexIndex + 3] += shake;
        }
    }
}
