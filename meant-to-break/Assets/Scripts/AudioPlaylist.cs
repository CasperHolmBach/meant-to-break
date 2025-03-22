using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlaylist : MonoBehaviour
{
    [Header("Playlist Configuration")]
    [SerializeField] private AudioClip[] songs;
    [SerializeField] private bool shuffle = false;
    [SerializeField] private bool playOnAwake = true;
    [SerializeField] private bool loop = true;
    [SerializeField] private float fadeTime = 1.0f; // Crossfade time
    
    [Header("Volume Settings")]
    [SerializeField] private float masterVolume = 0.5f;
    
    // Private variables
    private AudioSource audioSource;
    private int currentSongIndex = 0;
    private List<int> playbackOrder = new List<int>();
    private bool isChangingSong = false;
    
    private void Awake()
    {
        // Get or add audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Configure audio source
        audioSource.loop = false;
        audioSource.playOnAwake = false;
        audioSource.volume = masterVolume;
        
        // Generate initial playback order
        GeneratePlaybackOrder();
        
        // Start playing if enabled
        if (playOnAwake && songs.Length > 0)
        {
            PlayCurrentSong();
        }
    }
    
    private void Update()
    {
        // Check if current song has finished and we're not already changing songs
        if (audioSource.clip != null && !audioSource.isPlaying && !isChangingSong)
        {
            PlayNextSong();
        }
    }
    
    private void GeneratePlaybackOrder()
    {
        playbackOrder.Clear();
        
        if (shuffle)
        {
            // Create shuffled playlist
            List<int> indices = new List<int>();
            for (int i = 0; i < songs.Length; i++)
            {
                indices.Add(i);
            }
            
            // Fisher-Yates shuffle
            for (int i = indices.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                int temp = indices[i];
                indices[i] = indices[j];
                indices[j] = temp;
            }
            
            playbackOrder = indices;
        }
        else
        {
            // Sequential playlist
            for (int i = 0; i < songs.Length; i++)
            {
                playbackOrder.Add(i);
            }
        }
    }
    
    public void PlayCurrentSong()
    {
        if (songs.Length == 0) return;
        
        AudioClip nextClip = songs[playbackOrder[currentSongIndex]];
        audioSource.clip = nextClip;
        audioSource.Play();
        
        Debug.Log($"Now playing: {nextClip.name}");
    }
    
    public void PlayNextSong()
    {
        if (songs.Length == 0) return;
        
        // Advance to next song
        currentSongIndex++;
        
        // Loop back if we've reached the end
        if (currentSongIndex >= playbackOrder.Count)
        {
            if (loop)
            {
                currentSongIndex = 0;
                
                // Regenerate playback order if shuffling
                if (shuffle)
                {
                    GeneratePlaybackOrder();
                }
            }
            else
            {
                // Stop playback if we're at the end and not looping
                currentSongIndex = playbackOrder.Count - 1;
                return;
            }
        }
        
        // Start song transition
        StartCoroutine(FadeToNextSong());
    }
    
    public void PlayPreviousSong()
    {
        if (songs.Length == 0) return;
        
        // If we're more than 3 seconds into the song, restart it instead
        if (audioSource.time > 3f)
        {
            audioSource.time = 0f;
            return;
        }
        
        // Go to previous song
        currentSongIndex--;
        
        // Loop to end if at beginning
        if (currentSongIndex < 0)
        {
            currentSongIndex = playbackOrder.Count - 1;
        }
        
        // Fade to previous song
        StartCoroutine(FadeToNextSong());
    }
    
    public void PlayRandomSong()
    {
        if (songs.Length == 0) return;
        
        int previousIndex = currentSongIndex;
        
        // Keep selecting until we find a different song
        do
        {
            currentSongIndex = Random.Range(0, songs.Length);
        } while (songs.Length > 1 && currentSongIndex == previousIndex);
        
        StartCoroutine(FadeToNextSong());
    }
    
    public void SetShuffle(bool enableShuffle)
    {
        if (shuffle != enableShuffle)
        {
            shuffle = enableShuffle;
            GeneratePlaybackOrder();
        }
    }
    
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        audioSource.volume = masterVolume;
    }
    
    private IEnumerator FadeToNextSong()
    {
        // Prevent multiple fades
        if (isChangingSong) yield break;
        isChangingSong = true;
        
        // Store original volume
        float originalVolume = audioSource.volume;
        
        // Fade out
        float elapsed = 0f;
        while (elapsed < fadeTime/2f)
        {
            elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(originalVolume, 0f, elapsed / (fadeTime/2f));
            yield return null;
        }
        
        // Change track
        PlayCurrentSong();
        
        // Fade in
        elapsed = 0f;
        while (elapsed < fadeTime/2f)
        {
            elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, originalVolume, elapsed / (fadeTime/2f));
            yield return null;
        }
        
        // Ensure volume is back to original
        audioSource.volume = originalVolume;
        
        isChangingSong = false;
    }
}