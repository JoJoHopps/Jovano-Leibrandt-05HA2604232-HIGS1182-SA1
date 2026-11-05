using UnityEngine;

// AudioManager is the single place all audio is controlled from.
// It owns two AudioSources: one for looping background music and one for
// one-shot sound effects. Other scripts never touch AudioSources directly;
// they just ask the AudioManager to play a sound.
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    // A list of every sound effect the game can play
    public enum SoundType { Shoot, Pickup, PlayerHit, GameOver, PowerUp, Win }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Music")]
    [SerializeField] private AudioClip backgroundMusic;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip shootClip;
    [SerializeField] private AudioClip pickupClip;
    [SerializeField] private AudioClip playerHitClip;
    [SerializeField] private AudioClip gameOverClip;
    [SerializeField] private AudioClip powerUpClip; // Optional - falls back to the pickup sound
    [SerializeField] private AudioClip winClip;     // Optional - falls back to the pickup sound

    [Header("Volume Settings")]
    [Range(0f, 1f)][SerializeField] private float musicVolume = 0.4f;
    [Range(0f, 1f)][SerializeField] private float sfxVolume = 0.8f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        SetupAudioSources();
    }

    private void Start()
    {
        ApplyVolumes();
        PlayMusic();
    }

    // Makes sure both AudioSources exist and are configured correctly,
    // even if they were not assigned in the Inspector
    private void SetupAudioSources()
    {
        if (musicSource == null) musicSource = gameObject.AddComponent<AudioSource>();
        if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();

        musicSource.loop = true;
        musicSource.playOnAwake = false;
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
    }

    private void ApplyVolumes()
    {
        musicSource.volume = musicVolume;
        sfxSource.volume = 1f; // SFX volume is applied per-sound in PlaySound
    }

    // ─────────────────────────────────────────────
    // PUBLIC METHODS
    // ─────────────────────────────────────────────

    // Static helper so other scripts can write ONE line and be null-safe:
    // AudioManager.Play(AudioManager.SoundType.Shoot);
    public static void Play(SoundType type)
    {
        if (Instance != null)
        {
            Instance.PlaySound(type);
        }
    }

    // Plays a one-shot sound effect. PlayOneShot lets sounds overlap
    // (e.g. rapid-fire lasers) without cutting each other off.
    public void PlaySound(SoundType type)
    {
        AudioClip clip = GetClip(type);

        if (clip == null)
        {
            Debug.LogWarning($"AudioManager: No clip assigned for {type}.");
            return;
        }

        sfxSource.PlayOneShot(clip, sfxVolume);
        Debug.Log($"AudioManager: Played sound {type}");
    }

    public void PlayMusic()
    {
        if (backgroundMusic == null)
        {
            Debug.LogWarning("AudioManager: No background music assigned.");
            return;
        }

        musicSource.clip = backgroundMusic;
        musicSource.Play();
        Debug.Log("AudioManager: Background music started.");
    }

    public void StopMusic()
    {
        musicSource.Stop();
        Debug.Log("AudioManager: Background music stopped.");
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        musicSource.volume = musicVolume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
    }

    // Maps each SoundType to its AudioClip
    private AudioClip GetClip(SoundType type)
    {
        switch (type)
        {
            case SoundType.Shoot: return shootClip;
            case SoundType.Pickup: return pickupClip;
            case SoundType.PlayerHit: return playerHitClip;
            case SoundType.GameOver: return gameOverClip;
            case SoundType.PowerUp: return powerUpClip != null ? powerUpClip : pickupClip;
            case SoundType.Win: return winClip != null ? winClip : pickupClip;
            default: return null;
        }
    }
}
