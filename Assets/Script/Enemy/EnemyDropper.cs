using UnityEngine;

/// <summary>
/// Handles loot drops when any enemy dies.
/// Attach this component to a single persistent GameObject in the scene (e.g., GameManager).
/// All drop chances are public for easy balancing in the Inspector.
/// </summary>
public class EnemyDropper : MonoBehaviour
{
    [Header("Bi�re")]
    [Tooltip("Probabilit� de faire tomber une bi�re � la mort d'un ennemi (0 = jamais, 1 = toujours)")]
    [Range(0f, 1f)]
    public float beerDropChance = 0.2f;

    [Tooltip("Prefab de la bi�re � instancier")]
    public GameObject beerPrefab;

    [Header("Munitions")]
    [Tooltip("Probabilit� de faire tomber des munitions � la mort d'un ennemi (0 = jamais, 1 = toujours)")]
    [Range(0f, 1f)]
    public float ammoDropChance = 0.3f;

    [Tooltip("Prefab de munitions � instancier")]
    public GameObject ammoPrefab;

    [Header("Drop Settings")]
    [Tooltip("D�calage vertical pour que le pickup apparaisse l�g�rement au-dessus du sol")]
    public float dropHeightOffset = 0.5f;

    [Tooltip("Rayon de dispersion al�atoire autour de la position de mort")]
    [Range(0f, 3f)]
    public float scatterRadius = 0.5f;

    [Header("R�f�rences")]
    [Tooltip("L'inventaire du joueur � utilis� pour conditionner le drop de munitions")]
    public WeaponInventory playerInventory;

    private static readonly WeaponType[] AmmoWeapons = { WeaponType.Shotgun, WeaponType.Pitchfork };

    private void OnEnable()
    {
        EnemyBase.OnEnemyDied += HandleEnemyDied;
    }

    private void OnDisable()
    {
        EnemyBase.OnEnemyDied -= HandleEnemyDied;
    }

    private void HandleEnemyDied(Vector3 deathPosition)
    {
        Vector3 dropPosition = deathPosition + Vector3.up * dropHeightOffset;

        // Roll a single drop : bière OU munitions, jamais les deux en même temps.
        // On tire d'abord la bière. Si elle échoue et que le joueur a une arme à munitions,
        // on tente les munitions à la place.
        float roll = Random.value;

        if (roll < beerDropChance)
        {
            SpawnDrop(beerPrefab, dropPosition);
        }
        else if (PlayerHasAmmoWeapon())
        {
            // On recalcule la probabilité sur la portion restante pour conserver
            // la distribution voulue indépendamment de l'ordre des checks.
            float remainingRoll = Random.value;
            if (remainingRoll < ammoDropChance)
                SpawnDrop(ammoPrefab, dropPosition);
        }
    }

    /// <summary>
    /// Retourne true si le joueur poss�de au moins une arme consommant des munitions.
    /// </summary>
    private bool PlayerHasAmmoWeapon()
    {
        if (playerInventory == null) return false;

        foreach (WeaponType type in AmmoWeapons)
        {
            if (playerInventory.HasWeapon(type))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Instantiates the given prefab at a randomly scattered position around basePosition.
    /// </summary>
    private void SpawnDrop(GameObject prefab, Vector3 basePosition)
    {
        if (prefab == null) return;

        Vector2 scatter = Random.insideUnitCircle * scatterRadius;
        Vector3 spawnPosition = basePosition + new Vector3(scatter.x, 0f, scatter.y);

        Instantiate(prefab, spawnPosition, Quaternion.identity);
    }
}
