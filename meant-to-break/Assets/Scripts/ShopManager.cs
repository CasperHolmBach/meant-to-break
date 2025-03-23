using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI; 

public class ShopManager : MonoBehaviour
{
    [Header("Shop Settings")]
    [SerializeField] private string shopName = "Weapon Shop";
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private KeyCode interactKey = KeyCode.B;
    
    [Header("Items")]
    [SerializeField] private ShopItem[] availableItems;
    [SerializeField] private int currentItemIndex = 0;
    
    [Header("Simple UI")]
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private float promptYPosition = 150f;
    
    // Player reference
    private Transform playerTransform;
    private bool isInRange = false;
    private bool isShopOpen = false;
    private TextMeshProUGUI createdPromptText;
    
    void Start()
    {
        // Find player if not assigned
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }
        
        // Create the prompt text if not assigned
        if (promptText == null)
        {
            CreatePromptText();
        }
        else
        {
            createdPromptText = promptText;
        }
        
        // Hide prompt initially
        ShowPrompt(false, "");
    }
    
    void Update()
    {
        // Check if player is in range
        if (playerTransform != null)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            bool wasInRange = isInRange;
            isInRange = distance <= interactionDistance;
            
            // Show or hide prompt when entering/exiting range
            if (isInRange != wasInRange)
            {
                if (isInRange)
                {
                    if (!isShopOpen)
                    {
                        // Display shop prompt
                        ShowPrompt(true, $"Press {interactKey} to shop at {shopName}");
                    }
                }
                else
                {
                    // Hide prompt when out of range
                    ShowPrompt(false, "");
                    
                    // Close shop if open
                    if (isShopOpen)
                    {
                        CloseShop();
                    }
                }
            }
        }
        
        // Toggle shop mode
        if (isInRange && Input.GetKeyDown(interactKey))
        {
            if (isShopOpen)
            {
                CloseShop();
            }
            else
            {
                OpenShop();
            }
        }
        
        // Navigation in shop
        if (isShopOpen)
        {
            // Next item
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                currentItemIndex = (currentItemIndex + 1) % availableItems.Length;
                UpdateShopPrompt();
            }
            
            // Previous item
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                currentItemIndex = (currentItemIndex - 1 + availableItems.Length) % availableItems.Length;
                UpdateShopPrompt();
            }
            
            // Purchase item
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                PurchaseCurrentItem();
            }
        }
    }
    
    private void CreatePromptText()
    {
        // Create canvas if needed
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("ShopCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        // Create text object
        GameObject textObj = new GameObject("ShopPromptText");
        textObj.transform.SetParent(canvas.transform, false);
        
        // Add TextMeshProUGUI component
        createdPromptText = textObj.AddComponent<TextMeshProUGUI>();
        createdPromptText.alignment = TextAlignmentOptions.Center;
        createdPromptText.fontSize = 24;
        createdPromptText.color = Color.white;
        
        // Position the text
        RectTransform rectTransform = textObj.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0);
        rectTransform.anchorMax = new Vector2(0.5f, 0);
        rectTransform.pivot = new Vector2(0.5f, 0);
        rectTransform.sizeDelta = new Vector2(600, 50);
        rectTransform.anchoredPosition = new Vector2(0, promptYPosition);
        
        // Hide initially
        textObj.SetActive(false);
    }
    
    private void ShowPrompt(bool show, string message)
    {
        if (createdPromptText != null)
        {
            createdPromptText.gameObject.SetActive(show);
            if (show)
            {
                createdPromptText.text = message;
            }
        }
    }
    
    private void OpenShop()
    {
        isShopOpen = true;
        currentItemIndex = 0;
        UpdateShopPrompt();
    }
    
    private void CloseShop()
    {
        isShopOpen = false;
        
        // Show the basic interaction prompt again
        if (isInRange)
        {
            ShowPrompt(true, $"Press {interactKey} to shop at {shopName}");
        }
    }
    
    private void UpdateShopPrompt()
    {
        if (currentItemIndex >= 0 && currentItemIndex < availableItems.Length)
        {
            ShopItem item = availableItems[currentItemIndex];
            int playerMoney = CurrencyManager.Instance != null ? CurrencyManager.Instance.GetMoney() : 0;
            bool canAfford = playerMoney >= item.price;
            
            string affordText = canAfford ? "Can afford" : "Cannot afford";
            string navText = availableItems.Length > 1 ? " [←/→ to browse]" : "";
            
            string promptMessage = 
                $"{item.itemName} - {item.price} coins\n" +
                $"{item.description}\n" +
                $"Your money: {playerMoney} - {affordText}\n" +
                $"Press ENTER buy{navText} (B to close)";
                
            ShowPrompt(true, promptMessage);
        }
    }
    
    private void PurchaseCurrentItem()
    {
        if (currentItemIndex < 0 || currentItemIndex >= availableItems.Length)
            return;
            
        ShopItem item = availableItems[currentItemIndex];
        
        // Try to purchase
        if (CurrencyManager.Instance != null && CurrencyManager.Instance.SpendMoney(item.price))
        {
            // Purchase successful
            GiveItemToPlayer(item);
            
            // Update prompt to show success
            string successMessage = $"Purchased {item.itemName}!";
            StartCoroutine(ShowTempMessage(successMessage, 1.5f));
        }
        else
        {
            // Not enough money
            StartCoroutine(ShowTempMessage("Not enough coins!", 1.5f));
        }
    }
    
    private void GiveItemToPlayer(ShopItem item)
    {
        if (item.itemPrefab != null && playerTransform != null)
        {
            // Calculate a safe spawn position - right at the player's position
            // with a small upward offset to prevent colliding with the ground
            Vector3 spawnPosition = playerTransform.position + Vector3.up * 0.5f;
            
            // Spawn the item at the player's position
            GameObject spawnedItem = Instantiate(item.itemPrefab, spawnPosition, Quaternion.identity);
            
            // Optional visual effects
            StartCoroutine(BounceEffect(spawnedItem));
            
            Debug.Log($"Item {item.itemName} spawned at player's position");
        }
        else
        {
            if (item.itemPrefab == null)
                Debug.LogError($"Item {item.itemName} has no prefab assigned!");
            
            if (playerTransform == null)
                Debug.LogError("Cannot spawn item: Player not found!");
        }
    }
    
    private IEnumerator ShowTempMessage(string message, float duration)
    {
        // Save the current shop prompt
        string originalMessage = createdPromptText.text;
        
        // Show the temporary message
        ShowPrompt(true, message);
        
        yield return new WaitForSeconds(duration);
        
        // Restore the original message if we're still in shop mode
        if (isShopOpen)
        {
            UpdateShopPrompt();
        }
        else if (isInRange)
        {
            ShowPrompt(true, $"Press {interactKey} to shop at {shopName}");
        }
    }
    
    private IEnumerator BounceEffect(GameObject item)
    {
        if (item == null) yield break;
        
        Vector3 originalPosition = item.transform.position;
        float duration = 0.5f;
        float height = 0.3f;
        
        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (item == null) yield break;
            
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float yOffset = height * Mathf.Sin(t * Mathf.PI);
            
            item.transform.position = new Vector3(
                originalPosition.x,
                originalPosition.y + yOffset,
                originalPosition.z
            );
            
            yield return null;
        }
        
        if (item != null)
        {
            item.transform.position = originalPosition;
        }
    }
    
    // Visualize interaction range in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}