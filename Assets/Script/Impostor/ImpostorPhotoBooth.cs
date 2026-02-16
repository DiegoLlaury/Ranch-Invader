using UnityEngine;
using System.Collections.Generic;

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

    private Queue<ImpostorRequest> captureQueue = new Queue<ImpostorRequest>();
    private bool isCapturing = false;

    private class ImpostorRequest
    {
        public GameObject meshObject;
        public RenderTexture[] renderTextures;
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
            boothCamera.backgroundColor = new Color(0, 0, 1, 0);
            boothCamera.cullingMask = 1 << boothLayer;
            boothCamera.nearClipPlane = 0.3f;
            boothCamera.farClipPlane = 1000f;
        }

        DontDestroyOnLoad(gameObject);
    }


    // MODIFIÉ : Ajout du paramètre captureRotation
    public void RequestCapture(
    GameObject meshObject,
    RenderTexture[] renderTextures,
    float customScale = 1f,
    Quaternion? captureRotation = null,
    float customCameraHeight = -1f,
    float customLookAtRatio = -1f,
    float customFieldOfView = -1f,
    float customDistanceMultiplier = -1f, 
    System.Action onComplete = null)
    {
        if (renderTextures == null || renderTextures.Length != 8)
        {
            Debug.LogError("Il faut 8 RenderTextures pour une capture impostor !");
            return;
        }

        ImpostorRequest request = new ImpostorRequest
        {
            meshObject = meshObject,
            renderTextures = renderTextures,
            onComplete = onComplete,
            originalPosition = meshObject.transform.position,
            originalRotation = meshObject.transform.rotation,
            originalLayer = meshObject.layer,
            customScale = customScale,
            captureRotation = captureRotation ?? Quaternion.identity,
            customCameraHeight = customCameraHeight,
            customLookAtRatio = customLookAtRatio,
            customFieldOfView = customFieldOfView,
            customDistanceMultiplier = customDistanceMultiplier  
        };

        captureQueue.Enqueue(request);

        if (!isCapturing)
        {
            ProcessNextCapture();
        }
    }


    void ProcessNextCapture()
    {
        if (captureQueue.Count == 0)
        {
            isCapturing = false;
            return;
        }

        isCapturing = true;
        ImpostorRequest request = captureQueue.Dequeue();

        SetLayerRecursively(request.meshObject, boothLayer);
        request.meshObject.transform.position = captureZone.position;
        request.meshObject.transform.rotation = request.captureRotation; // Utilise la rotation configurée

        CaptureAllDirections(request);

        SetLayerRecursively(request.meshObject, request.originalLayer);
        request.meshObject.transform.position = request.originalPosition;
        request.meshObject.transform.rotation = request.originalRotation;

        request.onComplete?.Invoke();

        ProcessNextCapture();
    }

    void CaptureAllDirections(ImpostorRequest request)
    {
        Bounds meshBounds = CalculateBounds(request.meshObject);

        float maxSize = Mathf.Max(meshBounds.size.x, meshBounds.size.y, meshBounds.size.z);

        //  Utiliser le multiplicateur de distance personnalisé ou la valeur par défaut
        float activeDistanceMultiplier = request.customDistanceMultiplier > 0
            ? request.customDistanceMultiplier
            : cameraDistanceMultiplier;

        //  Utiliser le FOV personnalisé ou la valeur par défaut
        float activeFOV = request.customFieldOfView > 0 ? request.customFieldOfView : fieldOfView;

        float cameraDistance;

        // Configuration de la caméra selon le mode
        if (usePerspective)
        {
            float halfFOV = activeFOV * 0.5f * Mathf.Deg2Rad;
            float targetHeight = meshBounds.size.y * paddingMultiplier;

            // Calculer la distance de base
            cameraDistance = (targetHeight * 0.5f) / Mathf.Tan(halfFOV);

            //  Appliquer le multiplicateur de distance
            cameraDistance *= activeDistanceMultiplier;

            boothCamera.orthographic = false;
            boothCamera.fieldOfView = activeFOV;
        }
        else
        {
            float orthoSize = (maxSize * paddingMultiplier * request.customScale) / 2f;
            orthoSize = Mathf.Clamp(orthoSize, minOrthographicSize, maxOrthographicSize);

            cameraDistance = maxSize * activeDistanceMultiplier;

            boothCamera.orthographic = true;
            boothCamera.orthographicSize = orthoSize;
        }

        Vector3[] directions = new Vector3[]
        {
        new Vector3( 0, 0,  1),
        new Vector3(-1, 0,  1).normalized,
        new Vector3(-1, 0,  0),
        new Vector3(-1, 0, -1).normalized,
        new Vector3( 0, 0, -1),
        new Vector3( 1, 0, -1).normalized,
        new Vector3( 1, 0,  0),
        new Vector3( 1, 0,  1).normalized
        };

        Vector3 meshBasePosition = request.meshObject.transform.position;

        float activeLookAtRatio = request.customLookAtRatio >= 0 ? request.customLookAtRatio : lookAtHeightRatio;

        Vector3 lookAtPoint = meshBasePosition;
        lookAtPoint.y += meshBounds.size.y * activeLookAtRatio;

        float activeCameraHeight = request.customCameraHeight >= 0 ? request.customCameraHeight : cameraHeight;

        for (int i = 0; i < 8; i++)
        {
            Vector3 dir = directions[i];

            Vector3 camPos = meshBasePosition - dir * cameraDistance;
            camPos.y = meshBasePosition.y + activeCameraHeight;

            boothCamera.transform.position = camPos;
            boothCamera.transform.LookAt(lookAtPoint);

            boothCamera.targetTexture = request.renderTextures[i];
            boothCamera.Render();
        }
    }





    Bounds CalculateBounds(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
        {
            return new Bounds(obj.transform.position, Vector3.one);
        }

        Bounds bounds = renderers[0].bounds;

        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        bounds.center = obj.transform.InverseTransformPoint(bounds.center);

        return bounds;
    }

    void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    public RenderTexture[] CreateRenderTextures(string baseName)
    {
        RenderTexture[] textures = new RenderTexture[8];
        string[] directionNames = { "North", "NorthEast", "East", "SouthEast", "South", "SouthWest", "West", "NorthWest" };

        for (int i = 0; i < 8; i++)
        {
            textures[i] = new RenderTexture(renderTextureSize, renderTextureSize, 24);
            textures[i].name = $"{baseName}_{directionNames[i]}";
            textures[i].filterMode = FilterMode.Bilinear;
            textures[i].wrapMode = TextureWrapMode.Clamp;
        }

        return textures;
    }
}
