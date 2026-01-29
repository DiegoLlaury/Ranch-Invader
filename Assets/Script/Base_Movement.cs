using UnityEngine;

public class Base_Movement : MonoBehaviour
{

    [SerializeField]
    float speed = 1.0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector2 pos = transform.position;

        pos.x += h * speed * Time.deltaTime;
        pos.y += v * speed * Time.deltaTime;

        transform.position = pos;
    }
}
