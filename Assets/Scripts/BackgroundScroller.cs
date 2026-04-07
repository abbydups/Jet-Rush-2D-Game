using UnityEngine;

/*
 * Scrolls a sprite continuously to the left to create the illusion
 * of an infinite moving world. Uses two copies of the same sprite
 * placed side by side so when one scrolls off screen the other
 * takes its place seamlessly.
 */
public class BackgroundScroller : MonoBehaviour
{

    [Header("Scroll Settings")]
    [Tooltip("Multiplier for parallax effect. 1 = full speed, 0.5 = half speed.")]
    public float parallaxMultiplier = 1f;

    [Tooltip("Custom loop width. Leave at 0 to use sprite width automatically.")]
    public float customLoopWidth = 0f;

    [Tooltip("Fallback scroll speed when no GameManager exists.")]
    public float fallbackScrollSpeed = 2f;

    private float _loopWidth;
    private float _startX;
    private GameObject _secondCopy;

    /*
     * Gets sprite width and creates second copy for seamless looping.
     */
    private void Start()
    {
        if (customLoopWidth > 0f)
        {
            _loopWidth = customLoopWidth;
        }
        else
        {
            _loopWidth = GetComponent<SpriteRenderer>().bounds.size.x;
        }

        _startX = transform.position.x;

        _secondCopy = new GameObject(gameObject.name + "_copy");
        _secondCopy.transform.position = new Vector3(
            transform.position.x + _loopWidth,
            transform.position.y,
            transform.position.z
        );

        SpriteRenderer copyRenderer = _secondCopy.AddComponent<SpriteRenderer>();
        SpriteRenderer thisRenderer = GetComponent<SpriteRenderer>();
        copyRenderer.sprite = thisRenderer.sprite;
        copyRenderer.sortingOrder = thisRenderer.sortingOrder;
        copyRenderer.sortingLayerName = thisRenderer.sortingLayerName;

        _secondCopy.transform.localScale = transform.localScale;
    }

    /*
     * Scrolls sprites left and loops when off screen.
     * Only scrolls during Playing state.
     */
    private void Update()
    {
        float scrollSpeed;

        if (GameManager.Instance != null)
        {
            // Only scroll during Playing state
            if (GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            {
                return;
            }

            scrollSpeed = GameManager.Instance.scrollSpeed;
        }
        else
        {
            scrollSpeed = fallbackScrollSpeed;
        }

        float scrollAmount = scrollSpeed * parallaxMultiplier * Time.deltaTime;

        transform.Translate(Vector3.left * scrollAmount);
        _secondCopy.transform.Translate(Vector3.left * scrollAmount);

        if (transform.position.x <= _startX - _loopWidth)
        {
            transform.position = new Vector3(
                _secondCopy.transform.position.x + _loopWidth,
                transform.position.y,
                transform.position.z
            );
        }

        if (_secondCopy.transform.position.x <= _startX - _loopWidth)
        {
            _secondCopy.transform.position = new Vector3(
                transform.position.x + _loopWidth,
                transform.position.y,
                transform.position.z
            );
        }
    }

    /*
     * Cleans up the second copy when destroyed.
     */
    private void OnDestroy()
    {
        if (_secondCopy != null)
            Destroy(_secondCopy);
    }
}