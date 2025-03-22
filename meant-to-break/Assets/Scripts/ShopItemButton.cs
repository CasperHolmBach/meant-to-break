public class ShopManager : MonoBehaviour
{
    [Header("Shop Settings")]
    [SerializeField] private string shopName = "General Store";
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    
    [Header("Shop Items")]
    [SerializeField] private ShopItem[] availableItems;
    
    // Private UI references - will be created at runtime
    private Canvas shopCanvas;
    private GameObject shopPanel;
    private TextMeshProUGUI shopNameText;
    private TextMeshProUGUI playerMoneyText;
    private GameObject itemsContainer;
    private GameObject detailsPanel;
    private TextMeshProUGUI itemNameText;
    private TextMeshProUGUI itemDescriptionText;
    private TextMeshProUGUI itemPriceText;
    private Image itemIconImage;
    private Button purchaseButton;
    private TextMeshProUGUI interactionPrompt;
    
    // State tracking
    private Transform playerTransform;
    private bool isInRange = false;
    private bool isShopOpen = false;
    private ShopItem selectedItem;
    
    void Start()
    {
        // Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        
        // Create all UI elements
        CreateShopUI();
        
        // Initialize UI
        CloseShop();
    }
    
    void Update()
    {
        // Check if player is in range
        if (playerTransform != null)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            bool wasInRange = isInRange;
            isInRange = distance <= interactionDistance;
            
            // Show/hide prompt when entering/leaving range
            if (isInRange != wasInRange)
            {
                if (isInRange && !isShopOpen)
                    ShowInteractionPrompt();
                else
                    HideInteractionPrompt();
            }
        }
        
        // Toggle shop when interact key is pressed
        if (isInRange && Input.GetKeyDown(interactKey))
        {
            if (isShopOpen)
                CloseShop();
            else
                OpenShop();
        }
    }
    
    #region UI Creation
    
    private void CreateShopUI()
    {
        // Create Canvas
        GameObject canvasObj = new GameObject("ShopCanvas");
        shopCanvas = canvasObj.AddComponent<Canvas>();
        shopCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Set Canvas to persist and be invisible initially
        DontDestroyOnLoad(canvasObj);
        canvasObj.SetActive(false);
        
        // Create main shop panel
        shopPanel = CreatePanel(canvasObj.transform, "ShopPanel", new Vector2(600, 400));
        RectTransform shopRect = shopPanel.GetComponent<RectTransform>();
        shopRect.anchorMin = new Vector2(0.5f, 0.5f);
        shopRect.anchorMax = new Vector2(0.5f, 0.5f);
        shopRect.pivot = new Vector2(0.5f, 0.5f);
        
        // Create header with shop name
        GameObject headerPanel = CreatePanel(shopPanel.transform, "HeaderPanel", new Vector2(600, 50));
        RectTransform headerRect = headerPanel.GetComponent<RectTransform>();
        headerRect.anchorMin = new Vector2(0, 1);
        headerRect.anchorMax = new Vector2(1, 1);
        headerRect.pivot = new Vector2(0.5f, 1);
        headerRect.anchoredPosition = Vector2.zero;
        
        // Shop name text
        shopNameText = CreateText(headerPanel.transform, shopName, 24, TextAlignmentOptions.Center);
        
        // Player money display
        GameObject moneyPanel = CreatePanel(shopPanel.transform, "MoneyPanel", new Vector2(600, 30));
        RectTransform moneyRect = moneyPanel.GetComponent<RectTransform>();
        moneyRect.anchorMin = new Vector2(0, 1);
        moneyRect.anchorMax = new Vector2(1, 1);
        moneyRect.pivot = new Vector2(0.5f, 1);
        moneyRect.anchoredPosition = new Vector2(0, -50);
        
        playerMoneyText = CreateText(moneyPanel.transform, "0 coins", 18, TextAlignmentOptions.Right);
        RectTransform moneyTextRect = playerMoneyText.GetComponent<RectTransform>();
        moneyTextRect.anchoredPosition = new Vector2(-10, 0);
        
        // Left side - Items container
        itemsContainer = CreatePanel(shopPanel.transform, "ItemsContainer", new Vector2(300, 300));
        RectTransform itemsRect = itemsContainer.GetComponent<RectTransform>();
        itemsRect.anchorMin = new Vector2(0, 0);
        itemsRect.anchorMax = new Vector2(0.5f, 1);
        itemsRect.pivot = new Vector2(0.5f, 0.5f);
        itemsRect.anchoredPosition = new Vector2(0, -40);
        
        // Add vertical layout group for items
        VerticalLayoutGroup itemsLayout = itemsContainer.AddComponent<VerticalLayoutGroup>();
        itemsLayout.childAlignment = TextAnchor.UpperCenter;
        itemsLayout.spacing = 10;
        itemsLayout.padding = new RectOffset(10, 10, 10, 10);
        
        // Right side - Item details panel
        detailsPanel = CreatePanel(shopPanel.transform, "DetailsPanel", new Vector2(280, 300));
        RectTransform detailsRect = detailsPanel.GetComponent<RectTransform>();
        detailsRect.anchorMin = new Vector2(0.5f, 0);
        detailsRect.anchorMax = new Vector2(1, 1);
        detailsRect.pivot = new Vector2(0.5f, 0.5f);
        detailsRect.anchoredPosition = new Vector2(0, -40);
        
        // Item icon
        GameObject iconObj = new GameObject("ItemIcon");
        iconObj.transform.SetParent(detailsPanel.transform, false);
        itemIconImage = iconObj.AddComponent<Image>();
        RectTransform iconRect = iconObj.GetComponent<RectTransform>();
        iconRect.sizeDelta = new Vector2(100, 100);
        iconRect.anchorMin = new Vector2(0.5f, 1);
        iconRect.anchorMax = new Vector2(0.5f, 1);
        iconRect.pivot = new Vector2(0.5f, 1);
        iconRect.anchoredPosition = new Vector2(0, -10);
        
        // Item name
        itemNameText = CreateText(detailsPanel.transform, "Item Name", 20, TextAlignmentOptions.Center);
        RectTransform nameRect = itemNameText.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 1);
        nameRect.anchorMax = new Vector2(1, 1);
        nameRect.pivot = new Vector2(0.5f, 1);
        nameRect.anchoredPosition = new Vector2(0, -120);
        
        // Item description
        itemDescriptionText = CreateText(detailsPanel.transform, "Item description goes here.", 16, TextAlignmentOptions.Center);
        RectTransform descRect = itemDescriptionText.GetComponent<RectTransform>();
        descRect.anchorMin = new Vector2(0, 0.5f);
        descRect.anchorMax = new Vector2(1, 0.5f);
        descRect.pivot = new Vector2(0.5f, 0.5f);
        descRect.anchoredPosition = new Vector2(0, 0);
        descRect.sizeDelta = new Vector2(260, 100);
        
        // Item price
        itemPriceText = CreateText(detailsPanel.transform, "100 coins", 18, TextAlignmentOptions.Center);
        RectTransform priceRect = itemPriceText.GetComponent<RectTransform>();
        priceRect.anchorMin = new Vector2(0, 0);
        priceRect.anchorMax = new Vector2(1, 0);
        priceRect.pivot = new Vector2(0.5f, 0);
        priceRect.anchoredPosition = new Vector2(0, 60);
        
        // Purchase button
        GameObject buttonObj = CreateButton(detailsPanel.transform, "Purchase", new Vector2(120, 40));
        purchaseButton = buttonObj.GetComponent<Button>();
        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0);
        buttonRect.anchorMax = new Vector2(0.5f, 0);
        buttonRect.pivot = new Vector2(0.5f, 0);
        buttonRect.anchoredPosition = new Vector2(0, 10);
        
        // Add click handler to purchase button
        purchaseButton.onClick.AddListener(PurchaseSelectedItem);
        
        // Create interaction prompt
        GameObject promptObj = new GameObject("InteractionPrompt");
        promptObj.transform.SetParent(canvasObj.transform, false);
        interactionPrompt = promptObj.AddComponent<TextMeshProUGUI>();
        interactionPrompt.text = $"Press {interactKey} to interact with {shopName}";
        interactionPrompt.fontSize = 20;
        interactionPrompt.color = Color.white;
        interactionPrompt.alignment = TextAlignmentOptions.Center;
        
        RectTransform promptRect = promptObj.GetComponent<RectTransform>();
        promptRect.anchorMin = new Vector2(0.5f, 0);
        promptRect.anchorMax = new Vector2(0.5f, 0);
        promptRect.pivot = new Vector2(0.5f, 0);
        promptRect.sizeDelta = new Vector2(400, 50);
        promptRect.anchoredPosition = new Vector2(0, 100);
        
        interactionPrompt.gameObject.SetActive(false);
    }
    
    private GameObject CreatePanel(Transform parent, string name, Vector2 size)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        
        Image image = panel.AddComponent<Image>();
        image.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.sizeDelta = size;
        
        return panel;
    }
    
    private TextMeshProUGUI CreateText(Transform parent, string content, int fontSize, TextAlignmentOptions alignment)
    {
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(parent, false);
        
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = content;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = Color.white;
        
        RectTransform rect = textObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        
        return text;
    }
    
    private GameObject CreateButton(Transform parent, string text, Vector2 size)
    {
        GameObject buttonObj = new GameObject("Button");
        buttonObj.transform.SetParent(parent, false);
        
        Image image = buttonObj.AddComponent<Image>();
        image.color = new Color(0.2f, 0.6f, 0.9f);
        
        Button button = buttonObj.AddComponent<Button>();
        button.targetGraphic = image;
        
        // Create text inside button
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = text;
        text.fontSize = 16;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        // Set button size
        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.sizeDelta = size;
        
        return buttonObj;
    }
    
    #endregion
    
    private void PopulateShopItems()
    {
        // Clear existing items
        foreach (Transform child in itemsContainer.transform)
        {
            Destroy(child.gameObject);
        }
        
        // Create new item buttons for each available item
        foreach (ShopItem item in availableItems)
        {
            // Create button
            GameObject buttonObj = CreateItemButton(item);
            buttonObj.transform.SetParent(itemsContainer.transform, false);
            
            // Add click event
            Button button = buttonObj.GetComponent<Button>();
            button.onClick.AddListener(() => SelectItem(item));
        }
    }
    
    private GameObject CreateItemButton(ShopItem item)
    {
        // Create button container
        GameObject buttonObj = new GameObject(item.itemName + "Button");
        RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(280, 50);
        
        // Add image background
        Image background = buttonObj.AddComponent<Image>();
        background.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        
        // Add button component
        Button button = buttonObj.AddComponent<Button>();
        button.targetGraphic = background;
        
        // Create layout for button contents
        HorizontalLayoutGroup layout = buttonObj.AddComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.padding = new RectOffset(5, 5, 5, 5);
        layout.spacing = 10;
        
        // Create icon if available
        if (item.icon != null)
        {
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(buttonObj.transform, false);
            
            Image iconImage = iconObj.AddComponent<Image>();
            iconImage.sprite = item.icon;
            iconImage.preserveAspect = true;
            
            RectTransform iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.sizeDelta = new Vector2(40, 40);
        }
        
        // Create text content (name + price)
        GameObject textObj = new GameObject("ItemText");
        textObj.transform.SetParent(buttonObj.transform, false);
        
        TextMeshProUGUI itemText = textObj.AddComponent<TextMeshProUGUI>();
        itemText.text = item.itemName;
        itemText.fontSize = 16;
        itemText.alignment = TextAlignmentOptions.Left;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.sizeDelta = new Vector2(170, 40);
        
        // Create price text
        GameObject priceObj = new GameObject("PriceText");
        priceObj.transform.SetParent(buttonObj.transform, false);
        
        TextMeshProUGUI priceText = priceObj.AddComponent<TextMeshProUGUI>();
        priceText.text = item.price + " coins";
        priceText.fontSize = 14;
        priceText.alignment = TextAlignmentOptions.Right;
        priceText.color = Color.yellow;
        
        RectTransform priceRect = priceObj.GetComponent<RectTransform>();
        priceRect.sizeDelta = new Vector2(80, 40);
        
        return buttonObj;
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
        else
            itemIconImage.sprite = null;
            
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
        
        // Change button color based on affordability
        Image buttonImage = purchaseButton.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.color = canAfford ? new Color(0.2f, 0.6f, 0.9f) : new Color(0.5f, 0.5f, 0.5f);
        }
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
            StartCoroutine(ShowNotEnoughMoneyMessage());
        }
    }
    
    private IEnumerator ShowNotEnoughMoneyMessage()
    {
        // Store original text and color
        string originalText = itemPriceText.text;
        Color originalColor = itemPriceText.color;
        
        // Show error message
        itemPriceText.text = "Not enough coins!";
        itemPriceText.color = Color.red;
        
        // Wait a moment
        yield return new WaitForSeconds(1.5f);
        
        // Restore original text and color
        if (selectedItem != null)
        {
            itemPriceText.text = originalText;
            itemPriceText.color = originalColor;
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
        
        // Fade out the light after the bounce
        Light light = item.GetComponent<Light>();
        if (light != null)
        {
            StartCoroutine(FadeOutLight(light));
        }
    }

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
    
    private void ShowInteractionPrompt()
    {
        if (interactionPrompt != null)
            interactionPrompt.gameObject.SetActive(true);
    }
    
    private void HideInteractionPrompt()
    {
        if (interactionPrompt != null)
            interactionPrompt.gameObject.SetActive(false);
    }
    
    private void OpenShop()
    {
        isShopOpen = true;
        
        // Hide interaction prompt
        HideInteractionPrompt();
        
        // Ensure canvas is active
        if (shopCanvas != null)
            shopCanvas.gameObject.SetActive(true);
            
        // Show shop UI
        if (shopPanel != null)
            shopPanel.SetActive(true);
            
        // Set shop name
        if (shopNameText != null)
            shopNameText.text = shopName;
            
        // Populate items
        PopulateShopItems();
            
        // Update player money display
        UpdatePlayerMoneyDisplay();
        
        // Hide details panel initially
        if (detailsPanel != null)
            detailsPanel.SetActive(false);
        
        // Optional: Pause game, disable player movement, etc.
        Time.timeScale = 0f; // Pause the game (if you're using timeScale for gameplay)
    }
    
    private void CloseShop()
    {
        isShopOpen = false;
        
        // Hide shop UI
        if (shopCanvas != null)
            shopCanvas.gameObject.SetActive(false);
            
        // Show interaction prompt if player is still in range
        if (isInRange)
            ShowInteractionPrompt();
            
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
    
    // Visualize the interaction range in the editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}