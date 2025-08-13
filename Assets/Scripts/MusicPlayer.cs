using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class MusicPlayer : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource musicAudioSource;
    public AudioSource sfxAudioSource; // Optional separate source for sound effects
    
    [Header("Music Tracks")]
    public AudioClip[] musicTracks;
    public bool shuffleMode = false;
    public bool loopPlaylist = true;
    public bool autoPlay = true;
    
    [Header("Volume Settings")]
    [Range(0f, 1f)]
    public float masterVolume = 1f;
    [Range(0f, 1f)]
    public float musicVolume = 0.7f;
    [Range(0f, 1f)]
    public float sfxVolume = 1f;
    
    [Header("Fade Settings")]
    public bool useFadeTransitions = true;
    public float fadeInDuration = 2f;
    public float fadeOutDuration = 1.5f;
    public float crossfadeDuration = 3f;
    
    [Header("UI Controls (Optional)")]
    public Button playButton;
    public Button pauseButton;
    public Button nextButton;
    public Button previousButton;
    public Button shuffleButton;
    public Slider volumeSlider;
    public Text trackNameText;
    public Text trackTimeText;
    
    [Header("Debug")]
    public bool showDebugInfo = true;
    
    // Private variables
    private int currentTrackIndex = 0;
    private bool isPlaying = false;
    private bool isPaused = false;
    private bool isFading = false;
    private List<int> shuffledIndices;
    private int shuffledPosition = 0;
    
    // Coroutine references
    private Coroutine fadeCoroutine;
    private Coroutine trackProgressCoroutine;

    void Start()
    {
        InitializeMusicPlayer();
    }
    
    void InitializeMusicPlayer()
    {
        // Validate audio source
        if (musicAudioSource == null)
        {
            musicAudioSource = GetComponent<AudioSource>();
            if (musicAudioSource == null)
            {
                musicAudioSource = gameObject.AddComponent<AudioSource>();
                Debug.Log("Created AudioSource component for MusicPlayer");
            }
        }
        
        // Configure audio source
        musicAudioSource.playOnAwake = false;
        musicAudioSource.loop = false; // We handle looping manually
        
        // Validate tracks
        if (musicTracks == null || musicTracks.Length == 0)
        {
            Debug.LogWarning("MusicPlayer: No music tracks assigned!");
            return;
        }
        
        // Setup shuffle list
        if (shuffleMode)
        {
            CreateShuffleList();
        }
        
        // Setup UI
        SetupUI();
        
        // Apply volume settings
        UpdateVolume();
        
        // Auto-play if enabled
        if (autoPlay)
        {
            PlayCurrentTrack();
        }
        
        // Update UI
        UpdateUI();
        
        if (showDebugInfo)
        {
            Debug.Log($"MusicPlayer initialized with {musicTracks.Length} tracks. Auto-play: {autoPlay}, Shuffle: {shuffleMode}");
        }
    }
    
    void SetupUI()
    {
        // Setup button listeners
        if (playButton != null)
            playButton.onClick.AddListener(Play);
            
        if (pauseButton != null)
            pauseButton.onClick.AddListener(Pause);
            
        if (nextButton != null)
            nextButton.onClick.AddListener(NextTrack);
            
        if (previousButton != null)
            previousButton.onClick.AddListener(PreviousTrack);
            
        if (shuffleButton != null)
            shuffleButton.onClick.AddListener(ToggleShuffle);
            
        if (volumeSlider != null)
        {
            volumeSlider.value = musicVolume;
            volumeSlider.onValueChanged.AddListener(SetMusicVolume);
        }
    }
    
    void Update()
    {
        // Check if current track finished
        if (isPlaying && !musicAudioSource.isPlaying && !isPaused && !isFading)
        {
            OnTrackFinished();
        }
        
        // Update UI periodically
        if (Time.frameCount % 30 == 0) // Every 30 frames (about twice per second)
        {
            UpdateTrackTimeUI();
        }
    }
    
    void CreateShuffleList()
    {
        shuffledIndices = new List<int>();
        for (int i = 0; i < musicTracks.Length; i++)
        {
            shuffledIndices.Add(i);
        }
        
        // Fisher-Yates shuffle
        for (int i = shuffledIndices.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            int temp = shuffledIndices[i];
            shuffledIndices[i] = shuffledIndices[randomIndex];
            shuffledIndices[randomIndex] = temp;
        }
        
        shuffledPosition = 0;
    }
    
    // Public playback controls
    public void Play()
    {
        if (musicTracks == null || musicTracks.Length == 0) return;
        
        if (isPaused)
        {
            Resume();
        }
        else
        {
            PlayCurrentTrack();
        }
    }
    
    public void Pause()
    {
        if (isPlaying && !isPaused)
        {
            isPaused = true;
            
            if (useFadeTransitions)
            {
                StartCoroutine(FadeOut(fadeOutDuration, true)); // true = pause, don't stop
            }
            else
            {
                musicAudioSource.Pause();
            }
            
            UpdateUI();
            if (showDebugInfo) Debug.Log("Music paused");
        }
    }
    
    public void Resume()
    {
        if (isPaused)
        {
            isPaused = false;
            
            if (useFadeTransitions)
            {
                musicAudioSource.UnPause();
                StartCoroutine(FadeIn(fadeInDuration));
            }
            else
            {
                musicAudioSource.UnPause();
            }
            
            UpdateUI();
            if (showDebugInfo) Debug.Log("Music resumed");
        }
    }
    
    public void Stop()
    {
        if (isPlaying)
        {
            isPlaying = false;
            isPaused = false;
            
            if (useFadeTransitions)
            {
                StartCoroutine(FadeOut(fadeOutDuration, false)); // false = stop completely
            }
            else
            {
                musicAudioSource.Stop();
            }
            
            UpdateUI();
            if (showDebugInfo) Debug.Log("Music stopped");
        }
    }
    
    public void NextTrack()
    {
        if (musicTracks == null || musicTracks.Length <= 1) return;
        
        if (shuffleMode)
        {
            shuffledPosition = (shuffledPosition + 1) % shuffledIndices.Count;
            currentTrackIndex = shuffledIndices[shuffledPosition];
        }
        else
        {
            currentTrackIndex = (currentTrackIndex + 1) % musicTracks.Length;
        }
        
        if (useFadeTransitions && isPlaying)
        {
            StartCoroutine(CrossfadeToTrack());
        }
        else
        {
            PlayCurrentTrack();
        }
        
        if (showDebugInfo) Debug.Log($"Next track: {GetCurrentTrackName()}");
    }
    
    public void PreviousTrack()
    {
        if (musicTracks == null || musicTracks.Length <= 1) return;
        
        if (shuffleMode)
        {
            shuffledPosition = (shuffledPosition - 1 + shuffledIndices.Count) % shuffledIndices.Count;
            currentTrackIndex = shuffledIndices[shuffledPosition];
        }
        else
        {
            currentTrackIndex = (currentTrackIndex - 1 + musicTracks.Length) % musicTracks.Length;
        }
        
        if (useFadeTransitions && isPlaying)
        {
            StartCoroutine(CrossfadeToTrack());
        }
        else
        {
            PlayCurrentTrack();
        }
        
        if (showDebugInfo) Debug.Log($"Previous track: {GetCurrentTrackName()}");
    }
    
    public void PlayTrack(int trackIndex)
    {
        if (trackIndex < 0 || trackIndex >= musicTracks.Length) return;
        
        currentTrackIndex = trackIndex;
        
        if (shuffleMode)
        {
            // Update shuffle position to match
            for (int i = 0; i < shuffledIndices.Count; i++)
            {
                if (shuffledIndices[i] == trackIndex)
                {
                    shuffledPosition = i;
                    break;
                }
            }
        }
        
        if (useFadeTransitions && isPlaying)
        {
            StartCoroutine(CrossfadeToTrack());
        }
        else
        {
            PlayCurrentTrack();
        }
    }
    
    void PlayCurrentTrack()
    {
        if (musicTracks[currentTrackIndex] == null)
        {
            Debug.LogError($"Music track at index {currentTrackIndex} is null!");
            return;
        }
        
        musicAudioSource.clip = musicTracks[currentTrackIndex];
        
        if (useFadeTransitions)
        {
            musicAudioSource.volume = 0f;
            musicAudioSource.Play();
            StartCoroutine(FadeIn(fadeInDuration));
        }
        else
        {
            musicAudioSource.Play();
        }
        
        isPlaying = true;
        isPaused = false;
        
        UpdateUI();
        
        if (showDebugInfo)
        {
            Debug.Log($"Playing: {GetCurrentTrackName()}");
        }
    }
    
    void OnTrackFinished()
    {
        if (loopPlaylist)
        {
            NextTrack();
        }
        else
        {
            // Stop playing if we've reached the end
            if ((!shuffleMode && currentTrackIndex == musicTracks.Length - 1) ||
                (shuffleMode && shuffledPosition == shuffledIndices.Count - 1))
            {
                Stop();
            }
            else
            {
                NextTrack();
            }
        }
    }
    
    // Volume controls
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateVolume();
    }
    
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        UpdateVolume();
    }
    
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (sfxAudioSource != null)
        {
            sfxAudioSource.volume = masterVolume * sfxVolume;
        }
    }
    
    void UpdateVolume()
    {
        if (musicAudioSource != null && !isFading)
        {
            musicAudioSource.volume = masterVolume * musicVolume;
        }
    }
    
    // Shuffle control
    public void ToggleShuffle()
    {
        shuffleMode = !shuffleMode;
        
        if (shuffleMode)
        {
            CreateShuffleList();
            // Find current track in shuffle list
            for (int i = 0; i < shuffledIndices.Count; i++)
            {
                if (shuffledIndices[i] == currentTrackIndex)
                {
                    shuffledPosition = i;
                    break;
                }
            }
        }
        
        UpdateUI();
        if (showDebugInfo) Debug.Log($"Shuffle mode: {shuffleMode}");
    }
    
    // Fade coroutines
    IEnumerator FadeIn(float duration)
    {
        isFading = true;
        float targetVolume = masterVolume * musicVolume;
        float startVolume = 0f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            musicAudioSource.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
            yield return null;
        }
        
        musicAudioSource.volume = targetVolume;
        isFading = false;
    }
    
    IEnumerator FadeOut(float duration, bool pauseNotStop)
    {
        isFading = true;
        float startVolume = musicAudioSource.volume;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            musicAudioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }
        
        musicAudioSource.volume = 0f;
        
        if (pauseNotStop)
        {
            musicAudioSource.Pause();
        }
        else
        {
            musicAudioSource.Stop();
        }
        
        isFading = false;
    }
    
    IEnumerator CrossfadeToTrack()
    {
        isFading = true;
        
        // Fade out current track
        float startVolume = musicAudioSource.volume;
        float elapsed = 0f;
        
        while (elapsed < crossfadeDuration / 2)
        {
            elapsed += Time.deltaTime;
            musicAudioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / (crossfadeDuration / 2));
            yield return null;
        }
        
        // Switch track
        musicAudioSource.clip = musicTracks[currentTrackIndex];
        musicAudioSource.Play();
        
        // Fade in new track
        float targetVolume = masterVolume * musicVolume;
        elapsed = 0f;
        
        while (elapsed < crossfadeDuration / 2)
        {
            elapsed += Time.deltaTime;
            musicAudioSource.volume = Mathf.Lerp(0f, targetVolume, elapsed / (crossfadeDuration / 2));
            yield return null;
        }
        
        musicAudioSource.volume = targetVolume;
        isFading = false;
        
        UpdateUI();
    }
    
    // UI Updates
    void UpdateUI()
    {
        if (trackNameText != null)
        {
            trackNameText.text = GetCurrentTrackName();
        }
        
        if (volumeSlider != null && !isFading)
        {
            volumeSlider.value = musicVolume;
        }
        
        // Update button states
        if (playButton != null)
            playButton.interactable = !isPlaying || isPaused;
            
        if (pauseButton != null)
            pauseButton.interactable = isPlaying && !isPaused;
    }
    
    void UpdateTrackTimeUI()
    {
        if (trackTimeText != null && musicAudioSource.clip != null)
        {
            float currentTime = musicAudioSource.time;
            float totalTime = musicAudioSource.clip.length;
            
            string currentStr = FormatTime(currentTime);
            string totalStr = FormatTime(totalTime);
            
            trackTimeText.text = $"{currentStr} / {totalStr}";
        }
    }
    
    string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60);
        return $"{minutes:00}:{seconds:00}";
    }
    
    string GetCurrentTrackName()
    {
        if (musicTracks == null || currentTrackIndex >= musicTracks.Length || musicTracks[currentTrackIndex] == null)
            return "No Track";
            
        return musicTracks[currentTrackIndex].name;
    }
    
    // Public getters
    public bool IsPlaying => isPlaying;
    public bool IsPaused => isPaused;
    public int CurrentTrackIndex => currentTrackIndex;
    public string CurrentTrackName => GetCurrentTrackName();
    public float TrackProgress => musicAudioSource.clip != null ? musicAudioSource.time / musicAudioSource.clip.length : 0f;
}
