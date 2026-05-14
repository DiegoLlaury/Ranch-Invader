using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Contrôleur de crédits — génère dynamiquement le contenu depuis CreditsData
/// et anime un scroll continu vers le haut.
/// </summary>
public class CreditsController : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private CreditsData creditsData;

    [Header("Prefabs")]
    [SerializeField] private TMP_Text sectionTitlePrefab;
    [SerializeField] private TMP_Text entryNamePrefab;
    [SerializeField] private TMP_Text entryRolePrefab;

    [Header("Layout")]
    [SerializeField] private RectTransform contentRoot;
    [SerializeField] private float scrollSpeed = 80f;
    [SerializeField] private float startDelay = 0.5f;

    [Header("Close")]
    [SerializeField] private Button closeButton;

    private Coroutine scrollCoroutine;
    private float contentHeight;
    private MenuNavigator menuNavigator;
    private bool isStopping; // garde contre la récursion OnDisable  StopCredits

    private void OnEnable()
    {
        isStopping = false;
        closeButton.onClick.AddListener(OnCloseClicked);
    }

    private void OnDisable()
    {
        closeButton.onClick.RemoveAllListeners();

        // Stoppe la coroutine sans rappeler SetActive(false)
        if (scrollCoroutine != null)
        {
            StopCoroutine(scrollCoroutine);
            scrollCoroutine = null;
        }
    }

    /// <summary>Génère le contenu et lance le scroll. Reçoit le MenuNavigator pour le callback retour.</summary>
    public void PlayCredits(MenuNavigator navigator)
    {
        menuNavigator = navigator;
        isStopping = false;
        BuildContent();
        if (scrollCoroutine != null) StopCoroutine(scrollCoroutine);
        scrollCoroutine = StartCoroutine(ScrollRoutine());
    }

    private void OnCloseClicked()
    {
        FinishAndReturn();
    }

    // Appelé aussi à la fin du scroll automatique
    private void FinishAndReturn()
    {
        if (isStopping) return;
        isStopping = true;

        if (scrollCoroutine != null)
        {
            StopCoroutine(scrollCoroutine);
            scrollCoroutine = null;
        }

        // Retourne au menu AVANT de se désactiver pour que ReturnToMainMenu
        // puisse réactiver le buttonsGroup sans conflit de SetActive
        menuNavigator?.ReturnToMainMenu();
    }

    private void BuildContent()
    {
        foreach (Transform child in contentRoot)
            Destroy(child.gameObject);

        if (creditsData == null) return;

        foreach (var section in creditsData.sections)
        {
            var title = Instantiate(sectionTitlePrefab, contentRoot);
            title.text = section.sectionTitle;

            foreach (var entry in section.entries)
            {
                var nameLabel = Instantiate(entryNamePrefab, contentRoot);
                nameLabel.text = entry.name;

                if (!string.IsNullOrWhiteSpace(entry.role))
                {
                    var roleLabel = Instantiate(entryRolePrefab, contentRoot);
                    roleLabel.text = entry.role;
                }
            }
        }

        Canvas.ForceUpdateCanvases();
        contentHeight = contentRoot.rect.height;
    }

    private IEnumerator ScrollRoutine()
    {
        var maskRect = contentRoot.parent.GetComponent<RectTransform>();

        contentRoot.anchoredPosition = new Vector2(0f, -maskRect.rect.height);
        yield return new WaitForSeconds(startDelay);

        float targetY = contentHeight + maskRect.rect.height;

        while (contentRoot.anchoredPosition.y < targetY)
        {
            contentRoot.anchoredPosition += Vector2.up * scrollSpeed * Time.deltaTime;
            yield return null;
        }

        scrollCoroutine = null;
        FinishAndReturn();
    }
}
