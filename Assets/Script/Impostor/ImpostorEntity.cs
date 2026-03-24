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
            Debug.LogError("Mesh Prefab non assign� sur ImpostorEntity !");
            enabled = false;
            return;
        }

        if (autoFindPlayer && playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                Debug.LogWarning("Aucun GameObject avec le tag 'Player' trouv� !");
            }
        }

        quadRenderer = GetComponentInChildren<MeshRenderer>();

        if (quadRenderer == null)
        {
            Debug.LogError("Aucun MeshRenderer trouv� sur l'ImpostorQuad !");
            enabled = false;
            return;
        }

        if (snapToGround)
        {
            AlignToGround();
        }

        meshInstance = Instantiate(meshPrefab);
        meshInstance.name = $"{meshPrefab.name}_ImpostorMesh";
        meshInstance.transform.position = new Vector3(10000, 10000, 10000);
        meshInstance.SetActive(false);

        if (useParallax)
        {
            ImpostorPhotoBooth.Instance.CreateRenderTexturePair(gameObject.name, out renderTextures, out depthTextures);
        }
        else
        {
            renderTextures = ImpostorPhotoBooth.Instance.CreateRenderTextures(gameObject.name);
            depthTextures = null;
        }

        if (impostorMaterial == null)
        {
            string shaderName = useParallax ? "Custom/ImpostorParallax" : "Custom/ImpostorClean";
            impostorMaterial = new Material(Shader.Find(shaderName));
        }
        else
        {
            impostorMaterial = new Material(impostorMaterial);
        }

        quadRenderer.material = impostorMaterial;

        // Configurer les paramètres de parallax
        if (useParallax)
        {
            impostorMaterial.SetFloat("_ParallaxStrength", parallaxStrength);
            impostorMaterial.SetFloat("_ParallaxMinSamples", parallaxMinSamples);
            impostorMaterial.SetFloat("_ParallaxMaxSamples", parallaxMaxSamples);
        }

        updateInterval = isAnimated ? (1f / animatedFPS) : staticUpdateInterval;

        // Décalage aléatoire pour éviter que tous les impostors recapturent la même frame
        nextUpdateTime = Time.time + Random.Range(0f, updateInterval);

        quadScaler = GetComponent<ImpostorQuadScaler>();
        if (quadScaler != null && meshInstance != null)
        {
            Renderer meshRenderer = meshInstance.GetComponentInChildren<Renderer>();
            if (meshRenderer != null)
            {
                quadScaler.sourceRenderer = meshRenderer;
            }
        }

        CaptureImpostor();

        if (quadScaler != null)
        {
            quadScaler.UpdateScale();
        }

        if (autoGenerateCollider)
        {
            SetupCollider();
        }

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

        Quaternion captureRotation = Quaternion.Euler(meshRotationOffset);

        ImpostorPhotoBooth.Instance.RequestCapture(
            meshInstance,
            renderTextures,
            captureScale,
            captureRotation,
            customCameraHeight,
            customLookAtRatio,
            customFieldOfView,
            customDistanceMultiplier,
            () =>
            {
                if (!isAnimated)
                {
                    meshInstance.SetActive(false);
                }
            }
        );
    }

    void UpdateQuadTexture()
    {
        if (renderTextures == null)
            return;

        // Mode face fixe : ignore complètement la position du joueur
        if (useStaticFace)
        {
            int clampedIndex = Mathf.Clamp(staticFaceIndex, 0, renderTextures.Length - 1);
            if (lastDirIndex == clampedIndex) return;
            lastDirIndex = clampedIndex;
            impostorMaterial.SetTexture("_MainTex", renderTextures[clampedIndex]);
            impostorMaterial.SetFloat("_BlendAmount", 0f);
            return;
        }

        if (playerTransform == null)
            return;

        float distanceToPlayer = (transform.position - playerTransform.position).sqrMagnitude;
        float blendDistSqr = blendDistance * blendDistance;

        if (useBlending && (blendDistance <= 0 || distanceToPlayer <= blendDistSqr))
        {
            int dirIndex, nextDirIndex;
            float blendFactor;

            if (followParentRotation && transform.parent != null)
            {
                ImpostorDirectionHelper.GetDirectionBlendForRotatingEntity(
                    transform.parent,
                    playerTransform.position,
                    meshRotationOffset,
                    out dirIndex,
                    out nextDirIndex,
                    out blendFactor
                );
            }
            else
            {
                ImpostorDirectionHelper.GetDirectionBlend(
                    transform.position,
                    playerTransform.position,
                    out dirIndex,
                    out nextDirIndex,
                    out blendFactor
                );
            }

            if (dirIndex == lastDirIndex && nextDirIndex == lastNextDirIndex && Mathf.Approximately(blendFactor, lastBlendFactor))
                return;

            lastDirIndex = dirIndex;
            lastNextDirIndex = nextDirIndex;
            lastBlendFactor = blendFactor;

            impostorMaterial.SetTexture("_MainTex", renderTextures[dirIndex]);
            impostorMaterial.SetTexture("_BlendTex", renderTextures[nextDirIndex]);
            impostorMaterial.SetFloat("_BlendAmount", blendFactor);
        }
        else
        {
            int dirIndex;

            if (followParentRotation && transform.parent != null)
            {
                dirIndex = ImpostorDirectionHelper.GetDirectionIndexForRotatingEntity(
                    transform.parent,
                    playerTransform.position,
                    meshRotationOffset
                );
            }
            else
            {
                dirIndex = ImpostorDirectionHelper.GetDirectionIndex(
                    transform.position,
                    playerTransform.position
                );
            }

            if (dirIndex == lastDirIndex && lastBlendFactor == 0f) return;

            lastDirIndex = dirIndex;
            lastNextDirIndex = -1;
            lastBlendFactor = 0f;

            impostorMaterial.SetTexture("_MainTex", renderTextures[dirIndex]);
            impostorMaterial.SetFloat("_BlendAmount", 0f);
        }
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
