using UnityEngine;

public class UIIdleFloat : MonoBehaviour
{
    public RectTransform rectTransform;

    [Header("Position")]
    public float positionAmplitude = 5f;
    public float positionSpeed = 2f;

    [Header("Scale")]
    public float scaleAmplitude = 0.03f;
    public float scaleSpeed = 2f;

    Vector2 startPos;
    Vector3 startScale;

    public float updateRate = 0.1f;
    private float timer = 0f;

    void Start()
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        startPos = rectTransform.anchoredPosition;
        startScale = rectTransform.localScale;
    }

    void Update()
    {
        float t = Time.time;

        timer += Time.deltaTime;

        float yOffset = Mathf.Sin(t * positionSpeed) * positionAmplitude;
        float scaleOffset = Mathf.Sin(t * scaleSpeed) * scaleAmplitude;

        if (timer >= updateRate)
        {
            rectTransform.anchoredPosition = startPos + Vector2.up * yOffset;
            rectTransform.localScale = startScale + Vector3.one * scaleOffset;
            timer = 0f;
        }
        
    }
}
