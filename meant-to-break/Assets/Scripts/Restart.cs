using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Restart : MonoBehaviour
{
    [Header("Restart Settings")]
    [SerializeField] private string sceneToLoad = ""; // Leave empty to reload current scene
    [SerializeField] private float transitionDelay = 0.5f; // Optional delay before scene change
    
    [Header("Optional Effects")]
    [SerializeField] private bool fadeOut = false;
    [SerializeField] private Image fadePanel; // Assign a full-screen black panel for fade effect
    [SerializeField] private float fadeDuration = 1.0f;
    [SerializeField] private AudioClip buttonSound;
    
    private Button restartButton;
    
    private void Start()
    {
        // Get the button component
        restartButton = GetComponent<Button>();
        
        // Add listener to the button
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }
        else
        {
            Debug.LogError("Restart script needs to be attached to a UI Button!");
        }
        
        // Initialize fade panel if used
        if (fadeOut && fadePanel != null)
        {
            // Start with transparent panel
            Color panelColor = fadePanel.color;
            panelColor.a = 0f;
            fadePanel.color = panelColor;
            fadePanel.gameObject.SetActive(true);
        }
    }
    
    public void RestartGame()
    {
        // Play button sound if assigned
        if (buttonSound != null)
        {
            AudioSource.PlayClipAtPoint(buttonSound, Camera.main.transform.position);
        }
        
        // Disable the button to prevent multiple clicks
        if (restartButton != null)
        {
            restartButton.interactable = false;
        }
        
        if (fadeOut && fadePanel != null)
        {
            // Start fade out transition
            StartCoroutine(FadeOutAndRestart());
        }
        else
        {
            // Simple delayed restart
            if (transitionDelay > 0)
            {
                Invoke("LoadScene", transitionDelay);
            }
            else
            {
                LoadScene();
            }
        }
        
        Debug.Log("Restarting game...");
    }
    
    private System.Collections.IEnumerator FadeOutAndRestart()
    {
        float elapsedTime = 0;
        Color panelColor = fadePanel.color;
        
        // Fade from transparent to black
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsedTime / fadeDuration);
            
            panelColor.a = normalizedTime;
            fadePanel.color = panelColor;
            
            yield return null;
        }
        
        // Ensure we're fully faded out
        panelColor.a = 1f;
        fadePanel.color = panelColor;
        
        // Load the scene
        LoadScene();
    }
    
    private void LoadScene()
    {
        // Get scene to load
        string sceneToRestart = string.IsNullOrEmpty(sceneToLoad) ? 
            SceneManager.GetActiveScene().name : sceneToLoad;
        
        // Load the scene
        SceneManager.LoadScene(sceneToRestart);
    }
    
    // Public method that can be called from other scripts if needed
    public void RestartWithCustomScene(string sceneName)
    {
        sceneToLoad = sceneName;
        RestartGame();
    }
}