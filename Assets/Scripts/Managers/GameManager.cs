using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

// GameManager is the central orchestrator of the entire game loop.
// It tracks waves, score, difficulty, and tells the various spawners
// when to produce more asteroids, enemies, and collectibles.
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { Playing, GameOver, Win }
    public GameState CurrentState { get; private set; }

    // ─────────────────────────────────────────────
    // WAVE SETTINGS
    // ─────────────────────────────────────────────
    [Header("Wave Settings")]
    [SerializeField] private int collectiblesPerWave = 5;
    [SerializeField] private int totalWaves = 10;
    [SerializeField] private float difficultyIncreaseAmount = 0.15f;

    public int CurrentWave { get; private set; } = 1;
    public float DifficultyMultiplier { get; private set; } = 1f;

    // ─────────────────────────────────────────────
    // SCORE TRACKING
    // ─────────────────────────────────────────────
    private int currentScore = 0;
    private int collectedThisWave = 0;
    private int loadedHighScore = 0;

    // ─────────────────────────────────────────────
    // SPAWNER REFERENCES
    // ─────────────────────────────────────────────
    [Header("Spawner References")]
    [SerializeField] private AsteroidSpawner asteroidSpawner;
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private CollectibleManager collectibleManager;

    // ─────────────────────────────────────────────
    // UI REFERENCES
    // ─────────────────────────────────────────────
    [Header("UI References")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text waveText;
    [SerializeField] private TMP_Text highScoreText;
    [SerializeField] private TMP_Text difficultyAnnouncementText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private TMP_Text finalScoreText;

    // ─────────────────────────────────────────────
    // UNITY LIFECYCLE
    // ─────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Debug.Log("GameManager: Initialized.");
    }

    private void Start()
    {
        SetState(GameState.Playing);

        loadedHighScore = SaveManager.LoadHighScore();
        UpdateHighScoreUI();

        UpdateScoreUI();
        UpdateWaveUI();

        StartWaveOne();
    }

    // ─────────────────────────────────────────────
    // STATE MANAGEMENT
    // ─────────────────────────────────────────────

    private void SetState(GameState newState)
    {
        CurrentState = newState;

        gameOverPanel.SetActive(newState == GameState.GameOver);
        winPanel.SetActive(newState == GameState.Win);

        Debug.Log($"GameManager: Game state changed to {newState}");
    }

    // ─────────────────────────────────────────────
    // WAVE MANAGEMENT
    // ─────────────────────────────────────────────

    private void StartWaveOne()
    {
        Debug.Log("GameManager: Starting Wave 1.");

        asteroidSpawner.SpawnInitialAsteroids();
        enemySpawner.SpawnInitialEnemies();
        collectibleManager.SpawnWaveCollectibles(collectiblesPerWave);
    }

    public void AddScore(int amount = 1)
    {
        currentScore += amount;
        collectedThisWave += amount;

        UpdateScoreUI();

        Debug.Log($"GameManager: Collected scrap. Wave progress: {collectedThisWave}/{collectiblesPerWave}. Total score: {currentScore}");

        CheckWaveCompletion();
    }

    private void CheckWaveCompletion()
    {
        if (collectedThisWave >= collectiblesPerWave)
        {
            if (CurrentWave >= totalWaves)
            {
                Debug.Log("GameManager: Final wave complete! Triggering win.");
                TriggerWin();
            }
            else
            {
                AdvanceToNextWave();
            }
        }
    }

    private void AdvanceToNextWave()
    {
        CurrentWave++;
        collectedThisWave = 0;
        DifficultyMultiplier += difficultyIncreaseAmount;

        UpdateWaveUI();

        Debug.Log($"GameManager: Advancing to Wave {CurrentWave}. Difficulty multiplier now {DifficultyMultiplier:F2}");

        asteroidSpawner.SpawnAdditionalAsteroids();
        enemySpawner.SpawnAdditionalEnemies();
        collectibleManager.SpawnWaveCollectibles(collectiblesPerWave);

        ShowDifficultyAnnouncement($"Wave {CurrentWave} - Incoming!");
    }

    // ─────────────────────────────────────────────
    // UI UPDATES
    // ─────────────────────────────────────────────

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Scrap: {collectedThisWave} / {collectiblesPerWave}";
        }
    }

    private void UpdateWaveUI()
    {
        if (waveText != null)
        {
            waveText.text = $"Wave: {CurrentWave} / {totalWaves}";
        }
    }

    private void UpdateHighScoreUI()
    {
        if (highScoreText != null)
        {
            highScoreText.text = $"High Score: {loadedHighScore}";
        }
    }

    private void ShowDifficultyAnnouncement(string message)
    {
        if (difficultyAnnouncementText == null) return;

        difficultyAnnouncementText.text = message;
        difficultyAnnouncementText.gameObject.SetActive(true);
        StartCoroutine(HideAnnouncementAfterDelay());
    }

    private IEnumerator HideAnnouncementAfterDelay()
    {
        yield return new WaitForSeconds(3f);
        difficultyAnnouncementText.gameObject.SetActive(false);
    }

    // ─────────────────────────────────────────────
    // GAME OVER / WIN / HIGH SCORE
    // ─────────────────────────────────────────────

    public void TriggerGameOver()
    {
        if (CurrentState != GameState.Playing) return;

        Debug.Log("GameManager: Game Over triggered!");
        SetState(GameState.GameOver);

        HandleHighScoreCheck();

        if (finalScoreText != null)
        {
            finalScoreText.text = $"You reached Wave {CurrentWave} with {currentScore} scrap collected.";
        }
    }

    private void TriggerWin()
    {
        Debug.Log("GameManager: All 10 waves complete - player wins!");
        SetState(GameState.Win);

        HandleHighScoreCheck();

        if (finalScoreText != null)
        {
            finalScoreText.text = $"You survived all {totalWaves} waves with {currentScore} scrap collected!";
        }
    }

    // Checks the current score against the saved high score and updates it if needed.
    // Also refreshes the on-screen high score text (top-right) in case it just changed.
    private void HandleHighScoreCheck()
    {
        if (currentScore > loadedHighScore)
        {
            SaveManager.SaveHighScore(currentScore);
            loadedHighScore = currentScore;
            UpdateHighScoreUI();
            Debug.Log($"GameManager: New High Score Saved! {currentScore}");
        }
    }

    public void RestartGame()
    {
        Debug.Log("GameManager: Restarting game...");
        SceneManager.LoadScene(1);
    }

    public void GoToMainMenu()
    {
        Debug.Log("GameManager: Returning to Main Menu...");
        SceneManager.LoadScene(0);
    }
}