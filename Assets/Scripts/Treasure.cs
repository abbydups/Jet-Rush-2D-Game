using UnityEngine;

/*
 * Base class for all collectibles in Jet Rush.
 * Handles leftward movement and self destruction
 * when the collectible moves off the left edge of the screen.
 * Uses trigger collision so player passes through to collect.
 */
public class Treasure : MonoBehaviour
{
    [Tooltip("How far off screen left before this collectible is destroyed.")]
    public float destroyXPosition = -20f;

    /*
     * Moves the collectible left every frame using the global scroll speed.
     * Only moves during Playing state.
     * Destroys the collectible when it moves off the left edge of the screen.
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

        // Move left at the global scroll speed
        transform.Translate(Vector3.left * GameManager.Instance.scrollSpeed
            * Time.deltaTime);

        // Destroy when off screen to the left
        if (transform.position.x < destroyXPosition)
        {
            Destroy(gameObject);
        }
    }

    /*
     * Called when player touches this collectible.
     * Override in child classes to add specific collection behavior.
     *
     * @param other - The collider that triggered this event.
     */
    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            OnCollected();
            Destroy(gameObject);
        }
    }

    /*
     * Called when this collectible is collected by the player.
     * Override in child classes to add specific collection behavior.
     */
    protected virtual void OnCollected()
    {
        Debug.Log("[Treasure] Collected: " + gameObject.name);
    }
}