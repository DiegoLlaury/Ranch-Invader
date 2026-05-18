using UnityEngine;

public class BeerPickup : MonoBehaviour
{
    [Header("Propri�t�s de la Bi�re")]
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
        SoundManager.Instance.PlaySound2D("Beer_Drink");
        VoiceManager.Instance?.PlayVoice("Voice_DrinkBeer", VoicePriority.Normal);

        if (hasBeenPickedUp) return;
        hasBeenPickedUp = true;

        PlayerHealth health = player.GetComponent<PlayerHealth>();
        DrunkEffect drunkEffect = player.GetComponent<DrunkEffect>();

        if (health != null)
        {
            health.Heal(healthRestore);
            Debug.Log($"Bi�re ramass�e ! Vie restaur�e : +{healthRestore}");
        }
        else
        {
            Debug.LogWarning("PlayerHealth non trouv� sur le joueur !");
        }

        if (drunkEffect != null)
        {
            drunkEffect.ApplyDrunkEffect(drunkDuration, damageBoost);
            Debug.Log($"Effet bourr� activ� ! Dur�e : {drunkDuration}s, Bonus d�g�ts : +{damageBoost}");
        }
        else
        {
            Debug.LogWarning("DrunkEffect non trouv� sur le joueur !");
        }

        Destroy(gameObject);
    }
}
