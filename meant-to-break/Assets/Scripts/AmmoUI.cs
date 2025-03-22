using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AmmoUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private GameObject ammoPanel;
    [SerializeField] private Slider reloadProgressBar;
    
    // Cached references
    private GameObject playerObject;
    private Inventory playerInventory;
    private Glock glockWeapon;
    
    void Start()
    {
        // Find the player
        playerObject = GameObject.FindGameObjectWithTag("Player");
        
        if (playerObject != null)
        {
            playerInventory = playerObject.GetComponent<Inventory>();
        }
        else
        {
            Debug.LogError("Player not found! Make sure the player has the 'Player' tag.");
        }
        
        // Hide UI initially
        if (ammoPanel != null)
            ammoPanel.SetActive(false);
            
        if (reloadProgressBar != null)
            reloadProgressBar.gameObject.SetActive(false);
    }
    
    void Update()
    {
        if (playerInventory == null) return;
        
        int activeWeapon = playerInventory.GetActiveWeaponIndex();
        
        // Only show ammo UI for weapons that use ammo (currently just Glock)
        if (activeWeapon == 1) // Glock
        {
            UpdateGlockUI();
        }
        else
        {
            HideAmmoUI();
        }
    }
    
    private void UpdateGlockUI()
    {
        // Find the glock if we don't have it yet
        if (glockWeapon == null)
        {
            glockWeapon = FindWeaponComponent<Glock>();
            
            if (glockWeapon != null)
            {
                // Subscribe to events
                glockWeapon.OnAmmoChanged += UpdateAmmoDisplay;
                glockWeapon.OnReloadStateChanged += UpdateReloadDisplay;
            }
            else
            {
                return;
            }
        }
        
        // Show ammo panel
        if (ammoPanel != null)
            ammoPanel.SetActive(true);
        
        // Update ammo display
        UpdateAmmoDisplay(glockWeapon.GetCurrentAmmo(), glockWeapon.GetMaxAmmo());
        
        // Update reload display
        if (glockWeapon.IsReloading())
        {
            UpdateReloadDisplay(true, glockWeapon.GetReloadTimeRemaining());
        }
    }
    
    private void UpdateAmmoDisplay(int current, int max)
    {
        if (ammoText != null)
        {
            ammoText.text = current + " / " + max;
        }
    }
    
    private void UpdateReloadDisplay(bool isReloading, float timeRemaining)
    {
        if (reloadProgressBar != null)
        {
            reloadProgressBar.gameObject.SetActive(isReloading);
            
            if (isReloading)
            {
                float progress = 1 - (timeRemaining / glockWeapon.GetReloadTotalTime());
                reloadProgressBar.value = progress;
            }
        }
    }
    
    private void HideAmmoUI()
    {
        if (ammoPanel != null)
            ammoPanel.SetActive(false);
            
        if (reloadProgressBar != null)
            reloadProgressBar.gameObject.SetActive(false);
    }
    
    private T FindWeaponComponent<T>() where T : MonoBehaviour
    {
        if (playerObject == null) return null;
        
        Transform[] allChildren = playerObject.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in allChildren)
        {
            T component = child.GetComponent<T>();
            if (component != null)
            {
                Debug.Log($"Found {typeof(T).Name} component");
                return component;
            }
        }
        
        Debug.LogWarning($"Couldn't find {typeof(T).Name} component");
        return null;
    }
    
    // Cleanup when destroying
    void OnDestroy()
    {
        if (glockWeapon != null)
        {
            glockWeapon.OnAmmoChanged -= UpdateAmmoDisplay;
            glockWeapon.OnReloadStateChanged -= UpdateReloadDisplay;
        }
    }
}