using UnityEngine;
using UnityEngine.UI;

public class PixelArtCamera : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    [SerializeField] private RawImage _rawImage;

    [SerializeField] private int _cameraHeight = 180;

    private RenderTexture _renderTexture;

    private const int MaxSafeTextureSize = 16384;

    void Start()
    {
        UpdateRenderTexture();
    }

    void OnDestroy()
    {
        ReleaseTexture();
    }

    /// <summary>
    /// Recrée la RenderTexture pixel art en fonction de la taille d'écran actuelle.
    /// Appelé au démarrage et à chaque changement de résolution.
    /// </summary>
    public void UpdateRenderTexture()
    {
        // Screen.width/height peuvent être invalides pendant OnEnable du CanvasScaler
        if (Screen.width <= 0 || Screen.height <= 0)
            return;

        if (_cameraHeight <= 0)
        {
            Debug.LogError("[PixelArtCamera] _cameraHeight doit être supérieur à 0.");
            return;
        }

        float aspectRatio = (float)Screen.width / Screen.height;
        int cameraWidth = Mathf.RoundToInt(aspectRatio * _cameraHeight);

        // Clamp de sécurité pour éviter le crash GPU (max 16384)
        cameraWidth = Mathf.Clamp(cameraWidth, 1, MaxSafeTextureSize);
        int safeHeight = Mathf.Clamp(_cameraHeight, 1, MaxSafeTextureSize);

        if (cameraWidth == (_renderTexture?.width ?? 0) && safeHeight == (_renderTexture?.height ?? 0))
            return;

        ReleaseTexture();

        _renderTexture = new RenderTexture(cameraWidth, safeHeight, 16, RenderTextureFormat.ARGB32);
        _renderTexture.filterMode = FilterMode.Point;
        _renderTexture.name = "PixelArtRT";
        _renderTexture.Create();

        _camera.targetTexture = _renderTexture;
        _rawImage.texture = _renderTexture;
    }

    private void ReleaseTexture()
    {
        if (_renderTexture == null) return;
        _camera.targetTexture = null;
        _renderTexture.Release();
        Destroy(_renderTexture);
        _renderTexture = null;
    }
}
