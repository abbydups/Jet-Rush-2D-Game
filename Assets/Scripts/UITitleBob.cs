using UnityEngine;

/*
 * Makes the title bob up and down continuously.
 * Attach to the JET RUSH title GameObject.
 */
public class UITitleBob : MonoBehaviour
{
    [Header("Bob Settings")]
    [Tooltip("How high the title bobs up and down.")]
    public float bobHeight = 20f;

    [Tooltip("How fast the title bobs.")]
    public float bobSpeed = 2f;

    private Vector3 _startPosition;

    private void Start()
    {
        _startPosition = transform.localPosition;
    }

    private void Update()
    {
        float newY = _startPosition.y
            + Mathf.Sin(Time.time * bobSpeed) * bobHeight;

        transform.localPosition = new Vector3(
            _startPosition.x,
            newY,
            _startPosition.z
        );
    }
}