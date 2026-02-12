using UnityEngine;
using System.Collections;

public class RandomMovementAI : MonoBehaviour
{
    [Header("Mouvement")]
    public float moveSpeed = 2f;
    public float rotationSpeed = 360f;

    [Header("Zone de mouvement")]
    [Tooltip("Centre de la zone de mouvement")]
    public Vector3 zoneCenter = Vector3.zero;

    [Tooltip("Taille de la zone de mouvement (longueur, largeur)")]
    public Vector2 zoneSize = new Vector2(20f, 20f);

    [Header("Comportement")]
    public float waitTimeMin = 1f;
    public float waitTimeMax = 3f;

    [Range(1, 20)]
    [Tooltip("Nombre de tentatives pour trouver une position valide")]
    public int maxAttempts = 10;

    [Header("Détection d'obstacles")]
    public LayerMask obstacleLayer;

    [Tooltip("Distance de détection des obstacles devant l'entité")]
    public float obstacleDetectionDistance = 1f;

    [Tooltip("Rayon pour vérifier si une position est libre")]
    public float validationRadius = 0.5f;

    [Header("Debug")]
    public bool showDebugGizmos = true;
    public Color zoneColor = new Color(0f, 1f, 0f, 0.3f);
    public Color targetColor = Color.red;

    private Vector3 targetPos;
    private bool isMoving = false;
    private Coroutine movementCoroutine;

    public Vector3 FacingDirection { get; private set; }
    public bool IsMoving => isMoving;

    private void Start()
    {
        if (zoneCenter == Vector3.zero)
        {
            zoneCenter = transform.position;
        }

        StartCoroutine(BehaviourLoop());
    }

    private IEnumerator BehaviourLoop()
    {
        while (true)
        {
            Vector3 newTarget = GetValidRandomPosition();

            if (newTarget != Vector3.zero)
            {
                targetPos = newTarget;
                yield return StartCoroutine(MoveToTarget());
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] Could not find valid position after {maxAttempts} attempts");
            }

            isMoving = false;

            float waitTime = Random.Range(waitTimeMin, waitTimeMax);
            yield return new WaitForSeconds(waitTime);
        }
    }

    private Vector3 GetValidRandomPosition()
    {
        for (int i = 0; i < maxAttempts; i++)
        {
            float randomX = Random.Range(-zoneSize.x / 2f, zoneSize.x / 2f);
            float randomZ = Random.Range(-zoneSize.y / 2f, zoneSize.y / 2f);

            Vector3 randomPos = zoneCenter + new Vector3(randomX, 0f, randomZ);
            randomPos.y = transform.position.y;

            if (IsPositionValid(randomPos))
            {
                return randomPos;
            }
        }

        return Vector3.zero;
    }

    private bool IsPositionValid(Vector3 position)
    {
        Collider[] colliders = Physics.OverlapSphere(position, validationRadius, obstacleLayer);

        foreach (Collider col in colliders)
        {
            if (col.gameObject != gameObject)
            {
                return false;
            }
        }

        return true;
    }

    private IEnumerator MoveToTarget()
    {
        isMoving = true;

        Vector3 startPos = transform.position;
        Vector3 moveDir = (targetPos - transform.position);
        moveDir.y = 0f;

        if (moveDir.magnitude < 0.1f)
        {
            isMoving = false;
            yield break;
        }

        moveDir.Normalize();
        FacingDirection = moveDir;

        Quaternion targetRotation = Quaternion.LookRotation(moveDir);

        while (Quaternion.Angle(transform.rotation, targetRotation) > 1f)
        {
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
            yield return null;
        }

        transform.rotation = targetRotation;

        while (Vector3.Distance(transform.position, targetPos) > 0.2f)
        {
            if (DetectObstacleAhead())
            {
                Debug.Log($"[{gameObject.name}] Obstacle detected, stopping movement");
                break;
            }

            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPos,
                moveSpeed * Time.deltaTime
            );

            yield return null;
        }

        isMoving = false;
    }

    private bool DetectObstacleAhead()
    {
        Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;

        if (Physics.Raycast(rayOrigin, FacingDirection, out RaycastHit hit, obstacleDetectionDistance, obstacleLayer))
        {
            if (hit.collider.gameObject != gameObject)
            {
                return true;
            }
        }

        return false;
    }

    public void SetMovementZone(Vector3 center, Vector2 size)
    {
        zoneCenter = center;
        zoneSize = size;
    }

    private void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;

        Vector3 center = Application.isPlaying ? zoneCenter : (zoneCenter == Vector3.zero ? transform.position : zoneCenter);

        Gizmos.color = zoneColor;
        Vector3 size3D = new Vector3(zoneSize.x, 0.1f, zoneSize.y);
        Gizmos.DrawCube(center, size3D);

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(center, size3D);

        if (Application.isPlaying && isMoving)
        {
            Gizmos.color = targetColor;
            Gizmos.DrawSphere(targetPos, 0.3f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, targetPos);
        }

        if (Application.isPlaying && FacingDirection != Vector3.zero)
        {
            Gizmos.color = Color.blue;
            Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;
            Gizmos.DrawRay(rayOrigin, FacingDirection * obstacleDetectionDistance);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos) return;

        Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, validationRadius);

        Vector3 center = Application.isPlaying ? zoneCenter : (zoneCenter == Vector3.zero ? transform.position : zoneCenter);

        Gizmos.color = Color.white;
        Vector3 bottomLeft = center + new Vector3(-zoneSize.x / 2f, 0, -zoneSize.y / 2f);
        Vector3 bottomRight = center + new Vector3(zoneSize.x / 2f, 0, -zoneSize.y / 2f);
        Vector3 topRight = center + new Vector3(zoneSize.x / 2f, 0, zoneSize.y / 2f);
        Vector3 topLeft = center + new Vector3(-zoneSize.x / 2f, 0, zoneSize.y / 2f);

        Gizmos.DrawLine(bottomLeft, bottomRight);
        Gizmos.DrawLine(bottomRight, topRight);
        Gizmos.DrawLine(topRight, topLeft);
        Gizmos.DrawLine(topLeft, bottomLeft);
    }
}
