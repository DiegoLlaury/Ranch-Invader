using UnityEngine;

/// <summary>
/// Handles loot drops when any enemy dies.
/// Attach this component to a single persistent GameObject in the scene (e.g., GameManager).
/// All drop chances are public for easy balancing in the Inspector.
/// </summary>
public class EnemyDropper : MonoBehaviour
{
    [Header("Bière")]
    [Tooltip("Probabilité de faire tomber une bière à la mort d'un ennemi (0 = jamais, 1 = toujours)")]
    [Range(0f, 1f)]
    public float beerDropChance = 0.2f;

    [Tooltip("Prefab de la bière à instancier")]
    public GameObject beerPrefab;

    [Header("Munitions")]
    [Tooltip("Probabilité de faire tomber des munitions à la mort d'un ennemi (0 = jamais, 1 = toujours)")]
    [Range(0f, 1f)]
    public float ammoDropChance = 0.3f;

    [Tooltip("Prefab de munitions à instancier")]
    public GameObject ammoPrefab;

    [Header("Drop Settings")]
    [Tooltip("Décalage vertical pour que le pickup apparaisse légèrement au-dessus du sol")]
    public float dropHeightOffset = 0.5f;

    [Tooltip("Rayon de dispersion aléatoire autour de la position de mort")]
    [Range(0f, 3f)]
    public float scatterRadius = 0.5f;

    [Header("Références")]
    [Tooltip("L'inventaire du joueur — utilisé pour conditionner le drop de munitions")]
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

        TryDrop(beerPrefab, beerDropChance, dropPosition);

        // Les munitions n'apparaissent que si le joueur possède au moins une arme à munitions
        if (PlayerHasAmmoWeapon())
            TryDrop(ammoPrefab, ammoDropChance, dropPosition);
    }

    /// <summary>
    /// Retourne true si le joueur possède au moins une arme consommant des munitions.
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
    /// Rolls against the given chance and instantiates the prefab at a randomly scattered position.
    /// </summary>
    private void TryDrop(GameObject prefab, float chance, Vector3 basePosition)
    {
        if (prefab == null) return;
        if (Random.value > chance) return;

        Vector2 scatter = Random.insideUnitCircle * scatterRadius;
        Vector3 spawnPosition = basePosition + new Vector3(scatter.x, 0f, scatter.y);

        Instantiate(prefab, spawnPosition, Quaternion.identity);
    }
}
