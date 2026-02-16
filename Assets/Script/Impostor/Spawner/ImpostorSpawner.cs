using UnityEngine;

public class ImpostorSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject impostorQuadPrefab;
    public GameObject meshPrefab;
    public Material impostorMaterial;

    [Header("Spawn Settings")]
    public int count = 10;
    public float spawnRadius = 20f;
    public Transform playerTransform;

    [Header("Animation Settings")]
    public bool isAnimated = false;
    [Range(1, 60)]
    public int animatedFPS = 15;
    public float staticUpdateInterval = 1f;

    [Header("Capture Settings")]
    [Tooltip("Multiplicateur de taille pour la capture (1 = auto, >1 = zoom out, <1 = zoom in)")]
    [Range(0.5f, 3f)]
    public float captureScale = 1f;

    [Tooltip("Rotation du mesh lors de la capture (utilisez Y=180 si le mesh est inversé)")]
    public Vector3 meshRotationOffset = Vector3.zero;

    [Header("Quad Scale Settings")]
    [Tooltip("Multiplicateur de taille du quad")]
    [Range(0.1f, 5f)]
    public float quadScaleMultiplier = 1f;

    [Tooltip("Taille manuelle du quad (0,0 = auto depuis mesh)")]
    public Vector2 quadManualSize = Vector2.zero;

    [Header("Collision Settings")]
    public bool autoGenerateCollider = true;
    public bool dynamicCollider = false;

    [Tooltip("Taille du collider (0,0,0 = auto)")]
    public Vector3 colliderSize = Vector3.zero;
    public Vector3 colliderCenter = Vector3.zero;

    [Header("Ground Settings")]
    public bool snapToGround = true;
    public float groundOffset = 0f;
    public LayerMask groundLayers = -1;

    [Header("Billboard Settings")]
    public bool lockToYAxis = true;

    [Header("Camera Perspective Settings")]
    [Tooltip("Hauteur de caméra pour tous les spawns (-1 = défaut)")]
    public float customCameraHeight = -1f;

    [Tooltip("Point de regard sur le mesh (0-1, -1 = défaut)")]
    [Range(-1f, 1f)]
    public float customLookAtRatio = -1f;

    [Tooltip("Field of View pour tous les spawns (-1 = défaut)")]
    [Range(-1f, 120f)]
    public float customFieldOfView = -1f;

    [Tooltip("Multiplicateur de distance de la caméra (-1 = défaut)")]
    [Range(-1f, 5f)]
    public float customDistanceMultiplier = -1f;

    void Start()
    {
        if (playerTransform == null)
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        }

        SpawnImpostors();
    }

    public void SpawnImpostors()
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 randomPos = transform.position + Random.insideUnitSphere * spawnRadius;
            randomPos.y = transform.position.y;



            SpawnImpostor(randomPos, Quaternion.identity);
        }
    }

    public GameObject SpawnImpostor(Vector3 position, Quaternion rotation)
    {
        if (snapToGround)
        {
            position = GetGroundPosition(position);
        }

        GameObject quad = Instantiate(impostorQuadPrefab, position, rotation);
        quad.name = $"Impostor_{meshPrefab.name}_{Random.Range(1000, 9999)}";

        // Configuration de ImpostorEntity
        ImpostorEntity entity = quad.GetComponent<ImpostorEntity>();
        if (entity == null)
        {
            entity = quad.AddComponent<ImpostorEntity>();
        }

        // Références
        entity.meshPrefab = meshPrefab;
        entity.impostorMaterial = impostorMaterial;
        entity.playerTransform = playerTransform;
        entity.autoFindPlayer = false; // Désactivé car on assigne manuellement

        // Animation
        entity.isAnimated = isAnimated;
        entity.animatedFPS = animatedFPS;
        entity.staticUpdateInterval = staticUpdateInterval;

        // Capture
        entity.captureScale = captureScale;
        entity.meshRotationOffset = meshRotationOffset;

        // Collision
        entity.autoGenerateCollider = autoGenerateCollider;
        entity.dynamicCollider = dynamicCollider;
        entity.colliderSize = colliderSize;
        entity.colliderCenter = colliderCenter;

        // Ground (désactivé car déjà fait par le spawner)
        entity.snapToGround = false;
        entity.groundOffset = groundOffset;
        entity.groundLayers = groundLayers;

        // Camera Perspective
        entity.customCameraHeight = customCameraHeight;
        entity.customLookAtRatio = customLookAtRatio;
        entity.customFieldOfView = customFieldOfView;
        entity.customDistanceMultiplier = customDistanceMultiplier;

        // Configuration de ImpostorQuadScaler
        ImpostorQuadScaler scaler = quad.GetComponent<ImpostorQuadScaler>();
        if (scaler == null)
        {
            scaler = quad.AddComponent<ImpostorQuadScaler>();
        }

        scaler.scaleMultiplier = quadScaleMultiplier;
        scaler.manualSize = quadManualSize;
        scaler.autoUpdate = false; // Désactivé en runtime pour performance

        // Configuration de Billboard
        Billboard billboard = quad.GetComponent<Billboard>();
        if (billboard == null)
        {
            billboard = quad.AddComponent<Billboard>();
        }

        billboard.lockToYAxis = lockToYAxis;

        return quad;
    }

    Vector3 GetGroundPosition(Vector3 position)
    {
        RaycastHit hit;
        Vector3 rayStart = position + Vector3.up * 100f;

        if (Physics.Raycast(rayStart, Vector3.down, out hit, 200f, groundLayers))
        {
            Vector3 groundPos = hit.point;
            groundPos.y += groundOffset;
            return groundPos;
        }

        return position;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);

        // Afficher une preview du premier impostor
        if (count > 0)
        {
            Gizmos.color = Color.green;
            Vector3 previewPos = transform.position;
            if (snapToGround)
            {
                previewPos = GetGroundPosition(previewPos);
            }
            Gizmos.DrawWireCube(previewPos, Vector3.one * 0.5f);
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Spawn One Preview")]
    public void SpawnOnePreview()
    {
        if (playerTransform == null)
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        }

        SpawnImpostor(transform.position, Quaternion.identity);
    }

    [ContextMenu("Clear All Spawned")]
    public void ClearAllSpawned()
    {
        ImpostorEntity[] entities = FindObjectsOfType<ImpostorEntity>();
        foreach (var entity in entities)
        {
            if (entity.meshPrefab == meshPrefab)
            {
                DestroyImmediate(entity.gameObject);
            }
        }
    }
#endif
}
