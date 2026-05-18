using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Orchestrates the opening cinematic sequence:
/// black screen ? cat meow ? eyes open (fade in) ? dialogue ? camera rises to standing position.
/// Blocks player input for the entire duration.
/// </summary>
public class OpeningCinematicController : MonoBehaviour
{
    // ?? References ????????????????????????????????????????????????????????????

    [Header("Player")]
    [Tooltip("The player GameObject that holds CinematicInputBlocker.")]
    [SerializeField] private CinematicInputBlocker inputBlocker;

    [Header("Camera")]
    [Tooltip("The Cinemachine camera target (CinemachineCameraTarget child of the player).")]
    [SerializeField] private Transform cinemachineCameraTarget;

    [Tooltip("Local rotation of the camera target when lying on the floor (looking up).")]
    [SerializeField] private Vector3 lyingRotation = new Vector3(80f, 0f, 0f);

    [Tooltip("Local rotation of the camera target when fully standing (normal gameplay).")]
    [SerializeField] private Vector3 standingRotation = new Vector3(0f, 0f, 0f);

    [Tooltip("Duration in seconds for the camera to rise from lying to standing.")]
    [SerializeField] private float cameraRiseDuration = 2.5f;

    [Tooltip("AnimationCurve controlling the camera rise easing.")]
    [SerializeField] private AnimationCurve cameraRiseCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Screen Fade")]
    [Tooltip("Full-screen black Image used for the fade effect.")]
    [SerializeField] private Image fadeImage;

    [Tooltip("Duration in seconds for the eyes-open fade from black.")]
    [SerializeField] private float eyesOpenFadeDuration = 1.8f;

    [Header("Audio")]
    [Tooltip("Name of the cat meow sound registered in the SoundDatabase.")]
    [SerializeField] private string catMeowSoundName = "CatMeow";

    [Tooltip("Delay in seconds before the meow plays after scene start.")]
    [SerializeField] private float meowDelay = 1f;

    [Tooltip("Delay in seconds between the meow ending and the eyes opening.")]
    [SerializeField] private float pauseAfterMeow = 0.8f;

    [Header("Dialogue")]
    [SerializeField] private DialogueDisplay dialogueDisplay;
    [SerializeField] private DialogueLine[] openingLines;

    // ?????????????????????????????????????????????????????????????????????????

    private void Start()
    {
        // Snap camera to lying position immediately
        if (cinemachineCameraTarget != null)
            cinemachineCameraTarget.localRotation = Quaternion.Euler(lyingRotation);

        // Start fully black
        SetFadeAlpha(1f);

        inputBlocker.Block();
        StartCoroutine(RunCinematic());
    }

    private IEnumerator RunCinematic()
    {
        // 1 � Brief pause before anything happens
        yield return new WaitForSeconds(meowDelay);

        // 2 � Play cat meow
        SoundManager.Instance.PlaySound2D(catMeowSoundName);

        yield return new WaitForSeconds(pauseAfterMeow);

        // 3 – Eyes open: fade from black to clear
        yield return StartCoroutine(FadeScreen(1f, 0f, eyesOpenFadeDuration));

        VoiceManager.Instance?.PlayVoiceForced("Voice_CinematicIntro", VoicePriority.Normal);

        // 4 – Start dialogue immediately after fade
        bool dialogueDone = false;
        dialogueDisplay.OnDialogueComplete += () => dialogueDone = true;
        dialogueDisplay.StartDialogue(openingLines);

        // 5 � Rise camera while dialogue runs (both happen concurrently)
        yield return StartCoroutine(RiseCamera());

        // 6 � Wait for dialogue to finish if camera rose faster
        yield return new WaitUntil(() => dialogueDone);

        // 7 � Hand control back to the player
        inputBlocker.Unblock();
    }

    private IEnumerator FadeScreen(float fromAlpha, float toAlpha, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            SetFadeAlpha(Mathf.Lerp(fromAlpha, toAlpha, t));
            yield return null;
        }
        SetFadeAlpha(toAlpha);
    }

    private IEnumerator RiseCamera()
    {
        float elapsed = 0f;
        Quaternion from = Quaternion.Euler(lyingRotation);
        Quaternion to = Quaternion.Euler(standingRotation);

        while (elapsed < cameraRiseDuration)
        {
            elapsed += Time.deltaTime;
            float t = cameraRiseCurve.Evaluate(Mathf.Clamp01(elapsed / cameraRiseDuration));
            cinemachineCameraTarget.localRotation = Quaternion.Lerp(from, to, t);
            yield return null;
        }

        cinemachineCameraTarget.localRotation = to;
    }

    private void SetFadeAlpha(float alpha)
    {
        if (fadeImage == null) return;
        Color c = fadeImage.color;
        c.a = alpha;
        fadeImage.color = c;
    }
}
