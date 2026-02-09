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
    public int maxAmmo = 2;
    public int currentAmmo = 2;
    public float reloadTime = 1.5f;

    [Header("Durabilité (Pelle)")]
    public float maxDurability = 100f;
    public float currentDurability = 100f;
    public float durabilityLossPerHit = 10f;

    [Header("Sprites UI")]
    public Sprite idleSprite;
    public Sprite attackSprite;
    public Sprite weaponIconSprite;

    [Header("Animation")]
    public string attackAnimationTrigger = "Attack";
    public float animationDuration = 0.3f;

    [Header("Effets visuels")]
    public GameObject hitEffectPrefab;
    public GameObject muzzleFlashPrefab;
}

