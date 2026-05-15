using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Weapons/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Informations générales")]
    public string weaponName;
    public WeaponType weaponType;

    [Header("Statistiques")]
    public float damage = 10f;
    public float range = 2f;
    public float attackCooldown = 0.5f;

    [Header("Munitions (Fusil/Fourche)")]
    [Tooltip("Capacité maximale de départ — ne pas modifier en runtime")]
    public int maxAmmo = 2;
    [Tooltip("Munitions en chargeur au démarrage — ne pas modifier en runtime")]
    public int currentAmmo = 2;
    public float reloadTime = 1.5f;
    public int ammoPerReload = 2;

    [Header("Durabilité (Pelle)")]
    public float maxDurability = 100f;
    public float currentDurability = 100f;
    public float durabilityLossPerHit = 10f;

    [Header("Sprites UI (Legacy - gardé pour compatibilité)")]
    public Sprite idleSprite;
    public Sprite attackSprite;
    public Sprite weaponIconSprite;

    [Header("Animation Multi-Frame")]
    [Tooltip("Animation jouée lors de l'attaque")]
    public WeaponAnimationFrame[] attackAnimation;

    [Tooltip("Animation jouée lors du rechargement (fusil, fourche)")]
    public WeaponAnimationFrame[] reloadAnimation;

    [Header("Animation (Legacy)")]
    public string attackAnimationTrigger = "Attack";
    public float animationDuration = 0.3f;

    [Header("Effets visuels")]
    public GameObject hitEffectPrefab;
    public GameObject muzzleFlashPrefab;

    // ── Valeurs runtime (non sérialisées — réinitialisées à chaque session) ────
    [System.NonSerialized] private int runtimeMaxAmmo;
    [System.NonSerialized] private int runtimeCurrentAmmo;
    [System.NonSerialized] private bool runtimeInitialized;

    /// <summary>
    /// Initialise les valeurs runtime à partir des valeurs de l'asset.
    /// Doit être appelé dans BaseWeapon.Awake à chaque démarrage de session.
    /// </summary>
    public void InitializeRuntimeAmmo()
    {
        runtimeMaxAmmo = maxAmmo;
        runtimeCurrentAmmo = currentAmmo;
        runtimeInitialized = true;
    }

    /// <summary>Munitions actuelles dans le chargeur (runtime).</summary>
    public int RuntimeCurrentAmmo
    {
        get => runtimeInitialized ? runtimeCurrentAmmo : currentAmmo;
        set { if (runtimeInitialized) runtimeCurrentAmmo = value; }
    }

    /// <summary>Capacité maximale courante, incluant la réserve fusil (runtime).</summary>
    public int RuntimeMaxAmmo
    {
        get => runtimeInitialized ? runtimeMaxAmmo : maxAmmo;
        set { if (runtimeInitialized) runtimeMaxAmmo = value; }
    }

    /// <summary>Remet les valeurs runtime à celles de l'asset (pickup de l'arme).</summary>
    public void ResetRuntimeAmmo()
    {
        runtimeMaxAmmo = maxAmmo;
        runtimeCurrentAmmo = currentAmmo;
    }

    /// <summary>
    /// Ajoute des munitions en pourcentage de la capacité initiale totale,
    /// arrondi au supérieur. Pour le fusil, n'augmente que RuntimeMaxAmmo (réserve).
    /// Retourne le nombre de munitions effectivement ajoutées.
    /// </summary>
    public int AddAmmoByPercent(float percent, bool affectReserveOnly)
    {
        int toAdd = Mathf.CeilToInt(maxAmmo * percent);

        if (affectReserveOnly)
        {
            int maxReserve = maxAmmo - runtimeCurrentAmmo;
            int effectiveAdd = Mathf.Min(toAdd, maxReserve);
            runtimeMaxAmmo += effectiveAdd;
            return effectiveAdd;
        }
        else
        {
            int effectiveAdd = Mathf.Min(toAdd, maxAmmo - runtimeCurrentAmmo);
            runtimeCurrentAmmo += effectiveAdd;
            return effectiveAdd;
        }
    }
}
