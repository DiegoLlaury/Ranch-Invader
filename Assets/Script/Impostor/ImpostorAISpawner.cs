using UnityEngine;

public class ImpostorAISpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject entityAIPrefab; // Prefab avec RandomMovementAI + ImpostorEntityAI
    public GameObject impostorQuadPrefab;
    public GameObject meshPrefab;
    public Material impostorMaterial;

    [Header("Spawn Settings")]
    public int count = 10;
    public Vector3 zoneCenter;
    public Vector2 zoneSize = new Vector2(50f, 50f);
    public bool useSpawnerAsZoneCenter = true;

    [Header("AI Settings")]
    public float moveSpeed = 2f;
    public float waitTimeMin = 1f;
    public float waitTimeMax = 3f;
    public LayerMask obstacleLayer;

    [Header("Impostor Settings")]
    [Range(1, 60)]
    public int animatedFPS = 15;
    public float captureScale = 1f;
    public Vector3 meshRotationOffset = Vector3.zero;

    void Start()
    {
        if (useSpawnerAsZoneCenter)
        {
            zoneCenter = transform.position;
        }

        SpawnEntities();
    }

    void SpawnEntities()
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 randomPos = GetRandomPositionInZone();
            SpawnEntity(randomPos);
        }
    }

    GameObject SpawnEntity(Vector3 position)
    {
        GameObject entity = Instantiate(entityAIPrefab, position, Quaternion.identity);
        entity.name = $"{entityAIPrefab.name}_{Random.Range(1000, 9999)}";

        // Configurer RandomMovementAI
        RandomMovementAI ai = entity.GetComponent<RandomMovementAI>();
        if (ai != null)
        {
            ai.moveSpeed = moveSpeed;
            ai.waitTimeMin = waitTimeMin;
            ai.waitTimeMax = waitTimeMax;
            ai.obstacleLayer = obstacleLayer;
            ai.SetMovementZone(zoneCenter, zoneSize);
        }

        // Configurer ImpostorEntityAI
        ImpostorEntityAI impostorAI = entity.GetComponent<ImpostorEntityAI>();
        if (impostorAI != null)
        {
            impostorAI.meshPrefab = meshPrefab;
            impostorAI.impostorMaterial = impostorMaterial;
            impostorAI.impostorQuadPrefab = impostorQuadPrefab;
            impostorAI.animatedFPS = animatedFPS;
            impostorAI.captureScale = captureScale;
            impostorAI.meshRotationOffset = meshRotationOffset;
        }

        return entity;
    }

    Vector3 GetRandomPositionInZone()
    {
        float randomX = Random.Range(-zoneSize.x / 2f, zoneSize.x / 2f);
        float randomZ = Random.Range(-zoneSize.y / 2f, zoneSize.y / 2f);

        Vector3 randomPos = zoneCenter + new Vector3(randomX, 0f, randomZ);
        randomPos.y = transform.position.y;

        return randomPos;
    }

    void OnDrawGizmosSelected()
    {
        Vector3 center = useSpawnerAsZoneCenter ? transform.position : zoneCenter;

        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawCube(center, new Vector3(zoneSize.x, 0.1f, zoneSize.y));

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(center, new Vector3(zoneSize.x, 0.1f, zoneSize.y));
    }
}
