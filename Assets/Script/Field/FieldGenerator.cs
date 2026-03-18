using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Génère un champ de blés en croix sur un plane.
/// Chaque blé est composé de deux sprites croisés à 90° pour simuler un effet 3D.
/// </summary>
[ExecuteInEditMode]
public class FieldGenerator : MonoBehaviour
{
    [Header("Sprite")]
    [Tooltip("Sprite de blé à afficher")]
    public Sprite wheatSprite;

    [Tooltip("Matériau à utiliser pour les sprites (laissez vide pour le défaut)")]
    public Material wheatMaterial;

    [Header("Field Grid")]
    [Tooltip("Nombre de blés sur l'axe X")]
    public int countX = 10;

    [Tooltip("Nombre de blés sur l'axe Z")]
    public int countZ = 10;

    [Tooltip("Espacement entre chaque blé")]
    public float spacing = 1f;

    [Header("Wheat Size")]
    [Tooltip("Taille de base d'un blé (largeur, hauteur)")]
    public Vector2 wheatSize = new Vector2(1f, 2f);

    [Tooltip("Variation aléatoire de taille (0 = aucune, 1 = ±100%)")]
    [Range(0f, 1f)]
    public float sizeVariation = 0.2f;

    [Header("Wheat Offset")]
    [Tooltip("Décalage vertical du pivot (0 = centré, -0.5 = ancré en bas)")]
    [Range(-5f, 5f)]
    public float pivotOffsetY = 0f;

    [Tooltip("Variation aléatoire de position horizontale (rayon max)")]
    public float positionJitter = 0.2f;

    [Header("Color Variation")]
    [Tooltip("Couleur de base du blé")]
    public Color baseColor = Color.white;

    [Tooltip("Applique une légère variation de teinte aléatoire")]
    public bool useColorVariation = true;

    [Tooltip("Amplitude de la variation de luminosité")]
    [Range(0f, 0.5f)]
    public float brightnessVariation = 0.15f;

    [Header("Generation")]
    [Tooltip("Graine aléatoire (même graine = même champ)")]
    public int randomSeed = 42;

    [Tooltip("Régénère le champ automatiquement quand un paramètre change")]
    public bool autoRefresh = true;

    private List<GameObject> spawnedWheats = new List<GameObject>();

    private int lastCountX;
    private int lastCountZ;
    private float lastSpacing;
    private Vector2 lastWheatSize;
    private float lastSizeVariation;
    private float lastPivotOffsetY;
    private float lastPositionJitter;
    private int lastRandomSeed;
    private bool lastUseColorVariation;
    private float lastBrightnessVariation;

    void OnEnable()
    {
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

    bool HasParametersChanged()
    {
        return lastCountX != countX
            || lastCountZ != countZ
            || lastSpacing != spacing
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
        lastCountX = countX;
        lastCountZ = countZ;
        lastSpacing = spacing;
        lastWheatSize = wheatSize;
        lastSizeVariation = sizeVariation;
        lastPivotOffsetY = pivotOffsetY;
        lastPositionJitter = positionJitter;
        lastRandomSeed = randomSeed;
        lastUseColorVariation = useColorVariation;
        lastBrightnessVariation = brightnessVariation;
    }

    /// <summary>Efface tous les blés générés.</summary>
    public void Clear()
    {
        foreach (GameObject wheat in spawnedWheats)
        {
            if (wheat != null)
                DestroyImmediate(wheat);
        }

        spawnedWheats.Clear();

        // Sécurité : détruire les enfants résiduels
        List<Transform> toDestroy = new List<Transform>();
        foreach (Transform child in transform)
        {
            if (child.name.StartsWith("Wheat_"))
                toDestroy.Add(child);
        }

        foreach (Transform child in toDestroy)
            DestroyImmediate(child.gameObject);
    }

    /// <summary>Génère le champ complet.</summary>
    public void Generate()
    {
        if (wheatSprite == null)
        {
            Debug.LogWarning($"[FieldGenerator] Aucun sprite assigné sur {gameObject.name}.");
            return;
        }

        Clear();
        CacheParameters();

        Random.State previousState = Random.state;
        Random.InitState(randomSeed);

        float totalWidth = (countX - 1) * spacing;
        float totalDepth = (countZ - 1) * spacing;
        Vector3 origin = transform.position - new Vector3(totalWidth * 0.5f, 0f, totalDepth * 0.5f);

        for (int x = 0; x < countX; x++)
        {
            for (int z = 0; z < countZ; z++)
            {
                float jitterX = Random.Range(-positionJitter, positionJitter);
                float jitterZ = Random.Range(-positionJitter, positionJitter);

                Vector3 position = origin + new Vector3(
                    x * spacing + jitterX,
                    0f,
                    z * spacing + jitterZ
                );

                SpawnWheat(position, x, z);
            }
        }

        Random.state = previousState;
    }

    void SpawnWheat(Vector3 worldPosition, int x, int z)
    {
        GameObject wheatRoot = new GameObject($"Wheat_{x}_{z}");
        wheatRoot.transform.SetParent(transform, worldPositionStays: true);
        wheatRoot.transform.position = worldPosition;
        wheatRoot.transform.rotation = Quaternion.identity;

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

        float verticalOffset = finalSize.y * Mathf.Abs(pivotOffsetY);

        // Sprite A : vertical, face à Z
        CreateSpriteQuad(wheatRoot.transform, "SpriteA", Quaternion.identity, finalSize, finalColor, verticalOffset);

        // Sprite B : croisé à 90° sur Y
        CreateSpriteQuad(wheatRoot.transform, "SpriteB", Quaternion.Euler(0f, 90f, 0f), finalSize, finalColor, verticalOffset);


        spawnedWheats.Add(wheatRoot);
    }

    void CreateSpriteQuad(Transform parent, string spriteName, Quaternion rotation, Vector2 size, Color color, float verticalOffset)
    {
        GameObject spriteObj = new GameObject(spriteName);
        spriteObj.transform.SetParent(parent, worldPositionStays: false);
        spriteObj.transform.localPosition = new Vector3(0f, verticalOffset, 0f);
        spriteObj.transform.localRotation = rotation;
        spriteObj.transform.localScale = new Vector3(size.x, size.y, 1f);

        SpriteRenderer sr = spriteObj.AddComponent<SpriteRenderer>();
        sr.sprite = wheatSprite;
        sr.color = color;

        if (wheatMaterial != null)
            sr.material = wheatMaterial;
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        countX = Mathf.Max(1, countX);
        countZ = Mathf.Max(1, countZ);
        spacing = Mathf.Max(0.01f, spacing);
        wheatSize.x = Mathf.Max(0.01f, wheatSize.x);
        wheatSize.y = Mathf.Max(0.01f, wheatSize.y);
    }

    void OnDrawGizmosSelected()
    {
        float totalWidth = (countX - 1) * spacing;
        float totalDepth = (countZ - 1) * spacing;

        Gizmos.color = new Color(0.2f, 0.8f, 0.2f, 0.4f);
        Gizmos.DrawWireCube(
            transform.position,
            new Vector3(totalWidth + spacing, 0.1f, totalDepth + spacing)
        );
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

        if (GUILayout.Button("Regénérer"))
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
        int total = generator.countX * generator.countZ;
        EditorGUILayout.HelpBox(
            $"Total blés : {total} ({generator.countX} × {generator.countZ})\n" +
            $"Quads sprites : {total * 2}",
            MessageType.Info
        );
    }
}
#endif
