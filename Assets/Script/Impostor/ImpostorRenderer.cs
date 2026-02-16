using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class ImpostorRenderer : MonoBehaviour
{
    public Transform player;
    public RenderTexture[] impostorTextures; // 8 RT
    public Material impostorMaterial;

    private MeshRenderer meshRenderer;

    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material = impostorMaterial;
    }

    void LateUpdate()
    {

        int dirIndex = ImpostorDirectionHelper.GetDirectionIndex(
            transform.position,
            player.position
        );

        impostorMaterial.SetTexture("_MainTex", impostorTextures[dirIndex]);
    }
}
