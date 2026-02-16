using UnityEngine;

public class BeerPickup : MonoBehaviour
{
    [Header("Propriétés de la Bière")]
    [SerializeField] private float healthRestore = 20f;
    [SerializeField] private float damageBoost = 10f;
    [SerializeField] private float drunkDuration = 10f;

    [Header("Effets")]
    [SerializeField] private AudioClip pickupSound;

    private bool hasBeenPickedUp = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasBeenPickedUp) return;

        if (other.CompareTag("Player"))
        {
            PickupBeer(other.gameObject);
        }

        CharacterController controller = other.GetComponent<CharacterController>();
        if (controller != null && controller.CompareTag("Player"))
        {
            PickupBeer(controller.gameObject);
        }
    }

    private void PickupBeer(GameObject player)
    {
        SoundManager.Instance.PlaySound2D("BeerCan_Open");

        if (hasBeenPickedUp) return;
        hasBeenPickedUp = true;

        PlayerHealth health = player.GetComponent<PlayerHealth>();
        DrunkEffect drunkEffect = player.GetComponent<DrunkEffect>();

        if (health != null)
        {
            health.Heal(healthRestore);
            Debug.Log($"Bière ramassée ! Vie restaurée : +{healthRestore}");
        }
        else
        {
            Debug.LogWarning("PlayerHealth non trouvé sur le joueur !");
        }

        if (drunkEffect != null)
        {
            drunkEffect.ApplyDrunkEffect(drunkDuration, damageBoost);
            Debug.Log($"Effet bourré activé ! Durée : {drunkDuration}s, Bonus dégâts : +{damageBoost}");
        }
        else
        {
            Debug.LogWarning("DrunkEffect non trouvé sur le joueur !");
        }

        Destroy(gameObject);
    }
}
