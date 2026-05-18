using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class FieldGenerator : MonoBehaviour
{
    public enum FieldShape { Grid, Disc, Ring }

    private const string MeshChildName = "__WheatMesh__";

    [Header("Sprite")]
    public Sprite wheatSprite;
    public Material wheatMaterial;

    [Header("Field Shape")]
    public FieldShape shape = FieldShape.Grid;

    [Header("Grid Settings")]
    public int countX = 10;
    public int countZ = 10;
    public float spacing = 1f;

    [Header("Circle Settings")]
    public float radius = 1f;
    [Tooltip("Nombre cible de blés dans le disque ou l'anneau")]
    [Min(1)] public int circleCount = 30;
    [Tooltip("Épaisseur de l'anneau en nombre de rangées (Ring uniquement)")]
    [Min(1)] public int ringRows = 1;

    [Header("Wheat Size")]
    public Vector2 wheatSize = new Vector2(0.2f, 0.4f);
    [Range(0f, 1f)] public float sizeVariation = 0.2f;

    [Header("Wheat Offset")]
    [Range(-5f, 5f)] public float pivotOffsetY = 0f;
    public float positionJitter = 0.2f;

    [Header("Color Variation")]
    public Color baseColor = Color.white;
    public bool useColorVariation = true;
    [Range(0f, 0.5f)] public float brightnessVariation = 0.15f;

    [Header("Generation")]
    public int randomSeed = 42;
    public bool autoRefresh = true;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private int lastGeneratedCount;

    private FieldShape lastShape;
    private int lastCountX, lastCountZ, lastCircleCount, lastRingRows, lastRandomSeed;
    private float lastSpacing, lastRadius, lastSizeVariation, lastPivotOffsetY, lastPositionJitter, lastBrightnessVariation;
    private Vector2 lastWheatSize;
    private bool lastUseColorVariation;

    private struct WheatInstance
    {
        public Vector3 localPosition;
        public Vector2 size;
        public Color color;
        public float verticalOffset;
    }

    void Awake() => FetchComponents();
    void OnEnable() { FetchComponents(); Generate(); }
    void OnDisable() => Clear();

    void Update()
    {
        if (!Application.isPlaying && autoRefresh && HasParametersChanged())
            Generate();
    }

    void FetchComponents()
    {
        if (GetComponents<FieldGenerator>().Length > 1)
        {
            Debug.LogError($"[FieldGenerator] Plusieurs FieldGenerator sur '{gameObject.name}'. Chaque forme doit être sur un GameObject séparé.");
            return;
        }

        Transform meshChild = transform.Find(MeshChildName);
        if (meshChild == null)
        {
            var go = new GameObject(MeshChildName);
            go.transform.SetParent(transform, worldPositionStays: false);
            meshChild = go.transform;
#if UNITY_EDITOR
            GameObjectUtility.SetStaticEditorFlags(go, 0);
#endif
        }

        if (!meshChild.TryGetComponent(out meshFilter))
            meshFilter = meshChild.gameObject.AddComponent<MeshFilter>();
        if (!meshChild.TryGetComponent(out meshRenderer))
            meshRenderer = meshChild.gameObject.AddComponent<MeshRenderer>();
    }

    bool HasParametersChanged() =>
        lastShape != shape || lastCountX != countX || lastCountZ != countZ ||
        lastSpacing != spacing || lastRadius != radius || lastCircleCount != circleCount ||
        lastRingRows != ringRows || lastWheatSize != wheatSize || lastSizeVariation != sizeVariation ||
        lastPivotOffsetY != pivotOffsetY || lastPositionJitter != positionJitter ||
        lastRandomSeed != randomSeed || lastUseColorVariation != useColorVariation ||
        lastBrightnessVariation != brightnessVariation;

    void CacheParameters()
    {
        lastShape = shape; lastCountX = countX; lastCountZ = countZ;
        lastSpacing = spacing; lastRadius = radius; lastCircleCount = circleCount; lastRingRows = ringRows;
        lastWheatSize = wheatSize; lastSizeVariation = sizeVariation;
        lastPivotOffsetY = pivotOffsetY; lastPositionJitter = positionJitter;
        lastRandomSeed = randomSeed; lastUseColorVariation = useColorVariation;
        lastBrightnessVariation = brightnessVariation;
    }

    /// <summary>Efface le mesh combiné.</summary>
    public void Clear()
    {
        if (meshFilter == null) FetchComponents();
        if (meshFilter != null && meshFilter.sharedMesh != null)
        {
            DestroyImmediate(meshFilter.sharedMesh, true);
            meshFilter.sharedMesh = null;
        }
        lastGeneratedCount = 0;
    }

    /// <summary>Génère le champ complet en un seul mesh combiné.</summary>
    public void Generate()
    {
        if (wheatSprite == null) { Debug.LogWarning($"[FieldGenerator] Aucun sprite sur '{gameObject.name}'."); return; }
        if (GetComponents<FieldGenerator>().Length > 1) { Debug.LogError($"[FieldGenerator] Conflit multi-composants sur '{gameObject.name}'."); return; }
        if (meshFilter == null) FetchComponents();

        Clear();
        CacheParameters();
        ApplyMaterial();

        Random.State prev = Random.state;
        Random.InitState(randomSeed);

        List<WheatInstance> instances = shape switch
        {
            FieldShape.Grid => CollectGrid(),
            FieldShape.Disc => CollectDisc(),
            FieldShape.Ring => CollectRing(),
            _ => new List<WheatInstance>()
        };

        lastGeneratedCount = instances.Count;
        if (instances.Count > 0) BuildCombinedMesh(instances);

        Random.state = prev;
    }

    List<WheatInstance> CollectGrid()
    {
        var list = new List<WheatInstance>(countX * countZ);
        Vector3 origin = new Vector3(-(countX - 1) * spacing * 0.5f, 0f, -(countZ - 1) * spacing * 0.5f);
        for (int x = 0; x < countX; x++)
            for (int z = 0; z < countZ; z++)
                list.Add(BuildInstance(origin + new Vector3(
                    x * spacing + Random.Range(-positionJitter, positionJitter), 0f,
                    z * spacing + Random.Range(-positionJitter, positionJitter))));
        return list;
    }

    List<WheatInstance> CollectDisc()
    {
        float area = Mathf.PI * radius * radius;
        float s = Mathf.Max(0.01f, Mathf.Sqrt(area / Mathf.Max(1, circleCount)));
        int half = Mathf.CeilToInt(radius / s);
        float rSq = radius * radius;
        var list = new List<WheatInstance>();
        for (int x = -half; x <= half; x++)
            for (int z = -half; z <= half; z++)
            {
                float px = x * s, pz = z * s;
                if (px * px + pz * pz <= rSq)
                    list.Add(BuildInstance(new Vector3(
                        px + Random.Range(-positionJitter, positionJitter), 0f,
                        pz + Random.Range(-positionJitter, positionJitter))));
            }
        return list;
    }

    List<WheatInstance> CollectRing()
    {
        float innerRadius = Mathf.Max(0f, radius - ringRows * Mathf.Max(0.01f, spacing));
        float ringArea = Mathf.PI * (radius * radius - innerRadius * innerRadius);
        float s = Mathf.Max(0.01f, Mathf.Sqrt(ringArea / Mathf.Max(1, circleCount)));
        int half = Mathf.CeilToInt(radius / s);
        float outerSq = radius * radius;
        float innerSq = innerRadius * innerRadius;
        var list = new List<WheatInstance>();
        for (int x = -half; x <= half; x++)
            for (int z = -half; z <= half; z++)
            {
                float px = x * s, pz = z * s, dSq = px * px + pz * pz;
                if (dSq <= outerSq && dSq >= innerSq)
                    list.Add(BuildInstance(new Vector3(
                        px + Random.Range(-positionJitter, positionJitter), 0f,
                        pz + Random.Range(-positionJitter, positionJitter))));
            }
        return list;
    }

    WheatInstance BuildInstance(Vector3 pos)
    {
        float f = 1f + Random.Range(-sizeVariation, sizeVariation);
        Vector2 sz = wheatSize * f;
        Color c = baseColor;
        if (useColorVariation)
        {
            float b = 1f + Random.Range(-brightnessVariation, brightnessVariation);
            c = new Color(Mathf.Clamp01(baseColor.r * b), Mathf.Clamp01(baseColor.g * b), Mathf.Clamp01(baseColor.b * b), baseColor.a);
        }
        return new WheatInstance { localPosition = pos, size = sz, color = c, verticalOffset = sz.y * Mathf.Abs(pivotOffsetY) };
    }

    void BuildCombinedMesh(List<WheatInstance> instances)
    {
        var verts = new List<Vector3>(instances.Count * 8);
        var uvs = new List<Vector2>(instances.Count * 8);
        var cols = new List<Color>(instances.Count * 8);
        var tris = new List<int>(instances.Count * 24);

        Rect tr = wheatSprite.textureRect;
        Vector2 uvMin = new Vector2(tr.xMin / wheatSprite.texture.width, tr.yMin / wheatSprite.texture.height);
        Vector2 uvMax = new Vector2(tr.xMax / wheatSprite.texture.width, tr.yMax / wheatSprite.texture.height);

        // Compensation de la scale du parent : wheatSize est en world units
        Vector3 ls = transform.lossyScale;
        float iX = ls.x != 0f ? 1f / Mathf.Abs(ls.x) : 1f;
        float iY = ls.y != 0f ? 1f / Mathf.Abs(ls.y) : 1f;
        float iZ = ls.z != 0f ? 1f / Mathf.Abs(ls.z) : 1f;

        foreach (var inst in instances)
        {
            float hwX = inst.size.x * 0.5f * iX;
            float hwZ = inst.size.x * 0.5f * iZ;
            float yBot = inst.verticalOffset * iY;
            float yTop = yBot + inst.size.y * iY;
            Vector3 p = inst.localPosition;

            AddDoubleQuad(p + new Vector3(-hwX, yBot, 0), p + new Vector3(hwX, yBot, 0),
                          p + new Vector3(-hwX, yTop, 0), p + new Vector3(hwX, yTop, 0),
                          uvMin, uvMax, inst.color, verts, uvs, cols, tris);

            AddDoubleQuad(p + new Vector3(0, yBot, -hwZ), p + new Vector3(0, yBot, hwZ),
                          p + new Vector3(0, yTop, -hwZ), p + new Vector3(0, yTop, hwZ),
                          uvMin, uvMax, inst.color, verts, uvs, cols, tris);
        }

        var mesh = new Mesh { name = "FieldCombinedMesh", indexFormat = IndexFormat.UInt32 };
        mesh.SetVertices(verts); mesh.SetUVs(0, uvs); mesh.SetColors(cols);
        mesh.SetTriangles(tris, 0); mesh.RecalculateBounds();
        meshFilter.sharedMesh = mesh;
    }

    void AddDoubleQuad(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3,
        Vector2 uvMin, Vector2 uvMax, Color color,
        List<Vector3> verts, List<Vector2> uvs, List<Color> cols, List<int> tris)
    {
        int b = verts.Count;
        verts.Add(v0); verts.Add(v1); verts.Add(v2); verts.Add(v3);
        uvs.Add(new Vector2(uvMin.x, uvMin.y)); uvs.Add(new Vector2(uvMax.x, uvMin.y));
        uvs.Add(new Vector2(uvMin.x, uvMax.y)); uvs.Add(new Vector2(uvMax.x, uvMax.y));
        cols.Add(color); cols.Add(color); cols.Add(color); cols.Add(color);
        tris.Add(b); tris.Add(b + 2); tris.Add(b + 1); tris.Add(b + 1); tris.Add(b + 2); tris.Add(b + 3);
        tris.Add(b); tris.Add(b + 1); tris.Add(b + 2); tris.Add(b + 1); tris.Add(b + 3); tris.Add(b + 2);
    }

    void ApplyMaterial()
    {
        if (wheatMaterial != null) { meshRenderer.sharedMaterial = wheatMaterial; return; }
        Shader fb = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default");
        if (fb != null) meshRenderer.sharedMaterial = new Material(fb) { mainTexture = wheatSprite.texture };
        else Debug.LogWarning("[FieldGenerator] Aucun shader fallback. Assignez un matériau.");
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        countX = Mathf.Max(1, countX); countZ = Mathf.Max(1, countZ);
        spacing = Mathf.Max(0.01f, spacing); radius = Mathf.Max(0.01f, radius);
        circleCount = Mathf.Max(1, circleCount); ringRows = Mathf.Max(1, ringRows);
        wheatSize.x = Mathf.Max(0.01f, wheatSize.x); wheatSize.y = Mathf.Max(0.01f, wheatSize.y);
    }

    void OnDrawGizmosSelected()
    {
        if (shape == FieldShape.Grid)
        {
            Gizmos.color = new Color(0.2f, 0.8f, 0.2f, 0.4f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3((countX - 1) * spacing + spacing, 0.1f, (countZ - 1) * spacing + spacing));
            Gizmos.matrix = Matrix4x4.identity;
        }
        else
        {
            Gizmos.color = new Color(0.2f, 0.8f, 0.2f, 0.5f);
            DrawCircleWorld(radius);
            if (shape == FieldShape.Ring)
            {
                Gizmos.color = new Color(0.8f, 0.4f, 0.1f, 0.4f);
                DrawCircleWorld(Mathf.Max(0f, radius - ringRows * Mathf.Max(0.01f, spacing)));
            }
        }
    }

    void DrawCircleWorld(float r)
    {
        const int seg = 64;
        float step = 2f * Mathf.PI / seg;
        Vector3 prev = transform.TransformPoint(new Vector3(r, 0, 0));
        for (int i = 1; i <= seg; i++)
        {
            float a = i * step;
            Vector3 next = transform.TransformPoint(new Vector3(Mathf.Cos(a) * r, 0, Mathf.Sin(a) * r));
            Gizmos.DrawLine(prev, next); prev = next;
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
        var gen = (FieldGenerator)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Régénérer")) { gen.Generate(); EditorUtility.SetDirty(gen); }
        if (GUILayout.Button("Effacer")) { gen.Clear(); EditorUtility.SetDirty(gen); }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        if (gen.GetComponents<FieldGenerator>().Length > 1)
            EditorGUILayout.HelpBox("⚠ Plusieurs FieldGenerator sur ce GameObject.\nChaque forme doit être sur un GameObject séparé.", MessageType.Error);
        else
        {
            Transform child = gen.transform.Find("__WheatMesh__");
            MeshFilter mf = child != null ? child.GetComponent<MeshFilter>() : null;
            int v = mf?.sharedMesh != null ? mf.sharedMesh.vertexCount : 0;
            int t = mf?.sharedMesh != null ? mf.sharedMesh.triangles.Length / 3 : 0;
            string info = gen.shape == FieldGenerator.FieldShape.Ring
                ? $"Rayon int. calculé : {Mathf.Max(0f, gen.radius - gen.ringRows * Mathf.Max(0.01f, gen.spacing)):F2}\n"
                : "";
            EditorGUILayout.HelpBox($"Draw calls : 1  |  Blés : {v / 8}  |  Sommets : {v}  |  Tris : {t}\n{info}NavMesh : ignoré (enfant non-statique)", MessageType.Info);
        }
    }
}
#endif
