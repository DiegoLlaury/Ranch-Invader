using UnityEngine;

/// <summary>
/// Renders a procedural electric arc between two world-space transforms
/// using a LineRenderer. The arc is animated with fractal Perlin noise to simulate
/// electricity. Call Deactivate() to cleanly disable it.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class ElectricArcRenderer : MonoBehaviour
{
    [Header("Targets")]
    [Tooltip("Origin of the arc (e.g., the generator).")]
    [SerializeField] private Transform origin;

    [Tooltip("Destination of the arc (e.g., the force field center).")]
    [SerializeField] private Transform destination;

    [Header("Arc Shape")]
    [Tooltip("Number of segments along the arc. More = smoother but heavier.")]
    [SerializeField] private int segmentCount = 30;

    [Tooltip("Fixed perpendicular offset at the midpoint creating the visible curve. This is what makes it look like an arc.")]
    [SerializeField] private float baseArcOffset = 0.6f;

    [Tooltip("Maximum perpendicular displacement of each segment from the straight line.")]
    [SerializeField] private float noiseAmplitude = 0.25f;

    [Tooltip("Scale of the primary Perlin noise pattern. Higher = more chaotic.")]
    [SerializeField] private float noiseScale = 2.5f;

    [Tooltip("Speed at which the arc animates over time.")]
    [SerializeField] private float animationSpeed = 10f;

    [Header("Fractal Detail")]
    [Tooltip("Number of noise octaves layered on top of each other. More = more detailed electricity.")]
    [Range(1, 4)]
    [SerializeField] private int octaves = 3;

    [Tooltip("How much each octave's amplitude decreases relative to the previous one.")]
    [Range(0.1f, 0.9f)]
    [SerializeField] private float persistence = 0.5f;

    [Tooltip("How much each octave's frequency increases relative to the previous one.")]
    [Range(1f, 4f)]
    [SerializeField] private float lacunarity = 2f;

    [Header("Slow Drift")]
    [Tooltip("Amplitude of a slow, large-scale drift applied on top of the fast noise. Gives the arc a living, breathing quality.")]
    [SerializeField] private float driftAmplitude = 0.15f;

    [Tooltip("Speed of the slow drift.")]
    [SerializeField] private float driftSpeed = 0.8f;

    [Header("Appearance")]
    [Tooltip("Width of the arc line at its start.")]
    [SerializeField] private float startWidth = 0.05f;

    [Tooltip("Width of the arc line at its end.")]
    [SerializeField] private float endWidth = 0.02f;

    private LineRenderer lineRenderer;
    private bool isActive = true;
    private float timeOffset;
    private float driftOffsetX;
    private float driftOffsetY;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = segmentCount;
        lineRenderer.startWidth = startWidth;
        lineRenderer.endWidth = endWidth;
        lineRenderer.useWorldSpace = true;

        // Randomize offsets so multiple arcs never animate in sync
        timeOffset = Random.Range(0f, 100f);
        driftOffsetX = Random.Range(0f, 100f);
        driftOffsetY = Random.Range(0f, 100f);
    }

    private void Update()
    {
        if (!isActive || origin == null || destination == null)
            return;

        UpdateArc();
    }

    private void UpdateArc()
    {
        Vector3 start = origin.position;
        Vector3 end = destination.position;
        Vector3 direction = end - start;

        // Robust perpendicular frame valid regardless of arc orientation
        Vector3 forward = direction.normalized;
        Vector3 worldRef = (Mathf.Abs(Vector3.Dot(forward, Vector3.up)) < 0.9f) ? Vector3.up : Vector3.right;
        Vector3 right = Vector3.Cross(forward, worldRef).normalized;
        Vector3 up = Vector3.Cross(right, forward).normalized;

        float time = Time.time * animationSpeed + timeOffset;
        float driftTime = Time.time * driftSpeed;

        for (int i = 0; i < segmentCount; i++)
        {
            float t = (float)i / (segmentCount - 1);
            Vector3 basePosition = Vector3.Lerp(start, end, t);

            // Sin envelope: zero at endpoints, peak at midpoint
            float envelope = Mathf.Sin(t * Mathf.PI);

            // --- Deterministic base curve ---
            // Gives the arc its visible bow shape, independent of noise
            Vector3 arcCurve = up * baseArcOffset * envelope;

            // --- Fractal (fBm) noise ---
            float noiseX = 0f;
            float noiseY = 0f;
            float amplitude = 1f;
            float frequency = 1f;
            float normalization = 0f;

            for (int oct = 0; oct < octaves; oct++)
            {
                float sampleX = t * noiseScale * frequency + time * frequency;
                float sampleY = t * noiseScale * frequency + time * frequency + 17.3f;

                noiseX += (Mathf.PerlinNoise(sampleX, (float)oct * 31.7f) - 0.5f) * 2f * amplitude;
                noiseY += (Mathf.PerlinNoise((float)oct * 31.7f, sampleY) - 0.5f) * 2f * amplitude;

                normalization += amplitude;
                amplitude *= persistence;
                frequency *= lacunarity;
            }

            noiseX /= normalization;
            noiseY /= normalization;

            // --- Slow drift ---
            float driftX = (Mathf.PerlinNoise(t * 0.8f + driftTime + driftOffsetX, 0f) - 0.5f) * 2f;
            float driftY = (Mathf.PerlinNoise(0f, t * 0.8f + driftTime + driftOffsetY) - 0.5f) * 2f;

            Vector3 fastDisplacement = (right * noiseX + up * noiseY) * noiseAmplitude;
            Vector3 slowDrift = (right * driftX + up * driftY) * driftAmplitude;

            lineRenderer.SetPosition(i, basePosition + arcCurve + (fastDisplacement + slowDrift) * envelope);
        }
    }


    /// <summary>
    /// Disables the arc immediately. Called by GeneratorObject on death.
    /// </summary>
    public void Deactivate()
    {
        isActive = false;
        lineRenderer.enabled = false;
    }
}
