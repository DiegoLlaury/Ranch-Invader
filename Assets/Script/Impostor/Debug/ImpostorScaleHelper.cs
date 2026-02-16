#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ImpostorQuadScaler))]
public class ImpostorScaleHelperEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ImpostorQuadScaler scaler = (ImpostorQuadScaler)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Scale Setup Helper", EditorStyles.boldLabel);

        // Tailles prédéfinies
        EditorGUILayout.LabelField("Tailles rapides :");

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Petit (3x3)"))
        {
            scaler.manualSize = new Vector2(3, 3);
            scaler.UpdateScale();
            EditorUtility.SetDirty(scaler);
        }
        if (GUILayout.Button("Moyen (5x5)"))
        {
            scaler.manualSize = new Vector2(5, 5);
            scaler.UpdateScale();
            EditorUtility.SetDirty(scaler);
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Grand (8x8)"))
        {
            scaler.manualSize = new Vector2(8, 8);
            scaler.UpdateScale();
            EditorUtility.SetDirty(scaler);
        }
        if (GUILayout.Button("Très Grand (12x12)"))
        {
            scaler.manualSize = new Vector2(12, 12);
            scaler.UpdateScale();
            EditorUtility.SetDirty(scaler);
        }
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Auto (depuis mesh)"))
        {
            scaler.manualSize = Vector2.zero;
            scaler.UpdateScale();
            EditorUtility.SetDirty(scaler);
        }

        EditorGUILayout.Space();

        // Afficher les infos
        if (scaler.sourceRenderer != null)
        {
            Bounds bounds = scaler.CalculateBounds(scaler.sourceRenderer.gameObject);
            EditorGUILayout.HelpBox(
                $"Taille du mesh source:\n" +
                $"X: {bounds.size.x:F2}, Y: {bounds.size.y:F2}, Z: {bounds.size.z:F2}",
                MessageType.Info
            );
        }
        else
        {
            EditorGUILayout.HelpBox("Source Renderer non assigné - sera configuré au runtime", MessageType.Warning);
        }

        EditorGUILayout.LabelField($"Taille actuelle du quad: {scaler.transform.localScale.x:F2} x {scaler.transform.localScale.y:F2}");
    }
}
#endif
