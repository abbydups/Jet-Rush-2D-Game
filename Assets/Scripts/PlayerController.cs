using UnityEngine;
using System.Collections;

/*
 * Handles all player movement, input, death conditions and shield.
 * The player can only move vertically, holding Space thrusts upward,
 * releasing Space lets gravity pull the player back down.
 * Death occurs when the player collides with an obstacle or
 * exits the safe altitude range.
 * Shield makes the player temporarily invincible.
 * Player is frozen and hidden during Menu and Countdown states.
 * Player appears and unfreezes when Playing state begins.
 * Camera shakes on death and landing for dramatic effect.
 * Audio plays for thrust, crash and landing.
 */
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("How strongly the hoverboard thrusts upward.")]
    public float thrustForce = 10f;

    [Tooltip("The fastest the player can move upward.")]
    public float maxUpwardVelocity = 8f;

    [Tooltip("The highest Y position the player is allowed to reach.")]
    public float upperBoundary = 4f;

    [Tooltip("The lowest Y position the player is allowed to reach.")]
    public float lowerBoundary = -4f;

    [Header("Death Settings")]
    [Tooltip("How long to wait before showing the Game Over screen.")]
    public float gameOverDelay = 0.8f;

    [Tooltip("World Y position where the player crashes onto the sand.")]
    public float crashLandingY = -3.0f;

    [Tooltip("Small horizontal push applied on death so the crash feels less stiff.")]
    public float crashHorizontalNudge = 0.08f;

    [Tooltip("Final resting Z rotation in degrees for the crashed body.")]
    public float crashRotationZ = -90f;

    [Tooltip("How fast the player rotates into the crash pose.")]
    public float crashRotateSpeed = 540f;

    [Tooltip("How fast the player moves down to the sand crash position.")]
    public float crashDropSpeed = 8f;

    [Header("Impact Knock")]
    [Tooltip("Horizontal knockback force applied when hitting an obstacle.")]
    public float knockbackX = -3f;

    [Tooltip("Vertical knockback force applied when hitting an obstacle.")]
    public float knockbackY = 4f;

    [Tooltip("How long the knockback lasts before crash sequence begins.")]
    public float knockbackDuration = 0.15f;

    [Header("Camera Shake")]
    [Tooltip("How long the camera shakes on death.")]
    public float deathShakeDuration = 0.4f;

    [Tooltip("How intense the camera shake is on death.")]
    public float deathShakeMagnitude = 0.3f;

    [Tooltip("How long the camera shakes on landing.")]
    public float landingShakeDuration = 0.2f;

    [Tooltip("How intense the camera shake is on landing.")]
    public float landingShakeMagnitude = 0.15f;

    [Header("Crash Resting Pose")]
    [Tooltip("Sprite shown after the player finishes crashing onto the sand.")]
    public Sprite crashedSprite;

    [Header("Impact Bounce")]
    [Tooltip("How high the player bounces upward after hitting the sand.")]
    public float impactBounceHeight = 0.12f;

    [Tooltip("How fast the bounce moves.")]
    public float impactBounceSpeed = 6f;

    [Header("Dust Effect")]
    [Tooltip("Prefab spawned when the player hits the sand.")]
    public GameObject dustImpactPrefab;

    [Tooltip("Horizontal offset for the dust effect relative to the player.")]
    public float dustXOffset = 0.3f;

    [Tooltip("Vertical offset for the dust effect relative to the player.")]
    public float dustYOffset = 0f;

    private bool _isThrusting = false;
    private bool _wasThrustingLastFrame = false;
    private bool _isDead = false;
    private bool _isShielded = false;
    private bool _isCrashing = false;
    private float _shieldTimer = 0f;

    private float _normalGravityScale;
    private Quaternion _targetCrashRotation;
    private Vector3 _targetCrashPosition;
    private Sprite _defaultSprite;

    private Rigidbody2D _rb;
    private Animator _animator;
    private ShieldBubble _shieldBubble;
    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();

        _normalGravityScale = _rb.gravityScale;

        if (_spriteRenderer != null)
        {
            _defaultSprite = _spriteRenderer.sprite;
        }
    }

    private void Start()
    {
        _shieldBubble = GetComponentInChildren<ShieldBubble>();
        FreezePlayer();
    }

    private void Update()
    {
        if (_isDead)
        {
            if (_isCrashing)
            {
                UpdateCrashPose();
            }
            return;
        }

        if (GameManager.Instance != null &&
            GameManager.Instance.CurrentState != GameManager.GameState.Playing)
        {
            FreezePlayer();
            return;
        }

        if (_rb.bodyType == RigidbodyType2D.Kinematic)
        {
            UnfreezePlayer();
            ShowSprite();
        }

        _isThrusting = Input.GetKey(KeyCode.Space);

        // Handle thrust sound
        HandleThrustSound();

        CheckAltitudeBoundaries();

        if (_animator != null)
        {
            _animator.SetBool("isThrusting", _isThrusting);
        }

        if (_isShielded)
        {
            _shieldTimer -= Time.deltaTime;

            if (_shieldBubble != null)
            {
                _shieldBubble.UpdateTimer(_shieldTimer);
            }

            if (_shieldTimer <= 0f)
            {
                _isShielded = false;

                Physics2D.IgnoreLayerCollision(
                    gameObject.layer,
                    LayerMask.NameToLayer("Obstacle"),
                    false
                );

                if (_shieldBubble != null)
                {
                    _shieldBubble.Hide();
                }
            }
        }
        _wasThrustingLastFrame = _isThrusting;
    }

    private void FixedUpdate()
    {
        if (_isDead)
        {
            return;
        }

        if (GameManager.Instance != null &&
            GameManager.Instance.CurrentState != GameManager.GameState.Playing)
        {
            return;
        }

        if (_isThrusting)
        {
            _rb.AddForce(Vector2.up * thrustForce, ForceMode2D.Force);

            if (_rb.linearVelocity.y > maxUpwardVelocity)
            {
                _rb.linearVelocity = new Vector2(
                    _rb.linearVelocity.x,
                    maxUpwardVelocity
                );
            }
        }
    }

    /*
     * Starts thrust sound when Space is pressed.
     * Stops thrust sound when Space is released.
     */
    private void HandleThrustSound()
    {
        if (AudioManager.Instance == null)
        {
            return;
        }

        // Just started thrusting
        if (_isThrusting && !_wasThrustingLastFrame)
        {
            AudioManager.Instance.StartThrust();
        }

        // Just stopped thrusting
        if (!_isThrusting && _wasThrustingLastFrame)
        {
            AudioManager.Instance.StopThrust();
        } 
    }

    private void FreezePlayer()
    {
        _rb.bodyType = RigidbodyType2D.Kinematic;
        _rb.linearVelocity = Vector2.zero;
        _rb.angularVelocity = 0f;
        HideSprite();
    }

    private void UnfreezePlayer()
    {
        _rb.bodyType = RigidbodyType2D.Dynamic;
        _rb.gravityScale = _normalGravityScale;
        _rb.linearVelocity = Vector2.zero;
        _rb.angularVelocity = 0f;
        transform.rotation = Quaternion.identity;

        if (_animator != null)
        {
            _animator.enabled = true;
        }
    }

    private void HideSprite()
    {
        if (_spriteRenderer != null)
        {
            _spriteRenderer.enabled = false;
        }
    }

    private void ShowSprite()
    {
        if (_spriteRenderer != null)
        {
            _spriteRenderer.enabled = true;
        }
    }

    private void CheckAltitudeBoundaries()
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.CurrentState != GameManager.GameState.Playing)

        {
            return;
        }

        if (transform.position.y > upperBoundary ||
            transform.position.y < lowerBoundary)
        {
            Die();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.CurrentState != GameManager.GameState.Playing)

        {
            return;
        }

        if (collision.gameObject.CompareTag("Obstacle"))
        {
            Die();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Treasure"))
        {
            Debug.Log("[PlayerController] Treasure collected.");
        }
    }

    /*
     * Triggers the full death sequence.
     * Shakes camera and plays crash sound on impact.
     */
    private void Die()
    {
        if (_isShielded)
        {
            return;
        }

        if (_isDead)
        {
            return;
        }

        _isDead = true;
        _isCrashing = true;
        _isThrusting = false;

        // Stop thrust sound on death
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopThrust();
        }

        // Play crash sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayCrashSound();
        }

        // Shake camera on death impact
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(deathShakeDuration, deathShakeMagnitude);
        }

        if (_animator != null)
        {
            _animator.enabled = true;
            _animator.SetBool("isThrusting", false);
            _animator.SetTrigger("Die");
        }

        _rb.linearVelocity = Vector2.zero;
        _rb.angularVelocity = 0f;
        _rb.gravityScale = 0f;
        _rb.bodyType = RigidbodyType2D.Kinematic;

        _targetCrashPosition = new Vector3(
            transform.position.x + crashHorizontalNudge,
            crashLandingY,
            transform.position.z
        );

        _targetCrashRotation = Quaternion.Euler(0f, 0f, crashRotationZ);

        StartCoroutine(HandleDeathSequence());
    }

    private void UpdateCrashPose()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            _targetCrashPosition,
            crashDropSpeed * Time.deltaTime
        );

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            _targetCrashRotation,
            crashRotateSpeed * Time.deltaTime
        );
    }

    /*
     * Full death sequence with audio:
     * 1. Crash sound plays on impact
     * 2. Brief knockback jolt
     * 3. Player drops and rotates to crash position
     * 4. Dust spawns on landing
     * 5. Landing sound plays
     * 6. Camera shakes on landing
     * 7. Small bounce on landing
     * 8. Game over screen appears
     */
    private IEnumerator HandleDeathSequence()
    {
        // Brief knockback jolt
        _rb.bodyType = RigidbodyType2D.Dynamic;
        _rb.gravityScale = 0f;
        _rb.linearVelocity = new Vector2(knockbackX, knockbackY);

        yield return new WaitForSeconds(knockbackDuration);

        // Freeze and begin crash drop
        _rb.bodyType = RigidbodyType2D.Kinematic;
        _rb.linearVelocity = Vector2.zero;
        _rb.angularVelocity = 0f;

        // Move toward crash landing position
        while (Vector3.Distance(transform.position,
            _targetCrashPosition) > 0.01f)
        {
            yield return null;
        }

        _isCrashing = false;
        transform.position = _targetCrashPosition;
        transform.rotation = _targetCrashRotation;

        // Spawn dust at feet position
        SpawnDustImpact();

        // Play landing sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayLandingSound();
        }

        // Shake camera on landing
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(
                landingShakeDuration, landingShakeMagnitude);
        }

        // Tiny bounce upward on landing
        Vector3 bouncePeak = new Vector3(
            _targetCrashPosition.x,
            _targetCrashPosition.y + impactBounceHeight,
            _targetCrashPosition.z
        );

        while (Vector3.Distance(transform.position, bouncePeak) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                bouncePeak,
                impactBounceSpeed * Time.deltaTime
            );
            yield return null;
        }

        while (Vector3.Distance(transform.position,
            _targetCrashPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                _targetCrashPosition,
                impactBounceSpeed * Time.deltaTime
            );
            yield return null;
        }

        // Short pause so impact reads visually
        yield return new WaitForSeconds(gameOverDelay);

        // Stop animator and force final crashed pose
        if (_animator != null)
        {
            _animator.enabled = false;
        }

        if (_spriteRenderer != null && crashedSprite != null)
        {
            _spriteRenderer.sprite = crashedSprite;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
    }

    private void SpawnDustImpact()
    {
        if (dustImpactPrefab == null)
        {
            return;
        }

        Vector3 dustPosition = new Vector3(
            transform.position.x + dustXOffset,
            transform.position.y + dustYOffset,
            transform.position.z
        );

        Instantiate(dustImpactPrefab, dustPosition, Quaternion.identity);
    }

    public void ActivateShield(float duration)
    {
        _isShielded = true;
        _shieldTimer = duration;

        if (_shieldBubble != null)
        {
            _shieldBubble.Show(duration);
        }
            
        Physics2D.IgnoreLayerCollision(
            gameObject.layer,
            LayerMask.NameToLayer("Obstacle"),
            true
        );
    }

    public void ResetPlayer(Vector3 startPosition)
    {
        StopAllCoroutines();

        _isDead = false;
        _isShielded = false;
        _isCrashing = false;
        _shieldTimer = 0f;
        _isThrusting = false;
        _wasThrustingLastFrame = false;

        // Stop thrust sound on reset
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopThrust();
        }

        _rb.bodyType = RigidbodyType2D.Dynamic;
        _rb.gravityScale = _normalGravityScale;
        _rb.linearVelocity = Vector2.zero;
        _rb.angularVelocity = 0f;

        transform.position = startPosition;
        transform.rotation = Quaternion.identity;

        Physics2D.IgnoreLayerCollision(
            gameObject.layer,
            LayerMask.NameToLayer("Obstacle"),
            false
        );

        if (_shieldBubble != null)
        {
            _shieldBubble.Hide();
        }

        if (_spriteRenderer != null && _defaultSprite != null)
        {
            _spriteRenderer.sprite = _defaultSprite;
        }

        if (_animator != null)
        {
            _animator.enabled = true;
            _animator.ResetTrigger("Die");
            _animator.SetBool("isThrusting", false);
            _animator.Play("Idle", 0, 0f);
        }
    }
}