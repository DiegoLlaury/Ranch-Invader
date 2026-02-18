using UnityEngine;

public class ImpostorDirectionDiagnostic : MonoBehaviour
{
    [Header("References")]
    public Transform player;

    [Header("Debug")]
    public bool showDebug = true;
    public bool showGizmos = true;

    private ImpostorEntity impostorEntity;
    private RandomMovementAI movementAI;

    void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }

        movementAI = GetComponent<RandomMovementAI>();
        Invoke(nameof(FindImpostor), 0.5f);
    }

    void FindImpostor()
    {
        impostorEntity = GetComponentInChildren<ImpostorEntity>();
    }

    void OnGUI()
    {
        if (!showDebug || player == null) return;

        // ✅ FENÊTRE EN HAUT À GAUCHE
        GUILayout.BeginArea(new Rect(10, 10, 520, 400));

        // Background
        GUI.Box(new Rect(0, 0, 520, 400), "");

        GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontSize = 14;
        titleStyle.fontStyle = FontStyle.Bold;

        GUIStyle warningStyle = new GUIStyle(GUI.skin.label);
        warningStyle.normal.textColor = Color.red;
        warningStyle.fontStyle = FontStyle.Bold;

        GUIStyle successStyle = new GUIStyle(GUI.skin.label);
        successStyle.normal.textColor = Color.green;
        successStyle.fontStyle = FontStyle.Bold;

        GUILayout.Space(5);
        GUILayout.Label("=== DIAGNOSTIC DIRECTION ===", titleStyle);
        GUILayout.Space(10);

        // Direction impostor → player
        Vector3 impostorToPlayer = player.position - transform.position;
        impostorToPlayer.y = 0;
        impostorToPlayer.Normalize();

        float angleToPlayer = Mathf.Atan2(impostorToPlayer.x, impostorToPlayer.z) * Mathf.Rad2Deg;
        if (angleToPlayer < 0) angleToPlayer += 360;

        int dirIndexOld = ImpostorDirectionHelper.GetDirectionIndex(transform.position, player.position);

        GUILayout.Label("┌─ CALCUL ACTUEL (Impostor → Player) ─┐");
        GUILayout.Label($"│ Angle vers joueur: {angleToPlayer:F1}°");
        GUILayout.Label($"│ Texture index utilisé: {dirIndexOld}");
        GUILayout.Label($"│ Direction: {GetDirectionName(dirIndexOld)}");
        GUILayout.Label("└────────────────────────────────────┘");

        GUILayout.Space(10);

        // Direction de déplacement de l'AI
        GUILayout.Label("┌─ INFO ENTITÉ AI ─────────────────┐");
        GUILayout.Label($"│ Entity Rotation: {transform.rotation.eulerAngles.y:F1}°");

        if (movementAI != null)
        {
            GUILayout.Label($"│ Is Moving: {(movementAI.IsMoving ? "OUI" : "NON")}");

            if (movementAI.IsMoving)
            {
                Vector3 moveDir = movementAI.FacingDirection;
                float moveAngle = Mathf.Atan2(moveDir.x, moveDir.z) * Mathf.Rad2Deg;
                if (moveAngle < 0) moveAngle += 360;

                GUILayout.Label($"│ Movement angle: {moveAngle:F1}°");
            }
        }
        GUILayout.Label("└────────────────────────────────────┘");

        GUILayout.Space(10);

        // Ce qu'on DEVRAIT voir
        float meshFacingAngle = transform.rotation.eulerAngles.y;
        int expectedTextureIndex = GetTextureIndexForMeshFacing(meshFacingAngle);

        GUILayout.Label("┌─ TEXTURE ATTENDUE (Mesh Facing) ──┐");
        GUILayout.Label($"│ Mesh facing angle: {meshFacingAngle:F1}°");
        GUILayout.Label($"│ Texture index attendu: {expectedTextureIndex}");
        GUILayout.Label($"│ Direction: {GetDirectionName(expectedTextureIndex)}");
        GUILayout.Label("└────────────────────────────────────┘");

        GUILayout.Space(10);

        // Diagnostic
        GUILayout.Label("┌─ DIAGNOSTIC ──────────────────────┐");
        if (dirIndexOld != expectedTextureIndex)
        {
            GUILayout.Label("│ ⚠️ PROBLÈME DÉTECTÉ !", warningStyle);
            GUILayout.Label($"│ Utilisé: {dirIndexOld} ({GetDirectionName(dirIndexOld)})");
            GUILayout.Label($"│ Attendu: {expectedTextureIndex} ({GetDirectionName(expectedTextureIndex)})");
            GUILayout.Label($"│ Décalage: {Mathf.Abs(dirIndexOld - expectedTextureIndex)} index");
        }
        else
        {
            GUILayout.Label("│ ✅ MAPPING CORRECT", successStyle);
        }
        GUILayout.Label("└────────────────────────────────────┘");

        GUILayout.Space(10);
        GUILayout.Label("GIZMOS (Scene view):");
        GUILayout.Label("• CYAN = Direction vers joueur");
        GUILayout.Label("• MAGENTA = Forward du mesh");
        GUILayout.Label("• ROUGE = Direction déplacement");

        GUILayout.EndArea();
    }

    int GetTextureIndexForMeshFacing(float meshAngle)
    {
        // Le mesh regarde dans une direction (0-360°)
        // On doit trouver quelle texture correspond à cette orientation

        // Normaliser
        while (meshAngle < 0) meshAngle += 360;
        while (meshAngle >= 360) meshAngle -= 360;

        // Convertir en index de texture (0-7)
        // 0° = North, 45° = NorthEast, 90° = East, etc.
        int index = Mathf.RoundToInt(meshAngle / 45f) % 8;
        return index;
    }

    string GetDirectionName(int index)
    {
        string[] names = { "North(0°)", "NE(45°)", "East(90°)", "SE(135°)", "South(180°)", "SW(225°)", "West(270°)", "NW(315°)" };
        return names[index % 8];
    }

    void OnDrawGizmos()
    {
        if (!showGizmos || !Application.isPlaying || player == null) return;

        Vector3 basePos = transform.position + Vector3.up * 3f;

        // CYAN : Direction impostor → player (utilisée actuellement)
        Vector3 toPlayer = (player.position - transform.position);
        toPlayer.y = 0;
        toPlayer.Normalize();

        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(basePos, toPlayer * 2.5f);
        Gizmos.DrawWireSphere(basePos + toPlayer * 2.5f, 0.2f);

        // MAGENTA : Forward du mesh (ce que le mesh regarde)
        Gizmos.color = Color.magenta;
        Vector3 meshForward = transform.forward * 2f;
        Gizmos.DrawRay(basePos, meshForward);
        Gizmos.DrawWireSphere(basePos + meshForward, 0.25f);

        // ROUGE : Direction de déplacement
        if (movementAI != null && movementAI.IsMoving)
        {
            Gizmos.color = Color.red;
            Vector3 moveDir = movementAI.FacingDirection * 1.5f;
            Gizmos.DrawRay(basePos, moveDir);
            Gizmos.DrawSphere(basePos + moveDir, 0.15f);
        }

        // Dessiner les 8 directions avec leurs index
        for (int i = 0; i < 8; i++)
        {
            float angle = i * 45f;
            Vector3 dir = Quaternion.Euler(0, angle, 0) * Vector3.forward * 3.5f;

            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
            Gizmos.DrawLine(basePos, basePos + dir);
            Gizmos.DrawWireSphere(basePos + dir, 0.15f);
        }
    }
}
