using UnityEngine;
using UnityEngine.UI;

public class MusicPlayerUI : MonoBehaviour
{
    [Header("Music Player Reference")]
    public MusicPlayer musicPlayer;
    
    [Header("UI Elements")]
    public Button playPauseButton;
    public Button stopButton;
    public Button nextButton;
    public Button previousButton;
    public Button shuffleButton;
    public Slider volumeSlider;
    public Slider progressSlider;
    public Text trackNameText;
    public Text trackTimeText;
    public Text playPauseButtonText;
    public Text shuffleButtonText;
    
    [Header("UI Settings")]
    public bool autoFindMusicPlayer = true;
    public bool updateProgressSlider = true;
    public float progressUpdateRate = 0.1f;
    
    private float lastProgressUpdate;

    void Start()
    {
        // Auto-find music player if not assigned
        if (musicPlayer == null && autoFindMusicPlayer)
        {
            musicPlayer = FindObjectOfType<MusicPlayer>();
            if (musicPlayer == null)
            {
                Debug.LogError("MusicPlayerUI: No MusicPlayer found in scene!");
                return;
            }
        }
        
        SetupUI();
    }
    
    void SetupUI()
    {
        // Setup button listeners
        if (playPauseButton != null)
            playPauseButton.onClick.AddListener(TogglePlayPause);
            
        if (stopButton != null)
            stopButton.onClick.AddListener(() => musicPlayer.Stop());
            
        if (nextButton != null)
            nextButton.onClick.AddListener(() => musicPlayer.NextTrack());
            
        if (previousButton != null)
            previousButton.onClick.AddListener(() => musicPlayer.PreviousTrack());
            
        if (shuffleButton != null)
            shuffleButton.onClick.AddListener(() => musicPlayer.ToggleShuffle());
            
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.AddListener(musicPlayer.SetMusicVolume);
            volumeSlider.value = musicPlayer.musicVolume;
        }
        
        if (progressSlider != null)
        {
            progressSlider.onValueChanged.AddListener(OnProgressSliderChanged);
        }
    }
    
    void Update()
    {
        if (musicPlayer == null) return;
        
        // Update UI elements
        UpdatePlayPauseButton();
        UpdateShuffleButton();
        UpdateTrackInfo();
        
        // Update progress slider
        if (updateProgressSlider && Time.time - lastProgressUpdate > progressUpdateRate)
        {
            UpdateProgressSlider();
            lastProgressUpdate = Time.time;
        }
    }
    
    void TogglePlayPause()
    {
        if (musicPlayer.IsPlaying && !musicPlayer.IsPaused)
        {
            musicPlayer.Pause();
        }
        else
        {
            musicPlayer.Play();
        }
    }
    
    void UpdatePlayPauseButton()
    {
        if (playPauseButtonText != null)
        {
            if (musicPlayer.IsPlaying && !musicPlayer.IsPaused)
            {
                playPauseButtonText.text = "⏸️"; // Pause symbol
            }
            else
            {
                playPauseButtonText.text = "▶️"; // Play symbol
            }
        }
    }
    
    void UpdateShuffleButton()
    {
        if (shuffleButtonText != null)
        {
            shuffleButtonText.text = musicPlayer.shuffleMode ? "🔀 ON" : "🔀 OFF";
        }
        
        if (shuffleButton != null)
        {
            // Change button color based on shuffle state
            ColorBlock colors = shuffleButton.colors;
            colors.normalColor = musicPlayer.shuffleMode ? Color.green : Color.white;
            shuffleButton.colors = colors;
        }
    }
    
    void UpdateTrackInfo()
    {
        if (trackNameText != null)
        {
            trackNameText.text = musicPlayer.CurrentTrackName;
        }
        
        if (trackTimeText != null && musicPlayer.musicAudioSource.clip != null)
        {
            float currentTime = musicPlayer.musicAudioSource.time;
            float totalTime = musicPlayer.musicAudioSource.clip.length;
            
            trackTimeText.text = $"{FormatTime(currentTime)} / {FormatTime(totalTime)}";
        }
    }
    
    void UpdateProgressSlider()
    {
        if (progressSlider != null && musicPlayer.musicAudioSource.clip != null)
        {
            progressSlider.value = musicPlayer.TrackProgress;
        }
    }
    
    void OnProgressSliderChanged(float value)
    {
        if (musicPlayer.musicAudioSource.clip != null)
        {
            // Only seek if user is manually changing the slider
            float newTime = value * musicPlayer.musicAudioSource.clip.length;
            musicPlayer.musicAudioSource.time = newTime;
        }
    }
    
    string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60);
        return $"{minutes:00}:{seconds:00}";
    }
    
    // Public methods for external controls
    public void SetVolume(float volume)
    {
        musicPlayer.SetMusicVolume(volume);
        if (volumeSlider != null)
            volumeSlider.value = volume;
    }
    
    public void PlaySpecificTrack(int trackIndex)
    {
        musicPlayer.PlayTrack(trackIndex);
    }
}
