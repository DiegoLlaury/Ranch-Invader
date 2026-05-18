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
    [Tooltip("Seconds to wait after all captures complete before hiding the screen (lets the last frame render).")]
    public float hideDelay = 0.1f;

    private Canvas canvas;
    private bool allDone;
    private float hideTimer;

    // ──────────────────────────────────────────────
    // Lifecycle
    // ──────────────────────────────────────────────

    void Awake()
    {
        canvas = GetComponent<Canvas>();
        canvas.sortingOrder = 999;

        // Start visible — impostors haven't rendered yet.
        SetVisible(true);
        SetProgress(0f);
    }

    void OnEnable()
    {
        // Hook into the photobooth as soon as it exists.
        // Using a coroutine-free approach: poll once here, then rely on events.
        SubscribeToPhotoBooth();
    }

    void OnDisable()
    {
        UnsubscribeFromPhotoBooth();
    }

    // ──────────────────────────────────────────────
    // Subscription helpers
    // ──────────────────────────────────────────────

    private bool isSubscribed;

    /// <summary>
    /// Subscribes to ImpostorPhotoBooth events.
    /// Called in OnEnable and retried each frame until the singleton is ready.
    /// </summary>
    private void SubscribeToPhotoBooth()
    {
        if (isSubscribed) return;

        // Access the property — this will NOT create the singleton if it doesn't exist yet.
        // We use the backing field check via a try/catch-free method below.
        ImpostorPhotoBooth booth = FindAnyObjectByType<ImpostorPhotoBooth>();
        if (booth == null) return; // Not ready yet — will retry in Update.

        booth.OnCaptureProgressChanged += HandleProgressChanged;
        booth.OnAllCapturesDone += HandleAllDone;
        isSubscribed = true;

        // Sync immediately in case captures already started.
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
    // Update — retry subscription + handle hide delay
    // ──────────────────────────────────────────────

    void Update()
    {
        // Retry subscription each frame until the PhotoBooth singleton has spawned.
        if (!isSubscribed)
            SubscribeToPhotoBooth();

        if (allDone)
        {
            hideTimer -= Time.deltaTime;
            if (hideTimer <= 0f)
                SetVisible(false);
        }
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
        allDone = true;
        hideTimer = hideDelay;
    }

    // ──────────────────────────────────────────────
    // Display helpers
    // ──────────────────────────────────────────────

    private void SetVisible(bool visible)
    {
        canvas.enabled = visible;
    }

    /// <summary>
    /// Updates the progress bar and optional label. Progress is clamped 0–1.
    /// </summary>
    private void SetProgress(float progress)
    {
        progress = Mathf.Clamp01(progress);

        if (progressBarFill != null)
        {
            // Supports both Filled image type and a simple stretched fill rect.
            if (progressBarFill.type == Image.Type.Filled)
                progressBarFill.fillAmount = progress;
            else
                progressBarFill.rectTransform.localScale = new Vector3(progress, 1f, 1f);
        }

        if (percentageLabel != null)
            percentageLabel.text = Mathf.RoundToInt(progress * 100f) + "%";
    }
}
