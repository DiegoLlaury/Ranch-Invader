using UnityEngine;

public class ImpostorQuadScaler : MonoBehaviour
{
    public Renderer sourceRenderer;

    void Start()
    {
        Bounds b = sourceRenderer.bounds;
        transform.localScale = new Vector3(b.size.x, b.size.y, 1f);
    }
}
