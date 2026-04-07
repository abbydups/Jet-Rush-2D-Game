using UnityEngine;

/*
 * Spawns coins and shields based on obstacle positions.
 * Coins appear in front of birds, blimps and at top of trees.
 * During gauntlet coins stream continuously in the gap
 * following the curve from start to end of gauntlet.
 * Coins never spawn overlapping with any obstacle.
 * Shields spawn rarely at random intervals.
 */
public class TreasureSpawner : MonoBehaviour
{
    [Header("Treasure Prefabs")]
    [Tooltip("Coin prefab to spawn.")]
    public GameObject coinPrefab;

    [Tooltip("Shield prefab to spawn.")]
    public GameObject shieldPrefab;

    [Header("Coin Settings")]
    [Tooltip("X position where coins spawn.")]
    public float spawnX = 12f;

    [Tooltip("Horizontal spacing between coins.")]
    public float coinSpacing = 0.5f;

    [Tooltip("How far in front of obstacle coins appear.")]
    public float coinLeadDistance = 1.5f;

    // How often a coin spawns during gauntlet stream
    [Tooltip("Time between each coin during gauntlet stream.")]
    public float gauntletCoinInterval = 0.2f;

    [Header("Shield Settings")]
    [Tooltip("Minimum seconds between shield spawns.")]
    public float minShieldInterval = 30f;

    [Tooltip("Maximum seconds between shield spawns.")]
    public float maxShieldInterval = 45f;

    // Whether gauntlet coin stream is active
    private bool _isGauntletActive = false;

    // Whether the gap has been updated at least once
    // Prevents stray coins from spawning at Y: 0 before first gap update
    private bool _gauntletGapInitialized = false;

    // Current gap center Y for gauntlet streaming
    private float _gauntletGapCenterY = 0f;

    // Timer for gauntlet coin streaming
    private float _gauntletCoinTimer = 0f;

    // Timer for shield spawning
    private float _shieldTimer;

    /*
     * Sets initial shield timer.
     */
    private void Start()
    {
        _shieldTimer = Random.Range(minShieldInterval, maxShieldInterval);
    }

    /*
     * Handles gauntlet coin streaming and shield spawning.
     * Only streams coins after gap has been initialized
     * to prevent stray coins at Y: 0.
     */
    private void Update()
    {
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing)
        return;

        // Gauntlet coin stream
        // Only runs after gap has been initialized by first pair spawn
        if (_isGauntletActive && _gauntletGapInitialized)
        {
            _gauntletCoinTimer -= Time.deltaTime;
            if (_gauntletCoinTimer <= 0f)
            {
                // Spawn coin in gap following the curve
                SpawnCoin(spawnX - 1f, _gauntletGapCenterY);
                _gauntletCoinTimer = gauntletCoinInterval;
            }
        }

        // Shield timer
        _shieldTimer -= Time.deltaTime;
        if (_shieldTimer <= 0f)
        {
            SpawnShield();
            _shieldTimer = Random.Range(minShieldInterval, maxShieldInterval);
        }
    }

    /*
     * Called by ObstacleSpawner when gauntlet starts.
     * Resets gap initialized flag to prevent stray coins.
     *
     * @param gapCenterY - Initial Y center of the gauntlet gap.
     */
    public void SetGauntletActive(float gapCenterY)
    {
        _isGauntletActive = true;
        _gauntletGapCenterY = gapCenterY;
        _gauntletCoinTimer = 0f;
        _gauntletGapInitialized = false;
        Debug.Log("[TreasureSpawner] Gauntlet coin stream started.");
    }

    /*
     * Called by ObstacleSpawner each time a gauntlet pair spawns.
     * Updates gap center so coins follow the curve.
     * Sets initialized flag so streaming can begin.
     *
     * @param gapCenterY - New Y center of the gauntlet gap.
     */
    public void UpdateGauntletGap(float gapCenterY)
    {
        _gauntletGapCenterY = gapCenterY;
        _gauntletGapInitialized = true;
    }

    /*
     * Called by ObstacleSpawner when gauntlet ends.
     * Stops the continuous coin stream and resets flags.
     */
    public void SetGauntletInactive()
    {
        _isGauntletActive = false;
        _gauntletGapInitialized = false;
    }

    /*
     * Spawns coins to the left of a bird.
     * Player sees coins first then bird comes after.
     *
     * @param birdY - Y position of the bird.
     */
    public void SpawnCoinsInFrontOfBird(float birdY)
    {
        if (coinPrefab == null) return;

        int count = Random.Range(3, 6);
        for (int i = 0; i < count; i++)
        {
            float coinX = spawnX - coinLeadDistance - (i * coinSpacing);
            SpawnCoin(coinX, birdY);
        }
    }

    /*
     * Spawns coins to the left of a single blimp.
     * Player sees coins first then blimp comes after.
     *
     * @param blimpY - Y position of the blimp.
     */
    public void SpawnCoinsInFrontOfBlimp(float blimpY)
    {
        if (coinPrefab == null) return;

        int count = Random.Range(3, 6);
        for (int i = 0; i < count; i++)
        {
            float coinX = spawnX - coinLeadDistance - (i * coinSpacing);
            SpawnCoin(coinX, blimpY);
        }
    }

    /*
     * Spawns coins at the top of a tree.
     * Player must fly low to collect these rewarding risky flying.
     *
     * @param treeY      - Base Y position of the tree.
     * @param treeHeight - Height of the tree so coins appear at the top.
     */
    public void SpawnCoinsInFrontOfTree(float treeY, float treeHeight)
    {
        if (coinPrefab == null) return;

        float topOfTree = treeY + treeHeight;

        int count = Random.Range(3, 6);
        for (int i = 0; i < count; i++)
        {
            float coinX = spawnX - coinLeadDistance - (i * coinSpacing);
            SpawnCoin(coinX, topOfTree);
        }
    }

    /*
     * Spawns a coin cluster centered in the gap between a normal blimp pair.
     * Coins are evenly spread around the gap center X position.
     *
     * @param gapCenterY - Y center of the gap.
     */
    public void SpawnCoinsInGap(float gapCenterY)
    {
        if (coinPrefab == null)
        {
            return;
        }

        int count = Random.Range(3, 6);

        // Center the coin cluster at spawnX
        float totalWidth = (count - 1) * coinSpacing;
        float startX = spawnX - (totalWidth / 2f);

        for (int i = 0; i < count; i++)
        {
            float coinX = startX + (i * coinSpacing);
            SpawnCoin(coinX, gapCenterY);
        }
    }

    /*
     * Spawns a single coin at the given position.
     * Checks for nearby obstacles before spawning
     * to prevent coins from overlapping with obstacles.
     *
     * @param x - X position.
     * @param y - Y position.
     */
    private void SpawnCoin(float x, float y)
    {
        // Never spawn coin if obstacle is nearby
        if (IsObstacleNearby(x, y))
        {
            return;
        }

        Vector3 pos = new Vector3(x, y, 0f);
        Instantiate(coinPrefab, pos, Quaternion.identity);
    }

    /*
     * Checks if any obstacle is too close to the given position.
     * Prevents coins from overlapping with any obstacle.
     *
     * @param x - X position to check.
     * @param y - Y position to check.
     * @return  - True if an obstacle is too close.
     */
    private bool IsObstacleNearby(float x, float y)
    {
        GameObject[] obstacles = GameObject.FindGameObjectsWithTag("Obstacle");

        foreach (GameObject obstacle in obstacles)
        {
            float distX = Mathf.Abs(obstacle.transform.position.x - x);
            float distY = Mathf.Abs(obstacle.transform.position.y - y);

            // Check both X and Y distance
            if (distX < 1.5f && distY < 1f)
                return true;
        }

        return false;
    }

    /*
     * Spawns a shield in the middle of the screen.
     * Always spawns where the player can reach it.
     */
    private void SpawnShield()
    {
        if (shieldPrefab == null)
        {
            return;
        }

        float shieldY = Random.Range(-0.5f, 1.5f);
        Vector3 pos = new Vector3(spawnX, shieldY, 0f);
        Instantiate(shieldPrefab, pos, Quaternion.identity);
    }
}