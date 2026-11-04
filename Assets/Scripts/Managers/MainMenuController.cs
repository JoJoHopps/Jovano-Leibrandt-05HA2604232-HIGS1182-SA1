using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

// MainMenuController handles button functionality and displays the saved high score.
public class MainMenuController : MonoBehaviour
{
    [SerializeField] private TMP_Text highScoreText;

    private void Start()
    {
        DisplayHighScore();
    }

    // Loads and displays the saved high score when the menu appears
    private void DisplayHighScore()
    {
        int savedHighScore = SaveManager.LoadHighScore();

        if (highScoreText != null)
        {
            highScoreText.text = $"High Score: {savedHighScore}";
        }

        Debug.Log($"MainMenuController: Displaying high score - {savedHighScore}");
    }

    public void StartGame()
    {
        Debug.Log("MainMenuController: Starting game...");
        SceneManager.LoadScene(1);
    }

    public void QuitGame()
    {
        Debug.Log("MainMenuController: Quitting game...");
        Application.Quit();
    }
}