using UnityEngine;
using UnityEngine.EventSystems;

/*
 * Makes the Play button look like its about to take off when hovered.
 * Button shakes side to side rapidly like an engine revving up
 * then shoots upward when clicked.
 * Plays hover sound when mouse enters.
 * Stops hover sound when mouse leaves.
 * Plays click sound when button is pressed.
 */
public class UIPlayButtonEffect : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler
{

    [Header("Idle Settings")]
    [Tooltip("How much the button bobs when not hovered.")]
    public float idleBobHeight = 5f;

    [Tooltip("How fast the button bobs when not hovered.")]
    public float idleBobSpeed = 1.5f;

    [Header("Hover Settings")]
    [Tooltip("How much the button shakes side to side when hovered.")]
    public float hoverShakeAmount = 4f;

    [Tooltip("How fast the button shakes when hovered.")]
    public float hoverShakeSpeed = 25f;

    [Tooltip("How much the button scales up when hovered.")]
    public float hoverScale = 1.08f;

    [Header("Press Settings")]
    [Tooltip("How fast the button shoots up when pressed.")]
    public float launchSpeed = 15f;

    [Tooltip("How far the button shoots up when pressed.")]
    public float launchHeight = 30f;

    private Vector3 _originalPosition;
    private Vector3 _originalScale;
    private bool _isHovered = false;
    private bool _isPressed = false;
    private float _shakeTimer = 0f;

    private void Start()
    {
        _originalPosition = transform.localPosition;
        _originalScale = transform.localScale;
    }

    private void Update()
    {
        if (_isPressed)
        {
            return;
        }

        if (_isHovered)
        {
            // Shake side to side like engine revving
            _shakeTimer += Time.deltaTime;

            float shakeX = Mathf.Sin(_shakeTimer * hoverShakeSpeed)
                * hoverShakeAmount;

            transform.localPosition = new Vector3(
                _originalPosition.x + shakeX,
                _originalPosition.y,
                _originalPosition.z
            );

            // Scale up
            transform.localScale = Vector3.Lerp(
                transform.localScale,
                _originalScale * hoverScale,
                10f * Time.deltaTime
            );
        }
        else
        {
            // Gentle idle bob when not hovered
            float newY = _originalPosition.y
                + Mathf.Sin(Time.time * idleBobSpeed) * idleBobHeight;

            transform.localPosition = new Vector3(
                _originalPosition.x,
                newY,
                _originalPosition.z
            );

            // Scale back to normal
            transform.localScale = Vector3.Lerp(
                transform.localScale,
                _originalScale,
                10f * Time.deltaTime
            );
        }
    }

    /*
     * Start shaking like engine revving up.
     * Plays hover sound.
     */
    public void OnPointerEnter(PointerEventData eventData)
    {
        _isHovered = true;
        _shakeTimer = 0f;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonHover();
        }
    }

    /*
     * Stop shaking go back to idle bob.
     * Stops hover sound immediately.
     */
    public void OnPointerExit(PointerEventData eventData)
    {
        _isHovered = false;
        _isPressed = false;
        transform.localPosition = _originalPosition;

        // Stop hover sound when mouse leaves
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopButtonHover();
        }
    }

    /*
     * Button shoots upward like taking off!
     * Plays click sound.
     */
    public void OnPointerDown(PointerEventData eventData)
    {
        _isPressed = true;

        // Stop hover sound and play click sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopButtonHover();
        }

        StartCoroutine(LaunchEffect());
    }

    /*
     * Reset after launch.
     */
    public void OnPointerUp(PointerEventData eventData)
    {
        _isPressed = false;
        transform.localPosition = _originalPosition;
        transform.localScale = _originalScale;
    }

    /*
     * Shoots the button upward when clicked
     * then snaps back to original position.
     */
    private System.Collections.IEnumerator LaunchEffect()
    {
        Vector3 launchTarget = new Vector3(
            _originalPosition.x,
            _originalPosition.y + launchHeight,
            _originalPosition.z
        );

        float elapsed = 0f;
        float duration = 0.15f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            transform.localPosition = Vector3.Lerp(
                _originalPosition,
                launchTarget,
                t
            );

            yield return null;
        }

        transform.localPosition = _originalPosition;
        _isPressed = false;
    }
}