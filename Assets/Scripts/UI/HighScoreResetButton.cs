using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Main Menu button that resets the saved high score.
/// Uses a two-click confirmation: the first click asks "Are you sure?",
/// the second click (within confirmWindow seconds) performs the reset.
/// After resetting, it also refreshes the Main Menu high score text
/// so the player sees 0 immediately.
/// </summary>
public class HighScoreResetButton : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button resetButton;
    [Tooltip("Text on the button. If left empty, it is found automatically.")]
    [SerializeField] private TMP_Text buttonLabel;
    [Tooltip("Optional text that shows 'High score reset!' after a reset.")]
    [SerializeField] private TMP_Text feedbackText;
    [Tooltip("The high score text on the Main Menu. Updated right after a reset.")]
    [SerializeField] private TMP_Text highScoreText;

    [Header("Settings")]
    [SerializeField] private string defaultLabel = "Reset High Score";
    [SerializeField] private string confirmLabel = "Click again to confirm";
    [SerializeField] private string feedbackMessage = "High score reset!";
    [Tooltip("{0} is replaced by the score. Match this to how your menu text is written.")]
    [SerializeField] private string highScoreFormat = "High Score: {0}";
    [SerializeField] private float confirmWindow = 3f;
    [SerializeField] private float feedbackDuration = 2f;

    private bool awaitingConfirm;
    private Coroutine confirmRoutine;
    private Coroutine feedbackRoutine;

    private void Awake()
    {
        // Find the button on this object if it was not assigned.
        if (resetButton == null)
            resetButton = GetComponent<Button>();

        // Find the label inside the button if it was not assigned.
        if (buttonLabel == null && resetButton != null)
            buttonLabel = resetButton.GetComponentInChildren<TMP_Text>();

        if (resetButton != null)
            resetButton.onClick.AddListener(OnResetClicked);
        else
            Debug.LogWarning("HighScoreResetButton: no Button assigned.");

        if (feedbackText != null)
            feedbackText.gameObject.SetActive(false);

        SetLabel(defaultLabel);
    }

    private void OnDestroy()
    {
        // Good practice: remove the listener we added.
        if (resetButton != null)
            resetButton.onClick.RemoveListener(OnResetClicked);
    }

    // Called every time the button is clicked.
    private void OnResetClicked()
    {
        if (!awaitingConfirm)
        {
            // First click: ask for confirmation and start a timer.
            awaitingConfirm = true;
            SetLabel(confirmLabel);
            confirmRoutine = StartCoroutine(ConfirmTimeout());
            Debug.Log("HighScoreResetButton: waiting for confirmation click.");
        }
        else
        {
            // Second click: actually reset.
            PerformReset();
        }
    }

    // Custom method: does the reset, refreshes the text and shows feedback.
    private void PerformReset()
    {
        if (confirmRoutine != null)
            StopCoroutine(confirmRoutine);

        SaveManager.ResetHighScore();
        RefreshHighScoreText();

        awaitingConfirm = false;
        SetLabel(defaultLabel);

        if (feedbackText != null)
        {
            if (feedbackRoutine != null)
                StopCoroutine(feedbackRoutine);
            feedbackRoutine = StartCoroutine(ShowFeedback());
        }
    }

    // Custom method: re-reads the saved score (now 0) and updates the menu text.
    private void RefreshHighScoreText()
    {
        if (highScoreText == null)
        {
            Debug.LogWarning("HighScoreResetButton: no High Score Text assigned, so it will not update live.");
            return;
        }

        highScoreText.text = string.Format(highScoreFormat, SaveManager.LoadHighScore());
    }

    // If the player doesn't confirm in time, go back to normal.
    private IEnumerator ConfirmTimeout()
    {
        yield return new WaitForSecondsRealtime(confirmWindow);
        awaitingConfirm = false;
        SetLabel(defaultLabel);
        Debug.Log("HighScoreResetButton: confirmation timed out.");
    }

    // Show the feedback message briefly, then hide it.
    private IEnumerator ShowFeedback()
    {
        feedbackText.text = feedbackMessage;
        feedbackText.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(feedbackDuration);
        feedbackText.gameObject.SetActive(false);
    }

    private void SetLabel(string text)
    {
        if (buttonLabel != null)
            buttonLabel.text = text;
    }
}