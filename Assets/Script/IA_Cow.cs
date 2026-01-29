using UnityEngine;
using System.Collections;

public class RandomSpriteAI : MonoBehaviour
{
    [Header("Mouvement")]
    public float moveSpeed = 2f;            // Vitesse de déplacement
    public Vector2 zoneMin;                 // Limite bas-gauche de la zone
    public Vector2 zoneMax;                 // Limite haut-droite de la zone
    public float waitTime = 2f;             // Temps d’attente entre les déplacements

    [Header("Animation")]
    public Sprite[] walkSprites;            // Tes sprites de marche (2-3 images)
    public Sprite idleSprite;               // Sprite quand il ne bouge pas
    public float frameRate = 0.2f;          // Temps entre frames

    private SpriteRenderer sr;
    private Vector3 targetPos;
    private bool isMoving = false;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        StartCoroutine(BehaviourLoop());
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
            StartCoroutine(PlayWalkAnimation());

            while (Vector3.Distance(transform.position, targetPos) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
                yield return null;
            }

            // Arrêt
            isMoving = false;
            sr.sprite = idleSprite;

            // Attente avant prochain mouvement
            yield return new WaitForSeconds(waitTime);
        }
    }

    IEnumerator PlayWalkAnimation()
    {
        int index = 0;
        while (isMoving)
        {
            sr.sprite = walkSprites[index];
            index = (index + 1) % walkSprites.Length;
            yield return new WaitForSeconds(frameRate);
        }
    }
}
