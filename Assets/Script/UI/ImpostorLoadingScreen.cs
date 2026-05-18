using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays a full-screen black loading overlay with a progress bar while
/// the ImpostorPhotoBooth processes its capture queue.
/// Attach this to a Canvas GameObject in the scene.
/// </summary>
[RequireComponent(typeof(Canvas))]
public class ImpostorLoadingScreen : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The Image used as the progress bar fill (Image Type: Filled or Simple with stretched anchor).")]
    public Image progressBarFill;

    [Tooltip("Optional text label showing percentage.")]
    public Text percentageLabel;

    [Header("Settings")]
    [Tooltip("Frames à attendre après la fin des captures avant de cacher l'écran. Garantit que le GPU a rendu les impostors.")]
    public int hideDelayFrames = 3;

    private Canvas canvas;
    private bool isSubscribed;

    void Awake()
    {
        canvas = GetComponent<Canvas>();
        canvas.sortingOrder = 999;

        SetVisible(true);
        SetProgress(0f);
    }

    void OnEnable()
    {
        SubscribeToPhotoBooth();
    }

    void OnDisable()
    {
        UnsubscribeFromPhotoBooth();
    }

    void Update()
    {
        if (!isSubscribed)
            SubscribeToPhotoBooth();
    }

    // ──────────────────────────────────────────────
    // Subscription helpers
    // ──────────────────────────────────────────────

    private void SubscribeToPhotoBooth()
    {
        if (isSubscribed) return;

        ImpostorPhotoBooth booth = FindAnyObjectByType<ImpostorPhotoBooth>();
        if (booth == null) return;

        booth.OnCaptureProgressChanged += HandleProgressChanged;
        booth.OnAllCapturesDone += HandleAllDone;
        isSubscribed = true;

        SetProgress(booth.Progress);

        if (booth.IsAllCapturesDone)
            HandleAllDone();
    }

    private void UnsubscribeFromPhotoBooth()
    {
        if (!isSubscribed) return;

        ImpostorPhotoBooth booth = FindAnyObjectByType<ImpostorPhotoBooth>();
        if (booth != null)
        {
            booth.OnCaptureProgressChanged -= HandleProgressChanged;
            booth.OnAllCapturesDone -= HandleAllDone;
        }

        isSubscribed = false;
    }

    // ──────────────────────────────────────────────
    // Event handlers
    // ──────────────────────────────────────────────

    private void HandleProgressChanged(float progress)
    {
        SetProgress(progress);
    }

    private void HandleAllDone()
    {
        SetProgress(1f);
        StartCoroutine(HideAfterFrames());
    }

    // ──────────────────────────────────────────────
    // Hide coroutine — attend N fins de frame pour garantir le rendu GPU
    // ──────────────────────────────────────────────

    private IEnumerator HideAfterFrames()
    {
        for (int i = 0; i < hideDelayFrames; i++)
            yield return new WaitForEndOfFrame();

        SetVisible(false);
    }

    // ──────────────────────────────────────────────
    // Display helpers
    // ──────────────────────────────────────────────

    private void SetVisible(bool visible)
    {
        canvas.enabled = visible;
    }

    /// <summary>Updates the progress bar and optional label. Progress is clamped 0–1.</summary>
    private void SetProgress(float progress)
    {
        progress = Mathf.Clamp01(progress);

        if (progressBarFill != null)
        {
            if (progressBarFill.type == Image.Type.Filled)
                progressBarFill.fillAmount = progress;
            else
                progressBarFill.rectTransform.localScale = new Vector3(progress, 1f, 1f);
        }

        if (percentageLabel != null)
            percentageLabel.text = Mathf.RoundToInt(progress * 100f) + "%";
    }
}
