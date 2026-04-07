using UnityEngine;
using UnityEngine.SceneManagement;

/*
 * GameManager manages the overall game state,
 * scene transitions, and global game settings like scroll speed.
 */
public class GameManager : MonoBehaviour
{

    public static GameManager Instance;

    public enum GameState
    {
        Menu,
        Countdown,
        Playing,
        Paused,
        GameOver
    }

    [SerializeField] private GameState _currentState;
    public GameState CurrentState
    {
        get { return _currentState; }
        private set { _currentState = value; }
    }


    [Header("Scroll Settings")]
    [Tooltip("Starting speed at which the world scrolls left.")]
    public float scrollSpeed = 3f;

    [Tooltip("The fastest the world is allowed to scroll.")]
    public float maxScrollSpeed = 10f;

    private const int SCENE_GAME = 0;

    /*
     * Enforces singleton pattern.
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
    }

    /*
     * Transitions the game to a new state.
     * Notifies UIManager to update panels.
     * Switches music based on state.
     *
     * @param newState - The GameState to transition to.
     */
    public void SetState(GameState newState)
    {
        CurrentState = newState;

        // Switch music based on state
        if (AudioManager.Instance != null)
        {
            if (newState == GameState.Menu)
            {
                AudioManager.Instance.PlayMenuMusic();
            }
            else if (newState == GameState.Countdown)
            {
                AudioManager.Instance.StopMusic();
            }
            else if (newState == GameState.Playing)
            {
                AudioManager.Instance.PlayGameplayMusic();
            }
            else if (newState == GameState.GameOver)
            {
                AudioManager.Instance.StopMusic();
            }
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdatePanels(newState);
        }
    }

    /*
     * Called when countdown finishes.
     * Transitions to Playing state.
     */
    public void StartGame()
    {
        SetState(GameState.Playing);
    }

    /*
     * Pauses the game by freezing time scale.
     */
    public void PauseGame()
    {
        SetState(GameState.Paused);
        Time.timeScale = 0f;
    }

    /*
     * Resumes the game from paused state.
     */
    public void ResumeGame()
    {
        SetState(GameState.Playing);
        Time.timeScale = 1f;
    }

    /*
     * Triggered when player dies.
     * Saves high score.
     * Plays game over sound.
     */
    public void GameOver()
    {

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.SaveHighScore();
        }
        
        SetState(GameState.GameOver);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayGameOverSound();
        }
    }

    /*
     * Restarts the game by reloading the Game scene.
     * Signals UIManager to start with countdown.
     * Resets score and scroll speed.
     */
    public void RestartGame()
    {
        Time.timeScale = 1f;

        // Reset scroll speed back to starting value
        scrollSpeed = 3f;

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResetScore();
        }

        // Signal UIManager to start with countdown after reload
        PlayerPrefs.SetInt("StartWithCountdown", 1);
        PlayerPrefs.Save();

        SceneManager.LoadScene(SCENE_GAME);
    }

    /*
     * Returns to the main menu state.
     * Reloads scene so everything is fresh.
     */
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        scrollSpeed = 3f;

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResetScore();
        }

        // Reload scene so everything is fresh
        SceneManager.LoadScene(SCENE_GAME);
    }

    /*
     * Increases world scroll speed at milestones.
     * Clamped to maxScrollSpeed.
     *
     * @param amount - How much to increase scroll speed by.
     */
    public void IncreaseScrollSpeed(float amount)
    {
        scrollSpeed = Mathf.Clamp(
            scrollSpeed + amount, 0f, maxScrollSpeed);
    }
}