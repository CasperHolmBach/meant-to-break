using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour
{
    public int maxHealth = 100;
    [SerializeField] private int currentHealth;
    
    [Header("UI Settings")]
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private Slider healthBar;
    [SerializeField] private bool createUIAutomatically = true;
    [SerializeField] private bool isPlayerHealth = false; // Whether this is the player's health
    
    void Start()
    {
        // Set default current health if not set
        if (currentHealth <= 0)
            currentHealth = maxHealth;
        
        // Only create UI for player health
        if (createUIAutomatically && healthText == null && 
            (isPlayerHealth || gameObject.CompareTag("Player")))
        {
            CreateHealthUI();
        }
        
        // Always update the UI if it exists
        UpdateHealthUI();
    }
    
    void Update()
    {
        if(currentHealth <= 0)
        {
            // Handle death
            if(gameObject.CompareTag("Player"))
            {
                // You might want a game over screen instead of destroying the player
                Debug.Log("Player died!");
                Destroy(gameObject);
            }
            else
            {
                // Enemy death
                Destroy(gameObject);
            }        
        }
    }
    
    public void Applyhealing(GameObject healingItem, int healing)
    {
        currentHealth = Mathf.Min(currentHealth + healing, maxHealth);
        UpdateHealthUI();
        if (healingItem != null)
            Destroy(healingItem);
    }
    
    public void TakeDamage(int damage)
    {
        currentHealth = Mathf.Max(currentHealth - damage, 0);
        UpdateHealthUI();
    }
    
    private void UpdateHealthUI()
    {
        // Only update UI if it exists (which it will only for the player)
        if (healthText != null)
        {
            healthText.text = $"Health: {currentHealth} / {maxHealth}";
        }
        
        if (healthBar != null)
        {
            healthBar.value = (float)currentHealth / maxHealth;
        }
    }
    
    private void CreateHealthUI()
    {
        // Only create UI for the player
        if (!isPlayerHealth && !gameObject.CompareTag("Player"))
        {
            Debug.Log($"Not creating UI for non-player: {gameObject.name}");
            return;
        }
        
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
        
        // The rest of the UI creation code remains the same...
        // Create a panel for the health display
        GameObject healthPanel = new GameObject("HealthPanel");
        healthPanel.transform.SetParent(canvas.transform, false);
        healthPanel.AddComponent<RectTransform>();
        
        // Add a panel background
        Image panelImage = healthPanel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.5f);
        
        // Position in the top right corner
        RectTransform panelRect = healthPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(1, 1);
        panelRect.anchorMax = new Vector2(1, 1);
        panelRect.pivot = new Vector2(1, 1);
        panelRect.sizeDelta = new Vector2(200, 50);
        panelRect.anchoredPosition = new Vector2(-10, -10);
        
        // Create health text
        GameObject textObj = new GameObject("HealthText");
        textObj.AddComponent<RectTransform>();
        textObj.transform.SetParent(healthPanel.transform, false);
        
        healthText = textObj.AddComponent<TextMeshProUGUI>();
        healthText.alignment = TextAlignmentOptions.Center;
        healthText.fontSize = 20;
        healthText.color = Color.white;
        
        // Position the text
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0, 0.5f);
        textRect.anchorMax = new Vector2(1, 1);
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
        
        // Create the main slider object
        GameObject sliderObj = new GameObject("HealthBar");
        sliderObj.AddComponent<RectTransform>();
        sliderObj.transform.SetParent(healthPanel.transform, false);
        
        // Position the slider
        RectTransform sliderRect = sliderObj.GetComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0, 0);
        sliderRect.anchorMax = new Vector2(1, 0.4f);
        sliderRect.sizeDelta = new Vector2(-20, 0);
        sliderRect.anchoredPosition = Vector2.zero;
        
        // Create the background
        GameObject bgObj = new GameObject("Background");
        bgObj.AddComponent<RectTransform>();
        bgObj.transform.SetParent(sliderObj.transform, false);
        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = Color.gray;
        
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0, 0);
        bgRect.anchorMax = new Vector2(1, 1);
        bgRect.sizeDelta = Vector2.zero;
        
        // Create fill area container
        GameObject fillAreaObj = new GameObject("Fill Area");
        fillAreaObj.AddComponent<RectTransform>();
        fillAreaObj.transform.SetParent(sliderObj.transform, false);
        
        RectTransform fillAreaRect = fillAreaObj.GetComponent<RectTransform>();
        fillAreaRect.anchorMin = new Vector2(0, 0);
        fillAreaRect.anchorMax = new Vector2(1, 1);
        fillAreaRect.sizeDelta = new Vector2(-5, -5);
        fillAreaRect.anchoredPosition = new Vector2(-2.5f, 0);
        
        // Create fill image
        GameObject fillObj = new GameObject("Fill");
        fillObj.AddComponent<RectTransform>();
        fillObj.transform.SetParent(fillAreaObj.transform, false);
        
        Image fillImage = fillObj.AddComponent<Image>();
        fillImage.color = Color.red;
        
        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0, 0);
        fillRect.anchorMax = new Vector2(0, 1); // Important: Set max X to 0 for proper fill
        fillRect.sizeDelta = new Vector2(0, 0);
        fillRect.pivot = new Vector2(0, 0.5f);
        
        // Add slider component last, after all elements are in place
        healthBar = sliderObj.AddComponent<Slider>();
        healthBar.transition = Selectable.Transition.None;
        healthBar.navigation = new Navigation { mode = Navigation.Mode.None };
        healthBar.interactable = false;
        healthBar.fillRect = fillRect;
        healthBar.handleRect = null;
        healthBar.targetGraphic = null;
        healthBar.direction = Slider.Direction.LeftToRight;
        healthBar.minValue = 0f;
        healthBar.maxValue = 1f;
        healthBar.wholeNumbers = false;
        healthBar.value = (float)currentHealth / maxHealth;
        
        // Initialize health display
        UpdateHealthUI();
        
        Debug.Log($"Health UI created for player with health: {currentHealth}/{maxHealth}");
    }
    
    public int GetCurrentHealth()
    {
        return currentHealth;
    }
    
    public float GetHealthPercentage()
    {
        return (float)currentHealth / maxHealth;
    }
}