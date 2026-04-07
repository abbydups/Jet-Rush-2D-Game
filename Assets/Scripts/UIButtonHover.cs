using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

/*
 * Makes a button scale up when hovered and back down when unhovered.
 * Attach to any button you want hover effects on.
 */
public class UIButtonHover : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    [Header("Hover Settings")]
    [Tooltip("How big the button gets when hovered.")]
    public float hoverScale = 1.1f;

    [Tooltip("How big the button gets when pressed.")]
    public float pressedScale = 0.95f;

    [Tooltip("How fast the scale animation happens.")]
    public float scaleSpeed = 10f;

    private Vector3 _originalScale;
    private Vector3 _targetScale;
    private Coroutine _scaleCoroutine;

    private void Start()
    {
        _originalScale = transform.localScale;
        _targetScale = _originalScale;
    }

    private void Update()
    {
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            _targetScale,
            scaleSpeed * Time.deltaTime
        );
    }

    /*
     * Called when mouse hovers over button.
     * Scales button up slightly.
     */
    public void OnPointerEnter(PointerEventData eventData)
    {
        _targetScale = _originalScale * hoverScale;
    }

    /*
     * Called when mouse leaves button.
     * Scales button back to normal.
     */
    public void OnPointerExit(PointerEventData eventData)
    {
        _targetScale = _originalScale;
    }

    /*
     * Called when button is pressed.
     * Scales button down slightly for press feel.
     */
    public void OnPointerDown(PointerEventData eventData)
    {
        _targetScale = _originalScale * pressedScale;
    }

    /*
     * Called when button is released.
     * Scales back to hover size.
     */
    public void OnPointerUp(PointerEventData eventData)
    {
        _targetScale = _originalScale * hoverScale;
    }
}