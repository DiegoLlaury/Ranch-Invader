using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Génère un champ de blés selon une forme choisie : grille, disque plein ou anneau.
/// Tous les blés sont fusionnés en un seul mesh combiné pour minimiser les draw calls.
/// </summary>
[ExecuteInEditMode]
public class FieldGenerator : MonoBehaviour
{
    public enum FieldShape
    {
        Grid,
        Disc,
        Ring
    }

    [Header("Sprite")]
    [Tooltip("Sprite de blé à afficher")]
    public Sprite wheatSprite;

    [Tooltip("Matériau à utiliser (doit avoir la texture du sprite assignée)")]
    public Material wheatMaterial;

    [Header("Field Shape")]
    [Tooltip("Forme de la zone de génération")]
    public FieldShape shape = FieldShape.Grid;

    // --- Grille ---
    [Header("Grid Settings")]
    [Tooltip("Nombre de blés sur l'axe X (Grid uniquement)")]
    public int countX = 10;

    [Tooltip("Nombre de blés sur l'axe Z (Grid uniquement)")]
    public int countZ = 10;

    [Tooltip("Espacement entre chaque blé")]
    public float spacing = 1f;

    // --- Disque / Anneau ---
    [Header("Circle Settings")]
    [Tooltip("Rayon extérieur du disque ou de l'anneau")]
    public float radius = 5f;

    [Tooltip("Épaisseur de l'anneau en nombre de rangées (Ring uniquement)")]
    [Min(1)]
    public int ringRows = 1;

    [Header("Wheat Size")]
    [Tooltip("Taille de base d'un blé (largeur, hauteur)")]
    public Vector2 wheatSize = new Vector2(1f, 2f);

    [Tooltip("Variation aléatoire de taille (0 = aucune, 1 = ±100%)")]
    [Range(0f, 1f)]
    public float sizeVariation = 0.2f;

    [Header("Wheat Offset")]
    [Tooltip("Décalage vertical du pivot (0 = centré, >0 = décalé vers le haut)")]
    [Range(-5f, 5f)]
    public float pivotOffsetY = 0f;

    [Tooltip("Variation aléatoire de position horizontale (rayon max)")]
    public float positionJitter = 0.2f;

    [Header("Color Variation")]
    [Tooltip("Couleur de base du blé")]
    public Color baseColor = Color.white;

    [Tooltip("Applique une légère variation de luminosité aléatoire")]
    public bool useColorVariation = true;

    [Tooltip("Amplitude de la variation de luminosité")]
    [Range(0f, 0.5f)]
    public float brightnessVariation = 0.15f;

    [Header("Generation")]
    [Tooltip("Graine aléatoire (même graine = même champ)")]
    public int randomSeed = 42;

    [Tooltip("Régénère le champ automatiquement quand un paramètre change")]
    public bool autoRefresh = true;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    // Nombre de blés générés (pour l'Inspector)
    private int lastGeneratedCount;

    // --- Cache des paramètres pour autoRefresh ---
    private FieldShape lastShape;
    private int lastCountX;
    private int lastCountZ;
    private float lastSpacing;
    private float lastRadius;
    private int lastRingRows;
    private Vector2 lastWheatSize;
    private float lastSizeVariation;
    private float lastPivotOffsetY;
    private float lastPositionJitter;
    private int lastRandomSeed;
    private bool lastUseColorVariation;
    private float lastBrightnessVariation;

    // Données d'une instance de blé à inscrire dans le mesh
    private struct WheatInstance
    {
        public Vector3 localPosition;
        public Vector2 size;
        public Color color;
        public float verticalOffset;
    }

    void Awake()
    {
        FetchComponents();
    }

    void OnEnable()
    {
        FetchComponents();
        Generate();
    }

    void OnDisable()
    {
        Clear();
    }

    void Update()
    {
        if (!Application.isPlaying && autoRefresh && HasParametersChanged())
        {
            Generate();
        }
    }

    void FetchComponents()
    {
        // Vérifie si plusieurs FieldGenerator coexistent sur ce GameObject
        FieldGenerator[] siblings = GetComponents<FieldGenerator>();
        if (siblings.Length > 1)
        {
            Debug.LogError($"[FieldGenerator] Plusieurs FieldGenerator détectés sur '{gameObject.name}'. " +
                           "Chaque forme doit être sur un GameObject séparé.");
            return;
        }

        if (meshFilter == null)
            meshFilter = gameObject.GetComponent<MeshFilter>() ?? gameObject.AddComponent<MeshFilter>();

        if (meshRenderer == null)
            meshRenderer = gameObject.GetComponent<MeshRenderer>() ?? gameObject.AddComponent<MeshRenderer>();
    }

    bool HasParametersChanged()
    {
        return lastShape != shape
            || lastCountX != countX
            || lastCountZ != countZ
            || lastSpacing != spacing
            || lastRadius != radius
            || lastRingRows != ringRows
            || lastWheatSize != wheatSize
            || lastSizeVariation != sizeVariation
            || lastPivotOffsetY != pivotOffsetY
            || lastPositionJitter != positionJitter
            || lastRandomSeed != randomSeed
            || lastUseColorVariation != useColorVariation
            || lastBrightnessVariation != brightnessVariation;
    }

    void CacheParameters()
    {
        lastShape = shape;
        lastCountX = countX;
        lastCountZ = countZ;
        lastSpacing = spacing;
        lastRadius = radius;
        lastRingRows = ringRows;
        lastWheatSize = wheatSize;
        lastSizeVariation = sizeVariation;
        lastPivotOffsetY = pivotOffsetY;
        lastPositionJitter = positionJitter;
        lastRandomSeed = randomSeed;
        lastUseColorVariation = useColorVariation;
        lastBrightnessVariation = brightnessVariation;
    }

    /// <summary>Efface le mesh combiné.</summary>
    public void Clear()
    {
        if (meshFilter == null)
            FetchComponents();

        if (meshFilter.sharedMesh != null)
        {
            DestroyImmediate(meshFilter.sharedMesh, true);
            meshFilter.sharedMesh = null;
        }

        lastGeneratedCount = 0;
    }

    /// <summary>Génère le champ complet en un seul mesh combiné.</summary>
    public void Generate()
    {
        if (wheatSprite == null)
        {
            Debug.LogWarning($"[FieldGenerator] Aucun sprite assigné sur {gameObject.name}.");
            return;
        }

        if (meshFilter == null)
            FetchComponents();

        Clear();
        CacheParameters();
        ApplyMaterial();

        Random.State previousState = Random.state;
        Random.InitState(randomSeed);

        List<WheatInstance> instances = CollectInstances();
        lastGeneratedCount = instances.Count;

        if (instances.Count > 0)
            BuildCombinedMesh(instances);

        Random.state = previousState;
    }

    // -------------------------------------------------------------------------
    // Collecte des positions selon la forme
    // -------------------------------------------------------------------------

    List<WheatInstance> CollectInstances()
    {
        switch (shape)
        {
            case FieldShape.Grid:  return CollectGrid();
            case FieldShape.Disc:  return CollectDisc();
            case FieldShape.Ring:  return CollectRing();
            default:               return new List<WheatInstance>();
        }
    }

    List<WheatInstance> CollectGrid()
    {
        var instances = new List<WheatInstance>(countX * countZ);

        float totalWidth = (countX - 1) * spacing;
        float totalDepth = (countZ - 1) * spacing;
        Vector3 originLocal = new Vector3(-totalWidth * 0.5f, 0f, -totalDepth * 0.5f);

        for (int x = 0; x < countX; x++)
        {
            for (int z = 0; z < countZ; z++)
            {
                Vector3 localPos = originLocal + new Vector3(
                    x * spacing + Random.Range(-positionJitter, positionJitter),
                    0f,
                    z * spacing + Random.Range(-positionJitter, positionJitter)
                );

                instances.Add(BuildInstance(localPos));
            }
        }

        return instances;
    }

    List<WheatInstance> CollectDisc()
    {
        float safeSpacing = Mathf.Max(0.01f, spacing);
        int half = Mathf.CeilToInt(radius / safeSpacing);
        var instances = new List<WheatInstance>();

        for (int x = -half; x <= half; x++)
        {
            for (int z = -half; z <= half; z++)
            {
                float px = x * safeSpacing;
                float pz = z * safeSpacing;

                if (px * px + pz * pz <= radius * radius)
                {
                    Vector3 localPos = new Vector3(
                        px + Random.Range(-positionJitter, positionJitter),
                        0f,
                        pz + Random.Range(-positionJitter, positionJitter)
                    );

                    instances.Add(BuildInstance(localPos));
                }
            }
        }

        return instances;
    }

    List<WheatInstance> CollectRing()
    {
        float safeSpacing = Mathf.Max(0.01f, spacing);
        int half = Mathf.CeilToInt(radius / safeSpacing);

        float effectiveInner = Mathf.Max(0f, radius - ringRows * safeSpacing);
        float outerSq = radius * radius;
        float innerSq = effectiveInner * effectiveInner;

        var instances = new List<WheatInstance>();

        for (int x = -half; x <= half; x++)
        {
            for (int z = -half; z <= half; z++)
            {
                float px = x * safeSpacing;
                float pz = z * safeSpacing;
                float distSq = px * px + pz * pz;

                if (distSq <= outerSq && distSq >= innerSq)
                {
                    Vector3 localPos = new Vector3(
                        px + Random.Range(-positionJitter, positionJitter),
                        0f,
                        pz + Random.Range(-positionJitter, positionJitter)
                    );

                    instances.Add(BuildInstance(localPos));
                }
            }
        }

        return instances;
    }

    WheatInstance BuildInstance(Vector3 localPosition)
    {
        float sizeFactor = 1f + Random.Range(-sizeVariation, sizeVariation);
        Vector2 finalSize = wheatSize * sizeFactor;

        Color finalColor = baseColor;
        if (useColorVariation)
        {
            float brightness = 1f + Random.Range(-brightnessVariation, brightnessVariation);
            finalColor = new Color(
                Mathf.Clamp01(baseColor.r * brightness),
                Mathf.Clamp01(baseColor.g * brightness),
                Mathf.Clamp01(baseColor.b * brightness),
                baseColor.a
            );
        }

        return new WheatInstance
        {
            localPosition = localPosition,
            size = finalSize,
            color = finalColor,
            verticalOffset = finalSize.y * Mathf.Abs(pivotOffsetY)
        };
    }

    // -------------------------------------------------------------------------
    // Construction du mesh combiné
    // -------------------------------------------------------------------------

    void BuildCombinedMesh(List<WheatInstance> instances)
    {
        // Chaque blé = 2 quads double-face = 2 × 4 verts = 8 verts
        // Chaque quad double-face = 2 triangles × 2 faces = 4 triangles = 12 indices
        int totalVerts = instances.Count * 8;
        int totalIndices = instances.Count * 24;

        var vertices  = new List<Vector3>(totalVerts);
        var uvs       = new List<Vector2>(totalVerts);
        var colors    = new List<Color>(totalVerts);
        var triangles = new List<int>(totalIndices);

        // Calcul des UVs depuis le sprite
        Rect texRect = wheatSprite.textureRect;
        float texW = wheatSprite.texture.width;
        float texH = wheatSprite.texture.height;
        Vector2 uvMin = new Vector2(texRect.xMin / texW, texRect.yMin / texH);
        Vector2 uvMax = new Vector2(texRect.xMax / texW, texRect.yMax / texH);

        foreach (WheatInstance inst in instances)
        {
            float halfW = inst.size.x * 0.5f;
            float yBot  = inst.verticalOffset;
            float yTop  = inst.verticalOffset + inst.size.y;
            Vector3 p   = inst.localPosition;

            // Quad A — face à Z
            AddDoubleQuad(
                p + new Vector3(-halfW, yBot, 0f),
                p + new Vector3( halfW, yBot, 0f),
                p + new Vector3(-halfW, yTop, 0f),
                p + new Vector3( halfW, yTop, 0f),
                uvMin, uvMax, inst.color,
                vertices, uvs, colors, triangles
            );

            // Quad B — face à X (croisé à 90°)
            AddDoubleQuad(
                p + new Vector3(0f, yBot, -halfW),
                p + new Vector3(0f, yBot,  halfW),
                p + new Vector3(0f, yTop, -halfW),
                p + new Vector3(0f, yTop,  halfW),
                uvMin, uvMax, inst.color,
                vertices, uvs, colors, triangles
            );
        }

        Mesh mesh = new Mesh
        {
            name = "FieldCombinedMesh",
            indexFormat = IndexFormat.UInt32
        };

        mesh.SetVertices(vertices);
        mesh.SetUVs(0, uvs);
        mesh.SetColors(colors);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateBounds();

        meshFilter.sharedMesh = mesh;
    }

    /// <summary>
    /// Ajoute un quad double-face (4 sommets) dans les listes du mesh combiné.
    /// Les 4 sommets sont : bas-gauche, bas-droite, haut-gauche, haut-droite.
    /// </summary>
    void AddDoubleQuad(
        Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3,
        Vector2 uvMin, Vector2 uvMax,
        Color color,
        List<Vector3> verts, List<Vector2> uvs, List<Color> colors, List<int> tris)
    {
        int b = verts.Count;

        verts.Add(v0); verts.Add(v1); verts.Add(v2); verts.Add(v3);

        uvs.Add(new Vector2(uvMin.x, uvMin.y));
        uvs.Add(new Vector2(uvMax.x, uvMin.y));
        uvs.Add(new Vector2(uvMin.x, uvMax.y));
        uvs.Add(new Vector2(uvMax.x, uvMax.y));

        colors.Add(color); colors.Add(color); colors.Add(color); colors.Add(color);

        // Face avant
        tris.Add(b);     tris.Add(b + 2); tris.Add(b + 1);
        tris.Add(b + 1); tris.Add(b + 2); tris.Add(b + 3);

        // Face arrière
        tris.Add(b);     tris.Add(b + 1); tris.Add(b + 2);
        tris.Add(b + 1); tris.Add(b + 3); tris.Add(b + 2);
    }

    // -------------------------------------------------------------------------
    // Matériau
    // -------------------------------------------------------------------------

    void ApplyMaterial()
    {
        if (wheatMaterial != null)
        {
            meshRenderer.sharedMaterial = wheatMaterial;
            return;
        }

        // Fallback : créer un matériau unlit avec la texture du sprite
        Shader fallbackShader = Shader.Find("Universal Render Pipeline/Unlit");
        if (fallbackShader == null)
            fallbackShader = Shader.Find("Sprites/Default");

        if (fallbackShader != null)
        {
            Material mat = new Material(fallbackShader);
            mat.mainTexture = wheatSprite.texture;
            meshRenderer.sharedMaterial = mat;
        }
        else
        {
            Debug.LogWarning("[FieldGenerator] Aucun shader fallback trouvé. Assignez un matériau manuellement.");
        }
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        countX    = Mathf.Max(1, countX);
        countZ    = Mathf.Max(1, countZ);
        spacing   = Mathf.Max(0.01f, spacing);
        radius    = Mathf.Max(0.1f, radius);
        ringRows  = Mathf.Max(1, ringRows);
        wheatSize.x = Mathf.Max(0.01f, wheatSize.x);
        wheatSize.y = Mathf.Max(0.01f, wheatSize.y);
    }

    void OnDrawGizmosSelected()
    {
        switch (shape)
        {
            case FieldShape.Grid: DrawGridGizmo();  break;
            case FieldShape.Disc: DrawDiscGizmo();  break;
            case FieldShape.Ring: DrawRingGizmo();  break;
        }
    }

    void DrawGridGizmo()
    {
        float totalWidth = (countX - 1) * spacing;
        float totalDepth = (countZ - 1) * spacing;

        Gizmos.color = new Color(0.2f, 0.8f, 0.2f, 0.4f);
        Gizmos.DrawWireCube(transform.position,
            new Vector3(totalWidth + spacing, 0.1f, totalDepth + spacing));
    }

    void DrawDiscGizmo()
    {
        Gizmos.color = new Color(0.2f, 0.8f, 0.2f, 0.5f);
        DrawWireCircle(transform.position, radius, 64);
    }

    void DrawRingGizmo()
    {
        float effectiveInner = Mathf.Max(0f, radius - ringRows * Mathf.Max(0.01f, spacing));

        Gizmos.color = new Color(0.2f, 0.8f, 0.2f, 0.5f);
        DrawWireCircle(transform.position, radius, 64);

        Gizmos.color = new Color(0.8f, 0.4f, 0.1f, 0.4f);
        DrawWireCircle(transform.position, effectiveInner, 64);
    }

    void DrawWireCircle(Vector3 center, float r, int segments)
    {
        float step = 2f * Mathf.PI / segments;
        Vector3 prev = center + new Vector3(r, 0f, 0f);
        for (int i = 1; i <= segments; i++)
        {
            float angle = i * step;
            Vector3 next = center + new Vector3(Mathf.Cos(angle) * r, 0f, Mathf.Sin(angle) * r);
            Gizmos.DrawLine(prev, next);
            prev = next;
        }
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(FieldGenerator))]
public class FieldGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        FieldGenerator generator = (FieldGenerator)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Régénérer"))
        {
            generator.Generate();
            EditorUtility.SetDirty(generator);
        }

        if (GUILayout.Button("Effacer"))
        {
            generator.Clear();
            EditorUtility.SetDirty(generator);
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(BuildInfoText(generator), MessageType.Info);
    }

    static string BuildInfoText(FieldGenerator gen)
    {
        MeshFilter mf = gen.GetComponent<MeshFilter>();
        int verts = (mf != null && mf.sharedMesh != null) ? mf.sharedMesh.vertexCount : 0;
        int tris  = (mf != null && mf.sharedMesh != null) ? mf.sharedMesh.triangles.Length / 3 : 0;
        string meshInfo = $"Draw calls : 1\nSommets : {verts}  |  Triangles : {tris}";

        switch (gen.shape)
        {
            case FieldGenerator.FieldShape.Grid:
                int total = gen.countX * gen.countZ;
                return $"Forme : Grille  |  Blés : {total} ({gen.countX} × {gen.countZ})\n{meshInfo}";

            case FieldGenerator.FieldShape.Disc:
                return $"Forme : Disque plein  |  Rayon : {gen.radius}  |  Espacement : {gen.spacing}\n{meshInfo}";

            case FieldGenerator.FieldShape.Ring:
                float eff = Mathf.Max(0f, gen.radius - gen.ringRows * Mathf.Max(0.01f, gen.spacing));
                return $"Forme : Anneau  |  Ext. : {gen.radius}  |  Int. : {eff:F2}  |  Rangées : {gen.ringRows}\n{meshInfo}";

            default:
                return meshInfo;
        }
    }
}
#endif
