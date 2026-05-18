using System;
using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Displays dialogue lines one at a time with a typewriter effect.
/// Fires OnDialogueComplete when all lines have been shown.
/// </summary>
public class DialogueDisplay : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI speakerText;
    [SerializeField] private TextMeshProUGUI bodyText;

    [Header("Typewriter Settings")]
    [SerializeField] private float charactersPerSecond = 30f;
    [SerializeField] private float lingerDurationAfterLine = 1.5f;

    public event Action OnDialogueComplete;

    private Coroutine _activeCoroutine;

    private void Awake()
    {
        HidePanel();
    }

    /// <summary>Starts displaying the provided dialogue lines sequentially.</summary>
    public void StartDialogue(DialogueLine[] lines)
    {
        if (_activeCoroutine != null)
            StopCoroutine(_activeCoroutine);

        _activeCoroutine = StartCoroutine(RunDialogue(lines));
    }

    private IEnumerator RunDialogue(DialogueLine[] lines)
    {
        dialoguePanel.SetActive(true);

        foreach (DialogueLine line in lines)
        {
            speakerText.text = line.SpeakerName;
            bodyText.text = string.Empty;

            float delay = 1f / Mathf.Max(charactersPerSecond, 1f);
            foreach (char c in line.Text)
            {
                bodyText.text += c;
                yield return new WaitForSeconds(delay);
            }

            yield return new WaitForSeconds(lingerDurationAfterLine);
        }

        HidePanel();
        OnDialogueComplete?.Invoke();
    }

    private void HidePanel()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }
}

[Serializable]
public struct DialogueLine
{
    public string SpeakerName;
    [TextArea(2, 5)]
    public string Text;
}
