using UnityEngine;

/*
 * Flashes the warning triangle on and off
 * to warn the player of an incoming drone attack.
 * Attach to the WarningTriangle child of the Drone prefab.
 */
public class WarningTriangleAnimator : MonoBehaviour
{
    [Header("Flash Settings")]
    [Tooltip("How many times per second the triangle flashes.")]
    public float flashRate = 4f;

    private SpriteRenderer _spriteRenderer;
    private float _flashTimer = 0f;
    private bool _isVisible = true;

    /*
     * Gets the SpriteRenderer component.
     */
    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    /*
     * Flashes the triangle on and off at flashRate.
     */
    private void Update()
    {
        _flashTimer += Time.deltaTime;

        if (_flashTimer >= 1f / flashRate)
        {
            _flashTimer = 0f;
            _isVisible = !_isVisible;

            if (_spriteRenderer != null)
            {
                _spriteRenderer.enabled = _isVisible;
            }
        }
    }
}