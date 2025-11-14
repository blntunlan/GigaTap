using UnityEngine;

/// <summary>
/// Defines the different types of targets in the game.
/// </summary>
public enum TargetType
{
    Good,
    Bad,
    PowerUp_SlowMotion,
    PowerUp_DoubleScore,
    PowerUp_Shield,
    PowerUp_TimeFreeze,
    Special_Bomb,
    Special_Moving,
    Special_Tiny,
    Special_Giant
}

/// <summary>
/// Defines available power-up types.
/// </summary>
public enum PowerUpType
{
    None,
    SlowMotion,
    DoubleScore,
    Shield,
    TimeFreeze
}

/// <summary>
/// Handles target behavior including physics, interactions, and special abilities.
/// Supports multiple target types: standard (good/bad), power-ups, and special variants.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class Target : MonoBehaviour
{
    #region Serialized Fields

    [Header("Target Configuration")]
    [SerializeField] private TargetType targetType;
    [SerializeField] private int targetPoint = 1;
    [SerializeField] private ParticleSystem hitParticles;

    [Header("Special Target Settings")]
    [SerializeField] private float movingSpeed = 2f;
    [SerializeField] private float bombRadius = 3f;

    [Header("Force Settings")]
    [SerializeField] private float minUpForce = 12f;
    [SerializeField] private float maxUpForce = 16f;

    [Header("Torque Settings")]
    [SerializeField] private float minTorque = -10f;
    [SerializeField] private float maxTorque = 10f;

    [Header("Spawn Settings")]
    [SerializeField] private float minX = -4f;
    [SerializeField] private float maxX = 4f;
    [SerializeField] private float spawnY = -2f;
    [SerializeField] private float spawnZ = 0f;

    #endregion

    #region Private Fields

    private Rigidbody _rb;
    private int sensorLayerId;
    private Vector3 movingDirection;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        sensorLayerId = LayerMask.NameToLayer("Sensor");
    }

    private void Start()
    {
        InitializeTarget();
    }

    private void Update()
    {
        if (targetType == TargetType.Special_Moving)
        {
            MoveTarget();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == sensorLayerId)
        {
            HandleMissed();
        }
    }

    private void OnMouseDown()
    {
        HandleClick();
    }

    #endregion

    #region Initialization

    /// <summary>
    /// Initializes target position, physics, and special behaviors.
    /// </summary>
    private void InitializeTarget()
    {
        SpawnAtRandomPosition();
        ApplyPhysicsForces();
        ConfigureSpecialBehavior();
    }

    /// <summary>
    /// Spawns target at random X position and applies size modifications.
    /// </summary>
    private void SpawnAtRandomPosition()
    {
        float randomX = Random.Range(minX, maxX);
        transform.position = new Vector3(randomX, spawnY, spawnZ);

        ApplySizeModifier();
    }

    /// <summary>
    /// Applies size scaling for Tiny and Giant target types.
    /// </summary>
    private void ApplySizeModifier()
    {
        switch (targetType)
        {
            case TargetType.Special_Tiny:
                transform.localScale *= 0.5f;
                break;
            case TargetType.Special_Giant:
                transform.localScale *= 2f;
                break;
        }
    }

    /// <summary>
    /// Configures special behaviors like movement direction for moving targets.
    /// </summary>
    private void ConfigureSpecialBehavior()
    {
        if (targetType == TargetType.Special_Moving)
        {
            movingDirection = new Vector3(Random.Range(-1f, 1f), 0, 0).normalized;
        }
    }

    #endregion

    #region Physics

    /// <summary>
    /// Applies upward force and rotational torque to the target.
    /// </summary>
    private void ApplyPhysicsForces()
    {
        ApplyUpwardForce();
        ApplyRandomTorque();
    }

    /// <summary>
    /// Applies random upward impulse force.
    /// </summary>
    private void ApplyUpwardForce()
    {
        float upForce = Random.Range(minUpForce, maxUpForce);
        _rb.AddForce(Vector3.up * upForce, ForceMode.Impulse);
    }

    /// <summary>
    /// Applies random rotational torque on all axes.
    /// </summary>
    private void ApplyRandomTorque()
    {
        Vector3 torque = new Vector3(
            Random.Range(minTorque, maxTorque),
            Random.Range(minTorque, maxTorque),
            Random.Range(minTorque, maxTorque)
        );

        _rb.AddTorque(torque, ForceMode.Impulse);
    }

    /// <summary>
    /// Updates position for moving targets.
    /// </summary>
    private void MoveTarget()
    {
        transform.position += movingDirection * movingSpeed * Time.deltaTime;
    }

    #endregion

    #region Interaction Handling

    /// <summary>
    /// Handles mouse/touch click interaction with the target.
    /// Processes different target types and triggers appropriate game events.
    /// </summary>
    private void HandleClick()
    {
        if (!GameManager.Instance.isGameActive)
            return;

        SpawnHitParticles();
        ProcessTargetInteraction();
    }

    /// <summary>
    /// Spawns hit particle effect at target position.
    /// </summary>
    private void SpawnHitParticles()
    {
        if (hitParticles != null)
        {
            Instantiate(hitParticles, transform.position, Quaternion.identity);
        }
    }

    /// <summary>
    /// Routes interaction to appropriate handler based on target type.
    /// </summary>
    private void ProcessTargetInteraction()
    {
        switch (targetType)
        {
            case TargetType.Good:
                HandleGoodTarget();
                break;

            case TargetType.Bad:
                HandleBadTarget();
                break;

            case TargetType.PowerUp_SlowMotion:
                HandlePowerUp(PowerUpType.SlowMotion, 5f);
                break;

            case TargetType.PowerUp_DoubleScore:
                HandlePowerUp(PowerUpType.DoubleScore, 30f);
                break;

            case TargetType.PowerUp_Shield:
                HandlePowerUp(PowerUpType.Shield, 0f);
                break;

            case TargetType.PowerUp_TimeFreeze:
                HandlePowerUp(PowerUpType.TimeFreeze, 3f);
                break;

            case TargetType.Special_Bomb:
                HandleBombTarget();
                break;

            case TargetType.Special_Moving:
                HandleSpecialTarget(2);
                break;

            case TargetType.Special_Tiny:
                HandleSpecialTarget(3);
                break;

            case TargetType.Special_Giant:
                HandleSpecialTarget(1);
                break;
        }

        Destroy(gameObject);
    }

    #endregion

    #region Target Type Handlers

    /// <summary>
    /// Handles good target hit - awards points and increases combo.
    /// </summary>
    private void HandleGoodTarget()
    {
        var gm = GameManager.Instance;
        gm.AddCombo();
        gm.AddScore(targetPoint);
        gm.OnGoodHit();
    }

    /// <summary>
    /// Handles bad target hit - resets combo and decreases score.
    /// </summary>
    private void HandleBadTarget()
    {
        var gm = GameManager.Instance;
        gm.ResetCombo();
        gm.DecreaseScore(targetPoint);
        gm.OnMissOrBadHit();
    }

    /// <summary>
    /// Handles power-up target hit - activates corresponding power-up.
    /// </summary>
    private void HandlePowerUp(PowerUpType powerUpType, float duration)
    {
        GameManager.Instance.ActivatePowerUp(powerUpType, duration);
    }

    /// <summary>
    /// Handles special target hit with score multiplier.
    /// </summary>
    /// <param name="scoreMultiplier">Multiplier applied to base target points</param>
    private void HandleSpecialTarget(int scoreMultiplier)
    {
        var gm = GameManager.Instance;
        gm.AddCombo();
        gm.AddScore(targetPoint * scoreMultiplier);
        gm.OnGoodHit();
    }

    /// <summary>
    /// Handles bomb target - explodes and destroys nearby targets.
    /// </summary>
    private void HandleBombTarget()
    {
        ExplodeBomb();
        var gm = GameManager.Instance;
        gm.AddCombo();
        gm.AddScore(targetPoint);
    }

    /// <summary>
    /// Finds and destroys all valid targets within bomb radius.
    /// Awards points for each destroyed target.
    /// </summary>
    private void ExplodeBomb()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, bombRadius);

        foreach (Collider col in colliders)
        {
            Target otherTarget = col.GetComponent<Target>();
            if (otherTarget != null && otherTarget != this && IsValidBombTarget(otherTarget))
            {
                GameManager.Instance.AddScore(otherTarget.targetPoint);
                Destroy(otherTarget.gameObject);
            }
        }
    }

    /// <summary>
    /// Checks if target type is valid for bomb explosion.
    /// </summary>
    private bool IsValidBombTarget(Target target)
    {
        return target.targetType switch
        {
            TargetType.Good => true,
            TargetType.Special_Moving => true,
            TargetType.Special_Tiny => true,
            TargetType.Special_Giant => true,
            _ => false
        };
    }

    #endregion

    #region Miss Handling

    /// <summary>
    /// Handles target reaching the sensor (missed by player).
    /// Good targets reset combo and decrease score.
    /// Bad targets have no penalty (correct to ignore them).
    /// </summary>
    private void HandleMissed()
    {
        if (targetType == TargetType.Good)
        {
            var gm = GameManager.Instance;
            gm.ResetCombo();
            gm.DecreaseScore(targetPoint);
            gm.OnMissOrBadHit();
        }

        Destroy(gameObject);
    }

    #endregion

    #region Debug Visualization

    private void OnDrawGizmosSelected()
    {
        if (targetType == TargetType.Special_Bomb)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, bombRadius);
        }
    }

    #endregion
}