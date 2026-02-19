using StarterAssets;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DrunkEffect : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private GameObject cameraRoot;
    [SerializeField] private FirstPersonController playerController;
    [SerializeField] private Volume drunkVolume;

    [Header("Paramètres de Shake - Position")]
    [SerializeField] private float baseShakeIntensity = 0.1f;
    [SerializeField] private float shakeFrequency = 2f;
    [SerializeField] private float maxShakeIntensity = 0.4f;

    [Header("Paramètres de Shake - Rotation")]
    [SerializeField] private float baseRotationIntensity = 2f;
    [SerializeField] private float rotationFrequency = 1.5f;
    [SerializeField] private float maxRotationIntensity = 8f;

    [Header("Post-Processing - Valeurs de Base")]
    [SerializeField] private float baseChromaticAberration = 0.5f;
    [SerializeField] private float baseLensDistortion = -0.3f;
    [SerializeField] private float baseVignetteIntensity = 0.35f;
    [SerializeField] private float baseMotionBlur = 0.3f;
    [SerializeField] private float baseSaturation = 20f;
    [SerializeField] private float baseHueShift = 10f;

    [Header("Transition")]
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 1.0f;

    [Header("Stacking")]
    [SerializeField] private int maxBeerStack = 5;
    [SerializeField] private float stackIntensityMultiplier = 0.3f;

    private bool isDrunk = false;
    private float baseDamageBoost = 0f;
    private int currentBeerStack = 0;
    private float remainingDuration = 0f;
    private float currentBlendFactor = 0f;

    private Coroutine drunkCoroutine;
    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;
    private Quaternion originalCameraRootRotation;

    private ChromaticAberration chromaticAberration;
    private LensDistortion lensDistortion;
    private Vignette vignette;
    private MotionBlur motionBlur;
    private ColorAdjustments colorAdjustments;

    void Start()
    {
        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>();
        }

        if (playerCamera != null)
        {
            originalCameraPosition = playerCamera.transform.localPosition;
            originalCameraRotation = playerCamera.transform.localRotation;
        }

        if (cameraRoot != null)
        {
            originalCameraRootRotation = cameraRoot.transform.localRotation;
        }

        if (drunkVolume != null && drunkVolume.sharedProfile != null)
        {
            drunkVolume.sharedProfile.TryGet(out chromaticAberration);
            drunkVolume.sharedProfile.TryGet(out lensDistortion);
            drunkVolume.sharedProfile.TryGet(out vignette);
            drunkVolume.sharedProfile.TryGet(out motionBlur);
            drunkVolume.sharedProfile.TryGet(out colorAdjustments);

            UpdateVolumeEffects(0f);
            drunkVolume.weight = 0f;

            Debug.Log("DrunkEffect initialisé avec succès");
        }
        else
        {
            Debug.LogWarning("DrunkVolume n'est pas assigné correctement !");
        }
    }

    public void ApplyDrunkEffect(float duration, float damageBoost)
    {
        if (isDrunk)
        {
            currentBeerStack = Mathf.Min(currentBeerStack + 1, maxBeerStack);
            remainingDuration = duration;
            Debug.Log($"Bière supplémentaire ! Stack : {currentBeerStack}/{maxBeerStack}");
        }
        else
        {
            baseDamageBoost = damageBoost;
            currentBeerStack = 1;
            remainingDuration = duration;

            if (drunkCoroutine != null)
            {
                StopCoroutine(drunkCoroutine);
            }

            drunkCoroutine = StartCoroutine(DrunkRoutine());
        }
    }

    private IEnumerator DrunkRoutine()
    {
        isDrunk = true;

        yield return StartCoroutine(FadeInEffect());

        while (remainingDuration > 0)
        {
            ApplyCameraShakeAndRotation();
            remainingDuration -= Time.deltaTime;
            yield return null;
        }

        yield return StartCoroutine(FadeOutEffect());

        isDrunk = false;
        currentBeerStack = 0;

        if (playerCamera != null)
        {
            playerCamera.transform.localPosition = originalCameraPosition;
            playerCamera.transform.localRotation = originalCameraRotation;
        }

        if (cameraRoot != null)
        {
            cameraRoot.transform.localRotation = originalCameraRootRotation;
        }

        if (playerController != null)
        {
            playerController.externalRotationOffset = Vector3.zero;
        }
    }

    private void ApplyCameraShakeAndRotation()
    {
        if (playerCamera == null) return;

        float currentPosIntensity = GetCurrentShakeIntensity() * currentBlendFactor;
        float currentRotIntensity = GetCurrentRotationIntensity() * currentBlendFactor;

        float shakeX = Mathf.Sin(Time.time * shakeFrequency) * currentPosIntensity;
        float shakeY = Mathf.Cos(Time.time * shakeFrequency * 1.5f) * currentPosIntensity;

        playerCamera.transform.localPosition = originalCameraPosition + new Vector3(shakeX, shakeY, 0);

        float rotationX = Mathf.Sin(Time.time * rotationFrequency) * currentRotIntensity;
        float rotationZ = Mathf.Cos(Time.time * rotationFrequency * 0.7f) * (currentRotIntensity * 0.5f);

        if (playerController != null)
        {
            playerController.externalRotationOffset = new Vector3(rotationX, 0, rotationZ);
        }
    }

    private IEnumerator FadeInEffect()
    {
        float elapsed = 0f;

        while (elapsed < fadeInDuration)
        {
            float t = elapsed / fadeInDuration;
            currentBlendFactor = t;

            if (drunkVolume != null)
            {
                drunkVolume.weight = t;
            }

            UpdateVolumeEffects(t);
            ApplyCameraShakeAndRotation();

            elapsed += Time.deltaTime;
            yield return null;
        }

        currentBlendFactor = 1f;

        if (drunkVolume != null)
        {
            drunkVolume.weight = 1f;
        }

        UpdateVolumeEffects(1f);
        Debug.Log($"Effet d'ivresse activé ! Stack : {currentBeerStack}");
    }

    private IEnumerator FadeOutEffect()
    {
        float elapsed = 0f;

        while (elapsed < fadeOutDuration)
        {
            float t = 1f - (elapsed / fadeOutDuration);
            currentBlendFactor = t;

            if (drunkVolume != null)
            {
                drunkVolume.weight = t;
            }

            UpdateVolumeEffects(t);
            ApplyCameraShakeAndRotation();

            elapsed += Time.deltaTime;
            yield return null;
        }

        currentBlendFactor = 0f;

        if (drunkVolume != null)
        {
            drunkVolume.weight = 0f;
        }

        UpdateVolumeEffects(0f);
        Debug.Log("Effet d'ivresse terminé");
    }

    private void UpdateVolumeEffects(float blendFactor)
    {
        float stackMultiplier = GetStackMultiplier();

        if (chromaticAberration != null)
        {
            chromaticAberration.intensity.value = baseChromaticAberration * stackMultiplier * blendFactor;
        }

        if (lensDistortion != null)
        {
            lensDistortion.intensity.value = baseLensDistortion * stackMultiplier * blendFactor;
        }

        if (vignette != null)
        {
            vignette.intensity.value = baseVignetteIntensity * stackMultiplier * blendFactor;
        }

        if (motionBlur != null)
        {
            motionBlur.intensity.value = baseMotionBlur * stackMultiplier * blendFactor;
        }

        if (colorAdjustments != null)
        {
            colorAdjustments.saturation.value = baseSaturation * stackMultiplier * blendFactor;
            colorAdjustments.hueShift.value = baseHueShift * stackMultiplier * blendFactor;
        }
    }

    private float GetCurrentShakeIntensity()
    {
        float stackMultiplier = GetStackMultiplier();
        return Mathf.Min(baseShakeIntensity * stackMultiplier, maxShakeIntensity);
    }

    private float GetCurrentRotationIntensity()
    {
        float stackMultiplier = GetStackMultiplier();
        return Mathf.Min(baseRotationIntensity * stackMultiplier, maxRotationIntensity);
    }

    private float GetStackMultiplier()
    {
        return 1f + (currentBeerStack - 1) * stackIntensityMultiplier;
    }

    public bool IsDrunk() => isDrunk;

    public float GetDamageBoost()
    {
        if (!isDrunk) return 0f;
        return baseDamageBoost * GetStackMultiplier();
    }

    public int GetBeerStack() => currentBeerStack;

    public float GetRemainingDuration() => remainingDuration;
}
