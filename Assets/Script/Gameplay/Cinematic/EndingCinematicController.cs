using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Orchestre la cinématique de fin de niveau :
/// 1. Bloque les inputs du joueur.
/// 2. Le vaisseau UFO s'envole vers le ciel.
/// 3. Son de voix du joueur joué.
/// 4. Fondu vers le noir.
/// 5. Texte "Suite prochainement…" affiché.
/// 6. Les crédits de fin défilent.
/// Déclenché en appelant StartEnding() depuis un GameplayEventProxy.
/// </summary>
public class EndingCinematicController : MonoBehaviour
{
    // ── Player ────────────────────────────────────────────────────────────────

    [Header("Player")]
    [Tooltip("CinematicInputBlocker sur le PlayerCapsule.")]
    [SerializeField] private CinematicInputBlocker inputBlocker;

    // ── UFO ───────────────────────────────────────────────────────────────────

    [Header("UFO")]
    [Tooltip("Le GameObject de l'UFO à faire s'enfuir.")]
    [SerializeField] private GameObject ufoShip;

    [Tooltip("Décalage vers lequel l'UFO s'envole (relatif à sa position actuelle).")]
    [SerializeField] private Vector3 ufoEscapeOffset = new Vector3(0f, 120f, -80f);

    [Tooltip("Durée de l'animation de fuite de l'UFO.")]
    [SerializeField] private float ufoEscapeDuration = 4f;

    [Tooltip("Courbe d'easing pour la fuite de l'UFO.")]
    [SerializeField] private AnimationCurve ufoEscapeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Tooltip("Son joué au départ de l'UFO (enregistré dans SoundDatabase).")]
    [SerializeField] private string ufoEscapeSoundName = "UFOEscape";

    // ── Voix ──────────────────────────────────────────────────────────────────

    [Header("Voix du joueur")]
    [Tooltip("Son de voix joué après la fuite de l'UFO (enregistré dans SoundDatabase, ex: S_VoiceEndCinematic).")]
    [SerializeField] private string playerVoiceSoundName = "VoiceEndCinematic";

    [Tooltip("Durée estimée du clip de voix en secondes. La cinématique attend cette durée avant de continuer.")]
    [SerializeField] private float playerVoiceDuration = 3f;

    // ── Screen Fade ───────────────────────────────────────────────────────────

    [Header("Screen Fade")]
    [Tooltip("Image plein écran noir utilisée pour le fondu.")]
    [SerializeField] private Image fadeImage;

    [Tooltip("Durée du fondu vers le noir.")]
    [SerializeField] private float fadeToBlackDuration = 2f;

    // ── "Suite prochainement" ─────────────────────────────────────────────────

    [Header("Suite Prochainement")]
    [Tooltip("Panneau contenant le texte 'Suite prochainement'.")]
    [SerializeField] private GameObject comingSoonPanel;

    [Tooltip("Texte 'Suite prochainement' à faire apparaître en fondu.")]
    [SerializeField] private TMP_Text comingSoonText;

    [Tooltip("Durée du fondu d'apparition du texte.")]
    [SerializeField] private float comingSoonFadeInDuration = 1.5f;

    [Tooltip("Durée d'affichage du texte avant de passer aux crédits.")]
    [SerializeField] private float comingSoonDisplayDuration = 3f;

    [Tooltip("Durée du fondu de disparition du texte.")]
    [SerializeField] private float comingSoonFadeOutDuration = 1f;

    // ── Crédits ───────────────────────────────────────────────────────────────

    [Header("Crédits")]
    [Tooltip("Panneau de crédits de fin à afficher après 'Suite prochainement'.")]
    [SerializeField] private EndingCreditsPanel endingCreditsPanel;

    // ── Timing ────────────────────────────────────────────────────────────────

    [Header("Timing")]
    [Tooltip("Délai en secondes avant que l'UFO commence à s'enfuir après le déclenchement.")]
    [SerializeField] private float delayBeforeUfoEscape = 0.5f;

    [Tooltip("Délai entre la fin de la fuite de l'UFO et le début de la voix.")]
    [SerializeField] private float delayBetweenUfoAndVoice = 0.8f;

    [Tooltip("Délai entre la fin de la voix et le début du fondu.")]
    [SerializeField] private float delayBetweenVoiceAndFade = 0.5f;

    // ── État interne ──────────────────────────────────────────────────────────

    private bool _isRunning = false;

    // ─────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (fadeImage != null)
        {
            SetFadeAlpha(0f);
            fadeImage.gameObject.SetActive(true);
        }

        if (comingSoonPanel != null)
            comingSoonPanel.SetActive(false);
    }

    /// <summary>Point d'entrée public — appelé par GameplayEventProxy quand le joueur
    /// passe dans la zone de fin.</summary>
    public void StartEnding()
    {
        if (_isRunning) return;
        _isRunning = true;
        StartCoroutine(RunEnding());
    }

    // ── Séquence principale ───────────────────────────────────────────────────

    private IEnumerator RunEnding()
    {
        inputBlocker.Block();

        // 1 — Délai initial
        yield return new WaitForSeconds(delayBeforeUfoEscape);

        // 2 — UFO s'enfuit (son + animation en parallèle)
        if (!string.IsNullOrEmpty(ufoEscapeSoundName))
            SoundManager.Instance.PlaySound2D(ufoEscapeSoundName);

        if (ufoShip != null)
            StartCoroutine(AnimateUfoEscape());

        yield return new WaitForSeconds(ufoEscapeDuration);

        // 3 — Délai puis voix du joueur
        yield return new WaitForSeconds(delayBetweenUfoAndVoice);

        if (!string.IsNullOrEmpty(playerVoiceSoundName))
            VoiceManager.Instance?.PlayVoiceForced(playerVoiceSoundName, VoicePriority.Objective);

        yield return new WaitForSeconds(playerVoiceDuration);

        // 4 — Délai puis fondu au noir
        yield return new WaitForSeconds(delayBetweenVoiceAndFade);
        yield return StartCoroutine(FadeScreen(0f, 1f, fadeToBlackDuration));

        // 5 — "Suite prochainement"
        yield return StartCoroutine(ShowComingSoon());

        // 6 — Crédits de fin
        if (endingCreditsPanel != null)
            endingCreditsPanel.Play();
    }

    // ── Animation UFO ─────────────────────────────────────────────────────────

    private IEnumerator AnimateUfoEscape()
    {
        Vector3 startPos = ufoShip.transform.position;
        Vector3 endPos = startPos + ufoEscapeOffset;
        Vector3 startScale = ufoShip.transform.localScale;

        float elapsed = 0f;
        while (elapsed < ufoEscapeDuration)
        {
            elapsed += Time.deltaTime;
            float t = ufoEscapeCurve.Evaluate(Mathf.Clamp01(elapsed / ufoEscapeDuration));
            ufoShip.transform.position = Vector3.Lerp(startPos, endPos, t);
            // Rétrécit légèrement l'UFO pour l'effet de distance
            ufoShip.transform.localScale = Vector3.Lerp(startScale, startScale * 0.3f, t);
            yield return null;
        }

        ufoShip.transform.position = endPos;
        ufoShip.SetActive(false);
    }

    // ── Fondu écran ───────────────────────────────────────────────────────────

    private IEnumerator FadeScreen(float fromAlpha, float toAlpha, float duration)
    {
        if (fadeImage == null) yield break;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            SetFadeAlpha(Mathf.Lerp(fromAlpha, toAlpha, Mathf.Clamp01(elapsed / duration)));
            yield return null;
        }
        SetFadeAlpha(toAlpha);
    }

    private void SetFadeAlpha(float alpha)
    {
        if (fadeImage == null) return;
        Color c = fadeImage.color;
        c.a = alpha;
        fadeImage.color = c;
    }

    // ── "Suite prochainement" ─────────────────────────────────────────────────

    private IEnumerator ShowComingSoon()
    {
        if (comingSoonPanel == null || comingSoonText == null) yield break;

        comingSoonPanel.SetActive(true);
        SetComingSoonAlpha(0f);

        yield return StartCoroutine(FadeComingSoon(0f, 1f, comingSoonFadeInDuration));
        yield return new WaitForSeconds(comingSoonDisplayDuration);
        yield return StartCoroutine(FadeComingSoon(1f, 0f, comingSoonFadeOutDuration));

        comingSoonPanel.SetActive(false);
    }

    private IEnumerator FadeComingSoon(float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            SetComingSoonAlpha(Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration)));
            yield return null;
        }
        SetComingSoonAlpha(to);
    }

    private void SetComingSoonAlpha(float alpha)
    {
        if (comingSoonText == null) return;
        Color c = comingSoonText.color;
        c.a = alpha;
        comingSoonText.color = c;
    }
}
