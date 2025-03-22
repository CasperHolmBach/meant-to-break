using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopManager : MonoBehaviour
{
    [Header("Shop Settings")]
    [SerializeField] private string shopName = "General Store";
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private KeyCode interactKey = KeyCode.B;
    
    [Header("Shop Items")]
    [SerializeField] private ShopItem[] availableItems;
    
    [Header("UI References")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private TextMeshProUGUI shopNameText;
    [SerializeField] private TextMeshProUGUI playerMoneyText;
    [SerializeField] private Transform itemsContainer;
    [SerializeField] private GameObject itemButtonPrefab;
    
    [Header("Item Details Panel")]
    [SerializeField] private GameObject detailsPanel;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemDescriptionText;
    [SerializeField] private TextMeshProUGUI itemPriceText;
    [SerializeField] private Image itemIconImage;
    [SerializeField] private Button purchaseButton;
    
    private bool isInRange = false;
    private bool isShopOpen = false;
    private ShopItem selectedItem;
    
    void Start()
    {
        // Find player if not set
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }
        
        // Initialize UI
        CloseShop();
        
        // Create the shop items UI
        PopulateShopItems();
    }
    
    void Update()
    {
        // Check if player is in range
        if (playerTransform != null)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            isInRange = distance <= interactionDistance;
        }
        
        // Toggle shop when interact key is pressed
        if (isInRange && Input.GetKeyDown(interactKey))
        {
            if (isShopOpen)
                CloseShop();
            else
                OpenShop();
        }
        
        // Show "Press E to interact" when in range and shop is closed
        // (You would implement this with a UI element)
    }
    
    private void PopulateShopItems()
    {
        // Clear existing items
        foreach (Transform child in itemsContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Create new item buttons
        foreach (ShopItem item in availableItems)
        {
            GameObject buttonObj = Instantiate(itemButtonPrefab, itemsContainer);
            
            // Set up the button with item info
            ShopItemButton itemButton = buttonObj.GetComponent<ShopItemButton>();
            if (itemButton != null)
            {
                itemButton.Initialize(item, this);
            }
        }
    }
    
    public void SelectItem(ShopItem item)
    {
        selectedItem = item;
        
        // Update item details panel
        itemNameText.text = item.itemName;
        itemDescriptionText.text = item.description;
        itemPriceText.text = item.price.ToString() + " coins";
        
        if (item.icon != null)
            itemIconImage.sprite = item.icon;
            
        // Show details panel
        detailsPanel.SetActive(true);
        
        // Update purchase button status
        UpdatePurchaseButtonStatus();
    }
    
    private void UpdatePurchaseButtonStatus()
    {
        // Enable purchase button only if player has enough money
        bool canAfford = CurrencyManager.Instance != null && 
                          CurrencyManager.Instance.GetMoney() >= selectedItem.price;
                          
        purchaseButton.interactable = canAfford;
    }
    
    public void PurchaseSelectedItem()
    {
        if (selectedItem == null) return;
        
        // Try to purchase the item
        if (CurrencyManager.Instance != null && 
            CurrencyManager.Instance.SpendMoney(selectedItem.price))
        {
            // Purchase successful
            Debug.Log($"Purchased {selectedItem.itemName} for {selectedItem.price} coins");
            
            // Give the item to the player
            GiveItemToPlayer(selectedItem);
            
            // Update the UI
            UpdatePlayerMoneyDisplay();
            UpdatePurchaseButtonStatus();
        }
        else
        {
            // Purchase failed
            Debug.Log("Not enough money to purchase item");
            
            // You could show a message to the player here
        }
    }
    
    private void GiveItemToPlayer(ShopItem item)
    {
        if (item.itemPrefab != null)
        {
            // Calculate position in front of the shop
            Vector3 dropPosition = transform.position + transform.forward * 1.5f;
            
            // Add a slight vertical offset to prevent clipping with the floor
            dropPosition.y += 0.5f;
            
            // Instantiate the item at the drop position
            GameObject spawnedItem = Instantiate(item.itemPrefab, dropPosition, Quaternion.identity);
            
            // Make the item noticeable - optional effects
            AddDropEffects(spawnedItem);
            
            Debug.Log($"Dropped {item.itemName} in front of the shop");
        }
        else
        {
            Debug.LogWarning($"No prefab assigned for item: {item.itemName}");
        }
    }

    // Add visual effects to dropped items
    private void AddDropEffects(GameObject item)
    {
        // Optional: Add a light to make the item more visible
        Light highlightLight = item.AddComponent<Light>();
        highlightLight.color = Color.yellow;
        highlightLight.intensity = 1.5f;
        highlightLight.range = 2f;
        
        // Optional: Add a simple bounce effect
        StartCoroutine(BounceEffect(item));
    }

    // Simple animation to make the item bounce slightly
    private IEnumerator BounceEffect(GameObject item)
    {
        if (item == null) yield break;
        
        Vector3 originalPosition = item.transform.position;
        float duration = 0.5f;
        float height = 0.3f;
        
        // Simple bounce up
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
        
        // Ensure item is at the final position
        if (item != null)
        {
            item.transform.position = originalPosition;
        }
        
        // Optional: Make the light fade out after the bounce
        Light light = item.GetComponent<Light>();
        if (light != null)
        {
            StartCoroutine(FadeOutLight(light));
        }
    }

    // Fade out the highlight light
    private IEnumerator FadeOutLight(Light light)
    {
        float duration = 1.0f;
        float initialIntensity = light.intensity;
        
        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (light == null) yield break;
            
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            light.intensity = Mathf.Lerp(initialIntensity, 0, t);
            
            yield return null;
        }
        
        // Remove the light component when done
        if (light != null)
        {
            Destroy(light);
        }
    }
    
    private void OpenShop()
    {
        isShopOpen = true;
        
        // Show shop UI
        shopPanel.SetActive(true);
        
        // Set shop name
        if (shopNameText != null)
            shopNameText.text = shopName;
        
        // Update player money display
        UpdatePlayerMoneyDisplay();
        
        // Hide details panel initially
        detailsPanel.SetActive(false);
        
        // Optional: Pause game, disable player movement, etc.
        Time.timeScale = 0f; // Pause the game (if you're using timeScale for gameplay)
    }
    
    private void CloseShop()
    {
        isShopOpen = false;
        
        // Hide shop UI
        if (shopPanel != null)
            shopPanel.SetActive(false);
            
        // Resume game
        Time.timeScale = 1f;
    }
    
    private void UpdatePlayerMoneyDisplay()
    {
        if (playerMoneyText != null && CurrencyManager.Instance != null)
        {
            playerMoneyText.text = CurrencyManager.Instance.GetMoney().ToString() + " coins";
        }
    }
    
    // Optional: Visualize the interaction range in the editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}