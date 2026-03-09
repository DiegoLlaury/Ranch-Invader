using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Automatically configures a NavMeshObstacle that matches the ImpostorEntity's BoxCollider.
/// Attach this on any static impostor object so NavMesh agents path around it at runtime.
/// </summary>
[RequireComponent(typeof(NavMeshObstacle))]
[RequireComponent(typeof(BoxCollider))]
public class ImpostorNavMeshObstacle : MonoBehaviour
{
    [Header("Carving")]
    [Tooltip("Carves a hole in the NavMesh so agents plan paths around this obstacle.")]
    public bool carving = true;

    [Tooltip("Only update the carved hole when the obstacle is stationary. Recommended for static impostors.")]
    public bool carveOnlyStationary = true;

    [Header("Size Override")]
    [Tooltip("Manual obstacle size. If zero, auto-synced from the BoxCollider.")]
    public Vector3 manualSize = Vector3.zero;

    [Tooltip("Manual obstacle center offset.")]
    public Vector3 manualCenter = Vector3.zero;

    private NavMeshObstacle navObstacle;
    private BoxCollider boxCollider;

    private void Awake()
    {
        navObstacle = GetComponent<NavMeshObstacle>();
        boxCollider = GetComponent<BoxCollider>();

        ConfigureObstacle();
    }

    private void ConfigureObstacle()
    {
        navObstacle.shape = NavMeshObstacleShape.Box;
        navObstacle.carving = carving;
        navObstacle.carveOnlyStationary = carveOnlyStationary;

        Vector3 size = manualSize != Vector3.zero ? manualSize : boxCollider.size;
        Vector3 center = manualSize != Vector3.zero ? manualCenter : boxCollider.center;

        navObstacle.size = size;
        navObstacle.center = center;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (navObstacle == null) navObstacle = GetComponent<NavMeshObstacle>();
        if (boxCollider == null) boxCollider = GetComponent<BoxCollider>();

        if (navObstacle != null && boxCollider != null)
        {
            ConfigureObstacle();
        }
    }

    private void OnDrawGizmosSelected()
    {
        BoxCollider col = GetComponent<BoxCollider>();
        if (col == null) return;

        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.4f);
        Matrix4x4 matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.matrix = matrix;
        Gizmos.DrawCube(col.center, col.size);

        Gizmos.color = new Color(0.2f, 0.8f, 1f, 1f);
        Gizmos.DrawWireCube(col.center, col.size);
    }
#endif
}
