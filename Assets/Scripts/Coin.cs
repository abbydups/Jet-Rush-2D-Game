using UnityEngine;

/*
 * Extends the base Treasure class.
 * When collected adds to the player's coin count via ScoreManager.
 * Plays coin sound via AudioManager.
 */
public class Coin : Treasure
{
    [Tooltip("How many coins this collectible is worth.")]
    public int coinValue = 1;

    /*
     * Called when this coin is collected by the player.
     * Adds to ScoreManager coin count.
     * Plays coin collect sound.
     */
    protected override void OnCollected()
    {

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddCoins(coinValue);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayCoinSound();
        }
    }
}