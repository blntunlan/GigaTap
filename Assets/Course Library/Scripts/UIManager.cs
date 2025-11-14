using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

/// <summary>
/// Manages all UI elements and their updates based on game events.
/// Handles score display, combo feedback, game over screen, and power-up indicators.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("Core UI Elements")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text gameoverText;
    [SerializeField] private TMP_Text comboText;
    [SerializeField] private UnityEngine.UI.Button restartButton;

    [Header("Power-Up UI")]
    [SerializeField] private TMP_Text powerUpText;
    [SerializeField] private UnityEngine.UI.Image shieldIcon;

    [Header("Combo Colors")]
    [SerializeField] private Color comboColorX2 = Color.yellow;
    [SerializeField] private Color comboColorX3 = new Color(1f, 0.5f, 0f); // Orange
    [SerializeField] private Color comboColorX5 = new Color(1f, 0f, 1f); // Magenta

    private Coroutine powerUpTextCoroutine;

    #region Unity Lifecycle

    private void OnEnable()
    {
        SubscribeToEvents();
    }

    private void Start()
    {
        InitializeUI();
        SubscribeToEvents();
    }

    private void OnDisable()
    {
        UnsubscribeFromEvents();
    }

    #endregion

    #region Initialization

    /// <summary>
    /// Initializes UI elements to their default state at game start.
    /// </summary>
    private void InitializeUI()
    {
        // Initialize score display
        if (scoreText != null && GameManager.Instance != null)
        {
            scoreText.text = $"Score: {GameManager.Instance.GetScore()}";
        }

        // Hide combo text initially
        if (comboText != null)
        {
            comboText.gameObject.SetActive(false);
        }

        // Setup restart button
        if (restartButton != null)
        {
            restartButton.gameObject.SetActive(false);
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(RestartGame);
        }

        // Hide power-up UI elements
        if (powerUpText != null)
        {
            powerUpText.gameObject.SetActive(false);
        }

        if (shieldIcon != null)
        {
            shieldIcon.gameObject.SetActive(false);
        }
    }

    #endregion

    #region Event Subscription

    /// <summary>
    /// Subscribes to GameManager events. Safe to call multiple times.
    /// </summary>
    private void SubscribeToEvents()
    {
        if (GameManager.Instance == null) return;

        var gameManager = GameManager.Instance;
        
        // Unsubscribe first to prevent duplicate subscriptions
        gameManager.OnScoreChanged -= UpdateScoreUI;
        gameManager.OnGameOver -= GameOver;
        gameManager.OnComboChanged -= UpdateComboUI;
        gameManager.OnPowerUpActivated -= OnPowerUpActivated;
        gameManager.OnPowerUpDeactivated -= OnPowerUpDeactivated;

        // Subscribe to events
        gameManager.OnScoreChanged += UpdateScoreUI;
        gameManager.OnGameOver += GameOver;
        gameManager.OnComboChanged += UpdateComboUI;
        gameManager.OnPowerUpActivated += OnPowerUpActivated;
        gameManager.OnPowerUpDeactivated += OnPowerUpDeactivated;
    }

    /// <summary>
    /// Unsubscribes from all GameManager events.
    /// </summary>
    private void UnsubscribeFromEvents()
    {
        if (GameManager.Instance == null) return;

        var gameManager = GameManager.Instance;
        gameManager.OnScoreChanged -= UpdateScoreUI;
        gameManager.OnGameOver -= GameOver;
        gameManager.OnComboChanged -= UpdateComboUI;
        gameManager.OnPowerUpActivated -= OnPowerUpActivated;
        gameManager.OnPowerUpDeactivated -= OnPowerUpDeactivated;
    }

    #endregion

    #region UI Update Methods

    /// <summary>
    /// Updates the score display when score changes.
    /// </summary>
    private void UpdateScoreUI(int value)
    {
        if (scoreText == null)
        {
            Debug.LogWarning("ScoreText reference is missing!", this);
            return;
        }

        scoreText.text = $"Score: {value}";
    }

    /// <summary>
    /// Updates combo UI with current combo count and multiplier.
    /// Changes color based on multiplier level.
    /// </summary>
    private void UpdateComboUI(int combo, int multiplier)
    {
        if (comboText == null) return;

        if (combo <= 0)
        {
            comboText.gameObject.SetActive(false);
            return;
        }

        comboText.gameObject.SetActive(true);
        comboText.text = $"COMBO x{combo}";

        // Set color based on multiplier tier
        comboText.color = multiplier switch
        {
            >= 5 => comboColorX5,
            >= 3 => comboColorX3,
            >= 2 => comboColorX2,
            _ => Color.white
        };
    }

    /// <summary>
    /// Displays game over screen and shows restart button.
    /// </summary>
    private void GameOver()
    {
        if (gameoverText != null)
        {
            gameoverText.gameObject.SetActive(true);
            gameoverText.text = "GAME OVER!";
        }

        if (scoreText != null)
        {
            scoreText.gameObject.SetActive(false);
        }

        if (comboText != null)
        {
            comboText.gameObject.SetActive(false);
        }

        if (restartButton != null)
        {
            restartButton.gameObject.SetActive(true);
        }
    }

    #endregion

    #region Power-Up UI

    /// <summary>
    /// Shows power-up activation feedback based on type.
    /// </summary>
    private void OnPowerUpActivated(PowerUpType type, float duration)
    {
        switch (type)
        {
            case PowerUpType.SlowMotion:
                ShowPowerUpText("⏱️ SLOW MOTION", Color.cyan, duration);
                break;

            case PowerUpType.DoubleScore:
                ShowPowerUpText("💰 DOUBLE SCORE!", Color.yellow, duration);
                break;

            case PowerUpType.Shield:
                if (shieldIcon != null)
                {
                    shieldIcon.gameObject.SetActive(true);
                }
                break;

            case PowerUpType.TimeFreeze:
                ShowPowerUpText("❄️ TIME FREEZE!", new Color(0.5f, 0.8f, 1f), duration);
                break;
        }
    }

    /// <summary>
    /// Hides power-up UI when effect ends.
    /// </summary>
    private void OnPowerUpDeactivated(PowerUpType type)
    {
        if (type == PowerUpType.Shield && shieldIcon != null)
        {
            shieldIcon.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Displays power-up text with specified message and color.
    /// Automatically hides after duration expires.
    /// </summary>
    private void ShowPowerUpText(string message, Color color, float duration)
    {
        if (powerUpText == null) return;

        // Stop any existing coroutine to prevent conflicts
        if (powerUpTextCoroutine != null)
        {
            StopCoroutine(powerUpTextCoroutine);
        }

        powerUpText.text = message;
        powerUpText.color = color;
        powerUpText.gameObject.SetActive(true);

        powerUpTextCoroutine = StartCoroutine(HidePowerUpTextAfter(duration));
    }

    /// <summary>
    /// Coroutine to hide power-up text after specified duration.
    /// Uses WaitForSecondsRealtime to work with Time.timeScale changes.
    /// </summary>
    private IEnumerator HidePowerUpTextAfter(float duration)
    {
        yield return new WaitForSecondsRealtime(duration);
        
        if (powerUpText != null)
        {
            powerUpText.gameObject.SetActive(false);
        }

        powerUpTextCoroutine = null;
    }

    #endregion

    #region Game Control

    /// <summary>
    /// Restarts the current scene and resets time scale.
    /// </summary>
    private void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    #endregion
}