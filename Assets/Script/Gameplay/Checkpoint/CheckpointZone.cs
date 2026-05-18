using UnityEngine;

/// <summary>
/// Zone de checkpoint identifiée par un index entier unique.
/// Lorsque le joueur entre dans le collider trigger, l'index de ce checkpoint
/// est envoyé au CheckpointManager. Le spawn se fait à la position du respawnPoint
/// (ou du Transform de ce GameObject si non assigné).
/// Les index doivent être uniques et croissants dans le niveau (0, 1, 2, …).
/// </summary>
[RequireComponent(typeof(Collider))]
public class CheckpointZone : MonoBehaviour
{
    private const string PlayerTag = "Player";

    [Tooltip("Index unique de ce checkpoint dans le niveau. Doit être croissant (0, 1, 2, …).")]
    [SerializeField] private int checkpointIndex = 0;

    [Tooltip("Point de spawn exact. Si non assigné, utilise la position de ce GameObject.")]
    [SerializeField] private Transform respawnPoint;

    private bool hasActivated = false;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
        CheckpointManager.Instance?.RegisterCheckpoint(checkpointIndex, this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasActivated) return;
        if (!other.CompareTag(PlayerTag)) return;

        hasActivated = true;
        CheckpointManager.Instance?.ActivateCheckpoint(checkpointIndex);

        Debug.Log($"[CheckpointZone] '{name}' activé → index {checkpointIndex}, spawn : {GetSpawnPosition()}");
    }

    /// <summary>Retourne la position de spawn de ce checkpoint.</summary>
    public Vector3 GetSpawnPosition()
        => respawnPoint != null ? respawnPoint.position : transform.position;

    /// <summary>Index de ce checkpoint.</summary>
    public int Index => checkpointIndex;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 1f, 0.4f, 0.25f);
        Collider col = GetComponent<Collider>();
        if (col is BoxCollider box)
        {
            Matrix4x4 prev = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
            Gizmos.DrawCube(box.center, box.size);
            Gizmos.color = new Color(0f, 1f, 0.4f, 0.8f);
            Gizmos.DrawWireCube(box.center, box.size);
            Gizmos.matrix = prev;
        }
        else
        {
            Gizmos.DrawSphere(transform.position, 1f);
        }

        // Affiche la position de spawn et l'index
        Vector3 spawnPos = respawnPoint != null ? respawnPoint.position : transform.position;
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(spawnPos, 0.3f);

        UnityEditor.Handles.Label(
            transform.position + Vector3.up * 1.5f,
            $"CP [{checkpointIndex}]");
    }
#endif
}
