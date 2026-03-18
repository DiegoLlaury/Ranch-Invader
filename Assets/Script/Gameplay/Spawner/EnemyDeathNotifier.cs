using UnityEngine;

/// <summary>
/// Composant ajouté dynamiquement par EnemySpawner sur chaque ennemi spawné.
/// Notifie le spawner quand l'ennemi est détruit.
/// </summary>
public class EnemyDeathNotifier : MonoBehaviour
{
    private EnemySpawner spawner;

    /// <summary>Initialise le notifier avec le spawner parent.</summary>
    public void Initialize(EnemySpawner enemySpawner)
    {
        spawner = enemySpawner;
    }

    private void OnDestroy()
    {
        // Ne notifie pas si c'est un unload de scène
        if (spawner != null && gameObject.scene.isLoaded)
            spawner.OnEnemyDied(gameObject);
    }
}
