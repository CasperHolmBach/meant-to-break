using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class ZombieCount : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] private TextMeshProUGUI zombieCountText;
    [SerializeField] private bool createUIAutomatically = true;
    [SerializeField] private Color textColor = new Color(0.8f, 0.2f, 0.2f); // Red color
    [SerializeField] private int fontSize = 36;
    
    [Header("Behavior")]
    [SerializeField] private string enemyTag = "Enemy";
    [SerializeField] private float updateInterval = 0.2f; // More frequent updates
    [SerializeField] private float fadeSpeed = 2.0f;      // Speed of fade when counter reaches zero
    
    private int lastZombieCount = -1;  // Force update on first check
    private CanvasGroup canvasGroup;   // For fading
    private bool isFading = false;
    private bool hideWhenZero = true;
    
    // Reference to all active zombies
    private GameObject[] activeZombies;
    
    void Start()
    {
        // Create UI if needed
        if (zombieCountText == null && createUIAutomatically)
        {
            CreateZombieCountUI();
        }
        
        // Get reference to canvas group or add one
        canvasGroup = zombieCountText?.GetComponentInParent<CanvasGroup>();
        if (canvasGroup == null && zombieCountText != null)
        {
            canvasGroup = zombieCountText.gameObject.AddComponent<CanvasGroup>();
        }
        
        // Start counting zombies
        StartCoroutine(CountZombies());
    }
    
    private void CreateZombieCountUI()
    {
        // Find or create a canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("UICanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        // Create a container for the counter (with CanvasGroup for fading)
        GameObject counterContainer = new GameObject("ZombieCounterContainer");
        counterContainer.transform.SetParent(canvas.transform, false);
        
        // Add RectTransform component first
        RectTransform containerRect = counterContainer.AddComponent<RectTransform>();
        
        // Then add CanvasGroup for fading
        canvasGroup = counterContainer.AddComponent<CanvasGroup>();
        
        // Set up the container's RectTransform
        containerRect.anchorMin = new Vector2(0.5f, 1f);
        containerRect.anchorMax = new Vector2(0.5f, 1f);
        containerRect.pivot = new Vector2(0.5f, 1f);
        containerRect.anchoredPosition = new Vector2(0, -20); // 20 pixels from top
        containerRect.sizeDelta = new Vector2(300, 50);
        
        // Create text object for counter
        GameObject textObj = new GameObject("ZombieCountText");
        textObj.transform.SetParent(counterContainer.transform, false);
        
        // Add RectTransform to text object
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        
        // Add TextMeshProUGUI component
        zombieCountText = textObj.AddComponent<TextMeshProUGUI>();
        zombieCountText.alignment = TextAlignmentOptions.Center;
        zombieCountText.fontSize = fontSize;
        zombieCountText.color = textColor;
        zombieCountText.fontStyle = FontStyles.Bold;
        zombieCountText.text = "Zombies: --";
        
        // Set text RectTransform to fill the container
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        
        Debug.Log("Created zombie count UI");
    }
    
    private IEnumerator CountZombies()
    {
        while (true)
        {
            // Get a fresh count of zombies by tag - store references too
            activeZombies = GameObject.FindGameObjectsWithTag(enemyTag);
            int currentCount = activeZombies.Length;
            
            // Debug validation
            if (currentCount != lastZombieCount)
            {
                Debug.Log($"Zombie count changed from {lastZombieCount} to {currentCount}");
            }
            
            // Only update UI if the count changed
            if (currentCount != lastZombieCount)
            {
                UpdateZombieCountUI(currentCount);
                lastZombieCount = currentCount;
                
                // Handle visibility when count reaches zero
                if (currentCount == 0 && hideWhenZero && !isFading)
                {
                    StartCoroutine(FadeOutCounter());
                }
                else if (currentCount > 0 && canvasGroup != null && canvasGroup.alpha < 1)
                {
                    // Make visible again if zombies reappear
                    StopCoroutine(FadeOutCounter());
                    StartCoroutine(FadeInCounter());
                }
            }
            
            // Check if any of our stored references are now null (destroyed)
            if (activeZombies.Length > 0)
            {
                bool zombiesDestroyed = false;
                foreach (GameObject zombie in activeZombies)
                {
                    if (zombie == null)
                    {
                        zombiesDestroyed = true;
                        break;
                    }
                }
                
                // If we detect destroyed zombies, force immediate recount
                if (zombiesDestroyed)
                {
                    yield return null; // Wait one frame
                    continue; // Skip the normal wait and recount immediately
                }
            }
            
            // Wait before next regular update
            yield return new WaitForSeconds(updateInterval);
        }
    }
    
    private void UpdateZombieCountUI(int count)
    {
        if (zombieCountText != null)
        {
            zombieCountText.text = count > 0 ? $"Zombies: {count}" : "Area Clear!";
            
            // Optional: Add visual feedback for changes
            StartCoroutine(PulseTextSize());
        }
    }
    
    private IEnumerator FadeOutCounter()
    {
        isFading = true;
        
        if (canvasGroup != null)
        {
            // Wait a moment to show "Area Clear!" message
            yield return new WaitForSeconds(2f);
            
            // Fade out gradually
            while (canvasGroup.alpha > 0)
            {
                canvasGroup.alpha -= Time.deltaTime * fadeSpeed;
                yield return null;
            }
            
            // Ensure it's completely invisible
            canvasGroup.alpha = 0;
        }
        
        isFading = false;
    }
    
    private IEnumerator FadeInCounter()
    {
        isFading = true;
        
        if (canvasGroup != null)
        {
            // Fade in gradually
            while (canvasGroup.alpha < 1)
            {
                canvasGroup.alpha += Time.deltaTime * fadeSpeed * 2; // Faster fade-in
                yield return null;
            }
            
            // Ensure it's completely visible
            canvasGroup.alpha = 1;
        }
        
        isFading = false;
    }
    
    private IEnumerator PulseTextSize()
    {
        if (zombieCountText == null)
            yield break;
            
        // Store original size
        float originalSize = zombieCountText.fontSize;
        
        // Increase size briefly
        zombieCountText.fontSize = Mathf.RoundToInt(originalSize * 1.2f);
        
        // Wait a moment
        yield return new WaitForSeconds(0.1f);
        
        // Return to original size
        zombieCountText.fontSize = Mathf.RoundToInt(originalSize);
    }
    
    // Public method to manually refresh the count (useful for level loading)
    public void RefreshCount()
    {
        StopAllCoroutines();
        StartCoroutine(CountZombies());
    }
    
    // If you need immediate updates, you can also use this event-based approach
    // Add this to your ZombieController script:
    /*
    public static event System.Action OnZombieDestroyed;

    private void OnDestroy()
    {
        if (gameObject.CompareTag("Enemy"))
        {
            OnZombieDestroyed?.Invoke();
        }
    }
    */
    
    // And then in this class, add:
    /*
    private void OnEnable()
    {
        ZombieController.OnZombieDestroyed += OnZombieKilled;
    }

    private void OnDisable()
    {
        ZombieController.OnZombieDestroyed -= OnZombieKilled;
    }

    private void OnZombieKilled()
    {
        // Force immediate recount
        StartCoroutine(ImmediateRecount());
    }

    private IEnumerator ImmediateRecount()
    {
        // Wait one frame for the zombie to be fully destroyed
        yield return null;
        RefreshCount();
    }
    */
}