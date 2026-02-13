using UnityEngine;
using UnityEngine.UI;

public class PixelArtCamera : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    [SerializeField] private RawImage _rawImage;

    [SerializeField] private int _cameraHeight;

    private RenderTexture _renderTexture;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdateRenderTexture();
    }

    private void UpdateRenderTexture()
    {
        if (_renderTexture != null)
        {
            _renderTexture.Release();
        }

        float aspectRatio = (float)Screen.width / Screen.height;
        int cameraWidth = Mathf.RoundToInt(aspectRatio * _cameraHeight);

        _renderTexture = new RenderTexture(cameraWidth, _cameraHeight, 16, RenderTextureFormat.ARGB32);
        _renderTexture.filterMode = FilterMode.Point;

        _renderTexture.Create();
        _camera.targetTexture = _renderTexture;
        _rawImage.texture = _renderTexture;
    }
}
