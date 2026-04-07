using UnityEngine;

/*
 * Spawns obstacles at random intervals and positions
 * off the right edge of the screen.
 * Birds and blimps can spawn alone or in pairs with a gap.
 * Trees always spawn alone at the bottom.
 * Drones always spawn alone and avoid existing obstacle Y positions.
 * Gap between pairs shrinks over time for increasing difficulty.
 * Blimp gauntlet formation spawns randomly and at milestones.
 * Gauntlet uses sine wave to create smooth concave or convex curves.
 * Normal spawning is completely blocked during gauntlet mode.
 * Notifies TreasureSpawner when obstacles spawn so coins appear
 * in front of birds, blimps, trees and in gaps between pairs.
 * Drone spawn chance is separately controlled to avoid overuse.
 */
public class ObstacleSpawner : MonoBehaviour
{
    [Header("Obstacle Prefabs")]
    [Tooltip("Bird obstacle prefab.")]
    public GameObject birdPrefab;

    [Tooltip("Tall tree obstacle prefab.")]
    public GameObject treePrefab;

    [Tooltip("Drone obstacle prefab.")]
    public GameObject dronePrefab;

    [Tooltip("Blimp obstacle prefab.")]
    public GameObject blimpPrefab;

    [Header("Spawn Settings")]
    [Tooltip("X position where obstacles spawn off the right edge.")]
    public float spawnX = 12f;

    [Tooltip("Lowest Y position a bird can spawn at.")]
    public float minSpawnY = -1f;

    [Tooltip("Highest Y position a bird can spawn at.")]
    public float maxSpawnY = 2f;

    [Tooltip("Minimum time in seconds between obstacle spawns.")]
    public float minSpawnInterval = 4f;

    [Tooltip("Maximum time in seconds between obstacle spawns.")]
    public float maxSpawnInterval = 8f;

    [Header("Tree Settings")]
    [Tooltip("Fixed Y position for tree obstacles at the bottom of the screen.")]
    public float treeSpawnY = -3f;

    [Tooltip("Height of the tree sprite for coin placement at the top.")]
    public float treeHeight = 1f;

    [Header("Blimp Settings")]
    [Tooltip("Minimum Y position for blimp spawning.")]
    public float minBlimpSpawnY = 1f;

    [Tooltip("Maximum Y position for blimp spawning.")]
    public float maxBlimpSpawnY = 3f;

    [Header("Drone Settings")]
    [Tooltip("Lowest Y position the drone can spawn at.")]
    public float minDroneSpawnY = 0f;

    [Tooltip("Highest Y position the drone can spawn at.")]
    public float maxDroneSpawnY = 2f;

    [Tooltip("Chance of drone being added to the spawn pool per cycle. 1 = always, 0.3 = 30% chance.")]
    [Range(0f, 1f)]
    public float droneSpawnChance = 0.3f;

    [Header("Pair Gap Settings")]
    [Tooltip("Starting gap between paired obstacles. Large = easy.")]
    public float startingGap = 3f;

    [Tooltip("Minimum gap between paired obstacles. Never gets smaller than this.")]
    public float minimumGap = 2f;

    [Tooltip("How much the gap shrinks each time a milestone is reached.")]
    public float gapShrinkAmount = 0.2f;

    // Current gap between pairs - shrinks over time
    private float _currentGap;

    [Header("Gauntlet Settings")]
    [Tooltip("Base percentage chance of gauntlet triggering. 0.1 = 10% chance.")]
    public float gauntletBaseChance = 0.1f;

    [Tooltip("How much gauntlet chance increases per milestone reached.")]
    public float gauntletMilestoneBonus = 0.05f;

    [Tooltip("Horizontal space between each blimp pair in the gauntlet.")]
    public float gauntletSpacing = 2f;

    // Current gauntlet chance - increases with milestones
    private float _currentGauntletChance;

    // Whether a gauntlet is currently being spawned
    private bool _isGauntletActive = false;

    // How many blimp pairs are left to spawn in current gauntlet
    private int _gauntletPairsRemaining = 0;

    // Timer for spacing between gauntlet pairs
    private float _gauntletTimer = 0f;

    // Last center Y position used for smooth gap transitions
    private float _lastGauntletCenterY = 0f;

    // Sine wave offset - random start angle determines concave or convex
    private float _gauntletSineOffset = 0f;

    // Sine wave amplitude - how much the curve bends
    private float _gauntletSineAmplitude = 0f;

    // Current pair index in gauntlet used for sine calculation
    private float _gauntletPairIndex = 0;

    // Timer tracking time until next spawn
    private float _spawnTimer;

    // Reference to the treasure spawner for coin placement
    private TreasureSpawner _treasureSpawner;

    /*
     * Sets the initial spawn timer, gap size and gauntlet chance.
     * Finds the TreasureSpawner for coin placement notifications.
     */
    private void Start()
    {
        _spawnTimer = Random.Range(minSpawnInterval, maxSpawnInterval);
        _currentGap = startingGap;
        _currentGauntletChance = gauntletBaseChance;
        _treasureSpawner = FindFirstObjectByType<TreasureSpawner>();
    }

    /*
     * Handles both normal spawning and gauntlet spawning.
     * During gauntlet mode normal spawning is completely blocked.
     */
    private void Update()
    {
        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing)
        {
            return;
        }

        if (_isGauntletActive)
        {
            UpdateGauntlet();
            return;
        }

        _spawnTimer -= Time.deltaTime;

        if (_spawnTimer <= 0f)
        {
            if (ShouldTriggerGauntlet())
            {
                StartGauntlet();
            }
            else
            {
                SpawnObstacle();
            }
                
            _spawnTimer = Random.Range(minSpawnInterval, maxSpawnInterval);
        }
    }

    /*
     * Returns true if the gauntlet should trigger this spawn cycle.
     */
    private bool ShouldTriggerGauntlet()
    {
        return Random.value < _currentGauntletChance;
    }

    /*
     * Begins a blimp gauntlet formation.
     * Notifies TreasureSpawner first before any pairs spawn
     * to prevent stray coins from appearing.
     */
    private void StartGauntlet()
    {
        // Notify treasure spawner FIRST before any pairs spawn
        if (_treasureSpawner != null)
        {
            _treasureSpawner.SetGauntletActive(0f);
        }

        _isGauntletActive = true;
        _gauntletPairsRemaining = Random.Range(3, 6);
        _gauntletTimer = 0f;
        _gauntletPairIndex = 0;
        _gauntletSineOffset = Random.Range(0f, Mathf.PI * 2f);
        _gauntletSineAmplitude = Random.Range(0.8f, 1.5f);
        _lastGauntletCenterY = 0f;
    }

    /*
     * Handles spawning blimp pairs during gauntlet mode.
     * Ends gauntlet when all pairs have been spawned.
     */
    private void UpdateGauntlet()
    {
        _gauntletTimer -= Time.deltaTime;

        if (_gauntletTimer <= 0f)
        {
            if (_gauntletPairsRemaining > 0)
            {
                SpawnPair(blimpPrefab, smoothTransition: true);
                _gauntletPairsRemaining--;
                _gauntletPairIndex++;

                _gauntletTimer = gauntletSpacing
                    / GameManager.Instance.scrollSpeed;
            }
            else
            {
                _isGauntletActive = false;
                _spawnTimer = Random.Range(minSpawnInterval, maxSpawnInterval);

                // Notify treasure spawner to stop coin stream
                if (_treasureSpawner != null)
                {
                    _treasureSpawner.SetGauntletInactive();
                } 
            }
        }
    }

    /*
     * Picks a random obstacle type and spawns it.
     */
    private void SpawnObstacle()
    {
        GameObject prefabToSpawn = GetRandomObstaclePrefab();

        if (prefabToSpawn == null)
        {
            Debug.Log("[ObstacleSpawner] No prefabs assigned yet.");
            return;
        }

        if (prefabToSpawn == treePrefab)
        {
            SpawnSingle(prefabToSpawn, treeSpawnY);
        }
        else if (prefabToSpawn == dronePrefab)
        {
            SpawnDrone();
        }
        else if (prefabToSpawn == birdPrefab || prefabToSpawn == blimpPrefab)
        {
            bool spawnAsPair = Random.value > 0.5f;

            if (spawnAsPair)
            {
                SpawnPair(prefabToSpawn);
            }
            else
            {
                float spawnY = prefabToSpawn == blimpPrefab
                    ? Random.Range(minBlimpSpawnY, maxBlimpSpawnY)
                    : Random.Range(minSpawnY, maxSpawnY);

                SpawnSingle(prefabToSpawn, spawnY);
            }
        }
    }

    /*
     * Spawns a single obstacle at the given Y position.
     * Notifies TreasureSpawner based on obstacle type.
     *
     * @param prefab - The obstacle prefab to spawn.
     * @param spawnY - The Y position to spawn at.
     */
    private void SpawnSingle(GameObject prefab, float spawnY)
    {
        Vector3 spawnPosition = new Vector3(spawnX, spawnY, 0f);
        Instantiate(prefab, spawnPosition, Quaternion.identity);

        // Notify treasure spawner based on obstacle type
        if (_treasureSpawner != null)
        {
            if (prefab == birdPrefab)
            {
                _treasureSpawner.SpawnCoinsInFrontOfBird(spawnY);
            }
            else if (prefab == blimpPrefab)
            {
                _treasureSpawner.SpawnCoinsInFrontOfBlimp(spawnY);
            }
            else if (prefab == treePrefab)
            {
                _treasureSpawner.SpawnCoinsInFrontOfTree(spawnY, treeHeight);
            }
        }
    }

    /*
     * Spawns two obstacles at the same X position with a gap between them.
     * Gap size shrinks over time based on milestone progress.
     * If smoothTransition is true uses sine wave to create smooth
     * concave or convex curve pattern through the gauntlet.
     * Notifies TreasureSpawner to place coins in the gap.
     *
     * @param prefab           - The obstacle prefab to spawn as a pair.
     * @param smoothTransition - Whether to use sine wave center positioning.
     */
    private void SpawnPair(GameObject prefab, bool smoothTransition = false)
    {
        float centerY;

        if (smoothTransition)
        {
            // Use sine wave for smooth concave or convex curving
            float sineValue = Mathf.Sin(
                _gauntletSineOffset + (_gauntletPairIndex * 0.5f)
            );

            centerY = sineValue * _gauntletSineAmplitude;

            // Clamp to safe range so blimps stay on screen
            centerY = Mathf.Clamp(
                centerY,
                minSpawnY + (_currentGap / 2f) + 0.3f,
                maxSpawnY - (_currentGap / 2f) - 0.3f
            );
        }
        else
        {
            // Normal random center for non gauntlet pairs
            centerY = Random.Range(
                minSpawnY + (_currentGap / 2f),
                maxSpawnY - (_currentGap / 2f)
            );
        }

        _lastGauntletCenterY = centerY;

        float topY    = centerY + (_currentGap / 2f);
        float bottomY = centerY - (_currentGap / 2f);

        if (_treasureSpawner != null)
        {
            if (smoothTransition)
            {
                _treasureSpawner.UpdateGauntletGap(centerY);
            }
            else
            {
                _treasureSpawner.SpawnCoinsInGap(centerY);
            }
        }

        Instantiate(prefab, new Vector3(spawnX, topY,    0f), Quaternion.identity);
        Instantiate(prefab, new Vector3(spawnX, bottomY, 0f), Quaternion.identity);
    }

    /*
     * Spawns a drone at a safe Y position avoiding existing obstacles.
     */
    private void SpawnDrone()
    {
        for (int i = 0; i < 10; i++)
        {
            float spawnY = Random.Range(minDroneSpawnY, maxDroneSpawnY);

            if (!IsTooCloseToExistingObstacle(spawnY))
            {
                SpawnSingle(dronePrefab, spawnY);
                return;
            }
        }
    }

    /*
     * Checks if any existing obstacle is at a similar Y position.
     *
     * @param spawnY - The Y position to check.
     */
    private bool IsTooCloseToExistingObstacle(float spawnY)
    {
        GameObject[] existingObstacles =
            GameObject.FindGameObjectsWithTag("Obstacle");

        foreach (GameObject obstacle in existingObstacles)
        {
            float verticalDistance =
                Mathf.Abs(obstacle.transform.position.y - spawnY);

            if (verticalDistance < 1f)
            {
                return true;
            }
        }

        return false;
    }

    /*
     * Returns a random obstacle prefab.
     * Drone is excluded during gauntlet mode.
     * Drone is only added to the pool based on droneSpawnChance.
     */
    private GameObject GetRandomObstaclePrefab()
    {
        System.Collections.Generic.List<GameObject> availablePrefabs
            = new System.Collections.Generic.List<GameObject>();

        if (birdPrefab  != null)
        {
            availablePrefabs.Add(birdPrefab);
        }
       
        if (treePrefab  != null)
        {
            availablePrefabs.Add(treePrefab);
        }
        
        if (blimpPrefab != null)
        {
            availablePrefabs.Add(blimpPrefab);
        } 

        // Drone only added based on droneSpawnChance
        // Lower droneSpawnChance = drone spawns less frequently
        if (dronePrefab != null && !_isGauntletActive)
        {
            if (Random.value < droneSpawnChance)
            {
                availablePrefabs.Add(dronePrefab);
            }
        }

        if (availablePrefabs.Count == 0)
        {
            return null;
        }

        return availablePrefabs[Random.Range(0, availablePrefabs.Count)];
    }

    /*
     * Decreases spawn interval to increase difficulty.
     * Called by ScoreManager at milestones.
     *
     * @param amount - How much to decrease the spawn interval by.
     */
    public void IncreaseSpawnRate(float amount)
    {
        minSpawnInterval = Mathf.Max(0.5f, minSpawnInterval - amount);
        maxSpawnInterval = Mathf.Max(1f,   maxSpawnInterval - amount);
    }

    /*
     * Shrinks the gap between paired obstacles.
     * Called by ScoreManager at milestones.
     */
    public void ShrinkGap()
    {
        _currentGap = Mathf.Max(minimumGap, _currentGap - gapShrinkAmount);
    }

    /*
     * Increases gauntlet trigger chance at milestones.
     * Called by ScoreManager at milestones.
     */
    public void IncreaseGauntletChance()
    {
        _currentGauntletChance += gauntletMilestoneBonus;
    }
}