using UnityEngine;

/*
 * Base class for all obstacles in Jet Rush.
 * Handles leftward movement and self destruction
 * when the obstacle moves off the left edge of the screen.
 */
public class Obstacle : MonoBehaviour
{
    [Tooltip("How far off screen left before this obstacle is destroyed.")]
    public float destroyXPosition = -20f;

    /*
     * Virtual so child classes can override and extend initialization.
     */
    protected virtual void Start()
    {
        // Base initialization - intentionally empty
    }

    /*
     * Moves the obstacle left every frame using the global scroll speed.
     * Only moves during Playing state.
     * Virtual so child classes can override and extend this behavior.
     */
    protected virtual void Update()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        // Only move during Playing state
        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing)
        {
            return;
        }

        transform.Translate(
            Vector3.left * GameManager.Instance.scrollSpeed * Time.deltaTime
        );

        if (transform.position.x < destroyXPosition)
        {
            Destroy(gameObject);
        }  
    }

    /*
     * Called when this obstacle collides with another collider.
     *
     * @param collision - Data about the collision that occurred.
     */
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("[Obstacle] Hit player: " + gameObject.name);
        }      
    }
}