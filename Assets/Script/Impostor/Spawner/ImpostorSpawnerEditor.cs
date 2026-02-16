#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ImpostorSpawner))]
public class ImpostorSpawnerEditor : Editor
{
    private bool showCaptureSettings = true;
    private bool showQuadSettings = true;
    private bool showCollisionSettings = true;
    private bool showGroundSettings = true;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ImpostorSpawner spawner = (ImpostorSpawner)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);

        EditorGUI.BeginDisabledGroup(Application.isPlaying);

        if (GUILayout.Button("Spawn One Preview (Edit Mode)"))
        {
            spawner.SpawnOnePreview();
        }

        EditorGUI.EndDisabledGroup();

        if (GUILayout.Button("Clear All Spawned Impostors"))
        {
            if (EditorUtility.DisplayDialog("Confirmer",
                $"Supprimer tous les impostors de type '{spawner.meshPrefab?.name}' ?",
                "Oui", "Non"))
            {
                spawner.ClearAllSpawned();
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "Configuration Template :\n" +
            "• Tous les impostors spawnés utiliseront ces paramètres\n" +
            "• Modifiez ici pour affecter tous les futurs spawns",
            MessageType.Info
        );

        // Afficher un résumé
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Configuration Summary", EditorStyles.boldLabel);

        EditorGUILayout.LabelField($"Mesh: {spawner.meshPrefab?.name ?? "None"}");
        EditorGUILayout.LabelField($"Count: {spawner.count}");
        EditorGUILayout.LabelField($"Animated: {(spawner.isAnimated ? "Yes" : "No")}");
        EditorGUILayout.LabelField($"Capture Scale: {spawner.captureScale:F2}");
        EditorGUILayout.LabelField($"Mesh Rotation: {spawner.meshRotationOffset}");
        EditorGUILayout.LabelField($"Quad Scale: {spawner.quadScaleMultiplier:F2}");

        if (spawner.colliderSize != Vector3.zero)
        {
            EditorGUILayout.LabelField($"Collider: Manual ({spawner.colliderSize.x:F1}, {spawner.colliderSize.y:F1}, {spawner.colliderSize.z:F1})");
        }
        else
        {
            EditorGUILayout.LabelField("Collider: Auto");
        }
    }
}
#endif
