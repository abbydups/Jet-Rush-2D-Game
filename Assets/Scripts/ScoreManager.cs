using UnityEngine;

/*
 * Handles survival score, coin count and milestone progression.
 * Score increases based on scroll speed.
 * Milestones trigger every 30 seconds increasing difficulty.
 * All difficulty values are capped so the game stays fair and fun.
 */
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Score Settings")]
    [Tooltip("Score multiplier applied to scroll speed each second.")]
    public float scoreMultiplier = 10f;

    [Header("Milestone Settings")]
    [Tooltip("How many seconds between each milestone.")]
    public float milestoneInterval = 60f;

    [Tooltip("How much scroll speed increases per milestone.")]
    public float speedIncreasePerMilestone = 0.7f;

    [Header("Difficulty Caps")]
    [Tooltip("Maximum scroll speed. Game never exceeds this.")]
    public float maxScrollSpeed = 8f;

    [Tooltip("Maximum gauntlet chance. Never exceeds this.")]
    public float maxGauntletChance = 0.4f;

    private float _survivalScore = 0f;
    private int _coinCount = 0;
    private int _highScore = 0;
    private float _milestoneTimer = 0f;
    private int _milestoneCount = 0;
    private ObstacleSpawner _obstacleSpawner;

    /*
     * Sets up singleton and loads high score.
     * Uses DontDestroyOnLoad to persist across scenes.
     */
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        _highScore = PlayerPrefs.GetInt("HighScore", 0);
    }

    /*
     * Finds references to other managers.
     */
    private void Start()
    {
        _obstacleSpawner = FindFirstObjectByType<ObstacleSpawner>();
    }

    /*
     * Increases survival score based on scroll speed.
     * Counts down milestone timer.
     * Only runs during Playing state.
     */
    private void Update()
    {
        if (GameManager.Instance == null)
        {
            return;
        }
        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing)
        {
            return;
        }

        // Increase score based on scroll speed
        _survivalScore += GameManager.Instance.scrollSpeed
            * scoreMultiplier
            * Time.deltaTime;

        // Count down milestone timer
        _milestoneTimer += Time.deltaTime;

        if (_milestoneTimer >= milestoneInterval)
        {
            _milestoneTimer = 0f;
            TriggerMilestone();
        }
    }

    /*
     * Called every 30 seconds to increase difficulty.
     * All values are capped so game stays fun and fair.
     */
    private void TriggerMilestone()
    {
        _milestoneCount++;

        // Increase scroll speed up to cap
        float newSpeed = GameManager.Instance.scrollSpeed
            + speedIncreasePerMilestone;
        newSpeed = Mathf.Min(newSpeed, maxScrollSpeed);
        GameManager.Instance.scrollSpeed = newSpeed;


        if (_obstacleSpawner != null)
        {
            _obstacleSpawner.ShrinkGap();
            _obstacleSpawner.IncreaseSpawnRate(0.3f);

            if (GameManager.Instance.scrollSpeed < maxScrollSpeed)
            {
                _obstacleSpawner.IncreaseGauntletChance();
            }
        }

        // Show milestone sign via UIManager
        if (UIManager.Instance != null)
        {
            int distance = Mathf.FloorToInt(_survivalScore);
            UIManager.Instance.ShowMilestoneSign(distance, _milestoneCount);
        }
    }

    /*
     * Adds coins to the player's coin count.
     *
     * @param amount - How many coins to add.
     */
    public void AddCoins(int amount)
    {
        _coinCount += amount;
    }

    /*
     * Returns the current survival score as an integer.
     */
    public int GetScore()
    {
        return Mathf.FloorToInt(_survivalScore);
    }

    /*
     * Returns the current coin count.
     */
    public int GetCoinCount()
    {
        return _coinCount;
    }

    /*
     * Returns the all time high score.
     */
    public int GetHighScore()
    {
        return _highScore;
    }

    /*
     * Returns the current milestone number.
     */
    public int GetMilestoneCount()
    {
        return _milestoneCount;
    }

    /*
     * Returns milestone progress as a value between 0 and 1.
     */
    public float GetMilestoneProgress()
    {
        return _milestoneTimer / milestoneInterval;
    }

    /*
     * Saves high score to PlayerPrefs if current score beats it.
     */
    public void SaveHighScore()
    {
        int currentScore = GetScore();

        if (currentScore > _highScore)
        {
            _highScore = currentScore;
            PlayerPrefs.SetInt("HighScore", _highScore);
            PlayerPrefs.Save();
            Debug.Log("[ScoreManager] New high score: " + _highScore);
        }
    }

    /*
     * Resets score and coin count for a new game.
     */
    public void ResetScore()
    {
        _survivalScore = 0f;
        _coinCount = 0;
        _milestoneTimer = 0f;
        _milestoneCount = 0;

        _obstacleSpawner = FindFirstObjectByType<ObstacleSpawner>();
    }
}