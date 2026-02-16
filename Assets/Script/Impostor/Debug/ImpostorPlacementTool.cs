#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class ImpostorPlacementTool : EditorWindow
{
    private GameObject impostorQuadPrefab;
    private GameObject meshPrefab;
    private Material impostorMaterial;
    private bool isAnimated = false;
    private bool snapToGround = true;
    private float groundOffset = 0f;

    [MenuItem("Tools/Impostor Placement Tool")]
    public static void ShowWindow()
    {
        GetWindow<ImpostorPlacementTool>("Impostor Placer");
    }

    void OnGUI()
    {
        GUILayout.Label("Impostor Placement Tool", EditorStyles.boldLabel);

        impostorQuadPrefab = (GameObject)EditorGUILayout.ObjectField("Impostor Quad Prefab", impostorQuadPrefab, typeof(GameObject), false);
        meshPrefab = (GameObject)EditorGUILayout.ObjectField("Mesh Prefab", meshPrefab, typeof(GameObject), false);
        impostorMaterial = (Material)EditorGUILayout.ObjectField("Impostor Material", impostorMaterial, typeof(Material), false);

        EditorGUILayout.Space();
        isAnimated = EditorGUILayout.Toggle("Is Animated", isAnimated);
        snapToGround = EditorGUILayout.Toggle("Snap To Ground", snapToGround);
        groundOffset = EditorGUILayout.FloatField("Ground Offset", groundOffset);

        EditorGUILayout.Space();

        if (GUILayout.Button("Place at Scene View Center"))
        {
            PlaceImpostor(GetSceneViewCenter());
        }

        EditorGUILayout.HelpBox("Shift + Click dans la Scene View pour placer un Impostor", MessageType.Info);
    }

    void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    void OnSceneGUI(SceneView sceneView)
    {
        Event e = Event.current;

        if (e.type == EventType.MouseDown && e.button == 0 && e.shift)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                PlaceImpostor(hit.point);
                e.Use();
            }
        }
    }

    void PlaceImpostor(Vector3 position)
    {
        if (impostorQuadPrefab == null || meshPrefab == null)
        {
            EditorUtility.DisplayDialog("Erreur", "Veuillez assigner les prefabs requis !", "OK");
            return;
        }

        if (snapToGround)
        {
            RaycastHit hit;
            if (Physics.Raycast(position + Vector3.up * 100f, Vector3.down, out hit, 200f))
            {
                position = hit.point;
                position.y += groundOffset;
            }
        }

        GameObject quad = (GameObject)PrefabUtility.InstantiatePrefab(impostorQuadPrefab);
        quad.transform.position = position;
        quad.name = $"Impostor_{meshPrefab.name}";

        ImpostorEntity entity = quad.GetComponent<ImpostorEntity>();
        if (entity == null)
        {
            entity = quad.AddComponent<ImpostorEntity>();
        }

        entity.meshPrefab = meshPrefab;
        entity.impostorMaterial = impostorMaterial;
        entity.isAnimated = isAnimated;
        entity.snapToGround = false;
        entity.autoGenerateCollider = true;
        entity.groundOffset = groundOffset;

        Undo.RegisterCreatedObjectUndo(quad, "Place Impostor");
        Selection.activeGameObject = quad;
    }

    Vector3 GetSceneViewCenter()
    {
        SceneView sceneView = SceneView.lastActiveSceneView;
        if (sceneView != null)
        {
            return sceneView.camera.transform.position + sceneView.camera.transform.forward * 10f;
        }
        return Vector3.zero;
    }
}
#endif
