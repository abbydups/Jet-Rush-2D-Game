using UnityEngine;
using System.Collections;

/*
 * Shakes the camera for a dramatic effect.
 * Called when player dies from obstacle or boundary.
 */
public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    [Header("Shake Settings")]
    [Tooltip("How far the camera moves during shake.")]
    public float shakeMagnitude = 0.2f;

    [Tooltip("How quickly the shake dampens to zero.")]
    public float shakeDampening = 1.5f;

    // Original camera position
    private Vector3 _originalPosition;

    // Whether camera is currently shaking
    private bool _isShaking = false;

    /*
     * Sets up singleton and stores original camera position.
     */
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        _originalPosition = transform.localPosition;
    }

    /*
     * Triggers a camera shake with given duration and magnitude.
     *
     * @param duration  - How long the shake lasts.
     * @param magnitude - How intense the shake is.
     */
    public void Shake(float duration = 0.3f, float magnitude = 0f)
    {
        if (magnitude == 0f)
        {
            magnitude = shakeMagnitude;
        }

        // Stop any existing shake
        StopAllCoroutines();

        StartCoroutine(ShakeCoroutine(duration, magnitude));
    }

    /*
     * Moves camera randomly within magnitude range.
     * Dampens over time until shake is complete.
     * Resets camera to original position when done.
     *
     * @param duration  - How long the shake lasts.
     * @param magnitude - How intense the shake is.
     */
    private IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        _isShaking = true;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            // Dampen magnitude over time
            float currentMagnitude = Mathf.Lerp(
                magnitude,
                0f,
                elapsed / duration
            );

            // Random offset
            float offsetX = Random.Range(-1f, 1f) * currentMagnitude;
            float offsetY = Random.Range(-1f, 1f) * currentMagnitude;

            transform.localPosition = new Vector3(
                _originalPosition.x + offsetX,
                _originalPosition.y + offsetY,
                _originalPosition.z
            );

            yield return null;
        }

        // Reset to original position
        transform.localPosition = _originalPosition;
        _isShaking = false;
    }
}