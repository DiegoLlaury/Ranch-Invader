using UnityEngine;

public class ImpostorEntity : MonoBehaviour
{
    [Header("References")]
    public GameObject meshPrefab;
    public Material impostorMaterial;
    public Transform playerTransform;

    [Header("Settings")]
    public bool isAnimated = false;
    [Range(1, 60)]
    public int animatedFPS = 15;
    public float staticUpdateInterval = 1f;

    private GameObject meshInstance;
    private RenderTexture[] renderTextures;
    private MeshRenderer quadRenderer;
    private float nextUpdateTime;
    private float updateInterval;

    void Start()
    {
        if (meshPrefab == null)
        {
            Debug.LogError("Mesh Prefab non assigné sur ImpostorEntity !");
            enabled = false;
            return;
        }

        if (playerTransform == null)
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        }

        quadRenderer = GetComponentInChildren<MeshRenderer>();

        if (quadRenderer == null)
        {
            Debug.LogError("Aucun MeshRenderer trouvé sur l'ImpostorQuad !");
            enabled = false;
            return;
        }

        Initialize();
    }

    void Initialize()
    {
        meshInstance = Instantiate(meshPrefab);
        meshInstance.name = $"{meshPrefab.name}_ImpostorMesh";
        meshInstance.transform.position = new Vector3(10000, 10000, 10000);
        meshInstance.SetActive(false);

        renderTextures = ImpostorPhotoBooth.Instance.CreateRenderTextures(gameObject.name);

        if (impostorMaterial == null)
        {
            impostorMaterial = new Material(Shader.Find("Custom/ImpostorClean"));
        }
        else
        {
            impostorMaterial = new Material(impostorMaterial);
        }

        quadRenderer.material = impostorMaterial;

        updateInterval = isAnimated ? (1f / animatedFPS) : staticUpdateInterval;

        CaptureImpostor();
    }

    void Update()
    {
        if (Time.time >= nextUpdateTime)
        {
            if (isAnimated || meshInstance.activeSelf)
            {
                CaptureImpostor();
            }
            nextUpdateTime = Time.time + updateInterval;
        }

        UpdateQuadTexture();
    }

    void CaptureImpostor()
    {
        meshInstance.SetActive(true);

        ImpostorPhotoBooth.Instance.RequestCapture(meshInstance, renderTextures, () =>
        {
            if (!isAnimated)
            {
                meshInstance.SetActive(false);
            }
        });
    }

    void UpdateQuadTexture()
    {
        if (playerTransform == null || renderTextures == null)
            return;

        int dirIndex = ImpostorDirectionHelper.GetDirectionIndex(transform.position, playerTransform.position);
        impostorMaterial.SetTexture("_MainTex", renderTextures[dirIndex]);
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

        if (impostorMaterial != null)
        {
            Destroy(impostorMaterial);
        }
    }
}
