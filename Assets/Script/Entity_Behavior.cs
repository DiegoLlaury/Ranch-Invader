using UnityEngine;

public class Entity_Behavior : MonoBehaviour
{

    [Header("References")]
    public Transform player;
    public SpriteRenderer spriteRenderer;
    public RandomMovementAI ai;


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

    [Header("Movement Stretch")]
    public float stretchAmount = 0.15f;     // intensité du stretch
    public float stretchSpeed = 8f;          // vitesse de l'oscillation
    private Vector3 baseScale;

    private void Start()
    {
        baseScale = spriteRenderer.transform.localScale;
    }

    void LateUpdate()
    {
        if (player == null) return;

        FaceCamera();
        

        timer += Time.deltaTime;
        if (timer >= updateRate)
        {
            ApplyMovementStretch();
            UpdateSpriteDirection();
            timer = 0f;
        }

        
    }

    void UpdateSpriteDirection()
    {
        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f;
        toPlayer.Normalize();

        Vector3 moveDir = ai.FacingDirection;
        moveDir.y = 0f;
        moveDir.Normalize();

        float angle = Vector3.SignedAngle(moveDir, toPlayer, Vector3.up);
        angle = (angle + 360f) % 360f;

        spriteRenderer.sprite = GetSpriteFromAngle(angle);
    }

    Sprite GetSpriteFromAngle(float angle)
    {
        // FRONT (large)
        if (angle < 30f || angle >= 330f)
            return front;

        // FRONT RIGHT (réduit)
        else if (angle < 60f)
            return frontRight;

        // RIGHT (large)
        else if (angle < 120f)
            return right;

        // BACK RIGHT (réduit)
        else if (angle < 150f)
            return backRight;

        // BACK (large)
        else if (angle < 210f)
            return back;

        // BACK LEFT (réduit)
        else if (angle < 240f)
            return backLeft;

        // LEFT (large)
        else if (angle < 300f)
            return left;

        // FRONT LEFT (réduit)
        else
            return frontLeft;
    }

    void ApplyMovementStretch()
    {
        if (ai == null) return;

        if (ai.isMoving)
        {
            float t = Time.time * stretchSpeed;
            float stretch = Mathf.Sin(t) * stretchAmount;

            // Stretch vertical / squash horizontal
            spriteRenderer.transform.localScale = new Vector3(
                baseScale.x - stretch,
                baseScale.y + stretch,
                baseScale.z
            );
        }
        else
        {
            // Retour progressif à la taille normale
            spriteRenderer.transform.localScale = Vector3.Lerp(
                spriteRenderer.transform.localScale,
                baseScale,
                Time.deltaTime * 10f
            );
        }
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
