using UnityEngine;

/// <summary>
/// Composant ajout� dynamiquement par EnemySpawner sur chaque ennemi spawn�.
/// Notifie le spawner quand l'ennemi est d�truit.
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
        // Ne notifie pas si c'est un unload de sc�ne
        if (spawner != null && gameObject.scene.isLoaded)
        {
            spawner.OnEnemyDied(gameObject);
            VoiceManager.Instance?.PlayVoice("Voice_Combat", VoicePriority.Normal);
        }
    }
}
