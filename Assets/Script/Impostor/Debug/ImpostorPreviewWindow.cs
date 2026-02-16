#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class ImpostorPreviewWindow : EditorWindow
{
    private GameObject selectedImpostor;
    private ImpostorQuadScaler scaler;
    private float previewScale = 1f;
    private Vector2 previewSize = Vector2.zero;

    [MenuItem("Tools/Impostor Preview")]
    public static void ShowWindow()
    {
        GetWindow<ImpostorPreviewWindow>("Impostor Preview");
    }

    void OnSelectionChange()
    {
        if (Selection.activeGameObject != null)
        {
            ImpostorQuadScaler newScaler = Selection.activeGameObject.GetComponent<ImpostorQuadScaler>();
            if (newScaler != null)
            {
                selectedImpostor = Selection.activeGameObject;
                scaler = newScaler;
                previewScale = scaler.scaleMultiplier;
                previewSize = scaler.manualSize;
                Repaint();
            }
        }
    }

    void OnGUI()
    {
        GUILayout.Label("Impostor Live Preview", EditorStyles.boldLabel);

        if (scaler == null)
        {
            EditorGUILayout.HelpBox("Sélectionnez un GameObject avec ImpostorQuadScaler dans la scène", MessageType.Info);
            return;
        }

        EditorGUILayout.LabelField("Selected", selectedImpostor.name);
        EditorGUILayout.Space();

        EditorGUI.BeginChangeCheck();

        previewScale = EditorGUILayout.Slider("Scale Multiplier", previewScale, 0.1f, 5f);

        EditorGUILayout.LabelField("Manual Size (0 = auto)");
        previewSize.x = EditorGUILayout.FloatField("Width", previewSize.x);
        previewSize.y = EditorGUILayout.FloatField("Height", previewSize.y);

        if (EditorGUI.EndChangeCheck())
        {
            scaler.scaleMultiplier = previewScale;
            scaler.manualSize = previewSize;
            scaler.UpdateScale();
            EditorUtility.SetDirty(scaler.gameObject);
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Apply & Close"))
        {
            Close();
        }

        if (GUILayout.Button("Reset"))
        {
            previewScale = 1f;
            previewSize = Vector2.zero;
            scaler.scaleMultiplier = previewScale;
            scaler.manualSize = previewSize;
            scaler.UpdateScale();
            EditorUtility.SetDirty(scaler.gameObject);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField($"Current Scale: {scaler.transform.localScale.x:F2} x {scaler.transform.localScale.y:F2}");
    }
}
#endif
