using UnityEngine;

/*
 * Extends the base Obstacle class to add a warning phase
 * followed by launching straight left toward the player.
 * Phase 1 - Warning: Triangle warning sign shows at fixed screen position
 * Phase 2 - Attack: Drone launches straight left at locked Y position
 */
public class DroneObstacle : Obstacle
{
    [Header("Drone Settings")]
    [Tooltip("How many seconds the warning shows before the drone launches.")]
    public float warningDuration = 2f;

    [Tooltip("The warning triangle child GameObject.")]
    public GameObject warningIndicator;

    [Tooltip("How long the drone sound fades out when destroyed.")]
    public float soundFadeDuration = 0.5f;

    private float _warningTimer;
    private bool _isWarning = true;
    private float _lockedY;
    private SpriteRenderer _spriteRenderer;
    private bool _warningSoundPlayed = false;
    private bool _attackSoundPlayed = false;

    /*
     * Locks Y position, shows warning triangle, hides drone sprite.
     * Plays warning sound.
     */
    protected override void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _lockedY = transform.position.y;
        _warningTimer = warningDuration;

        // Hide drone sprite during warning phase
        if (_spriteRenderer != null)
        {
            _spriteRenderer.enabled = false;
        }

        // Show warning triangle
        if (warningIndicator != null)
        {
            warningIndicator.SetActive(true);
        }
        else
        {
            Debug.Log("[DroneObstacle] WARNING: No warning indicator assigned!");
        }

        // Play warning sound once
        if (!_warningSoundPlayed && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayDroneWarning();
            _warningSoundPlayed = true;
        }
    }

    /*
     * Counts down warning timer then launches drone straight left.
     * Plays attack sound when drone launches.
     * Destroys drone when off screen.
     */
    protected override void Update()
    {
        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing)
        {
            return;
        }

        if (_isWarning)
        {
            // Keep triangle at far right edge of screen
            if (warningIndicator != null)
            {
                warningIndicator.transform.position = new Vector3(
                    8f,
                    _lockedY,
                    0f
                );
            }

            _warningTimer -= Time.deltaTime;

            if (_warningTimer <= 0f)
            {
                _isWarning = false;

                if (warningIndicator != null)
                {
                    warningIndicator.SetActive(false);
                }

                if (_spriteRenderer != null)
                {
                    _spriteRenderer.enabled = true;
                }

                transform.position = new Vector3(
                    10f,
                    _lockedY,
                    0f
                );

                // Play attack sound once when drone launches
                if (!_attackSoundPlayed && AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayDroneAttack();
                    _attackSoundPlayed = true;
                }
            }
        }
        else
        {
            // Move drone left at 2x scroll speed
            transform.Translate(
                Vector3.left * GameManager.Instance.scrollSpeed
                    * 2f * Time.deltaTime
            );

            // Lock Y position so drone flies perfectly straight
            transform.position = new Vector3(
                transform.position.x,
                _lockedY,
                transform.position.z
            );

            // Destroy when off screen
            if (transform.position.x < destroyXPosition)
            {
                Destroy(gameObject);
                return;
            }

            // Safety destroy if way off screen
            if (transform.position.x < -30f)
            {
                Debug.Log("[Drone] Safety destroy triggered!");
                Destroy(gameObject);
            }
        }
    }

    /*
     * Fades out drone attack sound when drone is destroyed.
     */
    private void OnDestroy()
    {

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.FadeDroneAttack(soundFadeDuration);
        }  
    }
}