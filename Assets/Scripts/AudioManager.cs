using UnityEngine;
using System.Collections;

/*
 * Manages all game audio including music and sound effects.
 * Separate volume controls for every sound effect.
 * Background music plays on menu and during gameplay.
 * Hover and drone sounds can be stopped or faded mid play.
 * Coin sound has cooldown to prevent audio spam.
 */
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance 
    {
        get; 
        private set; 
    }

    [Header("Music")]
    [Tooltip("Music played on main menu.")]
    public AudioClip menuMusic;

    [Tooltip("Music played during gameplay.")]
    public AudioClip gameplayMusic;

    [Tooltip("Main menu music volume.")]
    [Range(0f, 1f)]
    public float menuMusicVolume = 0.5f;

    [Tooltip("Gameplay music volume.")]
    [Range(0f, 1f)]
    public float gameplayMusicVolume = 0.5f;

    [Header("Player Sounds")]
    [Tooltip("Thrust sound loop while holding Space.")]
    public AudioClip thrustSound;

    [Tooltip("Volume of thrust sound.")]
    [Range(0f, 1f)]
    public float thrustVolume = 0.2f;

    [Tooltip("Coin collect sound.")]
    public AudioClip coinSound;

    [Tooltip("Volume of coin collect sound.")]
    [Range(0f, 1f)]
    public float coinVolume = 0.4f;

    [Tooltip("Minimum time between coin sounds to prevent audio spam.")]
    [Range(0f, 1f)]
    public float coinSoundCooldownTime = 0.1f;

    [Tooltip("Shield collect sound.")]
    public AudioClip shieldSound;

    [Tooltip("Volume of shield collect sound.")]
    [Range(0f, 1f)]
    public float shieldVolume = 1f;

    [Tooltip("Death crash sound.")]
    public AudioClip crashSound;

    [Tooltip("Volume of crash sound.")]
    [Range(0f, 1f)]
    public float crashVolume = 1f;

    [Tooltip("Landing on sand sound.")]
    public AudioClip landingSound;

    [Tooltip("Volume of landing sound.")]
    [Range(0f, 1f)]
    public float landingVolume = 1f;

    [Header("Obstacle Sounds")]
    [Tooltip("Drone warning beep.")]
    public AudioClip droneWarningSound;

    [Tooltip("Volume of drone warning sound.")]
    [Range(0f, 1f)]
    public float droneWarningVolume = 1f;

    [Tooltip("Drone attack whoosh.")]
    public AudioClip droneAttackSound;

    [Tooltip("Volume of drone attack sound.")]
    [Range(0f, 1f)]
    public float droneAttackVolume = 1f;

    [Header("UI Sounds")]
    [Tooltip("Button click sound.")]
    public AudioClip buttonClickSound;

    [Tooltip("Volume of button click sound.")]
    [Range(0f, 1f)]
    public float buttonClickVolume = 1f;

    [Tooltip("Button hover sound.")]
    public AudioClip buttonHoverSound;

    [Tooltip("Volume of button hover sound.")]
    [Range(0f, 1f)]
    public float buttonHoverVolume = 1f;

    [Tooltip("Countdown sound - plays 3 2 1 Go in one file.")]
    public AudioClip countdownSound;

    [Tooltip("Volume of countdown sound.")]
    [Range(0f, 1f)]
    public float countdownVolume = 1f;

    [Tooltip("Milestone fanfare.")]
    public AudioClip milestoneSound;

    [Tooltip("Volume of milestone sound.")]
    [Range(0f, 1f)]
    public float milestoneVolume = 1f;

    [Tooltip("Game over sound.")]
    public AudioClip gameOverSound;

    [Tooltip("Volume of game over sound.")]
    [Range(0f, 1f)]
    public float gameOverVolume = 1f;

    private AudioSource _musicSource;
    private AudioSource _thrustSource;
    private AudioSource _sfxSource;
    private AudioSource _hoverSource;
    private AudioSource _droneSource;
    private bool _isThrustPlaying = false;

    // Coin sound cooldown timer
    private float _coinSoundCooldown = 0f;

    /*
     * Sets up and creates audio sources.
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

        SetupAudioSources();
    }

    /*
     * Starts menu music on launch.
     */
    private void Start()
    {
        PlayMenuMusic();
    }

    /*
     * Counts down coin sound cooldown timer.
     */
    private void Update()
    {
        if (_coinSoundCooldown > 0f)
        {
            _coinSoundCooldown -= Time.deltaTime;
        }
    }

    /*
     * Creates five AudioSource components.
     */
    private void SetupAudioSources()
    {
        _musicSource = gameObject.AddComponent<AudioSource>();
        _musicSource.loop = true;
        _musicSource.volume = menuMusicVolume;
        _musicSource.playOnAwake = false;

        _thrustSource = gameObject.AddComponent<AudioSource>();
        _thrustSource.loop = true;
        _thrustSource.volume = thrustVolume;
        _thrustSource.playOnAwake = false;

        _sfxSource = gameObject.AddComponent<AudioSource>();
        _sfxSource.loop = false;
        _sfxSource.playOnAwake = false;

        _hoverSource = gameObject.AddComponent<AudioSource>();
        _hoverSource.loop = false;
        _hoverSource.playOnAwake = false;

        _droneSource = gameObject.AddComponent<AudioSource>();
        _droneSource.loop = false;
        _droneSource.playOnAwake = false;
    }

    /*
     * Plays menu music with menu volume.
     */
    public void PlayMenuMusic()
    {
        if (menuMusic == null)
        {
            return;
        }

        if (_musicSource.clip == menuMusic && _musicSource.isPlaying)
        {
            return;
        }

        _musicSource.clip = menuMusic;
        _musicSource.volume = menuMusicVolume;
        _musicSource.Play();
    }

    /*
     * Plays gameplay music with gameplay volume.
     */
    public void PlayGameplayMusic()
    {
        if (gameplayMusic == null)
        {
            return;
        }
        if (_musicSource.clip == gameplayMusic && _musicSource.isPlaying)
        {
            return;
        }

        _musicSource.clip = gameplayMusic;
        _musicSource.volume = gameplayMusicVolume;
        _musicSource.Play();
    }

    /*
     * Stops all music.
     */
    public void StopMusic()
    {
        _musicSource.Stop();
    }

    /*
     * Sets menu music volume.
     */
    public void SetMenuMusicVolume(float volume)
    {
        menuMusicVolume = volume;
        if (_musicSource.clip == menuMusic)
        {
            _musicSource.volume = volume;
        }
    }

    /*
     * Sets gameplay music volume.
     */
    public void SetGameplayMusicVolume(float volume)
    {
        gameplayMusicVolume = volume;
        if (_musicSource.clip == gameplayMusic)
        {
            _musicSource.volume = volume;
        }
    }

    /*
     * Sets thrust sound volume.
     */
    public void SetThrustVolume(float volume)
    {
        thrustVolume = volume;
        _thrustSource.volume = volume;
    }

    /*
     * Starts thrust sound loop.
     */
    public void StartThrust()
    {
        if (thrustSound == null)
        {
            return;
        }

        if (_isThrustPlaying)
        {
            return;
        }
        
        _thrustSource.clip = thrustSound;
        _thrustSource.volume = thrustVolume;
        _thrustSource.Play();
        _isThrustPlaying = true;
    }

    /*
     * Stops thrust sound loop.
     */
    public void StopThrust()
    {
        if (!_isThrustPlaying)
        {
            return;
        }

        _thrustSource.Stop();
        _isThrustPlaying = false;
    }

    /*
     * Plays coin collect sound with cooldown.
     * Prevents audio spam when collecting many coins rapidly.
     */
    public void PlayCoinSound()
    {
        if (_coinSoundCooldown > 0f)
        {
            return;
        }

        PlaySFX(coinSound, coinVolume);
        _coinSoundCooldown = coinSoundCooldownTime;
    }

    /*
     * Plays shield collect sound.
     */
    public void PlayShieldSound()
    {
        PlaySFX(shieldSound, shieldVolume);
    }

    /*
     * Plays crash sound on death.
     */
    public void PlayCrashSound()
    {
        PlaySFX(crashSound, crashVolume);
    }

    /*
     * Plays landing sound when hitting sand.
     */
    public void PlayLandingSound()
    {
        PlaySFX(landingSound, landingVolume);
    }

    /*
     * Plays drone warning beep.
     */
    public void PlayDroneWarning()
    {
        PlaySFX(droneWarningSound, droneWarningVolume);
    }

    /*
     * Plays drone attack sound on dedicated source.
     */
    public void PlayDroneAttack()
    {
        if (droneAttackSound == null)
        {
            return;
        }
        _droneSource.clip = droneAttackSound;
        _droneSource.volume = droneAttackVolume;
        _droneSource.Play();
    }

    /*
     * Stops drone attack sound immediately.
     */
    public void StopDroneAttack()
    {
        if (_droneSource.isPlaying)
        {
            _droneSource.Stop();
        }
    }

    /*
     * Fades out drone attack sound smoothly.
     *
     * @param fadeDuration - How long the fade takes in seconds.
     */
    public void FadeDroneAttack(float fadeDuration = 0.5f)
    {
        if (_droneSource.isPlaying)
        {
            StartCoroutine(FadeOutCoroutine(_droneSource, fadeDuration));
        } 
    }

    /*
     * Plays button click sound.
     */
    public void PlayButtonClick()
    {
        PlaySFX(buttonClickSound, buttonClickVolume);
    }

    /*
     * Plays button hover sound on dedicated source.
     */
    public void PlayButtonHover()
    {
        if (buttonHoverSound == null)
        {
            return;
        }
        _hoverSource.clip = buttonHoverSound;
        _hoverSource.volume = buttonHoverVolume;
        _hoverSource.Play();
    }

    /*
     * Stops hover sound when mouse leaves button.
     */
    public void StopButtonHover()
    {
        if (_hoverSource.isPlaying)
        {
            _hoverSource.Stop();
        }
    }

    /*
     * Plays full countdown sound.
     */
    public void PlayCountdownSound()
    {
        PlaySFX(countdownSound, countdownVolume);
    }

    /*
     * Plays milestone fanfare.
     */
    public void PlayMilestoneSound()
    {
        PlaySFX(milestoneSound, milestoneVolume);
    }

    /*
     * Plays game over sound.
     */
    public void PlayGameOverSound()
    {
        PlaySFX(gameOverSound, gameOverVolume);
    }

    /*
     * Plays a one shot sound effect at specified volume.
     *
     * @param clip   - Audio clip to play.
     * @param volume - Volume to play at.
     */
    private void PlaySFX(AudioClip clip, float volume)
    {
        if (clip == null)
        {
            return;
        }
        _sfxSource.PlayOneShot(clip, volume);
    }

    /*
     * Gradually reduces volume to zero then stops the source.
     * Restores original volume after stopping.
     *
     * @param source       - The AudioSource to fade out.
     * @param fadeDuration - How long the fade takes.
     */
    private IEnumerator FadeOutCoroutine(
        AudioSource source, float fadeDuration)
    {
        float startVolume = source.volume;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(
                startVolume, 0f, elapsed / fadeDuration);
            yield return null;
        }

        source.Stop();
        source.volume = startVolume;
    }
}