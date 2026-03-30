using UnityEngine;

public class ImpostorEntity : MonoBehaviour
{
    [Header("References")]
    public GameObject meshPrefab;
    public Material impostorMaterial;

    [Header("Auto-Setup")]
    [Tooltip("Trouve automatiquement le joueur avec le tag 'Player'")]
    public bool autoFindPlayer = true;
    public Transform playerTransform;

    [Header("Settings")]
    public bool isAnimated = false;
    [Range(1, 60)]
    public int animatedFPS = 15;
    public float staticUpdateInterval = 1f;

    [Header("Collision")]
    [Tooltip("G�n�re automatiquement le collider au d�marrage")]
    public bool autoGenerateCollider = true;

    [Tooltip("Le collider se met � jour � chaque frame (lent)")]
    public bool dynamicCollider = false;

    [Tooltip("Taille manuelle du BoxCollider (si (0,0,0) = auto)")]
    public Vector3 colliderSize = Vector3.zero;

    [Tooltip("Centre du BoxCollider")]
    public Vector3 colliderCenter = Vector3.zero;

    [Header("Ground Alignment")]
    public bool snapToGround = true;
    public float groundOffset = 0f;
    public LayerMask groundLayers = -1;

    [Header("Transition Settings")]
    [Tooltip("Active le blend progressif entre textures")]
    public bool useBlending = true;

    [Tooltip("Distance � laquelle le blend s'active (0 = toujours)")]
    public float blendDistance = 50f;

    [Tooltip("Rotation du mesh lors de la capture (utilisez Y=180 si le mesh est invers�)")]
    public Vector3 meshRotationOffset = Vector3.zero;

    [Tooltip("Le mesh suit la rotation du parent (entit� AI) lors de la capture")]
    public bool followParentRotation = true;

    [Header("Capture Settings")]
    [Tooltip("Multiplicateur de taille pour la capture (1 = auto, >1 = zoom out, <1 = zoom in)")]
    [Range(0.5f, 3f)]
    public float captureScale = 1f;

    [Tooltip("Hauteur de cam�ra personnalis�e (0 = utilise le d�faut)")]
    public float customCameraHeight = 0f;

    [Tooltip("Point de regard personnalis� (0-1, n�gatif = utilise le d�faut)")]
    [Range(-1f, 1f)]
    public float customLookAtRatio = -1f;

    [Tooltip("Field of View personnalis� (-1 = utilise le d�faut)")]
    [Range(-1f, 120f)]
    public float customFieldOfView = -1f;

    [Tooltip("Multiplicateur de distance de la cam�ra (-1 = utilise le d�faut, plus grand = plus loin)")]
    [Range(-1f, 5f)]
    public float customDistanceMultiplier = -1f;

    [Header("Parallax Settings")]
    [Tooltip("Active l'effet de parallax mapping")]
    public bool useParallax = true;

    [Tooltip("Force de l'effet parallax")]
    [Range(0f, 0.1f)]
    public float parallaxStrength = 0.03f;

    [Tooltip("�chantillons minimum pour le parallax")]
    [Range(4, 32)]
    public int parallaxMinSamples = 8;

    [Tooltip("�chantillons maximum pour le parallax")]
    [Range(4, 64)]
    public int parallaxMaxSamples = 32;

    [Header("Billboard Settings")]
    [Tooltip("Active le billboard (le quad tourne vers le joueur). Désactiver pour un impostor statique orienté manuellement.")]
    public bool useBillboard = true;

    [Header("Position Offset")]
    [Tooltip("Décalage local du quad par rapport au pivot de l'entité parent")]
    public Vector3 positionOffset = Vector3.zero;

    [Header("Static Face Settings")]
    [Tooltip("Verrouille l'impostor sur une face fixe, ignorant la position de la cam�ra")]
    public bool useStaticFace = false;

    [Tooltip("Index de la face fixe (0=avant, 2=gauche, 4=arri�re, 6=droite, valeurs interm�diaires = diagonales)")]
    [Range(0, 7)]
    public int staticFaceIndex = 0;

    private RenderTexture[] depthTextures;
    public Material ImpostorMaterialInstance => impostorMaterial;

    private GameObject meshInstance;
    private RenderTexture[] renderTextures;
    private MeshRenderer quadRenderer;
    private BoxCollider boxCollider;
    private ImpostorQuadScaler quadScaler;
    private float nextUpdateTime;
    private float updateInterval;
    private bool isInitialized = false;
    private RenderTexture atlas;

    // Cache for UpdateQuadTexture to avoid redundant GPU calls
    private int lastDirIndex = -1;
    private int lastNextDirIndex = -1;
    private float lastBlendFactor = -1f;

    // Throttle UpdateQuadTexture : recalcul de direction max 10x/s
    private const float QuadTextureUpdateInterval = 0.1f;
    private float nextQuadTextureUpdateTime;

    // Dernière position joueur connue pour détecter les micro-changements
    private Vector3 lastPlayerPos;

    void Start()
    {
        if (!isInitialized)
        {
            Initialize();
        }
    }

    void Initialize()
    {
        if (meshPrefab == null)
        {
            Debug.LogError("Mesh Prefab non assigné !");
            return;
        }

        if (autoFindPlayer && playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerTransform = player.transform;
        }

        quadRenderer = GetComponentInChildren<MeshRenderer>();

        if (quadRenderer == null)
        {
            Debug.LogError("Aucun MeshRenderer trouvé !");
            return;
        }

        if (impostorMaterial == null)
        {
            Shader shader = Shader.Find("Custom/ImpostorAtlas");

            if (shader == null)
            {
                Debug.LogError("Shader Custom/ImpostorAtlas introuvable !");
                return;
            }

            impostorMaterial = new Material(shader);
        }

        // Création mesh caché
        meshInstance = Instantiate(meshPrefab);
        meshInstance.name = meshPrefab.name + "_Impostor";
        meshInstance.transform.position = new Vector3(10000, 10000, 10000);

        // Setup du scaler avec les bounds du mesh (avant SetActive pour avoir des bounds valides)
        ImpostorQuadScaler scaler = GetComponent<ImpostorQuadScaler>();
        if (scaler != null)
        {
            Renderer meshRenderer = meshInstance.GetComponentInChildren<Renderer>();
            if (meshRenderer != null)
                scaler.sourceRenderer = meshRenderer;
            scaler.autoUpdate = false;
            scaler.UpdateScale();
        }

        meshInstance.SetActive(false);

        // Création atlas
        atlas = new RenderTexture(512, 256, 24);
        atlas.name = gameObject.name + "_Atlas";
        atlas.Create();

        // Material
        impostorMaterial = new Material(Shader.Find("Custom/ImpostorAtlas"));
        quadRenderer.material = impostorMaterial;

        impostorMaterial.SetTexture("_MainTex", atlas);
        impostorMaterial.SetFloat("_Columns", 4);
        impostorMaterial.SetFloat("_Rows", 2);

        updateInterval = isAnimated ? (1f / animatedFPS) : staticUpdateInterval;
        nextUpdateTime = Time.time + Random.Range(0f, updateInterval);

        // Apply position offset
        if (positionOffset != Vector3.zero)
            transform.localPosition += positionOffset;

        // Enable / disable billboard selon le paramètre
        Billboard billboard = GetComponent<Billboard>();
        if (billboard != null)
            billboard.enabled = useBillboard;

        CaptureImpostor();

        isInitialized = true;
    }

    void Update()
    {
        // Recapture uniquement pour les impostors anim�s (les statiques capturent une seule fois � l'init)
        if (isAnimated && Time.time >= nextUpdateTime)
        {
            CaptureImpostor();

            if (autoGenerateCollider && dynamicCollider)
                UpdateCollider();

            nextUpdateTime = Time.time + updateInterval;
        }

        // Throttle du calcul de direction : inutile � 60 Hz pour un billboard statique
        if (Time.time >= nextQuadTextureUpdateTime)
        {
            UpdateQuadTexture();
            nextQuadTextureUpdateTime = Time.time + QuadTextureUpdateInterval;
        }
    }

    void CaptureImpostor()
    {
        meshInstance.SetActive(true);

        Quaternion captureRotation = followParentRotation
            ? transform.rotation * Quaternion.Euler(meshRotationOffset)
            : Quaternion.Euler(meshRotationOffset);

        ImpostorPhotoBooth.Instance.RequestAtlasCapture(
            meshInstance,
            atlas,
            captureScale,
            captureRotation,
            customCameraHeight,
            customLookAtRatio,
            customFieldOfView,
            customDistanceMultiplier,
            () => { if (!isAnimated) meshInstance.SetActive(false); }
        );
    }


    void UpdateQuadTexture()
    {
        if (impostorMaterial == null || atlas == null) return;

        if (useStaticFace)
        {
            impostorMaterial.SetFloat("_Direction", staticFaceIndex);
            return;
        }

        if (playerTransform == null) return;

        int dirIndex = ImpostorDirectionHelper.GetDirectionIndexForRotatingEntity(
            transform,
            playerTransform.position,
            meshRotationOffset
        );

        impostorMaterial.SetFloat("_Direction", dirIndex);
    }



    void AlignToGround()
    {
        RaycastHit hit;

        //  Raycast depuis tr�s haut pour �tre s�r de toucher le sol
        Vector3 rayStart = transform.position;
        rayStart.y = 1000f; // Tr�s haut pour �tre s�r

        if (Physics.Raycast(rayStart, Vector3.down, out hit, 2000f, groundLayers))
        {
            Vector3 newPos = hit.point;
            newPos.y += groundOffset;
            transform.position = newPos;

#if UNITY_EDITOR
            Debug.Log($"{gameObject.name} collé au sol à Y={newPos.y:F2}");
#endif
        }
        else
        {
            //  Avertissement si aucun sol trouv�
            Debug.LogWarning($"{gameObject.name} : Aucun sol trouv� ! V�rifiez le LayerMask 'Ground Layers'");
        }
    }


    // Setup initial du collider (appel� une seule fois)
    void SetupCollider()
    {
        boxCollider = GetComponent<BoxCollider>();
        if (boxCollider == null)
        {
            boxCollider = gameObject.AddComponent<BoxCollider>();
        }

        if (colliderSize != Vector3.zero)
        {
            // Utiliser la taille manuelle
            boxCollider.size = colliderSize;
            boxCollider.center = colliderCenter;
        }
        else
        {
            // Calculer automatiquement
            UpdateCollider();
        }
    }

    // Mise � jour du collider (utilis� seulement en mode dynamique ou setup initial)
    void UpdateCollider()
    {
        if (quadRenderer == null || boxCollider == null) return;

        if (colliderSize != Vector3.zero)
        {
            // Utiliser la taille manuelle si sp�cifi�e
            boxCollider.size = colliderSize;
            boxCollider.center = colliderCenter;
        }
        else
        {
            // Calculer depuis le quad (mode auto)
            Bounds bounds = quadRenderer.bounds;

            Vector3 localSize = transform.InverseTransformVector(bounds.size);
            Vector3 localCenter = transform.InverseTransformPoint(bounds.center);

            boxCollider.size = new Vector3(
                Mathf.Abs(localSize.x),
                Mathf.Abs(localSize.y),
                0.5f // Profondeur fixe
            );
            boxCollider.center = Vector3.zero; // Toujours centr�
        }
    }

    void OnDestroy()
    {
        if (meshInstance != null)
        {
            Destroy(meshInstance);
        }

        if (renderTextures != null)
        {
            foreach (var rt in renderTextures)
            {
                if (rt != null)
                {
                    rt.Release();
                    Destroy(rt);
                }
            }
        }

        if (depthTextures != null)
        {
            foreach (var rt in depthTextures)
            {
                if (rt != null)
                {
                    rt.Release();
                    Destroy(rt);
                }
            }
        }

        if (impostorMaterial != null)
        {
            Destroy(impostorMaterial);
        }
    }

#if UNITY_EDITOR
    //  Visualiser le collider dans l'�diteur
    void OnDrawGizmosSelected()
    {
        if (boxCollider != null)
        {
            Gizmos.color = Color.green;
            Matrix4x4 rotationMatrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            Gizmos.matrix = rotationMatrix;
            Gizmos.DrawWireCube(boxCollider.center, boxCollider.size);
        }
    }
#endif
}
