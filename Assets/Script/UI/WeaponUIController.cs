using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using StarterAssets;

public class WeaponUIController : MonoBehaviour
{
    [Header("Références")]
    public WeaponController weaponController;
    public FirstPersonController playerController;

    [Header("Affichage de l'arme")]
    public RectTransform weaponVisualRoot;
    public Image weaponSprite;

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

    [Header("Animation Idle Flottante")]
    public float idlePositionAmplitude = 5f;
    public float idlePositionSpeed = 2f;
    public float idleScaleAmplitude = 0.03f;
    public float idleScaleSpeed = 2f;
    public float speedMultiplier = 0.5f;
    public float updateRate = 0f;

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

    private Vector2 weaponIdlePosition;
    private Vector2 leftFistIdlePos;
    private Vector2 rightFistIdlePos;
    private Vector3 weaponStartScale;
    private Vector3 leftFistStartScale;
    private Vector3 rightFistStartScale;

    private bool isAnimating;
    private FistWeapon currentFistWeapon;
    private float updateTimer = 0f;

    private void Start()
    {
        if (weaponVisualRoot != null)
        {
            weaponIdlePosition = weaponVisualRoot.anchoredPosition;
            weaponStartScale = weaponVisualRoot.localScale;
        }

        if (leftFistRoot != null)
        {
            leftFistIdlePos = leftFistRoot.anchoredPosition;
            leftFistStartScale = leftFistRoot.localScale;
        }

        if (rightFistRoot != null)
        {
            rightFistIdlePos = rightFistRoot.anchoredPosition;
            rightFistStartScale = rightFistRoot.localScale;
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

    private void Update()
    {
        if (!isAnimating)
        {
            ApplyIdleAnimation();
        }
    }

    private void ApplyIdleAnimation()
    {
        float currentPlayerSpeed = 0f;
        if (playerController != null)
        {
            CharacterController controller = playerController.GetComponent<CharacterController>();
            if (controller != null)
            {
                currentPlayerSpeed = controller.velocity.magnitude;
            }
        }

        float speedFactor = 1f + (currentPlayerSpeed * speedMultiplier);
        float t = Time.time;
        updateTimer += Time.deltaTime;

        float yOffset = Mathf.Sin(t * idlePositionSpeed * speedFactor) * idlePositionAmplitude;
        float scaleOffset = Mathf.Sin(t * idleScaleSpeed * speedFactor) * idleScaleAmplitude;

        if (updateRate == 0f || updateTimer >= updateRate)
        {
            WeaponType currentType = weaponController.GetCurrentWeaponType();

            if (currentType == WeaponType.Fist)
            {
                if (leftFistRoot != null)
                {
                    leftFistRoot.anchoredPosition = leftFistIdlePos + Vector2.up * yOffset;
                    leftFistRoot.localScale = leftFistStartScale + Vector3.one * scaleOffset;
                }

                if (rightFistRoot != null)
                {
                    rightFistRoot.anchoredPosition = rightFistIdlePos + Vector2.up * yOffset;
                    rightFistRoot.localScale = rightFistStartScale + Vector3.one * scaleOffset;
                }
            }
            else
            {
                if (weaponVisualRoot != null)
                {
                    weaponVisualRoot.anchoredPosition = weaponIdlePosition + Vector2.up * yOffset;
                    weaponVisualRoot.localScale = weaponStartScale + Vector3.one * scaleOffset;
                }
            }

            if (updateRate > 0f)
            {
                updateTimer = 0f;
            }
        }
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
                StartCoroutine(PlayMultiFrameAnimation(AnimationType.Attack));
            }
        }
    }

    public void PlayReloadAnimation()
    {
        if (!isAnimating)
        {
            StartCoroutine(PlayMultiFrameAnimation(AnimationType.Reload));
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

    private IEnumerator PlayMultiFrameAnimation(AnimationType animType)
    {
        isAnimating = true;

        WeaponData currentData = weaponController.GetWeaponData(weaponController.GetCurrentWeaponType());
        if (currentData == null || weaponVisualRoot == null || weaponSprite == null)
        {
            isAnimating = false;
            yield break;
        }

        WeaponAnimationFrame[] frames = animType == AnimationType.Attack ? currentData.attackAnimation : currentData.reloadAnimation;

        if (frames == null || frames.Length == 0)
        {
            isAnimating = false;
            yield break;
        }

        Vector2 currentPosition = weaponIdlePosition;
        Quaternion currentRotation = Quaternion.identity;
        Vector3 currentScale = weaponStartScale;

        foreach (WeaponAnimationFrame frame in frames)
        {
            Vector2 targetPosition = weaponIdlePosition + frame.positionOffset;
            Quaternion targetRotation = Quaternion.Euler(0, 0, frame.rotationAngle);
            Vector3 targetScale = Vector3.Scale(weaponStartScale, frame.scaleMultiplier);

            if (frame.sprite != null)
            {
                weaponSprite.sprite = frame.sprite;
            }

            yield return AnimateToFrame(
                currentPosition, targetPosition,
                currentRotation, targetRotation,
                currentScale, targetScale,
                frame.duration,
                frame.curveType
            );

            currentPosition = targetPosition;
            currentRotation = targetRotation;
            currentScale = targetScale;
        }

        yield return AnimateToFrame(
            currentPosition, weaponIdlePosition,
            currentRotation, Quaternion.identity,
            currentScale, weaponStartScale,
            0.2f,
            AnimationCurveType.EaseOut
        );

        weaponSprite.sprite = currentData.idleSprite;

        isAnimating = false;
    }

    private IEnumerator AnimateToFrame(
        Vector2 startPos, Vector2 endPos,
        Quaternion startRot, Quaternion endRot,
        Vector3 startScale, Vector3 endScale,
        float duration,
        AnimationCurveType curveType)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            t = ApplyCurve(t, curveType);

            if (weaponVisualRoot != null)
            {
                weaponVisualRoot.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                weaponVisualRoot.localRotation = Quaternion.Lerp(startRot, endRot, t);
                weaponVisualRoot.localScale = Vector3.Lerp(startScale, endScale, t);
            }

            yield return null;
        }

        if (weaponVisualRoot != null)
        {
            weaponVisualRoot.anchoredPosition = endPos;
            weaponVisualRoot.localRotation = endRot;
            weaponVisualRoot.localScale = endScale;
        }
    }

    private float ApplyCurve(float t, AnimationCurveType curveType)
    {
        switch (curveType)
        {
            case AnimationCurveType.Linear:
                return t;

            case AnimationCurveType.EaseIn:
                return t * t;

            case AnimationCurveType.EaseOut:
                return 1f - (1f - t) * (1f - t);

            case AnimationCurveType.EaseInOut:
                return t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;

            case AnimationCurveType.Bounce:
                return Mathf.Sin(t * Mathf.PI);

            default:
                return t;
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
        Vector3 fistStartScale = useLeft ? leftFistStartScale : rightFistStartScale;

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

        yield return MoveFist(fistRoot, fistIdlePos, punchPos, fistStartScale, fistPunchDuration, useLeft);
        yield return MoveFist(fistRoot, punchPos, fistIdlePos, fistStartScale, fistReturnDuration, useLeft);

        punchSprite.enabled = false;
        idleSprite.enabled = true;

        isAnimating = false;
    }

    private IEnumerator MoveFist(RectTransform fist, Vector2 from, Vector2 to, Vector3 startScale, float duration, bool isLeft)
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
            fist.localScale = startScale;
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

public enum AnimationType
{
    Attack,
    Reload
}
