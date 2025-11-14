using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Core game manager implementing singleton pattern.
/// Handles spawning, scoring, combo system, dynamic difficulty, and power-ups.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    #region Serialized Fields

    [Header("Spawn Settings")]
    [SerializeField] private List<GameObject> targets;
    [SerializeField] private float spawnRate = 1f;
    [SerializeField] private float minSpawnRate = 0.4f;
    [SerializeField] private float maxSpawnRate = 2f;

    [Header("Combo Settings")]
    [SerializeField] private float comboTimeWindow = 2f;
    [SerializeField] private int comboThreshold_x2 = 3;
    [SerializeField] private int comboThreshold_x3 = 5;
    [SerializeField] private int comboThreshold_x5 = 10;

    [Header("Dynamic Difficulty")]
    [SerializeField] private bool enableDynamicDifficulty = true;
    [SerializeField] private int scoreThresholdEasy = 20;
    [SerializeField] private int scoreThresholdMedium = 50;
    [SerializeField] private int scoreThresholdHard = 100;
    [SerializeField] private float difficultyAdjustSpeed = 0.05f;

    #endregion

    #region Events

    public event System.Action<int> OnScoreChanged;
    public event System.Action OnGameOver;
    public event System.Action<int, int> OnComboChanged;
    public event System.Action<PowerUpType, float> OnPowerUpActivated;
    public event System.Action<PowerUpType> OnPowerUpDeactivated;

    #endregion

    #region Game State

    public bool isGameActive { get; private set; } = true;

    private int score = 0;
    private int currentCombo = 0;
    private int comboMultiplier = 1;
    private float currentSpawnRate;

    #endregion

    #region Difficulty Tracking

    private int consecutiveGoodHits = 0;
    private int consecutiveMisses = 0;

    #endregion

    #region Power-Up State

    private bool isSlowMotionActive = false;
    private bool isDoubleScoreActive = false;
    private bool hasShield = false;
    private bool isTimeFreezeActive = false;

    #endregion

    #region Coroutine References

    private Coroutine spawnCoroutine;
    private Coroutine comboTimerCoroutine;
    private Coroutine slowMotionCoroutine;
    private Coroutine doubleScoreCoroutine;
    private Coroutine timeFreezeCoroutine;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        InitializeSingleton();
        currentSpawnRate = spawnRate;
    }

    private void Start()
    {
        StartSpawning();
    }

    private void Update()
    {
        if (enableDynamicDifficulty && isGameActive)
        {
            AdjustDifficulty();
        }
    }

    #endregion

    #region Singleton

    /// <summary>
    /// Initializes singleton instance. Destroys duplicate instances.
    /// </summary>
    private void InitializeSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    #endregion

    #region Spawning System

    /// <summary>
    /// Starts the target spawning coroutine.
    /// </summary>
    public void StartSpawning()
    {
        if (spawnCoroutine != null) return;

        isGameActive = true;
        spawnCoroutine = StartCoroutine(SpawnTarget());
    }

    /// <summary>
    /// Stops target spawning and deactivates the game.
    /// </summary>
    public void StopSpawning()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }

        isGameActive = false;
    }

    /// <summary>
    /// Spawns targets at intervals determined by current spawn rate.
    /// </summary>
    private IEnumerator SpawnTarget()
    {
        while (isGameActive)
        {
            yield return new WaitForSeconds(currentSpawnRate);

            if (targets == null || targets.Count == 0)
            {
                Debug.LogWarning("Target list is empty or null!");
                continue;
            }

            int randomIndex = Random.Range(0, targets.Count);
            Instantiate(targets[randomIndex], transform.position, Quaternion.identity);
        }
    }

    #endregion

    #region Dynamic Difficulty System

    /// <summary>
    /// Adjusts spawn rate based on player performance and score.
    /// Increases difficulty when player performs well, decreases when struggling.
    /// </summary>
    private void AdjustDifficulty()
    {
        float deltaTime = Time.deltaTime;

        // Score-based difficulty adjustment
        if (score >= scoreThresholdHard)
        {
            currentSpawnRate = Mathf.Max(minSpawnRate, 
                currentSpawnRate - difficultyAdjustSpeed * deltaTime);
        }
        else if (score >= scoreThresholdMedium)
        {
            currentSpawnRate = Mathf.Max(minSpawnRate + 0.2f, 
                currentSpawnRate - difficultyAdjustSpeed * 0.5f * deltaTime);
        }
        else if (score < scoreThresholdEasy)
        {
            currentSpawnRate = Mathf.Min(maxSpawnRate, 
                currentSpawnRate + difficultyAdjustSpeed * deltaTime);
        }

        // Combo-based temporary difficulty spike
        if (currentCombo >= comboThreshold_x5)
        {
            currentSpawnRate = Mathf.Max(minSpawnRate, 
                currentSpawnRate - difficultyAdjustSpeed * 2f * deltaTime);
        }
        else if (currentCombo == 0 && consecutiveMisses >= 3)
        {
            currentSpawnRate = Mathf.Min(maxSpawnRate, 
                currentSpawnRate + difficultyAdjustSpeed * 1.5f * deltaTime);
        }

        currentSpawnRate = Mathf.Clamp(currentSpawnRate, minSpawnRate, maxSpawnRate);
    }

    /// <summary>
    /// Tracks successful hits for difficulty adjustment.
    /// </summary>
    public void OnGoodHit()
    {
        consecutiveGoodHits++;
        consecutiveMisses = 0;
    }

    /// <summary>
    /// Tracks misses and bad hits for difficulty adjustment.
    /// </summary>
    public void OnMissOrBadHit()
    {
        consecutiveMisses++;
        consecutiveGoodHits = 0;
    }

    #endregion

    #region Score System

    /// <summary>
    /// Adds score with combo multiplier and power-up effects applied.
    /// </summary>
    public void AddScore(int amount)
    {
        int finalAmount = amount * comboMultiplier;

        if (isDoubleScoreActive)
        {
            finalAmount *= 2;
        }

        score += finalAmount;
        OnScoreChanged?.Invoke(score);
        CheckGameOver();
    }

    /// <summary>
    /// Decreases score or consumes shield if active.
    /// </summary>
    public void DecreaseScore(int amount)
    {
        if (hasShield)
        {
            ConsumeShield();
            return;
        }

        score = Mathf.Max(0, score - amount);
        OnScoreChanged?.Invoke(score);
        CheckGameOver();
    }

    /// <summary>
    /// Returns the current score.
    /// </summary>
    public int GetScore() => score;

    /// <summary>
    /// Checks if score has reached game over condition.
    /// </summary>
    private void CheckGameOver()
    {
        if (score <= 0 && isGameActive)
        {
            GameOver();
        }
    }

    #endregion

    #region Combo System

    /// <summary>
    /// Increments combo counter and updates multiplier.
    /// Resets the combo timer to maintain the combo chain.
    /// </summary>
    public void AddCombo()
    {
        currentCombo++;
        UpdateComboMultiplier();
        OnComboChanged?.Invoke(currentCombo, comboMultiplier);

        if (comboTimerCoroutine != null)
        {
            StopCoroutine(comboTimerCoroutine);
        }

        comboTimerCoroutine = StartCoroutine(ComboTimer());
    }

    /// <summary>
    /// Resets combo counter and multiplier to default.
    /// </summary>
    public void ResetCombo()
    {
        currentCombo = 0;
        comboMultiplier = 1;
        OnComboChanged?.Invoke(currentCombo, comboMultiplier);

        if (comboTimerCoroutine != null)
        {
            StopCoroutine(comboTimerCoroutine);
            comboTimerCoroutine = null;
        }
    }

    /// <summary>
    /// Updates score multiplier based on current combo count.
    /// </summary>
    private void UpdateComboMultiplier()
    {
        comboMultiplier = currentCombo switch
        {
            >= 10 => 5, // comboThreshold_x5
            >= 5 => 3,  // comboThreshold_x3
            >= 3 => 2,  // comboThreshold_x2
            _ => 1
        };
    }

    /// <summary>
    /// Coroutine that resets combo if no hits occur within the time window.
    /// </summary>
    private IEnumerator ComboTimer()
    {
        yield return new WaitForSeconds(comboTimeWindow);
        ResetCombo();
    }

    #endregion

    #region Game Over

    /// <summary>
    /// Triggers game over state and notifies all listeners.
    /// </summary>
    public void GameOver()
    {
        StopSpawning();
        OnGameOver?.Invoke();
    }

    #endregion

    #region Power-Up System

    /// <summary>
    /// Activates a power-up effect based on type.
    /// </summary>
    public void ActivatePowerUp(PowerUpType type, float duration)
    {
        switch (type)
        {
            case PowerUpType.SlowMotion:
                RestartCoroutine(ref slowMotionCoroutine, SlowMotionEffect(duration));
                break;

            case PowerUpType.DoubleScore:
                RestartCoroutine(ref doubleScoreCoroutine, DoubleScoreEffect(duration));
                break;

            case PowerUpType.Shield:
                ActivateShield();
                break;

            case PowerUpType.TimeFreeze:
                RestartCoroutine(ref timeFreezeCoroutine, TimeFreezeEffect(duration));
                break;
        }
    }

    /// <summary>
    /// Helper method to safely restart a coroutine.
    /// </summary>
    private void RestartCoroutine(ref Coroutine coroutineRef, IEnumerator routine)
    {
        if (coroutineRef != null)
        {
            StopCoroutine(coroutineRef);
        }
        coroutineRef = StartCoroutine(routine);
    }

    /// <summary>
    /// Activates shield power-up that blocks one hit.
    /// </summary>
    private void ActivateShield()
    {
        hasShield = true;
        OnPowerUpActivated?.Invoke(PowerUpType.Shield, 0f);
    }

    /// <summary>
    /// Consumes shield when player takes damage.
    /// </summary>
    private void ConsumeShield()
    {
        hasShield = false;
        OnPowerUpDeactivated?.Invoke(PowerUpType.Shield);
    }

    /// <summary>
    /// Slows down time by 50% for the specified duration.
    /// </summary>
    private IEnumerator SlowMotionEffect(float duration)
    {
        isSlowMotionActive = true;
        Time.timeScale = 0.5f;
        OnPowerUpActivated?.Invoke(PowerUpType.SlowMotion, duration);

        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = 1f;
        isSlowMotionActive = false;
        OnPowerUpDeactivated?.Invoke(PowerUpType.SlowMotion);
    }

    /// <summary>
    /// Doubles all score gains for the specified duration.
    /// </summary>
    private IEnumerator DoubleScoreEffect(float duration)
    {
        isDoubleScoreActive = true;
        OnPowerUpActivated?.Invoke(PowerUpType.DoubleScore, duration);

        yield return new WaitForSeconds(duration);

        isDoubleScoreActive = false;
        OnPowerUpDeactivated?.Invoke(PowerUpType.DoubleScore);
    }

    /// <summary>
    /// Completely freezes time for the specified duration.
    /// </summary>
    private IEnumerator TimeFreezeEffect(float duration)
    {
        isTimeFreezeActive = true;
        Time.timeScale = 0f;
        OnPowerUpActivated?.Invoke(PowerUpType.TimeFreeze, duration);

        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = 1f;
        isTimeFreezeActive = false;
        OnPowerUpDeactivated?.Invoke(PowerUpType.TimeFreeze);
    }

    #endregion

    #region Power-Up State Queries

    public bool IsSlowMotionActive() => isSlowMotionActive;
    public bool IsDoubleScoreActive() => isDoubleScoreActive;
    public bool HasShield() => hasShield;
    public bool IsTimeFreezeActive() => isTimeFreezeActive;

    #endregion
}