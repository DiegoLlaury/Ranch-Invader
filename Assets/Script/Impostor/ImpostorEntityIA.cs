using UnityEngine;

/// <summary>
/// Connects the ImpostorEntity rendering system to any EnemyBase-driven AI on the same GameObject.
/// Replaces the old RandomMovementAI dependency with the NavMesh-based EnemyBase.
/// </summary>
[RequireComponent(typeof(EnemyBase))]
public class ImpostorEntityAI : MonoBehaviour
{
    [Header("Impostor Settings")]
    public GameObject meshPrefab;
    public Material impostorMaterial;
    public GameObject impostorQuadPrefab;

    [Header("Animation Settings")]
    [Range(1, 60)]
    public int animatedFPS = 15;

    [Header("Capture Settings")]
    [Range(0.5f, 3f)]
    public float captureScale = 1f;
    public Vector3 meshRotationOffset = Vector3.zero;

    [Header("Camera Perspective Settings")]
    public float customCameraHeight = -1f;
    [Range(-1f, 1f)]
    public float customLookAtRatio = -1f;
    [Range(-1f, 120f)]
    public float customFieldOfView = -1f;
    [Range(-1f, 5f)]
    public float customDistanceMultiplier = -1f;

    [Header("Quad Scale Settings")]
    [Range(0.1f, 5f)]
    public float quadScaleMultiplier = 1f;
    public Vector2 quadManualSize = Vector2.zero;

    [Header("Collision Settings")]
    public bool autoGenerateCollider = true;
    public Vector3 colliderSize = Vector3.zero;
    public Vector3 colliderCenter = Vector3.zero;

    [Header("Billboard Settings")]
    [Tooltip("Active le billboard (le quad tourne vers le joueur). Désactiver pour un impostor statique orienté manuellement.")]
    public bool useBillboard = true;
    public bool useSmoothRotation = true;
    [Range(0.1f, 50f)]
    public float rotationSpeed = 8f;
    [Range(0f, 45f)]
    public float rotationDeadZone = 8f;

    [Tooltip("Le mesh suit la rotation de l'entit� AI")]
    public bool followParentRotation = true;

    [Header("Parallax Settings")]
    public bool useParallax = true;
    [Range(0f, 0.1f)]
    public float parallaxStrength = 0.03f;
    [Range(4, 32)]
    public int parallaxMinSamples = 8;
    [Range(4, 64)]
    public int parallaxMaxSamples = 32;

    [Header("Position Offset")]
    [Tooltip("Décalage local du quad impostor par rapport au pivot de l'entité AI")]
    public Vector3 positionOffset = Vector3.zero;

    [Header("Static Face Settings")]
    [Tooltip("Verrouille l'impostor sur une face fixe, ignorant la position du joueur")]
    public bool useStaticFace = false;

    [Tooltip("Index de la face fixe (0=avant, 2=gauche, 4=arrière, 6=droite)")]
    [Range(0, 7)]
    public int staticFaceIndex = 0;

    [Header("Ground Alignment")]
    [Tooltip("Aligne automatiquement l'impostor sur le sol au d�marrage.")]
    public bool snapToGround = true;
    public float groundOffset = 0f;
    public LayerMask groundLayers = -1;

    private GameObject impostorQuadInstance;
    private ImpostorEntity impostorEntity;
    private Transform playerTransform;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        SetupImpostor();
    }

    private void SetupImpostor()
    {
        impostorQuadInstance = Instantiate(impostorQuadPrefab, transform);
        impostorQuadInstance.name = "ImpostorQuad";
        impostorQuadInstance.transform.localPosition = Vector3.zero;
        impostorQuadInstance.transform.localRotation = Quaternion.identity;


        impostorEntity = impostorQuadInstance.GetComponent<ImpostorEntity>();
        if (impostorEntity == null)
        {
            impostorEntity = impostorQuadInstance.AddComponent<ImpostorEntity>();
        }

        impostorEntity.meshPrefab = meshPrefab;
        impostorEntity.impostorMaterial = impostorMaterial;
        impostorEntity.playerTransform = playerTransform;
        impostorEntity.autoFindPlayer = false;

        impostorEntity.isAnimated = true;
        impostorEntity.animatedFPS = animatedFPS;

        impostorEntity.captureScale = captureScale;
        impostorEntity.meshRotationOffset = meshRotationOffset;
        impostorEntity.followParentRotation = followParentRotation;

        impostorEntity.customCameraHeight = customCameraHeight;
        impostorEntity.customLookAtRatio = customLookAtRatio;
        impostorEntity.customFieldOfView = customFieldOfView;
        impostorEntity.customDistanceMultiplier = customDistanceMultiplier;

        impostorEntity.autoGenerateCollider = autoGenerateCollider;
        impostorEntity.dynamicCollider = false;
        impostorEntity.colliderSize = colliderSize;
        impostorEntity.colliderCenter = colliderCenter;

        impostorEntity.useParallax = useParallax;
        impostorEntity.parallaxStrength = parallaxStrength;
        impostorEntity.parallaxMinSamples = parallaxMinSamples;
        impostorEntity.parallaxMaxSamples = parallaxMaxSamples;

        impostorEntity.snapToGround = snapToGround;
        impostorEntity.groundOffset = groundOffset;
        impostorEntity.groundLayers = groundLayers;
        impostorEntity.useBillboard = useBillboard;
        impostorEntity.positionOffset = positionOffset;
        impostorEntity.useStaticFace = useStaticFace;
        impostorEntity.staticFaceIndex = staticFaceIndex;

        ImpostorQuadScaler scaler = impostorQuadInstance.GetComponent<ImpostorQuadScaler>();
        if (scaler == null)
        {
            scaler = impostorQuadInstance.AddComponent<ImpostorQuadScaler>();
        }
        scaler.scaleMultiplier = quadScaleMultiplier;
        scaler.manualSize = quadManualSize;
        scaler.autoUpdate = false;

        Billboard billboard = impostorQuadInstance.GetComponent<Billboard>();
        if (billboard == null)
        {
            billboard = impostorQuadInstance.AddComponent<Billboard>();
        }
        billboard.lockToYAxis = true;
        billboard.useSmoothRotation = useSmoothRotation;
        billboard.rotationSpeed = rotationSpeed;
        billboard.rotationDeadZone = rotationDeadZone;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying && impostorEntity != null)
        {
            impostorEntity.meshRotationOffset = meshRotationOffset;
            impostorEntity.useStaticFace = useStaticFace;
            impostorEntity.staticFaceIndex = staticFaceIndex;

            Billboard billboard = impostorQuadInstance != null
                ? impostorQuadInstance.GetComponent<Billboard>()
                : null;
            if (billboard != null)
                billboard.enabled = useBillboard;

            var captureMethod = impostorEntity.GetType().GetMethod(
                "CaptureImpostor",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
            );

            captureMethod?.Invoke(impostorEntity, null);
        }
    }
#endif

}
