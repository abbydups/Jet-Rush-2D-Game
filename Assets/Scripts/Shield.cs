using UnityEngine;

/*
 * Extends the base Treasure class.
 * When collected activates a shield on the player
 * making them invincible for shieldDuration seconds.
 * Collecting another shield while shielded resets the timer.
 */
public class Shield : Treasure
{
    [Tooltip("How long the shield lasts in seconds.")]
    public float shieldDuration = 5f;

    /*
     * Called when this shield is collected by the player.
     * Finds the PlayerController and activates the shield.
     * Plays shield collect sound.
     */
    protected override void OnCollected()
    {
        // Find player and activate shield
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            player.ActivateShield(shieldDuration);
        }

        // Play shield collect sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayShieldSound();
        }
    }
}