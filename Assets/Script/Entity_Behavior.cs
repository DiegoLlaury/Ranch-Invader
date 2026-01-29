using UnityEngine;

public class Entity_Behavior : MonoBehaviour
{
    public Transform player;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null) return;

        transform.LookAt(player.position);

        transform.rotation = Quaternion.LookRotation(transform.position - player.position);
    }
}
