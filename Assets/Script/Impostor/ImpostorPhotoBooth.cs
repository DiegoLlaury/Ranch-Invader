using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ImpostorPhotoBooth : MonoBehaviour
{
    private static ImpostorPhotoBooth instance;
    public static ImpostorPhotoBooth Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("ImpostorPhotoBooth");
                instance = go.AddComponent<ImpostorPhotoBooth>();
                instance.Initialize();
            }
            return instance;
        }
    }

    // ──────────────────────────────────────────────
    // Loading tracking
    // ──────────────────────────────────────────────

    /// <summary>Total number of captures requested since scene start.</summary>
    public int TotalCapturesRequested { get; private set; }

    /// <summary>Number of captures completed so far.</summary>
    public int TotalCapturesCompleted { get; private set; }

    /// <summary>Progress from 0 to 1. Returns 1 if no captures were ever requested.</summary>
    public float Progress => TotalCapturesRequested == 0 ? 1f
        : (float)TotalCapturesCompleted / TotalCapturesRequested;

    /// <summary>True when all requested captures have been completed.</summary>
    public bool IsAllCapturesDone => TotalCapturesRequested > 0
        && TotalCapturesCompleted >= TotalCapturesRequested;

    /// <summary>Fired every time a capture completes. Parameter is current Progress (0–1).</summary>
    public event Action<float> OnCaptureProgressChanged;

    /// <summary>Fired once when every pending capture has been processed.</summary>
    public event Action OnAllCapturesDone;

    [Header("Booth Configuration")]
    public Camera boothCamera;
    public Transform captureZone;
    public Vector3 boothPosition = new Vector3(10000, 10000, 10000);
    public int boothLayer = 7;

    [Header("Camera Settings")]
    [Tooltip("Utiliser une caméra perspective au lieu d'orthographique")]
    public bool usePerspective = true;

    [Tooltip("Field of View de la caméra en mode perspective")]
    [Range(20f, 120f)]
    public float fieldOfView = 60f;

    [Header("Capture Settings")]
    [Tooltip("Résolution de chaque RenderTexture (256 = bon compromis qualité/perf, max 4096)")]
    [Range(64, 4096)]
    public int renderTextureSize = 256;
    public float paddingMultiplier = 1.2f;
    public float minOrthographicSize = 1f;
    public float maxOrthographicSize = 50f;

    [Tooltip("Multiplicateur de distance de la caméra (plus grand = plus loin)")]
    [Range(0.5f, 5f)]
    public float cameraDistanceMultiplier = 1.5f;

    [Tooltip("Hauteur de la caméra (niveau des yeux du joueur)")]
    public float cameraHeight = 1.7f;

    [Tooltip("Point de regard sur le mesh (0 = base, 0.5 = milieu, 1 = haut)")]
    [Range(0f, 1f)]
    public float lookAtHeightRatio = 0.4f;

    [Header("Depth Capture")]
    public bool captureDepth = false;
    public Shader depthCaptureShader;
    //public RenderTexture[] depthTextures;

    [Header("Performance")]
    [Tooltip("Nombre maximum de captures traitées par frame en mode normal (après le chargement).")]
    public int maxCapturesPerFrame = 1;

    [Tooltip("Captures par frame pendant le chargement initial. 8–16 = chargement rapide, pics de frame acceptables.")]
    [Range(1, 32)]
    public int maxCapturesPerFrameDuringLoading = 8;

    [Tooltip("Frames de grâce après vidage de la queue avant de quitter le mode loading.")]
    public int loadingGracePeriodFrames = 10;

    private int remainingGraceFrames;
    private bool isInLoadingPhase = true;

    private Queue<ImpostorRequest> captureQueue = new Queue<ImpostorRequest>();
    private bool isCapturing = false;
    private Material cachedDepthMaterial;

    private class ImpostorRequest
    {
        public GameObject meshObject;
        public RenderTexture atlas;
        public RenderTexture[] depthTextures;
        public System.Action onComplete;
        public Vector3 originalPosition;
        public Quaternion originalRotation;
        public int originalLayer;
        public float customScale;
        public Quaternion captureRotation;
        public float customCameraHeight;
        public float customLookAtRatio;
        public float customFieldOfView;
        public float customDistanceMultiplier;
        public Renderer[] cachedRenderers; // mis en cache une fois au RequestCapture
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ResetCounters();
    }

    /// <summary>Réinitialise les compteurs de captures pour un nouveau chargement de scène.</summary>
    public void ResetCounters()
    {
        TotalCapturesRequested = 0;
        TotalCapturesCompleted = 0;
        captureQueue.Clear();
        isCapturing = false;
        isInLoadingPhase = true;
        remainingGraceFrames = 0;
        enabled = false;
    }

    void Initialize()
    {
        transform.position = boothPosition;

        if (captureZone == null)
        {
            captureZone = new GameObject("CaptureZone").transform;
            captureZone.SetParent(transform);
            captureZone.localPosition = Vector3.zero;
        }

        if (boothCamera == null)
        {
            GameObject camGo = new GameObject("BoothCamera");
            camGo.transform.SetParent(transform);
            boothCamera = camGo.AddComponent<Camera>();

            boothCamera.enabled = false;

            //  Configuration en mode Perspective
            boothCamera.orthographic = !usePerspective;

            if (usePerspective)
            {
                boothCamera.fieldOfView = fieldOfView;
            }
            else
            {
                boothCamera.orthographicSize = 5f;
            }

            boothCamera.clearFlags = CameraClearFlags.SolidColor;
            boothCamera.backgroundColor = Color.clear;
            boothCamera.cullingMask = 1 << boothLayer;
            boothCamera.nearClipPlane = 0.3f;
            boothCamera.farClipPlane = 1000f;
        }

        if (captureDepth && depthCaptureShader == null)
        {
            depthCaptureShader = Shader.Find("Hidden/DepthCapture");
        }

        if (captureDepth && depthCaptureShader != null)
        {
            cachedDepthMaterial = new Material(depthCaptureShader);
        }

        DontDestroyOnLoad(gameObject);
    }


    // MODIFIÉ : Ajout du paramètre captureRotation
    public void RequestAtlasCapture(
     GameObject meshObject,
     RenderTexture atlas,
     float customScale = 1f,
     Quaternion? captureRotation = null,
     float customCameraHeight = -1f,
     float customLookAtRatio = -1f,
     float customFieldOfView = -1f,
     float customDistanceMultiplier = -1f,
     System.Action onComplete = null)
    {
        if (atlas == null)
        {
            Debug.LogError("Atlas null !");
            return;
        }

        ImpostorRequest request = new ImpostorRequest
        {
            meshObject = meshObject,
            atlas = atlas,
            onComplete = onComplete,
            originalPosition = meshObject.transform.position,
            originalRotation = meshObject.transform.rotation,
            originalLayer = meshObject.layer,
            customScale = customScale,
            captureRotation = captureRotation ?? Quaternion.identity,
            customCameraHeight = customCameraHeight,
            customLookAtRatio = customLookAtRatio,
            customFieldOfView = customFieldOfView,
            customDistanceMultiplier = customDistanceMultiplier,
            cachedRenderers = meshObject.GetComponentsInChildren<Renderer>()
        };

        TotalCapturesRequested++;
        captureQueue.Enqueue(request);
        isCapturing = true;
        enabled = true;
    }


    void Update()
    {
        if (!isCapturing || captureQueue.Count == 0)
        {
            // La queue est vide — gestion de la fin du mode loading.
            if (isInLoadingPhase)
            {
                if (remainingGraceFrames > 0)
                {
                    remainingGraceFrames--;
                }
                else
                {
                    isInLoadingPhase = false;
                }
            }

            isCapturing = false;
            enabled = false;
            return;
        }

        // Réinitialise la grâce si de nouvelles captures arrivent encore.
        remainingGraceFrames = loadingGracePeriodFrames;

        int capturesThisFrame = isInLoadingPhase ? maxCapturesPerFrameDuringLoading : maxCapturesPerFrame;

        int processed = 0;
        while (captureQueue.Count > 0 && processed < capturesThisFrame)
        {
            ProcessNextCapture();
            processed++;
        }

        if (captureQueue.Count == 0)
        {
            isCapturing = false;
            enabled = false;
        }
    }


    void ProcessNextCapture()
    {
        if (captureQueue.Count == 0)
        {
            isCapturing = false;
            return;
        }

        ImpostorRequest request = captureQueue.Dequeue();

        // L'objet peut avoir été détruit (ennemi mort) pendant qu'il attendait en queue.
        if (request.meshObject == null)
        {
            request.onComplete?.Invoke();
            return;
        }

        SetLayerRecursively(request.meshObject, boothLayer);
        request.meshObject.transform.position = captureZone.position;
        request.meshObject.transform.rotation = request.captureRotation;

        CaptureAtlas(request);

        // Re-vérifier après la capture — un Destroy() aurait pu survenir dans CaptureAtlas via events.
        if (request.meshObject != null)
        {
            SetLayerRecursively(request.meshObject, request.originalLayer);
            request.meshObject.transform.position = request.originalPosition;
            request.meshObject.transform.rotation = request.originalRotation;
        }

        request.onComplete?.Invoke();

        TotalCapturesCompleted++;
        OnCaptureProgressChanged?.Invoke(Progress);

        if (IsAllCapturesDone)
            OnAllCapturesDone?.Invoke();
    }

    void CaptureAtlas(ImpostorRequest request)
    {
        if (usePerspective)
        {
            boothCamera.fieldOfView = request.customFieldOfView > 0
                ? request.customFieldOfView
                : fieldOfView;
        }

        RenderTexture atlas = request.atlas;

        int gridX = 4;
        int gridY = 2;

        int cellWidth = atlas.width / gridX;
        int cellHeight = atlas.height / gridY;

        Bounds meshBounds = CalculateBoundsFromRenderers(
    request.cachedRenderers,
    request.meshObject.transform
);

        float maxSize = Mathf.Max(meshBounds.size.x, meshBounds.size.y, meshBounds.size.z);

        float distanceMultiplier = request.customDistanceMultiplier > 0
            ? request.customDistanceMultiplier
            : cameraDistanceMultiplier;

        float cameraDistance = maxSize * distanceMultiplier;

        float lookRatio = request.customLookAtRatio >= 0
            ? request.customLookAtRatio
            : lookAtHeightRatio;

        // Centre géométrique monde du mesh (indépendant du pivot FBX)
        // meshBounds.center est en espace local → on repasse en monde
        Vector3 meshWorldCenter = request.meshObject.transform.TransformPoint(meshBounds.center);

        // Le point de regard utilise X/Z du centre géométrique, Y ajustable via lookRatio
        Vector3 lookAtPoint = new Vector3(
            meshWorldCenter.x,
            request.meshObject.transform.position.y + meshBounds.size.y * lookRatio,
            meshWorldCenter.z
        );

        float camHeight = request.customCameraHeight != -1f
        ? request.customCameraHeight
        : cameraHeight;

        Vector3[] directions = new Vector3[]
        {
        new Vector3(0,0,1),
        new Vector3(1,0,1).normalized,
        new Vector3(1,0,0),
        new Vector3(1,0,-1).normalized,
        new Vector3(0,0,-1),
        new Vector3(-1,0,-1).normalized,
        new Vector3(-1,0,0),
        new Vector3(-1,0,1).normalized
        };

        boothCamera.targetTexture = atlas;

        for (int i = 0; i < 8; i++)
        {
            int x = i % gridX;
            int y = i / gridX;

            boothCamera.pixelRect = new Rect(
                x * cellWidth,
                y * cellHeight,
                cellWidth,
                cellHeight
            );

            Vector3 dir = directions[i];

            Vector3 camPos = meshWorldCenter + dir * cameraDistance;
            camPos.y = meshWorldCenter.y + camHeight;

            boothCamera.transform.position = camPos;
            boothCamera.transform.LookAt(lookAtPoint);

            boothCamera.Render();
        }

        boothCamera.pixelRect = new Rect(0, 0, atlas.width, atlas.height);
    }

    void SetCameraDirection(int i, ImpostorRequest request)
    {
        Vector3[] directions = new Vector3[]
        {
        new Vector3(0,0,1),
        new Vector3(1,0,1).normalized,
        new Vector3(1,0,0),
        new Vector3(1,0,-1).normalized,
        new Vector3(0,0,-1),
        new Vector3(-1,0,-1).normalized,
        new Vector3(-1,0,0),
        new Vector3(-1,0,1).normalized
        };

        Vector3 dir = directions[i];

        Vector3 pos = request.meshObject.transform.position - dir * 5f;
        pos.y += 1.5f;

        boothCamera.transform.position = pos;
        boothCamera.transform.LookAt(request.meshObject.transform.position);
    }

   


    void RenderDepthTexture(Renderer[] renderers, RenderTexture depthRT)
    {
        if (cachedDepthMaterial == null) return;

        RenderTexture originalRT = boothCamera.targetTexture;
        boothCamera.targetTexture = depthRT;

        Material[][] originalMaterials = new Material[renderers.Length][];

        for (int i = 0; i < renderers.Length; i++)
        {
            originalMaterials[i] = renderers[i].sharedMaterials;
            Material[] depthMats = new Material[renderers[i].sharedMaterials.Length];
            for (int j = 0; j < depthMats.Length; j++)
                depthMats[j] = cachedDepthMaterial;
            renderers[i].sharedMaterials = depthMats;
        }

        boothCamera.Render();

        for (int i = 0; i < renderers.Length; i++)
            renderers[i].sharedMaterials = originalMaterials[i];

        boothCamera.targetTexture = originalRT;
    }

    void OnDestroy()
    {
        if (cachedDepthMaterial != null)
            Destroy(cachedDepthMaterial);
    }


    Bounds CalculateBoundsFromRenderers(Renderer[] renderers, Transform root)
    {
        if (renderers == null || renderers.Length == 0)
            return new Bounds(Vector3.zero, Vector3.one);

        // On accumule uniquement les renderers actifs pour éviter des bounds aberrantes
        Bounds? accumulated = null;
        foreach (Renderer r in renderers)
        {
            if (r == null || !r.gameObject.activeInHierarchy) continue;
            if (accumulated == null)
                accumulated = r.bounds;
            else
            {
                Bounds b = accumulated.Value;
                b.Encapsulate(r.bounds);
                accumulated = b;
            }
        }

        if (accumulated == null)
        {
            Debug.LogWarning($"[ImpostorPhotoBooth] Aucun renderer actif trouvé sur '{root.name}'. Bounds par défaut utilisées.");
            return new Bounds(Vector3.zero, Vector3.one);
        }

        Bounds result = accumulated.Value;

        // Convertir le centre en espace local pour cohérence avec les positions relatives
        result.center = root.InverseTransformPoint(result.center);

        // Sanity check : taille aberrante = mesh probablement mal positionné
        float maxDim = Mathf.Max(result.size.x, result.size.y, result.size.z);
        if (maxDim > 100f)
            Debug.LogWarning($"[ImpostorPhotoBooth] Bounds suspectes sur '{root.name}' : size={result.size}, maxDim={maxDim:F1}. Vérifiez le scale et la position du mesh.");

        return result;
    }

    Bounds CalculateBounds(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        return CalculateBoundsFromRenderers(renderers, obj.transform);
    }

    void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    public void CreateRenderTexturePair(string baseName, out RenderTexture[] colorTextures, out RenderTexture[] depthTextures)
    {
        // Clamp de sécurité — évite le crash GPU si la valeur Inspector est aberrante
        int safeSize = Mathf.Clamp(renderTextureSize, 64, SystemInfo.maxTextureSize);
        if (safeSize != renderTextureSize)
            Debug.LogError($"[ImpostorPhotoBooth] renderTextureSize ({renderTextureSize}) dépasse la limite GPU ({SystemInfo.maxTextureSize}). Valeur ramenée à {safeSize}.");

        colorTextures = new RenderTexture[8];
        depthTextures = captureDepth ? new RenderTexture[8] : null;

        string[] directionNames = { "North", "NorthEast", "East", "SouthEast", "South", "SouthWest", "West", "NorthWest" };

        for (int i = 0; i < 8; i++)
        {
            colorTextures[i] = new RenderTexture(safeSize, safeSize, 24);
            colorTextures[i].name = $"{baseName}_Color_{directionNames[i]}";
            colorTextures[i].filterMode = FilterMode.Bilinear;
            colorTextures[i].wrapMode = TextureWrapMode.Clamp;

            if (captureDepth)
            {
                depthTextures[i] = new RenderTexture(safeSize, safeSize, 0, RenderTextureFormat.RFloat);
                depthTextures[i].name = $"{baseName}_Depth_{directionNames[i]}";
                depthTextures[i].filterMode = FilterMode.Bilinear;
                depthTextures[i].wrapMode = TextureWrapMode.Clamp;
            }
        }
    }

    // Gardez aussi l'ancienne méthode pour compatibilité
    public RenderTexture[] CreateRenderTextures(string baseName)
    {
        RenderTexture[] colorTextures;
        RenderTexture[] depthTextures;
        CreateRenderTexturePair(baseName, out colorTextures, out depthTextures);
        return colorTextures;
    }

}
