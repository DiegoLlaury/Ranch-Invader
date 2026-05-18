using System.Collections;
using UnityEngine;

/// <summary>
/// Orchestrates the Level 01 intro cinematic: enemy ships arrive from hyperspace,
/// Rangers deliver generators, force fields appear, and the player reacts with dialogue.
/// Non-skippable, runs once (persisted via CheckpointManager).
/// Waits for ImpostorPhotoBooth to finish loading before starting.
/// </summary>
public class Level01IntroCinematicController : MonoBehaviour
{
    // — References ————————————————————————————————————————————————————————————

    [Header("Player")]
    [Tooltip("The player GameObject that holds CinematicInputBlocker.")]
    [SerializeField] private CinematicInputBlocker inputBlocker;

    [Header("Camera")]
    [Tooltip("The Cinemachine camera target (PlayerCameraRoot child of the player).")]
    [SerializeField] private Transform cinemachineCameraTarget;

    [Tooltip("Local rotation when looking up at the sky to observe ship arrivals.")]
    [SerializeField] private Vector3 skyLookRotation = new Vector3(-60f, 0f, 0f);

    [Tooltip("Local rotation for normal standing gameplay view.")]
    [SerializeField] private Vector3 standingRotation = new Vector3(0f, 0f, 0f);

    [Header("Checkpoint")]
    [Tooltip("Unique event ID used to track whether this cinematic has been played.")]
    [SerializeField] private string cinematicEventId = "level01_intro";

    [Header("Ships")]
    [Tooltip("All Glorp ship GameObjects in the scene.")]
    [SerializeField] private GameObject[] glorpShips;

    [Tooltip("The UFO ship GameObject in the scene.")]
    [SerializeField] private GameObject ufoShip;

    [Tooltip("Offset from each ship's final position to its hyperspace entry point (Glorp).")]
    [SerializeField] private Vector3 glorpStartOffset = new Vector3(0f, 50f, 0f);

    [Tooltip("Offset from the ship's final position to its hyperspace entry point (UFO).")]
    [SerializeField] private Vector3 ufoStartOffset = new Vector3(0f, 60f, 10f);

    [Header("Generators & Force Fields")]
    [Tooltip("Generator GameObjects to activate during the cinematic.")]
    [SerializeField] private GameObject[] generators;

    [Tooltip("Force field GameObjects to activate during the cinematic.")]
    [SerializeField] private GameObject[] forceFields;

    [Tooltip("EnemyRanger prefab used as a temporary delivery vehicle for generators.")]
    [SerializeField] private GameObject rangerPrefab;

    [Tooltip("Height above each generator from which the Ranger descends.")]
    [SerializeField] private float rangerSkyHeight = 30f;

    [Header("Audio")]
    [Tooltip("Sound name for the Glorp ship arrival (registered in SoundDatabase).")]
    [SerializeField] private string glorpArrivalSoundName = "GlorpArrival";

    [Tooltip("Sound name for the UFO ship arrival (registered in SoundDatabase).")]
    [SerializeField] private string ufoArrivalSoundName = "UFOArrival";

    [Tooltip("Sound name for the player dialogue (registered in SoundDatabase).")]
    [SerializeField] private string playerDialogueSoundName = "PlayerDialogueIntro";

    [Tooltip("Sound name for the cinematic background music (registered in SoundDatabase).")]
    [SerializeField] private string cinematicMusicSoundName = "CinematicIntroMusic";

    [SerializeField] private GameObject musicTempory;

    [Header("Music")]
    [Tooltip("Tous les GameObjects portant une AudioSource de musique qui ne doivent démarrer " +
             "qu'après la cinématique (ou immédiatement si la cinématique est déjà vue).")]
    [SerializeField] private GameObject[] musicObjects;


    [Header("Animation")]
    [Tooltip("Easing curve applied to ship and Ranger arrival animations.")]
    [SerializeField] private AnimationCurve arrivalCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    // — Timing constants ——————————————————————————————————————————————————————

    private const float CameraLookUpDuration = 0.2f;
    private const float GlorpArrivalDuration = 2.3f;
    private const float UfoArrivalDuration = 2.0f;
    private const float CameraLookDownDuration = 0.5f;
    private const float DeliveryStart = 5.0f;
    private const float DeliveryDuration = 2.0f;
    private const float ForceFieldScaleDuration = 1.5f;
    private const float CameraHoldExtraDuration = 1f;
    private const float CameraReturnDelay = 9.0f + CameraHoldExtraDuration;
    private const float CameraReturnDuration = 1.0f;
    private Vector3[] _glorpOriginalScales;
    private Vector3 _ufoOriginalScale;
    private Vector3[] _forceFieldOriginalScales;

    // ————————————————————————————————————————————————————————————————————————

    private void Start()
    {
        if (musicTempory != null)
            musicTempory.SetActive(false);

        // Désactive toutes les sources de musique — elles ne démarreront qu'après la cinématique.
        SetMusicObjectsActive(false);

        if (CheckpointManager.Instance == null)
        {
            Debug.LogWarning("[Level01IntroCinematic] CheckpointManager not found. Activating all targets.");
            ActivateAllTargets();
            return;
        }

        if (CheckpointManager.Instance.IsEventExecuted(cinematicEventId))
        {
            Debug.Log("[Level01IntroCinematic] Cinematic already played, activating all targets.");
            ActivateAllTargets();
            return;
        }

        Debug.Log("[Level01IntroCinematic] Cinematic not yet played, starting sequence.");
        inputBlocker.Block();
        StartCoroutine(WaitForLoadingThenPlay());
    }

    private void CacheOriginalScales()
    {
        if (glorpShips != null)
        {
            _glorpOriginalScales = new Vector3[glorpShips.Length];
            for (int i = 0; i < glorpShips.Length; i++)
                _glorpOriginalScales[i] = glorpShips[i] != null ? glorpShips[i].transform.localScale : Vector3.one;
        }

        _ufoOriginalScale = ufoShip != null ? ufoShip.transform.localScale : Vector3.one;

        if (forceFields != null)
        {
            _forceFieldOriginalScales = new Vector3[forceFields.Length];
            for (int i = 0; i < forceFields.Length; i++)
                _forceFieldOriginalScales[i] = forceFields[i] != null ? forceFields[i].transform.localScale : Vector3.one;
        }
    }

    private IEnumerator WaitForLoadingThenPlay()
    {
        ImpostorPhotoBooth booth = FindAnyObjectByType<ImpostorPhotoBooth>();

        if (booth != null)
        {
            // Wait one frame so all Impostors can register their capture requests
            yield return null;

            if (booth.TotalCapturesRequested > 0 && !booth.IsAllCapturesDone)
            {
                Debug.Log($"[Level01IntroCinematic] Waiting for impostor loading ({booth.TotalCapturesCompleted}/{booth.TotalCapturesRequested})...");
                yield return new WaitUntil(() => booth.IsAllCapturesDone);
            }

            // Wait the same extra frames as ImpostorLoadingScreen to let the GPU finish
            for (int i = 0; i < 3; i++)
                yield return new WaitForEndOfFrame();
        }

        CacheOriginalScales();
        // Deactivate targets right before playing so Impostors had time to capture them
        DeactivateAllTargets();

        Debug.Log("[Level01IntroCinematic] Loading done, starting cinematic.");
        yield return StartCoroutine(RunCinematic());
    }


    private IEnumerator RunCinematic()
    {
        SoundManager.Instance.PlaySound2D(cinematicMusicSoundName);

        // [0s - 0.2s] Camera looks up at the sky
        yield return StartCoroutine(LerpCameraRotation(skyLookRotation, CameraLookUpDuration));


        // [0.2s - 2.5s] All Glorp ships arrive from hyperspace simultaneously
        for (int i = 0; i < glorpShips.Length; i++)
        {
            GameObject ship = glorpShips[i];
            if (ship == null) continue;

            Vector3 endPos = ship.transform.position;
            Vector3 startPos = endPos + glorpStartOffset;
            ship.SetActive(true);
            Vector3 targetScale = (_glorpOriginalScales != null && i < _glorpOriginalScales.Length)
            ? _glorpOriginalScales[i] : Vector3.one;
            StartCoroutine(AnimateShipArrival(ship.transform, startPos, endPos, targetScale, GlorpArrivalDuration));

        }

        yield return new WaitForSeconds(GlorpArrivalDuration);
        SoundManager.Instance.PlaySound2D(glorpArrivalSoundName);

        // [2.5s - 4.5s] UFO ship arrives from hyperspace
        Vector3 ufoEnd = ufoShip.transform.position;
        Vector3 ufoStart = ufoEnd + ufoStartOffset;
        ufoShip.SetActive(true);
        yield return StartCoroutine(AnimateShipArrival(ufoShip.transform, ufoStart, ufoEnd, _ufoOriginalScale, UfoArrivalDuration));
        SoundManager.Instance.PlaySound2D(ufoArrivalSoundName);

        // [4.5s - 5s] Camera pivots down toward generators / force fields
        yield return StartCoroutine(LerpCameraRotation(standingRotation, CameraLookDownDuration));

        // [5s - 7s] Rangers deliver generators + force fields scale in (parallel)
        for (int i = 0; i < generators.Length; i++)
        {
            StartCoroutine(AnimateRangerDelivery(generators[i], DeliveryDuration));
        }

        for (int i = 0; i < forceFields.Length; i++)
        {
            StartCoroutine(AnimateScaleIn(forceFields[i].transform, _forceFieldOriginalScales[i], ForceFieldScaleDuration));
        }

        // [5s - 9s] Player dialogue audio
        VoiceManager.Instance?.PlayVoiceForced(playerDialogueSoundName, VoicePriority.Normal);

        // Wait until camera return moment
        float waitForReturn = CameraReturnDelay - DeliveryStart;
        yield return new WaitForSeconds(waitForReturn);

        // [9s - 10s] Camera returns to standing rotation
        yield return StartCoroutine(LerpCameraRotation(standingRotation, CameraReturnDuration));

        if (musicTempory != null)
            musicTempory.SetActive(true);
        SetMusicObjectsActive(true);
        // [10s] Register event and unblock inputs
        CheckpointManager.Instance.RegisterEvent(cinematicEventId);
        inputBlocker.Unblock();
    }

    // — Helper coroutines ——————————————————————————————————————————————————

    /// <summary>Animates a ship from startPos to endPos with scale 0 to 1.</summary>
    private IEnumerator AnimateShipArrival(Transform ship, Vector3 startPos, Vector3 endPos, Vector3 targetScale, float duration)
    {
        ship.position = startPos;
        ship.localScale = Vector3.zero;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = arrivalCurve.Evaluate(Mathf.Clamp01(elapsed / duration));
            ship.position = Vector3.Lerp(startPos, endPos, t);
            ship.localScale = Vector3.Lerp(Vector3.zero, targetScale, t);
            yield return null;
        }

        ship.position = endPos;
        ship.localScale = targetScale;
    }


    /// <summary>Spawns a temporary Ranger that descends from the sky, delivers the generator, then self-destructs.</summary>
    private IEnumerator AnimateRangerDelivery(GameObject generator, float duration)
    {
        Vector3 generatorPos = generator.transform.position;
        Vector3 skyPos = generatorPos + Vector3.up * rangerSkyHeight;

        GameObject ranger = Instantiate(rangerPrefab, skyPos, Quaternion.identity);
        generator.transform.SetParent(ranger.transform);
        generator.SetActive(true);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = arrivalCurve.Evaluate(Mathf.Clamp01(elapsed / duration));
            ranger.transform.position = Vector3.Lerp(skyPos, generatorPos, t);
            yield return null;
        }

        ranger.transform.position = generatorPos;

        // Detach generator and place it at its final position
        generator.transform.SetParent(null);
        generator.transform.position = generatorPos;

        Destroy(ranger);
    }

    /// <summary>Scales a target from 0 to 1 over duration.</summary>
    private IEnumerator AnimateScaleIn(Transform target, Vector3 targetScale, float duration)
    {
        target.gameObject.SetActive(true);
        target.localScale = Vector3.zero;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = arrivalCurve.Evaluate(Mathf.Clamp01(elapsed / duration));
            target.localScale = Vector3.Lerp(Vector3.zero, targetScale, t);
            yield return null;
        }

        target.localScale = targetScale;
    }


    /// <summary>Lerps the camera target's local rotation toward targetRot over duration.</summary>
    private IEnumerator LerpCameraRotation(Vector3 targetRot, float duration)
    {
        Quaternion from = cinemachineCameraTarget.localRotation;
        Quaternion to = Quaternion.Euler(targetRot);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = arrivalCurve.Evaluate(Mathf.Clamp01(elapsed / duration));
            cinemachineCameraTarget.localRotation = Quaternion.Lerp(from, to, t);
            yield return null;
        }

        cinemachineCameraTarget.localRotation = to;
    }

    /// <summary>Activates all target GameObjects (ships, generators, force fields). Used when cinematic was already seen.</summary>
    private void ActivateAllTargets()
    {
        if (glorpShips != null)
        {
            for (int i = 0; i < glorpShips.Length; i++)
            {
                if (glorpShips[i] != null) glorpShips[i].SetActive(true);
            }
        }

        if (ufoShip != null) ufoShip.SetActive(true);

        if (generators != null)
        {
            for (int i = 0; i < generators.Length; i++)
            {
                if (generators[i] != null) generators[i].SetActive(true);
            }
        }

        if (forceFields != null)
        {
            for (int i = 0; i < forceFields.Length; i++)
            {
                if (forceFields[i] != null) forceFields[i].SetActive(true);
            }
        }

        if (musicTempory != null)
            musicTempory.SetActive(true);

        SetMusicObjectsActive(true);
    }

    /// <summary>Active ou désactive tous les GameObjects de musique référencés.</summary>
    private void SetMusicObjectsActive(bool active)
    {
        if (musicObjects == null) return;
        foreach (GameObject musicObj in musicObjects)
        {
            if (musicObj != null)
                musicObj.SetActive(active);
        }
    }

    /// <summary>Deactivates all target GameObjects before the cinematic plays.</summary>
    private void DeactivateAllTargets()
    {
        if (glorpShips != null)
        {
            for (int i = 0; i < glorpShips.Length; i++)
            {
                if (glorpShips[i] != null) glorpShips[i].SetActive(false);
            }
        }

        if (ufoShip != null) ufoShip.SetActive(false);

        if (generators != null)
        {
            for (int i = 0; i < generators.Length; i++)
            {
                if (generators[i] != null) generators[i].SetActive(false);
            }
        }

        if (forceFields != null)
        {
            for (int i = 0; i < forceFields.Length; i++)
            {
                if (forceFields[i] != null) forceFields[i].SetActive(false);
            }
        }
    }
}
