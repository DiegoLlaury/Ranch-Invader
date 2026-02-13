using UnityEngine;

public class FaceCameraScript : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;

    private void Update()
    {
        FaceCamera();
    }
    void FaceCamera()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 dir = transform.position - cam.transform.position;
        dir.y = 0f;
        spriteRenderer.transform.rotation = Quaternion.LookRotation(dir);
    }
}
