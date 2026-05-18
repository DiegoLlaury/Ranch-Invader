using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Panneau de crédits de fin affiché après l'écran "Suite prochainement".
/// Génère dynamiquement le contenu depuis CreditsData et anime un scroll vers le haut.
/// Retourne au menu principal une fois le scroll terminé ou si le joueur appuie sur Skip.
/// </summary>
public class EndingCreditsPanel : MonoBehaviour
{
    private const string MainMenuSceneName = "Menu";

    [Header("Data")]
    [Tooltip("ScriptableObject contenant les sections et entrées des crédits.")]
    [SerializeField] private CreditsData creditsData;

    [Header("Prefabs")]
    [SerializeField] private TMP_Text sectionTitlePrefab;
    [SerializeField] private TMP_Text entryNamePrefab;
    [SerializeField] private TMP_Text entryRolePrefab;

    [Header("Layout")]
    [SerializeField] private RectTransform contentRoot;
    [SerializeField] private float scrollSpeed = 60f;
    [SerializeField] private float startDelay = 1f;

    [Header("Skip")]
    [Tooltip("Bouton permettant de passer les crédits et retourner au menu.")]
    [SerializeField] private Button skipButton;

    [Header("Fade In")]
    [Tooltip("Image de fond noir à faire disparaître en fondu au début des crédits.")]
    [SerializeField] private Image backgroundFade;
    [SerializeField] private float fadeInDuration = 1f;

    private Coroutine _scrollCoroutine;
    private bool _isStopping;

    // ─────────────────────────────────────────────────────────────────────────

    private void OnEnable()
    {
        _isStopping = false;
        if (skipButton != null)
            skipButton.onClick.AddListener(OnSkipClicked);
    }

    private void OnDisable()
    {
        if (skipButton != null)
            skipButton.onClick.RemoveAllListeners();

        if (_scrollCoroutine != null)
        {
            StopCoroutine(_scrollCoroutine);
            _scrollCoroutine = null;
        }
    }

    /// <summary>Lance la séquence de crédits de fin.</summary>
    public void Play()
    {
        gameObject.SetActive(true);
        _isStopping = false;
        BuildContent();

        if (_scrollCoroutine != null)
            StopCoroutine(_scrollCoroutine);

        _scrollCoroutine = StartCoroutine(RunCredits());
    }

    private void OnSkipClicked()
    {
        Finish();
    }

    // ── Build ─────────────────────────────────────────────────────────────────

    private void BuildContent()
    {
        foreach (Transform child in contentRoot)
            Destroy(child.gameObject);

        if (creditsData == null) return;

        foreach (CreditsData.CreditSection section in creditsData.sections)
        {
            TMP_Text title = Instantiate(sectionTitlePrefab, contentRoot);
            title.text = section.sectionTitle;

            foreach (CreditsData.CreditEntry entry in section.entries)
            {
                TMP_Text nameLabel = Instantiate(entryNamePrefab, contentRoot);
                nameLabel.text = entry.name;

                if (!string.IsNullOrWhiteSpace(entry.role))
                {
                    TMP_Text roleLabel = Instantiate(entryRolePrefab, contentRoot);
                    roleLabel.text = entry.role;
                }
            }
        }

        Canvas.ForceUpdateCanvases();
    }

    // ── Scroll ────────────────────────────────────────────────────────────────

    private IEnumerator RunCredits()
    {
        RectTransform maskRect = contentRoot.parent.GetComponent<RectTransform>();

        contentRoot.anchoredPosition = new Vector2(0f, -maskRect.rect.height);

        // Fondu d'entrée sur le fond
        if (backgroundFade != null)
            yield return StartCoroutine(FadeBackground(1f, 0f, fadeInDuration));

        yield return new WaitForSeconds(startDelay);

        float contentHeight = contentRoot.rect.height;
        float targetY = contentHeight + maskRect.rect.height;

        while (contentRoot.anchoredPosition.y < targetY)
        {
            contentRoot.anchoredPosition += Vector2.up * scrollSpeed * Time.deltaTime;
            yield return null;
        }

        _scrollCoroutine = null;
        Finish();
    }

    private IEnumerator FadeBackground(float from, float to, float duration)
    {
        if (backgroundFade == null) yield break;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            Color c = backgroundFade.color;
            c.a = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
            backgroundFade.color = c;
            yield return null;
        }

        Color final = backgroundFade.color;
        final.a = to;
        backgroundFade.color = final;
    }

    // ── Fin ───────────────────────────────────────────────────────────────────

    private void Finish()
    {
        if (_isStopping) return;
        _isStopping = true;

        if (_scrollCoroutine != null)
        {
            StopCoroutine(_scrollCoroutine);
            _scrollCoroutine = null;
        }

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene(MainMenuSceneName);
    }
}
