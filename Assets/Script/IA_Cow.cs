using UnityEngine;
using System.Collections;

public class RandomSpriteAI : MonoBehaviour
{
    [Header("Mouvement")]
    public float moveSpeed = 2f;            // Vitesse de déplacement
    public Vector2 zoneMin;                 // Limite bas-gauche de la zone
    public Vector2 zoneMax;                 // Limite haut-droite de la zone
    public float waitTime = 2f;             // Temps d’attente entre les déplacements

    

    private SpriteRenderer sr;
    private Vector3 targetPos;
    private bool isMoving = false;



    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        StartCoroutine(BehaviourLoop());
        OnDrawGizmos();
    }

    IEnumerator BehaviourLoop()
    {
        while (true)
        {
            // Choisir une position aléatoire dans la zone
            targetPos = new Vector3(
                Random.Range(zoneMin.x, zoneMax.x),
                transform.position.y, // on garde la même hauteur
                Random.Range(zoneMin.y, zoneMax.y)
            );

            // Bouger vers la cible
            isMoving = true;


            while (Vector3.Distance(transform.position, targetPos) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
                yield return null;
            }

            // Arrêt
            isMoving = false;
   

            // Attente avant prochain mouvement
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
