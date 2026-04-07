using UnityEngine;

/*
 * Visual bubble that appears around the player when shielded.
 * Follows the player position and scales in smoothly when activated.
 * Blinks rapidly when the shield is about to expire.
 * Disappears when the shield expires.
 */
public class ShieldBubble : MonoBehaviour
{

    [Tooltip("How fast the bubble appears and disappears.")]
    public float scaleSpeed = 5f;

    [Tooltip("The full size of the shield bubble.")]
    public float fullScale = 1.5f;

    [Tooltip("How many seconds before expiry the bubble starts blinking.")]
    public float blinkStartTime = 2f;

    [Tooltip("How fast the bubble blinks when about to expire.")]
    public float blinkSpeed = 8f;

    // Whether the bubble is currently active
    private bool _isActive = false;

    // Target scale for smooth lerping
    private float _targetScale = 0f;

    // Reference to the sprite renderer for blinking
    private SpriteRenderer _spriteRenderer;

    // How much time is left on the shield
    private float _timeRemaining = 0f;

    // Total shield duration for reference
    private float _totalDuration = 0f;

    /*
     * Starts the bubble invisible and gets the sprite renderer.
     */
    private void Start()
    {
        transform.localScale = Vector3.zero;
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    /*
     * Smoothly scales the bubble in or out based on active state.
     * Blinks the bubble when shield is about to expire.
     */
    private void Update()
    {
        // Smoothly scale toward target
        float currentScale = transform.localScale.x;
        float newScale = Mathf.Lerp(
            currentScale,
            _targetScale,
            scaleSpeed * Time.deltaTime
        );
        transform.localScale = new Vector3(newScale, newScale, 1f);

        // Handle blinking when about to expire
        if (_isActive && _spriteRenderer != null)
        {
            if (_timeRemaining <= blinkStartTime && _timeRemaining > 0f)
            {
                // Blink using sine wave for smooth on off effect
                float alpha = (Mathf.Sin(Time.time * blinkSpeed) + 1f) / 2f;
                Color color = _spriteRenderer.color;
                color.a = alpha;
                _spriteRenderer.color = color;
            }
            else
            {
                // Full opacity when not blinking
                Color color = _spriteRenderer.color;
                color.a = 0.5f;
                _spriteRenderer.color = color;
            }
        }
    }

    /*
     * Makes the shield bubble visible by scaling it up.
     * Stores the duration for blink timing.
     *
     * @param duration - How long the shield lasts in seconds.
     */
    public void Show(float duration)
    {
        _isActive = true;
        _targetScale = fullScale;
        _totalDuration = duration;
        _timeRemaining = duration;

        // Reset alpha to half transparent
        if (_spriteRenderer != null)
        {
            Color color = _spriteRenderer.color;
            color.a = 0.5f;
            _spriteRenderer.color = color;
        }
    }

    /*
     * Makes the shield bubble invisible by scaling it down.
     */
    public void Hide()
    {
        _isActive = false;
        _targetScale = 0f;
        _timeRemaining = 0f;

        // Reset alpha
        if (_spriteRenderer != null)
        {
            Color color = _spriteRenderer.color;
            color.a = 0.5f;
            _spriteRenderer.color = color;
        }
    }

    /*
     * Called by PlayerController every frame to keep
     * the bubble in sync with the shield timer.
     *
     * @param timeRemaining - How many seconds are left on the shield.
     */
    public void UpdateTimer(float timeRemaining)
    {
        _timeRemaining = timeRemaining;
    }
}