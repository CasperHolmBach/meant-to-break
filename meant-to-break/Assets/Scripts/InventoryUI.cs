using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private float displayTime = 3.0f;
    
    [Header("Weapon Text References")]
    [SerializeField] private TextMeshProUGUI katanaText;
    [SerializeField] private TextMeshProUGUI glockText;
    [SerializeField] private TextMeshProUGUI smgText;
    [SerializeField] private TextMeshProUGUI rocketLauncherText;
    
    // References
    private Inventory playerInventory;
    private Coroutine hideCoroutine;
    
    void Start()
    {
        // Find player inventory
        playerInventory = FindObjectOfType<Inventory>();
        
        if (playerInventory == null)
        {
            Debug.LogError("Player Inventory not found!");
        }
        else
        {
            // Subscribe to weapon changes
            playerInventory.OnWeaponChanged += UpdateWeaponSelectionUI;
        }
        
        // Hide UI initially
        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);
            
        // Update the UI once at start
        UpdateWeaponListUI();
    }
    
    void Update()
    {
        // Toggle inventory UI on Tab press
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ShowInventory();
        }
    }
    
    void ShowInventory()
    {
        if (inventoryPanel != null)
        {
            UpdateWeaponListUI();
            
            // Show the panel
            inventoryPanel.SetActive(true);
            
            // Cancel any existing hide coroutine
            if (hideCoroutine != null)
                StopCoroutine(hideCoroutine);
                
            // Start a new hide coroutine
            hideCoroutine = StartCoroutine(HideInventoryAfterDelay());
        }
    }
    
    IEnumerator HideInventoryAfterDelay()
    {
        // Wait for the specified time
        yield return new WaitForSeconds(displayTime);
        
        // Hide the inventory panel
        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);
            
        hideCoroutine = null;
    }
    
    void UpdateWeaponListUI()
    {
        if (playerInventory == null) return;
        
        int activeWeapon = playerInventory.GetActiveWeaponIndex();
        
        // Update Katana UI
        if (katanaText != null)
        {
            UpdateWeaponText(katanaText, "1: Katana", 0, activeWeapon);
        }
        
        // Update Glock UI
        if (glockText != null)
        {
            UpdateWeaponText(glockText, "2: Glock", 1, activeWeapon);
        }
        
        // Update SMG UI
        if (smgText != null)
        {
            UpdateWeaponText(smgText, "3: SMG", 2, activeWeapon);
        }
        
        // Update Rocket Launcher UI
        if (rocketLauncherText != null)
        {
            UpdateWeaponText(rocketLauncherText, "4: Rocket Launcher", 3, activeWeapon);
        }
    }
    
    void UpdateWeaponText(TextMeshProUGUI textField, string weaponName, int weaponIndex, int activeWeapon)
    {
        bool unlocked = playerInventory.IsWeaponUnlocked(weaponIndex);
        
        if (unlocked)
        {

            if (weaponIndex == activeWeapon)
            {
                textField.text = "> " + weaponName;
                textField.color = Color.yellow; 
            }
            else
            {
                textField.text = "  " + weaponName;
                textField.color = Color.white;
            }
        }
        else
        {
            textField.text = "  " + weaponName + " (Locked)";
            textField.color = new Color(0.5f, 0.5f, 0.5f); 
        }
    }
    
    // When the active weapon changes, update the UI
    void UpdateWeaponSelectionUI(int activeWeaponIndex)
    {
        UpdateWeaponListUI();
    }
    
    void OnDestroy()
    {
        if (playerInventory != null)
        {
            playerInventory.OnWeaponChanged -= UpdateWeaponSelectionUI;
        }
    }
}