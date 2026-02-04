using UnityEngine;
using System.Collections;

public class RandomMovementAI : MonoBehaviour
{
    [Header("Mouvement")]
    public float moveSpeed = 2f;
    public float rotationSpeed = 360f; // degrés par seconde
    public Vector2 zoneMin;
    public Vector2 zoneMax;
    public float waitTime = 2f;          // Temps d’attente entre les déplacements



    private SpriteRenderer sr;
    private Vector3 targetPos;
    public bool isMoving = false;

    public Vector3 FacingDirection { get; private set; }
    public Transform player;



    void Start()
    {
        //sr = GetComponent<SpriteRenderer>();
        StartCoroutine(BehaviourLoop());
    }

    IEnumerator BehaviourLoop()
    {
        while (true)
        {
            // Position aléatoire dans la zone
            targetPos = new Vector3(
                Random.Range(zoneMin.x, zoneMax.x),
                transform.position.y,
                Random.Range(zoneMin.y, zoneMax.y)
            );

            isMoving = true;

            // Direction vers la cible
            Vector3 moveDir = (targetPos - transform.position);
            moveDir.y = 0f;
            moveDir.Normalize();
            FacingDirection = moveDir;

            transform.rotation = Quaternion.LookRotation(moveDir);

            // Déplacement
            while (Vector3.Distance(transform.position, targetPos) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime); ;
                yield return null;
            }

            isMoving = false;

            yield return new WaitForSeconds(waitTime);
        }
    }

    void OnDrawGizmos()
    {
        // Quatre coins
        Vector3 bottomLeft = new Vector3(zoneMin.x, zoneMin.y, 0f);
        Vector3 bottomRight = new Vector3(zoneMax.x, zoneMin.y, 0f);
        Vector3 topRight = new Vector3(zoneMax.x, zoneMax.y, 0f);
        Vector3 topLeft = new Vector3(zoneMin.x, zoneMax.y, 0f);

        // Tracer le rectangle
        Debug.DrawLine(bottomLeft, bottomRight, Color.green);
        Debug.DrawLine(bottomRight, topRight, Color.green);
        Debug.DrawLine(topRight, topLeft, Color.green);
        Debug.DrawLine(topLeft, bottomLeft, Color.green);
    }

}
