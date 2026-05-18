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

    [Tooltip("Active la hauteur de caméra personnalisée. Si désactivé, utilise la valeur par défaut du PhotoBooth.")]
    public bool overrideCameraHeight = false;
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

    [Header("Direction Hysteresis")]
    [Tooltip("Zone morte en degrés autour de chaque frontière de face. Évite le flickering quand le joueur est proche. 0 = désactivé, 20 = très stable.")]
    [Range(0f, 20f)]
    public float directionHysteresis = 8f;

    [Header("Face Weights")]
    [Tooltip("Poids angulaire de chaque face : 0=avant, 1=diag NE, 2=gauche, 3=diag SE, 4=arrière, 5=diag SO, 6=droite, 7=diag NO. " +
             "Augmenter le poids d'une face élargit son arc. Diminuer le rétrécit. Valeur uniforme = 1. Taille du tableau : 8 exactement.")]
    public float[] faceWeights = { 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f };

    [Header("Static Face Settings")]
    [Tooltip("Verrouille l'impostor sur une face fixe, ignorant la position de la cam�ra")]
    public bool useStaticFace = false;

    [Tooltip("Index de la face fixe (0=avant, 2=gauche, 4=arri�re, 6=droite, valeurs interm�diaires = diagonales)")]
    [Range(0, 7)]
    public int staticFaceIndex = 0;

    private RenderTexture[] depthTextures;
    public Material ImpostorMaterialInstance => impostorMaterial;

    private GameObject meshInstance;
    private Animator meshAnimator;
    private RenderTexture[] renderTextures;
    private MeshRenderer quadRenderer;
    private BoxCollider boxCollider;
    private ImpostorQuadScaler quadScaler;
    private float nextUpdateTime;
    private float updateInterval;
    private bool isInitialized = false;
    private RenderTexture atlas;
    private int currentDirectionIndex = 0;
    private float[] faceBoundaries;
    private Camera mainCamera;
    private bool isVisibleToCamera = true;

    // Transform du root ennemi (parent du quad) — utilisé pour le calcul de direction
    private Transform enemyRootTransform;

    // Animator parameter hashes — must match AnimC_Alien.controller
    private static readonly int IsMovingHash   = Animator.StringToHash("IsMoving");
    private static readonly int IsDetectedHash = Animator.StringToHash("IsDetected");
    private static readonly int AttackHash     = Animator.StringToHash("Attack");

    [Tooltip("AnimatorController à forcer sur le mesh instancié. Renseigné automatiquement par ImpostorEntityIA.")]
    [HideInInspector] public RuntimeAnimatorController animatorController;

    // Throttle UpdateQuadTexture : recalcul de direction max 10x/s
    private const float QuadTextureUpdateInterval = 0.1f;
    private float nextQuadTextureUpdateTime;

    // Dernière position joueur connue pour détecter les micro-changements
    private Vector3 lastPlayerPos;

    void Start()
    {
        // Si déjà initialisé par un appel externe (ex. ImpostorEntityIA), on ne re-initialise pas.
        if (!isInitialized)
        {
            Initialize();
        }
    }

    /// <summary>
    /// Initialise l'imposteur. Peut être appelé manuellement par ImpostorEntityIA
    /// après avoir configuré toutes les propriétés, pour éviter les problèmes d'ordre des Start().
    /// </summary>
    public void ForceInitialize()
    {
        if (!isInitialized)
            Initialize();
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
            mainCamera = Camera.main;
        }

        quadRenderer = GetComponentInChildren<MeshRenderer>();

        if (quadRenderer == null)
        {
            Debug.LogError("Aucun MeshRenderer trouvé !");
            return;
        }

        // Récupère le transform du parent ennemi pour le calcul de direction
        // (le quad lui-même est écrasé par Billboard, il faut la rotation du root)
        enemyRootTransform = transform.parent != null ? transform.parent : transform;

        // Création mesh caché
        meshInstance = Instantiate(meshPrefab);
        meshInstance.name = meshPrefab.name + "_Impostor";
        meshInstance.transform.position = new Vector3(10000, 10000, 10000);

        // Récupère et configure l'Animator uniquement en mode animé
        if (isAnimated)
        {
            meshAnimator = meshInstance.GetComponentInChildren<Animator>();
            if (meshAnimator == null)
            {
                Debug.LogWarning($"[ImpostorEntity] Aucun Animator trouvé sur '{meshPrefab.name}'. Les animations ne seront pas jouées.");
            }
            else
            {
                // Si un controller est fourni explicitement (ex. FBX sans controller natif), on l'assigne.
                // Sinon on garde celui déjà présent sur le prefab.
                if (animatorController != null)
                    meshAnimator.runtimeAnimatorController = animatorController;

                if (meshAnimator.runtimeAnimatorController == null)
                    Debug.LogWarning($"[ImpostorEntity] L'Animator de '{meshPrefab.name}' n'a pas de AnimatorController. Assignez-en un dans ImpostorEntityIA ou sur le prefab.");
            }
            
            if (isAnimated && (meshAnimator == null || meshAnimator.runtimeAnimatorController == null))
            {
                Debug.Log($"[ImpostorEntity] '{meshPrefab.name}' marqué animé mais sans AnimatorController valide. Bascule en atlas statique.");
                isAnimated = false;
            }
        }

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

        // En mode animé : le mesh reste actif en permanence pour que l'Animator joue
        // En mode statique : on le désactive après la première capture
        if (!isAnimated)
            meshInstance.SetActive(false);

        // Création atlas
        // Création atlas
        int cellSize = ImpostorPhotoBooth.Instance.renderTextureSize;
        atlas = new RenderTexture(cellSize * 4, cellSize * 2, 24, RenderTextureFormat.ARGB32);
        atlas.antiAliasing = 1;
        atlas.filterMode = FilterMode.Trilinear;
        atlas.anisoLevel = 4;
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

        // Précalcule les frontières angulaires custom si les poids ne sont pas uniformes.
        // null = chemin uniforme d'origine (aucun overhead).
        faceBoundaries = IsUniformFaceWeights(faceWeights)
            ? null
            : ImpostorDirectionHelper.BuildFaceBoundaries(faceWeights);

        CaptureImpostor();

        isInitialized = true;
    }

    void Update()
    {
        // Vérifie la visibilité par rapport à la caméra
        bool wasVisible = isVisibleToCamera;
        isVisibleToCamera = IsVisibleByCamera();

        // Active/désactive le renderer du quad sans toucher au reste
        if (quadRenderer != null && quadRenderer.enabled != isVisibleToCamera)
            quadRenderer.enabled = isVisibleToCamera;

        // Si hors écran, skip capture et calcul de direction
        if (!isVisibleToCamera) return;

        // Recapture uniquement pour les impostors animés
        if (isAnimated && Time.time >= nextUpdateTime)
        {
            CaptureImpostor();

            if (autoGenerateCollider && dynamicCollider)
                UpdateCollider();

            nextUpdateTime = Time.time + updateInterval;
        }

        // Throttle du calcul de direction
        if (Time.time >= nextQuadTextureUpdateTime)
        {
            UpdateQuadTexture();
            nextQuadTextureUpdateTime = Time.time + QuadTextureUpdateInterval;
        }
    }

    private bool IsVisibleByCamera()
    {
        if (mainCamera == null) return true;

        // Utilise la position du quad (pas du mesh caché)
        Vector3 viewportPoint = mainCamera.WorldToViewportPoint(transform.position);

        // viewportPoint.z < 0 = derrière la caméra
        // On ajoute une marge pour éviter le pop-in
        const float margin = 0.1f;
        return viewportPoint.z > 0f
            && viewportPoint.x > -margin && viewportPoint.x < 1f + margin
            && viewportPoint.y > -margin && viewportPoint.y < 1f + margin;
    }

    void CaptureImpostor()
    {
        meshInstance.SetActive(true);

        // On utilise enemyRootTransform (le root ennemi) pour la rotation de capture.
        // transform (le quad) est constamment réinitialisé par Billboard vers la caméra —
        // il ne représente jamais la rotation réelle de l'ennemi.
        Quaternion captureRotation;

        if (followParentRotation && enemyRootTransform != null)
        {
            // Entité rotative (IA NavMesh) : intègre la rotation monde du parent ennemi
            float yAngle = enemyRootTransform.eulerAngles.y;
            captureRotation = Quaternion.Euler(0f, yAngle, 0f) * Quaternion.Euler(meshRotationOffset);
        }
        else
        {
            // Entité statique (décor, vache...) : seul l'offset FBX est appliqué,
            // le quad billboard ne doit jamais polluer la rotation de capture
            captureRotation = Quaternion.Euler(meshRotationOffset);
        }

        ImpostorPhotoBooth.Instance.RequestAtlasCapture(
            meshInstance,
            atlas,
            captureScale,
            captureRotation,
            overrideCameraHeight ? customCameraHeight : -1f,
            customLookAtRatio,
            customFieldOfView,
            customDistanceMultiplier,
            // En mode statique on cache le mesh après capture. En mode animé il doit rester actif.
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

        Transform rotationSource = enemyRootTransform != null ? enemyRootTransform : transform;
        int candidateIndex;

        if (followParentRotation)
        {
            candidateIndex = ImpostorDirectionHelper.GetDirectionIndexForRotatingEntity(
                rotationSource,
                playerTransform.position,
                meshRotationOffset,
                faceBoundaries
            );
        }
        else
        {
            candidateIndex = ImpostorDirectionHelper.GetDirectionIndexFromRotation(
                rotationSource,
                playerTransform.position,
                meshRotationOffset,
                faceBoundaries
            );
        }

        // Hysteresis : on ne change de face que si le joueur est suffisamment
        // loin de la frontière actuelle dans la nouvelle direction.
        if (directionHysteresis > 0f && candidateIndex != currentDirectionIndex)
        {
            float rawAngle = GetRawAngle(rotationSource, playerTransform.position);

            // Calcule la frontière droite de la face courante selon le mode actif.
            float boundary;
            if (faceBoundaries != null)
            {
                boundary = faceBoundaries[currentDirectionIndex];
            }
            else
            {
                boundary = currentDirectionIndex * 45f + 22.5f;
            }

            float delta = Mathf.DeltaAngle(rawAngle, boundary);
            if (Mathf.Abs(delta) < directionHysteresis)
                candidateIndex = currentDirectionIndex;
        }

        currentDirectionIndex = candidateIndex;
        impostorMaterial.SetFloat("_Direction", currentDirectionIndex);
    }

    /// <summary>
    /// Retourne l'angle brut (0-360) depuis l'entité vers le joueur,
    /// dans le même espace que AngleToIndex (Atan2 X,Z en degrés positifs).
    /// </summary>
    private float GetRawAngle(Transform rotationSource, Vector3 playerPos)
    {
        Vector3 dir = playerPos - rotationSource.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return 0f;
        dir.Normalize();

        if (!followParentRotation && meshRotationOffset != Vector3.zero)
            dir = Quaternion.Euler(meshRotationOffset) * dir;

        float angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        if (angle < 0f) angle += 360f;
        return angle;
    }

    // Retourne true si tous les poids sont à 1 (frontières uniformes) → chemin rapide sans overhead.
    private bool IsUniformFaceWeights(float[] weights)
    {
        if (weights == null || weights.Length != 8) return true;
        foreach (float w in weights)
            if (!Mathf.Approximately(w, 1f)) return false;
        return true;
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

    // ── Animator Bridge ───────────────────────────────────────────────────────

    /// <summary>
    /// Sets the IsMoving and IsDetected booleans on the hidden mesh Animator.
    /// Called every frame by ImpostorEntityIA.
    /// </summary>
    public void SetMovementState(bool isMoving, bool isDetected)
    {
        if (meshAnimator == null || meshAnimator.runtimeAnimatorController == null) return;
        meshAnimator.SetBool(IsMovingHash,   isMoving);
        meshAnimator.SetBool(IsDetectedHash, isDetected);
    }

    /// <summary>
    /// Fires the Attack trigger on the hidden mesh Animator.
    /// Called by ImpostorEntityIA when the enemy attacks.
    /// </summary>
    public void TriggerAttack()
    {
        if (meshAnimator == null || meshAnimator.runtimeAnimatorController == null)
        {
            Debug.LogWarning($"[{gameObject.name}] TriggerAttack: meshAnimator is null or has no controller. " +
                             "Ensure ForceInitialize() completed successfully and the AnimatorController is assigned.");
            return;
        }
        meshAnimator.SetTrigger(AttackHash);
    }

    /// <summary>
    /// Resets the Attack trigger on the hidden mesh Animator to clear any queued trigger.
    /// </summary>
    public void ResetAttackTrigger()
    {
        if (meshAnimator == null || meshAnimator.runtimeAnimatorController == null)
        {
            Debug.LogWarning($"[{gameObject.name}] ResetAttackTrigger: meshAnimator is null or has no controller. " +
                             "Ensure ForceInitialize() completed successfully and the AnimatorController is assigned.");
            return;
        }
        meshAnimator.ResetTrigger(AttackHash);
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
