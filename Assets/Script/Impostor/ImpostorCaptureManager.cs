using UnityEngine;

public class ImpostorCaptureManager : MonoBehaviour
{
    [Header("References")]
    public Camera impostorCamera;
    public ImpostorCamera cameraController;
    public RenderTexture[] renderTextures;

    [Header("Capture Settings")]
    [Tooltip("Est-ce que le mesh est animé ?")]
    public bool isAnimated = false;

    [Tooltip("Nombre de snapshots par seconde pour les meshes animés (15-16 recommandé)")]
    [Range(1, 60)]
    public int animatedFPS = 15;

    [Tooltip("Intervalle entre captures pour meshes statiques (en secondes)")]
    public float staticCaptureInterval = 1f;

    [Tooltip("Capturer toutes les directions à chaque frame ou une par une en rotation")]
    public bool captureAllDirectionsAtOnce = true;

    private float nextCaptureTime;
    private int currentDirection = 0;
    private float animatedCaptureInterval;

    void Start()
    {
        if (renderTextures.Length != 8)
        {
            Debug.LogError("Il faut exactement 8 RenderTextures !");
            enabled = false;
            return;
        }

        if (impostorCamera == null || cameraController == null)
        {
            Debug.LogError("Camera ou CameraController non assigné !");
            enabled = false;
            return;
        }

        impostorCamera.enabled = false;

        animatedCaptureInterval = 1f / animatedFPS;

        CaptureAllDirections();

        nextCaptureTime = Time.time + GetCaptureInterval();
    }

    void Update()
    {
        if (Time.time >= nextCaptureTime)
        {
            if (captureAllDirectionsAtOnce)
            {
                CaptureAllDirections();
            }
            else
            {
                CaptureCurrentDirection();
                currentDirection = (currentDirection + 1) % 8;
            }

            nextCaptureTime = Time.time + GetCaptureInterval();
        }
    }

    void CaptureCurrentDirection()
    {
        cameraController.SetDirection(currentDirection);
        impostorCamera.targetTexture = renderTextures[currentDirection];
        impostorCamera.Render();
    }

    void CaptureAllDirections()
    {
        for (int i = 0; i < 8; i++)
        {
            cameraController.SetDirection(i);
            impostorCamera.targetTexture = renderTextures[i];
            impostorCamera.Render();
        }
    }

    float GetCaptureInterval()
    {
        if (isAnimated)
        {
            return captureAllDirectionsAtOnce ? animatedCaptureInterval : animatedCaptureInterval / 8f;
        }
        else
        {
            return captureAllDirectionsAtOnce ? staticCaptureInterval : staticCaptureInterval / 8f;
        }
    }

    public void ForceCapture()
    {
        CaptureAllDirections();
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        animatedCaptureInterval = 1f / Mathf.Max(1, animatedFPS);
    }
#endif
}
