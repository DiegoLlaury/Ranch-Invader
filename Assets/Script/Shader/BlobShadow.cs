using UnityEngine;

/// <summary>
/// Projette une fausse ombre circulaire (blob shadow) sur le sol sous un mesh cible.
/// Ajoute ce composant sur un GameObject vide enfant du mesh à ombrager.
/// </summary>
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class BlobShadow : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Le renderer du mesh à ombrager")]
    public Renderer targetRenderer;

    [Header("Shadow Settings")]
    [Tooltip("Multiplicateur de taille par rapport au footprint XZ du mesh")]
    [Range(0.5f, 3f)]
    public float radiusMultiplier = 1.2f;

    [Tooltip("Désactive le calcul automatique et utilise une taille fixe")]
    public bool useManualScale = false;

    [Tooltip("Taille manuelle de l'ombre en unités monde (X = largeur, Y = profondeur)")]
    public Vector2 manualSize = new Vector2(1f, 1f);

    [Tooltip("Opacité maximale de l'ombre au centre")]
    [Range(0f, 1f)]
    public float opacity = 0.5f;

    [Tooltip("Douceur du bord (plus grand = bord plus doux)")]
    [Range(0.01f, 1f)]
    public float softness = 0.35f;

    [Tooltip("Décalage vertical au-dessus du sol pour éviter le z-fighting")]
    public float groundOffset = 0.01f;

    [Tooltip("Layers considérés comme le sol")]
    public LayerMask groundLayers = -1;

    [Header("Performance")]
    [Tooltip("Recalcule position et taille à chaque frame (utile pour les meshes mobiles)")]
    public bool dynamicUpdate = false;

    private MeshRenderer shadowRenderer;
    private Material shadowMaterial;

    private static readonly int OpacityId = Shader.PropertyToID("_Opacity");
    private static readonly int SoftnessId = Shader.PropertyToID("_Softness");

    void Awake()
    {
        shadowRenderer = GetComponent<MeshRenderer>();

        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter.sharedMesh == null)
            meshFilter.sharedMesh = BuildQuadMesh();

        Shader shader = Shader.Find("Custom/BlobShadow");
        if (shader == null)
        {
            Debug.LogError("[BlobShadow] Shader 'Custom/BlobShadow' introuvable !");
            enabled = false;
            return;
        }

        shadowMaterial = new Material(shader);
        ApplyMaterialProperties();
        shadowRenderer.material = shadowMaterial;
        shadowRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        shadowRenderer.receiveShadows = false;
    }

    void Start()
    {
        UpdateShadow();
    }

    void Update()
    {
        if (dynamicUpdate)
            UpdateShadow();
    }

    /// <summary>
    /// Repositionne et redimensionne le quad d'ombre sous le mesh cible.
    /// </summary>
    public void UpdateShadow()
    {
        if (targetRenderer == null) return;

        Bounds bounds = targetRenderer.bounds;

        // Taille du quad basée sur le footprint XZ du mesh
        float width = useManualScale ? manualSize.x : bounds.size.x * radiusMultiplier;
        float depth = useManualScale ? manualSize.y : bounds.size.z * radiusMultiplier;

        // Compense le scale hérité du parent pour garder une taille monde cohérente
        Vector3 parentScale = transform.parent != null ? transform.parent.lossyScale : Vector3.one;
        transform.localScale = new Vector3(
            width / Mathf.Max(parentScale.x, 0.0001f),
            1f,
            depth / Mathf.Max(parentScale.z, 0.0001f)
        );

        // Orientation : quad à plat au sol
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        // Trouver le sol par raycast depuis le centre du mesh
        Vector3 rayOrigin = bounds.center;
        float groundY = bounds.min.y; // fallback : base du mesh

        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 200f, groundLayers))
            groundY = hit.point.y;

        transform.position = new Vector3(bounds.center.x, groundY + groundOffset, bounds.center.z);

        ApplyMaterialProperties();
    }

    private void ApplyMaterialProperties()
    {
        if (shadowMaterial == null) return;
        shadowMaterial.SetFloat(OpacityId, opacity);
        shadowMaterial.SetFloat(SoftnessId, softness);
    }

    private static Mesh BuildQuadMesh()
    {
        Mesh mesh = new Mesh { name = "BlobShadowQuad" };

        mesh.vertices = new Vector3[]
        {
            new Vector3(-0.5f, 0f, -0.5f),
            new Vector3( 0.5f, 0f, -0.5f),
            new Vector3( 0.5f, 0f,  0.5f),
            new Vector3(-0.5f, 0f,  0.5f)
        };

        mesh.uv = new Vector2[]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(1, 1),
            new Vector2(0, 1)
        };

        mesh.triangles = new int[] { 0, 2, 1, 0, 3, 2 };
        mesh.RecalculateNormals();

        return mesh;
    }

    void OnValidate()
    {
        ApplyMaterialProperties();
    }

    void OnDestroy()
    {
        if (shadowMaterial != null)
            Destroy(shadowMaterial);
    }
}
