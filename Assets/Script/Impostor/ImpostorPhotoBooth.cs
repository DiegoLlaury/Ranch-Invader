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

    [Header("Capture Settings")]
    public float captureDistance = 5f;
    public float captureHeight = 1.5f;
    public int renderTextureSize = 256;

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
            boothCamera.orthographic = true;
            boothCamera.orthographicSize = 5f;
            boothCamera.clearFlags = CameraClearFlags.SolidColor;
            boothCamera.backgroundColor = new Color(0, 0, 1, 0);
            boothCamera.cullingMask = 1 << boothLayer;
            boothCamera.nearClipPlane = 0.3f;
            boothCamera.farClipPlane = 1000f;
        }

        DontDestroyOnLoad(gameObject);
    }

    public void RequestCapture(GameObject meshObject, RenderTexture[] renderTextures, System.Action onComplete = null)
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
            originalLayer = meshObject.layer
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
        request.meshObject.transform.rotation = Quaternion.identity;

        CaptureAllDirections(request);

        SetLayerRecursively(request.meshObject, request.originalLayer);
        request.meshObject.transform.position = request.originalPosition;
        request.meshObject.transform.rotation = request.originalRotation;

        request.onComplete?.Invoke();

        ProcessNextCapture();
    }

    void CaptureAllDirections(ImpostorRequest request)
    {
        Vector3[] directions = new Vector3[]
        {
            new Vector3( 0, 0,  1),
            new Vector3( 1, 0,  1).normalized,
            new Vector3( 1, 0,  0),
            new Vector3( 1, 0, -1).normalized,
            new Vector3( 0, 0, -1),
            new Vector3(-1, 0, -1).normalized,
            new Vector3(-1, 0,  0),
            new Vector3(-1, 0,  1).normalized
        };

        for (int i = 0; i < 8; i++)
        {
            Vector3 dir = directions[i];
            Vector3 camPos = request.meshObject.transform.position - dir * captureDistance;
            camPos.y = request.meshObject.transform.position.y + captureHeight;

            boothCamera.transform.position = camPos;

            Vector3 lookTarget = request.meshObject.transform.position;
            lookTarget.y += captureHeight;
            boothCamera.transform.LookAt(lookTarget);

            boothCamera.targetTexture = request.renderTextures[i];
            boothCamera.Render();
        }
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
