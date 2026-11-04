using UnityEngine;

// SaveManager handles saving and loading persistent data using PlayerPrefs.
// PlayerPrefs stores small amounts of data on the player's device between
// play sessions (and between scenes, since the data is written to disk).
// This class is STATIC because it holds no state of its own - it is simply
// a reusable toolbox of save/load methods that any script can call directly
// without needing a GameObject reference, e.g. SaveManager.LoadHighScore()
public static class SaveManager
{
    // The key PlayerPrefs uses to store and retrieve the high score value
    // Using a constant prevents typos if we reference this string elsewhere
    private const string HighScoreKey = "HighScore";

    // Saves the given score as the new high score
    public static void SaveHighScore(int score)
    {
        PlayerPrefs.SetInt(HighScoreKey, score);
        PlayerPrefs.Save(); // Forces an immediate write to disk

        Debug.Log($"SaveManager: New High Score Saved - {score}");
    }

    // Loads the saved high score. Returns 0 if no high score has been saved yet.
    public static int LoadHighScore()
    {
        // GetInt's second parameter (0) is the default value if the key doesn't exist yet
        int savedScore = PlayerPrefs.GetInt(HighScoreKey, 0);
        Debug.Log($"SaveManager: High score loaded - {savedScore}");
        return savedScore;
    }

    // Utility method in case you want to reset progress during testing
    public static void ResetHighScore()
    {
        PlayerPrefs.DeleteKey(HighScoreKey);
        Debug.Log("SaveManager: High score reset.");
    }
}