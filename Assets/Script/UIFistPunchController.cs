using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIFistPunchController : MonoBehaviour
{
    [Header("Parents")]
    public RectTransform leftRoot;
    public RectTransform rightRoot;

    [Header("Sprites")]
    public Image leftIdle;
    public Image leftPunch;
    public Image rightIdle;
    public Image rightPunch;

    [Header("Punch Settings")]
    public float punchDistance = 120f;
    public float punchDuration = 0.12f;
    public float returnDuration = 0.15f;
    public float rotateAngle = 6f;

    [Header("Target Center")]
    public RectTransform targetCenter;

    bool useLeft = true;
    bool isPunching = false;

    Vector2 leftIdlePos;
    Vector2 rightIdlePos;

    void Start()
    {
        leftIdlePos = leftRoot.anchoredPosition;
        rightIdlePos = rightRoot.anchoredPosition;

        // État initial
        SetLeftIdle();
        SetRightIdle();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1) && !isPunching)
        {
            StartCoroutine(Punch());
        }
    }

    IEnumerator Punch()
    {
        isPunching = true;

        RectTransform root = useLeft ? leftRoot : rightRoot;
        Image idle = useLeft ? leftIdle : rightIdle;
        Image punch = useLeft ? leftPunch : rightPunch;
        Vector2 idlePos = useLeft ? leftIdlePos : rightIdlePos;

        // Calculer direction vers le centre
        Vector2 centerPos = targetCenter != null ? targetCenter.anchoredPosition : Vector2.zero;
        Vector2 direction = (centerPos - idlePos).normalized;

        // Inverser horizontalement si c'est le bras droit
        if (!useLeft)
        {
            direction.x = -direction.x;
        }

        // Déplacement du poing
        Vector2 punchPos = idlePos + direction * punchDistance;

        // SWAP SPRITES
        idle.enabled = false;
        punch.enabled = true;

        // AVANCE
        yield return MoveRect(root, idlePos, punchPos, punchDuration, root);

        // RETOUR
        yield return MoveRect(root, punchPos, idlePos, returnDuration, root);

        // RETOUR À IDLE
        punch.enabled = false;
        idle.enabled = true;

        useLeft = !useLeft;
        isPunching = false;
    }

    IEnumerator MoveRect(RectTransform rect, Vector2 from, Vector2 to, float duration, RectTransform rectTransform)
    {
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float lerp = t / duration;
            lerp = Mathf.Sin(lerp * Mathf.PI * 0.5f);

            rectTransform.localRotation = Quaternion.Euler(0, 0, useLeft ? -(rotateAngle) : rotateAngle);
            //rect.localScale = Vector3.one * 1.05f;
            rect.anchoredPosition = Vector2.Lerp(from, to, lerp);
            yield return null;
        }

        rectTransform.localRotation = Quaternion.identity;
        rect.anchoredPosition = to;
    }

    void SetLeftIdle()
    {
        leftIdle.enabled = true;
        leftPunch.enabled = false;
    }

    void SetRightIdle()
    {
        rightIdle.enabled = true;
        rightPunch.enabled = false;
    }
}
