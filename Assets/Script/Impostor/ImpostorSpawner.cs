using UnityEngine;

public class ImpostorSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject impostorQuadPrefab;
    public GameObject meshPrefab;
    public Material impostorMaterial;

    [Header("Spawn Settings")]
    public bool isAnimated = false;
    public int count = 10;
    public float spawnRadius = 20f;
    public Transform playerTransform;

    void Start()
    {
        if (playerTransform == null)
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        }

        SpawnImpostors();
    }

    public void SpawnImpostors()
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 randomPos = transform.position + Random.insideUnitSphere * spawnRadius;
            randomPos.y = transform.position.y;

            SpawnImpostor(randomPos, Quaternion.identity);
        }
    }

    public GameObject SpawnImpostor(Vector3 position, Quaternion rotation)
    {
        GameObject quad = Instantiate(impostorQuadPrefab, position, rotation);
        quad.name = $"Impostor_{meshPrefab.name}_{Random.Range(1000, 9999)}";

        ImpostorEntity entity = quad.AddComponent<ImpostorEntity>();
        entity.meshPrefab = meshPrefab;
        entity.impostorMaterial = impostorMaterial;
        entity.playerTransform = playerTransform;
        entity.isAnimated = isAnimated;

        Billboard billboard = quad.GetComponent<Billboard>();
        if (billboard == null)
        {
            billboard = quad.AddComponent<Billboard>();
        }

        return quad;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}
