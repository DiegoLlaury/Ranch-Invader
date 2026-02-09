using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class WeaponUIController : MonoBehaviour
{
    [Header("Références")]
    public WeaponController weaponController;

    [Header("Affichage de l'arme")]
    public RectTransform weaponVisualRoot;
    public Image weaponSprite;
    public Animator weaponAnimator;

    [Header("Affichage des poings (alternance)")]
    public RectTransform leftFistRoot;
    public RectTransform rightFistRoot;
    public Image leftFistIdleSprite;
    public Image leftFistPunchSprite;
    public Image rightFistIdleSprite;
    public Image rightFistPunchSprite;

    [Header("Animation des poings")]
    public float fistPunchDistance = 120f;
    public float fistPunchDuration = 0.12f;
    public float fistReturnDuration = 0.15f;
    public float fistRotateAngle = 6f;
    public RectTransform fistTargetCenter;

    [Header("Munitions/Durabilité")]
    public GameObject ammoPanel;
    public TextMeshProUGUI ammoText;
    public GameObject durabilityPanel;
    public Slider durabilitySlider;

    [Header("Sélecteur d'armes")]
    public Image weapon1Slot;
    public Image weapon2Slot;
    public Image weapon3Slot;
    public Image weapon4Slot;
    public Color selectedColor = Color.white;
    public Color unselectedColor = Color.gray;

    [Header("Animation UI armes classiques")]
    public float attackMoveDistance = 50f;
    public float attackDuration = 0.2f;
    public float returnDuration = 0.25f;

    private Vector2 idlePosition;
    private Vector2 leftFistIdlePos;
    private Vector2 rightFistIdlePos;
    private bool isAnimating;
    private FistWeapon currentFistWeapon;

    private void Start()
    {
        if (weaponVisualRoot != null)
        {
            idlePosition = weaponVisualRoot.anchoredPosition;
        }

        if (leftFistRoot != null)
        {
            leftFistIdlePos = leftFistRoot.anchoredPosition;
        }

        if (rightFistRoot != null)
        {
            rightFistIdlePos = rightFistRoot.anchoredPosition;
        }

        SetupFistIdle();

        if (weaponController != null)
        {
            weaponController.OnWeaponChanged += HandleWeaponChanged;
            weaponController.OnAttackTriggered += HandleAttackTriggered;

            BaseWeapon currentWeapon = weaponController.GetCurrentWeapon();
            if (currentWeapon != null)
            {
                SubscribeToWeaponEvents(currentWeapon);
            }
        }

        UpdateWeaponDisplay(weaponController.GetCurrentWeaponType());
    }

    private void SetupFistIdle()
    {
        if (leftFistIdleSprite != null && leftFistPunchSprite != null)
        {
            leftFistIdleSprite.enabled = true;
            leftFistPunchSprite.enabled = false;
        }

        if (rightFistIdleSprite != null && rightFistPunchSprite != null)
        {
            rightFistIdleSprite.enabled = true;
            rightFistPunchSprite.enabled = false;
        }
    }

    private void HandleWeaponChanged(WeaponType newWeaponType)
    {
        BaseWeapon oldWeapon = weaponController.GetCurrentWeapon();
        if (oldWeapon != null)
        {
            UnsubscribeFromWeaponEvents(oldWeapon);
        }

        UpdateWeaponDisplay(newWeaponType);

        BaseWeapon newWeapon = weaponController.GetCurrentWeapon();
        if (newWeapon != null)
        {
            SubscribeToWeaponEvents(newWeapon);
        }

        currentFistWeapon = newWeapon as FistWeapon;
    }

    private void HandleAttackTriggered()
    {
        if (!isAnimating)
        {
            WeaponType currentType = weaponController.GetCurrentWeaponType();

            if (currentType == WeaponType.Fist)
            {
                StartCoroutine(PlayFistPunchAnimation());
            }
            else
            {
                StartCoroutine(PlayAttackAnimation());
            }
        }
    }

    private void UpdateWeaponDisplay(WeaponType weaponType)
    {
        WeaponData data = weaponController.GetWeaponData(weaponType);

        bool isFist = weaponType == WeaponType.Fist;

        if (weaponVisualRoot != null)
            weaponVisualRoot.gameObject.SetActive(!isFist);

        if (leftFistRoot != null)
            leftFistRoot.gameObject.SetActive(isFist);
        if (rightFistRoot != null)
            rightFistRoot.gameObject.SetActive(isFist);

        if (!isFist && data != null && weaponSprite != null)
        {
            weaponSprite.sprite = data.idleSprite;
        }

        UpdateWeaponSlots(weaponType);
        UpdateAmmoDisplay(weaponType, data);
        UpdateDurabilityDisplay(weaponType, data);
    }

    private void UpdateWeaponSlots(WeaponType currentType)
    {
        if (weapon1Slot != null)
            weapon1Slot.color = currentType == WeaponType.Fist ? selectedColor : unselectedColor;
        if (weapon2Slot != null)
            weapon2Slot.color = currentType == WeaponType.Shovel ? selectedColor : unselectedColor;
        if (weapon3Slot != null)
            weapon3Slot.color = currentType == WeaponType.Shotgun ? selectedColor : unselectedColor;
        if (weapon4Slot != null)
            weapon4Slot.color = currentType == WeaponType.Pitchfork ? selectedColor : unselectedColor;
    }

    private void UpdateAmmoDisplay(WeaponType weaponType, WeaponData data)
    {
        bool showAmmo = weaponType == WeaponType.Shotgun || weaponType == WeaponType.Pitchfork;

        if (ammoPanel != null)
            ammoPanel.SetActive(showAmmo);

        if (showAmmo && data != null && ammoText != null)
        {
            ammoText.text = $"{data.currentAmmo} / {data.maxAmmo}";
        }
    }

    private void UpdateDurabilityDisplay(WeaponType weaponType, WeaponData data)
    {
        bool showDurability = weaponType == WeaponType.Shovel;

        if (durabilityPanel != null)
        {
            durabilityPanel.SetActive(showDurability);
        }

        if (durabilitySlider != null)
        {
            durabilitySlider.gameObject.SetActive(showDurability);

            if (showDurability && data != null)
            {
                durabilitySlider.value = data.currentDurability / data.maxDurability;
            }
        }
    }

    private IEnumerator PlayFistPunchAnimation()
    {
        isAnimating = true;

        bool useLeft = currentFistWeapon != null ? currentFistWeapon.IsLeftFist() : true;

        RectTransform fistRoot = useLeft ? leftFistRoot : rightFistRoot;
        Image idleSprite = useLeft ? leftFistIdleSprite : rightFistIdleSprite;
        Image punchSprite = useLeft ? leftFistPunchSprite : rightFistPunchSprite;
        Vector2 fistIdlePos = useLeft ? leftFistIdlePos : rightFistIdlePos;

        if (fistRoot == null || idleSprite == null || punchSprite == null)
        {
            isAnimating = false;
            yield break;
        }

        Vector2 centerPos = fistTargetCenter != null ? fistTargetCenter.anchoredPosition : Vector2.zero;
        Vector2 direction = (centerPos - fistIdlePos).normalized;

        if (!useLeft)
        {
            direction.x = -direction.x;
        }

        Vector2 punchPos = fistIdlePos + direction * fistPunchDistance;

        idleSprite.enabled = false;
        punchSprite.enabled = true;

        yield return MoveFist(fistRoot, fistIdlePos, punchPos, fistPunchDuration, useLeft);

        yield return MoveFist(fistRoot, punchPos, fistIdlePos, fistReturnDuration, useLeft);

        punchSprite.enabled = false;
        idleSprite.enabled = true;

        isAnimating = false;
    }

    private IEnumerator MoveFist(RectTransform fist, Vector2 from, Vector2 to, float duration, bool isLeft)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            t = Mathf.Sin(t * Mathf.PI * 0.5f);

            if (fist != null)
            {
                fist.localRotation = Quaternion.Euler(0, 0, isLeft ? -fistRotateAngle : fistRotateAngle);
                fist.anchoredPosition = Vector2.Lerp(from, to, t);
            }

            yield return null;
        }

        if (fist != null)
        {
            fist.localRotation = Quaternion.identity;
            fist.anchoredPosition = to;
        }
    }

    private IEnumerator PlayAttackAnimation()
    {
        isAnimating = true;

        WeaponData currentData = weaponController.GetWeaponData(weaponController.GetCurrentWeaponType());

        if (currentData != null && currentData.attackSprite != null && weaponSprite != null)
        {
            weaponSprite.sprite = currentData.attackSprite;
        }

        if (weaponAnimator != null && currentData != null)
        {
            weaponAnimator.SetTrigger(currentData.attackAnimationTrigger);
        }

        Vector2 targetPosition = idlePosition + Vector2.up * attackMoveDistance;
        yield return MoveWeapon(idlePosition, targetPosition, attackDuration);

        yield return MoveWeapon(targetPosition, idlePosition, returnDuration);

        if (currentData != null && currentData.idleSprite != null && weaponSprite != null)
        {
            weaponSprite.sprite = currentData.idleSprite;
        }

        isAnimating = false;
    }

    private IEnumerator MoveWeapon(Vector2 from, Vector2 to, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            t = Mathf.Sin(t * Mathf.PI * 0.5f);

            if (weaponVisualRoot != null)
            {
                weaponVisualRoot.anchoredPosition = Vector2.Lerp(from, to, t);
            }

            yield return null;
        }

        if (weaponVisualRoot != null)
        {
            weaponVisualRoot.anchoredPosition = to;
        }
    }

    private void SubscribeToWeaponEvents(BaseWeapon weapon)
    {
        weapon.OnAmmoChanged += HandleAmmoChanged;
        weapon.OnDurabilityChanged += HandleDurabilityChanged;
    }

    private void UnsubscribeFromWeaponEvents(BaseWeapon weapon)
    {
        weapon.OnAmmoChanged -= HandleAmmoChanged;
        weapon.OnDurabilityChanged -= HandleDurabilityChanged;
    }

    private void HandleAmmoChanged(int newAmmo)
    {
        WeaponData data = weaponController.GetWeaponData(weaponController.GetCurrentWeaponType());
        if (data != null && ammoText != null)
        {
            ammoText.text = $"{newAmmo} / {data.maxAmmo}";
        }
    }

    private void HandleDurabilityChanged(float normalizedDurability)
    {
        if (durabilitySlider != null)
        {
            durabilitySlider.value = normalizedDurability;
        }
    }

    private void OnDestroy()
    {
        if (weaponController != null)
        {
            weaponController.OnWeaponChanged -= HandleWeaponChanged;
            weaponController.OnAttackTriggered -= HandleAttackTriggered;

            BaseWeapon currentWeapon = weaponController.GetCurrentWeapon();
            if (currentWeapon != null)
            {
                UnsubscribeFromWeaponEvents(currentWeapon);
            }
        }
    }
}
