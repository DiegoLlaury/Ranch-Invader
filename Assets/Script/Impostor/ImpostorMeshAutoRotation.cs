using UnityEngine;

public class ImpostorMeshAutoRotation : MonoBehaviour
{
    [Header("Auto-Detection")]
    [Tooltip("Active la détection automatique de l'orientation du mesh")]
    public bool autoDetectOrientation = true;

    [Tooltip("Délai avant de commencer la détection (attendre que l'AI bouge)")]
    public float detectionDelay = 2f;

    [Tooltip("Nombre de samples nécessaires pour confirmer l'orientation")]
    public int samplesRequired = 3;

    [Header("Debug")]
    public bool showDebug = true;

    private ImpostorEntityAI entityAI;
    private RandomMovementAI movementAI;
    private ImpostorEntity impostorEntity;

    private bool detectionComplete = false;
    private int samplesCollected = 0;
    private Vector3 bestRotationOffset = Vector3.zero;
    private float bestAlignment = -1f;

    // Les 4 orientations possibles du mesh
    private Vector3[] possibleOffsets = new Vector3[]
    {
        new Vector3(0, 0, 0),      // Forward = Forward
        new Vector3(0, 90, 0),     // Forward = Right
        new Vector3(0, 180, 0),    // Forward = Back
        new Vector3(0, 270, 0)     // Forward = Left
    };

    void Start()
    {
        entityAI = GetComponent<ImpostorEntityAI>();
        movementAI = GetComponent<RandomMovementAI>();

        if (autoDetectOrientation)
        {
            Invoke(nameof(FindImpostorEntity), 0.5f);
        }
    }

    void FindImpostorEntity()
    {
        impostorEntity = GetComponentInChildren<ImpostorEntity>();

        if (impostorEntity != null)
        {
            Debug.Log($" Auto-rotation activée pour {gameObject.name}");
            Invoke(nameof(StartDetection), detectionDelay);
        }
    }

    void StartDetection()
    {
        if (!autoDetectOrientation || detectionComplete) return;

        InvokeRepeating(nameof(TryDetectOrientation), 0f, 0.5f);
    }

    void TryDetectOrientation()
    {
        if (detectionComplete) return;
        if (movementAI == null || !movementAI.IsMoving) return;

        // Direction de déplacement de l'AI
        Vector3 movementDirection = movementAI.FacingDirection.normalized;

        // Tester chaque orientation possible
        float bestDot = -1f;
        Vector3 bestOffset = Vector3.zero;

        foreach (Vector3 offset in possibleOffsets)
        {
            // Calculer le forward du mesh avec cette rotation
            Quaternion meshRotation = transform.rotation * Quaternion.Euler(offset);
            Vector3 meshForward = meshRotation * Vector3.forward;
            meshForward.y = 0; // Ignore Y
            meshForward.Normalize();

            // Produit scalaire pour mesurer l'alignement
            float dot = Vector3.Dot(meshForward, movementDirection);

            if (showDebug)
            {
                Debug.Log($"Offset {offset.y}° dot = {dot:F3} (meshForward={meshForward}, moveDir={movementDirection})");
            }

            if (dot > bestDot)
            {
                bestDot = dot;
                bestOffset = offset;
            }
        }

        // Si on a un bon alignement (dot > 0.8), on compte ce sample
        if (bestDot > 0.8f)
        {
            samplesCollected++;

            if (bestDot > bestAlignment)
            {
                bestAlignment = bestDot;
                bestRotationOffset = bestOffset;
            }

            Debug.Log($" Sample {samplesCollected}/{samplesRequired} - Best offset: {bestRotationOffset.y}° (alignment: {bestAlignment:F3})");

            if (samplesCollected >= samplesRequired)
            {
                ApplyDetectedRotation();
            }
        }
    }

    void ApplyDetectedRotation()
    {
        detectionComplete = true;
        CancelInvoke(nameof(TryDetectOrientation));

        Debug.Log($" DÉTECTION TERMINÉE ! Meilleure rotation : {bestRotationOffset.y}°");
        Debug.Log($"   Alignement : {bestAlignment:F3}");

        // Appliquer la rotation détectée
        if (entityAI != null)
        {
            entityAI.meshRotationOffset = bestRotationOffset;
        }

        if (impostorEntity != null)
        {
            impostorEntity.meshRotationOffset = bestRotationOffset;

            // Forcer une recapture
            var captureMethod = impostorEntity.GetType().GetMethod(
                "CaptureImpostor",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
            );

            captureMethod?.Invoke(impostorEntity, null);
        }

        Debug.Log($" Configuration finale : Mesh Rotation Offset = (0, {bestRotationOffset.y}, 0)");

        // Détruire ce script après détection
        Invoke(nameof(CleanupScript), 1f);
    }

    void CleanupScript()
    {
        Debug.Log($" Auto-détection terminée, script nettoyé.");
        Destroy(this);
    }

    void OnGUI()
    {
        if (!showDebug || !autoDetectOrientation) return;

        GUILayout.BeginArea(new Rect(10, 300, 350, 150));
        GUI.Box(new Rect(0, 0, 350, 150), "");

        GUILayout.Space(5);
        GUILayout.Label("=== AUTO-ROTATION ===");

        if (!detectionComplete)
        {
            GUILayout.Label($" Détection en cours...");
            GUILayout.Label($"Samples: {samplesCollected}/{samplesRequired}");

            if (movementAI != null)
            {
                GUILayout.Label($"En mouvement: {(movementAI.IsMoving ? "OUI" : "NON - en attente...")}");
            }

            if (bestAlignment > 0)
            {
                GUILayout.Label($"Meilleur angle: {bestRotationOffset.y}°");
                GUILayout.Label($"Alignement: {bestAlignment:F2}");
            }
        }
        else
        {
            GUILayout.Label($" DÉTECTION TERMINÉE !");
            GUILayout.Label($"Rotation trouvée: {bestRotationOffset.y}°");
            GUILayout.Label($"");
            GUILayout.Label($"Copiez cette valeur dans");
            GUILayout.Label($"le prefab si nécessaire.");
        }

        GUILayout.EndArea();
    }

    void OnDrawGizmos()
    {
        if (!showDebug || !Application.isPlaying || movementAI == null) return;
        if (!movementAI.IsMoving) return;

        Vector3 basePos = transform.position + Vector3.up * 2.5f;

        // ROUGE : Direction de déplacement (référence)
        Vector3 moveDir = movementAI.FacingDirection.normalized * 2f;
        Gizmos.color = Color.red;
        Gizmos.DrawRay(basePos, moveDir);
        Gizmos.DrawSphere(basePos + moveDir, 0.15f);

        // JAUNE : Forward de l'entité
        Gizmos.color = Color.yellow;
        Vector3 entityForward = transform.forward * 1.5f;
        Gizmos.DrawRay(basePos, entityForward);

        // VERT : Forward du mesh avec offset détecté
        if (bestRotationOffset != Vector3.zero || detectionComplete)
        {
            Quaternion meshRot = transform.rotation * Quaternion.Euler(bestRotationOffset);
            Vector3 meshForward = meshRot * Vector3.forward * 1.8f;

            Gizmos.color = detectionComplete ? Color.green : Color.cyan;
            Gizmos.DrawRay(basePos, meshForward);
            Gizmos.DrawSphere(basePos + meshForward, 0.2f);
        }
    }
}
