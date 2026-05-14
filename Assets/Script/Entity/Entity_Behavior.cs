using UnityEngine;

public class Entity_Behavior : MonoBehaviour
{
    [Header("References (Auto-assigned if empty)")]
    public Transform player;
    public SpriteRenderer spriteRenderer;
    public RandomMovementAI ai;

    [Header("Animation Settings")]
    public float updateRate = 0.1f;

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
    public float stretchAmount = 0.15f;
    public float stretchSpeed = 8f;
    private Vector3 baseScale;

    private Vector3 lastPosition;
    private Vector3 movementDirection;

    private void Start()
    {
        FindReferences();

        if (spriteRenderer != null)
        {
            baseScale = spriteRenderer.transform.localScale;
        }

        lastPosition = transform.position;
    }

    private void FindReferences()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (ai == null)
        {
            ai = GetComponent<RandomMovementAI>();
        }
    }

    void LateUpdate()
    {
        if (player == null || spriteRenderer == null) return;

        FaceCamera();
        UpdateMovementDirection();

        timer += Time.deltaTime;
        if (timer >= updateRate)
        {
            ApplyMovementStretch();
            UpdateSpriteDirection();
            timer = 0f;
        }
    }

    void UpdateMovementDirection()
    {
        Vector3 currentPosition = transform.position;
        Vector3 delta = currentPosition - lastPosition;

        if (delta.sqrMagnitude > 0.0001f)
        {
            movementDirection = delta.normalized;
        }

        lastPosition = currentPosition;
    }

    void UpdateSpriteDirection()
    {
        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f;
        toPlayer.Normalize();

        Vector3 moveDir = GetFacingDirection();
        moveDir.y = 0f;

        if (moveDir.sqrMagnitude < 0.0001f)
        {
            return;
        }

        moveDir.Normalize();

        float angle = Vector3.SignedAngle(moveDir, toPlayer, Vector3.up);
        angle = (angle + 360f) % 360f;

        spriteRenderer.sprite = GetSpriteFromAngle(angle);
    }

    Vector3 GetFacingDirection()
    {
        if (ai != null)
        {
            return ai.FacingDirection;
        }
        else
        {
            return movementDirection;
        }
    }

    bool IsMoving()
    {
        if (ai != null)
        {
            return ai.IsMoving;
        }
        else
        {
            return movementDirection.sqrMagnitude > 0.0001f;
        }
    }

    Sprite GetSpriteFromAngle(float angle)
    {
        if (angle < 30f || angle >= 330f)
            return front;
        else if (angle < 60f)
            return frontRight;
        else if (angle < 120f)
            return right;
        else if (angle < 150f)
            return backRight;
        else if (angle < 210f)
            return back;
        else if (angle < 240f)
            return backLeft;
        else if (angle < 300f)
            return left;
        else
            return frontLeft;
    }

    void ApplyMovementStretch()
    {
        if (IsMoving())
        {
            float t = Time.time * stretchSpeed;
            float stretch = Mathf.Sin(t) * stretchAmount;

            spriteRenderer.transform.localScale = new Vector3(
                baseScale.x - stretch,
                baseScale.y + stretch,
                baseScale.z
            );
        }
        else
        {
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

        Quaternion targetRotation = Quaternion.LookRotation(dir);
        spriteRenderer.transform.rotation = Quaternion.Euler(0f, targetRotation.eulerAngles.y, 0f);
    }
}
