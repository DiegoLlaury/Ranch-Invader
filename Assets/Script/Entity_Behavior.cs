using UnityEngine;

public class Entity_Behavior : MonoBehaviour
{

    [Header("References")]
    public Transform player;
    public SpriteRenderer spriteRenderer;

    [Header("Animation Settings")]
    public float updateRate = 0.1f; // temps entre chaque changement de sprite

    private float timer = 0f;


    [Header("Sprites")]
    public Sprite front;
    public Sprite frontRight;
    public Sprite right;
    public Sprite backRight;
    public Sprite back;
    public Sprite backLeft;
    public Sprite left;
    public Sprite frontLeft;

    void LateUpdate()
    {
        if (player == null) return;

        timer += Time.deltaTime;
        UpdateSpriteDirection();

        if (timer >= updateRate)
        {
            FaceCamera();
            timer = 0f;
        }

        
    }

    void UpdateSpriteDirection()
    {
        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f;
        toPlayer.Normalize();

        float angle = Mathf.Atan2(toPlayer.x, toPlayer.z) * Mathf.Rad2Deg;
        angle = (angle + 360f) % 360f;

        spriteRenderer.sprite = GetSpriteFromAngle(angle);
    }

    Sprite GetSpriteFromAngle(float angle)
    {
        if (angle < 22.5f || angle >= 337.5f)
            return front;
        else if (angle < 67.5f)
            return frontRight;
        else if (angle < 112.5f)
            return right;
        else if (angle < 157.5f)
            return backRight;
        else if (angle < 202.5f)
            return back;
        else if (angle < 247.5f)
            return backLeft;
        else if (angle < 292.5f)
            return left;
        else
            return frontLeft;
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
