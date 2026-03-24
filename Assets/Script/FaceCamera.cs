using UnityEngine;

public class FaceCameraScript : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;

    private Camera cam;

    private void Start()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        if (cam == null) return;
        FaceCamera();
    }

    void FaceCamera()
    {
        Vector3 dir = transform.position - cam.transform.position;
        dir.y = 0f;
        spriteRenderer.transform.rotation = Quaternion.LookRotation(dir);
    }
}
