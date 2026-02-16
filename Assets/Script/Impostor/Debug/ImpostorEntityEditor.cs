#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ImpostorEntity))]
public class ImpostorEntityEditor : Editor
{
    private bool showDebugInfo = false;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ImpostorEntity entity = (ImpostorEntity)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Preview & Testing", EditorStyles.boldLabel);

        EditorGUI.BeginDisabledGroup(!Application.isPlaying);

        if (GUILayout.Button("Force Capture Now"))
        {
            var method = entity.GetType().GetMethod("CaptureImpostor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method?.Invoke(entity, null);
        }

        if (GUILayout.Button("Update Collider"))
        {
            var method = entity.GetType().GetMethod("UpdateCollider", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method?.Invoke(entity, null);
        }

        EditorGUI.EndDisabledGroup();

        EditorGUILayout.Space();

        if (GUILayout.Button("Align to Ground"))
        {
            var method = entity.GetType().GetMethod("AlignToGround", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method?.Invoke(entity, null);
            EditorUtility.SetDirty(entity.gameObject);
        }

        EditorGUILayout.Space();
        showDebugInfo = EditorGUILayout.Foldout(showDebugInfo, "Debug Info");

        if (showDebugInfo)
        {
            EditorGUI.indentLevel++;

            if (Application.isPlaying)
            {
                EditorGUILayout.LabelField("Status", "Running");
            }
            else
            {
                EditorGUILayout.LabelField("Status", "Editor Mode");
            }

            EditorGUILayout.LabelField("Mesh Prefab", entity.meshPrefab != null ? entity.meshPrefab.name : "None");
            EditorGUILayout.LabelField("Player Transform", entity.playerTransform != null ? "Assigned" : "Not Found");

            ImpostorQuadScaler scaler = entity.GetComponent<ImpostorQuadScaler>();
            if (scaler != null)
            {
                EditorGUILayout.LabelField("Quad Scale", $"{scaler.transform.localScale.x:F2} x {scaler.transform.localScale.y:F2}");
            }

            BoxCollider collider = entity.GetComponent<BoxCollider>();
            if (collider != null)
            {
                EditorGUILayout.LabelField("Collider Size", $"{collider.size.x:F2} x {collider.size.y:F2} x {collider.size.z:F2}");
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
}
#endif
