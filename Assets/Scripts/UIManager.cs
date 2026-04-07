using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

/*
 * Manages all UI panels in the Game scene.
 * Shows and hides panels based on GameManager state.
 * Updates HUD with live score, coin count and best score.
 * Handles milestone sign drop animation.
 * Handles game over box slide down animation.
 * Handles countdown animation.
 * Shows simple in-game tutorial tips on first launch.
 */
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Panels")]
    [Tooltip("HUD shown during gameplay.")]
    public GameObject hud;

    [Tooltip("Main menu panel.")]
    public GameObject mainMenuPanel;

    [Tooltip("Countdown panel.")]
    public GameObject countdownPanel;

    [Tooltip("Game over panel.")]
    public GameObject gameOverPanel;

    [Tooltip("Milestone sign panel.")]
    public GameObject milestoneSign;

    [Tooltip("In-game tutorial tip panel.")]
    public GameObject tutorialTipPanel;

    [Header("HUD")]
    [Tooltip("Score text in HUD.")]
    public TextMeshProUGUI scoreText;

    [Tooltip("Coin count text in HUD.")]
    public TextMeshProUGUI coinText;

    [Tooltip("Best score text in HUD top left.")]
    public TextMeshProUGUI bestScoreText;

    [Header("Main Menu")]
    [Tooltip("High score text on main menu.")]
    public TextMeshProUGUI menuHighScoreText;

    [Header("Countdown")]
    [Tooltip("Countdown number text.")]
    public TextMeshProUGUI countdownText;

    [Header("Game Over")]
    [Tooltip("The sliding game over box.")]
    public RectTransform gameOverBox;

    [Tooltip("Final score text.")]
    public TextMeshProUGUI finalScoreText;

    [Tooltip("Final coins text.")]
    public TextMeshProUGUI finalCoinsText;

    [Tooltip("High score text on game over.")]
    public TextMeshProUGUI gameOverHighScoreText;

    [Tooltip("New high score text - only shows if new record.")]
    public TextMeshProUGUI newHighScoreText;

    [Header("Milestone Sign")]
    [Tooltip("The milestone sign RectTransform for animation.")]
    public RectTransform milestoneSignRect;

    [Tooltip("Text on the milestone sign.")]
    public TextMeshProUGUI milestoneSignText;

    [Header("Tutorial Tips")]
    [Tooltip("Text showing the current tutorial tip.")]
    public TextMeshProUGUI tutorialTipText;

    [Header("Animation Settings")]
    [Tooltip("How fast the game over box slides down.")]
    public float gameOverSlideSpeed = 5f;

    [Tooltip("Y position of game over box when visible.")]
    public float gameOverTargetY = -100f;

    [Tooltip("Y position of game over box when hidden above screen.")]
    public float gameOverHiddenY = 1200f;

    [Tooltip("How long milestone sign stays on screen in seconds.")]
    public float milestoneSignDuration = 3f;

    [Tooltip("How long each tutorial tip stays on screen.")]
    public float tutorialTipDuration = 3f;

    [Header("Sound Timing")]
    [Tooltip("How long to wait after button click before countdown starts.")]
    public float buttonClickDelay = 0.3f;

    [Tooltip("How long to wait before playing countdown sound.")]
    public float countdownSoundDelay = 0.1f;

    private bool _isGameOverSliding = false;
    private float _gameOverTargetY = 0f;

    /*
     * Sets up singleton.
     */
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    /*
     * Determines correct starting state.
     * If coming from retry starts with countdown.
     * Otherwise starts on menu.
     */
    private void Start()
    {
        if (PlayerPrefs.GetInt("StartWithCountdown", 0) == 1)
        {
            PlayerPrefs.SetInt("StartWithCountdown", 0);
            PlayerPrefs.Save();
            GameManager.Instance.SetState(GameManager.GameState.Countdown);
        }
        else
        {
            GameManager.Instance.SetState(GameManager.GameState.Menu);
        }

        if (menuHighScoreText != null && ScoreManager.Instance != null)
        {
            menuHighScoreText.text = "Best: "
                + ScoreManager.Instance.GetHighScore().ToString("N0");
        }  
    }

    /*
     * Updates HUD every frame during gameplay.
     * Shows New Best when current score beats high score.
     * Handles game over box sliding animation.
     */
    private void Update()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        if (GameManager.Instance.CurrentState == GameManager.GameState.Playing)
        {
            if (scoreText != null && ScoreManager.Instance != null)
            {
                scoreText.text = "Score: " + ScoreManager.Instance
                    .GetScore().ToString("N0");
            }

            if (coinText != null && ScoreManager.Instance != null)
            {
                coinText.text = "Coins: "
                    + ScoreManager.Instance.GetCoinCount();
            }

            if (bestScoreText != null && ScoreManager.Instance != null)
            {
                int currentScore = ScoreManager.Instance.GetScore();
                int highScore    = ScoreManager.Instance.GetHighScore();

                if (currentScore > highScore)
                {
                    bestScoreText.text = "New Best: "
                        + currentScore.ToString("N0");
                }
                else
                {
                    bestScoreText.text = "Best: "
                        + highScore.ToString("N0");
                }
            }
        }

        if (_isGameOverSliding && gameOverBox != null)
        {
            gameOverBox.anchoredPosition = Vector2.Lerp(
                gameOverBox.anchoredPosition,
                new Vector2(0f, _gameOverTargetY),
                gameOverSlideSpeed * Time.deltaTime
            );

            if (Mathf.Abs(gameOverBox.anchoredPosition.y
                - _gameOverTargetY) < 1f)
            {
                gameOverBox.anchoredPosition =
                    new Vector2(0f, _gameOverTargetY);
                _isGameOverSliding = false;
            }
        }
    }

    /*
     * Shows the correct panel based on the current game state.
     * Called by GameManager when state changes.
     *
     * @param state - The current game state.
     */
    public void UpdatePanels(GameManager.GameState state)
    {
        HideAllPanels();

        switch (state)
        {
            case GameManager.GameState.Menu:
                ShowMainMenu();
                break;

            case GameManager.GameState.Countdown:
                ShowCountdown();
                break;

            case GameManager.GameState.Playing:
                ShowHUD();
                break;

            case GameManager.GameState.GameOver:
                ShowGameOver();
                break;
        }
    }

    /*
     * Hides all UI panels.
     */
    private void HideAllPanels()
    {
        if (hud != null)
        {
            hud.SetActive(false);
        }              
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
        }    
        if (countdownPanel != null)
        {
            countdownPanel.SetActive(false);
        }   
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }    
        if (milestoneSign != null)
        {
            milestoneSign.SetActive(false);
        }    
        if (tutorialTipPanel != null)
        {
            tutorialTipPanel.SetActive(false);
        } 
    }

    /*
     * Shows the main menu panel and updates high score.
     */
    private void ShowMainMenu()
    {
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }

        if (menuHighScoreText != null && ScoreManager.Instance != null)
        {
            menuHighScoreText.text = "Best: "
                + ScoreManager.Instance.GetHighScore().ToString("N0");
        }
    }

    /*
     * Shows the HUD during gameplay.
     * Updates best score when HUD first shows.
     * Shows tutorial tips if first time playing.
     */
    private void ShowHUD()
    {
        if (hud != null)
        {
            hud.SetActive(true);
        }

        if (bestScoreText != null && ScoreManager.Instance != null)
        {
            bestScoreText.text = "Best: "
                + ScoreManager.Instance.GetHighScore().ToString("N0");
        }

        if (PlayerPrefs.GetInt("TutorialComplete", 0) == 0)
        {
            StartCoroutine(ShowTutorialTips());
        }
    }

    /*
     * Shows countdown panel and starts countdown coroutine.
     */
    private void ShowCountdown()
    {
        if (countdownPanel != null)
        {
            countdownPanel.SetActive(true);
        }

        StartCoroutine(CountdownCoroutine());
    }

    /*
     * Shows game over panel and slides box down.
     * Updates final score and coin count.
     */
    private void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (ScoreManager.Instance != null)
        {
            int score     = ScoreManager.Instance.GetScore();
            int coins     = ScoreManager.Instance.GetCoinCount();
            int highScore = ScoreManager.Instance.GetHighScore();

            if (finalScoreText != null)
            {
                finalScoreText.text = score.ToString("N0");
            }

            if (finalCoinsText != null)
            {
                finalCoinsText.text = "" + coins;
            }

            if (gameOverHighScoreText != null)
            {
                gameOverHighScoreText.text = ""
                    + highScore.ToString("N0");
            }

            if (newHighScoreText != null)
            {
                newHighScoreText.gameObject.SetActive(score >= highScore);
            }
        }

        if (gameOverBox != null)
        {
            gameOverBox.anchoredPosition =
                new Vector2(0f, gameOverHiddenY);
            _gameOverTargetY = gameOverTargetY;
            _isGameOverSliding = true;
        }
    }

    /*
     * Counts down from 3 to 1 then shows Go!
     * Waits before playing countdown sound so it does
     * not overlap with button click sound.
     * Then transitions to Playing state.
     */
    private IEnumerator CountdownCoroutine()
    {
        if (countdownText != null)
        {
            countdownText.text = "3";

            // Wait for countdown sound delay then play
            yield return new WaitForSeconds(countdownSoundDelay);

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayCountdownSound();
            }

            // Wait remaining time so total is still 1 second
            yield return new WaitForSeconds(1f - countdownSoundDelay);

            countdownText.text = "2";
            yield return new WaitForSeconds(1f);

            countdownText.text = "1";
            yield return new WaitForSeconds(1f);

            countdownText.text = "GO!";
            yield return new WaitForSeconds(0.5f);
        }

        GameManager.Instance.StartGame();
    }

    /*
     * Shows simple tips one at a time during first gameplay.
     * Only shows once ever — marks tutorial complete when done.
     */
    private IEnumerator ShowTutorialTips()
    {
        if (tutorialTipPanel == null || tutorialTipText == null)
        {
            yield break;
        }

        string[] tips = new string[]
        {
            "Hold SPACE to fly up! Release to descend.",
            "Avoid obstacles or it's game over!",
            "Collect coins!",
            "The longer you fly, the higher your score goes!"
        };

        tutorialTipPanel.SetActive(true);

        foreach (string tip in tips)
        {
            tutorialTipText.text = tip;
            yield return new WaitForSeconds(tutorialTipDuration);
        }

        tutorialTipPanel.SetActive(false);

        PlayerPrefs.SetInt("TutorialComplete", 1);
        PlayerPrefs.Save();

        Debug.Log("[UIManager] Tutorial tips complete.");
    }

    /*
     * Shows the milestone sign dropping from the top.
     * Plays milestone sound.
     *
     * @param distanceTraveled - Distance in meters to display.
     * @param milestoneNumber  - Current milestone number.
     */
    public void ShowMilestoneSign(int distanceTraveled, int milestoneNumber)
    {
        if (milestoneSign == null)
        {
            return;
        } 

        if (milestoneSignText != null)
        {
            milestoneSignText.text = distanceTraveled + " METERS";
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMilestoneSound();
        }

        StartCoroutine(MilestoneSignCoroutine());
    }

    /*
     * Animates the milestone sign dropping down with bounce
     * staying visible then sliding back up.
     */
    private IEnumerator MilestoneSignCoroutine()
    {
        milestoneSign.SetActive(true);

        milestoneSignRect.anchoredPosition = new Vector2(0f, 300f);

        float elapsed      = 0f;
        float dropDuration = 0.5f;
        float targetY      = -150f;

        while (elapsed < dropDuration)
        {
            elapsed += Time.deltaTime;
            float t      = elapsed / dropDuration;
            float bounce = Mathf.Sin(t * Mathf.PI) * 20f;
            float y      = Mathf.Lerp(300f, targetY, t) + bounce;

            milestoneSignRect.anchoredPosition = new Vector2(0f, y);
            yield return null;
        }

        milestoneSignRect.anchoredPosition = new Vector2(0f, targetY);

        yield return new WaitForSeconds(milestoneSignDuration);

        elapsed            = 0f;
        float riseDuration = 0.3f;

        while (elapsed < riseDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / riseDuration;
            float y = Mathf.Lerp(targetY, 300f, t);

            milestoneSignRect.anchoredPosition = new Vector2(0f, y);
            yield return null;
        }

        milestoneSign.SetActive(false);
    }

    /*
     * Called when Play button is pressed on main menu.
     * Plays button click sound then waits before
     * starting countdown so sounds do not overlap.
     */
    public void OnPlayPressed()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopButtonHover();
            AudioManager.Instance.PlayButtonClick();
        }

        // Wait for button click to finish before countdown
        StartCoroutine(DelayedCountdown());
    }

    /*
     * Waits for button click sound to finish
     * then starts the countdown.
     */
    private IEnumerator DelayedCountdown()
    {
        yield return new WaitForSeconds(buttonClickDelay);
        GameManager.Instance.SetState(GameManager.GameState.Countdown);
    }

    /*
     * Called when Retry button is pressed on game over screen.
     * Plays button click sound.
     */
    public void OnRetryPressed()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }

        GameManager.Instance.RestartGame();
    }

    /*
     * Called when Main Menu button is pressed on game over screen.
     * Plays button click sound.
     */
    public void OnMainMenuPressed()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }

        GameManager.Instance.GoToMainMenu();
    }
}