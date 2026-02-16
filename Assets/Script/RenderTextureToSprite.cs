using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class RenderTextureToSprite : MonoBehaviour
{
    public RenderTexture renderTexture;
    public Material chromaKeyMaterial;

    private Texture2D texture2D;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        texture2D = new Texture2D(
            renderTexture.width,
            renderTexture.height,
            TextureFormat.RGBA32,
            false
        );

        Sprite sprite = Sprite.Create(
            texture2D,
            new Rect(0, 0, texture2D.width, texture2D.height),
            new Vector2(0.5f, 0.5f),
            100f
        );

        spriteRenderer.sprite = sprite;
    }

    void LateUpdate()
    {
        RenderTexture.active = renderTexture;

        if (chromaKeyMaterial != null)
            Graphics.Blit(renderTexture, renderTexture, chromaKeyMaterial);

        texture2D.ReadPixels(
            new Rect(0, 0, renderTexture.width, renderTexture.height),
            0,
            0
        );

        texture2D.Apply();

        RenderTexture.active = null;
    }
}
