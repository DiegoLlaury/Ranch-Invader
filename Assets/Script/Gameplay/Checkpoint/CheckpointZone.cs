using UnityEngine;

/// <summary>
/// Zone de sauvegarde de position. Lorsque le joueur entre dans le collider trigger,
/// la position de ce GameObject devient la nouvelle position de respawn.
/// Aucun lien avec les GameplayEventSO — cette zone gère uniquement la position.
/// </summary>
[RequireComponent(typeof(Collider))]
public class CheckpointZone : MonoBehaviour
{
    private const string PlayerTag = "Player";

    [Tooltip("Override optionnel : si assigné, c'est cette position qui est enregistrée plutôt que celle du GameObject.")]
    [SerializeField] private Transform respawnPoint;

    private bool hasActivated = false;

    private void Awake()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasActivated) return;
        if (!other.CompareTag(PlayerTag)) return;

        hasActivated = true;

        Vector3 position = respawnPoint != null ? respawnPoint.position : transform.position;
        CheckpointManager.Instance?.RegisterCheckpointPosition(position);

        Debug.Log($"[CheckpointZone] Checkpoint activé : {name} → position {position}");
    }

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

        Vector3 spawnPos = respawnPoint != null ? respawnPoint.position : transform.position;
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(spawnPos, 0.3f);
    }
#endif
}
