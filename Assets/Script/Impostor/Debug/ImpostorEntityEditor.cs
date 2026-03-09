#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// Custom Inspector for ImpostorEntity.
/// Adds a live mesh preview panel using MeshPreview, and convenience action buttons.
/// </summary>
[CustomEditor(typeof(ImpostorEntity))]
public class ImpostorEntityEditor : Editor
{
    private bool showDebugInfo = false;
    private MeshPreview meshPreview;
    private Mesh lastPreviewMesh;

    private void OnEnable()
    {
        RefreshMeshPreview();
    }

    private void OnDisable()
    {
        meshPreview?.Dispose();
        meshPreview = null;
    }

    public override bool HasPreviewGUI() => GetPreviewMesh() != null;

    public override GUIContent GetPreviewTitle() => new GUIContent("Mesh Preview");

    public override void OnPreviewGUI(Rect rect, GUIStyle background)
    {
        Mesh mesh = GetPreviewMesh();
        if (mesh == null)
        {
            EditorGUI.LabelField(rect, "No mesh assigned.", EditorStyles.centeredGreyMiniLabel);
            return;
        }

        // Rebuild preview if mesh changed
        if (mesh != lastPreviewMesh)
        {
            RefreshMeshPreview();
        }

        meshPreview?.OnPreviewGUI(rect, background);
    }

    public override void OnPreviewSettings()
    {
        meshPreview?.OnPreviewSettings();
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ImpostorEntity entity = (ImpostorEntity)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Preview & Testing", EditorStyles.boldLabel);

        // Refresh preview if meshPrefab changed
        Mesh currentMesh = GetPreviewMesh();
        if (currentMesh != lastPreviewMesh)
        {
            RefreshMeshPreview();
            Repaint();
        }

        EditorGUI.BeginDisabledGroup(!Application.isPlaying);

        if (GUILayout.Button("Force Capture Now"))
        {
            var method = entity.GetType().GetMethod(
                "CaptureImpostor",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
            );
            method?.Invoke(entity, null);
        }

        if (GUILayout.Button("Update Collider"))
        {
            var method = entity.GetType().GetMethod(
                "UpdateCollider",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
            );
            method?.Invoke(entity, null);
        }

        EditorGUI.EndDisabledGroup();

        EditorGUILayout.Space();

        if (GUILayout.Button("Align to Ground"))
        {
            var method = entity.GetType().GetMethod(
                "AlignToGround",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
            );
            method?.Invoke(entity, null);
            EditorUtility.SetDirty(entity.gameObject);
        }

        EditorGUILayout.Space();

        showDebugInfo = EditorGUILayout.Foldout(showDebugInfo, "Debug Info");

        if (showDebugInfo)
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.LabelField("Status", Application.isPlaying ? "Running" : "Editor Mode");
            EditorGUILayout.LabelField("Mesh Prefab", entity.meshPrefab != null ? entity.meshPrefab.name : "None");
            EditorGUILayout.LabelField("Player Transform", entity.playerTransform != null ? "Assigned" : "Not Found");

            ImpostorQuadScaler scaler = entity.GetComponent<ImpostorQuadScaler>();
            if (scaler != null)
            {
                EditorGUILayout.LabelField("Quad Scale", $"{scaler.transform.localScale.x:F2} x {scaler.transform.localScale.y:F2}");
            }

            BoxCollider col = entity.GetComponent<BoxCollider>();
            if (col != null)
            {
                EditorGUILayout.LabelField("Collider Size", $"{col.size.x:F2} x {col.size.y:F2} x {col.size.z:F2}");
            }

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "Capture Scale: Ajustez pour les grands/petits objets\n" +
            "• Tracteur: 1.0\n" +
            "• Silo: 1.5-2.0\n" +
            "• Poulet: 0.7-0.8",
            MessageType.Info
        );
    }

    // ?? Helpers ??????????????????????????????????????????????????????????????

    private Mesh GetPreviewMesh()
    {
        ImpostorEntity entity = (ImpostorEntity)target;
        if (entity == null || entity.meshPrefab == null) return null;

        MeshFilter mf = entity.meshPrefab.GetComponentInChildren<MeshFilter>();
        return mf != null ? mf.sharedMesh : null;
    }

    private void RefreshMeshPreview()
    {
        meshPreview?.Dispose();
        meshPreview = null;

        Mesh mesh = GetPreviewMesh();
        if (mesh == null) return;

        lastPreviewMesh = mesh;
        meshPreview = new MeshPreview(mesh);
    }
}
#endif
