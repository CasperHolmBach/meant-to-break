using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponCooldownUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI cooldownText;
    [SerializeField] private Image cooldownFill;
    [SerializeField] private GameObject cooldownPanel;
    [Header("Colors")]
    [SerializeField] private Color readyColor = Color.green;
    [SerializeField] private Color cooldownColor = Color.red;
    
    // Cached references
    private GameObject playerObject;
    private Inventory playerInventory;
    private RocketLauncher rocketLauncher;
    private Katana katana;
    
    // Track currently displayed weapon type
    private int currentWeaponType = -1;
    
    void Start()
    {
        // Find the player
        playerObject = GameObject.FindGameObjectWithTag("Player");
        
        if (playerObject != null)
        {
            playerInventory = playerObject.GetComponent<Inventory>();
            Debug.Log("Found player and inventory");
        }
        else
        {
            Debug.LogError("Player not found! Make sure the player has the 'Player' tag.");
        }
        
        // Hide UI initially
        HideCooldownUI();
    }
    
    void Update()
    {
        if (playerInventory == null) return;
        
        int activeWeapon = playerInventory.GetActiveWeaponIndex();
        
        if (activeWeapon == -1 || (!HasCooldown(activeWeapon)))
        {
            HideCooldownUI();
            return;
        }
        
        if (currentWeaponType != activeWeapon)
        {
            currentWeaponType = activeWeapon;
        }
        
        // Handle the active weapon with cooldown
        switch (activeWeapon)
        {
            case 3: // Rocket Launcher
                UpdateRocketLauncherUI();
                break;
            case 0: // Katana
                UpdateKatanaUI();
                break;
            default:
                // Hide UI for weapons without cooldown mechanisms
                HideCooldownUI();
                break;
        }
    }
    
    private bool HasCooldown(int weaponIndex)
    {
        // Return true for weapons that have cooldowns
        return weaponIndex == 0 || weaponIndex == 3; // Katana and Rocket Launcher
    }
    
    private void UpdateRocketLauncherUI()
    {
        // Find the rocket launcher if we don't have it yet
        if (rocketLauncher == null)
        {
            rocketLauncher = FindWeaponComponent<RocketLauncher>();
            if (rocketLauncher == null) return;
        }
        
        ShowCooldownUI();
        
        // Get cooldown information
        float remainingCooldown = rocketLauncher.GetRemainingCooldown();
        float maxCooldown = rocketLauncher.cooldownTime;
        
        // Update UI
        if (remainingCooldown <= 0)
        {
            SetUIReady(false); // Not a dash
        }
        else
        {
            SetUICooldown(remainingCooldown, maxCooldown);
        }
    }
    
    private void UpdateKatanaUI()
    {
        // Find the katana
        if (katana == null)
        {
            katana = FindWeaponComponent<Katana>();
            if (katana == null) return;
        }
        
        ShowCooldownUI();
        
        // Calculate remaining dash cooldown time
        float remainingCooldown = 0;
        if (katana != null)
        {
            // Access the nextDashTime and dashCooldown
            float nextDashTime = 0;
            float dashCooldown = 0;
            
            // Use reflection to access private fields if needed
            var nextDashTimeField = katana.GetType().GetField("nextDashTime", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var dashCooldownField = katana.GetType().GetField("dashCooldown", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (nextDashTimeField != null && dashCooldownField != null)
            {
                nextDashTime = (float)nextDashTimeField.GetValue(katana);
                dashCooldown = (float)dashCooldownField.GetValue(katana);
                
                remainingCooldown = nextDashTime - Time.time;
            }
            
            // Update UI based on cooldown
            if (remainingCooldown <= 0)
            {
                SetUIReady(true); // Is a dash
            }
            else
            {
                SetUICooldown(remainingCooldown, dashCooldown);
            }
        }
    }
    
    private T FindWeaponComponent<T>() where T : MonoBehaviour
    {
        Transform[] allChildren = playerObject.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in allChildren)
        {
            T weaponComponent = child.GetComponent<T>();
            if (weaponComponent != null)
            {
                Debug.Log("Found " + typeof(T).Name + " component");
                return weaponComponent;
            }
        }
        
        Debug.LogWarning("Couldn't find " + typeof(T).Name + " component");
        return null;
    }
    
    private void SetUIReady(bool isDash)
    {
        if (cooldownText != null)
        {
            // Show different text based on weapon type
            cooldownText.text = isDash ? "DASH READY" : "WEAPON READY";
            cooldownText.color = readyColor;
        }
        
        if (cooldownFill != null)
        {
            cooldownFill.fillAmount = 0;
            cooldownFill.color = readyColor;
        }
    }
    
    private void SetUICooldown(float remainingTime, float maxTime)
    {
        if (cooldownText != null)
        {
            cooldownText.text = remainingTime.ToString("F1") + "s";
            cooldownText.color = cooldownColor;
        }
        
        if (cooldownFill != null)
        {
            cooldownFill.fillAmount = remainingTime / maxTime;
            cooldownFill.color = cooldownColor;
        }
    }
    
    private void ShowCooldownUI()
    {
        if (cooldownPanel != null)
        {
            cooldownPanel.SetActive(true);
        }
    }
    
    private void HideCooldownUI()
    {
        if (cooldownPanel != null)
        {
            cooldownPanel.SetActive(false);
        }
    }
}