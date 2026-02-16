using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ImpostorDebugger : MonoBehaviour
{
    public ImpostorEntity targetEntity;
    public bool showDebugGUI = true;
    public bool showDirectionGizmos = true;
    public float gizmoSize = 2f;

    private string[] directionNames = { "North", "NorthEast", "East", "SouthEast", "South", "SouthWest", "West", "NorthWest" };
    private int currentDebugIndex = 0;

    void OnGUI()
    {
        if (!showDebugGUI || targetEntity == null) return;

        GUILayout.BeginArea(new Rect(10, 10, 300, 400));
        GUILayout.Box("Impostor Debugger", GUILayout.Width(290));

        // Afficher la direction actuelle
        var field = typeof(ImpostorEntity).GetField("renderTextures", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        RenderTexture[] textures = field?.GetValue(targetEntity) as RenderTexture[];

        if (textures != null && textures.Length == 8)
        {
            var playerTransformField = typeof(ImpostorEntity).GetField("playerTransform", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            Transform player = playerTransformField?.GetValue(targetEntity) as Transform;

            if (player != null)
            {
                int currentIndex = ImpostorDirectionHelper.GetDirectionIndex(targetEntity.transform.position, player.position);

                GUILayout.Label($"Direction actuelle: {directionNames[currentIndex]} ({currentIndex})");
                GUILayout.Label($"Texture affichée: {textures[currentIndex].name}");

                Vector3 dir = targetEntity.transform.position - player.position;
                dir.y = 0;
                float angle = Vector3.SignedAngle(Vector3.forward, dir, Vector3.up);
                if (angle < 0) angle += 360;
                GUILayout.Label($"Angle: {angle:F1}°");
            }

            GUILayout.Space(10);
            GUILayout.Label("Test manuel des textures:");

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("<", GUILayout.Width(40)))
            {
                currentDebugIndex = (currentDebugIndex - 1 + 8) % 8;
            }
            GUILayout.Label($"{directionNames[currentDebugIndex]} ({currentDebugIndex})", GUILayout.Width(150));
            if (GUILayout.Button(">", GUILayout.Width(40)))
            {
                currentDebugIndex = (currentDebugIndex + 1) % 8;
            }
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Afficher cette texture"))
            {
                var material = targetEntity.GetComponent<MeshRenderer>().material;
                material.SetTexture("_MainTex", textures[currentDebugIndex]);
            }

            GUILayout.Space(10);
            GUILayout.Label("Liste des textures:");
            for (int i = 0; i < 8; i++)
            {
                GUILayout.Label($"{i}: {directionNames[i]} = {textures[i].name}");
            }
        }

        GUILayout.EndArea();
    }

    void OnDrawGizmos()
    {
        if (!showDirectionGizmos || targetEntity == null) return;

        Vector3 center = targetEntity.transform.position;
        center.y += 1f;

        // Dessiner les 8 directions
        Vector3[] directions = new Vector3[]
        {
            new Vector3( 0, 0,  1),           // North
            new Vector3( 1, 0,  1).normalized, // NorthEast
            new Vector3( 1, 0,  0),           // East
            new Vector3( 1, 0, -1).normalized, // SouthEast
            new Vector3( 0, 0, -1),           // South
            new Vector3(-1, 0, -1).normalized, // SouthWest
            new Vector3(-1, 0,  0),           // West
            new Vector3(-1, 0,  1).normalized  // NorthWest
        };

        Color[] colors = new Color[]
        {
            Color.red,      // North
            Color.magenta,  // NorthEast
            Color.blue,     // East
            Color.cyan,     // SouthEast
            Color.green,    // South
            Color.yellow,   // SouthWest
            Color.white,    // West
            new Color(1f, 0.5f, 0f) // NorthWest (orange)
        };

        for (int i = 0; i < 8; i++)
        {
            Gizmos.color = colors[i];
            Vector3 endPoint = center + directions[i] * gizmoSize;
            Gizmos.DrawLine(center, endPoint);
            Gizmos.DrawSphere(endPoint, 0.2f);

#if UNITY_EDITOR
            Handles.Label(endPoint + Vector3.up * 0.5f, $"{i}: {directionNames[i]}");
#endif
        }

        // Dessiner la position du joueur
        var playerTransformField = typeof(ImpostorEntity).GetField("playerTransform", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        Transform player = playerTransformField?.GetValue(targetEntity) as Transform;

        if (player != null)
        {
            Gizmos.color = Color.black;
            Gizmos.DrawLine(center, player.position);

            int currentIndex = ImpostorDirectionHelper.GetDirectionIndex(targetEntity.transform.position, player.position);
            Gizmos.color = colors[currentIndex];
            Gizmos.DrawWireSphere(player.position, 0.5f);
        }
    }
}
