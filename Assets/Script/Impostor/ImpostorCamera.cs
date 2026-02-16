using UnityEngine;

public class ImpostorCamera : MonoBehaviour
{
    public Transform target;
    public float distance = 5f;
    public float height = 1.5f;

    private static readonly Vector3[] directions =
    {
        new Vector3( 0, 0,  1),           // 0 - North (0°)
        new Vector3( 1, 0,  1).normalized, // 1 - NorthEast (45°)
        new Vector3( 1, 0,  0),           // 2 - East (90°)
        new Vector3( 1, 0, -1).normalized, // 3 - SouthEast (135°)
        new Vector3( 0, 0, -1),           // 4 - South (180°)
        new Vector3(-1, 0, -1).normalized, // 5 - SouthWest (225°)
        new Vector3(-1, 0,  0),           // 6 - West (270°)
        new Vector3(-1, 0,  1).normalized  // 7 - NorthWest (315°)
    };

    public void SetDirection(int index)
    {
        if (index < 0 || index >= directions.Length)
        {
            Debug.LogError($"Index de direction invalide : {index}");
            return;
        }

        Vector3 dir = directions[index];

        Vector3 pos = target.position - dir * distance;
        pos.y = target.position.y + height;

        transform.position = pos;

        Vector3 lookTarget = target.position;
        lookTarget.y += height;

        transform.LookAt(lookTarget);
    }
}
