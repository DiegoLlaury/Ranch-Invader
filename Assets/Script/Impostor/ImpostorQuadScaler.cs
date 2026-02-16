using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class ImpostorQuadScaler : MonoBehaviour
{
    [Header("Scale Settings")]
    [Tooltip("Multiplicateur manuel de la taille du quad")]
    [Range(0.1f, 5f)]
    public float scaleMultiplier = 1f;

    [Tooltip("Taille manuelle du quad (0 = auto depuis mesh)")]
    public Vector2 manualSize = Vector2.zero;

    [Header("Auto Setup")]
    [Tooltip("Source mesh pour calculer la taille automatiquement")]
    public Renderer sourceRenderer;

    [Tooltip("Mettre à jour automatiquement quand les valeurs changent")]
    public bool autoUpdate = true;

    private Vector3 lastScale;
    private float lastMultiplier;
    private Vector2 lastManualSize;

    void Start()
    {
        if (Application.isPlaying && sourceRenderer != null)
        {
            UpdateScale();
        }
    }

    void Update()
    {
        if (!Application.isPlaying && autoUpdate)
        {
            if (HasChanged())
            {
                UpdateScale();
            }
        }
    }

    bool HasChanged()
    {
        return lastScale != transform.localScale
            || lastMultiplier != scaleMultiplier
            || lastManualSize != manualSize;
    }

    public void UpdateScale()
    {
        Vector3 newScale;

        if (manualSize.x > 0 && manualSize.y > 0)
        {
            newScale = new Vector3(manualSize.x * scaleMultiplier, manualSize.y * scaleMultiplier, 1f);
        }
        else if (sourceRenderer != null)
        {
            Bounds b = CalculateBounds(sourceRenderer.gameObject);
            float maxSize = Mathf.Max(b.size.x, b.size.y);
            newScale = new Vector3(maxSize * scaleMultiplier, maxSize * scaleMultiplier, 1f);
        }
        else
        {
            newScale = new Vector3(1f * scaleMultiplier, 1f * scaleMultiplier, 1f);
        }

        transform.localScale = newScale;

        lastScale = newScale;
        lastMultiplier = scaleMultiplier;
        lastManualSize = manualSize;
    }

    public Bounds CalculateBounds(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(true); // Ajout de true pour inclure les désactivés

        if (renderers.Length == 0)
        {
            return new Bounds(obj.transform.position, Vector3.one);
        }

        Bounds bounds = renderers[0].bounds;

        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        return bounds;
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (autoUpdate)
        {
            UpdateScale();
        }
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(ImpostorQuadScaler))]
public class ImpostorQuadScalerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ImpostorQuadScaler scaler = (ImpostorQuadScaler)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);

        if (GUILayout.Button("Update Scale Now"))
        {
            scaler.UpdateScale();
            EditorUtility.SetDirty(scaler);
        }

        if (GUILayout.Button("Reset to Auto Size"))
        {
            scaler.manualSize = Vector2.zero;
            scaler.scaleMultiplier = 1f;
            scaler.UpdateScale();
            EditorUtility.SetDirty(scaler);
        }

        if (GUILayout.Button("Fit to Source Mesh"))
        {
            if (scaler.sourceRenderer != null)
            {
                scaler.manualSize = Vector2.zero;
                scaler.UpdateScale();
                EditorUtility.SetDirty(scaler);
            }
            else
            {
                EditorUtility.DisplayDialog("Erreur", "Aucun Source Renderer assigné !", "OK");
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "Scale Multiplier: Ajuste la taille globalement\n" +
            "Manual Size: Force une taille spécifique (laissez à 0 pour auto)",
            MessageType.Info
        );

        if (scaler.sourceRenderer != null)
        {
            Bounds bounds = scaler.CalculateBounds(scaler.sourceRenderer.gameObject);
            EditorGUILayout.LabelField($"Source Mesh Size: {bounds.size.x:F2} x {bounds.size.y:F2}");
        }

        EditorGUILayout.LabelField($"Current Scale: {scaler.transform.localScale.x:F2} x {scaler.transform.localScale.y:F2}");
    }
}
#endif
